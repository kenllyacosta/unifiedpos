using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnifiedPos;

namespace UnifiedPos.Transports
{
    public sealed class Tcp9100Transport : IPrinterTransport
    {
        private readonly string _host;
        private readonly int _port;
        private readonly EscPosEncoder _encoder;
        public Tcp9100Transport(string host, int port = 9100, EscPosEncoder encoder = null)
        {
            if (string.IsNullOrWhiteSpace(host)) throw new ArgumentException("A host is required.", nameof(host));
            _host = host; _port = port; _encoder = encoder ?? new EscPosEncoder();
        }

        public async Task SendAsync(Receipt receipt, CancellationToken cancellationToken = default(CancellationToken))
        {
            var payload = _encoder.Encode(receipt);
            using (var client = new TcpClient())
            {
                cancellationToken.ThrowIfCancellationRequested();
                await client.ConnectAsync(_host, _port).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                using (var stream = client.GetStream())
                    await stream.WriteAsync(payload, 0, payload.Length, cancellationToken).ConfigureAwait(false);
            }
        }

        public void Dispose() { }
    }
}
