using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace IncidentManagementsSystemNOSQL.Service
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _cfg;
        public SmtpEmailSender(IConfiguration cfg) => _cfg = cfg;

        public async Task Send(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
        {
            var host = _cfg["Smtp:Host"];
            var portValue = _cfg["Smtp:Port"];
            var port = int.TryParse(portValue, out var p) ? p : 587;

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_cfg["Smtp:User"], _cfg["Smtp:Pass"]),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 15000
            };

            using var mail = new MailMessage(_cfg["Smtp:From"], toEmail, subject, htmlBody) { IsBodyHtml = true };
            await client.SendMailAsync(mail);
        }
    }
}
