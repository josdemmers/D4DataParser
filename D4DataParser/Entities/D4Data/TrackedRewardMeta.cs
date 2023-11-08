namespace D4DataParser.Entities.D4Data
{
    public class TrackedRewardMeta
    {
        public string __fileName__ { get; set; }
        public int __snoID__ { get; set; }
        public string __type__ { get; set; }
        public long __typeHash__ { get; set; }
        public SnoAspect snoAspect { get; set; }
    }

    public class SnoAspect
    {
        public int __raw__ { get; set; }
        public int __group__ { get; set; }
        public string __type__ { get; set; }
        public long __typeHash__ { get; set; }
        public string __targetFileName__ { get; set; }
        public string groupName { get; set; }
        public string name { get; set; }
    }
}
