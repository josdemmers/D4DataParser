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
    public class AspectParser
    {
        private string _d4dataPath = string.Empty;
        private string _language = string.Empty;
        private List<string> _languages = new List<string>();

        private List<AffixMeta> _affixMetaJsonList = new List<AffixMeta>();
        private List<AspectMeta> _aspectMetaJsonList = new List<AspectMeta>();
        private List<AspectInfo> _aspectInfoList = new List<AspectInfo>();
        private List<TrackedRewardMeta> _trackedRewardCodexMetaJsonList = new List<TrackedRewardMeta>();
        private List<TrackedRewardMeta> _trackedRewardSeasonalMetaJsonList = new List<TrackedRewardMeta>();

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

        public void ParseAffixes()
        {
            foreach (var language in _languages)
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {language}");

                // TODO: - DEV - Comment language skip for release
                //if (!language.Equals("enUS")) continue;

                ParseAffixesByLanguage(language);
                UpdateAspects();

                ValidateAspects();
            }
        }

        private void ParseAffixesByLanguage(string language)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var elapsedMs = watch.ElapsedMilliseconds;

            _language = language;

            // reset
            _aspectMetaJsonList.Clear();
            _affixMetaJsonList.Clear();
            _aspectInfoList.Clear();

            // Parse CoreTOC.dat.json
            var jsonAsText = File.ReadAllText(CoreTOCPath);
            var coreTOC = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, Dictionary<int, string>>>(jsonAsText);
            var coreTOCAffixes = coreTOC[104];
            var coreTOCAspects = coreTOC[128];
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (CoreTOC.dat): {watch.ElapsedMilliseconds - elapsedMs}");
            elapsedMs = watch.ElapsedMilliseconds;

            // Parse .\d4data\json\base\meta\Affix\
            _affixMetaJsonList = new List<AffixMeta>();
            var directory = $"{Path.GetDirectoryName(CoreTOCPath)}\\meta\\Affix\\";
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
            directory = $"{Path.GetDirectoryName(CoreTOCPath)}\\meta\\Aspect\\";
            if (Directory.Exists(directory))
            {
                var fileEntries = Directory.EnumerateFiles(directory).Where(file => Path.GetFileName(file).StartsWith("Asp_Legendary_", StringComparison.OrdinalIgnoreCase));
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
            _trackedRewardSeasonalMetaJsonList = new List<TrackedRewardMeta>();
            directory = $"{Path.GetDirectoryName(CoreTOCPath)}\\meta\\TrackedReward\\";
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

                // TODO: - UPD - Requires update when season changes.
                // Seasonal
                fileEntries = Directory.EnumerateFiles(directory).Where(file => Path.GetFileName(file).StartsWith("TR_SJ_S04_", StringComparison.OrdinalIgnoreCase));
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
                            _trackedRewardSeasonalMetaJsonList.Add(trackedRewardMetaJson);
                        }
                    }
                }
            }
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (TrackedReward folder): {watch.ElapsedMilliseconds - elapsedMs}");
            elapsedMs = watch.ElapsedMilliseconds;

            // Create aspect class
            foreach (var aspect in coreTOCAspects)
            {
                var aspectMeta = _aspectMetaJsonList.FirstOrDefault(a => a.__snoID__ == aspect.Key);
                if (aspectMeta == null)
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Skipping {aspect.Key}: {aspect.Value}.");
                    continue;
                }

                _aspectInfoList.Add(new AspectInfo
                {
                    IdSno = aspectMeta.snoAffix.__raw__,
                    IdName = aspectMeta.snoAffix.name,
                    IsCodex = true
                });                
            }

            // Add remaining aspects
            // Note: No longer needed since start of season 4, all aspects are included in coreTOCAspects (Codex)

            //Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: coreTOCAffixes");
            //foreach (var affix in coreTOCAffixes)
            //{
            //    if (!affix.Value.StartsWith("Legendary_Generic", StringComparison.OrdinalIgnoreCase) &&
            //        !affix.Value.StartsWith("Legendary_Barb", StringComparison.OrdinalIgnoreCase) &&
            //        !affix.Value.StartsWith("Legendary_Druid", StringComparison.OrdinalIgnoreCase) &&
            //        !affix.Value.StartsWith("Legendary_Necro", StringComparison.OrdinalIgnoreCase) &&
            //        !affix.Value.StartsWith("Legendary_Rogue", StringComparison.OrdinalIgnoreCase) &&
            //        !affix.Value.StartsWith("Legendary_Sorc", StringComparison.OrdinalIgnoreCase))
            //    {
            //        Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Skipping {affix.Key}: {affix.Value}. Not an aspect.");
            //        continue;
            //    }

            //    if (_aspectInfoList.Any(aspect => aspect.IdSno == affix.Key))
            //    {
            //        Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Skipping {affix.Key}: {affix.Value}. Duplicate.");
            //        continue;
            //    }

            //    _aspectInfoList.Add(new AspectInfo
            //    {
            //        IdSno = affix.Key,
            //        IdName = affix.Value,
            //        IsCodex = false
            //    });
            //}

            // TODO: Check if aspect is enabled by looking through all power files?
            // Local function
            //bool IsAspectEnabled(string idName)
            //{
            //    var coreTOCEnabled = coreTOC[29];
            //    bool isEnabled = coreTOCEnabled.Values.Any(a => a.Equals(idName, StringComparison.OrdinalIgnoreCase)) ||
            //        coreTOCAspects.Values.Any(a => a.EndsWith(idName, StringComparison.OrdinalIgnoreCase));

            //    return isEnabled;
            //}

            // Remove disabled aspects
            //_aspectInfoList.RemoveAll(aspect => !IsAspectEnabled(aspect.IdName));
            _aspectInfoList.RemoveAll(aspect => aspect.IdName.Equals("legendary_necro_126", StringComparison.OrdinalIgnoreCase)); // (PH) Shadow Warriors (added by coreTOCAspects)
            _aspectInfoList.RemoveAll(aspect => aspect.IdName.Equals("legendary_sorc_034", StringComparison.OrdinalIgnoreCase)); // (PH) of Ensnaring Current (added by coreTOCAspects)
            _aspectInfoList.RemoveAll(aspect => aspect.IdName.Equals("legendary_sorc_139", StringComparison.OrdinalIgnoreCase)); // (PH) Split Incinerate (added by coreTOCAspects)

            watch.Stop();
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (Total): {watch.ElapsedMilliseconds}");
        }

        private void UpdateAspects()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var elapsedMs = watch.ElapsedMilliseconds;

            // Update aspect class
            // - Allowed classes
            // - Allowed item labels
            // - MagicType (affix, aspect)
            // - Localisation data
            // - Seasonal
            foreach (var aspect in _aspectInfoList)
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
                string directory = $"{Path.GetDirectoryName(CoreTOCPath)}\\..\\{_language}_Text\\meta\\StringList\\";
                string fileName = affixMeta.__fileName__;
                string fileNameLoc = $"{directory}Affix_{Path.GetFileNameWithoutExtension(fileName)}.stl.json";
                var jsonAsText = File.ReadAllText(fileNameLoc);
                var localisation = JsonSerializer.Deserialize<Localisation>(jsonAsText);
                if (localisation != null)
                {
                    var aspectInfo = localisation.arStrings.FirstOrDefault(l => l.szLabel.Equals("name", StringComparison.OrdinalIgnoreCase));
                    if (aspectInfo != null)
                    {
                        aspect.Name = aspectInfo.szText;
                    }
                    aspectInfo = localisation.arStrings.FirstOrDefault(l => l.szLabel.Equals("desc", StringComparison.OrdinalIgnoreCase));
                    if (aspectInfo != null)
                    {
                        aspect.Localisation = aspectInfo.szText;
                        aspect.Description = aspectInfo.szText;
                    }
                }

                // Find seasonal data
                aspect.IsSeasonal = _trackedRewardSeasonalMetaJsonList.Any(t => t.snoAspect?.name.EndsWith(aspect.IdName, StringComparison.OrdinalIgnoreCase) ?? false);

                // Find dungeon data
                aspect.Dungeon = GetAspectDungeon(aspect);
            }

            // Replace numeric value placeholders
            ReplacePlaceholders();

            // Add a cleaned up description for fuzzy searches.
            AddCleanDescription();

            // Sort
            _aspectInfoList.Sort((x, y) =>
            {
                return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
            });

            // Save
            SaveAspects();

            watch.Stop();
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (Total): {watch.ElapsedMilliseconds}");
        }

        private void SaveAspects()
        {
            string fileName = $"Data/Aspects.{_language}.json";
            string path = Path.GetDirectoryName(fileName) ?? string.Empty;
            Directory.CreateDirectory(path);

            using FileStream stream = File.Create(fileName);
            var options = new JsonSerializerOptions { WriteIndented = true };
            options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            JsonSerializer.Serialize(stream, _aspectInfoList, options);
        }

        private void ValidateAspects()
        {
            var duplicates = _aspectInfoList.GroupBy(a => a.Description).Where(a => a.Count() > 1);
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
            string directory = $"{Path.GetDirectoryName(CoreTOCPath)}\\..\\{_language}_Text\\meta\\StringList\\";
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

        private void AddCleanDescription()
        {
            foreach (var aspect in _aspectInfoList)
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

            foreach (var aspect in _aspectInfoList)
            {
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

                // deDE
                aspect.Description = aspect.Description.Replace("|4Wirkung:Wirkungen;", "Wirkungen");

                // enUS
                aspect.Description = aspect.Description.Replace("|4cast:casts;", "casts");

                // esES
                aspect.Description = aspect.Description.Replace("|4lanzamiento adicional:lanzamientos adicionales;", "lanzamientos adicionales");

                // esMX
                aspect.Description = aspect.Description.Replace("|4lanzamiento adicional:lanzamientos adicionales;", "lanzamientos adicionales");

                // frFR
                aspect.Description = aspect.Description.Replace("|4lancer supplémentaire:lancers supplémentaires;", "lancers supplémentaires");

                // itIT
                aspect.Description = aspect.Description.Replace("|4utilizzo:utilizzi;", "utilizzi");

                // jaJP
                aspect.Description = aspect.Description.Replace("|4cast:casts;", "casts");

                // plPL
                aspect.Description = aspect.Description.Replace("|4dodatkowe użycie:dodatkowe użycia:dodatkowych użyć;", "dodatkowych użyć");

                // ptBR
                aspect.Description = aspect.Description.Replace("|4lançamento:lançamentos;", "lançamentos");

                // ruRU
                aspect.Description = aspect.Description.Replace("|4применение:применения:применений;", "применений");

                // trTR
                aspect.Description = aspect.Description.Replace("|4kullanım:kullanım;", "kullanım");
            }
        }
        #endregion
    }
}
