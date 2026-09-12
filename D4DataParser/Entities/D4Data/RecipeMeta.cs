using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace D4DataParser.Entities.D4Data
{
    public class RecipeMeta
    {        
        [JsonPropertyName("__fileName__")]
        public string FileName { get; set; } = string.Empty;

        [JsonPropertyName("__type__")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("temperingData")]
        public TemperingData TemperingData { get; set; } = new();
    }

    public class TemperingData
    {
        [JsonPropertyName("__type__")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("arTemperedAffixes")]
        public List<ARTemperedAffix> ARTemperedAffixes { get; set; } = [];
    }

    public class ARTemperedAffix
    {
        [JsonPropertyName("snoAffix")]
        public SnoAffix SnoAffix { get; set; } = new();
    }
}
