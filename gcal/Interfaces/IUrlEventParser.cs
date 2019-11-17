using System.Collections.Generic;
using gcal.Models;

namespace gcal.Interfaces
{
    public interface IUrlEventParser
    {
        bool ParseEvent(string URL, List<EventInformation> EventList);
    }
}
