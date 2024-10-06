using D4DataParser.Entities.D4Data;
using D4DataParser.Entities;
using D4DataParser.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using D4DataParser.Mappings;

namespace D4DataParser.Parsers
{
    public class AspectParser
    {
        private string _d4dataPath = string.Empty;
        private string _language = string.Empty;
        private List<string> _languages = new List<string>();
        private Dictionary<string, List<AspectInfo>> _aspectInfoDictionary = new Dictionary<string, List<AspectInfo>>();

        // D4Data repo data
        private Dictionary<int, string> _affixDictionary = new Dictionary<int, string>();
        private Dictionary<int, string> _aspectDictionary = new Dictionary<int, string>();
        private List<AffixMeta> _affixMetaJsonList = new List<AffixMeta>();
        private List<AspectMeta> _aspectMetaJsonList = new List<AspectMeta>();
        private List<TrackedRewardMeta> _trackedRewardCodexMetaJsonList = new List<TrackedRewardMeta>();

        // Start of Constructors region

        #region Constructors

        public AspectParser()
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
            _affixDictionary.Clear();
            _affixMetaJsonList.Clear();
            _aspectDictionary.Clear();
            _aspectInfoDictionary.Clear();
            _aspectMetaJsonList.Clear();
            _trackedRewardCodexMetaJsonList.Clear();

            // Parse CoreTOC.dat.json -- Affixes
            string coreTOCPath = $"{_d4dataPath}json\\base\\CoreTOC.dat.json";
            int affixIndex = 104;
            var jsonAsText = File.ReadAllText(coreTOCPath);
            var coreTOCDictionary = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, Dictionary<int, string>>>(jsonAsText) ?? new Dictionary<int, Dictionary<int, string>>();
            _affixDictionary = coreTOCDictionary.ContainsKey(affixIndex) ? coreTOCDictionary[affixIndex] : new Dictionary<int, string>();
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (CoreTOC.dat.json - Affixes): {watch.ElapsedMilliseconds - elapsedMs}");
            elapsedMs = watch.ElapsedMilliseconds;

            // Parse CoreTOC.dat.json -- Aspects
            int aspectIndex = 128;
            _aspectDictionary = coreTOCDictionary.ContainsKey(aspectIndex) ? coreTOCDictionary[aspectIndex] : new Dictionary<int, string>();
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (CoreTOC.dat.json - Aspects): {watch.ElapsedMilliseconds - elapsedMs}");
            elapsedMs = watch.ElapsedMilliseconds;

            // Parse .\d4data\json\base\meta\Affix\
            _affixMetaJsonList = new List<AffixMeta>();
            var directory = $"{Path.GetDirectoryName(coreTOCPath)}\\meta\\Affix\\";
            if (Directory.Exists(directory))
            {
                var fileEntries = Directory.EnumerateFiles(directory).Where(file => file.EndsWith(".json", StringComparison.OrdinalIgnoreCase));
                foreach (string fileName in fileEntries)
                {
                    using (FileStream? stream = File.OpenRead(fileName))
                    {
                        if (stream != null)
                        {
                            // create the options
                            var options = new JsonSerializerOptions()
                            {
                                WriteIndented = true
                            };
                            // register the converter
                            //options.Converters.Add(new BoolConverter());
                            options.Converters.Add(new UIntConverter());

                            var affixMetaJson = JsonSerializer.Deserialize<AffixMeta>(stream, options) ?? new AffixMeta();
                            _affixMetaJsonList.Add(affixMetaJson);
                        }
                    }
                }
            }
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (Affix folder): {watch.ElapsedMilliseconds - elapsedMs}");
            elapsedMs = watch.ElapsedMilliseconds;

