using App.Metrics.Counter;
using App.Metrics.Meter;

namespace RenewDeviceClientMemoryLeak.Diagnostics
{
    public static class MetricsRegistry
    {
        public const string IotContext = "IoT Hub";

        public static CounterOptions IotHubBytesSent => new CounterOptions
        {
            Context = IotContext,
            Name = "IoT Hub Bytes Sent"
        };

        public static CounterOptions IotHubEvents = new CounterOptions
        {
            Context = IotContext,
            Name = "Iot Hub Events"
        };
    }
}
