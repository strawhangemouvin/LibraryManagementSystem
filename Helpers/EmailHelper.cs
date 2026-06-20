using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace LibraryManagementSystem.Helpers
{
    public class EmailHelper
    {
        public static void SendEmail(string toEmail, string subject, string body)
        {
            var smtpEmail = ConfigurationManager.AppSettings["SmtpEmail"];
            var smtpPassword = ConfigurationManager.AppSettings["SmtpPassword"];
            var smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
            var smtpPort = Convert.ToInt32(ConfigurationManager.AppSettings["SmtpPort"]);

            var message = new MailMessage();
            message.From = new MailAddress(smtpEmail, "Library Management System");
            message.To.Add(toEmail);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            using (var smtp = new SmtpClient(smtpHost, smtpPort))
            {
                smtp.Credentials = new NetworkCredential(smtpEmail, smtpPassword);
                smtp.EnableSsl = true;
                smtp.Send(message);
            }
        }
    }
}