using System;

namespace yas
{
    class Program
    {
        const string DefaultOutputString = "queen";

        static void Main(string[] args)
        {
            string ToPrint;

            if (args.Length > 0)
            {
                ToPrint = args[0];
            }
            else
            {
                ToPrint = DefaultOutputString;
            }

            PrintString(ToPrint);
        }

        static void PrintString(string OutputString)
        {
            while (true)
            {
                Console.WriteLine($"{OutputString}");
            }
        }
    }
}
