using System;
using System.Threading;
using System.Threading.Tasks;
using RenewDeviceClientMemoryLeak.Data;

namespace RenewDeviceClientMemoryLeak.Tasks
{
    internal class SendDataTask : IDeviceTask
    {
        private readonly DeviceHubClient _deviceHubClient;

        public SendDataTask(DeviceHubClient deviceHubClient)
        {
            _deviceHubClient = deviceHubClient;
        }

        public async Task Run(CancellationToken cancelToken)
        {
            var r = new Random();

            while (!cancelToken.IsCancellationRequested)
            {
                await _deviceHubClient.SendData(SampleDeviceData.GetBytes(), cancelToken);

                await Task.Delay(TimeSpan.FromSeconds(r.Next(2, 5)), cancelToken);
            }
        }
    }
}
