using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace TwitchNotify.Services;

public class DiscordWebhookSenderService(IConfiguration configuration)
{
    private readonly string _webhookUrl = configuration["DiscordWebhookUrl"]!;
    private readonly string _messageJson = configuration["DiscordMessageJson"]!;

    public async Task SendMessageAsync()
    {
        using var client = new HttpClient();
        var content = new StringContent(_messageJson, Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync(_webhookUrl, content);
            response.EnsureSuccessStatusCode(); // Throws if the response is not successful
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message: {ex.Message}");
        }
    }
}
