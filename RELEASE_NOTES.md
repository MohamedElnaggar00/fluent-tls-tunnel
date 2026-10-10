# Fluent TLS Tunnel v0.1.5

## Fixes
- Fix Windows-wide routing stopping with "Find-NetRoute: The requested object could not be found". If Windows cannot find the endpoint route, use the available default route with the lowest combined route and interface metric.
- Make DNS-rule cleanup safe when there are no matching rules.

- Fix a second desktop launch not reopening the current session. It now restores and focuses the running window and exits the second process cleanly.
- Ensure normal shutdown cannot leave a hidden process after the window and tray icon close.

## Downloads
- `Fluent-TLS-Tunnel-installer.exe`: bundled-runtime installer, recommended.
- `Fluent-TLS-Tunnel-portable.zip`: bundled-runtime portable app. Unzip and run `FluentTlsTunnel.exe`.
- `Fluent-TLS-Tunnel-runtime-dependent.exe`: smaller installer, requires .NET 8 Desktop Runtime x64.

## Compatibility
Windows x64. Automatic routing needs administrator approval. SSH dynamic forwarding carries TCP only; general UDP traffic is not supported, and the app does not register the native Windows VPN badge. The binaries are unsigned and may trigger a SmartScreen warning.
