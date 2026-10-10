<div align="center">

# Fluent TLS Tunnel

**A private path. One simple connection.**

Windows 11 · WinUI 3 · SSH over TLS · SNI · SOCKS5

English | [العربية](#العربية)

brought to you by [Instinct](https://instinct.com)

</div>

## What it does

Fluent TLS Tunnel puts an SSH connection inside a TLS connection with your chosen SNI hostname. A local SOCKS5 listener lets compatible apps use that connection. No stunnel, PuTTY or Docker installation is needed.

- Native Fluent interface with Mica, light/dark/system themes.
- English and formal Arabic, with mirrored right-to-left layout.
- Server, port, SNI and SSH credentials in one place.
- One button to connect, cancel or disconnect.
- Local SOCKS5 on `127.0.0.1:1080`, with a configurable port.
- Optional custom DNS for a selected Windows network adapter.
- Save profiles locally; import/export `.ftlsc` files (JSON inside) without passwords or trusted host keys.
- TLS certificate validation and SSH SHA-256 host-key checks.
- Optional password-encrypted exports: AES-256-GCM, PBKDF2-SHA256 (600,000 iterations), random salt and nonce.
- Connection status and an in-memory log.

## Connect

1. Enter your SSH-over-TLS server address, port, SNI hostname and SSH username/password.
2. Leave TLS certificate verification enabled when the provider supports it. Some tunneling services use mismatched certificates and require it disabled. This reduces TLS protection: check the SSH fingerprint with your provider.
3. Click **Connect**. On first use, verify the SSH host-key fingerprint before trusting it. A changed key needs a fresh decision.
4. Set a SOCKS-aware app, such as Firefox, to SOCKS5 `127.0.0.1`, port `1080` (or your chosen port).
5. Click **Disconnect** when finished.

**This is not a whole-device VPN.** It does not install a network driver or redirect every Windows app. Traffic from apps that do not use SOCKS remains outside the tunnel. There is no kill switch. UDP is not supported. Use only servers and networks you are permitted to use.

## Custom DNS

Choose your network adapter, enter DNS IP addresses separated by commas, and enable custom DNS before connecting. Windows requests administrator approval. Existing static/DHCP settings are backed up before the change and restored on disconnect or normal exit. If the app crashes or approval to restore is declined, use **Restore DNS** on the next start. Do not delete a pending DNS backup.

This changes the selected Windows adapter's DNS, not DNS resolution performed by the remote SSH server. For Firefox, enabling "Proxy DNS when using SOCKS v5" sends hostname resolution to the SSH server instead of your local adapter DNS. Choose the behavior that fits your needs.

## Privacy & security

Passwords are kept in memory only and cleared from the field after connection. They are never written to profiles, exports, logs or process command lines. Profiles contain server addresses, usernames and DNS settings: treat exports as private information. Importing never connects automatically or imports trust records. No telemetry is included.

## Requirements

Windows 11 x64 is recommended. The framework-dependent build needs .NET 8 Desktop Runtime. Downloadable releases will appear after the first preview is approved. This project is under development, not a stable release.

## Credits

Developed by Mohamed Elnaggar. brought to you by [Instinct](https://instinct.com).

MIT-licensed application. SSH uses [SSH.NET](https://github.com/sshnet/SSH.NET) (MIT); TLS uses .NET's SslStream. Third-party notices are included.

---

## العربية

**مسار خاص. اتصال بسيط.**

يغلّف Fluent TLS Tunnel اتصال SSH داخل TLS، مع اسم SNI تحدده بنفسك. يوفر SOCKS5 محليًا للتطبيقات المتوافقة دون تثبيت برامج إضافية.

### المزايا

- واجهة ويندوز أصلية بتصميم Fluent وخلفية Mica.
- العربية الفصحى والإنجليزية، مع عكس تخطيط الواجهة في العربية.
- زر واحد للاتصال والإلغاء وقطع الاتصال.
- حفظ الإعدادات واستيرادها وتصديرها بامتداد `.ftlsc` ومحتوى JSON دون كلمات مرور أو مفاتيح موثوقة.
- SOCKS5 محلي بمنفذ قابل للتغيير، وسجل لحالة الاتصال.
- DNS مخصص لمحول شبكة تختاره، مع حفظ الإعدادات السابقة واستعادتها.
- التحقق من شهادة TLS وبصمة مفتاح خادم SSH.

### الاستخدام

أدخل عنوان الخادم والمنفذ وSNI وبيانات حساب SSH، ثم اضغط **اتصال**. تحقق من بصمة مفتاح الخادم لدى مزود الخدمة قبل الوثوق به. اضبط التطبيق الذي تريد توجيهه على SOCKS5 في `127.0.0.1:1080`، ثم اضغط **قطع الاتصال** عند الانتهاء.

ليست شبكة VPN للجهاز بالكامل. لا تتغير حركة التطبيقات التي لا تستخدم SOCKS، ولا توجد خاصية منع التسرب عند انقطاع الاتصال. لا يُدعم UDP.

يتطلب تغيير DNS موافقة مسؤول ويندوز. تُحفظ إعدادات المحول السابقة قبل تغييرها، وتُستعاد عند قطع الاتصال أو الخروج الطبيعي. بعد تعطل التطبيق أو رفض موافقة الاستعادة، استخدم **استعادة DNS** عند فتحه مجددًا. يغيّر ذلك DNS المحلي، لا DNS الذي يستخدمه خادم SSH البعيد.

تُحفظ كلمات المرور في الذاكرة فقط. تحتوي ملفات الإعدادات على عناوين الخوادم وأسماء المستخدمين؛ تعامل معها بوصفها معلومات خاصة. المشروع قيد التطوير، ولا يُعد إصدارًا مستقرًا بعد.

تطوير محمد النجار. مقدم إليكم من [Instinct](https://instinct.com).
