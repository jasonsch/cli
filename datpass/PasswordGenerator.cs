using System;
using System.Text;
using System.Collections.Generic;

namespace datpass
{
    public enum GeneratorType
    {
        None = 0x0,
        Alpha = 0x1,
        Numeric = 0x2,
        Symbols = 0x4
    }

    class PasswordGenerator
    {
        private readonly Random random = new System.Random();
        private readonly IList<Func<char>> Generators = new List<Func<char>>();

        public int MinimumLength { get; set; }
        public int MaximumLength { get; set; }

        public PasswordGenerator() { }
        public PasswordGenerator(int Length) : this(Length, Length) { }

        public PasswordGenerator(int MinimumLength, int MaximumLength)
        {
            this.MinimumLength = MinimumLength;
            this.MaximumLength = MaximumLength;
        }

        public void AddGenerator(GeneratorType Type)
        {
            if ((Type & GeneratorType.Alpha) != 0)
            {
                Generators.Add(GenerateAlpha);
            }

            if ((Type & GeneratorType.Numeric) != 0)
            {
                Generators.Add(GenerateNumber);
            }

            if ((Type & GeneratorType.Symbols) != 0)
            {
                Generators.Add(GenerateSymbol);
            }
        }

        public void AddAllGenerators()
        {
            AddGenerator(GeneratorType.Alpha | GeneratorType.Numeric | GeneratorType.Symbols);
        }

        public string Generate()
        {
            return Generate(MinimumLength, MaximumLength);
        }

        public string Generate(int MinLength, int MaxLength)
        {
            return Generate(random.Next(0, MaxLength - MinLength + 1) + MinLength);
        }

        public string Generate(int Length)
        {
            StringBuilder Password = new StringBuilder(Length);

            for (int i = 0; i < Length; ++i)
            {
                int GeneratorIndex = random.Next(0, Generators.Count - 1);
                Password.Append(Generators[GeneratorIndex].Invoke());
            }

            return Password.ToString();
        }

        private char GenerateAlpha()
        {
            return GenerateCharacterFromAlphabet("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ");
        }

        private char GenerateNumber()
        {
            return GenerateCharacterFromAlphabet("0123456789");
        }

        private char GenerateSymbol()
        {
            return GenerateCharacterFromAlphabet(@"!@#$%^&*()-_=+[{]}\|;:,<.>/?");
        }

        private char GenerateCharacterFromAlphabet(string Alphabet)
        {
            return Alphabet[random.Next(Alphabet.Length)];
        }
    }
}
