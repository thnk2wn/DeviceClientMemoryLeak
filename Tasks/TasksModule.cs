using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using App.Metrics;

namespace RenewDeviceClientMemoryLeak.Tasks
{
    internal class TasksModule
    {
        private readonly SendDataTask _sendDataTask;
        private readonly HealthCheckTask _healthCheckTask;

        public TasksModule(DeviceHubClient deviceHubClient, IMetricsRoot metrics)
        {
            _sendDataTask = new SendDataTask(deviceHubClient);
            _healthCheckTask = new HealthCheckTask(metrics);
        }

        public IEnumerable<Task> GetTasks(CancellationToken cancelToken)
        {
            return new[]
            {
                _healthCheckTask.Run(cancelToken),
                _sendDataTask.Run(cancelToken)
            };
        }
    }
}
