using System;

namespace sleep
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
            }

            System.Threading.Thread.Sleep(Convert.ToInt32(args[0]) * 1000);
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Invalid usage!");
            Console.WriteLine("sleep <seconds to sleep>");
            Environment.Exit(0);
        }
    }
}
