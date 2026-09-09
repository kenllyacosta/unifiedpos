using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using UnifiedPos.Commands;
using UnifiedPos;

namespace UnifiedPos.Transports
{
    public sealed class EpsonEposTransport : IPrinterTransport
    {
        private const string EposNamespace = "http://www.epson-pos.com/schemas/2011/03/epos-print";
        private readonly HttpClient _client;
        private readonly bool _ownsClient;
        private readonly Uri _endpoint;

        public EpsonEposTransport(string printerBaseUrl, string deviceId = "local_printer", int timeoutMilliseconds = 10000, HttpClient client = null)
        {
            if (string.IsNullOrWhiteSpace(printerBaseUrl)) throw new ArgumentException("A printer URL is required.", nameof(printerBaseUrl));
            if (string.IsNullOrWhiteSpace(deviceId)) throw new ArgumentException("A device ID is required.", nameof(deviceId));
            var root = printerBaseUrl.TrimEnd('/');
            _endpoint = new Uri(root + "/cgi-bin/epos/service.cgi?devid=" + Uri.EscapeDataString(deviceId) + "&timeout=" + timeoutMilliseconds.ToString(CultureInfo.InvariantCulture));
            _client = client ?? new HttpClient();
            _ownsClient = client == null;
        }

        public async Task SendAsync(Receipt receipt, CancellationToken cancellationToken = default(CancellationToken))
        {
            var xml = BuildEnvelope(receipt);
            using (var content = new StringContent(xml, Encoding.UTF8, "text/xml"))
            using (var response = await _client.PostAsync(_endpoint, content, cancellationToken).ConfigureAwait(false))
            {
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!response.IsSuccessStatusCode) throw new HttpRequestException("ePOS HTTP error " + (int)response.StatusCode + ": " + body);
                if (body.IndexOf("success=\"false\"", StringComparison.OrdinalIgnoreCase) >= 0)
                    throw new InvalidOperationException("The Epson printer rejected the job: " + body);
            }
        }

        public string BuildEnvelope(Receipt receipt)
        {
            if (receipt == null) throw new ArgumentNullException(nameof(receipt));
            var settings = new XmlWriterSettings { Encoding = Encoding.UTF8, OmitXmlDeclaration = false, Indent = false };
            using (var buffer = new MemoryStream())
            {
                using (var writer = XmlWriter.Create(buffer, settings))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("s", "Envelope", "http://schemas.xmlsoap.org/soap/envelope/");
                    writer.WriteStartElement("s", "Body", "http://schemas.xmlsoap.org/soap/envelope/");
                    writer.WriteStartElement("epos-print", EposNamespace);
                    foreach (var command in receipt.Commands) WriteCommand(writer, command);
                    writer.WriteEndElement(); writer.WriteEndElement(); writer.WriteEndElement(); writer.WriteEndDocument();
                }
                return Encoding.UTF8.GetString(buffer.ToArray());
            }
        }

        private static void WriteCommand(XmlWriter writer, PrintCommand command)
        {
            var text = command as TextCommand;
            if (text != null) { writer.WriteElementString("text", EposNamespace, text.Value); return; }
            var feed = command as LineFeedCommand;
            if (feed != null) { Empty(writer, "feed", "line", feed.Lines.ToString(CultureInfo.InvariantCulture)); return; }
            var align = command as AlignmentCommand;
            if (align != null) { Empty(writer, "text", "align", align.Alignment.ToString().ToLowerInvariant()); return; }
            var bold = command as BoldCommand;
            if (bold != null) { Empty(writer, "text", "em", bold.Enabled ? "true" : "false"); return; }
            var underline = command as UnderlineCommand;
            if (underline != null) { Empty(writer, "text", "ul", underline.Enabled ? "true" : "false"); return; }
            var size = command as TextSizeCommand;
            if (size != null) { Empty(writer, "text", "width", size.Width.ToString(CultureInfo.InvariantCulture), "height", size.Height.ToString(CultureInfo.InvariantCulture)); return; }
            var cut = command as CutCommand;
            if (cut != null) { Empty(writer, "cut", "type", cut.Mode == CutMode.Partial ? "feed" : "no_feed"); return; }
            var pulse = command as DrawerPulseCommand;
            if (pulse != null) { Empty(writer, "pulse", "drawer", pulse.Pin == 2 ? "drawer_1" : "drawer_2", "time", "pulse_" + pulse.OnTime); return; }
            var qr = command as QrCodeCommand;
            if (qr != null) { writer.WriteStartElement("symbol", EposNamespace); writer.WriteAttributeString("type", "qrcode_model_2"); writer.WriteAttributeString("level", "level_l"); writer.WriteAttributeString("width", qr.ModuleSize.ToString(CultureInfo.InvariantCulture)); writer.WriteString(qr.Value); writer.WriteEndElement(); return; }
            var barcode = command as BarcodeCommand;
            if (barcode != null) { writer.WriteStartElement("barcode", EposNamespace); writer.WriteAttributeString("type", "code128"); writer.WriteAttributeString("height", barcode.Height.ToString(CultureInfo.InvariantCulture)); writer.WriteString(barcode.Value); writer.WriteEndElement(); return; }
            var raw = command as RawCommand;
            if (raw != null) { writer.WriteStartElement("command", EposNamespace); writer.WriteString(Convert.ToBase64String(raw.Data)); writer.WriteEndElement(); return; }
            throw new NotSupportedException("Unsupported command: " + command.GetType().Name);
        }

        private static void Empty(XmlWriter writer, string name, params string[] attributes)
        {
            writer.WriteStartElement(name, EposNamespace);
            for (var i = 0; i < attributes.Length; i += 2) writer.WriteAttributeString(attributes[i], attributes[i + 1]);
            writer.WriteEndElement();
        }

        public void Dispose() { if (_ownsClient) _client.Dispose(); }
    }
}
