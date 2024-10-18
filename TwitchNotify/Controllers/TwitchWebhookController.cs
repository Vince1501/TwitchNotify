using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Security.Cryptography;
using System.Text;

namespace TwitchNotify.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TwitchWebhookController : ControllerBase
{
    // TODO : Get the secret from the environment variables
    private const string SECRET = "";
    private const string HMAC_PREFIX = "sha256=";


    [HttpPost]
    public IActionResult HandleEvent([FromBody] dynamic payload)
    {
        // Verify if request is from Twitch
        if (!VerifyRequestFromTwitch(Request, payload))
        {
            return Unauthorized();
        }

        // Twitch sends a verification request when the subscription is created
        if (Request.Headers.ContainsKey("Twitch-Eventsub-Message-Type") &&
            Request.Headers["Twitch-Eventsub-Message-Type"] == "webhook_callback_verification")
        {
            // Return the Challange
            return Ok(payload.GetProperty("challenge").GetString());
        }

        // Process the EventSub notification (stream.online or stream.offline)
        Console.WriteLine($"Received event: {payload}");

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
            Console.WriteLine("Message is not from twitch");
            return false;
        }

        Console.WriteLine("Message is verified");
        return true;
    }

    private static string GetHmac(string secret, string message)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }


}
