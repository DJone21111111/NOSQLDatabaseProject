using System.Security.Cryptography;
using System.Text;
using IncidentManagementsSystemNOSQL.Models.ViewModels;
using IncidentManagementsSystemNOSQL.Repositories;

namespace IncidentManagementsSystemNOSQL.Service
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly IUserRepository _users;
        private readonly IPasswordResetTokenRepository _tokens;
        private readonly IEmailSender _email;
        private readonly IConfiguration _cfg;

        public PasswordResetService(
            IUserRepository users,
            IPasswordResetTokenRepository tokens,
            IEmailSender email,
            IConfiguration cfg)
        {
            _users = users; _tokens = tokens; _email = email; _cfg = cfg;
        }

        public void IssueTokenByUsername(string username, string requestIp)
        {
            var user = _users.GetByUsername(username.Trim());
            if (user == null) { System.Threading.Thread.Sleep(50); return; }

            _tokens.MarkAllActiveAsUsed(user.Id);

            var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            var hash = Sha256(raw);
            var ttlMin = _cfg.GetValue<int>("PasswordReset:TokenTTLMinutes", 60);

            _tokens.Insert(new PasswordResetToken
            {
                UserId = user.Id,
                TokenHash = hash,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(ttlMin),
                Used = false,
                CreatedAtUtc = DateTime.UtcNow,
                IssuerIp = requestIp
            });

            var baseUrl = _cfg["App:BaseUrl"] ?? "https://localhost:5001";
            var link = $"{baseUrl}/Account/ResetPassword?uid={Uri.EscapeDataString(user.Id)}&token={Uri.EscapeDataString(raw)}";

            var subject = "Reset your Garden Group password";
            var body = $@"<p>Hello {user.Name},</p>
<p>We received a request to reset your password. Click the link below:</p>
<p><a href=""{link}"">{link}</a></p>
<p>This link expires in {ttlMin} minutes. If you didn't request it, you can ignore this email.</p>";

            _ = Task.Run(() => _email.Send(user.Email, subject, body));
        }

        public bool ResetPassword(string userId, string rawToken, string newPassword)
        {
            var token = _tokens.FindActiveByUserAndHash(userId, Sha256(rawToken));
            if (token == null) return false;

            var user = _users.GetById(userId);
            if (user == null) return false;

            var hash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            _users.SetPasswordHash(user.Id, hash);

            token.Used = true;
            _tokens.Update(token);
            return true;
        }

        private static string Sha256(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes);
        }
    }
}
