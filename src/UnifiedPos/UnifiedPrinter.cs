using System;
using System.Threading;
using System.Threading.Tasks;

namespace UnifiedPos
{
    public sealed class UnifiedPrinter : IDisposable
    {
        private readonly IPrinterTransport _transport;
        public UnifiedPrinter(IPrinterTransport transport) { _transport = transport ?? throw new ArgumentNullException(nameof(transport)); }

        public Task PrintAsync(Action<Receipt> build, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (build == null) throw new ArgumentNullException(nameof(build));
            var receipt = new Receipt();
            build(receipt);
            return _transport.SendAsync(receipt, cancellationToken);
        }

        public void Dispose() { _transport.Dispose(); }
    }
}
