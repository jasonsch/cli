using System;
using System.IO;

namespace mv
{
    class Program
    {
        private static void PrintUsage()
        {
            Console.WriteLine("Invalid parameters ...");
            Console.WriteLine("Usage: ");
            Console.WriteLine("mv <file1> <file2>");
            Console.WriteLine("mv <files> <directory>");

            Environment.Exit(0);
        }

        static void Main(string[] args)
        {
            string Destination;

            if (args.Length < 2)
            {
                PrintUsage();
                return;
            }

            //
            // There are two scenarios: Either we're renaming one file to another or we're
            // moving one or more files into a directory (can't be a file).
            //
            Destination = args[args.Length - 1];
            if (Directory.Exists(Destination))
            {
                for (int i = 0; i < args.Length - 1; ++i)
                {
                    //
                    // Check if this is an actual file. Otherwise, we'll assume it's a wildcard expression.
                    //
                    if (File.Exists(args[i]))
                    {
                        MoveFile(args[i], Path.Combine(Destination, Path.GetFileName(args[i])));
                    }
                    else
                    {
                        foreach (string file in Directory.EnumerateFiles(".", args[i]))
                        {
                            MoveFile(file, Path.Combine(Destination, Path.GetFileName(file)));
                        }
                    }
                }
            }
            else
            {
                //
                // If we have more than two parameters than the last one needed to be a directory.
                //
                if (args.Length > 2)
                {
                    PrintUsage();
                }

                MoveFile(args[0], args[1]);
            }
        }

        static void MoveFile(string SourceFile, string DestFile)
        {
            if (File.Exists(DestFile))
            {
                File.Delete(DestFile);
            }

            File.Move(SourceFile, DestFile);
        }
    }
}
