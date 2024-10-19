using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Security.Cryptography;
using System.Text;

namespace TwitchNotify.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TwitchWebhookController(ILogger<TwitchWebhookController> logger, IConfiguration configuration) : ControllerBase
{
    private string SECRET = configuration["SecretWebhook"]!;
    private const string HMAC_PREFIX = "sha256=";

    [HttpPost]
    public IActionResult HandleEvent([FromBody] dynamic payload)
    {
        // Verify if request is from Twitch
        if (!VerifyRequestFromTwitch(Request, payload))
        {
            return Unauthorized("Who the fuck are you");
        }

        // Twitch sends a verification request when the subscription is created
        if (Request.Headers.ContainsKey("Twitch-Eventsub-Message-Type") &&
            Request.Headers["Twitch-Eventsub-Message-Type"] == "webhook_callback_verification")
        {
            logger.LogInformation("Verification request received");
            // Return the Challange
            return Ok(payload.GetProperty("challenge").GetString());
        }

        // Process the EventSub notification (stream.online or stream.offline)
        logger.LogInformation($"Received event: {payload}");

        return Ok();
    }

    private bool VerifyRequestFromTwitch(HttpRequest request, dynamic payload)
    {
        if (!Request.Headers.TryGetValue("Twitch-Eventsub-Message-Signature", out StringValues twitchHmacMessage))
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
