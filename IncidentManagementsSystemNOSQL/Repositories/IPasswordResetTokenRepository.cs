using IncidentManagementsSystemNOSQL.Models.ViewModels;

namespace IncidentManagementsSystemNOSQL.Repositories
{
    public interface IPasswordResetTokenRepository
    {                                     
        void Insert(PasswordResetToken token);
        PasswordResetToken FindActiveByUserAndHash(string userId, string tokenHash);
        void MarkAllActiveAsUsed(string userId);
        void Update(PasswordResetToken token);
        void EnsureIndexes();
    }
}
