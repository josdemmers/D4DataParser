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
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace D4DataParser.Parsers
{
    public class AffixParser
    {
        private string _d4dataPath = string.Empty;
        private string _language = string.Empty;
        private List<string> _languages = new List<string>();
        private Dictionary<string, List<AffixInfo>> _affixInfoDictionary = new Dictionary<string, List<AffixInfo>>();

        // D4Data repo data
        private Dictionary<int, string> _affixDictionary = new Dictionary<int, string>();
        private List<AffixMeta> _affixMetaJsonList = new List<AffixMeta>();
        List<PowerMeta> _powerMetaJsonList = new List<PowerMeta>();
        private Dictionary<uint, List<string>> _skillTagDictionary = new Dictionary<uint, List<string>>();
        private Dictionary<uint, string> _weaponTypeDictionary = new Dictionary<uint, string>();
        private Localisation _attributeDescriptions = new Localisation();
        private Localisation _frontEnd = new Localisation();
        private Localisation _itemRequirements = new Localisation();
        private Localisation _necromancerArmy = new Localisation();
        private Localisation _skillTagNames = new Localisation();
        private Localisation _skillTags = new Localisation();
        private Localisation _uiToolTips = new Localisation();

        // Start of Constructors region

        #region Constructors

        public AffixParser()
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

        public List<AffixInfo> GetAffixInfoByLanguage(string language)
        {
            if (_affixInfoDictionary.TryGetValue(language, out var affixInfoList))
            {
                return affixInfoList;
            }
            return new List<AffixInfo>();

        }

        public void Parse()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var elapsedMs = watch.ElapsedMilliseconds;

            // Reset
            _affixDictionary.Clear();
            _affixMetaJsonList.Clear();
            _affixInfoDictionary.Clear();
            _powerMetaJsonList.Clear();
            _skillTagDictionary.Clear();

            // Parse CoreTOC.dat.json -- Affixes
            string coreTOCPath = $"{_d4dataPath}json\\base\\CoreTOC.dat.json";
            int affixIndex = 104;
            var jsonAsText = File.ReadAllText(coreTOCPath);
            var coreTOCDictionary = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, Dictionary<int, string>>>(jsonAsText) ?? new Dictionary<int, Dictionary<int, string>>();
            _affixDictionary = coreTOCDictionary.ContainsKey(affixIndex) ? coreTOCDictionary[affixIndex] : new Dictionary<int, string>();
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (CoreTOC.dat.json - Affixes): {watch.ElapsedMilliseconds - elapsedMs}");
            elapsedMs = watch.ElapsedMilliseconds;

            // Parse CoreTOC.dat.json -- WeaponTypes
            int weaponTypeIndex = 116;
            jsonAsText = File.ReadAllText(coreTOCPath);
            var coreTOCDictionaryUInt = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, Dictionary<uint, string>>>(jsonAsText) ?? new Dictionary<int, Dictionary<uint, string>>();
            _weaponTypeDictionary = coreTOCDictionaryUInt.ContainsKey(weaponTypeIndex) ? coreTOCDictionaryUInt[weaponTypeIndex] : new Dictionary<uint, string>();
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (CoreTOC.dat.json - WeaponTypes): {watch.ElapsedMilliseconds - elapsedMs}");
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

            // Parse .\d4data\json\base\meta\Power\
            directory = $"{Path.GetDirectoryName(coreTOCPath)}\\meta\\Power\\";
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
                            //options.Converters.Add(new IntConverter());

                            var powerMetaJson = JsonSerializer.Deserialize<PowerMeta>(stream, options) ?? new PowerMeta();
                            _powerMetaJsonList.Add(powerMetaJson);
                        }
                    }
                }
            }
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (Power folder): {watch.ElapsedMilliseconds - elapsedMs}");
            elapsedMs = watch.ElapsedMilliseconds;

            // Parse ".\d4data\json\GBID.json"
            string gbidPath = $"{_d4dataPath}json\\GBID.json";
            int skillTagIndex = 56;
            jsonAsText = File.ReadAllText(gbidPath);
            var gbidDictionary = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, Dictionary<uint, List<string>>>>(jsonAsText) ?? new Dictionary<int, Dictionary<uint, List<string>>>();
            _skillTagDictionary = gbidDictionary[skillTagIndex];
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (GBID.json): {watch.ElapsedMilliseconds - elapsedMs}");
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
        }

        private void ParseByLanguage(string language)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var elapsedMs = watch.ElapsedMilliseconds;

            _language = language;

            // Reset
            _attributeDescriptions = new Localisation();
            _frontEnd = new Localisation();
            _itemRequirements = new Localisation();
            _necromancerArmy = new Localisation();
            _skillTagNames = new Localisation();
            _skillTags = new Localisation();
            _uiToolTips = new Localisation();

            // Parse ".\d4data\json\{language}_Text\meta\StringList\AttributeDescriptions.stl.json"
            var directory = $"{_d4dataPath}json\\{_language}_Text\\meta\\StringList\\";
            if (Directory.Exists(directory))
            {
                string fileName = $"{directory}AttributeDescriptions.stl.json";
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
                        //options.Converters.Add(new IntConverter());

                        _attributeDescriptions = JsonSerializer.Deserialize<Localisation>(stream, options) ?? new Localisation();
                    }
                }
            }

            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (AttributeDescriptions.stl.json): {watch.ElapsedMilliseconds - elapsedMs}");
            elapsedMs = watch.ElapsedMilliseconds;

            // Parse ".\d4data\json\{language}_Text\meta\StringList\FrontEnd.stl.json"
            directory = $"{_d4dataPath}json\\{_language}_Text\\meta\\StringList\\";
            if (Directory.Exists(directory))
            {
                string fileName = $"{directory}FrontEnd.stl.json";
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
                        //options.Converters.Add(new IntConverter());

                        _frontEnd = JsonSerializer.Deserialize<Localisation>(stream, options) ?? new Localisation();
                    }
                }
            }
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (FrontEnd.stl.json): {watch.ElapsedMilliseconds - elapsedMs}");
            elapsedMs = watch.ElapsedMilliseconds;

            // Parse ".\d4data\json\{language}_Text\meta\StringList\ItemRequirements.stl.json"
            directory = $"{_d4dataPath}json\\{_language}_Text\\meta\\StringList\\";
            if (Directory.Exists(directory))
            {
                string fileName = $"{directory}ItemRequirements.stl.json";
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
                        //options.Converters.Add(new IntConverter());

                        _itemRequirements = JsonSerializer.Deserialize<Localisation>(stream, options) ?? new Localisation();
                    }
                }
            }
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (ItemRequirements.stl.json): {watch.ElapsedMilliseconds - elapsedMs}");
            elapsedMs = watch.ElapsedMilliseconds;

            // Parse ".\d4data\json\{language}_Text\meta\StringList\NecromancerArmy.stl.json"
            directory = $"{_d4dataPath}json\\{_language}_Text\\meta\\StringList\\";
            if (Directory.Exists(directory))
            {
                string fileName = $"{directory}NecromancerArmy.stl.json";
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
                        //options.Converters.Add(new IntConverter());

                        _necromancerArmy = JsonSerializer.Deserialize<Localisation>(stream, options) ?? new Localisation();
                    }
                }
            }
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (NecromancerArmy.stl.json): {watch.ElapsedMilliseconds - elapsedMs}");
            elapsedMs = watch.ElapsedMilliseconds;

            // Parse ".\d4data\json\{language}_Text\meta\StringList\SkillTagNames.stl.json"
            directory = $"{_d4dataPath}json\\{_language}_Text\\meta\\StringList\\";
            if (Directory.Exists(directory))
            {
                string fileName = $"{directory}SkillTagNames.stl.json";
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
                        //options.Converters.Add(new IntConverter());

                        _skillTagNames = JsonSerializer.Deserialize<Localisation>(stream, options) ?? new Localisation();
                    }
                }
            }
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (SkillTagNames.stl.json): {watch.ElapsedMilliseconds - elapsedMs}");
            elapsedMs = watch.ElapsedMilliseconds;

            // Parse ".\d4data\json\{language}_Text\meta\StringList\SkillTags.stl.json"
            directory = $"{_d4dataPath}json\\{_language}_Text\\meta\\StringList\\";
            if (Directory.Exists(directory))
            {
                string fileName = $"{directory}SkillTags.stl.json";
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
                        //options.Converters.Add(new IntConverter());

                        _skillTags = JsonSerializer.Deserialize<Localisation>(stream, options) ?? new Localisation();
                    }
                }
            }
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (SkillTags.stl.json): {watch.ElapsedMilliseconds - elapsedMs}");
            elapsedMs = watch.ElapsedMilliseconds;

            // Parse ".\d4data\json\{language}_Text\meta\StringList\UIToolTips.stl.json"
            directory = $"{_d4dataPath}json\\{_language}_Text\\meta\\StringList\\";
            if (Directory.Exists(directory))
            {
                string fileName = $"{directory}UIToolTips.stl.json";
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
                        //options.Converters.Add(new IntConverter());

                        _uiToolTips = JsonSerializer.Deserialize<Localisation>(stream, options) ?? new Localisation();
                    }
                }
            }
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (UIToolTips.stl.json): {watch.ElapsedMilliseconds - elapsedMs}");
            elapsedMs = watch.ElapsedMilliseconds;

            // Initialise AffixInfo
            var affixInfoList = new List<AffixInfo>();
            _affixInfoDictionary[language] = affixInfoList;
            foreach (var affixEntry in _affixDictionary)
            {
                var affixMeta = _affixMetaJsonList.FirstOrDefault(affix => affix.__snoID__ == affixEntry.Key);
                if (affixMeta != null)
                {
                    affixInfoList.Add(new AffixInfo
                    {
                        IdSno = affixEntry.Key.ToString(),
                        IdName = affixEntry.Value
                    });
                }
                else
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Missing affix meta. ({affixEntry.Key}) {affixEntry.Value}");
                }
            }
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Current affix count: {affixInfoList.Count}");
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (Initialise AffixInfo): {watch.ElapsedMilliseconds - elapsedMs}");
            elapsedMs = watch.ElapsedMilliseconds;

            // Update AffixInfo
            // - eAffixType
            // - eCategory
            // - dwFlags
            // - bIsTemperedAffix
            // - eMagicType
            // - fAllowedForPlayerClass
            // - arAllowedItemLabels
            // - ptItemAffixAttributes
            foreach (var affixInfo in affixInfoList)
            {
                var affixMeta = _affixMetaJsonList.FirstOrDefault(affix => affix.__snoID__.ToString().Equals(affixInfo.IdSno));
                if (affixMeta != null)
                {
                    affixInfo.AffixType = affixMeta.eAffixType;
                    affixInfo.Category = affixMeta.eCategory;
                    affixInfo.Flags = affixMeta.dwFlags;
                    affixInfo.IsTemperingAvailable = affixMeta.bIsTemperedAffix;
                    affixInfo.MagicType = affixMeta.eMagicType;

                    affixInfo.AllowedForPlayerClass = affixMeta.fAllowedForPlayerClass ?? new List<int>();
                    affixInfo.AllowedItemLabels = affixMeta.arAllowedItemLabels ?? new List<int>();

                    // Bug - Fix sorc affixes
                    if (affixInfo.IdName.Contains("_Sorc_", StringComparison.OrdinalIgnoreCase))
                    {
                        affixInfo.AllowedForPlayerClass = new List<int> { 1, 0, 0, 0, 0, 0, 0 };
                    }

                    // Bug - Fix necro affixes
                    if (affixInfo.IdName.Contains("_Necro_", StringComparison.OrdinalIgnoreCase))
                    {
                        affixInfo.AllowedForPlayerClass = new List<int> { 0, 0, 0, 0, 1, 0, 0 };
                    }

                    var itemAffixAttributes = affixMeta.ptItemAffixAttributes ?? new List<PtItemAffixAttribute>();
                    foreach (var itemAffixAttribute in itemAffixAttributes)
                    {
                        string localisationId = itemAffixAttribute.tAttribute.__eAttribute_name__ ?? string.Empty;
                        if (string.IsNullOrWhiteSpace(localisationId)) continue;

                        affixInfo.AffixAttributes.Add(new AffixAttribute
                        {
                            LocalisationId = itemAffixAttribute.tAttribute.__eAttribute_name__ ?? string.Empty,
                            LocalisationParameter = itemAffixAttribute.tAttribute.nParam,
                            LocalisationAttributeFormulaValue = itemAffixAttribute?.tAttribute?.szAttributeFormula?.value ?? string.Empty
                        });
                    }

                    if (affixInfo.AffixAttributes.Count == 0)
                    {
                        Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Missing localisation data. ({affixInfo.IdSno}) {affixInfo.IdName}");
                    }
                }
            }
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (Update AffixInfo): {watch.ElapsedMilliseconds - elapsedMs}");
            elapsedMs = watch.ElapsedMilliseconds;

            // Cleanup affixes
            // - Remove aspects
            // - Remove all affixes starting with "zz"
            // - Remove all affixes with no localisation data
            // - Remove all affixes with dual resistance
            affixInfoList.RemoveAll(a => a.MagicType != 0);
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Current affix count: {affixInfoList.Count}");
            affixInfoList.RemoveAll(a => a.IdName.StartsWith("zz"));
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Current affix count: {affixInfoList.Count}");
            affixInfoList.RemoveAll(a => a.AffixAttributes.Count == 0);
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Current affix count: {affixInfoList.Count}");
            affixInfoList.RemoveAll(a => a.IdName.Contains("_Resistance_", StringComparison.OrdinalIgnoreCase) && a.IdName.Contains("_Dual_", StringComparison.OrdinalIgnoreCase));
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Current affix count: {affixInfoList.Count}");
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (Cleanup AffixInfo): {watch.ElapsedMilliseconds - elapsedMs}");
            elapsedMs = watch.ElapsedMilliseconds;

            // Update localisation ids
            // - Replace with sub localisation id when available.
            foreach (var affixInfo in affixInfoList)
            {
                foreach (var affixAttributes in affixInfo.AffixAttributes)
                {
                    if (_attributeDescriptions.arStrings.Any(a => a.szLabel.StartsWith($"{affixAttributes.LocalisationId}#")))
                    {
                        uint subSno = affixAttributes.LocalisationParameter;
                        string subLocalisationId = string.Empty;
                        if (affixAttributes.LocalisationId.Equals("AoE_Size_Bonus_Per_Power") ||
                            affixAttributes.LocalisationId.Equals("Bonus_Count_Per_Power") ||
                            affixAttributes.LocalisationId.Equals("Bonus_Percent_Per_Power") ||
                            affixAttributes.LocalisationId.Equals("Bonus_Percent_Per_Power_2") ||
                            affixAttributes.LocalisationId.Equals("Bonus_Percent_Per_Power_3") ||
                            affixAttributes.LocalisationId.Equals("Chance_To_Hit_Twice_Per_Power") ||
                            affixAttributes.LocalisationId.Equals("Cleave_Damage_Bonus_Percent_Per_Power") ||
                            affixAttributes.LocalisationId.Equals("Damage_Percent_Bonus_While_Affected_By_Power") ||
                            affixAttributes.LocalisationId.Equals("MaxStacks") ||
                            affixAttributes.LocalisationId.Equals("Movement_Speed_Bonus_Percent_Per_Power") ||
                            affixAttributes.LocalisationId.Equals("Percent_Bonus_Projectiles_Per_Power") ||
                            affixAttributes.LocalisationId.Equals("Power_Cooldown_Reduction_Percent") ||
                            affixAttributes.LocalisationId.Equals("Spiritborn_Spirit_Bonus"))
                        {
                            string subId = GetPowerId(subSno);
                            if (!string.IsNullOrWhiteSpace(subId))
                            {
                                subLocalisationId = $"{affixAttributes.LocalisationId}#{subId}";
                            }
                        }
                        else if (affixAttributes.LocalisationId.Equals("Damage_Percent_Bonus_Per_Skill_Tag") ||
                            affixAttributes.LocalisationId.Equals("Damage_Percent_Bonus_To_Targets_Affected_By_Skill_Tag"))
                        {
                            string subId = GetSkillTagId(subSno);
                            if (!string.IsNullOrWhiteSpace(subId))
                            {
                                subLocalisationId = $"{affixAttributes.LocalisationId}#{subId}";
                            }
                        }
                        else if (affixAttributes.LocalisationId.Equals("Primary_Resource_Gain_Bonus_Percent_Per_Weapon_Requirement"))
                        {
                            string subId = GetWeaponId(subSno);
                            if (!string.IsNullOrWhiteSpace(subId))
                            {
                                subLocalisationId = $"{affixAttributes.LocalisationId}#{subId}";
                            }
                        }
                        else if (affixAttributes.LocalisationId.Equals("Resistance"))
                        {
                            string subId = LocalisationMappings.Resistance[affixAttributes.LocalisationParameter];
                            subLocalisationId = $"{affixAttributes.LocalisationId}#{subId}";
                        }
                        else if (affixAttributes.LocalisationId.Equals("Damage_Percent_Bonus_Against_Dot_Type"))
                        {
                            string subId = GetDamagePercentBonusAgainstDotType(subSno);
                            if (!string.IsNullOrWhiteSpace(subId))
                            {
                                subLocalisationId = $"{affixAttributes.LocalisationId}#{subId}";
                            }
                        }
                        else if (affixAttributes.LocalisationId.Equals("Damage_Percent_Reduction_From_Dotted_Enemy"))
                        {
                            string subId = GetDamagePercentReductionFromDottedEnemy(subSno);
                            if (!string.IsNullOrWhiteSpace(subId))
                            {
                                subLocalisationId = $"{affixAttributes.LocalisationId}#{subId}";
                            }
                        }
                        else if (affixAttributes.LocalisationId.Equals("DOT_DPS_Bonus_Percent_Per_Damage_Type"))
                        {
                            string subId = GetDOTDPSBonusPercentPerDamageType(subSno);
                            if (!string.IsNullOrWhiteSpace(subId))
                            {
                                subLocalisationId = $"{affixAttributes.LocalisationId}#{subId}";
                            }
                        }
                        else
                        {
                            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Sub localisation data available but rules not set. ({affixAttributes.LocalisationId})");
                            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {affixInfo.IdName}: {affixAttributes.LocalisationParameter}");

                            throw new NotImplementedException();
                        }

                        if (!string.IsNullOrWhiteSpace(subLocalisationId) && _attributeDescriptions.arStrings.Any(a => a.szLabel.Equals(subLocalisationId)))
                        {
                            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Replaced {affixAttributes.LocalisationId} with {subLocalisationId}");
                            affixAttributes.LocalisationId = subLocalisationId;
                        }
                    }
                }
            }
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (Update localisation ids): {watch.ElapsedMilliseconds - elapsedMs}");
            elapsedMs = watch.ElapsedMilliseconds;

            // Update localisation ids
            // - Replace renamed / combined localisation id
            foreach (var affixInfo in affixInfoList)
            {
                foreach (var affixAttributes in affixInfo.AffixAttributes)
                {
                    // Remaining missing localisation ids
                    // - Item_Find

                    if (affixAttributes.LocalisationId.Equals("On_Hit_Vulnerable_Proc_Chance")) affixAttributes.LocalisationId = "On_Hit_Vulnerable_Proc";
                    if (affixAttributes.LocalisationId.Equals("On_Hit_Vulnerable_Proc_Duration_Seconds")) affixAttributes.LocalisationId = "On_Hit_Vulnerable_Proc";
                    if (affixAttributes.LocalisationId.Equals("Movement_Bonus_On_Elite_Kill")) affixAttributes.LocalisationId = "Movement_Speed_Bonus_On_Elite_Kill";
                    if (affixAttributes.LocalisationId.Equals("Movement_Bonus_On_Elite_Kill_Duration")) affixAttributes.LocalisationId = "Movement_Speed_Bonus_On_Elite_Kill";
                    if (affixAttributes.LocalisationId.Equals("Weapon_On_Hit_Percent_Bleed_Proc_Chance")) affixAttributes.LocalisationId = "Weapon_On_Hit_Percent_Bleed_Proc_Chance_Combined";
                    if (affixAttributes.LocalisationId.Equals("Weapon_On_Hit_Percent_Bleed_Proc_Damage")) affixAttributes.LocalisationId = "Weapon_On_Hit_Percent_Bleed_Proc_Chance_Combined";
                    if (affixAttributes.LocalisationId.Equals("Weapon_On_Hit_Percent_Bleed_Proc_Duration")) affixAttributes.LocalisationId = "Weapon_On_Hit_Percent_Bleed_Proc_Chance_Combined";
                    if (affixAttributes.LocalisationId.Equals("Evade_Movement_Speed")) affixAttributes.LocalisationId = "Evade_Movement_Speed_Combined";
                    if (affixAttributes.LocalisationId.Equals("Evade_Movement_Speed_Duration")) affixAttributes.LocalisationId = "Evade_Movement_Speed_Combined";
                    if (affixAttributes.LocalisationId.Equals("Damage_Bonus_On_Elite_Kill")) affixAttributes.LocalisationId = "Damage_Bonus_On_Elite_Kill_Combined";
                    if (affixAttributes.LocalisationId.Equals("Damage_Bonus_On_Elite_Kill_Duration")) affixAttributes.LocalisationId = "Damage_Bonus_On_Elite_Kill_Combined";
                    if (affixAttributes.LocalisationId.Equals("Damage_Bonus_Percent_On_Dodge")) affixAttributes.LocalisationId = "Damage_Bonus_Percent_After_Dodge";
                    if (affixAttributes.LocalisationId.Equals("Damage_Bonus_Percent_On_Dodge_Duration")) affixAttributes.LocalisationId = "Damage_Bonus_Percent_After_Dodge";
                    if (affixAttributes.LocalisationId.Equals("Attack_Speed_Bonus_On_Dodge")) affixAttributes.LocalisationId = "Attack_Speed_Bonus_After_Dodge";
                    if (affixAttributes.LocalisationId.Equals("Attack_Speed_Bonus_On_Dodge_Duration")) affixAttributes.LocalisationId = "Attack_Speed_Bonus_After_Dodge";
                    if (affixAttributes.LocalisationId.Equals("Blood_Orb_Pickup_Damage_Percent_Bonus")) affixAttributes.LocalisationId = "Blood_Orb_Pickup_Damage_Combined";
                    if (affixAttributes.LocalisationId.Equals("Blood_Orb_Pickup_Damage_Bonus_Duration")) affixAttributes.LocalisationId = "Blood_Orb_Pickup_Damage_Combined";
                    if (affixAttributes.LocalisationId.Equals("Barrier_When_Struck_Percent_Chance")) affixAttributes.LocalisationId = "Barrier_When_Struck_Chance";
                    if (affixAttributes.LocalisationId.Equals("Fortified_When_Struck_Percent_Chance")) affixAttributes.LocalisationId = "Fortified_When_Struck_Chance";
                    if (affixAttributes.LocalisationId.Equals("Fortified_When_Struck_Amount")) affixAttributes.LocalisationId = "Fortified_When_Struck_Chance";
                    if (affixAttributes.LocalisationId.Equals("On_Hit_Damage_Bonus_Proc_Chance")) affixAttributes.LocalisationId = "On_Hit_Damage_Bonus_Combined";
                    if (affixAttributes.LocalisationId.Equals("On_Hit_Damage_Bonus_Percent")) affixAttributes.LocalisationId = "On_Hit_Damage_Bonus_Combined";
                    if (affixAttributes.LocalisationId.Equals("On_Hit_Damage_Bonus_Duration")) affixAttributes.LocalisationId = "On_Hit_Damage_Bonus_Combined";
                }
            }
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (Update missing localisation ids): {watch.ElapsedMilliseconds - elapsedMs}");
            elapsedMs = watch.ElapsedMilliseconds;

            // Update localisation
            foreach (var affixInfo in affixInfoList)
            {
                foreach (var affixAttribute in affixInfo.AffixAttributes)
                {
                    var affixLoc = _attributeDescriptions.arStrings.FirstOrDefault(a => a.szLabel.Equals(affixAttribute.LocalisationId, StringComparison.OrdinalIgnoreCase));
                    if (affixLoc == null)
                    {
                        Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: ({affixInfo.IdName}) Localisation id {affixAttribute.LocalisationId} not found.");
                        continue;
                    }

                    string localisationId = affixLoc.szLabel;
                    string localisation = affixLoc.szText;

                    affixAttribute.Localisation = localisation;
                    if (string.IsNullOrWhiteSpace(affixInfo.Description) || !affixInfo.Description.Equals(localisation))
                    {
                        affixInfo.Description += localisation;
                    }
                }
            }
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (Update localisation): {watch.ElapsedMilliseconds - elapsedMs}");
            elapsedMs = watch.ElapsedMilliseconds;

            // Cleanup affixes
            // - Remove all affixes with missing localisation
            affixInfoList.RemoveAll(a => string.IsNullOrWhiteSpace(a.Description));
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Current affix count: {affixInfoList.Count}");
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (Cleanup AffixInfo): {watch.ElapsedMilliseconds - elapsedMs}");
            elapsedMs = watch.ElapsedMilliseconds;

            // Replace numeric value placeholders
            ReplaceNumericValuePlaceholders();
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (Replace numeric value placeholders): {watch.ElapsedMilliseconds - elapsedMs}");
            elapsedMs = watch.ElapsedMilliseconds;

            // Fill in value placeholders
            foreach (var affixInfo in affixInfoList)
            {
                for (int i = 0; i < affixInfo.AffixAttributes.Count; i++)
                {
                    var affixAttribute = affixInfo.AffixAttributes[i];

                    if (i > 0)
                    {
                        // Only need to process the first one for combined localisations.
                        if (affixAttribute.LocalisationId.Equals(affixInfo.AffixAttributes[i - 1].LocalisationId)) break;
                    }

                    if (affixAttribute.LocalisationId.Equals("AoE_Size_Bonus_Per_Power") || affixAttribute.LocalisationId.StartsWith("AoE_Size_Bonus_Per_Power#") ||
                        affixAttribute.LocalisationId.Equals("Attack_Speed_Percent_Bonus_For_Power") ||
                        affixAttribute.LocalisationId.Equals("Blood_Orb_Bonus_Chance_Per_Power") ||
                        affixAttribute.LocalisationId.Equals("Bonus_Count_Per_Power") || affixAttribute.LocalisationId.StartsWith("Bonus_Count_Per_Power#") ||
                        affixAttribute.LocalisationId.Equals("Bonus_Max_Skill_Charges_For_Power") ||
                        affixAttribute.LocalisationId.Equals("Bonus_Percent_Per_Power") || affixAttribute.LocalisationId.StartsWith("Bonus_Percent_Per_Power#") ||
                        affixAttribute.LocalisationId.Equals("Bonus_Percent_Per_Power_2") || affixAttribute.LocalisationId.StartsWith("Bonus_Percent_Per_Power_2#") ||
                        affixAttribute.LocalisationId.Equals("Bonus_Percent_Per_Power_3") || affixAttribute.LocalisationId.StartsWith("Bonus_Percent_Per_Power_3#") ||
                        affixAttribute.LocalisationId.Equals("CC_Duration_Bonus_Percent_Per_Power") ||
                        affixAttribute.LocalisationId.Equals("Chance_For_Double_Damage_Per_Power") ||
                        affixAttribute.LocalisationId.Equals("Chance_To_Consume_No_Charges_Per_Power") ||
                        affixAttribute.LocalisationId.Equals("Chance_To_Hit_Twice_Per_Power") || affixAttribute.LocalisationId.StartsWith("Chance_To_Hit_Twice_Per_Power#") ||
                        affixAttribute.LocalisationId.Equals("Cleave_Damage_Bonus_Percent_Per_Power") || affixAttribute.LocalisationId.StartsWith("Cleave_Damage_Bonus_Percent_Per_Power#") ||
                        affixAttribute.LocalisationId.Equals("Combat_Effect_Chance_Bonus_Per_Skill") ||
                        affixAttribute.LocalisationId.Equals("Damage_Percent_Bonus_While_Affected_By_Power") || affixAttribute.LocalisationId.StartsWith("Damage_Percent_Bonus_While_Affected_By_Power#") ||
                        affixAttribute.LocalisationId.Equals("Movement_Speed_Bonus_Percent_Per_Power") || affixAttribute.LocalisationId.StartsWith("Movement_Speed_Bonus_Percent_Per_Power#") ||
                        affixAttribute.LocalisationId.Equals("Paladin_Aura_Potency_Per_Skill") ||
                        affixAttribute.LocalisationId.Equals("Percent_Bonus_Projectiles_Per_Power") || affixAttribute.LocalisationId.StartsWith("Percent_Bonus_Projectiles_Per_Power#") ||
                        affixAttribute.LocalisationId.Equals("Power Bonus Attack Radius Percent") ||
                        affixAttribute.LocalisationId.Equals("Power_Cooldown_Reduction_Percent") || affixAttribute.LocalisationId.StartsWith("Power_Cooldown_Reduction_Percent#") ||
                        affixAttribute.LocalisationId.Equals("Power_Crit_Percent_Bonus") ||
                        affixAttribute.LocalisationId.Equals("Power_Damage_Percent_Bonus") ||
                        affixAttribute.LocalisationId.Equals("Power_Duration_Bonus_Pct") ||
                        affixAttribute.LocalisationId.Equals("Power_Resource_Cost_Reduction_Percent") ||
                        affixAttribute.LocalisationId.Equals("Resource_Gain_Bonus_Percent_Per_Power") ||
                        affixAttribute.LocalisationId.Equals("Skill_Rank_Bonus") ||
                        affixAttribute.LocalisationId.Equals("Sorc_Conjurations_BonusSummons_Chance") ||
                        affixAttribute.LocalisationId.Equals("Talent_Rank_Bonus"))
                    {
                        ReplaceSkillPlaceholders(affixInfo, i);
                        SetClassRestriction(affixInfo);
                    }
                    else if (affixAttribute.LocalisationId.Equals("AoE_Size_Bonus_Per_Skill_Tag") ||
                        affixAttribute.LocalisationId.Equals("Attack_Speed_Percent_Bonus_Per_Skill_Tag") ||
                        affixAttribute.LocalisationId.Equals("Crit_Damage_Percent_Per_Skill_Tag") ||
                        affixAttribute.LocalisationId.Equals("Crit_Percent_Bonus_Per_Skill_Tag") ||
                        affixAttribute.LocalisationId.Equals("Damage_Percent_Bonus_Per_Skill_Tag") || affixAttribute.LocalisationId.StartsWith("Damage_Percent_Bonus_Per_Skill_Tag#") ||
                        affixAttribute.LocalisationId.Equals("Damage_Percent_Bonus_To_Targets_Affected_By_Skill_Tag") || affixAttribute.LocalisationId.StartsWith("Damage_Percent_Bonus_To_Targets_Affected_By_Skill_Tag#") ||
                        affixAttribute.LocalisationId.Equals("Damage_Percent_Reduction_From_Targets_With_Skill_Tag") ||
                        affixAttribute.LocalisationId.Equals("Generic_Chance_For_Double_Damage_Per_SkillTag") ||
                        affixAttribute.LocalisationId.Equals("Generic_Chance_For_Hit_Twice_Per_SkillTag") ||
                        affixAttribute.LocalisationId.Equals("Hit_Effect_Chance_Bonus_Per_Skill_Tag") ||
                        affixAttribute.LocalisationId.Equals("Overpower_Damage_Percent_Bonus_Per_Skill_Tag") ||
                        affixAttribute.LocalisationId.Equals("Per_Skill_Tag_Buff_Duration_Bonus_Percent") ||
                        affixAttribute.LocalisationId.Equals("Percent_Bonus_Projectiles_Per_Skill_Tag") ||
                        affixAttribute.LocalisationId.Equals("Primary_Resource_On_Cast_Per_Skill_Tag") ||
                        affixAttribute.LocalisationId.Equals("Skill_Rank_Skill_Tag_Bonus") ||
                        affixAttribute.LocalisationId.Equals("Skill_Tag_Cooldown_Reduction_Percent") ||
                        affixAttribute.LocalisationId.Equals("Resource_Gain_Bonus_Percent_Per_Skill_Tag"))
                    {
                        ReplaceSkillTagPlaceholders(affixInfo, i);
                    }
                    else if (affixAttribute.LocalisationId.Equals("Resource_Cost_Reduction_Percent") ||
                        affixAttribute.LocalisationId.Equals("Resource_Max_Bonus") ||
                        affixAttribute.LocalisationId.Equals("Resource_On_Hit") ||
                        affixAttribute.LocalisationId.Equals("Resource_On_Kill") ||
                        affixAttribute.LocalisationId.Equals("Resource_Regen_Per_Second"))
                    {
                        ReplaceResourcePlaceholders(affixInfo, i);
                    }
                    else if (affixAttribute.LocalisationId.Equals("Combat_Effect_Chance_Bonus_Per_Damage_Type") ||
                        affixAttribute.LocalisationId.Equals("Damage_Type_Crit_Damage_Percent_Bonus") ||
                        affixAttribute.LocalisationId.Equals("Damage_Type_Crit_Percent_Bonus_Vs_Elites") ||
                        affixAttribute.LocalisationId.Equals("Damage_Type_Percent_Bonus") ||
                        affixAttribute.LocalisationId.Equals("DOT_DPS_Bonus_Percent_Per_Damage_Type") ||
                        affixAttribute.LocalisationId.Equals("Proc_Flat_Element_Damage_On_Hit") ||
                        affixAttribute.LocalisationId.Equals("Resistance") ||
                        affixAttribute.LocalisationId.Equals("Resistance_Max_Bonus") ||
                        affixAttribute.LocalisationId.Equals("Multiplicative_Damage_Type_Percent_Bonus"))
                    {
                        ReplaceDamageTypePlaceholders(affixInfo, i);
                    }
                    else if (affixAttribute.LocalisationId.Equals("Crit_Percent_Bonus_Vs_CC_Target") ||
                        affixAttribute.LocalisationId.Equals("Damage_Percent_Bonus_Vs_CC_Target"))
                    {
                        ReplaceCrowdControlledTypePlaceholders(affixInfo, i);
                    }
                    else if (affixAttribute.LocalisationId.Equals("On_Crit_CC_Proc_Chance") ||
                        affixAttribute.LocalisationId.Equals("On_Hit_CC_Proc_Chance") ||
                        affixAttribute.LocalisationId.Equals("CC_Duration_Reduction_Per_Type") ||
                        affixAttribute.LocalisationId.Equals("CC_Duration_Bonus_Percent_Per_Type"))
                    {
                        ReplaceCrowdControlTypePlaceholders(affixInfo, i);
                    }
                    else if (affixAttribute.LocalisationId.Equals("Damage_Percent_Bonus_Per_Weapon_Requirement") ||
                        affixAttribute.LocalisationId.Equals("Overpower_Damage_Percent_Bonus_Per_Weapon_Requirement") ||
                        affixAttribute.LocalisationId.Equals("Primary_Resource_Gain_Bonus_Percent_Per_Weapon_Requirement") || affixAttribute.LocalisationId.StartsWith("Primary_Resource_Gain_Bonus_Percent_Per_Weapon_Requirement#"))
                    {
                        ReplaceWeaponTypePlaceholders(affixInfo, i);
                    }
                    else if (affixAttribute.LocalisationId.Equals("Damage_Percent_Reduction_From_Dotted_Enemy") ||
                        affixAttribute.LocalisationId.Equals("Damage_Percent_Bonus_Against_Dot_Type"))
                    {
                        ReplaceDotTypePlaceholders(affixInfo, i);
                    }
                    else if (affixAttribute.LocalisationId.Equals("NecroArmy_Pet_Type_Damage_Bonus_Pct") ||
                        affixAttribute.LocalisationId.Equals("NecroArmy_Pet_Type_Inherit_Thorns_Bonus_Pct"))
                    {
                        ReplaceNecroPetTypePlaceholders(affixInfo, i);
                    }
                    else if (affixAttribute.LocalisationId.Equals("Damage_Percent_Bonus_Per_Shapeshift_Form"))
                    {
                        ReplaceShapeshiftFormPlaceholders(affixInfo, i);
                    }
                    else if (affixAttribute.LocalisationId.Equals("Weapon_On_Hit_Percent_Bleed_Proc_Chance_Combined"))
                    {
                        ReplaceAttributeFormulaValue(affixInfo);
                    }
                }
            }
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (Fill in value placeholders): {watch.ElapsedMilliseconds - elapsedMs}");
            elapsedMs = watch.ElapsedMilliseconds;

            // Replace numeric value placeholders - Post process
            ReplaceNumericValuePlaceholdersPost();

            // Set cleaned up description for fuzzy searches.
            foreach (var affixInfo in affixInfoList)
            {
                SetCleanDescription(affixInfo);
            }

            // Sort
            affixInfoList.Sort((x, y) =>
            {
                //return string.Compare(x.Description, y.Description, StringComparison.Ordinal);
                return string.Compare(x.IdName, y.IdName, StringComparison.Ordinal);
            });

            // Combine similar affixes - Create lists
            foreach (var affixInfo in affixInfoList)
            {
                var affixInfoDuplicates = affixInfoList.FindAll(a => a.DescriptionClean.Equals(affixInfo.DescriptionClean));
                foreach (var affixInfoDuplicate in affixInfoDuplicates)
                {
                    affixInfo.IdSnoList.Add(affixInfoDuplicate.IdSno);
                    affixInfo.IdNameList.Add(affixInfoDuplicate.IdName);
                }
            }

            // Combine similar affixes - Sort. To keep output files consistent across versions.
            foreach (var affixInfo in affixInfoList)
            {
                affixInfo.IdSnoList.Sort((x, y) =>
                {
                    return string.Compare(x, y, StringComparison.Ordinal);
                });
                affixInfo.IdNameList.Sort((x, y) =>
                {
                    return string.Compare(x, y, StringComparison.Ordinal);
                });
            }

            // Combine similar affixes - Update AllowedForPlayerClass
            foreach (var affixInfo in affixInfoList)
            {
                // Replaces data old affixes with the newer S04 variant.
                string? idName = affixInfo.IdNameList.FirstOrDefault(a => !affixInfo.IdName.Contains("S04_") && a.Contains("S04_", StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(idName))
                {
                    var affix = affixInfoList.FirstOrDefault(a => a.IdName.Equals(idName));
                    if (affix != null)
                    {
                        affixInfo.AllowedForPlayerClass.Clear();
                        affixInfo.AllowedForPlayerClass.AddRange(affix.AllowedForPlayerClass);
                    }
                }

                // Replaces data old affixes with the newer X1 variant.
                idName = affixInfo.IdNameList.FirstOrDefault(a => !affixInfo.IdName.Contains("X1_") && a.Contains("X1_", StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(idName))
                {
                    var affix = affixInfoList.FirstOrDefault(a => a.IdName.Equals(idName));
                    if (affix != null)
                    {
                        affixInfo.AllowedForPlayerClass.Clear();
                        affixInfo.AllowedForPlayerClass.AddRange(affix.AllowedForPlayerClass);
                    }
                }
            }

            // Combine similar affixes - Update sno/name, tempered
            foreach (var affixInfo in affixInfoList)
            {
                affixInfo.IdSno = string.Join(";", affixInfo.IdSnoList);
                affixInfo.IdName = string.Join(";", affixInfo.IdNameList);

                affixInfo.IsTemperingAvailable = affixInfo.IdName.Contains("tempered", StringComparison.OrdinalIgnoreCase);
            }

            SaveAffixes(language);
            ValidateAffixes(language);

            watch.Stop();
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (Total): {watch.ElapsedMilliseconds}");
        }

        private string GetDamagePercentBonusAgainstDotType(uint sno)
        {
            //Damage_Percent_Bonus_Against_Dot_Type
            //Damage_Percent_Bonus_Against_Dot_Type#Shadow

            string type = string.Empty;
            switch (sno)
            {
                // 0, 1, 4 not defined.
                case 5:
                    type = "Shadow";
                    break;
            }
            return type;
        }

        private string GetDamagePercentReductionFromDottedEnemy(uint sno)
        {
            //Damage_Percent_Reduction_From_Dotted_Enemy
            //Damage_Percent_Reduction_From_Dotted_Enemy#Shadow

            string type = string.Empty;
            switch (sno)
            {
                // 0, 1, 4 not defined.
                case 5:
                    type = "Shadow";
                    break;
            }
            return type;
        }

        private string GetDOTDPSBonusPercentPerDamageType(uint sno)
        {
            //DOT_DPS_Bonus_Percent_Per_Damage_Type
            //DOT_DPS_Bonus_Percent_Per_Damage_Type#Fire
            //DOT_DPS_Bonus_Percent_Per_Damage_Type#Physical
            //DOT_DPS_Bonus_Percent_Per_Damage_Type#Poison
            //DOT_DPS_Bonus_Percent_Per_Damage_Type#Shadow

            string type = string.Empty;
            switch (sno)
            {
                case 0:
                    type = "Physical";
                    break;
                case 1:
                    type = "Fire";
                    break;
                case 4:
                    type = "Poison";
                    break;
                case 5:
                    type = "Shadow";
                    break;
            }
            return type;
        }

        private string GetPowerId(uint sno)
        {
            var powerMeta = _powerMetaJsonList.FirstOrDefault(p => p.__snoID__ == sno);
            if (powerMeta == null)
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: power id {sno} not found.");
                return string.Empty;
            }
            else
            {
                string fileName = powerMeta.__fileName__;
                return Path.GetFileNameWithoutExtension(fileName);
            }
        }

        private string GetSkillTagId(uint sno)
        {
            return _skillTagDictionary[sno][0];
        }

        private string GetWeaponId(uint sno)
        {
            return _weaponTypeDictionary[sno];
        }

        private void SetClassRestriction(AffixInfo affix)
        {
            int classCount = affix.AllowedForPlayerClass.Count(c => c == 1);
            uint classIndex = (uint)affix.AllowedForPlayerClass.IndexOf(1);

            var classLoc = _attributeDescriptions.arStrings.FirstOrDefault(a => a.szLabel.Equals(LocalisationMappings.ClassRestrictions[classIndex], StringComparison.OrdinalIgnoreCase));
            if (classLoc == null || classCount != 1) return;

            affix.ClassRestriction = classLoc.szText;

            if (classIndex == 5)
            {
                // Spiritborn workaround
                var classParamLoc = _frontEnd.arStrings.FirstOrDefault(a => a.szLabel.Equals("SpiritbornTitle", StringComparison.OrdinalIgnoreCase));
                if (classParamLoc == null) return;

                affix.ClassRestriction = affix.ClassRestriction.Replace("{s1}", classParamLoc.szText);
            }
            else if (classIndex == 6)
            {
                // Paladin workaround
                var classParamLoc = _frontEnd.arStrings.FirstOrDefault(a => a.szLabel.Equals("PaladinTitle", StringComparison.OrdinalIgnoreCase));
                if (classParamLoc == null) return;

                affix.ClassRestriction = affix.ClassRestriction.Replace("{s1}", classParamLoc.szText);
            }
        }

        private void SetCleanDescription(AffixInfo affix)
        {
            affix.DescriptionClean = affix.Description.Replace("+", string.Empty);
            affix.DescriptionClean = affix.DescriptionClean.Replace("#", string.Empty);
            affix.DescriptionClean = affix.DescriptionClean.Replace("%", string.Empty);
            affix.DescriptionClean = affix.DescriptionClean.Replace("  ", " ");
            affix.DescriptionClean = affix.DescriptionClean.Replace(" .", ".");
            affix.DescriptionClean = affix.DescriptionClean.Trim();

            // Note: Class restriction text removed from affixes in season 7
            // Append class restriction
            //if (!string.IsNullOrEmpty(affix.ClassRestriction))
            //{
            //    affix.DescriptionClean += $" {affix.ClassRestriction}";
            //}
        }

        private void ReplaceNumericValuePlaceholders()
        {
            var affixInfoList = _affixInfoDictionary[_language];

            foreach (var affix in affixInfoList)
            {
                // Note: https://regex101.com/

                //string pattern = @"\[(.*?%+?.*?)\]";
                //affix.Description = Regex.Replace(affix.Description, pattern, "#%");

                //pattern = @"\[(.*?)\]";
                //affix.Description = Regex.Replace(affix.Description, pattern, "#");

                string pattern = @"\[([^%]+?)\]";
                affix.Description = Regex.Replace(affix.Description, pattern, "#");

                pattern = @"\[(.+?)\]";
                affix.Description = Regex.Replace(affix.Description, pattern, "#%");

                /*affix.Description = affix.Description.Replace("[{VALUE}]", "#");
                affix.Description = affix.Description.Replace("[{VALUE}|1|]", "#");
                affix.Description = affix.Description.Replace("[{VALUE}|~|]", "#");
                affix.Description = affix.Description.Replace("[{VALUE2}]", "#");
                affix.Description = affix.Description.Replace("[{VALUE2}|~|]", "#");
                affix.Description = affix.Description.Replace("[{vALUE2}|%|]", "#%");
                affix.Description = affix.Description.Replace("[{VALUE}*100|%|]", "#%");
                affix.Description = affix.Description.Replace("[{VALUE}*100|1%|]", "#%");
                affix.Description = affix.Description.Replace("[{VALUE}*100}|1%|]", "#%");
                affix.Description = affix.Description.Replace("[{vALUE}*100|%|]", "#%");
                affix.Description = affix.Description.Replace("[{VALUE} * 100|1%|]", "#%");
                affix.Description = affix.Description.Replace("[{vALUE} * 100|%|]", "#%");
                affix.Description = affix.Description.Replace("[{VALUE1}*100|1%|]", "#%");
                affix.Description = affix.Description.Replace("[{VALUE2} * 100|1%|]", "#%");
                affix.Description = affix.Description.Replace("[{VALUE2}*100|1%|]", "#%");
                affix.Description = affix.Description.Replace("[{vALUE2}*100|1%|]", "#%");
                affix.Description = affix.Description.Replace("[{vALUE2}*100|%|]", "#%");*/

                // Missed by regex
                affix.Description = affix.Description.Replace("+{VALUE1}", "+#");
                affix.Description = affix.Description.Replace("{VALUE2}", "#");
                affix.Description = affix.Description.Replace("+{VALUE2}", "+#");
                affix.Description = affix.Description.Replace("+{vALUE2}", "+#");
                affix.Description = affix.Description.Replace("{s1}", "#");
                affix.Description = affix.Description.Replace("{s2}", "#");

                affix.Description = affix.Description.Replace("{icon:bullet}", string.Empty);
                affix.Description = affix.Description.Replace("{c_important}", string.Empty);
                affix.Description = affix.Description.Replace("{c_label}", string.Empty);
                affix.Description = affix.Description.Replace("{c_legendary}", string.Empty);
                affix.Description = affix.Description.Replace("{c_number}", string.Empty);
                affix.Description = affix.Description.Replace("{/c}", string.Empty);
                affix.Description = affix.Description.Replace("{d}", " ");
                affix.Description = affix.Description.Replace("{u}", string.Empty);
                affix.Description = affix.Description.Replace("{/u}", string.Empty);

                // Prefix found in frFR
                affix.Description = affix.Description.Replace("|2", string.Empty);

                foreach (var placeholder in StringPlaceholderMappings.StringPlaceholder)
                {
                    affix.Description = affix.Description.Replace(placeholder.Key, placeholder.Value);
                }
            }
        }

        private void ReplaceNumericValuePlaceholdersPost()
        {
            var affixInfoList = _affixInfoDictionary[_language];

            foreach (var affix in affixInfoList)
            {
                foreach (var placeholder in StringPlaceholderMappings.StringPlaceholder)
                {
                    affix.Description = affix.Description.Replace(placeholder.Key, placeholder.Value);
                }
            }
        }

        private void ReplaceAttributeFormulaValue(AffixInfo affix)
        {
            for (int i = 0; i < affix.AffixAttributes.Count; i++)
            {
                affix.Description = affix.Description.Replace($"{{VALUE{i+1}}}", affix.AffixAttributes[i].LocalisationAttributeFormulaValue);
            }
        }

        private void ReplaceCrowdControlTypePlaceholders(AffixInfo affix, int index)
        {
            var affixAttribute = affix.AffixAttributes[index];
            string localisationParameterAsString = LocalisationMappings.CrowdControlTypes[affixAttribute.LocalisationParameter];
            var crowdControlTypeInfo = _uiToolTips.arStrings.FirstOrDefault(l => l.szLabel.Equals(localisationParameterAsString));
            if (crowdControlTypeInfo != null)
            {
                string crowdControl = crowdControlTypeInfo.szText;
                affix.Description = affix.Description.Replace("{VALUE1}", crowdControl);
            }
            else
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {localisationParameterAsString} not found.");
            }
        }

        private void ReplaceCrowdControlledTypePlaceholders(AffixInfo affix, int index)
        {
            var affixAttribute = affix.AffixAttributes[index];
            string localisationParameterAsString = LocalisationMappings.CrowdControlledTypes[affixAttribute.LocalisationParameter];
            var crowdControlTypeInfo = _uiToolTips.arStrings.FirstOrDefault(l => l.szLabel.Equals(localisationParameterAsString));
            if (crowdControlTypeInfo != null)
            {
                string crowdControl = crowdControlTypeInfo.szText;
                affix.Description = affix.Description.Replace("{VALUE1}", crowdControl);
            }
            else
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {localisationParameterAsString} not found.");
            }
        }

        private void ReplaceDamageTypePlaceholders(AffixInfo affix, int index)
        {
            var affixAttribute = affix.AffixAttributes[index];
            string localisationParameterAsString = LocalisationMappings.DamageTypes[affixAttribute.LocalisationParameter];
            var damageTypeInfo = _skillTagNames.arStrings.FirstOrDefault(l => l.szLabel.Equals(localisationParameterAsString));
            if (damageTypeInfo != null)
            {
                string resource = damageTypeInfo.szText;
                affix.Description = affix.Description.Replace("{VALUE1}", resource);
            }
            else
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {localisationParameterAsString} not found.");
            }
        }

        private void ReplaceDotTypePlaceholders(AffixInfo affix, int index)
        {
            var affixAttribute = affix.AffixAttributes[index];
            string localisationParameterAsString = LocalisationMappings.DotTypes[affixAttribute.LocalisationParameter];
            var dotTypeInfo = _uiToolTips.arStrings.FirstOrDefault(l => l.szLabel.Equals(localisationParameterAsString));
            if (dotTypeInfo != null)
            {
                string dotType = dotTypeInfo.szText;
                affix.Description = affix.Description.Replace("{VALUE1}", dotType);
            }
            else
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {localisationParameterAsString} not found.");
            }
        }

        private void ReplaceNecroPetTypePlaceholders(AffixInfo affix, int index)
        {
            var affixAttribute = affix.AffixAttributes[index];
            string localisationParameterAsString = LocalisationMappings.NecroPetNames[affixAttribute.LocalisationParameter];
            var petTypeInfo = _necromancerArmy.arStrings.FirstOrDefault(l => l.szLabel.Equals(localisationParameterAsString));
            if (petTypeInfo != null)
            {
                string petType = petTypeInfo.szText;
                affix.Description = affix.Description.Replace("{VALUE1}", petType);
            }
            else
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {localisationParameterAsString} not found.");
            }
        }

        private void ReplaceResourcePlaceholders(AffixInfo affix, int index)
        {
            var affixAttribute = affix.AffixAttributes[index];
            string localisationParameterAsString = LocalisationMappings.Resources[affixAttribute.LocalisationParameter];
            var resourceInfo = _skillTags.arStrings.FirstOrDefault(l => l.szLabel.Equals(localisationParameterAsString));
            if (resourceInfo != null)
            {
                string resource = resourceInfo.szText;
                affix.Description = affix.Description.Replace("{VALUE1}", resource);
            }
            else
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {localisationParameterAsString} not found.");
            }
        }

        private void ReplaceShapeshiftFormPlaceholders(AffixInfo affix, int index)
        {
            var affixAttribute = affix.AffixAttributes[index];
            string localisationParameterAsString = LocalisationMappings.ShapeshiftForms[affixAttribute.LocalisationParameter];
            var shapeshiftFormInfo = _uiToolTips.arStrings.FirstOrDefault(l => l.szLabel.Equals(localisationParameterAsString));
            if (shapeshiftFormInfo != null)
            {
                string shapeshiftForm = shapeshiftFormInfo.szText;
                affix.Description = affix.Description.Replace("{VALUE1}", shapeshiftForm);
            }
            else
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {localisationParameterAsString} not found.");
            }
        }

        private void ReplaceSkillPlaceholders(AffixInfo affix, int index)
        {
            if (!affix.Description.Contains("{") && !affix.Description.Contains("}")) return;

            var affixAttribute = affix.AffixAttributes[index];
            var powerMeta = _powerMetaJsonList.FirstOrDefault(p => p.__snoID__ == affixAttribute.LocalisationParameter);
            if (powerMeta == null)
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: power id {affixAttribute.LocalisationParameter} not found.");
            }
            else
            {
                // Read power localisation
                string directory = $"{_d4dataPath}json\\{_language}_Text\\meta\\StringList\\";
                string fileName = powerMeta.__fileName__;
                string fileNameLocalisation = $"{directory}Power_{Path.GetFileNameWithoutExtension(fileName)}.stl.json";
                if (File.Exists(fileNameLocalisation))
                {
                    var jsonAsText = File.ReadAllText(fileNameLocalisation);
                    var localisation = JsonSerializer.Deserialize<Localisation>(jsonAsText);
                    if (localisation != null)
                    {
                        var skillInfo = localisation.arStrings.FirstOrDefault(l => l.szLabel.Equals("name"));
                        if (skillInfo != null)
                        {
                            string skill = skillInfo.szText;
                            affix.Description = affix.Description.Replace("{VALUE1}", skill).Replace("{vALUE1}", skill);
                        }
                    }
                }
                else
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Power_{Path.GetFileNameWithoutExtension(fileName)}.stl.json not found.");
                }
            }
        }

        private void ReplaceSkillTagPlaceholders(AffixInfo affix, int index)
        {
            var affixAttribute = affix.AffixAttributes[index];
            string skillCategory = _skillTagDictionary[affixAttribute.LocalisationParameter][0];
            string localisationParameterAsString = $"{skillCategory}_TagName";
            var skillCategoryInfo = _skillTags.arStrings.FirstOrDefault(l => l.szLabel.Equals(localisationParameterAsString));
            if (skillCategoryInfo != null)
            {
                string skillCategoryLocalisation = skillCategoryInfo.szText;
                affix.Description = affix.Description.Replace("{VALUE1}", skillCategoryLocalisation);
            }
            else
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {localisationParameterAsString} not found.");
            }
        }

        private void ReplaceWeaponTypePlaceholders(AffixInfo affix, int index)
        {
            var affixAttribute = affix.AffixAttributes[index];
            string localisationParameterAsString = _weaponTypeDictionary[affixAttribute.LocalisationParameter];
            var weaponTypeInfo = _itemRequirements.arStrings.FirstOrDefault(l => l.szLabel.Equals(localisationParameterAsString));
            if (weaponTypeInfo != null)
            {
                string skillCategoryLocalisation = weaponTypeInfo.szText;
                affix.Description = affix.Description.Replace("{VALUE1}", skillCategoryLocalisation);
            }
            else 
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {localisationParameterAsString} not found.");
            }
        }

        private void SaveAffixes(string language)
        {
            // Create export list without any duplicates
            var affixInfoList = _affixInfoDictionary[language];
            var affixInfoListExport = new List<AffixInfo>();
            foreach (var affixInfo in affixInfoList)
            {
                if (affixInfoListExport.Any(a => a.DescriptionClean.Equals(affixInfo.DescriptionClean))) continue;

                affixInfoListExport.Add(affixInfo);
            }

            string fileName = $"Data/Affixes.{language}.json";
            string path = Path.GetDirectoryName(fileName) ?? string.Empty;
            Directory.CreateDirectory(path);

            using FileStream stream = File.Create(fileName);
            var options = new JsonSerializerOptions { WriteIndented = true };
            options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            JsonSerializer.Serialize(stream, affixInfoListExport, options);
        }

        private void ValidateAffixes(string language)
        {
            var affixInfoList = _affixInfoDictionary[language];
            foreach (var affixInfo in affixInfoList)
            {
                if (affixInfo.Description.Contains("{") || affixInfo.Description.Contains("}"))
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {affixInfo.IdNameList[0]}: missing values.");
                }
            }
        }

    }
    #endregion
}
