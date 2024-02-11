﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D4DataParser.Entities
{
    public class AspectInfo
    {
        public int IdSno { get; set; }
        public string IdName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DescriptionClean { get; set; } = string.Empty;
        public string Localisation { get; set; } = string.Empty;
        public bool IsSeasonal { get; set; } = false;
        public bool IsCodex { get; set; } = false;
        public string Dungeon { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        /// <summary>
        /// None: 0 (Affixes)
        /// Legendary: 1 (Aspects)
        /// Unique: 2
        /// Test: 3
        /// </summary>
        public int MagicType { get; set; }
        public List<int> AllowedForPlayerClass { get; set; } = new List<int>();
        public List<int> AllowedItemLabels { get; set; } = new List<int>();
    }
}


