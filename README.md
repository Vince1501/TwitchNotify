# TwitchNotify

This project provides a webhook handler for Twitch events, allowing integration with a Discord webhook to send notifications when your favorite streamer goes live.

## Features

- **Twitch Webhook Controller**: Receives and verifies Twitch webhook events.
- **Event Handling**: Handles `stream.online` and `channel.update` events.
- **Discord Integration**: Sends a notifications to a Discord channel when a `stream.online` event is received using a Discord Webhook.

## Prerequisites

- **.NET 8 SDK**
- **Twitch EventSub subscription**: To receive events from Twitch, you need to configure EventSub.
- **Discord Webhook URL**: Set up a Discord webhook to receive notifications.

## Configuration

The project relies on the following configuration values:

- `SecretWebhook`: A secret string for verifying Twitch requests.
- `DiscordWebhookUrl`: The URL of your Discord webhook.
- `DiscordMessageJson`: A JSON string template for the message sent to Discord.

These can be configured in the `appsettings.json` or through environment variables.

Example `appsettings.json`:

```json
{
  "SecretWebhook": "your_secret",
  "DiscordWebhookUrl": "your_discord_webhook_url",
  "DiscordMessageJson": "{ \"embeds\": [ { \"title\": \"\" } ] }"
}
```

I suggest you use the following [website](https://message.style/app/editor) to design an embed message for discord.

Export your design as JSON

## How to Run

To run this project, Docker is the recommended method. Use the provided Dockerfile to build the image and deploy it in a container.

Make sure the API endpoint is secured with SSL and accessible on port 443, as this is required for Twitch to send events to your application.

For more information on handling Twitch webhooks, refer to the [Twitch API documentation](https://dev.twitch.tv/docs/eventsub/handling-webhook-events).

Once the application is running in a Docker container, you can subscribe to the [channel.update](https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types/#channelupdate) and [stream.online](https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types/#streamonline) events by sending a POST request to the Twitch API.

