using Microsoft.Extensions.Primitives;
using System.Security.Cryptography;
using System.Text;

namespace TwitchNotify.Services;

public class TwitchWebhookService(IConfiguration configuration, ILogger<TwitchWebhookService> logger, DiscordWebhookSenderService discordWebhookSenderService)
{
    private readonly string SECRET = configuration["SecretWebhook"]!;
    private const string HMAC_PREFIX = "sha256=";
    private string messageId = "";

    public void HandleIncommingEvents(HttpRequest request)
    {
        // Process the EventSub notification (stream.online)
        if (request.Headers.ContainsKey("Twitch-Eventsub-Subscription-Type") &&
            request.Headers["Twitch-Eventsub-Subscription-Type"] == "stream.online")
        {

            // Check if the message id is the same as the previous one to prevent duplicate messages
            string incommingMessageId = request.Headers["Twitch-Eventsub-Message-Id"]!;

            if (messageId != incommingMessageId)
            {
                messageId = incommingMessageId;
                discordWebhookSenderService.SendMessageAsync();

            }
        }
    }

    public bool VerifyRequestFromTwitch(HttpRequest request, dynamic payload)
    {
        if (!request.Headers.TryGetValue("Twitch-Eventsub-Message-Signature", out StringValues twitchHmacMessage))
        {
            // No HMAC message in the headers so it's not from Twitch
            return false;
        }

        string message = request.Headers["Twitch-Eventsub-Message-Id"] + request.Headers["Twitch-Eventsub-Message-TimeStamp"] + payload;
        string hmac = HMAC_PREFIX + GetHmac(SECRET, message);

        if (twitchHmacMessage != hmac)
        {
            // HMAC message is not valid
            logger.LogWarning("Invalid HMAC message received");
            return false;
        }

        logger.LogInformation("Message is verified");
        return true;
    }

    private static string GetHmac(string secret, string message)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }

}
