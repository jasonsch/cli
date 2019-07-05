using System;
using System.Collections.Generic;

namespace datpass
{
    public class PasswordHistoryEntry
    {
        /// <summary>
        /// The unencrypted former password.
        /// </summary>
        public string Password;

        /// <summary>
        /// When this password was removed from service.
        /// </summary>
        public DateTime RetirementDate;
    }

    public class PasswordEntry
    {
        public PasswordEntry(string Url, string Account, string Password, string MiscData)
        {
            Label = Url;
            this.Url = Url;
            this.Account = Account;
            this.Password = Password;
            this.MiscData = MiscData;
        }


        /// <summary>
        /// The label for this entry. Defaults to the Url.
        /// </summary>
        public string Label { get; set; }
        public string Url { get; set; }
        public string Account { get; set; }
        public string Password { get; set; }
        public PasswordHistoryEntry[] PasswordHistory { get; set; }
        public string MiscData { get; set; }
    }
}
