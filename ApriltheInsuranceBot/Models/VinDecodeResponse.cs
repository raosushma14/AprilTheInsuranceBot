using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApriltheInsuranceBot.Models
{
    public class VinDecodeResponse
    {
        [JsonProperty("Message")]
        public string Message { get; set; }

        [JsonProperty("Results")]
        public IEnumerable<VinDecodeResult> Results { get; set; }
    }

    public class VinDecodeResult
    {
        [JsonProperty("Variable")]
        public string Variable { get; set; }

        [JsonProperty("Value")]
        public string Value { get; set; }
    }
}
