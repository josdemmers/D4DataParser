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

namespace D4DataParser.Parsers
{
    public class AffixParser
    {
        private string _d4dataPath = string.Empty;
        private bool _keepDuplicates = false;
        private string _language = string.Empty;
        private List<string> _languages = new List<string>();

        private List<AffixMeta> _affixMetaJsonList = new List<AffixMeta>();
        Localisation _localisationJson = new Localisation();
        List<PowerMeta> _powerMetaJsonList = new List<PowerMeta>();
        private List<AffixInfo> _affixInfoList = new List<AffixInfo>();

        private Dictionary<uint, string> _mappingClassRestrictions = new Dictionary<uint, string>();
        private Dictionary<uint, string> _mappingCrowdControlledTypes = new Dictionary<uint, string>();
        private Dictionary<uint, string> _mappingCrowdControlTypes = new Dictionary<uint, string>();
        private Dictionary<uint, string> _mappingDamageTypes = new Dictionary<uint, string>(); 
        private Dictionary<uint, string> _mappingDotTypes = new Dictionary<uint, string>();
        private Dictionary<uint, string> _mappingNecroPetTypes = new Dictionary<uint, string>();
        private Dictionary<uint, string> _mappingResources = new Dictionary<uint, string>();
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
        public bool KeepDuplicates { get => _keepDuplicates; set => _keepDuplicates = value; }

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
            // .\d4data\json\enUS_Text\meta\StringList\AttributeDescriptions.stl.json
            _mappingClassRestrictions.Clear();
            _mappingClassRestrictions.Add(0, "Mana_Restriction");
            _mappingClassRestrictions.Add(1, "Spirit_Restriction");
            _mappingClassRestrictions.Add(2, "Fury_Restriction");
            _mappingClassRestrictions.Add(3, "Energy_Restriction");
            _mappingClassRestrictions.Add(4, "Essence_Restriction");

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

                if (KeepDuplicates && !language.Equals("enUS")) continue;

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

            // Create affix class
            foreach (var affix in affixDictionary)
            {
                _affixInfoList.Add(new AffixInfo
                {
                    IdSno = affix.Key,
                    IdName = affix.Value,
                    IsTemperingAvailable = affix.Value.StartsWith("Tempered")
                });
            }

            if (!KeepDuplicates)
            {
                // Remove unwanted affixes.
                RemoveUnwantedAffixes(language);
            }

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

                // Bug - Fix sorc affixes
                if (affix.IdName.StartsWith("Tempered") && affix.IdName.Contains("_Sorc_"))
                {
                    allowedForPlayerClass = new List<int> { 1, 0, 0, 0, 0 };
                }

                // Bug - Fix necro affixes
                if (affix.IdName.StartsWith("Tempered") && affix.IdName.Contains("_Necro_"))
                {
                    allowedForPlayerClass = new List<int> { 0, 0, 0, 0, 1 };
                }

                // LocalisationId
                var itemAffixAttributes = affixMeta.ptItemAffixAttributes ?? new List<PtItemAffixAttribute>();
                if (!itemAffixAttributes.Any())
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: No localisation data available.");
                    continue;
                }

                foreach (var itemAffixAttribute in itemAffixAttributes)
                {
                    // Attack_Speed_Bonus_On_Dodge --> IGNORE
                    // Attack_Speed_Bonus_On_Dodge_Duration --> Attack_Speed_Bonus_After_Dodge
                    // Barrier_When_Struck_Percent_Chance
                    // Blood_Orb_Pickup_Damage_Percent_Bonus --> IGNORE
                    // Blood_Orb_Pickup_Damage_Bonus_Duration --> Blood_Orb_Pickup_Damage_Combined
                    // Damage_Bonus_On_Elite_Kill --> IGNORE
                    // Damage_Bonus_On_Elite_Kill_Duration --> Damage_Bonus_On_Elite_Kill_Combined
                    // Damage_Bonus_Percent_On_Dodge --> IGNORE
                    // Damage_Bonus_Percent_On_Dodge_Duration --> Damage_Bonus_Percent_After_Dodge
                    // Evade_Movement_Speed --> IGNORE
                    // Evade_Movement_Speed_Duration --> Evade_Movement_Dodge_Chance
                    // Fortified_When_Struck_Percent_Chance
                    // Fortified_When_Struck_Amount
                    // Item_Find
                    // Mount_Fear_Reduction_Pct
                    // Movement_Bonus_On_Elite_Kill --> IGNORE
                    // Movement_Bonus_On_Elite_Kill_Duration --> Movement_Speed_Bonus_On_Elite_Kill
                    // On_Hit_Vulnerable_Proc_Chance --> IGNORE
                    // On_Hit_Vulnerable_Proc_Duration_Seconds --> On_Hit_Vulnerable_Proc
                    // Weapon_On_Hit_Percent_Bleed_Proc_Chance
                    // Weapon_On_Hit_Percent_Bleed_Proc_Damage
                    // Weapon_On_Hit_Percent_Bleed_Proc_Duration

                    string localisationId = itemAffixAttribute.tAttribute.__eAttribute_name__ ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(localisationId))
                    {
                        Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: No localisation data available.");
                        continue;
                    }

