using System.Threading;
using System.Threading.Tasks;

namespace IncidentManagementsSystemNOSQL.Service
{
    public interface IEmailSender
    {
        Task Send(string toEmail, string subject, string htmlBody, CancellationToken ct = default);
    }
}
