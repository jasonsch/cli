using System;
using System.Collections.Generic;
using Mono.Options;
using System.Text.RegularExpressions;

namespace find
{
    class Program
    {
        static void PrintUsage()
        {
            Console.WriteLine("Error: invalid number of parameters!");
            Console.WriteLine("Usage: find <starting dir>");
            Environment.Exit(0);
        }

        static void Main(string[] args)
        {
            OptionSet options = new OptionSet();
            string startingDir = null;
            string name = null;
            bool printFiles = true; // TODO

            options.Add("?|h|help", value => { PrintUsage(); });
            options.Add("f=", value => startingDir = value);
            options.Add("name=", value => name = value);
            options.Add("print", value => printFiles = true);


            List<string> nakedParameters = options.Parse(args);
            if (startingDir == null)
            {
                if (nakedParameters.Count == 0)
                {
                    PrintUsage();
                }

                // TODO -- Can there be more than one?
                startingDir = nakedParameters[0];
            }

            name = MassageRegularExpression(name);

            List<string> files = GetFilesFromDirectory(startingDir); // TODO
            foreach (string file in files)
            {
                if (Regex.IsMatch(file, name))
                {
                    if (printFiles)
                    {
                        Console.WriteLine(file);
                    }
                }
            }
        }

        /// <summary>
        /// We need to convert a RE one might use on the command line (e.g., "*.txt") into the
        /// corresponding .NET RE. This isn't ideal and should revisited.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string MassageRegularExpression(string name)
        {
            name = Regex.Replace(name, "\\.", "\\.");
            name = Regex.Replace(name, "\\*", "[a-zA-Z0-9]+");
            Console.WriteLine("name is now ==> " + name); // TODO
            return name;
        }

        // TODO -- Create a custom enumerator so we don't have to return a giant list.
        static List<string> GetFilesFromDirectory(string dir, List<string> fileList = null)
        {
            if (fileList == null)
            {
                fileList = new List<string>();
            }

            foreach (string childDir in System.IO.Directory.GetDirectories(dir))
            {
                GetFilesFromDirectory(childDir, fileList);
            }

            foreach (string file in System.IO.Directory.GetFiles(dir))
            {
                // TODO
                // Console.WriteLine($"Adding file {file}");
                fileList.Add(file);
            }

            return fileList;
        }
    }
}
