using System;
using System.IO;

namespace b64dec
{
    class Program

    {
        static void PrintUsage()
        {
            Console.WriteLine("Usage: b64dec <file> [string]");
            Console.WriteLine("If 'string' is omitted then we read from stdin");
            Environment.Exit(0);
        }

        static void Main(string[] args)
        {
            if (args.Length < 1 || args.Length > 2)
            {
                PrintUsage();
            }

            string Base64Input;

            if (args.Length == 2)
            {
                Base64Input = args[1];
            }
            else
            {
                Base64Input = Console.ReadLine();
            }

            File.WriteAllBytes(args[0], Convert.FromBase64String(Base64Input));
        }
    }
}
