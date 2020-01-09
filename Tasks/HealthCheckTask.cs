using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace RenewDeviceClientMemoryLeak.Tasks
{
    internal class HealthCheckTask : IDeviceTask
    {
        private static readonly TimeSpan MonitorInterval = TimeSpan.FromSeconds(30);

        private float? _baselineMemoryMb;

        public async Task Run(CancellationToken cancelToken)
        {
            while (!cancelToken.IsCancellationRequested)
            {
                Process p = Process.GetCurrentProcess();
                float memoryMb = p.WorkingSet64 / 1024f / 1024f;

                Log.Information(
                    "Memory MB => Current: {memoryMb:###.00} MB, Baseline: {baseline:###.00} MB",
                    memoryMb,
                    _baselineMemoryMb);

                if (!_baselineMemoryMb.HasValue)
                {
                    _baselineMemoryMb = memoryMb;
                }

                await Task.Delay(MonitorInterval, cancelToken);
            }
        }
    }
}
