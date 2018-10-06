using System;
using System.Collections.Generic;
using Mono.Options;

namespace password
{
    class Program
    {
        static readonly int DefaultPasswordLength = 12;

        static void PrintUsage()
        {
            Console.WriteLine("password [--alpha] [--numeric] [--symbols] ([length] | [minlength] [maxlength])");
            Environment.Exit(0);
        }

        static void Main(string[] args)
        {
            PasswordGenerator Generator = new PasswordGenerator();
            OptionSet options = new OptionSet();
            int PasswordMinLength = DefaultPasswordLength;
            int PasswordMaxLength = DefaultPasswordLength;
            GeneratorType Generators = GeneratorType.None;
            List<string> RemainingParameters;

            options.Add("?|h|help", value => { PrintUsage(); });
            options.Add("a|alpha", value => { Generators |= GeneratorType.Alpha; });
            options.Add("n|numeric", value => { Generators |= GeneratorType.Numeric; });
            options.Add("s|symbols=", value => { Generators |= GeneratorType.Symbols; });

            RemainingParameters = options.Parse(args);
            if (RemainingParameters.Count == 1)
            {
                PasswordMinLength = PasswordMaxLength = Convert.ToInt32(RemainingParameters[0]);
            }
            else if (RemainingParameters.Count == 2)
            {
                PasswordMinLength = Convert.ToInt32(RemainingParameters[0]);
                PasswordMaxLength = Convert.ToInt32(RemainingParameters[1]);
            }
            else if (RemainingParameters.Count != 0)
            {
                PrintUsage();
            }

            //
            // Default to all generators if none are specified on the command line.
            //
            if (Generators == GeneratorType.None)
            {
                Generator.AddAllGenerators();
            }

            // Console.WriteLine(Generator.Generate(5));
        }
    }
}
