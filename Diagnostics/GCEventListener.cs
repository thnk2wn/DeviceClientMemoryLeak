using System;
using System.Diagnostics.Tracing;

namespace RenewDeviceClientMemoryLeak.Diagnostics
{
    internal sealed class GCEventListener : EventListener
    {
        private readonly Action _gcTriggeredAction;

        // from https://docs.microsoft.com/en-us/dotnet/framework/performance/garbage-collection-etw-events
        private const int GC_KEYWORD =                 0x0000001;
        private const int TYPE_KEYWORD =               0x0080000;
        private const int GCHEAPANDTYPENAMES_KEYWORD = 0x1000000;

        private const string GCSource = "Microsoft-Windows-DotNETRuntime";

        public GCEventListener(Action gcTriggeredAction)
        {
            _gcTriggeredAction = gcTriggeredAction;
        }

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            // Consider also listening for Microsoft-Azure-Devices-Device-Client

            // look for .NET Garbage Collection events
            if (eventSource.Name.Equals("Microsoft-Windows-DotNETRuntime"))
            {
                EnableEvents(
                    eventSource,
                    EventLevel.Verbose,
                    (EventKeywords) (GC_KEYWORD | GCHEAPANDTYPENAMES_KEYWORD | TYPE_KEYWORD)
                );
            }
        }

        // from https://blogs.msdn.microsoft.com/dotnet/2018/12/04/announcing-net-core-2-2/
        // Called whenever an event is written.
        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            if (eventData.EventSource.Name == GCSource && eventData.EventName == "GCTriggered")
            {
                _gcTriggeredAction?.Invoke();
            }
        }
    }
}
