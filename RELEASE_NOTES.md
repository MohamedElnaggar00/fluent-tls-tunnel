## English

First public preview of Fluent TLS Tunnel for Windows x64.

### Downloads
- **Fluent-TLS-Tunnel-installer.exe**: recommended. Includes the .NET runtime; installs per user without an administrator requirement.
- **Fluent-TLS-Tunnel-portable.zip**: unzip and run FluentTlsTunnel.exe. Includes the runtime.
- **Fluent-TLS-Tunnel-runtime-dependent.exe**: smaller installer; requires the .NET 8 Desktop Runtime (x64).

### Included
- Native WinUI 3 interface with Mica, NavigationView, Connection, Settings and About.
- English and Arabic, mirrored RTL, and system/light/dark appearance.
- SSH over TLS with SNI, a loopback SOCKS5 proxy, one Connect/Cancel/Disconnect button and a connection timer.
- TLS certificate verification, SSH host-key review and saved host-key trust.
- Local profiles and .ftlsc import/export, with optional password-based encryption. SSH passwords are not saved or exported.
- Optional custom adapter DNS, prefilled with 1.1.1.1 and 1.0.0.1, backup and Restore DNS.
- Approved globe, key and check app icon.

### Preview notes
This is a **pre-release**, not a stable build. It is a SOCKS5 tunnel, not a full-device VPN; configure compatible apps to use 127.0.0.1:1080 (or your chosen port). Custom DNS needs Windows administrator approval and has not yet been runtime-tested on a real adapter. Your own SSH service still needs a connection test. TLS verification is on by default; turning it off weakens protection. The binaries are unsigned, so Windows may show a SmartScreen warning. No automatic reconnect or system tray controls in this preview.

Windows build, TLS/SNI and encryption tests passed. The bundled installer passed install, app launch, normal exit and uninstall checks. Screenshots of a connected state are UI previews, not proof of a live server connection.

## العربية

أول نسخة تجريبية عامة من Fluent TLS Tunnel لويندوز بمعمارية x64.

### التنزيلات
- **Fluent-TLS-Tunnel-installer.exe**: الخيار الموصى به. يتضمن بيئة .NET ويُثبَّت لحساب المستخدم دون اشتراط صلاحية المسؤول.
- **Fluent-TLS-Tunnel-portable.zip**: فك الضغط وشغّل FluentTlsTunnel.exe. يتضمن بيئة التشغيل.
- **Fluent-TLS-Tunnel-runtime-dependent.exe**: مُثبّت أصغر يحتاج .NET 8 Desktop Runtime بمعمارية x64.

### المزايا
- واجهة WinUI 3 أصلية مع Mica وNavigationView وصفحات الاتصال والإعدادات وحول التطبيق.
- العربية والإنجليزية، واتجاه RTL، والمظهر الفاتح والداكن والتلقائي.
- SSH عبر TLS مع SNI ووكيل SOCKS5 محلي وزر واحد للاتصال والإلغاء وقطع الاتصال، مع عداد للمدة.
- التحقق من شهادة TLS ومراجعة مفتاح خادم SSH وحفظ الثقة به.
- حفظ الإعدادات واستيراد وتصدير ملفات .ftlsc مع تشفير اختياري. لا تُحفظ كلمة مرور SSH ولا تُصدَّر.
- DNS مخصص اختياري للمحول، بقيم افتراضية 1.1.1.1 و1.0.0.1، مع نسخة احتياطية وخيار استعادة DNS.
- الأيقونة المعتمدة: كرة أرضية ومفتاح وعلامة صح.

### ملاحظات النسخة التجريبية
هذا إصدار **pre-release** وليس إصدارًا مستقرًا. يعمل كنفق SOCKS5، وليس VPN للجهاز بالكامل. اضبط التطبيقات المتوافقة على 127.0.0.1:1080 أو المنفذ الذي تختاره. تغيير DNS يحتاج موافقة مسؤول ويندوز ولم يُختبر بعد على محول فعلي. يلزم اختبار الاتصال بخدمة SSH الخاصة بك. التحقق من TLS مفعّل افتراضيًا، وتعطيله يضعف الحماية. الملفات غير موقّعة رقميًا، وقد يظهر تحذير SmartScreen. لا تتضمن هذه النسخة إعادة اتصال تلقائية أو تحكمًا من شريط النظام.

نجح بناء ويندوز واختبارات TLS وSNI والتشفير. نجح اختبار تثبيت النسخة المضمّنة وتشغيل التطبيق وإغلاقه وإزالته. صور حالة الاتصال معاينة للواجهة وليست دليلًا على اتصال بخادم فعلي.

brought to you by [Instinct](https://instinct.com)
