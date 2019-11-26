using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using gcal.Interfaces;
using gcal.Models;

namespace gcal
{
    /// <summary>
    /// Handles URLs from www.axs.com.
    /// </summary>
    class AxsHandler : IUrlEventParser
    {
        private readonly IUrlDownload PageDownloader;

        public AxsHandler(IUrlDownload PageDownloader)
        {
            this.PageDownloader = PageDownloader;
        }

        private static bool IsAxsUrl(string Url)
        {
            return Regex.IsMatch(Url, "^https://www.axs.com/");
        }

        public bool ParseEvent(string EventURL, List<EventInformation> EventList)
        {
            string EventContents;

            if (!IsAxsUrl(EventURL))
            {
                return false;
            }

            EventContents = PageDownloader.GetPageContents(EventURL);

            EventInformation EventInfo = new EventInformation();
            EventInfo.SetStartDate(GetStartDate(EventContents));
            EventInfo.Title = GetTitle(EventContents);
            EventInfo.Description = EventURL;
            EventInfo.Location = GetLocation(EventContents);

            return true;
        }

        private static string GetStartDate(string EventContents)
        {
            Match match;

            match = Regex.Match(EventContents, @"<meta itemprop=""startDate"" content=""(.*)"">");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                throw new InvalidProgramException($"Couldn't parse start time for event: {EventContents}!");
            }
        }

        private static string GetTitle(string EventContents)
        {
            Match match;

            match = Regex.Match(EventContents, @"<meta itemprop=""name"" content=""(.*)"">");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return null;
            }
        }

        private static string GetLocation(string EventContents)
        {
            Match match;

            match = Regex.Match(EventContents, @"<span itemprop=""location"" itemscope itemtype=""http://schema.org/EventVenue"">\s*\n\s*<meta itemprop=""name"" content=""(.*)"">");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return "";
            }
        }
    }
}
