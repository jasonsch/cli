using System;

namespace uuid
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine(System.Guid.NewGuid().ToString());
            }
            else
            {
                bool IsValidGUID = Guid.TryParse(args[0], out Guid uuid);
                Console.WriteLine($"'{args[0]}' is " + (IsValidGUID ? "" : "NOT ") + "a valid GUID");
            }
        }
    }
}
