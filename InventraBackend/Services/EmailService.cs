using MailKit.Net.Smtp;
using MimeKit;

namespace InventraBackend.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendVerificationEmailAsync(string toEmail, string token)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Inventra", _config["Email:Sender"]));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = "Verify your Inventra account";

            string verifyUrl = $"https://localhost:5001/api/verify?token={token}";

            message.Body = new TextPart("html")
            {
                Text = $@"
                    <h2>Welcome to Inventra!</h2>
                    <p>Click below to verify your email:</p>
                    <a href='{verifyUrl}'>Verify Email</a>
                "
            };

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_config["Email:Sender"], _config["Email:Password"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}