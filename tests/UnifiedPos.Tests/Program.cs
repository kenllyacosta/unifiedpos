using System.Text;
using UnifiedPos;
using UnifiedPos.Commands;
using UnifiedPos.Transports;

var tests = new (string Name, Action Run)[]
{
    ("Text", () => Contains(Build(r => r.Text("HELLO")), Encoding.ASCII.GetBytes("HELLO"))),
    ("Feed", () => Contains(Build(r => r.Feed(3)), new byte[] { 0x1B, 0x64, 3 })),
    ("Center", () => Contains(Build(r => r.AlignCenter()), new byte[] { 0x1B, 0x61, 1 })),
    ("Bold", () => Contains(Build(r => r.Bold()), new byte[] { 0x1B, 0x45, 1 })),
    ("Underline", () => Contains(Build(r => r.Underline()), new byte[] { 0x1B, 0x2D, 1 })),
    ("Text size", () => Contains(Build(r => r.TextSize(2, 3)), new byte[] { 0x1D, 0x21, 0x12 })),
    ("Partial cut", () => Contains(Build(r => r.Cut()), new byte[] { 0x1D, 0x56, 1 })),
    ("Drawer", () => Contains(Build(r => r.OpenDrawer()), new byte[] { 0x1B, 0x70, 0, 50, 250 })),
    ("QR", () => Contains(Build(r => r.QrCode("123")), new byte[] { 0x1D, 0x28, 0x6B })),
    ("Barcode", () => Contains(Build(r => r.Barcode("{B123")), new byte[] { 0x1D, 0x6B, 73, 5 })),
    ("Raw", () => Contains(Build(r => r.Raw(1, 2, 3)), new byte[] { 1, 2, 3 })),
    ("ePOS envelope", EposEnvelope),
    ("Unified printer", UnifiedApi)
};

var failed = 0;
foreach (var test in tests)
{
    try { test.Run(); Console.WriteLine("PASS " + test.Name); }
    catch (Exception ex) { failed++; Console.Error.WriteLine("FAIL " + test.Name + ": " + ex.Message); }
}
Console.WriteLine($"{tests.Length - failed}/{tests.Length} tests passed");
return failed;

static byte[] Build(Action<Receipt> configure)
{
    var receipt = new Receipt(); configure(receipt); return new EscPosEncoder().Encode(receipt);
}

static void Contains(byte[] actual, byte[] expected)
{
    for (var start = 0; start <= actual.Length - expected.Length; start++)
    {
        var match = true;
        for (var i = 0; i < expected.Length; i++) if (actual[start + i] != expected[i]) { match = false; break; }
        if (match) return;
    }
    throw new Exception("Expected byte sequence was not generated.");
}

static void EposEnvelope()
{
    var receipt = new Receipt().AlignCenter().Bold().TextLine("TEST").QrCode("123").Cut();
    using var transport = new EpsonEposTransport("https://10.0.0.127");
    var xml = transport.BuildEnvelope(receipt);
    foreach (var value in new[] { "Envelope", "epos-print", "align=\"center\"", "em=\"true\"", "qrcode_model_2", "TEST" })
        if (!xml.Contains(value, StringComparison.Ordinal)) throw new Exception("Missing XML value: " + value);
}

static void UnifiedApi()
{
    using var transport = new CaptureTransport();
    using var printer = new UnifiedPrinter(transport);
    printer.PrintAsync(r => r.TextLine("OK").Cut()).GetAwaiter().GetResult();
    if (transport.Last == null || transport.Last.Commands.Count != 2) throw new Exception("Receipt was not sent through the transport.");
}

sealed class CaptureTransport : IPrinterTransport
{
    public Receipt? Last { get; private set; }
    public Task SendAsync(Receipt receipt, CancellationToken cancellationToken = default) { Last = receipt; return Task.CompletedTask; }
    public void Dispose() { }
}
