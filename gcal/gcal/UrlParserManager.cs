using System;
using System.Collections.Generic;
using gcal.Models;
using System.Reflection;
using System.Linq;
using gcal.Interfaces;

namespace gcal
{
    public class UrlParserManager
    {
        private readonly List<IUrlEventParser> Parsers = new List<IUrlEventParser>();

        public UrlParserManager(IUrlDownload Downloader)
        {
            Assembly a = Assembly.GetEntryAssembly();

            foreach (TypeInfo ti in a.DefinedTypes)
            {
                if (ti.ImplementedInterfaces.Contains(typeof(IUrlEventParser)))
                {
                    Parsers.Add(a.CreateInstance(ti.FullName, false, 0, null, new object[] { Downloader }, null, null) as IUrlEventParser);
                }
            }
        }

        public bool HandleUrl(String Url, List<EventInformation> EventList)
        {
            for (int i = 0; i < Parsers.Count; ++i)
            {
                if (Parsers[i].ParseEvent(Url, EventList))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
