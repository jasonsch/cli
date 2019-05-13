using System;
using System.Collections.Generic;
using Mono.Options;

namespace filecount
{
    class Program
    {
        static void PrintUsage(string AdditionalInfo = null)
        {
            Console.Write("Invalid usage!");
            if (string.IsNullOrEmpty(AdditionalInfo))
            {
                Console.WriteLine("");
            }
            else
            {
                Console.WriteLine($" {AdditionalInfo}");
            }

            Console.WriteLine("Usage: FileCount [--count <count_to_find>] [directory to inspect]");
            Console.WriteLine("");
            Console.WriteLine("");

            Environment.Exit(0);
        }

        /// <summary>
        /// Maps directories to their file size.
        /// </summary>
        private static readonly Dictionary<string, long> DirectorySizeHash = new Dictionary<string, long>();
        private static readonly Dictionary<string, int> DirectoryCountHash = new Dictionary<string, int>();

        static void Main(string[] args)
        {
            OptionSet options = new OptionSet();
            int? CountToLookFor = null;
            bool PrintTotals = false;
            bool PrintSizes = false;

            options.Add("?|h|help", value => { PrintUsage(); });
            options.Add("c|count=", value => { CountToLookFor = Int32.Parse(value); });
            options.Add("t|totals", value => { PrintTotals = true; });
            options.Add("s|sizes", value => { PrintSizes = true; });

            string TargetDirectory = null;
            var Arguments = options.Parse(args);
            if (Arguments.Count == 0)
            {
                TargetDirectory = Environment.CurrentDirectory;
            }
            else if (Arguments.Count == 1)
            {
                TargetDirectory = Arguments[0];
            }
            else
            {
                PrintUsage("Too many CL arguments!");
            }

            BuildFileCounts(TargetDirectory, PrintTotals, PrintSizes, CountToLookFor);

            foreach (string Key in DirectoryCountHash.Keys)
            {
                Console.WriteLine(string.Format("{0}{1}{2}{3}{4}", Key, CountToLookFor.IsNull() || PrintTotals || PrintSizes ? ": " : "", CountToLookFor.IsNull() ? DirectoryCountHash[Key].ToString() : "", PrintTotals ? " " + "(TODO)" : "", PrintSizes ? " " + DirectorySizeHash[Key].ToString() : ""));
            }
        }

        private static void BuildFileCounts(string targetDirectory, bool printTotals, bool printSizes, int? TargetCount)
        {
            foreach (string SubDirectory in System.IO.Directory.EnumerateDirectories(targetDirectory))
            {
                BuildFileCounts(SubDirectory, printTotals, printSizes, TargetCount);
            }

            int FileCount = 0;
            long FileSizeTotal = 0;
            foreach (string FileName in System.IO.Directory.EnumerateFiles(targetDirectory))
            {
                FileCount++;
                FileSizeTotal += new System.IO.FileInfo(FileName).Length;
            }

            if (!TargetCount.HasValue || TargetCount.Value == FileCount)
            {
                DirectoryCountHash[targetDirectory] = FileCount;
                DirectorySizeHash[targetDirectory] = FileSizeTotal;
            }
        }
    }

    static class ExtensionMethods
    {
        public static bool IsNull<T>(this Nullable<T> NullableValue) where T : struct
        {
            return !NullableValue.HasValue;
        }
    }
}
