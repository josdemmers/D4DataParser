using D4DataParser.Entities;
using D4DataParser.Entities.D4Data;
using D4DataParser.Helpers;
using D4DataParser.Mappings;
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
    /// <summary>
    /// Provides functionality for parsing and processing unique item data from game files.
    /// </summary>
    /// <remarks>
    /// Parser loops through all items using _uniqueItemDictionary. That is based on CoreTOC.dat.json section 42. The list is filtered by only acception entries starting with Item_ and containing _Unique_.
    ///
    /// The item data for each entry is found using _itemMetaJsonList. That is based on the files available in \json\base\meta\Item\.
    /// Entries from _uniqueItemDictionary and _itemMetaJsonList match when the value from _uniqueItemDictionary matches the __fileName__ property of _itemMetaJsonList.
    ///
    /// e.g.
    /// Item_1HAxe_Unique_Generic_001
    /// "__fileName__": "base/meta/Item/1HAxe_Unique_Generic_001.itm",
    /// 
    /// For each item the passive power(affix) is located using the arForcedAffixes property of _itemMetaJsonList.
    /// 
    /// Results are saved in uniqueInfoList of type UniqueInfo.
    /// 
    /// Localisation data:
    /// Loops through each item in the uniqueInfoList. Looks for data in json\enUS_Text\meta\StringList\.
    /// </remarks>
    public class UniqueParser
    {
        private string _d4dataPath = string.Empty;
        private string _language = string.Empty;
        private List<string> _languages = new List<string>();
        private Dictionary<string, List<UniqueInfo>> _uniqueInfoDictionary = new Dictionary<string, List<UniqueInfo>>();

        // D4Data repo data
        private Dictionary<int, string> _uniqueItemDictionary = new Dictionary<int, string>();
        private List<AffixMeta> _affixMetaJsonList = new List<AffixMeta>();
        private List<ItemMeta> _itemMetaJsonList = new List<ItemMeta>();

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
            _uniqueItemDictionary.Clear();
            _itemMetaJsonList.Clear();
            _affixMetaJsonList.Clear();
            _uniqueInfoDictionary.Clear();

            // Parse CoreTOC.dat.json
            var jsonAsText = File.ReadAllText(CoreTOCPath);
            var coreTOC = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, Dictionary<int, string>>>(jsonAsText);
            var coreTOCUniques = coreTOC[42];

            foreach (var item in coreTOCUniques.Where(kvp =>
                !kvp.Value.StartsWith("Item_", StringComparison.InvariantCultureIgnoreCase) ||
                !kvp.Value.Contains("_Unique_", StringComparison.InvariantCultureIgnoreCase) ||
                kvp.Value.Contains("_TEST_", StringComparison.InvariantCultureIgnoreCase) ||
                kvp.Value.Contains("_NoPowers", StringComparison.InvariantCultureIgnoreCase)).ToList())
            {
                coreTOCUniques.Remove(item.Key);
            }
            _uniqueItemDictionary = coreTOCUniques;

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

            foreach (var language in _languages)
            {
                if (Directory.Exists($"{_d4dataPath}json\\{language}_Text\\meta\\StringList\\"))
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {language}");
                    ParseByLanguage(language);
                }
                else
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Skipped {language}, not available.");
                }   
            }

            watch.Stop();
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (Total): {watch.ElapsedMilliseconds}");
        }

        private void ParseByLanguage(string language)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var elapsedMs = watch.ElapsedMilliseconds;

            _language = language;

            var uniqueInfoList = new List<UniqueInfo>();
            _uniqueInfoDictionary[language] = uniqueInfoList;
            foreach (var item in _uniqueItemDictionary)
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

                    string idNameItemActor = itemMeta.snoActor.name;
                    uniqueInfoList.Add(new UniqueInfo
                    {
                        IdSno = idSno.ToString(),
                        IdName = idName,
                        IdNameItem = idNameItem,
                        IdNameItemActor = idNameItemActor,
                        AllowedForPlayerClass = affixMeta.fAllowedForPlayerClass,
                        AllowedItemLabels = affixMeta.arAllowedItemLabels,
                        MagicType = affixMeta.eMagicType
                    });
                }
            }

            // Update unique class
            // - Localisation data
            foreach (var unique in uniqueInfoList)
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Processing ({unique.IdSno}) {unique.IdName} {unique.IdNameItem}");

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
                        var names = localisationName.szText.Contains("]") ? localisationName.szText.Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries) : [];
                        unique.Name = names.Count() > 1 ? names[1] : localisationName.szText;
                        unique.Name = unique.Name.Trim();
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
            uniqueInfoList.RemoveAll(u => string.IsNullOrWhiteSpace(u.Localisation) || string.IsNullOrWhiteSpace(u.Name));
            // Remove not implemented
            uniqueInfoList.RemoveAll(u => u.Localisation.Length < 20); // For most languages set as TBD.
            uniqueInfoList.RemoveAll(u => u.Name.StartsWith("PH"));
            uniqueInfoList.RemoveAll(u => u.Name.StartsWith("(PH)"));
            uniqueInfoList.RemoveAll(u => u.Name.StartsWith("[PH]"));
            // Remove test items
            uniqueInfoList.RemoveAll(u =>
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

            // Replace numeric value placeholders
            ReplacePlaceholders();

            // Add a cleaned up description for fuzzy searches.
            AddCleanDescription();

            // Sort
            uniqueInfoList.Sort((x, y) =>
            {
                return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
            });

            // Combine similar uniques - Create lists
            foreach (var uniqueInfo in uniqueInfoList)
            {
                var uniqueInfoDuplicates = uniqueInfoList.FindAll(a => a.DescriptionClean.Equals(uniqueInfo.DescriptionClean));
                foreach (var uniqueInfoDuplicate in uniqueInfoDuplicates)
                {
                    if (!uniqueInfo.IdSnoList.Any(u => u.Equals(uniqueInfoDuplicate.IdSno)))
                    {
                        uniqueInfo.IdSnoList.Add(uniqueInfoDuplicate.IdSno);
                    }
                    if (!uniqueInfo.IdNameList.Any(u => u.Equals(uniqueInfoDuplicate.IdName)))
                    {
                        uniqueInfo.IdNameList.Add(uniqueInfoDuplicate.IdName);
                    }
                    if (!uniqueInfo.IdNameItemList.Any(u => u.Equals(uniqueInfoDuplicate.IdNameItem)))
                    {
                        uniqueInfo.IdNameItemList.Add(uniqueInfoDuplicate.IdNameItem);
                    }
                }
            }

            // Combine similar uniques - Sort. To keep output files consistent across versions.
            foreach (var uniqueInfo in uniqueInfoList)
            {
                uniqueInfo.IdSnoList.Sort((x, y) =>
                {
                    return string.Compare(x, y, StringComparison.Ordinal);
                });
                uniqueInfo.IdNameList.Sort((x, y) =>
                {
                    return string.Compare(x, y, StringComparison.Ordinal);
                });
                uniqueInfo.IdNameItemList.Sort((x, y) =>
                {
                    return string.Compare(x, y, StringComparison.Ordinal);
                });
            }

            // Combine similar uniques - Update sno/name
            foreach (var uniqueInfo in uniqueInfoList)
            {
                uniqueInfo.IdSno = string.Join(";", uniqueInfo.IdSnoList);
                uniqueInfo.IdName = string.Join(";", uniqueInfo.IdNameList);
                uniqueInfo.IdNameItem = string.Join(";", uniqueInfo.IdNameItemList);
            }

            // Save
            SaveUniques();

            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (Total): {watch.ElapsedMilliseconds}");
            watch.Stop();
        }

        private void SaveUniques()
        {
            // Create export list without any duplicates
            var uniqueInfoList = _uniqueInfoDictionary[_language];
            var uniqueInfoListExport = new List<UniqueInfo>();
            foreach (var uniqueInfo in uniqueInfoList)
            {
                if (uniqueInfoListExport.Any(a => a.DescriptionClean.Equals(uniqueInfo.DescriptionClean))) continue;

                uniqueInfoListExport.Add(uniqueInfo);
            }

            string fileName = $"Data/Uniques.{_language}.json";
            string path = Path.GetDirectoryName(fileName) ?? string.Empty;
            Directory.CreateDirectory(path);

            using FileStream stream = File.Create(fileName);
            var options = new JsonSerializerOptions { WriteIndented = true };
            options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            JsonSerializer.Serialize(stream, uniqueInfoListExport, options);
        }

        private void ValidateUniques(string language)
        {
            var uniqueInfoList = _uniqueInfoDictionary[language];

            var duplicates = uniqueInfoList.GroupBy(a => a.Description).Where(a => a.Count() > 1);
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
            var uniqueInfoList = _uniqueInfoDictionary[_language];

            foreach (var aspect in uniqueInfoList)
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
            var uniqueInfoList = _uniqueInfoDictionary[_language];

            // Note: https://regex101.com/

            foreach (var unique in uniqueInfoList)
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

                foreach (var placeholder in StringPlaceholderMappings.StringPlaceholder)
                {
                    unique.Description = unique.Description.Replace(placeholder.Key, placeholder.Value);
                }
            }
        }
        #endregion
    }
}
