using UnifiedPos;
using UnifiedPos.Commands;
using UnifiedPos.Transports;

// Use the exact friendly name shown in Windows Settings > Printers & scanners.
using var printer = new UnifiedPrinter(
    new WindowsPrintQueueTransport("EPSON TM-T88VII Receipt"));

await printer.PrintAsync(PrintCompleteReceipt);
Console.WriteLine("Complete receipt sent through the Windows printer queue.");

static void PrintCompleteReceipt(Receipt ticket) => ticket
    .AlignCenter().Bold().TextSize(2, 2).TextLine("COFFEE HOUSE")
    .Normal().AlignCenter()
    .TextLine("RNC 123-45678-9")
    .TextLine("Av. Principal #100")
    .TextLine("Tel. 809-555-0100")
    .TextLine("------------------------------------------")
    .AlignLeft()
    .TextLine("FACTURA: B0200000123")
    .TextLine("FECHA: 2026-09-09  10:30 AM")
    .TextLine("CAJERO: ADMIN")
    .TextLine("------------------------------------------")
    .TextLine("2  Cafe latte              RD$ 300.00")
    .TextLine("1  Croissant               RD$ 175.00")
    .TextLine("------------------------------------------")
    .TextLine("SUBTOTAL                   RD$ 402.54")
    .TextLine("ITBIS 18%                  RD$  72.46")
    .Bold().TextSize(2, 2).TextLine("TOTAL RD$475.00")
    .Normal().AlignCenter()
    .QrCode("https://example.com/receipt/B0200000123")
    .TextLine("Gracias por su compra")
    .Feed(3).Cut(CutMode.Partial);
