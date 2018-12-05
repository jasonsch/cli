using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using Mono.Options;

namespace gcal
{
    class Program
    {
        // TODO -- Handle events that have more than three dates (e.g., https://www.facebook.com/events/262577911273702/)
        // TODO -- Add high level notes about what this program does.
        // TODO -- Need to handle FB events that aren't public (OAuth?)
        //
        // TODO -- Couldn't read multiple event page like: https://www.facebook.com/events/323430185071606/?event_time_id=323430201738271
        //
        // C:\Users\jasonsch\code\WindowsCLI\gcal\bin\Release>gcal -c Potential -f https://www.facebook.com/events/184707418897024/
        // ERROR: Couldn't find start/end time!
        //
        // C:\Users\jasonsch\code\WindowsCLI\gcal\bin\Release>gcal -c Potential -f https://www.facebook.com/events/1227594960711361/permalink/1227595037378020/
        // ERROR: Couldn't find start/end time!
        //
        // C:\Users\jasonsch\code\WindowsCLI\gcal\bin\Release>gcal -c Em-tertainment -f https://www.facebook.com/events/1690972410964409/
        // WARNING: Couldn't find location info!
        //
        // C:\Users\jasonsch\code\WindowsCLI\gcal\bin\Release>gcal -c Potential -f https://www.facebook.com/events/392444487934008/
        // WARNING: Couldn't find location info!
        // Event 'South Lake Union Block Party 2018' on '8/10/2018 12:00PM' successfully created at https://www.google.com/calendar/event?eid=bDd1Zmc4ZWI4ZXBhOHZwZWVlY2F1MGsybWsgN2tlb3Y5NGtqc3VpZXIxaDZiYmY5ZmlhZGtAZw
        //
        // C:\Users\jasonsch\code\WindowsCLI\gcal\bin\Release>gcal -f https://www.facebook.com/events/187623255199018/
        // WARNING: Couldn't find location info!
        // Event 'Collegiate Shag Workshop w/Kendall and Ronnie Roderick!' on '6/17/2018 12:00PM' successfully created
        //
        // TODO -- Add a '--gui' flag that brings up a calendar that one can click on the individual days that an event covers (good for something like https://www.facebook.com/events/234056634022933/)
        // TODO -- Support all-day events.
        // TODO -- Support recurrence rules (https://tools.ietf.org/html/rfc5545#section-3.8.5) ( newEvent.Recurrence = new String[] { "RRULE:FREQ=DAILY;COUNT=2" };)
        //
        // TODO -- Cleanup interface to different parsers (FB / TicketWeb / ...)
        // C:\Users\jasonsch\code\WindowsCLI\gcal\bin\Release>gcal -c Em-tertainment -f https://www.facebook.com/events/1934943356769465/
        // WARNING: Couldn't find location info!
        // WARNING: Couldn't find location info!
        // WARNING: Couldn't find location info!
        // https://developers.google.com/calendar/v3/reference/#resource-types
        //
        // TODO: Non-ascii characters:
        // C:\Users\jasonsch\code\WindowsCLI\gcal\bin\Release>gcal -c Em-tertainment -f https://www.facebook.com/events/1004070966420330/
        // Event 'a?Sæo^èÎåÎåÎá< Moon Viewing 2018' on '8/25/2018 6:00PM' successfully created at https://www.google.com/calendar/event?eid=OXRtbzFlbjgzZjdidWZrdTRkODI5N2hlNG8gNmdyNXJ0MTFuYWlzZHZobDhiYnR0aDFpaTBAZw
        //
        // TODO -- Coalesce multi-day events like https://www.facebook.com/events/626414344369451/?event_time_id=626414347702784 (maybe prompt the user?)
        //
        private static readonly string[] Scopes = { CalendarService.Scope.Calendar };
        private static readonly string ApplicationName = "gcal";

