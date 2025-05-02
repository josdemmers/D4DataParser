using D4DataParser.Constants;
using D4DataParser.Entities;
using D4DataParser.Entities.D4Data;
using D4DataParser.Mappings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace D4DataParser.Parsers
{
    public class RuneParser
    {
        private string _d4dataPath = string.Empty;
        private string _language = string.Empty;
        private List<string> _languages = new List<string>();
        private Dictionary<string, List<RuneInfo>> _runeInfoDictionary = new Dictionary<string, List<RuneInfo>>();

        // D4Data repo data
        private Dictionary<int, string> _runeDictionary = new Dictionary<int, string>();

        // Start of Constructors region

        #region Constructors

        public RuneParser()
        {
            // Init languages
            InitLocalisations();
        }

        #endregion

        // Start of Events region

        #region Events

        #endregion

        // Start of Properties region

        #region Properties

        public string D4dataPath { get => _d4dataPath; set => _d4dataPath = value; }

        #endregion

        // Start of Event handlers region

        #region Event handlers

        #endregion

        // Start of Methods region

        #region Methods

        private void InitLocalisations()
        {
            _languages.Clear();

            // TODO: - DEV - Enable languages for release
            _languages.Add("deDE");
            _languages.Add("enUS");
            _languages.Add("esES");
            _languages.Add("esMX");
            _languages.Add("frFR");
            _languages.Add("itIT");
            _languages.Add("jaJP");
            _languages.Add("koKR");
            _languages.Add("plPL");
            _languages.Add("ptBR");
            _languages.Add("ruRU");
            _languages.Add("trTR");
            _languages.Add("zhCN");
            _languages.Add("zhTW");
        }

        public void Parse()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var elapsedMs = watch.ElapsedMilliseconds;

            // Reset
            _runeDictionary.Clear();
            _runeInfoDictionary.Clear();

            // Parse CoreTOC.dat.json -- Runes
            string coreTOCPath = $"{_d4dataPath}json\\base\\CoreTOC.dat.json";
            int affixIndex = 42;
            var jsonAsText = File.ReadAllText(coreTOCPath);
            var coreTOCDictionary = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, Dictionary<int, string>>>(jsonAsText) ?? new Dictionary<int, Dictionary<int, string>>();
            _runeDictionary = coreTOCDictionary.ContainsKey(affixIndex) ? coreTOCDictionary[affixIndex] : new Dictionary<int, string>();
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (CoreTOC.dat.json - Runes): {watch.ElapsedMilliseconds - elapsedMs}");

            foreach (var language in _languages)
            {
                if (Directory.Exists($"{_d4dataPath}json\\{language}_Text\\"))
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {language}");
                    ParseByLanguage(language);
                }
                else
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Skipped {language}, not available.");
                }
            }
        }

        private void ParseByLanguage(string language)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var elapsedMs = watch.ElapsedMilliseconds;

            _language = language;

            // Initialise RuneInfo
            var runeInfoList = new List<RuneInfo>();
            _runeInfoDictionary[language] = runeInfoList;
            foreach (var runeEntry in _runeDictionary)
            {
                if (runeEntry.Value.StartsWith("Item_Rune_Condition", StringComparison.OrdinalIgnoreCase) ||
                    runeEntry.Value.StartsWith("Item_Rune_Effect", StringComparison.OrdinalIgnoreCase))
                {
                    runeInfoList.Add(new RuneInfo
                    {
                        IdSno = runeEntry.Key.ToString(),
                        IdName = runeEntry.Value,
                        RuneType = runeEntry.Value.Contains(RuneTypeConstants.Condition, StringComparison.OrdinalIgnoreCase) ? RuneTypeConstants.Condition : RuneTypeConstants.Effect
                    });
                }
            }

            // Remove test runes
            runeInfoList.RemoveAll(r => r.IdName.Equals("Item_Rune_Condition_OncePerXSeconds"));
            runeInfoList.RemoveAll(r => r.IdName.Equals("Item_Rune_Condition_OncePerSecond"));

            // Update localisation
            foreach (var runeInfo in runeInfoList)
            {
                // Find localisation data
                string directory = $"{_d4dataPath}json\\{_language}_Text\\meta\\StringList\\";
                string fileNameLocalisation = $"{directory}{runeInfo.IdName}.stl.json";
                var jsonAsText = File.ReadAllText(fileNameLocalisation);
                var localisation = JsonSerializer.Deserialize<Localisation>(jsonAsText);
                if (localisation != null)
                {
                    var localisationName = localisation.arStrings.FirstOrDefault(l => l.szLabel.Equals("Name", StringComparison.OrdinalIgnoreCase));
                    var localisationRuneDescription = localisation.arStrings.FirstOrDefault(l => l.szLabel.Equals("RuneDescription", StringComparison.OrdinalIgnoreCase));
                    var localisationRuneOverflowBehavior = localisation.arStrings.FirstOrDefault(l => l.szLabel.Equals("RuneOverflowBehavior", StringComparison.OrdinalIgnoreCase));

                    if (localisationName != null)
                    {
                        runeInfo.Name = localisationName.szText;
                    }

                    if (localisationRuneDescription != null)
                    {
                        runeInfo.Description = localisationRuneDescription.szText;
                        runeInfo.RuneDescription = localisationRuneDescription.szText;
                    }

                    if (localisationRuneOverflowBehavior != null)
                    {
                        runeInfo.RuneOverflowBehavior = localisationRuneOverflowBehavior.szText;
                    }
                }
            }
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (Update localisation): {watch.ElapsedMilliseconds - elapsedMs}");
            elapsedMs = watch.ElapsedMilliseconds;

            // Replace numeric value placeholders
            ReplaceNumericValuePlaceholders();
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (Replace numeric value placeholders): {watch.ElapsedMilliseconds - elapsedMs}");
            elapsedMs = watch.ElapsedMilliseconds;

            // Set cleaned up description for fuzzy searches.
            foreach (var runeInfo in runeInfoList)
            {
                SetCleanDescription(runeInfo);
            }

            // Sort
            runeInfoList.Sort((x, y) =>
            {
                return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
            });

            SaveRunes(language);
            ValidateRunes(language);

            watch.Stop();
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (Total): {watch.ElapsedMilliseconds}");
        }

        private void ReplaceNumericValuePlaceholders()
        {
            var runeInfoList = _runeInfoDictionary[_language];

            foreach (var rune in runeInfoList)
            {
                rune.Description = rune.Description.Replace("{c_gray}", string.Empty);
                rune.Description = rune.Description.Replace("{c_important}", string.Empty);
                rune.Description = rune.Description.Replace("{c_label}", string.Empty);
                rune.Description = rune.Description.Replace("{c_mythic}", string.Empty);
                rune.Description = rune.Description.Replace("{c_number}", string.Empty);
                rune.Description = rune.Description.Replace("{c_Number}", string.Empty);
                rune.Description = rune.Description.Replace("{c_RuneCondition}", string.Empty);
                rune.Description = rune.Description.Replace("{C_RuneCondition}", string.Empty);
                rune.Description = rune.Description.Replace("{c_RuneEffect}", string.Empty);
                rune.Description = rune.Description.Replace("{/c}", string.Empty);
                rune.Description = rune.Description.Replace("{/c]", string.Empty);
                rune.Description = rune.Description.Replace("{u}", string.Empty);
                rune.Description = rune.Description.Replace("{/u}", string.Empty);            

                rune.Description = rune.Description.Replace("[100|%|]", "100%");
                rune.Description = rune.Description.Replace("[2.5*10]", "25");
                rune.Description = rune.Description.Replace("[6*5]", "30");
                rune.Description = rune.Description.Replace("{s1}", "#");

                foreach (var placeholder in StringPlaceholderMappings.StringPlaceholder)
                {
                    rune.Description = rune.Description.Replace(placeholder.Key, placeholder.Value);
                }

                // Localisation
                rune.Name = rune.Name.Replace("[ms]", string.Empty);
                rune.Description = rune.Description.Replace("[fs]", string.Empty);
            }
        }

        private void SetCleanDescription(RuneInfo rune)
        {
            rune.DescriptionClean = rune.Description.Replace("+", string.Empty);
            rune.DescriptionClean = rune.DescriptionClean.Replace("#", string.Empty);
            rune.DescriptionClean = rune.DescriptionClean.Replace("%", string.Empty);
            rune.DescriptionClean = rune.DescriptionClean.Replace("  ", " ");
            rune.DescriptionClean = rune.DescriptionClean.Replace(" .", ".");
            rune.DescriptionClean = rune.DescriptionClean.Trim();
        }

        private void SaveRunes(string language)
        {
            // Create export list without any duplicates
            // Note: Removing duplicates is a workaround for bugged localisations of some languages, e.g. German.
            var runeInfoList = _runeInfoDictionary[language];
            var runeInfoListExport = new List<RuneInfo>();
            foreach (var runeInfo in runeInfoList)
            {
                if (runeInfoListExport.Any(r => r.Name.Equals(runeInfo.Name))) continue;
                if (runeInfoListExport.Any(r => r.DescriptionClean.Equals(runeInfo.DescriptionClean))) continue;

                runeInfoListExport.Add(runeInfo);
            }

            string fileName = $"Data/Runes.{language}.json";
            string path = Path.GetDirectoryName(fileName) ?? string.Empty;
            Directory.CreateDirectory(path);

            using FileStream stream = File.Create(fileName);
            var options = new JsonSerializerOptions { WriteIndented = true };
            options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            JsonSerializer.Serialize(stream, runeInfoListExport, options);
        }

        private void ValidateRunes(string language)
        {
            var runeInfoList = _runeInfoDictionary[language];
            foreach (var runeInfo in runeInfoList)
            {
                if (runeInfo.Description.Contains("{") || runeInfo.Description.Contains("}") ||
                    runeInfo.Description.Contains("[") || runeInfo.Description.Contains("]") ||
                    runeInfo.Description.Contains("|4"))
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {runeInfo.IdName}: missed formatting.");
                }
            }
        }

        #endregion
    }
}
