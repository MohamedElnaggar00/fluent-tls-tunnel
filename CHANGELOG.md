# Changelog

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
