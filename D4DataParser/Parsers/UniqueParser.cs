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

namespace D4DataParser.Parsers
{
    public class UniqueParser
    {
        private string _d4dataPath = string.Empty;
        private string _language = string.Empty;
        private List<string> _languages = new List<string>();

        private List<AffixMeta> _affixMetaJsonList = new List<AffixMeta>();
        private List<ItemMeta> _itemMetaJsonList = new List<ItemMeta>();
        private List<UniqueInfo> _uniqueInfoList = new List<UniqueInfo>();

        // Start of Constructors region

        #region Constructors

        public UniqueParser()
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

        public string CoreTOCPath
        {
            get => $"{_d4dataPath}json\\base\\CoreTOC.dat.json";
        }

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

        public void ParseUniques()
        {
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
                UpdateUniques();

                ValidateUniques();
            }
        }

        private void ParseByLanguage(string language)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var elapsedMs = watch.ElapsedMilliseconds;

            _language = language;

            // reset
            _affixMetaJsonList.Clear();
            _itemMetaJsonList.Clear();
            _uniqueInfoList.Clear();

            // Parse CoreTOC.dat.json
            var jsonAsText = File.ReadAllText(CoreTOCPath);
            var coreTOC = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, Dictionary<int, string>>>(jsonAsText);
            var coreTOCUniques = coreTOC[42];

            foreach (var item in coreTOCUniques.Where(kvp =>
                !kvp.Value.StartsWith("Item_", StringComparison.InvariantCultureIgnoreCase) ||
                !kvp.Value.Contains("_Unique_", StringComparison.InvariantCultureIgnoreCase) ||
                kvp.Value.Contains("_TEST_") ||
                kvp.Value.Contains("_NoPowers")).ToList())
            {
                coreTOCUniques.Remove(item.Key);
            }

            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (CoreTOC.dat): {watch.ElapsedMilliseconds - elapsedMs}");
            elapsedMs = watch.ElapsedMilliseconds;

            // Parse .\d4data\json\base\meta\Item\
            _itemMetaJsonList = new List<ItemMeta>();
            var directory = $"{Path.GetDirectoryName(CoreTOCPath)}\\meta\\Item\\";
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

