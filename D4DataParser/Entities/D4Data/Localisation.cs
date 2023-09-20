using System.Collections.Generic;

namespace D4DataParser.Entities.D4Data
{
    public class Localisation
    {
        public string __fileName__ { get; set; }
        public int __snoID__ { get; set; }
        public string __type__ { get; set; }
        public long __typeHash__ { get; set; }
        public List<ArString> arStrings { get; set; }
        public string ptMapStringTable { get; set; }
    }

    public class ArString
    {
        public string __type__ { get; set; }
        public long __typeHash__ { get; set; }
        public string szLabel { get; set; }
        public string szText { get; set; }
        public long hLabel { get; set; }
    }
}
