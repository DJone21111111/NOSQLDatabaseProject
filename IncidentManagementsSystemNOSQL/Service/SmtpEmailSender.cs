using System.Net;
using System.Net.Mail;

namespace IncidentManagementsSystemNOSQL.Service
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _cfg;
        public SmtpEmailSender(IConfiguration cfg) => _cfg = cfg;

        public void Send(string toEmail, string subject, string htmlBody)
        {
            using var client = new SmtpClient(_cfg["Smtp:Host"], int.Parse(_cfg["Smtp:Port"]))
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_cfg["Smtp:User"], _cfg["Smtp:Pass"]),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 15000
            };
            using var mail = new MailMessage(_cfg["Smtp:From"], toEmail, subject, htmlBody) { IsBodyHtml = true };
            client.Send(mail);
        }
    }
}
