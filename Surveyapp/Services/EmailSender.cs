using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace jirafrelance.Services
{
    public class EmailSender: IEmailSender
    {
        private readonly EmailSettings _emailSettings;
        private readonly IHostingEnvironment _env;

        public EmailSender(
            IOptions<EmailSettings> emailSettings,
            IHostingEnvironment env)
        {
            _emailSettings = emailSettings.Value;
            _env = env;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                var mimeMessage = new MimeMessage();

                mimeMessage.From.Add(new MailboxAddress("surveysdkut", "surveysdkut@gmail.com"));

                mimeMessage.To.Add(new MailboxAddress(email));

                mimeMessage.Subject = subject;

                mimeMessage.Body = new TextPart("html")
                {
                    Text = message
                };

                using (var smtp = new SmtpClient())
                {
                    
                    smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                    await smtp.AuthenticateAsync("surveysdkut@gmail.com", "surveysdkut??");
                    await smtp.SendAsync(mimeMessage);
                    await smtp.DisconnectAsync(true);

                    // if (_env.IsDevelopment())
                    // {
                    //    // The third parameter is useSSL (true if the client should make an SSL-wrapped
                    //    // connection to the server; otherwise, false).
                    //    await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                    // }
                    // else
                    // {
                    //    await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                    // }
                    // // SMTP server authentication
                    // await smtp.AuthenticateAsync("surveysdkut@gmail.com", "surveysdkut??");

                    // await smtp.SendAsync(mimeMessage);

                    // await smtp.DisconnectAsync(true);
                }

            }
            catch (Exception ex)
            {
                // TODO: handle exception
                throw new InvalidOperationException(ex.Message);
            }
        }
    }
    public class EmailSettings
    {
        public string MailServer { get; set; }
        public int MailPort { get; set; }
        public string SenderName { get; set; }
        public string Sender { get; set; }
        public string Password { get; set; }
    }
}
