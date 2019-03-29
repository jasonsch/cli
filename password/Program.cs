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
            List<GeneratorType> Generators = new List<GeneratorType>();
            List<string> RemainingParameters;

            options.Add("?|h|help", value => PrintUsage());
            options.Add("a|alpha", value => Generators.Add(GeneratorType.Alpha));
            options.Add("n|numeric", value => Generators.Add(GeneratorType.Numeric));
            options.Add("s|symbols=", value => Generators.Add(GeneratorType.Symbols));

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
            if (Generators.Count == 0)
            {
                Generator.AddAllGenerators();
            }
            else
            {
                foreach (var Gen in Generators)
                {
                    Generator.AddGenerator(Gen);
                }
            }

            Console.WriteLine(Generator.Generate(PasswordMinLength, PasswordMaxLength));
        }
    }
}