            // Parse .\d4data\json\base\meta\Aspect\
            _aspectMetaJsonList = new List<AspectMeta>();
            directory = $"{Path.GetDirectoryName(coreTOCPath)}\\meta\\Aspect\\";
            if (Directory.Exists(directory))
            {
                var fileEntries = Directory.EnumerateFiles(directory).Where(file =>
                    Path.GetFileName(file).StartsWith("Asp_Legendary_", StringComparison.OrdinalIgnoreCase) ||
                    Path.GetFileName(file).StartsWith("Asp_x1_Legendary_", StringComparison.OrdinalIgnoreCase) ||
                    Path.GetFileName(file).StartsWith("Asp_S05_BSK_", StringComparison.OrdinalIgnoreCase));
                foreach (string fileName in fileEntries)
                {
                    using (FileStream? stream = File.OpenRead(fileName))
                    {
                        if (stream != null)
                        {
                            // create the options
                            var options = new JsonSerializerOptions()
                            {
                                WriteIndented = true
                            };
                            // register the converter
                            //options.Converters.Add(new BoolConverter());
                            options.Converters.Add(new UIntConverter());

                            var aspectMetaJson = JsonSerializer.Deserialize<AspectMeta>(stream, options) ?? new AspectMeta();
                            _aspectMetaJsonList.Add(aspectMetaJson);
                        }
                    }
                }
            }
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (Aspect folder): {watch.ElapsedMilliseconds - elapsedMs}");
            elapsedMs = watch.ElapsedMilliseconds;

            // Parse .\d4data\json\base\meta\TrackedReward\
            _trackedRewardCodexMetaJsonList = new List<TrackedRewardMeta>();
            directory = $"{Path.GetDirectoryName(coreTOCPath)}\\meta\\TrackedReward\\";
            if (Directory.Exists(directory))
            {
                // Codex
                var fileEntries = Directory.EnumerateFiles(directory).Where(file => Path.GetFileName(file).StartsWith("TR_ASP_", StringComparison.OrdinalIgnoreCase));
                foreach (string fileName in fileEntries)
                {
                    using (FileStream? stream = File.OpenRead(fileName))
                    {
                        if (stream != null)
                        {
                            // create the options
                            var options = new JsonSerializerOptions()
                            {
                                WriteIndented = true
                            };
                            // register the converter
                            //options.Converters.Add(new BoolConverter());
                            options.Converters.Add(new UIntConverter());

                            var trackedRewardMetaJson = JsonSerializer.Deserialize<TrackedRewardMeta>(stream, options) ?? new TrackedRewardMeta();
                            _trackedRewardCodexMetaJsonList.Add(trackedRewardMetaJson);
                        }
                    }
                }
            }
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (TrackedReward folder): {watch.ElapsedMilliseconds - elapsedMs}");
            elapsedMs = watch.ElapsedMilliseconds;

            foreach (var language in _languages)
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {language}");

                // TODO: - DEV - Comment language skip for release
                //if (!language.Equals("deDE")) continue;
                //if (!language.Equals("enUS")) continue;
                //if (!language.Equals("esES")) continue;
                //if (!language.Equals("esMX")) continue;
                //if (!language.Equals("frFR")) continue;
                //if (!language.Equals("itIT")) continue;
                //if (!language.Equals("jaJP")) continue;
                //if (!language.Equals("koKR")) continue;
                //if (!language.Equals("plPL")) continue;
                //if (!language.Equals("ptBR")) continue;
                //if (!language.Equals("ruRU")) continue;
                //if (!language.Equals("trTR")) continue;
                //if (!language.Equals("zhCN")) continue;
                //if (!language.Equals("zhTW")) continue;

