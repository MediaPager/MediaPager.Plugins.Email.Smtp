# MediaPager.Plugins.Email.Smtp

Official SMTP email provider plugin for [MediaPager](https://github.com/MediaPager/MediaPager).
Ships with the app and is **loaded by default**.

Sends password resets and invitations through any standards-compliant SMTP server —
self-hosted Postfix, Postmark, Amazon SES, your ISP's relay, whatever has an endpoint.

## Settings

Defined by `IPluginSettingsSchema` and stored under
`Settings:Plugins:mediapager.email.smtp` (legacy `plugins.smtp.*` values are migrated):

| Key | Label | Notes |
| --- | --- | --- |
| `host` | SMTP host | required |
| `port` | SMTP port | defaults to `587` |
| `username` | SMTP username | optional (unauthenticated relays work) |
| `password` | SMTP password | secret |
| `from` | From address | required |
| `enableSsl` | Use SSL/TLS | defaults to `true`; port `465` connects SSL-on-connect, otherwise STARTTLS |

Configure them in **Settings → General → Email provider → settings** in the SPA. Environment
fallbacks use `MEDIAPAGER_Settings__Plugins__mediapager.email.smtp__<key>`. Values saved in the
UI are stored in the runtime database; all plugin setting values are protected at rest and
secret fields are never returned by the API.