        static void PrintUsage(string ErrorMessage = null)
        {
            if (!String.IsNullOrEmpty(ErrorMessage))
            {
                Console.WriteLine("An error occurred: {0}", ErrorMessage);
            }

            Console.WriteLine("Usage:");
            Console.WriteLine("gcal ");
            Console.WriteLine("-a, --all-day\tThis event lasts all day");
            Console.WriteLine("-c, --calendar <calendar name>\tSpecifies the calendar for this event (defaults to users's primary calendar)");
            Console.WriteLine("-d, --description <event description>\tThe description for the calendar event");
            Console.WriteLine("-e, --end <date>\tThe end date of the entry");
            Console.WriteLine("-f, --facebook <URL>\tA facebook event URL that is parsed for the event info");
            Console.WriteLine("-m, --ticketweb <URL>\tAn EventBrite or TicketWeb URL");
            Console.WriteLine("-n, --notification <'email|popup==<time period>'>\tThe type of reminder notification and when to show it.");
            Console.WriteLine("-r, --recurrence <rule>\tA recurrence rule for this event (e.g., 'RRULE:FREQ=DAILY;COUNT=2'). Can be specified multiple times.");
            Console.WriteLine("-s, --start <date>\tThe start date of the entry");
            Console.WriteLine("-t, --title <title>\tThe title for the calendar event");
            Console.WriteLine("-w, --where <location>\tThe location for the calendar event");
            Console.WriteLine("-?, -h, --help\tDisplays this usage");

            Environment.Exit(0);
        }

        static void ShowCalendar(int Month, int Year)
        {
            int FirstOfMonthOffset = 0; // How far "into" the month (really, week) the first day of the month is.

            Console.WriteLine("    {0} {1}", CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Month), Year);
            Console.WriteLine("Su Mo Tu We Th Fr Sa"); // TODO -- Localize?

            //
            // Move over to the right place to start printing the first day of the month.
            //
            FirstOfMonthOffset = (int)(new DateTime(Year, Month, 1)).DayOfWeek;
            for (int i = 0; i < FirstOfMonthOffset; ++i)
            {
                Console.Write("   ");
            }

            for (int i = 1; i <= DateTime.DaysInMonth(Year, Month);)
            {
                Console.Write("{0,2} ", i);
                if ((++i + FirstOfMonthOffset) % 7 == 1)
                {
                    Console.WriteLine("");
                }
            }
        }

        // TODO -- Fix output to be more compact.
        static void ShowCalendar(int Year)
        {
            for (int i = 0; i < 12; ++i)
            {
                ShowCalendar(i + 1, Year);
                Console.WriteLine("");
            }
        }

        static void ShowCalendar()
        {
            ShowCalendar(DateTime.Now.Month, DateTime.Now.Year);
        }

        static bool ValidateDateString(string DateToValidate)
        {
            try
            {
                DateTime.Parse(DateToValidate);
            }
            catch (FormatException)
            {
                return false;
            }

            return true;
        }

        //
        // Returns the calendar's ID given its name.
        //
        static string FindCalendarByName(CalendarService service, string CalendarName)
        {
            var CalListRequest = service.CalendarList.List();
            CalListRequest.ShowDeleted = false;
            var CalList = CalListRequest.Execute();
            foreach (var Cal in CalList.Items)
            {
                if (Cal.Summary == CalendarName)
                {
                    return Cal.Id;
                }
            }

            return null;
        }

