using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace D4DataParser.Entities
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

    public class AffixMeta
    {
        public string __fileName__ { get; set; }
        public int __snoID__ { get; set; }
        public string __type__ { get; set; }
        public long __typeHash__ { get; set; }
        public int dwFlags { get; set; }
        public int eMagicType { get; set; }
        public int eCategory { get; set; }
        public int nItemPowerMin { get; set; }
        public int nItemPowerMax { get; set; }
        public TCost tCost { get; set; }
        public SnoRareNamePrefixStringList snoRareNamePrefixStringList { get; set; }
        public SnoRareNameSuffixStringList snoRareNameSuffixStringList { get; set; }
        public GbidAffixFamily gbidAffixFamily { get; set; }
        public List<int> fAllowedForPlayerClass { get; set; }
        public List<int> arAllowedItemLabels { get; set; }
        public int dwAllowedQualityLevels { get; set; }
        public int eAffixType { get; set; }
        public List<PtItemAffixAttribute> ptItemAffixAttributes { get; set; }
        public List<double> arStaticValues { get; set; }
        public SnoPassivePower snoPassivePower { get; set; }
        public SnoClassRequirement snoClassRequirement { get; set; }
        public List<ArAffixSkillTag> arAffixSkillTags { get; set; }
        public List<ArPowerToModify> arPowerToModify { get; set; }
        public List<ArSkillTagsToAdd> arSkillTagsToAdd { get; set; }
        public List<ArSkillTagsToRemove> arSkillTagsToRemove { get; set; }
        public GbidMalignantColor gbidMalignantColor { get; set; }
    }

    public class ArAffixSkillTag
    {
        public object __raw__ { get; set; }
        public string __type__ { get; set; }
        public long __typeHash__ { get; set; }
        public int group { get; set; }
        public string name { get; set; }
    }

    public class ArPowerToModify
    {
        public int __raw__ { get; set; }
        public int __group__ { get; set; }
        public string __type__ { get; set; }
        public long __typeHash__ { get; set; }
        public string __targetFileName__ { get; set; }
        public string groupName { get; set; }
        public string name { get; set; }
    }

    public class ArSkillTagsToAdd
    {
        public long __raw__ { get; set; }
        public string __type__ { get; set; }
        public long __typeHash__ { get; set; }
        public int group { get; set; }
        public string name { get; set; }
    }

    public class ArSkillTagsToRemove
    {
        public long __raw__ { get; set; }
        public string __type__ { get; set; }
        public long __typeHash__ { get; set; }
        public int group { get; set; }
        public string name { get; set; }
    }

    public class GbidAffixFamily
    {
        public long __raw__ { get; set; }
        public string __type__ { get; set; }
        public long __typeHash__ { get; set; }
        public int group { get; set; }
    }

    public class GbidFormula
    {
        public long __raw__ { get; set; }
        public string __type__ { get; set; }
        public long __typeHash__ { get; set; }
        public int group { get; set; }
        public string name { get; set; }
    }

    public class GbidMalignantColor
    {
        public long __raw__ { get; set; }
        public string __type__ { get; set; }
        public long __typeHash__ { get; set; }
        public int group { get; set; }
        public string name { get; set; }
    }

    public class PtItemAffixAttribute
    {
        public string __type__ { get; set; }
        public long __typeHash__ { get; set; }
        public bool bIgnoreLegendaryScaling { get; set; }
        public bool unk_288eecd { get; set; }
        public TAttribute tAttribute { get; set; }
    }

    public class SnoClassRequirement
    {
        public long __raw__ { get; set; }
        public int __group__ { get; set; }
        public string __type__ { get; set; }
        public long __typeHash__ { get; set; }
        public string __targetFileName__ { get; set; }
        public string groupName { get; set; }
        public string name { get; set; }
    }

    public class SnoPassivePower
    {
        public long __raw__ { get; set; }
        public int __group__ { get; set; }
        public string __type__ { get; set; }
        public long __typeHash__ { get; set; }
        public string __targetFileName__ { get; set; }
        public string groupName { get; set; }
        public string name { get; set; }
    }

    public class SnoRareNamePrefixStringList
    {
        public long __raw__ { get; set; }
        public int __group__ { get; set; }
        public string __type__ { get; set; }
        public long __typeHash__ { get; set; }
        public string __targetFileName__ { get; set; }
        public string groupName { get; set; }
        public string name { get; set; }
    }

    public class SnoRareNameSuffixStringList
    {
        public long __raw__ { get; set; }
        public int __group__ { get; set; }
        public string __type__ { get; set; }
        public long __typeHash__ { get; set; }
        public string __targetFileName__ { get; set; }
        public string groupName { get; set; }
        public string name { get; set; }
    }

    public class SzAttributeFormula
    {
        public string value { get; set; }
        public string compiled { get; set; }
    }

    public class TAttribute
    {
        public string __type__ { get; set; }
        public long __typeHash__ { get; set; }
        public int eAttribute { get; set; }
        public string __eAttribute_name__ { get; set; }
        public uint nParam { get; set; }
        public SzAttributeFormula szAttributeFormula { get; set; }
        public GbidFormula gbidFormula { get; set; }
        public UnkDffdf28 unk_dffdf28 { get; set; }
    }

    public class TCost
    {
        public string value { get; set; }
        public string compiled { get; set; }
    }

    public class UnkDffdf28
    {
        public string value { get; set; }
        public string compiled { get; set; }
    }
}
