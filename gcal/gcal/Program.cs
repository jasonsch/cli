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
        /*
C:\Users\jasonsch\code\cli\gcal\bin\Release\netcoreapp2.1>dotnet gcal.dll -c Potential -f https://www.facebook.com/events/429356794181427/

Unhandled Exception: System.Net.Http.HttpRequestException: No such host is known ---> System.Net.Sockets.SocketException: No such host is known
   at System.Net.Http.ConnectHelper.ConnectAsync(String host, Int32 port, CancellationToken cancellationToken)
   --- End of inner exception stack trace ---
   at Google.Apis.Http.ConfigurableMessageHandler.SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) in C:\Apiary\2018-09-13.09-09-57\Src\Support\Google.Apis.Core\Http\ConfigurableMessageHandler.cs:line 494
   at System.Net.Http.HttpClient.FinishSendAsyncBuffered(Task`1 sendTask, HttpRequestMessage request, CancellationTokenSource cts, Boolean disposeCts)
   at Google.Apis.Auth.OAuth2.Requests.TokenRequestExtenstions.ExecuteAsync(TokenRequest request, HttpClient httpClient, String tokenServerUrl, CancellationToken taskCancellationToken, IClock clock) in C:\Apiary\2018-09-13.09-09-57\Src\Support\Google.Apis.Auth\OAuth2\Requests\TokenRequestExtenstions.cs:line 51
   at Google.Apis.Auth.OAuth2.Flows.AuthorizationCodeFlow.FetchTokenAsync(String userId, TokenRequest request, CancellationToken taskCancellationToken) in C:\Apiary\2018-09-13.09-09-57\Src\Support\Google.Apis.Auth\OAuth2\Flows\AuthorizationCodeFlow.cs:line 315
   at Google.Apis.Auth.OAuth2.Flows.AuthorizationCodeFlow.RefreshTokenAsync(String userId, String refreshToken, CancellationToken taskCancellationToken) in C:\Apiary\2018-09-13.09-09-57\Src\Support\Google.Apis.Auth\OAuth2\Flows\AuthorizationCodeFlow.cs:line 264
   at Google.Apis.Auth.OAuth2.UserCredential.RefreshTokenAsync(CancellationToken taskCancellationToken) in C:\Apiary\2018-09-13.09-09-57\Src\Support\Google.Apis.Auth\OAuth2\UserCredential.cs:line 133
   at Google.Apis.Auth.OAuth2.TokenRefreshManager.RefreshTokenAsync() in C:\Apiary\2018-09-13.09-09-57\Src\Support\Google.Apis.Auth\OAuth2\TokenRefreshManager.cs:line 129
   at Google.Apis.Auth.OAuth2.TokenRefreshManager.ResultWithUnwrappedExceptions[T](Task`1 task) in C:\Apiary\2018-09-13.09-09-57\Src\Support\Google.Apis.Auth\OAuth2\TokenRefreshManager.cs:line 174
   at System.Threading.Tasks.ContinuationResultTaskFromResultTask`2.InnerInvoke()
   at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state)
--- End of stack trace from previous location where exception was thrown ---
   at System.Threading.Tasks.Task.ExecuteWithThreadLocal(Task& currentTaskSlot)
--- End of stack trace from previous location where exception was thrown ---
   at Google.Apis.Auth.OAuth2.TokenRefreshManager.GetAccessTokenForRequestAsync(CancellationToken cancellationToken) in C:\Apiary\2018-09-13.09-09-57\Src\Support\Google.Apis.Auth\OAuth2\TokenRefreshManager.cs:line 114
   at Google.Apis.Auth.OAuth2.UserCredential.InterceptAsync(HttpRequestMessage request, CancellationToken taskCancellationToken) in C:\Apiary\2018-09-13.09-09-57\Src\Support\Google.Apis.Auth\OAuth2\UserCredential.cs:line 75
   at Google.Apis.Http.ConfigurableMessageHandler.SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) in C:\Apiary\2018-09-13.09-09-57\Src\Support\Google.Apis.Core\Http\ConfigurableMessageHandler.cs:line 415
   at System.Net.Http.HttpClient.FinishSendAsyncBuffered(Task`1 sendTask, HttpRequestMessage request, CancellationTokenSource cts, Boolean disposeCts)
   at Google.Apis.Requests.ClientServiceRequest`1.ExecuteUnparsedAsync(CancellationToken cancellationToken) in C:\Apiary\2018-09-13.09-09-57\Src\Support\Google.Apis\Requests\ClientServiceRequest.cs:line 180
   at Google.Apis.Requests.ClientServiceRequest`1.Execute() in C:\Apiary\2018-09-13.09-09-57\Src\Support\Google.Apis\Requests\ClientServiceRequest.cs:line 116
   at gcal.Program.FindCalendarByName(CalendarService service, String CalendarName) in C:\Users\jasonsch\code\cli\gcal\Program.cs:line 154
   at gcal.Program.<>c__DisplayClass9_0.<Main>b__2(String value) in C:\Users\jasonsch\code\cli\gcal\Program.cs:line 205
   at Mono.Options.Option.Invoke(OptionContext c)
   at Mono.Options.OptionSet.Parse(String argument, OptionContext c)
   at Mono.Options.OptionSet.Parse(IEnumerable`1 arguments)
   at gcal.Program.Main(String[] args) in C:\Users\jasonsch\code\cli\gcal\Program.cs:line 216
*/
        // TODO -- Support for axs events: https://www.axs.com/events/364765/yonder-mountain-string-band-tickets?src=YBOVMWLI48LN98HDVZGPSPQV&t_tags=YBOVMWLI48LN98HDVZGPSPQV&mkt_campaign=YBOVMWLI48LN98HDVZGPSPQV&mkt_source=AMsySZasdJlhYR4SjRVCortnB_PI&mkt_content=A170_A483_C141547&fbclid=IwAR1fE2hUr7vu4wCder8iXQQhMb063vNSxaMmbAbwis4M-yNqxkYSzEk8Vd8
        // TODO -- Support for cascade bike events (https://www.cascade.org/rides-and-events-major-rides/seattle-bike-n-brews / https://www.cascade.org/rides-and-events-major-rides/seattle-night-ride)
        // TODO -- Handle siff events (e.g., https://www.siff.net/education/film-appreciation/cinema-dissection/terminator-2)
        // TODO -- Handle events that have more than three dates (e.g., https://www.facebook.com/events/262577911273702/ or https://www.facebook.com/events/2075275352740171/?event_time_id=2075275389406834)
        // TODO -- Add high level notes about what this program does.
        // TODO -- Need to handle FB events that aren't public (OAuth?)
        //      -- https://www.facebook.com/events/163743240932607/
        // TODO -- Support metafilter IRL events: https://irl.metafilter.com/4074/The-needle-moved-trivially
        // TODO -- Couldn't read multiple event page like: https://www.facebook.com/events/323430185071606/?event_time_id=323430201738271
        // TODO -- use a recurring entry for FB events with multiple dates/times (or some other way to link them so they can all be [e.g.] deleted together).
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
            Console.WriteLine("-n, --notification <'email|popup==<time period>'>\tThe type of reminder notification and when to show it.");
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
            string EventUrl = null;
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
            options.Add("n|notification=", value => { EventInfo.SetReminderNotification(value); });
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
                    HandleEventURL(service, CalendarID, EventUrl);
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

        private static bool HandleEventURL(CalendarService service, string CalendarID, string Url)
        {
            List<EventInformation> Events = new List<EventInformation>();

            if (!UrlParsers.HandleUrl(Url, Events))
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
