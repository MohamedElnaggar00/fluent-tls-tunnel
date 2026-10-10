## English

Update to the Fluent TLS Tunnel preview for Windows x64. Still a **pre-release**.

### Downloads
- **Fluent-TLS-Tunnel-installer.exe**: recommended. Includes the .NET runtime; installs per user without an administrator requirement.
- **Fluent-TLS-Tunnel-portable.zip**: unzip and run FluentTlsTunnel.exe. Includes the runtime.
- **Fluent-TLS-Tunnel-runtime-dependent.exe**: smaller installer; requires the .NET 8 Desktop Runtime (x64).

### Changes
- The connected state now matches the Windows 11 Settings style: a compact card with a status check, host and SOCKS5 address, and the connection timer.
- The Connect/Disconnect button uses the standard WinUI 3 accent style and default size instead of the large custom green button.
- The status banner appears only when there is a message or an error.
- The Save profile button uses the standard button height.

Everything else is unchanged from v0.1.0, including the notes: this is a SOCKS5 tunnel, not a full-device VPN; custom DNS has not yet been runtime-tested on a real adapter; the binaries are unsigned, so Windows may show a SmartScreen warning.

## العربية

تحديث للنسخة التجريبية من Fluent TLS Tunnel لويندوز بمعمارية x64. ما زال الإصدار **pre-release**.

### التنزيلات
- **Fluent-TLS-Tunnel-installer.exe**: الخيار الموصى به. يتضمن بيئة .NET ويُثبَّت لحساب المستخدم دون اشتراط صلاحية المسؤول.
- **Fluent-TLS-Tunnel-portable.zip**: فك الضغط وشغّل FluentTlsTunnel.exe. يتضمن بيئة التشغيل.
- **Fluent-TLS-Tunnel-runtime-dependent.exe**: مُثبّت أصغر يحتاج .NET 8 Desktop Runtime بمعمارية x64.

### التغييرات
- أصبحت حالة الاتصال بنفس نمط تطبيق الإعدادات في ويندوز 11: بطاقة مدمجة فيها علامة الحالة وعنوان الخادم وSOCKS5 وعداد مدة الاتصال.
- زر الاتصال وقطع الاتصال يستخدم نمط WinUI 3 القياسي وحجمه الافتراضي بدل الزر الأخضر الكبير المخصص.
- يظهر شريط الحالة فقط عند وجود رسالة أو خطأ.
- زر حفظ الإعدادات بالارتفاع القياسي.

بقية المزايا والملاحظات كما في v0.1.0: الأداة نفق SOCKS5 وليست VPN للجهاز بالكامل، ولم يُختبر DNS المخصص بعد على محول فعلي، والملفات غير موقّعة رقميًا وقد يظهر تحذير SmartScreen.

brought to you by [Instinct](https://instinct.com)
