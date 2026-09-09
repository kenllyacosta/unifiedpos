using System;

namespace UnifiedPos.Commands
{
    public enum TextAlignment { Left, Center, Right }
    public enum CutMode { Full, Partial }

    public abstract class PrintCommand { }

    public sealed class TextCommand : PrintCommand
    {
        public TextCommand(string value) { Value = value ?? throw new ArgumentNullException(nameof(value)); }
        public string Value { get; }
    }

    public sealed class LineFeedCommand : PrintCommand
    {
        public LineFeedCommand(int lines) { if (lines < 0 || lines > 255) throw new ArgumentOutOfRangeException(nameof(lines)); Lines = lines; }
        public int Lines { get; }
    }

    public sealed class AlignmentCommand : PrintCommand
    {
        public AlignmentCommand(TextAlignment alignment) { Alignment = alignment; }
        public TextAlignment Alignment { get; }
    }

    public sealed class BoldCommand : PrintCommand
    {
        public BoldCommand(bool enabled) { Enabled = enabled; }
        public bool Enabled { get; }
    }

    public sealed class UnderlineCommand : PrintCommand
    {
        public UnderlineCommand(bool enabled) { Enabled = enabled; }
        public bool Enabled { get; }
    }

    public sealed class TextSizeCommand : PrintCommand
    {
        public TextSizeCommand(int width, int height)
        {
            if (width < 1 || width > 8) throw new ArgumentOutOfRangeException(nameof(width));
            if (height < 1 || height > 8) throw new ArgumentOutOfRangeException(nameof(height));
            Width = width; Height = height;
        }
        public int Width { get; }
        public int Height { get; }
    }

    public sealed class CutCommand : PrintCommand
    {
        public CutCommand(CutMode mode) { Mode = mode; }
        public CutMode Mode { get; }
    }

    public sealed class DrawerPulseCommand : PrintCommand
    {
        public DrawerPulseCommand(int pin, int onTime, int offTime)
        {
            if (pin != 2 && pin != 5) throw new ArgumentOutOfRangeException(nameof(pin));
            if (onTime < 0 || onTime > 255) throw new ArgumentOutOfRangeException(nameof(onTime));
            if (offTime < 0 || offTime > 255) throw new ArgumentOutOfRangeException(nameof(offTime));
            Pin = pin; OnTime = onTime; OffTime = offTime;
        }
        public int Pin { get; }
        public int OnTime { get; }
        public int OffTime { get; }
    }

    public sealed class QrCodeCommand : PrintCommand
    {
        public QrCodeCommand(string value, int moduleSize, int errorCorrection)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            if (moduleSize < 1 || moduleSize > 16) throw new ArgumentOutOfRangeException(nameof(moduleSize));
            if (errorCorrection < 48 || errorCorrection > 51) throw new ArgumentOutOfRangeException(nameof(errorCorrection));
            ModuleSize = moduleSize; ErrorCorrection = errorCorrection;
        }
        public string Value { get; }
        public int ModuleSize { get; }
        public int ErrorCorrection { get; }
    }

    public sealed class BarcodeCommand : PrintCommand
    {
        public BarcodeCommand(string value, int type, int height, int width)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            if (height < 1 || height > 255) throw new ArgumentOutOfRangeException(nameof(height));
            if (width < 2 || width > 6) throw new ArgumentOutOfRangeException(nameof(width));
            Type = type; Height = height; Width = width;
        }
        public string Value { get; }
        public int Type { get; }
        public int Height { get; }
        public int Width { get; }
    }

    public sealed class RawCommand : PrintCommand
    {
        public RawCommand(byte[] data) { Data = data ?? throw new ArgumentNullException(nameof(data)); }
        public byte[] Data { get; }
    }
}
