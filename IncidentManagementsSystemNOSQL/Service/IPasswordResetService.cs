namespace IncidentManagementsSystemNOSQL.Service
{
    public interface IPasswordResetService
    {
        void IssueTokenByUsername(string username, string requestIp);
        bool ResetPassword(string userId, string rawToken, string newPassword);
    }
}
