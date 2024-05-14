using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D4DataParser.Entities
{
    public class SigilInfo
    {
        public long IdSno { get; set; }
        public string IdName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DungeonZoneInfo {  get; set; } = string.Empty;
        public bool IsSeasonal { get; set; } = false;
        public string Type { get; set; } = string.Empty;
    }
}
