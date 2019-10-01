using System;
using System.Collections.Generic;
using Mono.Options;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;

namespace grep
{
    class Program
    {
        static bool verbosePrint = false;

        static void Main(string[] args)
        {
            OptionSet options = new OptionSet();
            bool bRecurse = false;

            options.Add("r|recurse", arg => bRecurse = true);
            options.Add("v|verbose", arg => verbosePrint = true);
            List<string> commandLine = options.Parse(args);

            // TODO -- Read from stdin.
            if (commandLine.Count == 0)
            {
                PrintUsage();
            }

            GrepFiles(bRecurse, commandLine[0], Environment.CurrentDirectory, commandLine.Skip(1));
        }

        private static void GrepFiles(bool bRecurse, string pattern, string currentDirectory, IEnumerable<string> filePattern)
        {
            foreach (string file in GetMatchingFiles(currentDirectory, filePattern))
            {
                VerbosePrint($"Matching file ==> {file}");
                string result = FindStringInStream(pattern, File.OpenRead(file));
                if (result != null)
                {
                    Console.WriteLine($"{file}: {result}"); // TODO -- Format
                }
            }

            if (bRecurse)
            {
                foreach (string directory in Directory.EnumerateDirectories(currentDirectory))
                {
                    GrepFiles(bRecurse, pattern, directory, filePattern);
                }
            }
        }

        private static string FindStringInStream(string pattern, Stream fileStream)
        {
            var TextReader = new StreamReader(fileStream);
            string line;

            while ((line = TextReader.ReadLine()) != null)
            {
                //
                // TODO -- Support -i flag.
                //
                if (line.Contains(pattern))
                {
                    return line;
                }
            }

            return null;
        }

        private static void VerbosePrint(string output)
        {
            if (verbosePrint)
            {
                Console.WriteLine(output);
            }
        }

        private static string FixRegularExpression(string pattern)
        {
            pattern = Regex.Replace(pattern, @"\.", "\\.");
            pattern = Regex.Replace(pattern, "\\*", @"[\w\d\._-]*");
            return "^" + pattern + "$";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentDirectory"></param>
        /// <param name="filePattern">List of file names or regular expressions (e.g., "foo.txt baz.bar" or "*.c *.h")</param>
        /// <returns></returns>
        private static IEnumerable<string> GetMatchingFiles(string currentDirectory, IEnumerable<string> filePattern)
        {
            List<string> matchingFiles = new List<string>();

            foreach (string file in System.IO.Directory.EnumerateFiles(currentDirectory))
            {
                string fileName = System.IO.Path.GetFileName(file);

                foreach (string pattern in filePattern)
                {
                    string fixedupPattern = FixRegularExpression(pattern); // TODO -- Fix these up front.
                    if (Regex.IsMatch(fileName, fixedupPattern))
                    {
                        yield return file;
                    }
                }
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Invalid usage!");
            Console.WriteLine("Usage: grep [-v] [-r] <pattern> <files*>");

            Environment.Exit(0);
        }
    }
}
