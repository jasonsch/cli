using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using gcal.Interfaces;
using gcal.Models;

namespace gcal
{
    public class MetaFilterHandler : IUrlEventParser
    {
        private readonly IUrlDownload PageDownloader;

        public MetaFilterHandler(IUrlDownload PageDownloader)
        {
            this.PageDownloader = PageDownloader;
        }

        private static bool IsMetafilterUrl(string Url)
        {
            return Regex.IsMatch(Url, "^https://irl.metafilter.com/");
        }

        public bool ParseEvent(string EventURL, List<EventInformation> EventList)
        {
            string EventContents;
            Match match;

            EventContents = PageDownloader.GetPageContents(EventURL);

            match = Regex.Match(EventContents, @"<div id=""maplocation"" style=""line-height:100%;"">(.*?) at (.*?), (.*?)<div id=""address"" style=""font-size:12px;margin-top:3px;"">(.*?)\(");
            // match = Regex.Match(EventContents, @"<script type=""application/ld\+json"">(.*?)</script>", RegexOptions.Singleline);
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

                EventList.Add(new EventInformation() { Title = EventTitle, StartDate = StartTime, Location = $"{match.Groups[3].Value} ({match.Groups[4].Value})", Description = EventURL });

                return true;
            }

            return false;
        }
    }
}
