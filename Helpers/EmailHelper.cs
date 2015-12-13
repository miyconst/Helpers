using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using System.Net;

namespace Helpers
{
    /// <summary>
    /// Helper for sending emails. 
    /// </summary>
    public class EmailHelper
    {
        public static void SendEmail(string Host, int Port, string Login, string Password, bool EnableSsl, string From, string FromDisplay, string To, string Subject, string Body)
        {
            Body = Body ?? "";
            using (SmtpClient client = new SmtpClient(Host, Port))
            {
                client.Credentials = new NetworkCredential(Login, Password);
                client.EnableSsl = EnableSsl;

                MailMessage message = new MailMessage();
                message.From = new MailAddress(From, FromDisplay);
                message.To.Add(To);
                message.Subject = Subject;
                message.Body = Body;
                message.IsBodyHtml = Body.Contains("</");

                try
                {
                    client.Send(message);
                }
                catch (Exception)
                {
                    
                }
            }
        }

        /// <summary>
        /// This method uses default settings for sending emails.
        /// <para>So you should define a set of settings (Host, Port, Login, Password, EnableSsl, From, FromDisplay) in you project first.</para>
        /// &lt;applicationSettings&gt;&lt;Helpers.Properties.Settings&gt;...
        /// </summary>
        /// <param name="To"></param>
        /// <param name="Subject"></param>
        /// <param name="Body"></param>
        public static void SendEmail(string To, string Subject, string Body)
        {
            var ds = Helpers.Properties.Settings.Default;
            SendEmail(ds.Host, ds.Port, ds.Login, ds.Password, ds.EnableSsl, ds.From, ds.FromDisplay, To, Subject, Body);
        }
    }
}