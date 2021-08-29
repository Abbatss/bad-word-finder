using System;
using System.Collections.Generic;

namespace BWF.Api.Host.Models
{
    public class CheckResult
    {
        public List<WordResult> Words { get; set; } = new List<WordResult>();
        public long MilisecondsDuration { get; set; }
        public long DbReadMilisecondsDuration { get; set; }
    }
    public class WordResult
    {
       public string Word { get; set; }
       public int Count { get; set; }
    }
}
