using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Net;
using Newtonsoft.Json;

namespace gcal
{
    public static class MetaFilterFilter
    {

        private static string GetPageContents(string URL)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            using (var client = new WebClient())
            {
                client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                return client.DownloadString(URL);
            }

        }

        public static bool ParseUrl(string EventUrl, List<EventInformation> EventList)
        {
            Match match;
            string EventContents;

            EventContents = GetPageContents(EventUrl);

            match = Regex.Match(EventContents, @"<div id=""maplocation"" style=""line-height:100%;"">(.*?) at (.*?), (.*?)<div id=""address"" style=""font-size:12px;margin-top:3px;"">(.*?)\(");            // match = Regex.Match(EventContents, @"<script type=""application/ld\+json"">(.*?)</script>", RegexOptions.Singleline);
            if (match.Success)
            {
                string EventTitle;

                // TODO -- This is ugly but seems to mostly work. We still need a fuzzy/NLP date parser as DateTime.Parse is pretty dumb/rigid.
                DateTime StartTime = DateTime.Parse($"{match.Groups[1].Value} {DateTime.Now.Year} {match.Groups[2].Value}");

                Match match2 = Regex.Match(EventContents, @"<link rel=""alternate"" type=""application/rss\+xml"" title=""(.*?)""");
                if (match2.Success)
                {
                    EventTitle = match2.Groups[1].Value;
                }
                else
                {
                    EventTitle = "Error! Couldn't determine title!";
                }

                EventList.Add(new EventInformation() { Title = EventTitle, StartDate = StartTime, Location = $"{match.Groups[3].Value} ({match.Groups[4].Value})", Description = EventUrl });

                return true;
            }

            return false;
        }
    }
}