                ParseByLanguage(language);
                ValidateAspects(language);
            }
        }

        private void ParseByLanguage(string language)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var elapsedMs = watch.ElapsedMilliseconds;

            _language = language;

            // Initialise AspectInfo
            var aspectInfoList = new List<AspectInfo>();
            _aspectInfoDictionary[language] = aspectInfoList;
            foreach (var aspectEntry in _aspectDictionary)
            {
                var aspectMeta = _aspectMetaJsonList.FirstOrDefault(aspect => aspect.__snoID__ == aspectEntry.Key);
                if (aspectMeta != null)
                {
                    aspectInfoList.Add(new AspectInfo
                    {
                        IdSno = aspectMeta.snoAffix.__raw__,
                        IdName = aspectMeta.snoAffix.name,
                        IsCodex = true
                    });
                }
                else
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Missing aspect meta. ({aspectEntry.Key}) {aspectEntry.Value}");
                }
            }
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Current aspect count: {aspectInfoList.Count}");
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (Initialise AspectInfo): {watch.ElapsedMilliseconds - elapsedMs}");
            elapsedMs = watch.ElapsedMilliseconds;

            // Cleanup aspects
            // - Disabled aspects
            aspectInfoList.RemoveAll(aspect => aspect.IdName.Equals("legendary_sorc_034", StringComparison.OrdinalIgnoreCase)); // (PH) of Ensnaring Current (added by coreTOCAspects)

            // Update AspectInfo
            // - Allowed classes
            // - Allowed item labels
            // - MagicType (affix, aspect)
            // - Localisation data
            // - Seasonal
            foreach (var aspect in aspectInfoList)
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Processing ({aspect.IdSno}) {aspect.IdName}");

                var affixMeta = _affixMetaJsonList.FirstOrDefault(a => a.__snoID__ == aspect.IdSno);

                int IdSno = affixMeta.__snoID__;
                List<int> allowedForPlayerClass = affixMeta.fAllowedForPlayerClass ?? new List<int>();
                List<int> allowedItemLabels = affixMeta.arAllowedItemLabels ?? new List<int>();
                int magicType = affixMeta.eMagicType;

                aspect.AllowedForPlayerClass = allowedForPlayerClass;
                aspect.AllowedItemLabels = allowedItemLabels;
                aspect.Category = GetAspectCategory(affixMeta);
                aspect.MagicType = magicType;

                // Find localisation data
                string directory = $"{_d4dataPath}json\\{_language}_Text\\meta\\StringList\\";
                string fileName = affixMeta.__fileName__;
                string fileNameLocalisation = $"{directory}Affix_{Path.GetFileNameWithoutExtension(fileName)}.stl.json";
                var jsonAsText = File.ReadAllText(fileNameLocalisation);
                var localisation = JsonSerializer.Deserialize<Localisation>(jsonAsText);
                if (localisation != null)
                {
                    var aspectInfo = localisation.arStrings.FirstOrDefault(l => l.szLabel.Equals("name", StringComparison.OrdinalIgnoreCase));
                    if (aspectInfo != null)
                    {
                        // Remove variants (no idea where to get the correct form, so using the first one for now)
                        aspect.Name = aspectInfo.szText.Contains("]") ? aspectInfo.szText.Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries)[1] : aspectInfo.szText;
                    }
                    aspectInfo = localisation.arStrings.FirstOrDefault(l => l.szLabel.Equals("desc", StringComparison.OrdinalIgnoreCase));
                    if (aspectInfo != null)
                    {
                        aspect.Localisation = aspectInfo.szText;
                        aspect.Description = aspectInfo.szText;
                    }
                }

                // Find seasonal data
                //aspect.IsSeasonal = false;

                // Find dungeon data
                aspect.Dungeon = GetAspectDungeon(aspect);
            }

            // Replace numeric value placeholders
            ReplacePlaceholders(language);

            // Add a cleaned up description for fuzzy searches.
            SetCleanDescription(language);

            // Sort
            aspectInfoList.Sort((x, y) =>
            {
                return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
            });

            SaveAspects(language);
            ValidateAspects(language);

            watch.Stop();
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (Total): {watch.ElapsedMilliseconds}");
        }

        private void SaveAspects(string language)
        {
            var aspectInfoList = _aspectInfoDictionary[language];

            string fileName = $"Data/Aspects.{language}.json";
            string path = Path.GetDirectoryName(fileName) ?? string.Empty;
            Directory.CreateDirectory(path);

            using FileStream stream = File.Create(fileName);
            var options = new JsonSerializerOptions { WriteIndented = true };
            options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            JsonSerializer.Serialize(stream, aspectInfoList, options);
        }

        private void ValidateAspects(string language)
        {
            var aspectInfoList = _aspectInfoDictionary[language];

            var duplicates = aspectInfoList.GroupBy(a => a.Description).Where(a => a.Count() > 1);
            if (duplicates.Any())
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Duplicates found!");

                foreach (var group in duplicates)
                {
                    Console.WriteLine("Key: {0}", group.Key);
                    foreach (var affixInfo in group)
                    {
                        Debug.WriteLine($"{affixInfo.IdName}: {affixInfo.Description}");
                    }
                }
            }
        }

        private string GetAspectCategory(AffixMeta affixMeta)
        {
            var arAffixSkillTags = affixMeta.arAffixSkillTags;
            var arAffixSkillTag = arAffixSkillTags.FirstOrDefault(c => c.name.Contains("FILTER_Legendary_"));
            return arAffixSkillTag?.name ?? string.Empty;
        }

        private string GetAspectDungeon(AspectInfo aspectInfo)
        {
            string aspectDungeon = string.Empty;

            var trackedReward = _trackedRewardCodexMetaJsonList.FirstOrDefault(c => c.snoAspect.name.EndsWith(aspectInfo.IdName, StringComparison.OrdinalIgnoreCase));
            if (trackedReward == null) return string.Empty;

            string dungeon = Path.GetFileNameWithoutExtension(trackedReward.__fileName__);
            string fileName = $"World_DGN_{dungeon.Replace("TR_ASP_", string.Empty)}.stl.json";

            // Find localisation data
            string directory = $"{_d4dataPath}json\\{_language}_Text\\meta\\StringList\\";
            string filePath = $"{directory}{fileName}";
            // Fix Blizzard bugs
            filePath = filePath.Replace("ImmortalEmmanation", "ImmortalEmanation");

            if (File.Exists(filePath))
            {
                var jsonAsText = File.ReadAllText(filePath);
                var localisation = JsonSerializer.Deserialize<Localisation>(jsonAsText);
                if (localisation != null)
                {
                    var name = localisation.arStrings.FirstOrDefault(l => l.szLabel.Equals("name", StringComparison.OrdinalIgnoreCase));
                    if (name != null)
                    {
                        aspectDungeon = name.szText;
                    }
                }
            }

            return aspectDungeon;
        }

        private void SetCleanDescription(string language)
        {
            var aspectInfoList = _aspectInfoDictionary[language];

            foreach (var aspect in aspectInfoList)
            {
                aspect.DescriptionClean = aspect.Description.Replace("+", string.Empty);
                aspect.DescriptionClean = aspect.DescriptionClean.Replace("#", string.Empty);
                aspect.DescriptionClean = aspect.DescriptionClean.Replace("%", string.Empty);
                aspect.DescriptionClean = aspect.DescriptionClean.Replace("\r\n", " ");
                aspect.DescriptionClean = aspect.DescriptionClean.Replace("  ", " ");
                aspect.DescriptionClean = aspect.DescriptionClean.Replace(" .", ".");
                aspect.DescriptionClean = aspect.DescriptionClean.Trim();
            }
        }

        private void ReplacePlaceholders(string language)
        {
            var aspectInfoList = _aspectInfoDictionary[language];

            foreach (var aspect in aspectInfoList)
            {
                // Note: https://regex101.com/

                //string pattern = @"\[(.*?%+?.*?)\]";
                //aspect.Description = Regex.Replace(aspect.Description, pattern, "#%");

                //pattern = @"\[(.*?)\]";
                //aspect.Description = Regex.Replace(aspect.Description, pattern, "#");

                string pattern = @"\[([^%]+?)\]";
                aspect.Description = Regex.Replace(aspect.Description, pattern, "#");

                pattern = @"\[(.+?)\]";
                aspect.Description = Regex.Replace(aspect.Description, pattern, "#%");

                pattern = @"{(.*?)}";
                aspect.Description = Regex.Replace(aspect.Description, pattern, string.Empty);

                foreach (var placeholder in StringPlaceholderMappings.StringPlaceholder)
                {
                    aspect.Description = aspect.Description.Replace(placeholder.Key, placeholder.Value);
                }
            }
        }
        #endregion
    }
}
