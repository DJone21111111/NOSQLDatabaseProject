
namespace IncidentManagementsSystemNOSQL.Service
{
    public interface IPasswordResetService
    {
        Task IssueTokenByUsername(string username, string requestIp, CancellationToken ct = default);
        bool ResetPassword(string userId, string rawToken, string newPassword);
    }
}
