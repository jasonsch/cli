using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace datpass
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Error: Wrong number of args!");
                Console.WriteLine("Usage: datpass <password file> <url fragment>");
                return;
            }
            string PasswordFile = args[0];
            // TODO -- Mask password so it doesn't show up as it's typed in the console.
            Console.WriteLine("Master password: ");
            string MasterPassword = Console.ReadLine();
            List<PasswordEntry> Passwords = ReadPasswords(PasswordFile, MasterPassword);

            PasswordEntry Entry = Passwords.Find(pe => pe.Url.Contains(args[1], StringComparison.CurrentCultureIgnoreCase));
            Console.WriteLine($"user ==> {Entry.Account}, password ==> {Entry.Password}");
        }

        private static void WritePasswords(string PasswordFile, string MasterPassword, List<PasswordEntry> Passwords)
        {
            string Contents = Encryption.Encrypt(JsonConvert.SerializeObject(Passwords), MasterPassword);
            File.WriteAllText(PasswordFile, Contents);
        }

        private static List<PasswordEntry> ReadPasswords(string PasswordFile, string MasterPassword)
        {
            return JsonConvert.DeserializeObject<List<PasswordEntry>>(Encryption.Decrypt(File.ReadAllText(PasswordFile), MasterPassword));
        }
    }
}
