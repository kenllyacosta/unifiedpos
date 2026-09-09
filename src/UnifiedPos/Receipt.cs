using System;
using System.Collections.Generic;
using UnifiedPos.Commands;

namespace UnifiedPos
{
    public sealed class Receipt
    {
        private readonly List<PrintCommand> _commands = new List<PrintCommand>();
        public IReadOnlyList<PrintCommand> Commands => _commands;

        public Receipt Text(string value) { _commands.Add(new TextCommand(value)); return this; }
        public Receipt TextLine(string value = "") { _commands.Add(new TextCommand(value + "\n")); return this; }
        public Receipt Feed(int lines = 1) { _commands.Add(new LineFeedCommand(lines)); return this; }
        public Receipt Align(TextAlignment alignment) { _commands.Add(new AlignmentCommand(alignment)); return this; }
        public Receipt AlignLeft() => Align(TextAlignment.Left);
        public Receipt AlignCenter() => Align(TextAlignment.Center);
        public Receipt AlignRight() => Align(TextAlignment.Right);
        public Receipt Bold(bool enabled = true) { _commands.Add(new BoldCommand(enabled)); return this; }
        public Receipt Underline(bool enabled = true) { _commands.Add(new UnderlineCommand(enabled)); return this; }
        public Receipt TextSize(int width, int height) { _commands.Add(new TextSizeCommand(width, height)); return this; }
        public Receipt Normal() { return Bold(false).Underline(false).TextSize(1, 1).AlignLeft(); }
        public Receipt Cut(CutMode mode = CutMode.Partial) { _commands.Add(new CutCommand(mode)); return this; }
        public Receipt OpenDrawer(int pin = 2, int onTime = 50, int offTime = 250) { _commands.Add(new DrawerPulseCommand(pin, onTime, offTime)); return this; }
        public Receipt QrCode(string value, int moduleSize = 6, int errorCorrection = 48) { _commands.Add(new QrCodeCommand(value, moduleSize, errorCorrection)); return this; }
        public Receipt Barcode(string value, int type = 73, int height = 80, int width = 3) { _commands.Add(new BarcodeCommand(value, type, height, width)); return this; }
        public Receipt Raw(params byte[] data) { _commands.Add(new RawCommand(data)); return this; }
    }
}
