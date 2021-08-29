using System;
using System.Collections.Generic;

namespace BWF.Api.Services
{
    public class CheckResult
    {
        public readonly Dictionary<string, int> Words = new Dictionary<string, int>();
        public TimeSpan ProcessingTime { get;internal set; }
        public TimeSpan DBReadTime { get; internal set; }
        public void AddBadWord(string s)
        {
            if (!Words.ContainsKey(s))
            {
                Words[s] = 1;
            }
            else
            {
                Words[s]++;
            }
        }
    }
}
