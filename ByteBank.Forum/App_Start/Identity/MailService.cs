using ByteBank.Forum.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace ByteBank.Forum.App_Start.Identity
{
    public class MailService : IIdentityMessageService
    {
        private readonly string Email = ConfigurationManager.AppSettings["MailService:email_from"];
        private readonly string Email_Password = ConfigurationManager.AppSettings["MailService:email_password"];

        public async Task SendAsync(IdentityMessage message)
        {
            using (var messageEmail = new MailMessage())
            {
                messageEmail.From = new MailAddress(Email);

                messageEmail.Subject = message.Subject;
                messageEmail.To.Add(message.Destination);
                messageEmail.Body = message.Body;

                // SMTP - Simple Mail Transport Protocol

                using (var smtpClient = new SmtpClient())
                {
                    // Conexão com a caixa de email da aplicação
                    smtpClient.UseDefaultCredentials = true;
                    smtpClient.Credentials = new NetworkCredential(Email, Email_Password);

                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.Host = "smtp.gmail.com";
                    smtpClient.Port = 587;
                    smtpClient.EnableSsl = true;

                    smtpClient.Timeout = 20000;

                    await smtpClient.SendMailAsync(messageEmail);
                        
                }
            }
        }
    }
}