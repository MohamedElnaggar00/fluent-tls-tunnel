# Changelog

## v0.2.1
- Fixed browsing failing with connection-closed errors while connected: DNS answers now always stay on IPv4 (AAAA queries receive an empty answer), keeping the IPv6 routes inside the tunnel so no IPv6 traffic leaks.
- After routing is applied, the app tests HTTPS by hostname; if browsing does not work, routing is restored automatically and a clear message is shown.
- Improved restoring the window from the tray on a second launch.
- The connection log now reports a rate-limited summary of SOCKS channel failures.
- Updated the single-instance test to match closing to the tray.
- Three bundled-runtime downloads: portable, installer and full offline kit.

### العربية
- إصلاح فشل تصفح المواقع برسالة إغلاق الاتصال أثناء الاتصال: أصبحت إجابات DNS تقتصر دائمًا على IPv4 (تتلقى استعلامات AAAA إجابة فارغة)، مع إبقاء مسارات IPv6 داخل النفق حتى لا تتسرب أي حركة IPv6.
- بعد تطبيق التوجيه يختبر التطبيق اتصال HTTPS بالاسم؛ وإذا لم يعمل التصفح تُستعاد إعدادات التوجيه تلقائيًا وتظهر رسالة واضحة.
- تحسين استعادة النافذة من منطقة الإشعارات عند تشغيل التطبيق مرة ثانية.
- يعرض سجل الاتصال الآن ملخصًا محدود التكرار لإخفاقات قنوات SOCKS.
- تحديث اختبار النسخة الواحدة ليتوافق مع الإغلاق إلى منطقة الإشعارات.
- ثلاثة خيارات تنزيل تتضمن ملفات التشغيل المطلوبة: محمول ومثبت وحزمة كاملة دون اتصال.

## v0.2.0
- Added pooled HTTPS DNS queries through the tunnel, with fallback across Cloudflare resolvers, retries and short-lived caching.
- Enlarged and widened the connection log and added a Clear log button.
- Closing the window now minimizes the app to the system tray; the tray menu has a Quit command.
- Encrypted .ftlsc imports hide values in both the confirmation dialog and settings fields. The real values remain in use; typing into a field replaces its hidden value.
- Three bundled-runtime downloads: portable, installer and full offline kit.

### العربية
- إضافة استعلامات DNS عبر اتصال HTTPS مُعاد الاستخدام عبر النفق، مع التبديل بين خوادم Cloudflare وإعادة المحاولة والتخزين المؤقت لفترة قصيرة.
- تكبير سجل الاتصال وتوسيعه وإضافة زر لمسح السجل.
- يؤدي إغلاق النافذة الآن إلى تصغير التطبيق إلى منطقة الإشعارات؛ وتتضمن قائمتها أمر «إنهاء».
- تُخفى القيم عند استيراد ملفات .ftlsc المشفرة في نافذة التأكيد وخانات الإعدادات. وتظل القيم الفعلية مستخدمة، بينما يؤدي الكتابة في أي خانة إلى استبدال قيمتها المخفية.
- ثلاثة خيارات تنزيل تتضمن ملفات التشغيل المطلوبة: محمول ومثبت وحزمة كاملة دون اتصال.

## v0.1.9
- Fixed automatic routing being blocked by ordinary Windows DNS policies for specific domains (for example Windows DNS resiliency rules).
- Remove only this app's own stale DNS rules at startup; rules owned by Windows or other software are never modified.
- Stop only when another catch-all DNS policy is active, and name that policy in the message.
- Three bundled-runtime downloads: portable, installer and full offline kit.

### العربية
- إصلاح توقف التوجيه التلقائي بسبب سياسات DNS العادية في ويندوز الخاصة بنطاقات محددة (مثل قواعد مرونة DNS في ويندوز).
- إزالة قواعد DNS المتبقية الخاصة بالتطبيق وحدها عند البدء، دون تعديل القواعد التي تعود إلى ويندوز أو برامج أخرى.
- الإيقاف فقط عند وجود سياسة DNS شاملة أخرى فعالة، مع ذكر اسمها في الرسالة.
- ثلاثة خيارات تنزيل تتضمن ملفات التشغيل المطلوبة: محمول ومثبت وحزمة كاملة دون اتصال.

## v0.1.8
- Fixed connection failure when the Windows credential vault is unavailable.
- Store passwords with current-user Windows DPAPI encryption, keeping them out of profile/export files.
- Continue connecting with the entered password if secure persistence is unavailable.
- Three bundled-runtime downloads: portable, installer and full offline kit.

### العربية
- إصلاح تعطل الاتصال عند عدم توفر مخزن بيانات اعتماد ويندوز.
- حفظ كلمات المرور بتشفير DPAPI الخاص بالمستخدم الحالي دون تضمينها في ملفات الإعدادات والتصدير.
- استمرار الاتصال بكلمة المرور المدخلة إذا تعذر حفظها بأمان.
- ثلاث خيارات تنزيل تتضمن ملفات التشغيل المطلوبة: محمول ومثبت وحزمة كاملة دون اتصال.
