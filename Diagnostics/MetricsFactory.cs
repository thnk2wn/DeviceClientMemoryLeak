using App.Metrics;

namespace RenewDeviceClientMemoryLeak.Diagnostics
{
    internal static class MetricsFactory
    {
        public static IMetricsRoot Create()
        {
            IMetricsRoot metrics = new MetricsBuilder()
                .Configuration.Configure(
                    options =>
                    {
                        options.Enabled = true;
                        options.ReportingEnabled = true;
                    })
                .SampleWith.AlgorithmR(sampleSize: 300)
                .Build();
            return metrics;
        }
    }
}
