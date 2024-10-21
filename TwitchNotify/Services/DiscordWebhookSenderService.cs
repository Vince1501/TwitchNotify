using Newtonsoft.Json.Linq;
using System.Text;


namespace TwitchNotify.Services;

public class DiscordWebhookSenderService(IConfiguration configuration)
{
    private readonly string _webhookUrl = configuration["DiscordWebhookUrl"]!;
    private readonly string _DiscordMessageJson = configuration["DiscordMessageJson"]!;


    public async Task SendMessageAsync(string title)
    {
        using var client = new HttpClient();

        var messageJson = JObject.Parse(_DiscordMessageJson);

        // Access the "embeds" array and the first embed's title
        var embeds = messageJson["embeds"];
        if (embeds != null && embeds.HasValues)
        {
            // Get the existing title
            string existingTitle = embeds[0]!["title"]!.ToString();

            if (!string.IsNullOrEmpty(title))
            {
                embeds[0]!["title"] = $"{existingTitle} - {title}";  // Append the new title at the end
            }
        }

        // Convert the updated JObject back to a JSON string
        string updatedMessageJson = messageJson.ToString();

        var content = new StringContent(updatedMessageJson, Encoding.UTF8, "application/json");

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
