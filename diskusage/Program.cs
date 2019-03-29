using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace diskusage
{
    public static class Ext
    {
        private const long OneKb = 1024;
        private const long OneMb = OneKb * 1024;
        private const long OneGb = OneMb * 1024;
        private const long OneTb = OneGb * 1024;

        public static string ToPrettySize(this int value, int decimalPlaces = 0)
        {
            return ((long)value).ToPrettySize(decimalPlaces);
        }

        public static string ToPrettySize(this long value, int decimalPlaces = 0)
        {
            var asTb = Math.Round((double)value / OneTb, decimalPlaces);
            var asGb = Math.Round((double)value / OneGb, decimalPlaces);
            var asMb = Math.Round((double)value / OneMb, decimalPlaces);
            var asKb = Math.Round((double)value / OneKb, decimalPlaces);
            string chosenValue = asTb > 1 ? string.Format("{0}Tb", asTb)
                : asGb > 1 ? string.Format("{0}Gb", asGb)
                : asMb > 1 ? string.Format("{0}Mb", asMb)
                : asKb > 1 ? string.Format("{0}Kb", asKb)
                : string.Format("{0}B", Math.Round((double)value, decimalPlaces));
            return chosenValue;
        }
    }

    // TODO -- Interface that encapsulates file operations and can read a .json/.xml file for testing (and has "expressive" functions for creating a directory structure).
    class Program
    {
        private static int NumberToDisplay = 10; // TODO
        private static string[] ToExclude = null; // TODO

        static void PrintUsage()
        {
            Console.WriteLine("Invalid usage!");
            Console.WriteLine("Usage: diskusage [Starting Directory]");
            Environment.Exit(0);
        }

        static void Main(string[] args)
        {
            string StartingDirectory = null;

            if (args.Length == 0)
            {
                StartingDirectory = Environment.CurrentDirectory;
            }
            else if (args.Length == 1)
            {
                StartingDirectory = args[0];
            }
            else
            {
                PrintUsage();
            }

            var Totals = GetDirectorySizes(StartingDirectory);

            foreach (KeyValuePair<string, long> kvp in Totals.OrderByDescending(kvp => kvp.Value).Take(NumberToDisplay))
            {
                Console.WriteLine("{0} => {1}", kvp.Key, kvp.Value.ToPrettySize());
            }
        }

        private static Dictionary<string, long> GetDirectorySizes(string StartingDirectory)
        {
            Dictionary<string, long> Totals = new Dictionary<string, long>();

            GetDirectorySizesWorker(StartingDirectory, Totals);

            return Totals;
        }

        private static long GetDirectorySizesWorker(string StartingDirectory, Dictionary<string, long> Totals)
        {
            long Total = 0;

            try
            {

                foreach (string ChildDirectory in Directory.EnumerateDirectories(StartingDirectory))
                {
                    long ChildTotal = GetDirectorySizesWorker(ChildDirectory, Totals);
                    Totals[ChildDirectory] = ChildTotal;
                    Total += ChildTotal;
                }
            } catch (UnauthorizedAccessException e)
            {
                Console.WriteLine($"Can't access some child directory in {StartingDirectory}");
            }

            foreach (string ChildFile in Directory.EnumerateFiles(StartingDirectory))
            {
                Total += new FileInfo(ChildFile).Length;
            }

            Totals[StartingDirectory] = Total;

            return Total;
        }
    }
}
