# MediaPager.Plugins.Email.Smtp

Official SMTP email provider plugin for [MediaPager](https://github.com/MediaPager/MediaPager).
Ships with the app and is **loaded by default**.

Sends password resets and invitations through any standards-compliant SMTP server —
self-hosted Postfix, Postmark, Amazon SES, your ISP's relay, whatever has an endpoint.

## Settings

Defined by `IPluginSettingsSchema` and stored under `plugins.smtp.*`:

| Key | Label | Notes |
| --- | --- | --- |
| `host` | SMTP host | required |
| `port` | SMTP port | defaults to `587` |
| `username` | SMTP username | optional (unauthenticated relays work) |
| `password` | SMTP password | secret |
| `from` | From address | required |
| `enableSsl` | Use SSL/TLS | defaults to `true`; port `465` connects SSL-on-connect, otherwise STARTTLS |

Configure them in **Settings → Email** in the SPA.
