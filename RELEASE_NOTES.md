# Fluent TLS Tunnel v0.1.3

## Fixes
- Fix the runtime-dependent installer reporting that the .NET 8 Desktop Runtime is missing when it is installed. Detection now checks the 64-bit and 32-bit registry views and the standard install folders, and offers to continue instead of blocking if the runtime cannot be detected.
- Fix Connect failing with "The requested object could not be found" on systems where IPv6 is disabled or unavailable. Routing now continues with IPv4 when the IPv6 setup is unavailable, and routing errors now name the failing Windows step.

## Downloads
- `Fluent-TLS-Tunnel-installer.exe`: bundled-runtime installer, recommended.
- `Fluent-TLS-Tunnel-portable.zip`: bundled-runtime portable app. Unzip and run `FluentTlsTunnel.exe`.
- `Fluent-TLS-Tunnel-runtime-dependent.exe`: smaller installer, requires .NET 8 Desktop Runtime x64.

## Compatibility
Windows x64. Automatic routing needs administrator approval. SSH dynamic forwarding carries TCP only; general UDP traffic is not supported, and the app does not register the native Windows VPN badge. The binaries are unsigned and may trigger a SmartScreen warning.
