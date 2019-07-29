using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace gcal
{
    public static class FuzzyDateParser
    {
        private static readonly Dictionary<string, int> PeriodConversions = new Dictionary<string, int>()
        {
            {"second", 1},
            {"seconds", 1},
            {"minute", 60},
            {"minutes", 60},
            {"hour", 3600},
            {"hours", 3600},
            {"day", 86400},
            {"days", 86400},
            {"week", 604800},
            {"weeks", 604800},
            {"month", 2419200},
            {"months", 2419200}
        };

        /*
         * Very simple and rigid NL date parser.
         */
        public static TimeSpan ParseTime(string Date)
        {
            Match match;

            match = Regex.Match(Date, @"^(\d+) (.*)$");
            if (match.Success)
            {
                return new TimeSpan(0, 0, Convert.ToInt32(match.Groups[1].Value) * PeriodConversions[match.Groups[2].Value]);
            }
            else
            {
                return default(TimeSpan);
            }
        }
    }
}