                    // Replace some localisationIds with an id that is available in AttributeDescriptions.stl.json
                    if (localisationId.Equals("Attack_Speed_Bonus_On_Dodge_Duration")) localisationId = "Attack_Speed_Bonus_After_Dodge";
                    if (localisationId.Equals("Blood_Orb_Pickup_Damage_Bonus_Duration")) localisationId = "Blood_Orb_Pickup_Damage_Combined";
                    if (localisationId.Equals("Damage_Bonus_On_Elite_Kill_Duration")) localisationId = "Damage_Bonus_On_Elite_Kill_Combined";
                    if (localisationId.Equals("Damage_Bonus_Percent_On_Dodge_Duration")) localisationId = "Damage_Bonus_Percent_After_Dodge";
                    if (localisationId.Equals("Evade_Movement_Speed_Duration")) localisationId = "Evade_Movement_Dodge_Chance";
                    if (localisationId.Equals("Movement_Bonus_On_Elite_Kill_Duration")) localisationId = "Movement_Speed_Bonus_On_Elite_Kill";
                    if (localisationId.Equals("On_Hit_Vulnerable_Proc_Duration_Seconds")) localisationId = "On_Hit_Vulnerable_Proc";

                    // Replace localisationIds with sub localisationIds when available
                    if (_localisationJson.arStrings.Any(a => a.szLabel.StartsWith($"{localisationId}#")) &&
                        !localisationId.Equals("Resistance"))
                    {
                        uint subSno = itemAffixAttribute.tAttribute.nParam;
                        string subLocalisationId = string.Empty;
                        if (localisationId.Equals("AoE_Size_Bonus_Per_Power") ||
                            localisationId.Equals("Bonus_Count_Per_Power") ||
                            localisationId.Equals("Bonus_Percent_Per_Power") ||
                            localisationId.Equals("Cleave_Damage_Bonus_Percent_Per_Power") ||
                            localisationId.Equals("Damage_Percent_Bonus_While_Affected_By_Power") ||
                            localisationId.Equals("Movement_Speed_Bonus_Percent_Per_Power") ||
                            localisationId.Equals("Percent_Bonus_Projectiles_Per_Power") ||
                            localisationId.Equals("Power_Cooldown_Reduction_Percent"))
                        {
                            string subId = GetPowerId(subSno);
                            if(!string.IsNullOrWhiteSpace(subId))
                            {
                                subLocalisationId = $"{localisationId}#{subId}";
                            }
                        }
                        else if (localisationId.Equals("Damage_Percent_Bonus_Per_Skill_Tag") ||
                            localisationId.Equals("Damage_Percent_Bonus_To_Targets_Affected_By_Skill_Tag"))
                        {
                            string subId = GetSkillTagId(subSno);
                            if (!string.IsNullOrWhiteSpace(subId))
                            {
                                subLocalisationId = $"{localisationId}#{subId}";
                            }
                        }
                        else if(localisationId.Equals("Primary_Resource_Gain_Bonus_Percent_Per_Weapon_Requirement"))
                        {
                            string subId = GetWeaponId(subSno);
                            if (!string.IsNullOrWhiteSpace(subId))
                            {
                                subLocalisationId = $"{localisationId}#{subId}";
                            }
                        }
                        else
                        {
                            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Sub localisation data available but rules not set. ({localisationId})");
                            throw new NotImplementedException();
                        }

                        localisationId = !string.IsNullOrWhiteSpace(subLocalisationId) && _localisationJson.arStrings.Any(a => a.szLabel.Equals(subLocalisationId)) ? subLocalisationId : localisationId;
                    }

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
                    if (affixAttribute.LocalisationId.Equals("AoE_Size_Bonus_Per_Power") || affixAttribute.LocalisationId.StartsWith("AoE_Size_Bonus_Per_Power#") ||
                        affixAttribute.LocalisationId.Equals("Attack_Speed_Percent_Bonus_For_Power") ||
                        affixAttribute.LocalisationId.Equals("Blood_Orb_Bonus_Chance_Per_Power") ||
                        affixAttribute.LocalisationId.Equals("Bonus_Count_Per_Power") || affixAttribute.LocalisationId.StartsWith("Bonus_Count_Per_Power#") ||
                        affixAttribute.LocalisationId.Equals("Bonus_Percent_Per_Power") || affixAttribute.LocalisationId.StartsWith("Bonus_Percent_Per_Power#") ||
                        affixAttribute.LocalisationId.Equals("Cleave_Damage_Bonus_Percent_Per_Power") || affixAttribute.LocalisationId.StartsWith("Cleave_Damage_Bonus_Percent_Per_Power#") ||
                        affixAttribute.LocalisationId.Equals("Combat_Effect_Chance_Bonus_Per_Skill") ||
                        affixAttribute.LocalisationId.Equals("Damage_Percent_Bonus_While_Affected_By_Power") || affixAttribute.LocalisationId.StartsWith("Damage_Percent_Bonus_While_Affected_By_Power#") ||
                        affixAttribute.LocalisationId.Equals("Movement_Speed_Bonus_Percent_Per_Power") || affixAttribute.LocalisationId.StartsWith("Movement_Speed_Bonus_Percent_Per_Power#") ||
                        affixAttribute.LocalisationId.Equals("Percent_Bonus_Projectiles_Per_Power") || affixAttribute.LocalisationId.StartsWith("Percent_Bonus_Projectiles_Per_Power#") ||
                        affixAttribute.LocalisationId.Equals("Power Bonus Attack Radius Percent") ||
                        affixAttribute.LocalisationId.Equals("Power_Cooldown_Reduction_Percent") || affixAttribute.LocalisationId.StartsWith("Power_Cooldown_Reduction_Percent#") ||
                        affixAttribute.LocalisationId.Equals("Power_Crit_Percent_Bonus") ||
                        affixAttribute.LocalisationId.Equals("Power_Damage_Percent_Bonus") ||
                        affixAttribute.LocalisationId.Equals("Power_Duration_Bonus_Pct") ||
                        affixAttribute.LocalisationId.Equals("Power_Resource_Cost_Reduction_Percent") ||
                        affixAttribute.LocalisationId.Equals("Resource_Gain_Bonus_Percent_Per_Power") ||
                        affixAttribute.LocalisationId.Equals("Skill_Rank_Bonus") ||
                        affixAttribute.LocalisationId.Equals("Talent_Rank_Bonus"))
                    {
                        ReplaceSkillPlaceholders(affix, _powerMetaJsonList);
                        AddClassRestriction(affix);
                    }
                    else if (affixAttribute.LocalisationId.Equals("Attack_Speed_Percent_Bonus_Per_Skill_Tag") ||
                        affixAttribute.LocalisationId.Equals("Crit_Damage_Percent_Per_Skill_Tag") ||
                        affixAttribute.LocalisationId.Equals("Crit_Percent_Bonus_Per_Skill_Tag") ||
                        affixAttribute.LocalisationId.Equals("Damage_Percent_Bonus_Per_Skill_Tag") || affixAttribute.LocalisationId.StartsWith("Damage_Percent_Bonus_Per_Skill_Tag#") ||
                        affixAttribute.LocalisationId.Equals("Damage_Percent_Bonus_To_Targets_Affected_By_Skill_Tag") || affixAttribute.LocalisationId.StartsWith("Damage_Percent_Bonus_To_Targets_Affected_By_Skill_Tag#") ||
                        affixAttribute.LocalisationId.Equals("Hit_Effect_Chance_Bonus_Per_Skill_Tag") ||
                        affixAttribute.LocalisationId.Equals("Overpower_Damage_Percent_Bonus_Per_Skill_Tag") ||
                        affixAttribute.LocalisationId.Equals("Per_Skill_Tag_Buff_Duration_Bonus_Percent") ||
                        affixAttribute.LocalisationId.Equals("Primary_Resource_On_Cast_Per_Skill_Tag") ||
                        affixAttribute.LocalisationId.Equals("Skill_Rank_Skill_Tag_Bonus") ||
                        affixAttribute.LocalisationId.Equals("Skill_Tag_Cooldown_Reduction_Percent") ||
                        affixAttribute.LocalisationId.Equals("Resource_Gain_Bonus_Percent_Per_Skill_Tag"))
                    {
                        ReplaceSkillTagPlaceholders(affix);
                    }
                    else if (affixAttribute.LocalisationId.Equals("Resource_Cost_Reduction_Percent") ||
                        affixAttribute.LocalisationId.Equals("Resource_Max_Bonus") ||
                        affixAttribute.LocalisationId.Equals("Resource_On_Kill") ||
                        affixAttribute.LocalisationId.Equals("Resource_Regen_Per_Second"))
                    {
                        ReplaceResourcePlaceholders(affix);
                    }
                    else if (affixAttribute.LocalisationId.Equals("Combat_Effect_Chance_Bonus_Per_Damage_Type") ||
                        affixAttribute.LocalisationId.Equals("Damage_Type_Crit_Damage_Percent_Bonus") ||
                        affixAttribute.LocalisationId.Equals("Damage_Type_Crit_Percent_Bonus_Vs_Elites") ||
                        affixAttribute.LocalisationId.Equals("Damage_Type_Percent_Bonus") ||
                        affixAttribute.LocalisationId.Equals("DOT_DPS_Bonus_Percent_Per_Damage_Type") ||
                        affixAttribute.LocalisationId.Equals("Proc_Flat_Element_Damage_On_Hit") ||
                        affixAttribute.LocalisationId.Equals("Resistance"))
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
                    else if (affixAttribute.LocalisationId.Equals("Damage_Percent_Bonus_Per_Weapon_Requirement") ||
                        affixAttribute.LocalisationId.Equals("Overpower_Damage_Percent_Bonus_Per_Weapon_Requirement") ||
                        affixAttribute.LocalisationId.Equals("Primary_Resource_Gain_Bonus_Percent_Per_Weapon_Requirement") || affixAttribute.LocalisationId.StartsWith("Primary_Resource_Gain_Bonus_Percent_Per_Weapon_Requirement#"))
                    {
                        ReplaceWeaponTypePlaceholders(affix);
                    }
                    else if (affixAttribute.LocalisationId.Equals("Damage_Percent_Reduction_From_Dotted_Enemy") ||
                        affixAttribute.LocalisationId.Equals("Damage_Percent_Bonus_Against_Dot_Type"))
                    {
                        ReplaceDotTypePlaceholders(affix);
                    }
                    else if (affixAttribute.LocalisationId.Equals("NecroArmy_Pet_Type_Damage_Bonus_Pct") ||
                        affixAttribute.LocalisationId.Equals("NecroArmy_Pet_Type_Inherit_Thorns_Bonus_Pct"))
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

