using Microsoft.AspNetCore.Mvc;
using TwitchNotify.Services;

namespace TwitchNotify.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TwitchWebhookController(ILogger<TwitchWebhookController> logger, TwitchWebhookService twitchWebhookService) : ControllerBase
{

    [HttpPost]
    public IActionResult HandleEvent([FromBody] dynamic payload)
    {
        // Verify if request is from Twitch
        if (!twitchWebhookService.VerifyRequestFromTwitch(Request, payload))
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

        twitchWebhookService.HandleIncommingEvents(Request, payload);

        return Ok();
    }
}