        [STAThread]
        static void Main(string[] args)
        {
            UserCredential credential;
            OptionSet options = new OptionSet();
            string CalendarID = "primary";
            EventInformation EventInfo = new EventInformation();
            string FacebookURL = null;
            string TicketWebUrl = null;
            bool eventAllDay = false;
            List<string> NakedParameters = null;
            bool FlagsPassed = false; // If we don't see any arguments then we act like the classic "cal" command

            // TODO -- https://www.twilio.com/blog/2018/05/user-secrets-in-a-net-core-console-app.html
            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read)) // TODO -- Make this a resource
            {
                string credPath = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/calendar-dotnet-quickstart.json"); // TODO

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            // Create Google Calendar API service.
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // TODO -- Check for required arguments
            // TODO -- Do some sanity checking (end >= start, end not specified if all-day, ...)
            // TODO -- A date string like "2018-01-28" will get round-tripped (through DateTime.Parse()) as having an explicit time of 12AM
            options.Add("?|h|help", value => { PrintUsage(); });
            options.Add("a|all-day", value => { eventAllDay = true; FlagsPassed = true; });
            options.Add("c|calendar=", value => { CalendarID = FindCalendarByName(service, value); if (CalendarID == null) { PrintUsage("Couldn't find specified calendar!"); } FlagsPassed = true; });
            options.Add("d|description=", value => { EventInfo.Description = value; FlagsPassed = true; });
            options.Add("e|end=", value => { EventInfo.SetEndDate(value); FlagsPassed = true; });
            options.Add("f|facebook=", value => { FacebookURL = value; FlagsPassed = true; });
            options.Add("n|notification=", value => { EventInfo.SetReminderNotification(value); });
            options.Add("r|recurrence=", value => { EventInfo.AddRecurrenceRule(value); });
            options.Add("s|start=", value => { EventInfo.SetStartDate(value); FlagsPassed = true; });
            options.Add("t|title=", value => { EventInfo.Title = value; FlagsPassed = true; });
            options.Add("m|ticketweb=", value => { TicketWebUrl = value; FlagsPassed = true; });
            options.Add("w|where=", value => { EventInfo.Location = value; FlagsPassed = true; });

            NakedParameters = options.Parse(args);

            if (FlagsPassed)
            {
                if (!String.IsNullOrEmpty(FacebookURL))
                {
                    HandleFacebookEvent(service, CalendarID, FacebookURL);
                }
                else if (!String.IsNullOrEmpty(TicketWebUrl))
                {
                    HandleTicketwebEvent(service, CalendarID, TicketWebUrl);
                }
                else
                {
                    //
                    // The event start and title are required, everything else is optional.
                    //
                    if (EventInfo.StartDate == null || String.IsNullOrEmpty(EventInfo.Title))
                    {
                        PrintUsage();
                    }

                    AddCalendarEvent(service, CalendarID, EventInfo, eventAllDay);
                }
            }
            else
            {
                if (NakedParameters.Count == 0)
                {
                    ShowCalendar();
                }
                else if (NakedParameters.Count == 1)
                {
                    ShowCalendar(Int32.Parse(NakedParameters[0]));
                }
                else if (NakedParameters.Count == 2)
                {
                    ShowCalendar(Int32.Parse(NakedParameters[0]), Int32.Parse(NakedParameters[1]));
                }
                else
                {
                    PrintUsage();
                }
            }
        }

        private static bool HandleTicketwebEvent(CalendarService service, string CalendarID, string Url)
        {
            List<EventInformation> Events = new List<EventInformation>();

            if (!TicketWebFilter.ParseUrl(Url, Events))
            {
                return false;
            }

            foreach (var Event in Events)
            {
                AddCalendarEvent(service, CalendarID, Event, false);
            }

            return true;
        }

        private static bool HandleFacebookEvent(CalendarService service, string CalendarID, string FacebookURL)
        {
            List<EventInformation> Events = new List<EventInformation>();

            if (!FacebookFilter.ParseFacebookEvent(FacebookURL, Events))
            {
                return false;
            }

            foreach (var Event in Events)
            {
                AddCalendarEvent(service, CalendarID, Event, false);
            }

            return true;
        }

        public static void AddCalendarEvent(CalendarService service, string CalendarID, EventInformation EventInfo, bool eventAllDay)
        {
            Event NewEvent = new Event()
            {
                Summary = EventInfo.Title,
                Location = EventInfo.Location,
                Description = EventInfo.Description,
                Start = new EventDateTime() { DateTime = EventInfo.StartDate, TimeZone = "America/Los_Angeles" }, // TODO -- Get right timezone
                End = new EventDateTime() { DateTime = EventInfo.EndDate, TimeZone = "America/Los_Angeles" }, // TODO -- Get right timezone
            };

            if (EventInfo.RecurrenceRules != null)
            {
                NewEvent.Recurrence = EventInfo.RecurrenceRules;
            }

            if (EventInfo.Reminders != null)
            {
                Event.RemindersData data = new Event.RemindersData();

                data.UseDefault = false;
                data.Overrides = EventInfo.Reminders;
                NewEvent.Reminders = data;
            }

            var request = service.Events.Insert(NewEvent, CalendarID);
            var e = request.Execute();

            Console.WriteLine("Event '{0}' on '{1:M/d/yyyy h:mmtt}' successfully created at {2}", EventInfo.Title, EventInfo.StartDate, e.HtmlLink);
        }

        // TODO -- Not currently used.
        public static void ListCalendarEvents(CalendarService service)
        {
            EventsResource.ListRequest request = service.Events.List("primary");
            request.TimeMin = DateTime.Now;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 10;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            Events events = request.Execute();
            Console.WriteLine("Upcoming events:");
            if (events.Items != null && events.Items.Count > 0)
            {
                foreach (var eventItem in events.Items)
                {
                    string when = eventItem.Start.DateTime.ToString();
                    if (String.IsNullOrEmpty(when))
                    {
                        when = eventItem.Start.Date;
                    }
                    Console.WriteLine("{0} ({1})", eventItem.Summary, when);
                }
            }
            else
            {
                Console.WriteLine("No upcoming events found.");
            }
        }
    }
}