                            var itemMetaJson = JsonSerializer.Deserialize<ItemMeta>(stream, options) ?? new ItemMeta();
                            _itemMetaJsonList.Add(itemMetaJson);
                        }
                    }
                }
            }
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (Item folder): {watch.ElapsedMilliseconds - elapsedMs}");
            elapsedMs = watch.ElapsedMilliseconds;

            // Parse .\d4data\json\base\meta\Affix\
            _affixMetaJsonList = new List<AffixMeta>();
            directory = $"{Path.GetDirectoryName(CoreTOCPath)}\\meta\\Affix\\";
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

            // Validate and create unique items
            foreach (var item in coreTOCUniques)
            {
                string idNameItem = item.Value.Substring(5);
                var itemMeta = _itemMetaJsonList.FirstOrDefault(i => i.__fileName__.EndsWith($"{idNameItem}.itm"));

                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Validating ({item.Key}) {item.Value}");

                if (itemMeta == null)
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Skipped. Item file not found: {idNameItem}.itm");
                    continue;
                }

                // Validate ForcedAffixes
                if (itemMeta.arForcedAffixes.Count < 5)
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Skipped. Invalid affix count: {idNameItem}.itm");
                    continue;
                }

                // Find affix with a snoPassivePower
                foreach (var forcedAffix in itemMeta.arForcedAffixes)
                {
                    int idSno = forcedAffix.__raw__;
                    string idName = forcedAffix.name;
                    var affixMeta = _affixMetaJsonList.FirstOrDefault(a => a.__snoID__ == idSno);

                    if (affixMeta == null)
                    {
                        Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Skipped. Affix not found. {idSno} {idName}");
                        continue;
                    }

                    // Skip all normal affixes. Only need the aspect.
                    if (affixMeta.snoPassivePower == null && affixMeta.eMagicType == 0) continue;

                    _uniqueInfoList.Add(new UniqueInfo
                    {
                        IdSno = idSno,
                        IdName = idName,
                        IdNameItem = idNameItem,
                        AllowedForPlayerClass = affixMeta.fAllowedForPlayerClass,
                        AllowedItemLabels = affixMeta.arAllowedItemLabels,
                        MagicType = affixMeta.eMagicType
                    });

                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: OK");
                }
            }

            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (Total): {watch.ElapsedMilliseconds}");
            watch.Stop();
        }

        private void UpdateUniques()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var elapsedMs = watch.ElapsedMilliseconds;

            // Update unique class
            // - Localisation data
            foreach (var unique in _uniqueInfoList)
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Processing ({unique.IdSno}) {unique.IdName}");

                // Find localisation data
                string directory = $"{Path.GetDirectoryName(CoreTOCPath)}\\..\\{_language}_Text\\meta\\StringList\\";
                string fileNameAffix = $"Affix_{unique.IdName}.stl.json";
                string fileNameItem = $"Item_{unique.IdNameItem}.stl.json";

                if (!File.Exists($"{directory}{fileNameItem}") || !File.Exists($"{directory}{fileNameAffix}"))
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Skipped");
                    continue;
                }

                // Get item localisation              
                var jsonAsText = File.ReadAllText($"{directory}{fileNameItem}");
                var localisation = JsonSerializer.Deserialize<Localisation>(jsonAsText);
                if (localisation != null)
                {
                    var localisationName = localisation.arStrings.FirstOrDefault(l => l.szLabel.Equals("name", StringComparison.OrdinalIgnoreCase));
                    if (localisationName != null)
                    {
                        // Remove variants (no idea where to get the correct form, so using the first one for now)
                        unique.Name = localisationName.szText.Contains("]") ? localisationName.szText.Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries)[0] : localisationName.szText;
                        //unique.Name = localisationName.szText.Contains("]") ? localisationName.szText.Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries)[1] : localisationName.szText;
                    }
                }

                // Get affix/aspect localisation
                jsonAsText = File.ReadAllText($"{directory}{fileNameAffix}");
                localisation = JsonSerializer.Deserialize<Localisation>(jsonAsText);
                if (localisation != null)
                {
                    var localisationDesc = localisation.arStrings.FirstOrDefault(l => l.szLabel.Equals("desc", StringComparison.OrdinalIgnoreCase));
                    if (localisationDesc != null)
                    {
                        unique.Localisation = localisationDesc.szText;
                        unique.Description = localisationDesc.szText;
                    }
                }
            }

            // Remove skipped
            _uniqueInfoList.RemoveAll(u => string.IsNullOrWhiteSpace(u.Localisation) || string.IsNullOrWhiteSpace(u.Name));
            // Remove not implemented
            // - Amulet_Unique_Generic_102 (Eye of the Depths)
            _uniqueInfoList.RemoveAll(u => u.Localisation.Length < 20); // For most languages set as TBD.
            // Remove test items
            _uniqueInfoList.RemoveAll(u =>
                u.IdNameItem.Equals("Gloves_Unique_Barbarian_099") ||
                u.IdNameItem.Equals("Gloves_Unique_Druid_95") ||
                u.IdNameItem.Equals("Gloves_Unique_Druid_97") ||
                u.IdNameItem.Equals("Gloves_Unique_Necromancer_99") ||
                u.IdNameItem.Equals("Helm_Unique_Druid_95") ||
                u.IdNameItem.Equals("Helm_Unique_Generic_125") ||
                u.IdNameItem.Equals("Helm_Unique_Necro_95") ||
                u.IdNameItem.Equals("Helm_Unique_Necro_98") ||
                u.IdNameItem.Equals("Helm_Unique_Rogue_95") ||
                u.IdNameItem.Equals("Pants_Unique_Barbarian_099")
            );
            // Remove duplicates
            _uniqueInfoList.RemoveAll(u =>
                u.IdNameItem.Equals("Boots_Unique_Generic_125") // Boots_Unique_Generic_003
            );

            // Replace numeric value placeholders
            ReplacePlaceholders();

            // Add a cleaned up description for fuzzy searches.
            AddCleanDescription();

            // Sort
            _uniqueInfoList.Sort((x, y) =>
            {
                return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
            });

            // Save
            SaveUniques();

            watch.Stop();
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (Total): {watch.ElapsedMilliseconds}");
        }

        private void SaveUniques()
        {
            string fileName = $"Data/Uniques.{_language}.json";
            string path = Path.GetDirectoryName(fileName) ?? string.Empty;
            Directory.CreateDirectory(path);

            using FileStream stream = File.Create(fileName);
            var options = new JsonSerializerOptions { WriteIndented = true };
            options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            JsonSerializer.Serialize(stream, _uniqueInfoList, options);
        }

        private void ValidateUniques()
        {
            var duplicates = _uniqueInfoList.GroupBy(a => a.Description).Where(a => a.Count() > 1);
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

        private void AddCleanDescription()
        {
            foreach (var aspect in _uniqueInfoList)
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

        private void ReplacePlaceholders()
        {
            // Note: https://regex101.com/

            foreach (var unique in _uniqueInfoList)
            {
                //string pattern = @"\[(.*?%+?.*?)\]";
                //aspect.Description = Regex.Replace(aspect.Description, pattern, "#%");

                //pattern = @"\[(.*?)\]";
                //aspect.Description = Regex.Replace(aspect.Description, pattern, "#");

                string pattern = @"\[([^%]+?)\]";
                unique.Description = Regex.Replace(unique.Description, pattern, "#");

                pattern = @"\[(.+?)\]";
                unique.Description = Regex.Replace(unique.Description, pattern, "#%");

                pattern = @"{(.*?)}";
                unique.Description = Regex.Replace(unique.Description, pattern, string.Empty);
            }
        }
        #endregion
    }
}
