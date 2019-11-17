using System.Collections.Generic;
using System.Net;
using gcal.Interfaces;

namespace gcal
{
    class URLDownloader : IUrlDownload
    {
        private readonly Dictionary<string, string> Cache = new Dictionary<string, string>();

        public string GetPageContents(string URL)
        {
            if (!Cache.ContainsKey(URL))
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                using (var client = new WebClient())
                {
                    client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                    Cache[URL] = client.DownloadString(URL);
                }
            }

            return Cache[URL];
        }
    }
}
