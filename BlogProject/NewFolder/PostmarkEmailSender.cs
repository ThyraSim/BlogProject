using Microsoft.AspNetCore.Identity.UI.Services;
using PostmarkDotNet;
using System.Threading.Tasks;

namespace BlogProject.NewFolder
{
    public class PostmarkEmailSender : IEmailSender
    {
        //"2cc86aa1-cdc2-4f7a-9caa-51579c889084"
        private readonly string _serverToken;

        public PostmarkEmailSender(string serverToken)
        {
            _serverToken = serverToken;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new PostmarkClient(_serverToken);
            var message = new PostmarkMessage()
            {
                To = email,
                From = "1564848@bdeb.qc.ca", // Your registered Postmark sender
                Subject = subject,
                HtmlBody = htmlMessage
            };

            var sendResult = await client.SendMessageAsync(message);

            if (sendResult.Status != PostmarkStatus.Success)
            {
                throw new InvalidOperationException($"Failed to send email. Reason: {sendResult.Message}");
            }
        }
    }
}