            // List all affixes with no description
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Affixes with missing localisation: {_affixInfoList.Count(a => string.IsNullOrWhiteSpace(a.Description))}");
            _affixInfoList.ForEach(a =>
            {
                if (string.IsNullOrWhiteSpace(a.Description))
                {
                    Debug.WriteLine($"{a.IdName}");
                }
            });

            // Remove all affixes with no description
            _affixInfoList.RemoveAll(a => string.IsNullOrWhiteSpace(a.Description));

            // Add a cleaned up description for fuzzy searches.
            AddCleanDescription();

            // Remove normal affixes when there is an equal tempered affix.
            var duplicates = _affixInfoList.GroupBy(a => a.DescriptionClean).Where(a => a.Count() > 1);
            List<string> affixesToRemove = new();
            if (duplicates.Any() && !KeepDuplicates)
            {
                foreach (var group in duplicates)
                {
                    if (group.Count() == 2 && group.Count(a => a.IdName.StartsWith("Tempered")) == 1)
                    {
                        string affixToRemove = group.First(a => !a.IdName.StartsWith("Tempered")).IdName;
                        affixesToRemove.Add(affixToRemove);
                    }
                }

                foreach (var affix in affixesToRemove)
                {
                    Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Removed: {affix}");
                    _affixInfoList.RemoveAll(a => a.IdName.Equals(affix));
                }
            }

