using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace D4DataParser.Entities.D4Data
{
    public class ItemMeta
    {
        public string __fileName__ { get; set; }
        public int __snoID__ { get; set; }
        public string __type__ { get; set; }
        public long __typeHash__ { get; set; }
        public SnoItemType snoItemType {  get; set; }
        public int eMagicType { get; set; }
        public List<ArInherentAffix> arInherentAffixes { get; set; }
        public List<ArForcedAffix> arForcedAffixes { get; set; }
        public List<int> fUsableByClass { get; set; }
    }

    public class SnoItemType
    {
        public string groupName { get; set; }
        public string name { get; set; }
    }

    public class ArInherentAffix
    {
        public int __raw__ { get; set; }
        public string __targetFileName__ { get; set; }
        public string groupName { get; set; }
        public string name { get; set; }
    }

    public class ArForcedAffix
    {
        public int __raw__ { get; set; }
        public string __targetFileName__ { get; set; }
        public string groupName { get; set; }
        public string name { get; set; }
    }
}
