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
using gcal.Models;

namespace gcal
{
    class Program
    {
        // TODO -- OCR images? E.g., https://www.facebook.com/PonoRanch/photos/a.500038980030584/2591427614225033/?type=3&theater
        // TODO -- Handle events that have more than three dates (e.g., https://www.facebook.com/events/262577911273702/ or https://www.facebook.com/events/2075275352740171/?event_time_id=2075275389406834)
        // TODO -- Add high level notes about what this program does.
        // TODO -- Need to handle FB events that aren't public (OAuth?)
        //      -- https://www.facebook.com/events/163743240932607/
        // TODO -- Support metafilter IRL events: https://irl.metafilter.com/4074/The-needle-moved-trivially
        // TODO -- Couldn't read multiple event page like: https://www.facebook.com/events/323430185071606/?event_time_id=323430201738271
        // TODO -- use a recurring entry for FB events with multiple dates/times (or some other way to link them so they can all be [e.g.] deleted together) or if they're
        //         on adjacent days make one entry (e.g., https://www.facebook.com/events/783809855296368/).
        // TODO -- We're not properly reading multiple entries for https://www.facebook.com/events/380233632769259/?event_time_id=380233659435923 (we just see the first one).
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
        private static readonly URLDownloader Downloader = new URLDownloader();
        private static readonly UrlParserManager UrlParsers = new UrlParserManager(Downloader);

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
            Console.WriteLine("-n, --notification <'email|popup=<time period>'>\tThe type of reminder notification and when to show it.");
            Console.WriteLine("-r, --recurrence <rule>\tA recurrence rule for this event (e.g., 'RRULE:FREQ=DAILY;COUNT=2'). Can be specified multiple times.");
            Console.WriteLine("-s, --start <date>\tThe start date of the entry");
            Console.WriteLine("-t, --title <title>\tThe title for the calendar event");
            Console.WriteLine("-u, --url<URL>\tAn EventBrite, TicketWeb, or Metafilter IRL URL");
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
            string EventUrl = null;
            bool ParseOnly = false; // If this is true we parse the event URL and display its info but don't add it to the calendar.
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
            options.Add("c|calendar=", value => { CalendarID = FindCalendarByName(service, value); if (CalendarID == null) { PrintUsage("Couldn't find specified calendar!"); } FlagsPassed = true; });
            options.Add("d|description=", value => { EventInfo.Description = value; FlagsPassed = true; });
            options.Add("e|end=", value => { SetEndingDate(EventInfo, value); FlagsPassed = true; });
            options.Add("n|notification=", value => { EventInfo.SetReminderNotification(value); FlagsPassed = true; });
            options.Add("p|parse-only", value => { ParseOnly = true; FlagsPassed = true; });
            options.Add("r|recurrence=", value => { EventInfo.AddRecurrenceRule(value); });
            options.Add("s|start=", value => { EventInfo.SetStartDate(value); FlagsPassed = true; });
            options.Add("t|title=", value => { EventInfo.Title = value; FlagsPassed = true; });
            options.Add("u|url=", value => { EventUrl = value; FlagsPassed = true; });
            options.Add("w|where=", value => { EventInfo.Location = value; FlagsPassed = true; });

            NakedParameters = options.Parse(args);

