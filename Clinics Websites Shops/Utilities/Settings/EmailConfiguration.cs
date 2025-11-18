using Clinics_Websites_Shops.DataAccess;
using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;

namespace Clinics_Websites_Shops.Utilities.Settings
{
    public class EmailConfiguration
    {
        public string FromName { get; set; }
        public string FromEmail { get; set; }
        public string SmtpServer { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}