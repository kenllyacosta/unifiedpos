using UnifiedPos;
using UnifiedPos.Transports;

// Choose exactly one transport:
IPrinterTransport transport = new Tcp9100Transport("10.0.0.127");
// IPrinterTransport transport = new EpsonEposTransport("https://10.0.0.127", "local_printer");
// IPrinterTransport transport = new WindowsPrintQueueTransport("EPSON TM-T88VII Receipt");

using var printer = new UnifiedPrinter(transport);
await printer.PrintAsync(ticket => ticket
    .AlignCenter()
    .Bold()
    .TextSize(2, 2)
    .TextLine("UNIFIEDPOS")
    .Normal()
    .TextLine("Ticket de prueba")
    .QrCode("https://github.com/kenllyacosta/unifiedpos")
    .Feed(3)
    .Cut());

Console.WriteLine("Trabajo enviado.");
