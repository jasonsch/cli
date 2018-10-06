using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Net;
using Newtonsoft.Json;

namespace gcal
{
    public class TicketWebFilter
    {
        private class EventDataPhysicalLocation
        {
            public string streetAddress;
            public string addressLocality;
            public string addressRegion;
            public string postalCode;
            public string addressCountry;
        }

        private class EventDataLocation
        {
            public string name;
            public string sameAs; // URL
            public EventDataPhysicalLocation address;
        }

        private class EventDataJson
        {
            public string name;
            public string startDate;
            public EventDataLocation location;
        }

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

            match = Regex.Match(EventContents, @"<script type=""application/ld\+json"">(.*?)</script>", RegexOptions.Singleline);
            if (match.Success)
            {
                // For some reason the match has the <script> / </script> tags in it so we strip those out.
                string json = match.Groups[0].Value.Substring(35);
                json = json.Remove(json.Length - 10);

                EventDataJson EventInfo = JsonConvert.DeserializeObject<EventDataJson>(json);
                EventList.Add(new EventInformation() { Title = EventInfo.name, StartDate = DateTime.Parse(EventInfo.startDate), Location = EventInfo.location.name, Description = EventUrl });

                return true;
            }

            Console.WriteLine("ERROR: Couldn't parse event at {0}", EventUrl);
            return false;
        }
    }
}
