using System;
using System.IO;

namespace b64enc
{
    class Program
    {
        static void PrintUsage()
        {
            Console.WriteLine("Usage: b64enc <file>");
            Environment.Exit(0);
        }

        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                PrintUsage();
            }

            Console.WriteLine(Convert.ToBase64String(File.ReadAllBytes(args[0])));
        }
    }
}
