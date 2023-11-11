using D4DataParser.Entities.D4Data;
using D4DataParser.Entities;
using D4DataParser.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace D4DataParser.Parsers
{
    public class AffixParser
    {
        private string _d4dataPath = string.Empty;
        private string _language = string.Empty;
        private List<string> _languages = new List<string>();

        private List<AffixMeta> _affixMetaJsonList = new List<AffixMeta>();
        Localisation _localisationJson = new Localisation();
        List<PowerMeta> _powerMetaJsonList = new List<PowerMeta>();
        private List<AffixInfo> _affixInfoList = new List<AffixInfo>();

        private Dictionary<uint, string> _mappingResources = new Dictionary<uint, string>();
        private Dictionary<uint, string> _mappingDamageTypes = new Dictionary<uint, string>();
        private Dictionary<uint, string> _mappingCrowdControlledTypes = new Dictionary<uint, string>();
        private Dictionary<uint, string> _mappingCrowdControlTypes = new Dictionary<uint, string>();
        private Dictionary<uint, string> _mappingDotTypes = new Dictionary<uint, string>();
        private Dictionary<uint, string> _mappingNecroPetTypes = new Dictionary<uint, string>();
        private Dictionary<uint, string> _mappingShapeshiftForms = new Dictionary<uint, string>();

        // Start of Constructors region

        #region Constructors

        public AffixParser()
        {
            // Init languages
            InitLocalisations();

            // Init mapping
            InitMappings();
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

        private void InitMappings()
        {
            // .\d4data\json\enUS_Text\meta\StringList\SkillTagNames.stl.json
            _mappingResources.Clear();
            _mappingResources.Add(0, "RESOURCE_MANA");
            _mappingResources.Add(1, "RESOURCE_FURY");
            _mappingResources.Add(3, "RESOURCE_ENERGY");
            _mappingResources.Add(5, "RESOURCE_SPIRIT");
            _mappingResources.Add(6, "RESOURCE_ESSENCE");

            // .\d4data\json\enUS_Text\meta\StringList\SkillTagNames.stl.json
            _mappingDamageTypes.Clear();
            _mappingDamageTypes.Add(0, "DAMAGE_PHYSICAL");
            _mappingDamageTypes.Add(1, "DAMAGE_FIRE");
            _mappingDamageTypes.Add(2, "DAMAGE_LIGHTNING");
            _mappingDamageTypes.Add(3, "DAMAGE_COLD");
            _mappingDamageTypes.Add(4, "DAMAGE_POISON");
            _mappingDamageTypes.Add(5, "DAMAGE_SHADOW");

            // .\d4data\json\enUS_Text\meta\StringList\UIToolTips.stl.json
            _mappingCrowdControlledTypes.Clear();
            _mappingCrowdControlledTypes.Add(0, "Affected_By_CC_Type_Slow");
            _mappingCrowdControlledTypes.Add(1, "Affected_By_CC_Type_Immobilize");
            _mappingCrowdControlledTypes.Add(2, "Affected_By_CC_Type_Stun");
            _mappingCrowdControlledTypes.Add(7, "Affected_By_CC_Type_Disabled"); // Dazed
            _mappingCrowdControlledTypes.Add(9, "Affected_By_CC_Type_Chill");
            _mappingCrowdControlledTypes.Add(10, "Affected_By_CC_Type_Frozen");
            _mappingCrowdControlledTypes.Add(11, "Affected_By_CC_Type_Knockdown");

            // .\d4data\json\enUS_Text\meta\StringList\UIToolTips.stl.json
            _mappingCrowdControlTypes.Clear();
            _mappingCrowdControlTypes.Add(0, "CC_Type_Slow");
            _mappingCrowdControlTypes.Add(1, "CC_Type_Immobilize");
            _mappingCrowdControlTypes.Add(2, "CC_Type_Stun");
            _mappingCrowdControlTypes.Add(7, "CC_Type_Disabled"); // Daze
            _mappingCrowdControlTypes.Add(9, "CC_Type_Chill");
            _mappingCrowdControlTypes.Add(10, "CC_Type_Frozen");
            _mappingCrowdControlTypes.Add(11, "CC_Type_Knockback");
            _mappingCrowdControlTypes.Add(13, "CC_Type_Fear");

            // .\d4data\json\enUS_Text\meta\StringList\UIToolTips.stl.json
            _mappingDotTypes.Clear();
            _mappingDotTypes.Add(0, "DOT_Damage_Physical"); // Bleeding
            _mappingDotTypes.Add(1, "DOT_Damage_Fire");
            //_mappingDotTypes.Add(?, "DOT_Damage_Cold"); // Chilled
            _mappingDotTypes.Add(4, "DOT_Damage_Poison");
            _mappingDotTypes.Add(5, "DOT_Damage_Shadow");

            // .\d4data\json\enUS_Text\meta\StringList\NecromancerArmy.stl.json
            _mappingNecroPetTypes.Clear();
            _mappingNecroPetTypes.Add(0, "UnitType_Warrior");
            _mappingNecroPetTypes.Add(1, "UnitType_Mage");
            _mappingNecroPetTypes.Add(2, "UnitType_Golem");

            _mappingShapeshiftForms.Clear();
            _mappingShapeshiftForms.Add(0, "Shapeshift_Form_Human");
            //_mappingShapeshiftForms.Add(?, "Shapeshift_Form_Bear");
            //_mappingShapeshiftForms.Add(?, "Shapeshift_Form_Wolf");
        }

        public void ParseAffixes()
        {
            foreach (var language in _languages)
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: {language}");

                //if (!language.Equals("enUS")) continue;

                ParseAffixesByLanguage(language);
                UpdateAffixes();

                ValidateAffixes();
            }
        }

        private void ParseAffixesByLanguage(string language)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var elapsedMs = watch.ElapsedMilliseconds;

            _language = language;

            // reset
            _localisationJson = new Localisation();
            _affixMetaJsonList.Clear();
            _powerMetaJsonList.Clear();

            _affixInfoList.Clear();

            int affixIndex = 104;

            // Parse CoreTOC.dat.json
            var jsonAsText = File.ReadAllText(CoreTOCPath);
            var coreTOCDictionary = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, Dictionary<int, string>>>(jsonAsText);
            var affixDictionary = coreTOCDictionary[affixIndex];
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

            // Parse .\d4data\json\enUS_Text\meta\StringList
            directory = $"{Path.GetDirectoryName(CoreTOCPath)}\\..\\{_language}_Text\\meta\\StringList\\";
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

                        _localisationJson = JsonSerializer.Deserialize<Localisation>(stream, options) ?? new Localisation();
                    }
                }
            }
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (Localisation): {watch.ElapsedMilliseconds - elapsedMs}");
            elapsedMs = watch.ElapsedMilliseconds;

            // Parse .\d4data\json\base\meta\Power\
            directory = $"{Path.GetDirectoryName(CoreTOCPath)}\\meta\\Power\\";
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

            // Create affix and aspect class
            foreach (var affix in affixDictionary)
            {
                // Note: Inherent are the garanteed affixes by item type.
                if (affix.Value.StartsWith("INHERENT_"))
                //|| affix.Value.Contains("_Unique_"))
                {
                    continue;
                }

                _affixInfoList.Add(new AffixInfo
                {
                    IdSno = affix.Key,
                    IdName = affix.Value,
                });
            }

            // Remove unwanted affixes.
            RemoveUnwantedAffixes();

            watch.Stop();
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (Total): {watch.ElapsedMilliseconds}");
        }

        private void UpdateAffixes()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var elapsedMs = watch.ElapsedMilliseconds;

            // Update affix class
            // - Allowed classes
            // - Allowed item labels
            // - MagicType (affix, aspect)
            // - Localisation id
            // - Localisation parameter
            foreach (var affix in _affixInfoList)
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Processing ({affix.IdSno}) {affix.IdName}");

                var affixMeta = _affixMetaJsonList.FirstOrDefault(a => a.__snoID__ == affix.IdSno);
                if (affixMeta == null)
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: IdSno not found.");
                    continue;
                }

                // Skip if affix is of type aspect.
                if (GetAffixType(affixMeta).Equals("aspect"))
                {
                    continue;
                }

                int IdSno = affixMeta.__snoID__;
                List<int> allowedForPlayerClass = affixMeta.fAllowedForPlayerClass ?? new List<int>();
                List<int> allowedItemLabels = affixMeta.arAllowedItemLabels ?? new List<int>();
                int magicType = affixMeta.eMagicType;

                // LocalisationId
                var itemAffixAttributes = affixMeta.ptItemAffixAttributes ?? new List<PtItemAffixAttribute>();
                if (!itemAffixAttributes.Any())
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: No localisation data available.");
                    continue;
                }

                foreach (var itemAffixAttribute in itemAffixAttributes)
                {
                    // On_Hit_Vulnerable_Proc_Chance --> DISABLED
                    // On_Hit_Vulnerable_Proc_Duration_Seconds --> DISABLED
                    // Movement_Bonus_On_Elite_Kill --> IGNORE
                    // Movement_Bonus_On_Elite_Kill_Duration --> Movement_Speed_Bonus_On_Elite_Kill
                    // Weapon_On_Hit_Percent_Bleed_Proc_Chance
                    // Weapon_On_Hit_Percent_Bleed_Proc_Damage
                    // Weapon_On_Hit_Percent_Bleed_Proc_Duration
                    // Mount_Fear_Reduction_Pct
                    // Item_Find
                    // Evade_Movement_Speed --> IGNORE
                    // Evade_Movement_Speed_Duration --> Evade_Movement_Dodge_Chance
                    // Damage_Bonus_On_Elite_Kill --> IGNORE
                    // Damage_Bonus_On_Elite_Kill_Duration --> Damage_Bonus_On_Elite_Kill_Combined
                    // Damage_Bonus_Percent_On_Dodge --> IGNORE
                    // Damage_Bonus_Percent_On_Dodge_Duration --> Damage_Bonus_Percent_After_Dodge
                    // Attack_Speed_Bonus_On_Dodge --> IGNORE
                    // Attack_Speed_Bonus_On_Dodge_Duration --> Attack_Speed_Bonus_After_Dodge
                    // Blood_Orb_Pickup_Damage_Percent_Bonus --> IGNORE
                    // Blood_Orb_Pickup_Damage_Bonus_Duration --> Blood_Orb_Pickup_Damage_Combined
                    // Barrier_When_Struck_Percent_Chance
                    // Fortified_When_Struck_Percent_Chance
                    // Fortified_When_Struck_Amount

                    // Replace some localisationIds with an id that is available in AttributeDescriptions.stl.json
                    string localisationId = itemAffixAttribute.tAttribute.__eAttribute_name__;
                    if (localisationId.Equals("Movement_Bonus_On_Elite_Kill_Duration")) localisationId = "Movement_Speed_Bonus_On_Elite_Kill";
                    if (localisationId.Equals("Damage_Bonus_On_Elite_Kill_Duration")) localisationId = "Damage_Bonus_On_Elite_Kill_Combined";
                    if (localisationId.Equals("Evade_Movement_Speed_Duration")) localisationId = "Evade_Movement_Dodge_Chance";
                    if (localisationId.Equals("Attack_Speed_Bonus_On_Dodge_Duration")) localisationId = "Attack_Speed_Bonus_After_Dodge";
                    if (localisationId.Equals("Damage_Bonus_Percent_On_Dodge_Duration")) localisationId = "Damage_Bonus_Percent_After_Dodge";
                    if (localisationId.Equals("Blood_Orb_Pickup_Damage_Bonus_Duration")) localisationId = "Blood_Orb_Pickup_Damage_Combined";

                    affix.AffixAttributes.Add(new AffixAttribute
                    {
                        LocalisationId = localisationId,
                        LocalisationParameter = itemAffixAttribute.tAttribute.nParam,
                        LocalisationAttributeFormulaValue = itemAffixAttribute?.tAttribute?.szAttributeFormula?.value ?? string.Empty
                    });
                }

                affix.AllowedForPlayerClass = allowedForPlayerClass;
                affix.AllowedItemLabels = allowedItemLabels;
                affix.MagicType = magicType;
            }

            // Update affix class
            // - Localisation data
            foreach (var affix in _affixInfoList)
            {
                foreach (var affixAttribute in affix.AffixAttributes)
                {
                    var affixLoc = _localisationJson.arStrings.FirstOrDefault(a => a.szLabel.Equals(affixAttribute.LocalisationId, StringComparison.OrdinalIgnoreCase));
                    if (affixLoc == null)
                    {
                        Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Localisation id {affixAttribute.LocalisationId} not found.");
                        continue;
                    }

                    string localisationId = affixLoc.szLabel;
                    string localisation = affixLoc.szText;

                    affixAttribute.Localisation = localisation;
                    affix.Description += localisation;
                }
            }

            // Replace numeric value placeholders
            ReplaceNumericValuePlaceholders();

            // Update affix class
            // - Skill rank related affixes
            // - Resources
            // - Damage types
            // - Resistances
            // - Crowd control types
            // - Skill categories
            foreach (var affix in _affixInfoList)
            {
                foreach (var affixAttribute in affix.AffixAttributes)
                {
                    if (affixAttribute.LocalisationId.Equals("Skill_Rank_Bonus") ||
                        affixAttribute.LocalisationId.Equals("Talent_Rank_Bonus") ||
                        affixAttribute.LocalisationId.Equals("Power_Damage_Percent_Bonus") ||
                        affixAttribute.LocalisationId.Equals("Power_Duration_Bonus_Pct") ||
                        affixAttribute.LocalisationId.Equals("Damage_Percent_Bonus_While_Affected_By_Power") ||
                        affixAttribute.LocalisationId.Equals("Attack_Speed_Percent_Bonus_For_Power") ||
                        affixAttribute.LocalisationId.Equals("Power_Cooldown_Reduction_Percent"))
                    {
                        ReplaceSkillPlaceholders(affix, _powerMetaJsonList);
                    }
                    else if (affixAttribute.LocalisationId.Equals("Resource_Max_Bonus") ||
                        affixAttribute.LocalisationId.Equals("Resource_Cost_Reduction_Percent") ||
                        affixAttribute.LocalisationId.Equals("Resource_On_Kill"))
                    {
                        ReplaceResourcePlaceholders(affix);
                    }
                    else if (affixAttribute.LocalisationId.Equals("Damage_Type_Percent_Bonus") ||
                        affixAttribute.LocalisationId.Equals("Damage_Type_Crit_Damage_Percent_Bonus") ||
                        affixAttribute.LocalisationId.Equals("Resistance") ||
                        affixAttribute.LocalisationId.Equals("Combat_Effect_Chance_Bonus_Per_Damage_Type") ||
                        affixAttribute.LocalisationId.Equals("Damage_Type_Crit_Percent_Bonus_Vs_Elites") ||
                        affixAttribute.LocalisationId.Equals("DOT_DPS_Bonus_Percent_Per_Damage_Type"))
                    {
                        ReplaceDamageTypePlaceholders(affix);
                    }
                    else if (affixAttribute.LocalisationId.Equals("Damage_Percent_Bonus_Vs_CC_Target"))
                    {
                        ReplaceCrowdControlledTypePlaceholders(affix);
                    }
                    else if (affixAttribute.LocalisationId.Equals("On_Crit_CC_Proc_Chance") ||
                        affixAttribute.LocalisationId.Equals("On_Hit_CC_Proc_Chance") ||
                        affixAttribute.LocalisationId.Equals("CC_Duration_Reduction_Per_Type") ||
                        affixAttribute.LocalisationId.Equals("CC_Duration_Bonus_Percent_Per_Type"))
                    {
                        ReplaceCrowdControlTypePlaceholders(affix);
                    }
                    else if (affixAttribute.LocalisationId.Equals("Skill_Rank_Skill_Tag_Bonus") ||
                        affixAttribute.LocalisationId.Equals("Damage_Percent_Bonus_Per_Skill_Tag") ||
                        affixAttribute.LocalisationId.Equals("Skill_Tag_Cooldown_Reduction_Percent") ||
                        affixAttribute.LocalisationId.Equals("Damage_Percent_Bonus_To_Targets_Affected_By_Skill_Tag") ||
                        affixAttribute.LocalisationId.Equals("Crit_Damage_Percent_Per_Skill_Tag") ||
                        affixAttribute.LocalisationId.Equals("Overpower_Damage_Percent_Bonus_Per_Skill_Tag") ||
                        affixAttribute.LocalisationId.Equals("Attack_Speed_Percent_Bonus_Per_Skill_Tag"))
                    {
                        ReplaceSkillTagPlaceholders(affix);
                    }
                    else if (affixAttribute.LocalisationId.Equals("Damage_Percent_Bonus_Per_Weapon_Requirement") ||
                        affixAttribute.LocalisationId.Equals("Overpower_Damage_Percent_Bonus_Per_Weapon_Requirement"))
                    {
                        ReplaceWeaponTypePlaceholders(affix);
                    }
                    else if (affixAttribute.LocalisationId.Equals("Damage_Percent_Reduction_From_Dotted_Enemy") ||
                        affixAttribute.LocalisationId.Equals("Damage_Percent_Bonus_Against_Dot_Type"))
                    {
                        ReplaceDotTypePlaceholders(affix);
                    }
                    else if (affixAttribute.LocalisationId.Equals("NecroArmy_Pet_Type_Inherit_Thorns_Bonus_Pct"))
                    {
                        ReplaceNecroPetTypePlaceholders(affix);
                    }
                    else if (affixAttribute.LocalisationId.Equals("Damage_Percent_Bonus_Per_Shapeshift_Form"))
                    {
                        ReplaceShapeshiftFormPlaceholders(affix);
                    }
                    else if (affixAttribute.LocalisationId.Equals("Movement_Speed_Bonus_On_Elite_Kill") ||
                        affixAttribute.LocalisationId.Equals("Damage_Bonus_On_Elite_Kill_Combined") || 
                        affixAttribute.LocalisationId.Equals("Evade_Movement_Dodge_Chance") ||
                        affixAttribute.LocalisationId.Equals("Attack_Speed_Bonus_After_Dodge") ||
                        affixAttribute.LocalisationId.Equals("Damage_Bonus_Percent_After_Dodge") ||
                        affixAttribute.LocalisationId.Equals("Blood_Orb_Pickup_Damage_Combined"))
                    {
                        ReplaceAttributeFormulaValue(affix, affixAttribute);
                    }
                }
            }

            // Remove all affixes with no description
            _affixInfoList.RemoveAll(a => string.IsNullOrWhiteSpace(a.Description));

            // Sort
            _affixInfoList.Sort((x, y) =>
            {
                return string.Compare(x.Description, y.Description, StringComparison.Ordinal);
            });

            // Save affixes
            SaveAffixes();

            watch.Stop();
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (Total): {watch.ElapsedMilliseconds}");
        }

        private void SaveAffixes()
        {
            string fileName = $"Data/Affixes.{_language}.json";
            string path = Path.GetDirectoryName(fileName) ?? string.Empty;
            Directory.CreateDirectory(path);

            using FileStream stream = File.Create(fileName);
            var options = new JsonSerializerOptions { WriteIndented = true };
            options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            JsonSerializer.Serialize(stream, _affixInfoList, options);
        }

        private void ValidateAffixes()
        {
            var duplicates = _affixInfoList.GroupBy(a => a.Description).Where(a => a.Count() > 1);
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

        private string GetAffixType(AffixMeta affixMeta)
        {
            string affixType = "affix";

            var arAffixSkillTags = affixMeta.arAffixSkillTags;
            bool hasLegendaryFilter = false;
            bool hasLengendaryPower = false;
            foreach (var arAffixSkillTag in arAffixSkillTags)
            {
                hasLegendaryFilter = arAffixSkillTag.name.Contains("FILTER_Legendary_");
                if (hasLegendaryFilter) break;
            }

            if (affixMeta.snoPassivePower != null)
            {
                hasLengendaryPower = affixMeta.snoPassivePower.name?.StartsWith("legendary_", StringComparison.OrdinalIgnoreCase) ?? false;
            }

            if (affixMeta.eMagicType == 1 && (hasLegendaryFilter || hasLengendaryPower))
            {
                affixType = "aspect";
            }

            return affixType;
        }

        private void ReplaceNumericValuePlaceholders()
        {
            // Note: https://regex101.com/

            foreach (var affix in _affixInfoList)
            {
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

                affix.Description = affix.Description.Replace("+{VALUE1}", "+#");
                affix.Description = affix.Description.Replace("+{VALUE2}", "+#");
                affix.Description = affix.Description.Replace("+{vALUE2}", "+#");

                affix.Description = affix.Description.Replace("{c_label}", string.Empty);
                affix.Description = affix.Description.Replace("{/c}", string.Empty);

                // deDE
                affix.Description = affix.Description.Replace("|4Aufladung:Aufladungen;", "Aufladungen");
                affix.Description = affix.Description.Replace("|4Rang:Ränge;", "Ränge");

                // enUS
                affix.Description = affix.Description.Replace("|4Charge:Charges;", "Charges");
                affix.Description = affix.Description.Replace("|4Rank:Ranks;", "Ranks");
                affix.Description = affix.Description.Replace("|4Second:Seconds;", "Seconds");

                // esES
                affix.Description = affix.Description.Replace("|4carga máxima:cargas máximas;", "cargas máximas");
                affix.Description = affix.Description.Replace("|4rango:rangos;", "rangos");

                // esMX
                affix.Description = affix.Description.Replace("|4carga:cargas;", "cargas");
                affix.Description = affix.Description.Replace("|4rango:rangos;", "rangos");
                affix.Description = affix.Description.Replace("|4segundo:segundos;", "segundos");

                // frFR
                affix.Description = affix.Description.Replace("|4charge:charges;", "charges");
                affix.Description = affix.Description.Replace("|4rang:rangs;", "rangs");

                // itIT
                affix.Description = affix.Description.Replace("|4carica massima:cariche massime;", "cariche massime");
                affix.Description = affix.Description.Replace("|4grado:gradi;", "gradi");

                // plPL
                affix.Description = affix.Description.Replace("|4ładunek:ładunki:ładunków;", "ładunków");
                affix.Description = affix.Description.Replace("|4ranga:rangi:rang;", "rang");

                // ptBR
                affix.Description = affix.Description.Replace("|4carga:cargas;", "cargas");
                affix.Description = affix.Description.Replace("|4grau:graus;", "graus");
                affix.Description = affix.Description.Replace("|4segundo:segundos;", "segundos");

                // trTR
                affix.Description = affix.Description.Replace("|4Yükü:Yükü;", "Yükü");
                affix.Description = affix.Description.Replace("|4Kademesi:Kademesi;", "Kademesi");
                affix.Description = affix.Description.Replace("|4Saniye:Saniye;", "Saniye");
            }
        }        

        private void ReplaceSkillPlaceholders(AffixInfo affix, List<PowerMeta> powerMetaJsonList)
        {
            foreach (var affixAttribute in affix.AffixAttributes)
            {
                var powerMeta = powerMetaJsonList.FirstOrDefault(p => p.__snoID__ == affixAttribute.LocalisationParameter);
                if (powerMeta == null)
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: power id {affixAttribute.LocalisationParameter} not found.");
                }
                else
                {
                    // Read power loc
                    string directory = $"{Path.GetDirectoryName(CoreTOCPath)}\\..\\{_language}_Text\\meta\\StringList\\";
                    string fileName = powerMeta.__fileName__;
                    string fileNameLoc = $"{directory}Power_{Path.GetFileNameWithoutExtension(fileName)}.stl.json";
                    var jsonAsText = File.ReadAllText(fileNameLoc);
                    var localisation = JsonSerializer.Deserialize<Localisation>(jsonAsText);
                    if (localisation != null)
                    {
                        var skillInfo = localisation.arStrings.FirstOrDefault(l => l.szLabel.Equals("name"));
                        if (skillInfo != null)
                        {
                            string skill = skillInfo.szText;
                            affix.Description = affix.Description.Replace("{VALUE1}", skill);
                        }
                    }
                }
            }
        }

        private void ReplaceResourcePlaceholders(AffixInfo affix)
        {
            foreach (var affixAttribute in affix.AffixAttributes)
            {
                if (affixAttribute.LocalisationId.Equals("Resource_Max_Bonus") ||
                    affixAttribute.LocalisationId.Equals("Resource_Cost_Reduction_Percent") ||
                    affixAttribute.LocalisationId.Equals("Resource_On_Kill"))
                {
                    string localisationParameterAsString = _mappingResources[affixAttribute.LocalisationParameter];
                    string directory = $"{Path.GetDirectoryName(CoreTOCPath)}\\..\\{_language}_Text\\meta\\StringList\\";
                    string fileNameLoc = $"{directory}SkillTagNames.stl.json";
                    var jsonAsText = File.ReadAllText(fileNameLoc);
                    var localisation = JsonSerializer.Deserialize<Localisation>(jsonAsText);
                    if (localisation != null)
                    {
                        var resourceInfo = localisation.arStrings.FirstOrDefault(l => l.szLabel.Equals(localisationParameterAsString));
                        if (resourceInfo != null)
                        {
                            string resource = resourceInfo.szText;
                            affix.Description = affix.Description.Replace("{VALUE1}", resource);
                        }
                    }
                }
            }
        }

        private void ReplaceDamageTypePlaceholders(AffixInfo affix)
        {
            foreach (var affixAttribute in affix.AffixAttributes)
            {
                if (affixAttribute.LocalisationId.Equals("Damage_Type_Percent_Bonus") ||
                    affixAttribute.LocalisationId.Equals("Damage_Type_Crit_Damage_Percent_Bonus") ||
                    affixAttribute.LocalisationId.Equals("Resistance") ||
                    affixAttribute.LocalisationId.Equals("Combat_Effect_Chance_Bonus_Per_Damage_Type") ||
                    affixAttribute.LocalisationId.Equals("Damage_Type_Crit_Percent_Bonus_Vs_Elites") ||
                    affixAttribute.LocalisationId.Equals("DOT_DPS_Bonus_Percent_Per_Damage_Type"))
                {
                    string localisationParameterAsString = _mappingDamageTypes[affixAttribute.LocalisationParameter];
                    string directory = $"{Path.GetDirectoryName(CoreTOCPath)}\\..\\{_language}_Text\\meta\\StringList\\";
                    string fileNameLoc = $"{directory}SkillTagNames.stl.json";
                    var jsonAsText = File.ReadAllText(fileNameLoc);
                    var localisation = JsonSerializer.Deserialize<Localisation>(jsonAsText);
                    if (localisation != null)
                    {
                        var damageTypeInfo = localisation.arStrings.FirstOrDefault(l => l.szLabel.Equals(localisationParameterAsString));
                        if (damageTypeInfo != null)
                        {
                            string resource = damageTypeInfo.szText;
                            affix.Description = affix.Description.Replace("{VALUE1}", resource);
                        }
                    }
                }
            }
        }

        private void ReplaceCrowdControlledTypePlaceholders(AffixInfo affix)
        {
            foreach (var affixAttribute in affix.AffixAttributes)
            {
                if (affixAttribute.LocalisationId.Equals("Damage_Percent_Bonus_Vs_CC_Target"))
                {
                    string localisationParameterAsString = _mappingCrowdControlledTypes[affixAttribute.LocalisationParameter];
                    string directory = $"{Path.GetDirectoryName(CoreTOCPath)}\\..\\{_language}_Text\\meta\\StringList\\";
                    string fileNameLoc = $"{directory}UIToolTips.stl.json";
                    var jsonAsText = File.ReadAllText(fileNameLoc);
                    var localisation = JsonSerializer.Deserialize<Localisation>(jsonAsText);
                    if (localisation != null)
                    {
                        var crowdControlTypeInfo = localisation.arStrings.FirstOrDefault(l => l.szLabel.Equals(localisationParameterAsString));
                        if (crowdControlTypeInfo != null)
                        {
                            string crowdControl = crowdControlTypeInfo.szText;
                            affix.Description = affix.Description.Replace("{VALUE1}", crowdControl);
                        }
                    }
                }
            }
        }

        private void ReplaceCrowdControlTypePlaceholders(AffixInfo affix)
        {
            foreach (var affixAttribute in affix.AffixAttributes)
            {
                if (affixAttribute.LocalisationId.Equals("On_Crit_CC_Proc_Chance") ||
                    affixAttribute.LocalisationId.Equals("On_Hit_CC_Proc_Chance") ||
                    affixAttribute.LocalisationId.Equals("CC_Duration_Reduction_Per_Type") ||
                    affixAttribute.LocalisationId.Equals("CC_Duration_Bonus_Percent_Per_Type"))
                {
                    string localisationParameterAsString = _mappingCrowdControlTypes[affixAttribute.LocalisationParameter];
                    string directory = $"{Path.GetDirectoryName(CoreTOCPath)}\\..\\{_language}_Text\\meta\\StringList\\";
                    string fileNameLoc = $"{directory}UIToolTips.stl.json";
                    var jsonAsText = File.ReadAllText(fileNameLoc);
                    var localisation = JsonSerializer.Deserialize<Localisation>(jsonAsText);
                    if (localisation != null)
                    {
                        var crowdControlTypeInfo = localisation.arStrings.FirstOrDefault(l => l.szLabel.Equals(localisationParameterAsString));
                        if (crowdControlTypeInfo != null)
                        {
                            string crowdControl = crowdControlTypeInfo.szText;
                            affix.Description = affix.Description.Replace("{VALUE1}", crowdControl);
                        }
                    }
                }
            }
        }

        private void ReplaceSkillTagPlaceholders(AffixInfo affix)
        {
            // Parse GBID.json
            int skillTagIndex = 56;
            var jsonAsText = File.ReadAllText($"{Path.GetDirectoryName(CoreTOCPath)}\\..\\GBID.json");
            var gbidDictionary = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, Dictionary<uint, List<string>>>>(jsonAsText);
            var skillTagDictionary = gbidDictionary[skillTagIndex];

            foreach (var affixAttribute in affix.AffixAttributes)
            {
                if (affixAttribute.LocalisationId.Equals("Skill_Rank_Skill_Tag_Bonus") ||
                    affixAttribute.LocalisationId.Equals("Damage_Percent_Bonus_Per_Skill_Tag") ||
                    affixAttribute.LocalisationId.Equals("Skill_Tag_Cooldown_Reduction_Percent") ||
                    affixAttribute.LocalisationId.Equals("Damage_Percent_Bonus_To_Targets_Affected_By_Skill_Tag") ||
                    affixAttribute.LocalisationId.Equals("Crit_Damage_Percent_Per_Skill_Tag") ||
                    affixAttribute.LocalisationId.Equals("Overpower_Damage_Percent_Bonus_Per_Skill_Tag") ||
                    affixAttribute.LocalisationId.Equals("Attack_Speed_Percent_Bonus_Per_Skill_Tag"))
                {
                    var skillCategory = skillTagDictionary[affixAttribute.LocalisationParameter][0];

                    // Localisation
                    string localisationParameterAsString = $"{skillCategory}_TagName";
                    string directory = $"{Path.GetDirectoryName(CoreTOCPath)}\\..\\{_language}_Text\\meta\\StringList\\";
                    string fileNameLoc = $"{directory}SkillTags.stl.json";
                    jsonAsText = File.ReadAllText(fileNameLoc);
                    var localisation = JsonSerializer.Deserialize<Localisation>(jsonAsText);
                    if (localisation != null)
                    {
                        var skillCategoryInfo = localisation.arStrings.FirstOrDefault(l => l.szLabel.Equals(localisationParameterAsString));
                        if (skillCategoryInfo != null)
                        {
                            string skillCategoryLoc = skillCategoryInfo.szText;
                            affix.Description = affix.Description.Replace("{VALUE1}", skillCategoryLoc);
                        }
                    }
                }
            }
        }

        private void ReplaceWeaponTypePlaceholders(AffixInfo affix)
        {
            // Parse CoreTOC.dat.json
            int weaponTypeIndex = 116;
            var jsonAsText = File.ReadAllText(CoreTOCPath);
            var coreTOCDictionary = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, Dictionary<uint, string>>>(jsonAsText);
            var weaponTypeDictionary = coreTOCDictionary[weaponTypeIndex];

            foreach (var affixAttribute in affix.AffixAttributes)
            {
                if (affixAttribute.LocalisationId.Equals("Damage_Percent_Bonus_Per_Weapon_Requirement") ||
                    affixAttribute.LocalisationId.Equals("Overpower_Damage_Percent_Bonus_Per_Weapon_Requirement"))
                {
                    var weaponTypeParameterAsString = weaponTypeDictionary[affixAttribute.LocalisationParameter];

                    // Localisation
                    string directory = $"{Path.GetDirectoryName(CoreTOCPath)}\\..\\{_language}_Text\\meta\\StringList\\";
                    string fileNameLoc = $"{directory}ItemRequirements.stl.json";
                    jsonAsText = File.ReadAllText(fileNameLoc);
                    var localisation = JsonSerializer.Deserialize<Localisation>(jsonAsText);
                    if (localisation != null)
                    {
                        var weaponTypeInfo = localisation.arStrings.FirstOrDefault(l => l.szLabel.Equals(weaponTypeParameterAsString));
                        if (weaponTypeInfo != null)
                        {
                            string skillCategoryLoc = weaponTypeInfo.szText;
                            affix.Description = affix.Description.Replace("{VALUE1}", skillCategoryLoc);
                        }
                    }
                }
            }
        }

        private void ReplaceDotTypePlaceholders(AffixInfo affix)
        {
            foreach (var affixAttribute in affix.AffixAttributes)
            {
                if (affixAttribute.LocalisationId.Equals("Damage_Percent_Reduction_From_Dotted_Enemy") ||
                    affixAttribute.LocalisationId.Equals("Damage_Percent_Bonus_Against_Dot_Type"))
                {
                    string localisationParameterAsString = _mappingDotTypes[affixAttribute.LocalisationParameter];
                    string directory = $"{Path.GetDirectoryName(CoreTOCPath)}\\..\\{_language}_Text\\meta\\StringList\\";
                    string fileNameLoc = $"{directory}UIToolTips.stl.json";
                    var jsonAsText = File.ReadAllText(fileNameLoc);
                    var localisation = JsonSerializer.Deserialize<Localisation>(jsonAsText);
                    if (localisation != null)
                    {
                        var dotTypeInfo = localisation.arStrings.FirstOrDefault(l => l.szLabel.Equals(localisationParameterAsString));
                        if (dotTypeInfo != null)
                        {
                            string dotType = dotTypeInfo.szText;
                            affix.Description = affix.Description.Replace("{VALUE1}", dotType);
                        }
                    }
                }
            }
        }

        private void ReplaceNecroPetTypePlaceholders(AffixInfo affix)
        {
            foreach (var affixAttribute in affix.AffixAttributes)
            {
                if (affixAttribute.LocalisationId.Equals("NecroArmy_Pet_Type_Inherit_Thorns_Bonus_Pct"))
                {
                    string localisationParameterAsString = _mappingNecroPetTypes[affixAttribute.LocalisationParameter];
                    string directory = $"{Path.GetDirectoryName(CoreTOCPath)}\\..\\{_language}_Text\\meta\\StringList\\";
                    string fileNameLoc = $"{directory}NecromancerArmy.stl.json";
                    var jsonAsText = File.ReadAllText(fileNameLoc);
                    var localisation = JsonSerializer.Deserialize<Localisation>(jsonAsText);
                    if (localisation != null)
                    {
                        var petTypeInfo = localisation.arStrings.FirstOrDefault(l => l.szLabel.Equals(localisationParameterAsString));
                        if (petTypeInfo != null)
                        {
                            string petType = petTypeInfo.szText;
                            affix.Description = affix.Description.Replace("{VALUE1}", petType);
                        }
                    }
                }
            }
        }

        private void ReplaceShapeshiftFormPlaceholders(AffixInfo affix)
        {
            foreach (var affixAttribute in affix.AffixAttributes)
            {
                if (affixAttribute.LocalisationId.Equals("Damage_Percent_Bonus_Per_Shapeshift_Form"))
                {
                    string localisationParameterAsString = _mappingShapeshiftForms[affixAttribute.LocalisationParameter];
                    string directory = $"{Path.GetDirectoryName(CoreTOCPath)}\\..\\{_language}_Text\\meta\\StringList\\";
                    string fileNameLoc = $"{directory}UIToolTips.stl.json";
                    var jsonAsText = File.ReadAllText(fileNameLoc);
                    var localisation = JsonSerializer.Deserialize<Localisation>(jsonAsText);
                    if (localisation != null)
                    {
                        var shapeshiftFormInfo = localisation.arStrings.FirstOrDefault(l => l.szLabel.Equals(localisationParameterAsString));
                        if (shapeshiftFormInfo != null)
                        {
                            string shapeshiftForm = shapeshiftFormInfo.szText;
                            affix.Description = affix.Description.Replace("{VALUE1}", shapeshiftForm);
                        }
                    }
                }
            }
        }

        private void ReplaceAttributeFormulaValue(AffixInfo affix, AffixAttribute affixAttribute)
        {
            affix.Description = affix.Description.Replace("{VALUE2}", affixAttribute.LocalisationAttributeFormulaValue);
        }

        private void RemoveUnwantedAffixes()
        {
            //var jsonAsText = File.ReadAllText("Data/Blacklist.json");
            //var affixBlacklist = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, string>>(jsonAsText);
            //if (affixBlacklist != null ) 
            //{
            //    foreach (var affix in affixBlacklist)
            //    {
            //        _affixInfoList.RemoveAll(a => a.IdSno == affix.Key);
            //    }
            //}

            // Remove all "INHERENT_" affixes
            _affixInfoList.RemoveAll(a => a.IdName.StartsWith("INHERENT_"));

            // Remove all "_WishingWell_" affixes
            _affixInfoList.RemoveAll(a => a.IdName.Contains("_WishingWell_"));

            // Remove all "_Jewelry" affixes
            _affixInfoList.RemoveAll(a => a.IdName.Contains("_Jewelry_"));
            _affixInfoList.RemoveAll(a => a.IdName.EndsWith("Jewelry"));
            //_affixInfoList.RemoveAll(a => a.IdName.Equals("LuckJewelry"));
            //_affixInfoList.RemoveAll(a => a.IdName.Equals("CritChanceJewelry"));

            // Remove all "Resistance_Dual" affixes
            _affixInfoList.RemoveAll(a => a.IdName.StartsWith("Resistance_Dual"));

            // Remove all "Mount_" affixes
            _affixInfoList.RemoveAll(a => a.IdName.StartsWith("Mount_"));

            // Remove all "QA_" affixes
            _affixInfoList.RemoveAll(a => a.IdName.StartsWith("QA_"));

            // Remove disabled gem affixes
            _affixInfoList.RemoveAll(a => a.IdName.Equals("GemPower1Socket"));
            _affixInfoList.RemoveAll(a => a.IdName.Equals("GemPower2Socket"));

            // Remove specific duplicates
            _affixInfoList.RemoveAll(a => a.IdName.Equals("CoreStats_All_Weapon")); // "# All Stats", using "CoreStats_All" instead.
            _affixInfoList.RemoveAll(a => a.IdName.Equals("ElementalDamageReduction")); // "#% Resistance to All Elements", using "Resistance_All" instead.
            _affixInfoList.RemoveAll(a => a.IdName.Equals("Damage_FullScaling")); // "+#% Damage", using "Damage" instead.
            _affixInfoList.RemoveAll(a => a.IdName.Equals("Dodge_Additive")); // "+#% Dodge Chance", using "Dodge" instead.
            _affixInfoList.RemoveAll(a => a.IdName.Equals("CoreStat_Dexterity_Weapon")); // "# Dexterity", using "CoreStat_Dexterity" instead.      
            _affixInfoList.RemoveAll(a => a.IdName.Equals("CoreStat_Intelligence_Weapon")); // "# Intelligence", using "CoreStat_Intelligence" instead.      
            _affixInfoList.RemoveAll(a => a.IdName.Equals("CoreStat_Strength_Weapon")); // "# Strength", using "CoreStat_Strength" instead.      
            _affixInfoList.RemoveAll(a => a.IdName.Equals("CoreStat_Willpower_Weapon")); // "# Willpower", using "CoreStat_Willpower" instead.      
            _affixInfoList.RemoveAll(a => a.IdName.Equals("Thorns_Shields")); // "+# Thorns", using "Thorns" instead.
            _affixInfoList.RemoveAll(a => a.IdName.Equals("Evade_Attack_Reset_Random")); // "Attacks Reduce Evade's Cooldown by # Seconds", using "Evade_Attack_Reset" instead.
            _affixInfoList.RemoveAll(a => a.IdName.Equals("Evade_Charges_Random")); // "+# Max Evade Charges", using "Evade_Max_Charges" instead.

            // Remove duplicates
            _affixInfoList.RemoveAll(a => a.IdName.EndsWith("_UBERUNIQUE"));
            _affixInfoList.RemoveAll(a => a.IdName.EndsWith("_UNIQUE"));
            _affixInfoList.RemoveAll(a => a.IdName.Contains("_UNIQUE_"));
            _affixInfoList.RemoveAll(a => a.IdName.Contains("_Unique_"));
            _affixInfoList.RemoveAll(a => a.IdName.EndsWith("_UniqueRandom"));
            _affixInfoList.RemoveAll(a => a.IdName.EndsWith("_Unique"));
            _affixInfoList.RemoveAll(a => a.IdName.EndsWith("Rebalance")); // e.g. CDR_Rupture_S2Rebalance
            _affixInfoList.RemoveAll(a => a.IdName.EndsWith("_Lesser"));
            _affixInfoList.RemoveAll(a => a.IdName.EndsWith("_Greater"));
            _affixInfoList.RemoveAll(a => a.IdName.Contains("_Legendary_"));
            _affixInfoList.RemoveAll(a => a.IdName.EndsWith("_Always1")); // Remove all _Always1 affixes - Used by Rank(s) of affixes
            _affixInfoList.RemoveAll(a => a.IdName.EndsWith("_Scaled2H")); // Remove all "_Scaled2H" affixes - Used by Rank(s) of affixes
        }

        #endregion
    }
}
