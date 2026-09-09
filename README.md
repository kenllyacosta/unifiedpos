# UnifiedPos

Unified .NET API for receipt printers using Epson ePOS over HTTPS, ESC/POS over TCP 9100, or a Windows printer queue by its friendly name.

## Compatibility

The library targets `netstandard2.0` and `net8.0`. The ePOS and TCP transports are cross-platform. `WindowsPrintQueueTransport` sends RAW ESC/POS bytes through the Windows spooler and therefore runs only on Windows.

For a USB printer, install it normally in Windows and pass its visible printer name. Vendor ID and Product ID are not required. Complete, executable examples are available for all three transports:

- [`examples/EposSample`](examples/EposSample) — Epson ePOS HTTPS/XML.
- [`examples/Tcp9100Sample`](examples/Tcp9100Sample) — direct ESC/POS on port 9100.
- [`examples/WindowsQueueSample`](examples/WindowsQueueSample) — USB or another installed Windows queue by friendly name.

### USB / Windows printer name

```csharp
using UnifiedPos;
using UnifiedPos.Transports;

using var printer = new UnifiedPrinter(
    new WindowsPrintQueueTransport("EPSON TM-T88VII Receipt"));

await printer.PrintAsync(ticket => ticket
    .AlignCenter()
    .Bold()
    .TextLine("MI NEGOCIO")
    .Normal()
    .QrCode("https://example.com")
    .Feed(3)
    .Cut());
```

The configured Windows queue must accept RAW data and the device must understand ESC/POS. The printer name must match Windows exactly.

## TCP 9100

```csharp
using var printer = new UnifiedPrinter(
    new Tcp9100Transport("10.0.0.127", 9100));
```

## Epson ePOS

```csharp
using var printer = new UnifiedPrinter(
    new EpsonEposTransport("https://10.0.0.127", "local_printer"));
```

The printer certificate must be trusted by the machine. Do not disable TLS certificate validation in production.

## Commands in 0.1.0

Text, lines, feed, alignment, bold, underline, text size, QR, Code 128 barcode, cut, cash drawer pulse and raw commands.

## Build and test

```powershell
dotnet build UnifiedPos.slnx --configuration Release
dotnet run --project tests/UnifiedPos.Tests --configuration Release
dotnet pack src/UnifiedPos/UnifiedPos.csproj --configuration Release
```

## Automated NuGet publishing

The `Publish NuGet` GitHub Action runs when a GitHub Release is published. Add a repository secret named `NUGET_API_KEY`, then publish a release whose tag follows `v1.2.3`. The workflow builds, packs and pushes that version to NuGet.org.

## Contributing

Contributions are welcome. Bug reports, printer compatibility results, documentation improvements and pull requests are appreciated. Read [CONTRIBUTING.md](CONTRIBUTING.md) before submitting a change.

## License

MIT
