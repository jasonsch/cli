using System;
using System.IO;

namespace uniq
{
    class Program
    {
        static void Main(string[] args)
        {
            StreamReader Input;

            if (args.Length == 0)
            {
                Input = new StreamReader(Console.OpenStandardInput());
            }
            else
            {
                Input = new StreamReader(args[0]);
            }

            UniqueLines(Input);
        }

        static void UniqueLines(StreamReader LineStream)
        {
            string PreviousLine = null;
            string Line;

            while (true)
            {
                Line = LineStream.ReadLine();
                if (Line == null)
                {
                    break;
                }

                if (Line != PreviousLine)
                {
                    Console.WriteLine(Line);
                    PreviousLine = Line;
                }
            }
        }
    }
}
