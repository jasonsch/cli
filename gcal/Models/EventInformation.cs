using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using Google.Apis.Calendar.v3.Data;

namespace gcal.Models
{
    public class EventInformation
    {
        public override string ToString()
        {
            return String.Format("Title ==> {0}, StartDate ==> {1}, EndDate ==> {2}, Location ==> {3}, Desc ==> {4}", Title, StartDate, EndDate, String.IsNullOrEmpty(Location) ? "" : Location, String.IsNullOrEmpty(Description) ? "" : Description);
        }

        public void SetStartDate(string Date)
        {
            //
            // If we can't successfully parse the starting date we'll assume the 
            //
            if (!DateTime.TryParse(Date, out _StartDate))
            {
                DateTime[] dates = YellowLab.FuzzyDateParser.Parse(Date);
                if (dates == null)
                {
                    throw new ArgumentException($"Couldn't parse date {Date}!"); // TODO
                }

                StartDate = dates[0];
                if (dates.Length > 1)
                {
                    //
                    // TODO -- Right now the only recurrence that FuzzyDateParser understands is 
                    // weekly so we hard-code this.
                    //
                    _RecurrenceRules.Add($"RRULE:FREQ=WEEKLY;COUNT={dates.Length}");
                }
            }
        }

        public void SetEndDate(string Date)
        {
            EndDate = DateTime.Parse(Date);
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

                _Reminders.Add(new EventReminder() { Method = match.Groups[1].Value, Minutes = (int)Span.TotalMinutes });
            }
            else
            {
                Console.WriteLine("Couldn't parse reminder {0}", Notification);
            }
        }

        #region Properties
        private readonly List<EventReminder> _Reminders = new List<EventReminder>();
        public EventReminder[] Reminders
        {
            get { return _Reminders.ToArray(); }
        }

        private readonly List<string> _RecurrenceRules = new List<string>();
        public string[] RecurrenceRules
        {
            get { return _RecurrenceRules.ToArray(); }
        }

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

        private DateTime _StartDate;
        public DateTime StartDate
        {
            get { return _StartDate; }
            set
            {
                _StartDate = value;
                // We assume events last one hour unless otherwise specified.
                EndDate = value.AddHours(1.0);
            }
        }

        public string StartTime
        {
            get
            {
                if (AllDay)
                {
                    //
                    // TODO -- This is the formatting that google wants but we shouldn't show this to the user.
                    //
                    return StartDate.Date.ToString("yyyy-MM-dd");
                }
                else
                {
                    return StartDate.ToString("yyyy-MM-dd h:mmtt");
                }
            }
        }

        public EventDateTime EventStart
        {
            get
            {
                // TODO -- We check AllDay once here and then again in StartTime
                if (AllDay)
                {
                    return new EventDateTime() { Date = StartTime };
                }
                else
                {
                    return new EventDateTime() { DateTime = StartDate, TimeZone = "America/Los_Angeles" }; // TODO -- Get right timezone
                }
            }
        }

        public EventDateTime EventEnd
        {
            get
            {
                // TODO -- We check AllDay once here and then again in StartTime
                if (AllDay)
                {
                    return new EventDateTime() { Date = StartTime };
                }
                else
                {
                    return new EventDateTime() { DateTime = EndDate, TimeZone = "America/Los_Angeles" }; // TODO -- Get right timezone
                }
            }
        }



        public DateTime EndDate { get; set; }

        public bool AllDay
        {
            get
            {
                //
                // TODO -- This is bogus but will mostly work as few events will start at midnight.
                //
                return (StartDate.Hour == 0);
            }
        }

        #endregion
    }
}