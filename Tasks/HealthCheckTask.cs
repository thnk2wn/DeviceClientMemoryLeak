using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Counter;
using Humanizer;
using RenewDeviceClientMemoryLeak.Diagnostics;
using Serilog;

namespace RenewDeviceClientMemoryLeak.Tasks
{
    internal class HealthCheckTask : DisposableObject, IDeviceTask
    {
        private readonly IMetricsRoot _metrics;
        private static readonly TimeSpan monitorInterval = TimeSpan.FromSeconds(30);

        private float? _baselineMemoryMb;
        private int _gcCycles;

        public GCEventListener _listener;

        public HealthCheckTask(IMetricsRoot metrics)
        {
            _metrics = metrics;
        }

        public async Task Run(CancellationToken cancelToken)
        {
            try
            {
                _listener = new GCEventListener(OnGarbageCollected);

                while (!cancelToken.IsCancellationRequested)
                {
                    OutputHealthInfo();
                    await Task.Delay(monitorInterval, cancelToken);
                }
            }
            catch (OperationCanceledException)
            {
                _listener.Dispose();
                _listener = null;
            }
        }

        protected override void DisposeManagedResources()
        {
            _listener?.Dispose();
        }

        private void OutputHealthInfo()
        {
            Process p = Process.GetCurrentProcess();
            float memoryMb = p.WorkingSet64 / 1024f / 1024f;

            if (!_baselineMemoryMb.HasValue)
            {
                _baselineMemoryMb = memoryMb;
            }

            TimeSpan uptime = DateTime.Now - p.StartTime;

            Console.WriteLine();

            Log.Information(
                "Memory => Current: {memoryMb:###.00} MB, Baseline: {baseline:###.00} MB. GC cycles: {gcCycles}",
                memoryMb,
                _baselineMemoryMb,
                _gcCycles);

            MetricsContextValueSource iotContext = _metrics.Snapshot.Get()?.Contexts?.FirstOrDefault(
                c => c.Context == MetricsRegistry.IotContext);

            if (iotContext != null)
            {
                CounterValueSource eventCounter = iotContext.Counters.FirstOrDefault(
                    c => c.Name == MetricsRegistry.IotHubEvents.Name);

                CounterValueSource bytesCounter = iotContext.Counters.FirstOrDefault(
                    c => c.Name == MetricsRegistry.IotHubBytesSent.Name);

                if (eventCounter != null && bytesCounter != null)
                {
                    float mbSent = bytesCounter.Value.Count / 1024f / 1024f;
                    double roughEventsPerMin = eventCounter.Value.Count / uptime.TotalMinutes;

                    Log.Information(
                        "Iot Hub Events => {eventsCount:###,###,##0} events, Rate / min: {eventRate:##0}, sent: {mbSent:###,##0.00} MB",
                        eventCounter.Value.Count,
                        roughEventsPerMin,
                        mbSent);
                }
            }

            Log.Information("Uptime: {uptime}", uptime.Humanize());

            Console.WriteLine();
        }

        private void OnGarbageCollected()
        {
            _gcCycles++;
            Console.WriteLine();
            Log.Information("*** Detected system garbage collection. Cycles: {gcCycles} ***", _gcCycles);
            Console.WriteLine();
        }
    }
}
