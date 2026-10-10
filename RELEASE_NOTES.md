# Fluent TLS Tunnel v0.1.2

## Features and fixes
- Automatic Windows-wide TCP routing through the SSH-over-TLS tunnel, with IPv4 and IPv6 support and DNS carried through SSH.
- Upstream TLS endpoint bypass to keep the tunnel connection outside its own routes.
- Restore the original routes and DNS policy on disconnect, with an owner-process watchdog and recovery journal for interrupted sessions.
- System tray menu with Connect/Disconnect, Settings, About and Close. Closing the main window keeps the app in the tray.
- Save SSH passwords securely with Windows Credential Manager.
- Larger Fluent window with a labeled navigation rail, grouped settings, an inline connection profile and a collapsible connection log.
- Mica, Mica Alt and Acrylic background options. Arabic layouts use left-side window caption buttons.
- Separate primary and optional secondary DNS address inputs.
- Manual inline update checks in Settings and About, including pre-releases, with download links opened only by an explicit click.
- TLS certificate verification is unchecked for new profiles; the recommendation and protection warning remain visible.

## Downloads
- `Fluent-TLS-Tunnel-installer.exe`: bundled-runtime installer, recommended.
- `Fluent-TLS-Tunnel-portable.zip`: bundled-runtime portable app. Unzip and run `FluentTlsTunnel.exe`.
- `Fluent-TLS-Tunnel-runtime-dependent.exe`: smaller installer, requires .NET 8 Desktop Runtime x64.

## Compatibility
Windows x64. Automatic routing needs administrator approval. SSH dynamic forwarding carries TCP only; general UDP traffic is not supported, and the app does not register the native Windows VPN badge. The binaries are unsigned and may trigger a SmartScreen warning.
