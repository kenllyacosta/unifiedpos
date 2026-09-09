using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnifiedPos;

namespace UnifiedPos.Transports
{
    public sealed class WindowsPrintQueueTransport : IPrinterTransport
    {
        private readonly string _printerName;
        private readonly EscPosEncoder _encoder;
        public WindowsPrintQueueTransport(string printerName, EscPosEncoder encoder = null)
        {
            if (string.IsNullOrWhiteSpace(printerName)) throw new ArgumentException("A Windows printer name is required.", nameof(printerName));
            _printerName = printerName; _encoder = encoder ?? new EscPosEncoder();
        }

        public Task SendAsync(Receipt receipt, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            RawSpooler.Write(_printerName, _encoder.Encode(receipt));
            return Task.FromResult(0);
        }

        public void Dispose() { }

        private static class RawSpooler
        {
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            private sealed class DocInfo
            {
                [MarshalAs(UnmanagedType.LPWStr)] public string DocName = "UnifiedPos receipt";
                [MarshalAs(UnmanagedType.LPWStr)] public string OutputFile = null;
                [MarshalAs(UnmanagedType.LPWStr)] public string DataType = "RAW";
            }

            [DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)] private static extern bool OpenPrinter(string name, out IntPtr handle, IntPtr defaults);
            [DllImport("winspool.drv", SetLastError = true)] private static extern bool ClosePrinter(IntPtr handle);
            [DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)] private static extern int StartDocPrinter(IntPtr handle, int level, [In] DocInfo info);
            [DllImport("winspool.drv", SetLastError = true)] private static extern bool EndDocPrinter(IntPtr handle);
            [DllImport("winspool.drv", SetLastError = true)] private static extern bool StartPagePrinter(IntPtr handle);
            [DllImport("winspool.drv", SetLastError = true)] private static extern bool EndPagePrinter(IntPtr handle);
            [DllImport("winspool.drv", SetLastError = true)] private static extern bool WritePrinter(IntPtr handle, byte[] bytes, int count, out int written);

            internal static void Write(string printerName, byte[] bytes)
            {
                if (Environment.OSVersion.Platform != PlatformID.Win32NT) throw new PlatformNotSupportedException("Printer-name transport requires Windows.");
                IntPtr handle;
                if (!OpenPrinter(printerName, out handle, IntPtr.Zero)) ThrowLastError("Could not open printer '" + printerName + "'.");
                try
                {
                    if (StartDocPrinter(handle, 1, new DocInfo()) == 0) ThrowLastError("Could not start print job.");
                    try
                    {
                        if (!StartPagePrinter(handle)) ThrowLastError("Could not start printer page.");
                        try
                        {
                            int written;
                            if (!WritePrinter(handle, bytes, bytes.Length, out written) || written != bytes.Length) ThrowLastError("Could not write the complete print job.");
                        }
                        finally { EndPagePrinter(handle); }
                    }
                    finally { EndDocPrinter(handle); }
                }
                finally { ClosePrinter(handle); }
            }

            private static void ThrowLastError(string message) { throw new Win32Exception(Marshal.GetLastWin32Error(), message); }
        }
    }
}
