using System;
using System.Threading;
using System.Threading.Tasks;

namespace UnifiedPos
{
    public interface IPrinterTransport : IDisposable
    {
        Task SendAsync(Receipt receipt, CancellationToken cancellationToken = default(CancellationToken));
    }
}