            // Sort
            _affixInfoList.Sort((x, y) =>
            {
                //return string.Compare(x.Description, y.Description, StringComparison.Ordinal);
                return string.Compare(x.IdName, y.IdName, StringComparison.Ordinal);
            });

            // Save affixes
            SaveAffixes();

            watch.Stop();
            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Elapsed time (Total): {watch.ElapsedMilliseconds}");
            Debug.WriteLine(string.Empty);
        }

        private void SaveAffixes()
        {
            string fileName = KeepDuplicates ? $"Data/Affixes.Full.{_language}.json" : $"Data/Affixes.{_language}.json";
            string path = Path.GetDirectoryName(fileName) ?? string.Empty;
            Directory.CreateDirectory(path);

            using FileStream stream = File.Create(fileName);
            var options = new JsonSerializerOptions { WriteIndented = true };
            options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            JsonSerializer.Serialize(stream, _affixInfoList, options);
        }

        private void ValidateAffixes()
        {
            var duplicates = _affixInfoList.GroupBy(a => a.DescriptionClean).Where(a => a.Count() > 1);
            if (duplicates.Any())
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Duplicates found!");

                foreach (var group in duplicates)
                {
                    Console.WriteLine("Key: {0}", group.Key);
                    foreach (var affixInfo in group)
                    {
                        Debug.WriteLine($"{affixInfo.IdName}: {affixInfo.DescriptionClean}");
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
            // Parse GBID.json
            int skillTagIndex = 56;
            var jsonAsText = File.ReadAllText($"{Path.GetDirectoryName(CoreTOCPath)}\\..\\GBID.json");
            var gbidDictionary = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, Dictionary<uint, List<string>>>>(jsonAsText);
            var skillTagDictionary = gbidDictionary[skillTagIndex];

            return skillTagDictionary[sno][0];
        }

        private string GetWeaponId(uint sno)
        {
            // Parse CoreTOC.dat.json
            int weaponTypeIndex = 116;
            var jsonAsText = File.ReadAllText(CoreTOCPath);
            var coreTOCDictionary = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, Dictionary<uint, string>>>(jsonAsText);
            var weaponTypeDictionary = coreTOCDictionary[weaponTypeIndex];

            return weaponTypeDictionary[sno];
        }

        private void AddClassRestriction(AffixInfo affix)
        {
            int classCount = affix.AllowedForPlayerClass.Count(c => c == 1);
            uint classIndex = (uint)affix.AllowedForPlayerClass.IndexOf(1);

            var classLoc = _localisationJson.arStrings.FirstOrDefault(a => a.szLabel.Equals(_mappingClassRestrictions[classIndex], StringComparison.OrdinalIgnoreCase));
            if (classLoc == null || classCount != 1) return;

            affix.ClassRestriction = classLoc.szText;
        }

        private void AddCleanDescription()
        {
            foreach (var affix in _affixInfoList)
            {
                affix.DescriptionClean = affix.Description.Replace("+", string.Empty);
                affix.DescriptionClean = affix.DescriptionClean.Replace("#", string.Empty);
                affix.DescriptionClean = affix.DescriptionClean.Replace("%", string.Empty);
                affix.DescriptionClean = affix.DescriptionClean.Replace("  ", " ");
                affix.DescriptionClean = affix.DescriptionClean.Replace(" .", ".");
                affix.DescriptionClean = affix.DescriptionClean.Trim();

                // Append class restriction
                if (!string.IsNullOrEmpty(affix.ClassRestriction))
                {
                    affix.DescriptionClean += $" {affix.ClassRestriction}";
                }
            }
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

                // Missed by regex
                affix.Description = affix.Description.Replace("{VALUE2}", "#");

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

                affix.Description = affix.Description.Replace("{c_important}", string.Empty);
                affix.Description = affix.Description.Replace("{c_label}", string.Empty);
                affix.Description = affix.Description.Replace("{/c}", string.Empty);

                // Prefix found in frFR
                affix.Description = affix.Description.Replace("|2", string.Empty);

                // deDE
                affix.Description = affix.Description.Replace("|4Aufladung:Aufladungen;", "Aufladungen");
                affix.Description = affix.Description.Replace("|4Rang:Ränge;", "Ränge");
                affix.Description = affix.Description.Replace("|4Wirkung:Wirkungen;", "Wirkungen");

                // enUS
                affix.Description = affix.Description.Replace("|4Cast:Casts;", "Casts");
                affix.Description = affix.Description.Replace("|4Charge:Charges;", "Charges");
                affix.Description = affix.Description.Replace("|4Rank:Ranks;", "Ranks");
                affix.Description = affix.Description.Replace("|4Second:Seconds;", "Seconds");

                // esES
                affix.Description = affix.Description.Replace("|4carga máxima:cargas máximas;", "cargas máximas");
                affix.Description = affix.Description.Replace("|4rango:rangos;", "rangos");
                affix.Description = affix.Description.Replace("|4lanzamiento:lanzamientos;", "lanzamientos");

                // esMX
                affix.Description = affix.Description.Replace("|4carga:cargas;", "cargas");
                affix.Description = affix.Description.Replace("|4rango:rangos;", "rangos");
                affix.Description = affix.Description.Replace("|4segundo:segundos;", "segundos");
                affix.Description = affix.Description.Replace("|4lanzamiento:lanzamientos;", "lanzamientos");

                // frFR
                affix.Description = affix.Description.Replace("|4charge:charges;", "charges");
                affix.Description = affix.Description.Replace("|4lancer supplémentaire:lancers supplémentaires;", "lancers supplémentaires");
                affix.Description = affix.Description.Replace("|4rang:rangs;", "rangs");

                // itIT
                affix.Description = affix.Description.Replace("|4carica massima:cariche massime;", "cariche massime");
                affix.Description = affix.Description.Replace("|4grado:gradi;", "gradi");

                // plPL
                affix.Description = affix.Description.Replace("|4ładunek:ładunki:ładunków;", "ładunków");
                affix.Description = affix.Description.Replace("|4ranga:rangi:rang;", "rang");
                affix.Description = affix.Description.Replace("|4użycie:użycia:użyć;", "użyć");

                // ptBR
                affix.Description = affix.Description.Replace("|4carga:cargas;", "cargas");
                affix.Description = affix.Description.Replace("|4grau:graus;", "graus");
                affix.Description = affix.Description.Replace("|4lançamento:lançamentos;", "lançamentos");
                affix.Description = affix.Description.Replace("|4segundo:segundos;", "segundos");

                // ruRU
                affix.Description = affix.Description.Replace("|4атаку:атаки:атак;", "атак");

                // trTR
                affix.Description = affix.Description.Replace("|4Yükü:Yükü;", "Yükü");
                affix.Description = affix.Description.Replace("|4Kademesi:Kademesi;", "Kademesi");
                affix.Description = affix.Description.Replace("|4Saniye:Saniye;", "Saniye");

                // zhTW
                affix.Description = affix.Description.Replace("|4次施放:次施放;", "次施放");
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
                    if (File.Exists(fileNameLoc))
                    {
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
                    else
                    {
                        Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name}: Power_{Path.GetFileNameWithoutExtension(fileName)}.stl.json not found.");
                    }
                }
            }
        }

        private void ReplaceResourcePlaceholders(AffixInfo affix)
        {
            foreach (var affixAttribute in affix.AffixAttributes)
            {
                if (affixAttribute.LocalisationId.Equals("Resource_Cost_Reduction_Percent") || 
                    affixAttribute.LocalisationId.Equals("Resource_Max_Bonus") ||
                    affixAttribute.LocalisationId.Equals("Resource_On_Kill") ||
                    affixAttribute.LocalisationId.Equals("Resource_Regen_Per_Second"))
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
                if (affixAttribute.LocalisationId.Equals("Combat_Effect_Chance_Bonus_Per_Damage_Type") ||
                    affixAttribute.LocalisationId.Equals("Damage_Type_Crit_Damage_Percent_Bonus") ||
                    affixAttribute.LocalisationId.Equals("Damage_Type_Crit_Percent_Bonus_Vs_Elites") ||
                    affixAttribute.LocalisationId.Equals("Damage_Type_Percent_Bonus") ||
                    affixAttribute.LocalisationId.Equals("DOT_DPS_Bonus_Percent_Per_Damage_Type") ||
                    affixAttribute.LocalisationId.Equals("Proc_Flat_Element_Damage_On_Hit") ||
                    affixAttribute.LocalisationId.Equals("Resistance"))
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
                if (affixAttribute.LocalisationId.Equals("Attack_Speed_Percent_Bonus_Per_Skill_Tag") ||
                    affixAttribute.LocalisationId.Equals("Crit_Damage_Percent_Per_Skill_Tag") ||
                    affixAttribute.LocalisationId.Equals("Crit_Percent_Bonus_Per_Skill_Tag") ||
                    affixAttribute.LocalisationId.Equals("Damage_Percent_Bonus_Per_Skill_Tag") ||
                    affixAttribute.LocalisationId.Equals("Damage_Percent_Bonus_To_Targets_Affected_By_Skill_Tag") ||
                    affixAttribute.LocalisationId.Equals("Hit_Effect_Chance_Bonus_Per_Skill_Tag") ||
                    affixAttribute.LocalisationId.Equals("Overpower_Damage_Percent_Bonus_Per_Skill_Tag") ||
                    affixAttribute.LocalisationId.Equals("Per_Skill_Tag_Buff_Duration_Bonus_Percent") ||
                    affixAttribute.LocalisationId.Equals("Primary_Resource_On_Cast_Per_Skill_Tag") ||
                    affixAttribute.LocalisationId.Equals("Skill_Rank_Skill_Tag_Bonus") ||
                    affixAttribute.LocalisationId.Equals("Skill_Tag_Cooldown_Reduction_Percent") ||
                    affixAttribute.LocalisationId.Equals("Resource_Gain_Bonus_Percent_Per_Skill_Tag"))
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
                    affixAttribute.LocalisationId.Equals("Overpower_Damage_Percent_Bonus_Per_Weapon_Requirement") ||
                    affixAttribute.LocalisationId.Equals("Primary_Resource_Gain_Bonus_Percent_Per_Weapon_Requirement"))
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
                if (affixAttribute.LocalisationId.Equals("NecroArmy_Pet_Type_Damage_Bonus_Pct") ||
                    affixAttribute.LocalisationId.Equals("NecroArmy_Pet_Type_Inherit_Thorns_Bonus_Pct"))
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

        private void RemoveUnwantedAffixes(string language)
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

            // Only keep the following affixes:
            // - S04
            // - Tempered
            // - Implicit (only those that exist as implicit only)
            _affixInfoList.RemoveAll(a => !a.IdName.StartsWith("S04_") && !a.IdName.StartsWith("Tempered") &&
                !a.IdName.Equals("INHERENT_Block") &&
                !a.IdName.Equals("INHERENT_Evade_Attack_Reset") &&
                !a.IdName.Equals("INHERENT_Evade_Charges") &&
                !a.IdName.Equals("INHERENT_Shield_Damage_Bonus") &&
                !a.IdName.Equals("PotionBarrier") &&
                !a.IdName.Equals("INHERENT_Damage_to_HighLife") &&
                !a.IdName.Equals("INHERENT_Damage_to_LowLife") &&
                !a.IdName.Equals("INHERENT_On_Kill_Health")
                );

            // TODO: Need to split INHERENT_Block into two affixes.
            // Workaround to split INHERENT_Block like this:
            // INHERENT_Block_Block_Chance
            // INHERENT_Block_Block_Damage_Percent
            // --> problem there is only one sno id(577278)
            // --> maybe by using a list of localistions instead?

            // Remove duplicates
            _affixInfoList.RemoveAll(a => a.IdName.EndsWith("Jewelry"));
            _affixInfoList.RemoveAll(a => a.IdName.EndsWith("_Lesser"));
            _affixInfoList.RemoveAll(a => a.IdName.EndsWith("_Shields"));

            // Remove tempered tier 1 and tier 2
            // TODO: Need better approach to only keep one for each tempered affix.
            _affixInfoList.RemoveAll(a => a.IdName.StartsWith("Tempered") && a.IdName.EndsWith("Tier1"));
            _affixInfoList.RemoveAll(a => a.IdName.StartsWith("Tempered") && a.IdName.EndsWith("Tier2"));

            // Remove specific duplicates            
            _affixInfoList.RemoveAll(a => a.IdName.Equals("S04_CoreStat_DexterityPercent")); // "+#% Dexterity", using "S04_CoreStat_Dexterity" instead.
            _affixInfoList.RemoveAll(a => a.IdName.Equals("S04_CoreStat_IntelligencePercent")); // "+#% Intelligence", using "S04_CoreStat_Intelligence" instead.
            _affixInfoList.RemoveAll(a => a.IdName.Equals("S04_CoreStat_StrengthPercent")); // "+#% Strength", using "S04_CoreStat_Strength" instead.
            _affixInfoList.RemoveAll(a => a.IdName.Equals("S04_CoreStat_WillpowerPercent")); // "+#% Willpower", using "S04_CoreStat_Willpower" instead.

            // Bugs??
            // - Affixes have the wrong localisation string
            // - Different affixes don't have an unique name in some languages
            // - Season 5: "+# to Concussive" renamed to "to Unstable Elixirs". Is that intended?. For now no action taken to change this.
            // TODO: - UPD - Check if localisation bugs are still present after each update
            _affixInfoList.RemoveAll(a => a.IdName.Equals("Tempered_PassiveRankBonus_Druid_08_Unrestrained")); // "+# to Nature's Reach", using "Tempered_PassiveRankBonus_Druid_06_NaturesReach" instead.
            _affixInfoList.RemoveAll(a => a.IdName.Equals("Tempered_PassiveRankBonus_Druid_09_Vigilance")); // "+# to Nature's Reach", using "Tempered_PassiveRankBonus_Druid_06_NaturesReach" instead.
            _affixInfoList.RemoveAll(a => a.IdName.Equals("Tempered_PassiveRankBonus_Druid_NatureMagic_T2_N3_AncestralFortitude")); // "+# to Crushing Earth", using "Tempered_PassiveRankBonus_Druid_NatureMagic_T2_N2_CrushingEarth" instead.
            _affixInfoList.RemoveAll(a => a.IdName.Equals("Tempered_PassiveRankBonus_Druid_NatureMagic_T4_N1_ElectricShock")); // "+# to Defiance", using "Tempered_PassiveRankBonus_Druid_NatureMagic_T4_N2_Defiance" instead.
            _affixInfoList.RemoveAll(a => a.IdName.Equals("Tempered_PassiveRankBonus_Druid_Shapeshifting_T1_N2_PredatoryInstinct")); // "+# to Quickshift", using "Tempered_PassiveRankBonus_Druid_Shapeshifting_T4_N1_Quickshift" instead.
            _affixInfoList.RemoveAll(a => a.IdName.Equals("Tempered_PassiveRankBonus_Druid_Shapeshifting_T4_N2_NaturalFortitude")); // "+# to Quickshift", using "Tempered_PassiveRankBonus_Druid_Shapeshifting_T4_N1_Quickshift" instead.
            _affixInfoList.RemoveAll(a => a.IdName.Equals("Tempered_PassiveRankBonus_Rogue_Discipline_T4_N1_AlchemicalAdvantage")); // "+# to Concussive", using "Tempered_PassiveRankBonus_Rogue_Discipline_T3_N4_Concussive" instead.
            if (language.Equals("esMX"))
            {
                // Tempered_Damage_Generic_Type_Lightning_Tier3
                // Tempered_Damage_Sorc_Tag_Shock_Tier3
                _affixInfoList.RemoveAll(a => a.IdName.Equals("Tempered_Damage_Sorc_Tag_Shock_Tier3")); // "+#% de daño de Rayo", using "Tempered_Damage_Generic_Type_Lightning_Tier3" instead.
            }
            else if (language.Equals("jaJP"))
            {
                // Tempered_Damage_Sorc_Tag_Conjuration_Tier3
                // Tempered_Damage_Necro_Tag_Summoning_Tier3
                _affixInfoList.RemoveAll(a => a.IdName.Equals("Tempered_Damage_Necro_Tag_Summoning_Tier3")); // "召喚スキルのダメージ+#%", using "Tempered_Damage_Sorc_Tag_Conjuration_Tier3" instead.
            }
        }

        #endregion
    }
}
