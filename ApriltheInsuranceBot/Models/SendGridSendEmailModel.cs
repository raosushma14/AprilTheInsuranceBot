using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApriltheInsuranceBot.Models
{
    class SendGridSendEmailModel
    {
        [JsonProperty("personalizations")]
        public IEnumerable<Recipient> Personalizations { get; set; }

        [JsonProperty("from")]
        public EmailAddress From { get; set; }

        [JsonProperty("subject")]
        public string Subject { get; set; }

        [JsonProperty("content")]
        public IEnumerable<EmailContent> Content { get; set; }
    }

    class EmailAddress
    {
        [JsonProperty("email")]
        public string Email { get; set; }
    }
    class Recipient
    {
        [JsonProperty("to")]
        public IEnumerable<EmailAddress> To { get; set; }
    }
    class EmailContent
    {
        [JsonProperty("type")]
        public string Type { get; set; }


        [JsonProperty("value")]
        public string Value { get; set; }
    }
    
}
