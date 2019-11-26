using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using Google.Apis.Calendar.v3.Data;

namespace gcal.Models
{
    public class EventInformation
    {
        private static readonly TimeSpan defaultEventLength = TimeSpan.FromMinutes(60);

        public override string ToString()
        {
            return $"Title ==> {Title}, Location ==> {Location}, Desc ==> {Description}";
        }

        private DateTime[] startDates;
        private DateTime[] endDates;

        public bool SetStartDate(string date)
        {
            startDates = YellowLab.FuzzyDateParser.Parse(date);
            if (startDates == null)
            {
                return false;
            }

            /*
            if (dates.Length > 1)
            {
                //
                // TODO -- Right now the only recurrence that FuzzyDateParser understands is 
                // weekly so we hard-code this.
                //
                _RecurrenceRules.Add($"RRULE:FREQ=WEEKLY;COUNT={dates.Length}");
            }
            */

            return true;
        }

        public bool SetEndDate(string date)
        {
            //
            // TODO -- We want the user to be able to specify an amount of time for the event (e.g., "one hour").
            // We can do that if they already gave us a start time but if they haven't then we're screwed (we could
            // save the value and process it when we get a start time but that seems like a lot of work).
            //
            endDates = YellowLab.FuzzyDateParser.Parse(date, startDates != null ? startDates[0] : DateTime.Now);
            return (endDates != null);
        }

        /// <summary>
        /// Notification will look like "(popup|email)=(time period)". E.g.,
        /// "email=2 days" or "popup=3 hours"
        /// </summary>
        /// <param name="Notification"></param>
        public void AddReminder(string Notification)
        {
            Match match;

            match = Regex.Match(Notification, @"^(email|popup)=(.*)$");
            if (match.Success)
            {
                DateTime now = DateTime.Now;
                DateTime future = YellowLab.FuzzyDateParser.Parse(match.Groups[2].Value, now)[0];
                TimeSpan Span = future - now;

                Reminders.Add(new EventReminder() { Method = match.Groups[1].Value, Minutes = (int)Span.TotalMinutes });
            }
            else
            {
                Console.WriteLine("Couldn't parse reminder {0}", Notification);
            }
        }

        #region Properties
        public List<EventReminder> Reminders { get; private set; } = new List<EventReminder>();
        public List<string> RecurrenceRules { get; private set; } = new List<string>();

        private string _Title;
        public string Title
        {
            get { return _Title; }
            set
            {
                _Title = WebUtility.HtmlDecode(value);
            }
        }

        private string _Description;
        public string Description
        {
            get { return _Description; }
            set
            {
                _Description = WebUtility.HtmlDecode(value);
            }
        }

        private string _Location;
        public string Location
        {
            get
            {
                return _Location;
            }

            set
            {
                _Location = WebUtility.HtmlDecode(value);
            }
        }

        /// <summary>
        /// Returns a Tuple of start and end times for all the events (there can be more than one if it's a recurring event).
        /// </summary>
        /// <returns></returns>
        public List<Tuple<EventDateTime, EventDateTime>> GetEventTimes()
        {
            List<Tuple<EventDateTime, EventDateTime>> events = new List<Tuple<EventDateTime, EventDateTime>>();
            int i = 0;

            foreach (DateTime startDate in startDates)
            {
                DateTime endDate;

                if (endDates == null)
                {
                    if (IsAllDayEvent(startDate))
                    {
                        endDate = startDate;
                    }
                    else
                    {
                        // If we don't have an explicit end date then we assume one hour.
                        endDate = startDate + defaultEventLength;
                    }
                }
                else
                {
                    endDate = endDates[i];
                }

                events.Add(new Tuple<EventDateTime, EventDateTime>(EventTimeFromDate(startDate), EventTimeFromDate(endDate)));

                ++i;
            }

            return events;
        }

        private static EventDateTime EventTimeFromDate(DateTime date)
        {
            if (IsAllDayEvent(date))
            {
                return new EventDateTime() { Date = date.ToString("yyyy-MM-dd") };
            }
            else
            {
                return new EventDateTime() { DateTime = date, TimeZone = "America/Los_Angeles" }; // TODO
            }
        }

        private static bool IsAllDayEvent(DateTime date)
        {
            //
            // TODO -- This is bogus but will mostly work as few events will start at midnight.
            //
            return (date.Hour == 0);
        }

        #endregion
    }
}