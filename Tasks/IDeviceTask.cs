using System.Threading;
using System.Threading.Tasks;

namespace RenewDeviceClientMemoryLeak.Tasks
{
    internal interface IDeviceTask
    {
        Task Run(CancellationToken cancelToken);
    }
}
