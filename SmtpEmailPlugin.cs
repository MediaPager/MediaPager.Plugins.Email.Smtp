using MailKit.Net.Smtp;
using MediaPager.App.PluginContracts;
using MimeKit;

namespace MediaPager.Plugins.Email.Smtp;

/// <summary>
/// Official SMTP email provider: delivers through any standards-compliant SMTP server
/// (self-hosted, Postmark, SES, your ISP — anything with an endpoint). Settings live under
/// plugins.smtp.* and flow through the SDK's IPluginSettingsStore; config is the fallback.
/// Settings are read per call so a runtime update takes effect on the next send.
/// </summary>
public sealed class SmtpEmailPlugin(IPluginSettingsStore settingsStore) : IMediaPagerPlugin, IPluginSettingsSchema, IEmailProviderPlugin
{
    public const string PluginKey = "smtp";
    public const string HostSetting = "host";
    public const string PortSetting = "port";
    public const string UsernameSetting = "username";
    public const string PasswordSetting = "password";
    public const string FromSetting = "from";
    public const string EnableSslSetting = "enableSsl";

    public const int DefaultPort = 587;

    public PluginDescriptor Descriptor { get; } = new(
        Id: "mediapager.email.smtp",
        Name: "SMTP",
        Version: "0.1.0",
        Author: "MediaPager",
        Description: "Send email through any standards-compliant SMTP server.");

    public IReadOnlyList<PluginSettingDefinition> Settings { get; } =
    [
        new PluginSettingDefinition(HostSetting, "SMTP host", PluginSettingType.String, Required: true),
        new PluginSettingDefinition(PortSetting, "SMTP port", PluginSettingType.String, Default: DefaultPort.ToString()),
        new PluginSettingDefinition(UsernameSetting, "SMTP username", PluginSettingType.String),
        new PluginSettingDefinition(PasswordSetting, "SMTP password", PluginSettingType.Password, Secret: true),
        new PluginSettingDefinition(FromSetting, "From address", PluginSettingType.String, Required: true),
        new PluginSettingDefinition(EnableSslSetting, "Use SSL/TLS (true/false)", PluginSettingType.String, Default: "true"),
    ];

    public async Task<bool> IsConfiguredAsync(CancellationToken cancellationToken = default) =>
        !string.IsNullOrWhiteSpace(await settingsStore.GetAsync(PluginKey, HostSetting, cancellationToken)) &&
        !string.IsNullOrWhiteSpace(await settingsStore.GetAsync(PluginKey, FromSetting, cancellationToken));

    public async Task<bool> SendAsync(string recipient, string subject, string text, string html,
        CancellationToken cancellationToken = default)
    {
        var host = await settingsStore.GetAsync(PluginKey, HostSetting, cancellationToken);
        var from = await settingsStore.GetAsync(PluginKey, FromSetting, cancellationToken);
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from)) return false;

        var portText = await settingsStore.GetAsync(PluginKey, PortSetting, cancellationToken);
        var port = int.TryParse(portText, out var parsed) ? parsed : DefaultPort;
        var username = await settingsStore.GetAsync(PluginKey, UsernameSetting, cancellationToken);
        var password = await settingsStore.GetAsync(PluginKey, PasswordSetting, cancellationToken);
        var sslText = await settingsStore.GetAsync(PluginKey, EnableSslSetting, cancellationToken);
        var enableSsl = string.IsNullOrWhiteSpace(sslText) || !string.Equals(sslText, "false", StringComparison.OrdinalIgnoreCase);

        try
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(from));
            message.To.Add(MailboxAddress.Parse(recipient));
            message.Subject = subject;
            message.Body = new BodyBuilder { TextBody = text, HtmlBody = html }.ToMessageBody();

            using var client = new SmtpClient();
            var secure = enableSsl
                ? (port == 465 ? MailKit.Security.SecureSocketOptions.SslOnConnect : MailKit.Security.SecureSocketOptions.StartTls)
                : MailKit.Security.SecureSocketOptions.Auto;
            await client.ConnectAsync(host, port, secure, cancellationToken);
            if (!string.IsNullOrWhiteSpace(username))
                await client.AuthenticateAsync(username, password ?? "", cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
            return true;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            Console.Error.WriteLine($"[email] SMTP send failed via {host}:{port}: {exception.Message}");
            return false;
        }
    }
}
