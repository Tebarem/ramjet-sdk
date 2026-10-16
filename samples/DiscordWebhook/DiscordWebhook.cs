using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DiscordWebhook;

/// <summary>The settings of the DiscordWebhook plugin (<c>plugins/discordwebhook/config.json</c>, written with these defaults on the first run).
/// The webhook URL is a secret: anyone who has it can post to the channel.</summary>
public sealed class DiscordWebhookConfig
{
    /// <summary>The Discord webhook URL (https://discord.com/api/webhooks/...). Empty = the plugin posts nothing.</summary>
    public string WebhookUrl { get; set; } = "";

    /// <summary>The line posted when a player joins ({player} is the name). Empty = no line.</summary>
    public string JoinMessage { get; set; } = "{player} joined the server";

    /// <summary>The line posted when a player leaves ({player} is the name). Empty = no line.</summary>
    public string QuitMessage { get; set; } = "{player} left the server";

    /// <summary>The name the webhook posts under.</summary>
    public string Username { get; set; } = "Ramjet";
}

[JsonSerializable(typeof(DiscordWebhookConfig))]
internal sealed partial class DiscordWebhookJson : JsonSerializerContext;

/// <summary>Sample: posts join and quit messages to a Discord webhook. It shows the plugin HTTP client: <c>Http.Client</c> sends the request
/// through the server (the server resolves the name, refuses private, loopback and metadata addresses, and applies the limits of
/// <c>scripting.http</c> in ramjet.toml). The answer returns on the tick thread; nothing here blocks the tick.</summary>
[PluginInfo("DiscordWebhook", Version = "1.0.0", Author = "Ramjet", Description = "Join and quit messages to a Discord webhook")]
public sealed class DiscordWebhookPlugin : Plugin
{
    private Config<DiscordWebhookConfig> _config = null!;

    protected override void OnEnable()
    {
        _config = Config.Load(DiscordWebhookJson.Default.DiscordWebhookConfig);

        Events.On((ref PlayerJoinEvent e) => Post(_config.Value.JoinMessage, e.Username.ToString()));
        Events.On((ref PlayerQuitEvent e) => Post(_config.Value.QuitMessage, e.Username.ToString()));
    }

    /// <summary>Starts one post. Fire and forget: the task is not awaited by the tick, and its failure is logged by SendAsync.</summary>
    private void Post(string template, string player)
    {
        var config = _config.Value;
        if (config.WebhookUrl.Length == 0 || template.Length == 0) return;
        var payload = new JsonObject
        {
            ["content"] = template.Replace("{player}", player),
            ["username"] = config.Username,
        };
        _ = SendAsync(config.WebhookUrl, payload.ToJsonString());
    }

    private async Task SendAsync(string url, string json)
    {
        try
        {
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            using HttpResponseMessage response = await Http.Client.PostAsync(url, content, cts.Token);
            // the webhook URL is not logged (it is the secret); the status is enough to diagnose
            if (!response.IsSuccessStatusCode) Log.Warn($"the Discord webhook answered {(int)response.StatusCode}");
        }
        catch (Exception e)
        {
            Log.Warn($"the Discord webhook post failed: {e.Message}");
        }
    }
}
