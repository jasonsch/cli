using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Mono.Options;

namespace datpass
{
    partial class Program
    {
        enum PasswordAction
        {
            Add, // Add a new entry
            Delete, // Deletes an entry
            Find, // Find entries based on a URL fragment
            Update, // Update an entry's password
            Export,
            Import
        }

        class ConfigOptions
        {
            public ConfigOptions()
            {
                passwordFile = System.Environment.GetEnvironmentVariable("DATPASS_PASSWORD_FILE");
                action = PasswordAction.Find;
            }

            /// <summary>
            /// Can be specified on the command line; if not, we look for using the environment variable DATPASS_PASSWORD_FILE.
            /// </summary>
            public string passwordFile;

            /// <summary>
            /// The URL that we're operating on.
            /// </summary>
            public string url;

            /// <summary>
            /// The username to use if we're creating a new entry (only valid sometimes).
            /// </summary>
            public string userName;

            /// <summary>
            /// The password to use if we're updating an existing entry and the user specifies one.
            /// </summary>
            public string password;

            public PasswordAction action;
        }

        static void PrintUsage(bool invalidParameter = false)
        {
            // TODO
            if (invalidParameter)
            {
                Console.WriteLine("Error: Wrong number of args!");
            }
            Console.WriteLine("Usage: datpass [?|h|help] [-a|--add] [-d|--delete] [-e|--export] [-i|-import <password file>] [-f|--file <password file>] [-s|--set <password (can be empty)>] [-u|--username] <username>] <url>");
            System.Environment.Exit(1);
        }

        static ConfigOptions ParseParameters(string[] args)
        {
            // TODO -- Prevent multiple action commands.
            ConfigOptions config = new ConfigOptions();
            OptionSet options = new OptionSet();
            options.Add("?|h|help", value => { PrintUsage(); });
            options.Add("a|add", value => { config.action = PasswordAction.Add; });
            options.Add("d|delete", value => { config.action = PasswordAction.Delete; });
            options.Add("e|export", value => { config.action = PasswordAction.Export; });
            options.Add("f|file=", value => { config.passwordFile = value; });
            options.Add("i|import=", value => { config.action = PasswordAction.Import; config.url = value; });
            options.Add("s|set=", value => { config.action = PasswordAction.Update; config.password = value; });
            options.Add("u|username=", value => { config.userName = value; });

            List<string> nakedParameters = options.Parse(args);
            if (nakedParameters.Count == 1)
            {
                config.url = nakedParameters[0];
            }
            else if (nakedParameters.Count > 1)
            {
                PrintUsage();
            }

            return config;
        }

        static string ReadMaskedString()
        {
            // We set these colors to the same value to hide the password that's typed..
            var color = Console.ForegroundColor;
            Console.ForegroundColor = Console.BackgroundColor;
            string str = Console.ReadLine();
            Console.ForegroundColor = color;

            return str;
        }

        static void Main(string[] args)
        {
            var config = ParseParameters(args);

            Console.WriteLine("Master password: ");
            string masterPassword = ReadMaskedString();

            List<PasswordEntry> passwords = ReadPasswords(config.passwordFile, masterPassword);

            if (config.action == PasswordAction.Add)
            {
                if (AddPassword(passwords, config.url, config.userName))
                {
                    WritePasswords(config.passwordFile, masterPassword, passwords);
                }
            }
            else if (config.action == PasswordAction.Delete)
            {
                if (DeletePassword(passwords, config.url, config.userName))
                {
                    WritePasswords(config.passwordFile, masterPassword, passwords);
                }
            }
            else if (config.action == PasswordAction.Find)
            {
                var entries = passwords.FindAll(pe => pe.url.Contains(config.url, StringComparison.CurrentCultureIgnoreCase));
                foreach (var entry in entries)
                {
                    Console.WriteLine(entry);
                }
            }
            else if (config.action == PasswordAction.Export)
            {
                ExportPasswords(passwords);
            }
            else if (config.action == PasswordAction.Import)
            {
                ImportPasswords(config.passwordFile, config.url, masterPassword);
            }
            else
            {
                if (UpdatePassword(passwords, config.url, config.password))
                {
                    WritePasswords(config.passwordFile, masterPassword, passwords);
                }
            }
        }

