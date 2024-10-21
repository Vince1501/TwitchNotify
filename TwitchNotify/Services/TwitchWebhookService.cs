using Microsoft.Extensions.Primitives;
using System.Security.Cryptography;
using System.Text;

namespace TwitchNotify.Services;

public class TwitchWebhookService(IConfiguration configuration, ILogger<TwitchWebhookService> logger, DiscordWebhookSenderService discordWebhookSenderService)
{
    private readonly string SECRET = configuration["SecretWebhook"]!;
    private const string HMAC_PREFIX = "sha256=";
    const string SubscriptionTypeHeader = "Twitch-Eventsub-Subscription-Type";
    const string MessageIdHeader = "Twitch-Eventsub-Message-Id";
    private string messageId = "";
    private string streamTitle = "";

    public async Task HandleIncommingEvents(HttpRequest request, dynamic payload)
    {
        // Get the subscription type from the request headers
        if (request.Headers.TryGetValue(SubscriptionTypeHeader, out StringValues value))
        {
            string subscriptionType = value!;

            switch (subscriptionType)
            {
                case "stream.online":
                    await HandleStreamOnlineEvent(request);
                    break;

                case "channel.update":
                    HandleChannelUpdateEvent(payload);
                    break;

                default:
                    // Handle other event types if needed
                    break;
            }
        }
    }

    private async Task HandleStreamOnlineEvent(HttpRequest request)
    {
        // Check if the message id is different from the previous one to avoid duplicate messages
        string incomingMessageId = request.Headers[MessageIdHeader]!;

        if (messageId != incomingMessageId)
        {
            messageId = incomingMessageId;
            await discordWebhookSenderService.SendMessageAsync(streamTitle); 
        }
    }

    private void HandleChannelUpdateEvent(dynamic payload)
    {
        // Update the stream title from the channel.update event
        streamTitle = payload.GetProperty("event").GetProperty("title").GetString();
        logger.LogInformation($"Stream title updated: {streamTitle} at: {DateTime.UtcNow}");
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
