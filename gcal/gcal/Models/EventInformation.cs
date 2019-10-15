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
                    AddRecurrenceRule($"RRULE:FREQ=WEEKLY;COUNT={dates.Length}");
                }
            }
        }

        public void SetEndDate(string Date)
        {
            EndDate = DateTime.Parse(Date);
        }

        public void AddRecurrenceRule(string Rule)
        {
            _RecurrenceRules.Add(Rule);
        }

        /// <summary>
        /// Notification will look like "(popup|email)=(time period)". E.g.,
        /// "email=2 days" or "popup=3 hours"
        /// </summary>
        /// <param name="Notification"></param>
        public void SetReminderNotification(string Notification)
        {
            Match match;

            match = Regex.Match(Notification, @"^(email|popup)=(.*)$");
            if (match.Success)
            {
                TimeSpan Span = FuzzyDateParser.ParseTime(match.Groups[2].Value);

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
                    return StartDate.Date.ToString("M/d/yyyy");
                }
                else
                {
                    return StartDate.ToString("M/d/yyyy h:mmtt");
                }
            }
        }


        public DateTime EndDate { get; set; }
        public bool AllDay { get; set; }

        #endregion
    }
}