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
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                throw new Exception("Email tujuan kosong.");
            }

            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new Exception("Subject email kosong.");
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                throw new Exception("Isi email kosong.");
            }

            var smtpEmail = ConfigurationManager.AppSettings["SmtpEmail"];
            var smtpPassword = ConfigurationManager.AppSettings["SmtpPassword"];
            var smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
            var smtpPortText = ConfigurationManager.AppSettings["SmtpPort"];

            if (string.IsNullOrWhiteSpace(smtpEmail))
            {
                throw new Exception("SmtpEmail belum diisi di Web.config.");
            }

            if (string.IsNullOrWhiteSpace(smtpPassword))
            {
                throw new Exception("SmtpPassword belum diisi di Web.config.");
            }

            if (string.IsNullOrWhiteSpace(smtpHost))
            {
                throw new Exception("SmtpHost belum diisi di Web.config.");
            }

            if (string.IsNullOrWhiteSpace(smtpPortText))
            {
                throw new Exception("SmtpPort belum diisi di Web.config.");
            }

            int smtpPort = Convert.ToInt32(smtpPortText);

            using (MailMessage message = new MailMessage())
            {
                message.From = new MailAddress(smtpEmail, "Moon Books Area");

                // INI YANG PALING PENTING
                message.To.Add(toEmail);

                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                using (SmtpClient smtp = new SmtpClient(smtpHost, smtpPort))
                {
                    smtp.Credentials = new NetworkCredential(smtpEmail, smtpPassword);
                    smtp.EnableSsl = true;
                    smtp.Send(message);
                }
            }
        }
    }
}