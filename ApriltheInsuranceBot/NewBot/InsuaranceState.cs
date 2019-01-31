using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApriltheInsuranceBot.NewBot
{
    public class InsuaranceState
    {
        public InsuranceQuoteForm InsuranceQuoteForm { get; set; } = new InsuranceQuoteForm();

        public string UserLanguage { get; set; }

        public string UserChoice { get; set; }
    }
}
