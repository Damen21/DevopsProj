using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

public class NoOpEmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        // For development, just log or do nothing
        Console.WriteLine($"SendEmailAsync called: {email}, {subject}");
        return Task.CompletedTask;
    }
}