            if (FlagsPassed)
            {
                if (!String.IsNullOrEmpty(EventUrl))
                {
                    HandleEventURL(service, CalendarID, EventUrl, ParseOnly);
                }
                else
                {
                    //
                    // The event start and title are required, everything else is optional.
                    //
                    if (EventInfo.StartDate == null)
                    {
                        PrintUsage("Start date is required!");
                    }
                    else if (String.IsNullOrEmpty(EventInfo.Title))
                    {
                        PrintUsage("A title is required!");
                    }

                    AddCalendarEvent(service, CalendarID, EventInfo);
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

        //
        // The endDate can be absolute or just the time.
        //
        private static void SetEndingDate(EventInformation eventInfo, string endDateString)
        {
            DateTime endDate;

            if (DateTime.TryParseExact(endDateString, new string[] { "HH:mm", "HH:mm:ss", "hh:mm", "hh:mm:ss", "hh" }, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out endDate))
            {
                if (eventInfo.StartDate == default(DateTime))
                {
                    PrintUsage("A relative end time must be specified after the start time on the command line!");
                }

                eventInfo.EndDate = new DateTime(eventInfo.StartDate.Date.Year, eventInfo.StartDate.Date.Month, eventInfo.StartDate.Date.Day, endDate.Hour, endDate.Minute, endDate.Second);
            }
            else
            {
                if (!DateTime.TryParse(endDateString, out endDate))
                {
                    PrintUsage($"Couldn't parse end date {endDateString}!");
                }

                eventInfo.EndDate = endDate;
            }
        }

        private static bool CompareEventsDates(Event eventItem, EventInformation eventInfo)
        {
            if (eventItem.Start.Date == null)
            {
                return (eventItem.Start.DateTime == eventInfo.StartDate);
            }
            else
            {
                //
                // eventItem is an all day event.
                //
                return (eventInfo.AllDay && DateTime.Parse(eventItem.Start.Date) == eventInfo.StartDate.Date);
            }
        }

        private static bool FindCalendarEvent(CalendarService service, string calendarID, EventInformation eventInfo)
        {
            EventsResource.ListRequest request = service.Events.List(calendarID);
            request.TimeMin = eventInfo.StartDate - TimeSpan.FromMinutes(1);
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 10;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            Events events = request.Execute();
            foreach (var eventItem in events.Items)
            {
                if (CompareEventsDates(eventItem, eventInfo))
                {
                    if (eventItem.Summary == eventInfo.Title)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool HandleEventURL(CalendarService service, string CalendarID, string Url, bool ParseOnly)
        {
            List<EventInformation> Events = new List<EventInformation>();

            if (!UrlParsers.HandleUrl(Url, Events))
            {
                return false;
            }

            foreach (var Event in Events)
            {
                if (ParseOnly)
                {
                    Console.WriteLine($"Event '{Event.Title}' starting at {Event.StartTime} and ending at {Event.EndDate} was successfully parsed.", Event.Title, Event.StartDate, Event.EndDate);
                    if (!string.IsNullOrEmpty(Event.Location))
                    {
                        Console.WriteLine($"Location: {Event.Location}");
                    }

                    if (!string.IsNullOrEmpty(Event.Description))
                    {
                        Console.WriteLine($"Description: {Event.Description}");
                    }
                }
                else
                {
                    AddCalendarEvent(service, CalendarID, Event);
                }
            }

            return true;
        }

        public static void AddCalendarEvent(CalendarService service, string CalendarID, EventInformation EventInfo)
        {
            if (!FindCalendarEvent(service, CalendarID, EventInfo))
            {
                InsertCalendarEvent(service, CalendarID, EventInfo);
            }
            else
            {
                Console.WriteLine("Found an existing event that matches so ignoring add.");
            }
        }

        private static void InsertCalendarEvent(CalendarService service, string CalendarID, EventInformation EventInfo)
        {
            Event NewEvent = new Event()
            {
                Summary = EventInfo.Title,
                Location = EventInfo.Location,
                Description = EventInfo.Description,
                Start = EventInfo.EventStart,
                End = EventInfo.EventEnd
            };

            if (EventInfo.RecurrenceRules != null)
            {
                NewEvent.Recurrence = EventInfo.RecurrenceRules;
            }

            if (EventInfo.Reminders != null)
            {
                NewEvent.Reminders = new Event.RemindersData() { UseDefault = false, Overrides = EventInfo.Reminders };
            }

            var request = service.Events.Insert(NewEvent, CalendarID);
            var e = request.Execute();

            Console.WriteLine($"Event '{EventInfo.Title}' on '{EventInfo.StartTime}' successfully created at '{e.HtmlLink}'");
        }
    }
}
