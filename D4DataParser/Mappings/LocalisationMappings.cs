namespace D4DataParser.Mappings
{
    public static class LocalisationMappings
    {
        public static readonly Dictionary<uint, string> ClassRestrictions = new Dictionary<uint, string>
        {
            // .\d4data\json\enUS_Text\meta\StringList\AttributeDescriptions.stl.json
            { 0, "Mana_Restriction" },
            { 1, "Spirit_Restriction" },
            { 2, "Fury_Restriction" },
            { 3, "Energy_Restriction" },
            { 4, "Essence_Restriction" },
            { 5, "Spirit_Restriction" }  // TODO: Update this for season 6. Vigor_Restriction?
        };

        public static readonly Dictionary<uint, string> CrowdControlTypes = new Dictionary<uint, string>
        {
            // .\d4data\json\enUS_Text\meta\StringList\UIToolTips.stl.json
            { 0, "CC_Type_Slow" },
            { 1, "CC_Type_Immobilize" },
            { 2, "CC_Type_Stun" },
            { 7, "CC_Type_Disabled" }, // Daze
            { 9, "CC_Type_Chill" },
            { 10, "CC_Type_Frozen" },
            { 11, "CC_Type_Knockback" },
            { 13, "CC_Type_Fear" }
        };

        public static readonly Dictionary<uint, string> CrowdControlledTypes = new Dictionary<uint, string>
        {
            // .\d4data\json\enUS_Text\meta\StringList\UIToolTips.stl.json
            { 0, "Affected_By_CC_Type_Slow" },
            { 1, "Affected_By_CC_Type_Immobilize" },
            { 2, "Affected_By_CC_Type_Stun" },
            { 7, "Affected_By_CC_Type_Disabled" }, // Dazed
            { 9, "Affected_By_CC_Type_Chill" },
            { 10, "Affected_By_CC_Type_Frozen" },
            { 11, "Affected_By_CC_Type_Knockdown" },
            { 13, "Affected_By_CC_Type_Fear" }
        };

        public static readonly Dictionary<uint, string> DamageTypes = new Dictionary<uint, string>
        {
            // .\d4data\json\enUS_Text\meta\StringList\SkillTagNames.stl.json
            { 0, "DAMAGE_PHYSICAL" },
            { 1, "DAMAGE_FIRE" },
            { 2, "DAMAGE_LIGHTNING" },
            { 3, "DAMAGE_COLD" },
            { 4, "DAMAGE_POISON" },
            { 5, "DAMAGE_SHADOW" }
        };

        public static readonly Dictionary<uint, string> DotTypes = new Dictionary<uint, string>
        {
            // .\d4data\json\enUS_Text\meta\StringList\UIToolTips.stl.json
            { 0, "DOT_Damage_Physical" }, // Bleeding
            { 1, "DOT_Damage_Fire" },
            //{ ?, "DOT_Damage_Cold" }, // Chilled
            { 4, "DOT_Damage_Poison" },
            { 5, "DOT_Damage_Shadow" }
        };

        public static readonly Dictionary<uint, string> NecroPetNames = new Dictionary<uint, string>
        {
            // .\d4data\json\enUS_Text\meta\StringList\NecromancerArmy.stl.json
            { 0, "UnitType_Warrior" },
            { 1, "UnitType_Mage" },
            { 2, "UnitType_Golem" }
        };

        public static readonly Dictionary<uint, string> Resistance = new Dictionary<uint, string>
        {
            // .\d4data\json\enUS_Text\meta\StringList\AttributeDescriptions.stl.json
            { 1, "Fire_Gem" },
            { 2, "Lightning_Gem" },
            { 3, "Cold_Gem" },
            { 4, "Poison_Gem" },
            { 5, "Shadow_Gem" }
        };

        public static readonly Dictionary<uint, string> Resources = new Dictionary<uint, string>
        {
            // .\d4data\json\enUS_Text\meta\StringList\SkillTagNames.stl.json
            //{ 0, "RESOURCE_MANA" },
            //{ 1, "RESOURCE_FURY" },
            //{ 3, "RESOURCE_ENERGY" },
            //{ 5, "RESOURCE_SPIRIT" },
            //{ 6, "RESOURCE_ESSENCE" },
            //{ 7, "RESOURCE_***" } // Missing

            // .\d4data\json\enUS_Text\meta\StringList\UIToolTips.stl.json
            //{ 0, "Resource_Type_Mana" },
            //{ 1, "Resource_Type_Fury" },
            //{ 3, "Resource_Type_Energy" },
            //{ 5, "Resource_Type_Spirit" },
            //{ 6, "Resource_Type_Essence" },
            //{ 7, "Resource_Type_Vigor" }

            // .\d4data\json\enUS_Text\meta\StringList\FrontEnd.stl.json
            //{ 0, "SorcererResource" },
            //{ 1, "BarbarianResouce" }, // Rage. Note: typo BarbarianResouce instead of BarbarianResource
            //{ 3, "RogueResource" },
            //{ 5, "DruidResource" },
            //{ 6, "NecromancerResource" },
            //{ 7, "SpiritbornResource" }

            // .\d4data\json\enUS_Text\meta\StringList\SkillTags.stl.json
            { 0, "Search_ResourceMana_TagName" },
            { 1, "Search_ResourceFury_TagName" },
            { 3, "Search_ResourceEnergy_TagName" },
            { 5, "Search_ResourceSpirit_TagName" },
            { 6, "Search_ResourceEssence_TagName" },
            { 7, "Search_ResourceVigor_TagName" }
        };

        public static readonly Dictionary<uint, string> ShapeshiftForms = new Dictionary<uint, string>
        {
            // .\d4data\json\enUS_Text\meta\StringList\UIToolTips.stl.json
            { 0, "Shapeshift_Form_Human" }
            //{ ?, "Shapeshift_Form_Bear" }
            //{ ?, "Shapeshift_Form_Wolf" }
        };
    }
}
