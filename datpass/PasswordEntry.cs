using System;
using System.Collections.Generic;

namespace datpass
{
    public class PasswordHistoryEntry
    {
        /// <summary>
        /// The former password.
        /// </summary>
        public string password;

        /// <summary>
        /// When this password was removed from service.
        /// </summary>
        public DateTime retirementDate;
    }

    public class PasswordEntry
    {
        public PasswordEntry(string url, string account, string password, string title)
        {
            this.label = string.IsNullOrEmpty(title) ? url : title;
            this.url = url;
            this.account = account;
            this.password = password;
            // TODO
            // this.miscData = MiscData;
            this.creationDate = this.updateDate = DateTime.Now;
        }


        /// <summary>
        /// The label for this entry. Defaults to the Url.
        /// </summary>
        public string label { get; set; }
        public string url { get; set; }
        public string account { get; set; }
        public string password { get; set; }
        public string miscData { get; set; }
        public List<PasswordHistoryEntry> passwordHistory { get; set; }

        /// <summary>
        /// When an entry was created for this URL+user pair.
        /// </summary>
        public DateTime creationDate { get; set; }

        /// <summary>
        /// The last time the password for this URL+user was set. This will match the creation date until the password has been changed.
        /// </summary>
        public DateTime updateDate { get; set; }

        public void UpdatePassword(string newPassword)
        {
            var history = passwordHistory;
            if (history == null)
            {
                history = new List<PasswordHistoryEntry>();
            }

            history.Add(new PasswordHistoryEntry() { password = this.password, retirementDate = DateTime.Now });
            passwordHistory = history;

            updateDate = DateTime.Now;
            password = newPassword;
        }

        // TODO -- Not currently used.
        private string HistoryToString()
        {
            var history = passwordHistory;
            if (history == null)
            {
                return "";
            }

            string s = string.Format("There are {0} entries in the history", history.Count);
            foreach (var h in history)
            {
                s += string.Format("Old password {0} was changed at {1}", h.password, h.retirementDate);
            }

            return s;
        }

        public override string ToString()
        {
            return string.Format($"URL: {url}, user: {account}, title: {label}, password: {password}, created: {creationDate}, updated: {updateDate}");
        }
    }
}
