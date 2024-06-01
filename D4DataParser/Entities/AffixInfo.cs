﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D4DataParser.Entities
{
    public class AffixInfo
    {
        public int IdSno { get; set; }
        public string IdName { get; set; } = string.Empty;
        public string ClassRestriction { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DescriptionClean { get; set; } = string.Empty;
        public bool IsTemperingAvailable { get; set; } = false;
        /// <summary>
        /// None: 0 (Affixes)
        /// Legendary: 1 (Aspects)
        /// Unique: 2
        /// Test: 3
        /// </summary>
        public int MagicType { get; set; }
        public List<int> AllowedForPlayerClass { get; set; } = new List<int>();
        public List<int> AllowedItemLabels { get; set; } = new List<int>();
        public List<AffixAttribute> AffixAttributes { get; set; } = new List<AffixAttribute>();
    }

    public class AffixAttribute
    {
        public string LocalisationId { get; set; } = string.Empty;
        public uint LocalisationParameter { get; set; } // Keep this at uint, need to automatic fix overflowed values.
        public string LocalisationAttributeFormulaValue { get; set; } = string.Empty;
        public string Localisation { get; set; } = string.Empty;
    }
}
