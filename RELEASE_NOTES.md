# Fluent TLS Tunnel v0.1.4

## Fixes
- Fix the app staying in the background after the window is closed. Closing now disconnects, restores Windows routing and fully exits.
- Fix the installer being unable to close a running copy of the app. It now stops the app before installing or uninstalling.
- Fix white borders around the tray right-click menu. The menu now has a clean rounded edge in light and dark themes.

## Downloads
- `Fluent-TLS-Tunnel-installer.exe`: bundled-runtime installer, recommended.
- `Fluent-TLS-Tunnel-portable.zip`: bundled-runtime portable app. Unzip and run `FluentTlsTunnel.exe`.
- `Fluent-TLS-Tunnel-runtime-dependent.exe`: smaller installer, requires .NET 8 Desktop Runtime x64.

## Compatibility
Windows x64. Automatic routing needs administrator approval. SSH dynamic forwarding carries TCP only; general UDP traffic is not supported, and the app does not register the native Windows VPN badge. The binaries are unsigned and may trigger a SmartScreen warning.
