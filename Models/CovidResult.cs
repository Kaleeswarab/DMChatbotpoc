using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBot
{
    public class CovidResult
    {
        public string Result { get; set; }
        public string Country { get; set; }
        public string Active { get; set; }
        public string Death { get; set; }
        public DateTime Date { get; set; }
        public string General { get; set; }
        public string Recovered { get; set; }
        public string LanguageCode { get; set; }
    }
}
