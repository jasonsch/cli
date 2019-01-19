using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using gcal.Interfaces;
using gcal.Models;

namespace gcal
{
    //
    // TODO -- Use API (https://developers.facebook.com/docs/graph-api/reference/event/#Overview)
    //
    class FacebookHandler : IUrlEventParser
    {
        private readonly IUrlDownload PageDownloader;

        public FacebookHandler(IUrlDownload PageDownloader)
        {
            this.PageDownloader = PageDownloader;
        }

        /*
         * If we try to retrieve an event that isn't public Facebook won't give us an auth-needed status code but will
         * just return a page indicating to the user that they must login. As such, we sniff this out and propagate that
         * info back to the caller.
         */
        private static bool LoginRequired(string Contents)
        {
            if (Contents.Contains("<div class=\"_585r _50f4\">You must log in to continue."))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool ParseEvent(string EventURL, List<EventInformation> EventList)
        {
            string EventContents;
            EventInformation EventInfo;
            Match match;

            EventContents = PageDownloader.GetPageContents(EventURL);
            if (LoginRequired(EventContents))
            {
                Console.WriteLine("ERROR: {0} is a non-public event and requires login.", EventURL);
                return false;
            }

            //
            // This is true if this is a multi-event event.
            //
            match = Regex.Match(EventContents, @"<a href=""/events/(\d+)/\?event_time_id=(\d+)");
            if (match.Success)
            {
                do
                {
                    string url;

                    url = String.Format("https://www.facebook.com/events/{0}/?event_time_id={1}", match.Groups[1].Value, match.Groups[2].Value);
                    if (!ParseEvent(url, EventList))
                    {
                        Console.WriteLine("Couldn't parse FB sub-event {0}", url);
                        return false;
                    }

                    match = match.NextMatch();
                } while (match.Success);
            }
            else
            {
                if (ParseFacebookEventListing(EventURL, EventContents, out EventInfo))
                {
                    EventList.Add(EventInfo);
                }
            }

            return true;
        }

        private static bool ParseFacebookEventListing(string URL, string EventContents, out EventInformation EventInfo)
        {
            string StartDate;
            string EndDate;

            EventInfo = new EventInformation();
            EventInfo.Location = GetEventLocation(EventContents);

            EventInfo.Title = GetEventTitle(EventContents);
            if (string.IsNullOrEmpty(EventInfo.Title))
            {
                return false;
            }

            EventInfo.Description = "Source: " + URL + "\n";

            if (!GetEventDates(EventContents, out StartDate, out EndDate))
            {
                return false;
            }

            EventInfo.SetStartDate(StartDate);
            if (!string.IsNullOrEmpty(EndDate))
            {
                EventInfo.SetEndDate(EndDate);
            }

            return true;
        }

        private static bool GetEventDates(string EventContents, out string EventStartDate, out string EventEndDate)
        {
            Match match;

            EventStartDate = EventEndDate = null;

            match = Regex.Match(EventContents, "<div class=\"_publicProdFeedInfo__timeRowTitle _5xhk\" content=\"([^ ]+) to ([^\"]+)\">");
            if (match.Success)
            {
                EventStartDate = match.Groups[1].Value;
                EventEndDate = match.Groups[2].Value;
                return true;
            }

            match = Regex.Match(EventContents, "<div class=\"_2ycp _5xhk\" content=\"(.*) to ([^\"]+)\">");
            if (match.Success)
            {
                EventStartDate = match.Groups[1].Value;
                EventEndDate = match.Groups[2].Value;

                return true;
            }

            match = Regex.Match(EventContents, "<div class=\"_2ycp _5xhk\" content=\"([^\"]+)\">");
            if (match.Success)
            {
                EventStartDate = match.Groups[1].Value;

                return true;
            }


            match = Regex.Match(EventContents, "<span itemprop=\"startDate\">([^<]+)</span></span> at <span>([^<]+)</span> - <span>([^<]+)</span>");
            if (match.Success)
            {
                EventStartDate = string.Format("{0} {1}", match.Groups[1].Value, match.Groups[2].Value);
                EventEndDate = string.Format("{0} {2}", match.Groups[1].Value, match.Groups[3].Value);
                return true;
            }
            match = Regex.Match(EventContents, "<div class=\"_publicProdFeedInfo__timeRowTitle _5xhk\" content=\"([^\"]+)\">");
            if (match.Success)
            {
                EventStartDate = match.Groups[1].Value;
                return true;
            }

            Console.WriteLine("ERROR: Couldn't find start/end time!");
            return false;
        }

        private static string GetEventTitle(string EventContents)
        {
            Match match;
            string EventTitle;

            match = Regex.Match(EventContents, "<title id=\"pageTitle\">([^<]+)</title>");
            if (match.Success)
            {
                EventTitle = match.Groups[1].Value;
            }
            else
            {
                Console.WriteLine("ERROR: Couldn't find title!");
                EventTitle = null;
            }

            return EventTitle;
        }

        private static string GetEventLocation(string EventContents)
        {
            Match match = Regex.Match(EventContents, "<title id=\"pageTitle\">([^<]+)</title>");
            string EventLocation;

            match = Regex.Match(EventContents, "<div class=\"_5xhp fsm fwn fcg\">([^<]+)</div>");
            if (match.Success)
            {
                EventLocation = match.Groups[1].Value;
            }
            else
            {
                match = Regex.Match(EventContents, "<span class=\"_5xhk\" id=\"u_0_p\">([^<]+)</span>");
                if (match.Success)
                {
                    EventLocation = match.Groups[1].Value;
                }
                else
                {
                    Console.WriteLine("WARNING: Couldn't find location info!");
                    EventLocation = null;
                }
            }

            return EventLocation;
        }
    }
}
