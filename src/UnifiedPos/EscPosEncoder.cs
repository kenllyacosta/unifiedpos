using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnifiedPos.Commands;

namespace UnifiedPos
{
    public sealed class EscPosEncoder
    {
        public EscPosEncoder(Encoding encoding = null) { Encoding = encoding ?? Encoding.ASCII; }
        public Encoding Encoding { get; }

        public byte[] Encode(Receipt receipt)
        {
            if (receipt == null) throw new ArgumentNullException(nameof(receipt));
            using (var output = new MemoryStream())
            {
                Write(output, 0x1B, 0x40);
                foreach (var command in receipt.Commands) EncodeCommand(output, command);
                return output.ToArray();
            }
        }

        private void EncodeCommand(Stream output, PrintCommand command)
        {
            var text = command as TextCommand;
            if (text != null) { Write(output, Encoding.GetBytes(text.Value)); return; }
            var feed = command as LineFeedCommand;
            if (feed != null) { Write(output, 0x1B, 0x64, (byte)feed.Lines); return; }
            var align = command as AlignmentCommand;
            if (align != null) { Write(output, 0x1B, 0x61, (byte)align.Alignment); return; }
            var bold = command as BoldCommand;
            if (bold != null) { Write(output, 0x1B, 0x45, bold.Enabled ? (byte)1 : (byte)0); return; }
            var underline = command as UnderlineCommand;
            if (underline != null) { Write(output, 0x1B, 0x2D, underline.Enabled ? (byte)1 : (byte)0); return; }
            var size = command as TextSizeCommand;
            if (size != null) { Write(output, 0x1D, 0x21, (byte)(((size.Width - 1) << 4) | (size.Height - 1))); return; }
            var cut = command as CutCommand;
            if (cut != null) { Write(output, 0x1D, 0x56, cut.Mode == CutMode.Partial ? (byte)1 : (byte)0); return; }
            var pulse = command as DrawerPulseCommand;
            if (pulse != null) { Write(output, 0x1B, 0x70, pulse.Pin == 2 ? (byte)0 : (byte)1, (byte)pulse.OnTime, (byte)pulse.OffTime); return; }
            var qr = command as QrCodeCommand;
            if (qr != null) { EncodeQr(output, qr); return; }
            var barcode = command as BarcodeCommand;
            if (barcode != null) { EncodeBarcode(output, barcode); return; }
            var raw = command as RawCommand;
            if (raw != null) { Write(output, raw.Data); return; }
            throw new NotSupportedException("Unsupported command: " + command.GetType().Name);
        }

        private void EncodeQr(Stream output, QrCodeCommand qr)
        {
            Write(output, 0x1D, 0x28, 0x6B, 0x04, 0x00, 0x31, 0x41, 0x32, 0x00);
            Write(output, 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x43, (byte)qr.ModuleSize);
            Write(output, 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x45, (byte)qr.ErrorCorrection);
            var data = Encoding.GetBytes(qr.Value);
            var length = data.Length + 3;
            Write(output, 0x1D, 0x28, 0x6B, (byte)(length & 0xFF), (byte)(length >> 8), 0x31, 0x50, 0x30);
            Write(output, data);
            Write(output, 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x51, 0x30);
        }

        private void EncodeBarcode(Stream output, BarcodeCommand barcode)
        {
            var data = Encoding.GetBytes(barcode.Value);
            if (data.Length > 255) throw new ArgumentException("Barcode data cannot exceed 255 encoded bytes.");
            Write(output, 0x1D, 0x68, (byte)barcode.Height);
            Write(output, 0x1D, 0x77, (byte)barcode.Width);
            Write(output, 0x1D, 0x6B, (byte)barcode.Type, (byte)data.Length);
            Write(output, data);
        }

        private static void Write(Stream stream, params byte[] data) { stream.Write(data, 0, data.Length); }
    }
}
