using System;
using System.Web;

namespace urldec
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Write(HttpUtility.UrlDecode(Console.ReadLine()));
            }
            else
            {
                for (int i = 0; i < args.Length; ++i)
                {
                    Console.WriteLine(HttpUtility.UrlDecode(args[i]));
                }
            }
        }
    }
}