        private static void ImportPasswords(string passwordFile, string importFile, string masterPassword)
        {
            string passwordJson = File.ReadAllText(importFile);
            List<PasswordEntry> passwords = JsonConvert.DeserializeObject<List<PasswordEntry>>(passwordJson);

            WritePasswords(passwordFile, masterPassword, passwords);
        }

        private static void ExportPasswords(List<PasswordEntry> passwords)
        {
            System.Console.WriteLine(JsonConvert.SerializeObject(passwords));
        }

        private static bool DeletePassword(List<PasswordEntry> passwords, string url, string userName)
        {
            var entry = passwords.Find(pe => pe.url.Contains(url, StringComparison.CurrentCultureIgnoreCase) && (string.IsNullOrEmpty(userName) || pe.account == userName));
            if (entry == default(PasswordEntry))
            {
                Console.WriteLine("Couldn't find any entry for {0}!", url);
                return false;
            }

            Console.WriteLine("Deleting entry for {0} / {1}", entry.url, entry.account);
            passwords.Remove(entry);
            return true;
        }

        private static bool AddPassword(List<PasswordEntry> passwords, string url, string userName)
        {
            var entries = passwords.FindAll(pe => pe.url.Contains(url, StringComparison.CurrentCultureIgnoreCase) && pe.account == userName);
            if (entries.Count != 0)
            {
                Console.WriteLine("There's an existing entry for {0} / {1}", url, userName);
                return false;
            }

            Console.Write("Title: ");
            string title = Console.ReadLine();

            var passwordGenerator = new PasswordGenerator(10, 15);
            passwordGenerator.AddAllGenerators();
            string password = passwordGenerator.Generate();
            Console.WriteLine("Password for {0}/{1} is {2}", url, userName, password);

            var pe = new PasswordEntry(url, userName, password, title);
            passwords.Add(pe);

            return true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="passwords"></param>
        /// <param name="url"></param>
        /// <returns>True if the password list was modified, meaning the passwords should be re-written.</returns>
        private static bool UpdatePassword(List<PasswordEntry> passwords, string url, string password)
        {
            var entries = passwords.FindAll(pe => pe.url.Contains(url, StringComparison.CurrentCultureIgnoreCase));
            if (entries.Count == 0)
            {
                // TODO -- Some kind of fuzzy searching.
                Console.WriteLine("Couldn't find any entries for {0}", url);
            }
            else if (entries.Count == 1)
            {
                if (string.IsNullOrEmpty(password))
                {
                    var passwordGenerator = new PasswordGenerator(10, 15);
                    passwordGenerator.AddAllGenerators();
                    password = passwordGenerator.Generate();
                }

                entries[0].UpdatePassword(password);
                Console.WriteLine("Password for {0} is now '{1}'", entries[0].url, password);
            }
            else
            {
                Console.WriteLine("URL {0} was ambiguous:", url);
                foreach (var entry in entries)
                {
                    Console.WriteLine($"user ==> {entry.account}, title ==> {entry.label}, password ==> {entry.password}");
                }
            }

            return entries.Count != 0;
        }

        private static void WritePasswords(string passwordFile, string masterPassword, List<PasswordEntry> passwords)
        {
            string contents = Encryption.Encrypt(JsonConvert.SerializeObject(passwords), masterPassword);
            File.WriteAllText(passwordFile, contents);
        }

        private static List<PasswordEntry> ReadPasswords(string passwordFile, string masterPassword)
        {
            if (File.Exists(passwordFile))
            {
                return JsonConvert.DeserializeObject<List<PasswordEntry>>(Encryption.Decrypt(File.ReadAllText(passwordFile), masterPassword));
            }
            else
            {
                Console.WriteLine("Password file '{0}' doesn't exist, assuming this is the first run ...", passwordFile);
                return new List<PasswordEntry>();
            }
        }
    }
}
