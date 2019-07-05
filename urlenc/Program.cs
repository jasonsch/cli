using System;
using System.Web;

namespace urlenc
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Write(HttpUtility.UrlEncode(Console.ReadLine()));
            }
            else
            {
                for (int i = 0; i < args.Length; ++i)
                {
                    Console.WriteLine(HttpUtility.UrlEncode(args[i]));
                }
            }
        }
    }
}
