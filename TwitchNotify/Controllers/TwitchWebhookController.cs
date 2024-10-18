using Microsoft.AspNetCore.Mvc;

namespace TwitchNotify.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TwitchWebhookController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> HandleEvent([FromBody] dynamic payload)
        {
            // Twitch sends a verification request when the subscription is created
            if (Request.Headers.ContainsKey("Twitch-Eventsub-Message-Type") &&
                Request.Headers["Twitch-Eventsub-Message-Type"] == "webhook_callback_verification")
            {
                // Print all the headers
                foreach (var header in Request.Headers)
                {
                    Console.WriteLine($"{header.Key}: {header.Value}");
                }

                // Return the Challange
                Console.WriteLine($"Received event: {payload}");
                return Ok(payload.GetProperty("challenge").GetString());
            }

            // Process the EventSub notification (stream.online or stream.offline)
            Console.WriteLine($"Received event: {payload}");

            return Ok();
        }
    }
}
