using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace RenewDeviceClientMemoryLeak.Tasks
{
    internal class SendDataTask : IDeviceTask
    {
        private readonly DeviceHubClient _deviceHubClient;

        public SendDataTask(
            DeviceHubClient deviceHubClient)
        {
            _deviceHubClient = deviceHubClient;
        }

        public async Task Run(CancellationToken cancelToken)
        {
            while (!cancelToken.IsCancellationRequested)
            {
                await _deviceHubClient.SendSampleData(cancelToken);

                int delay = RandomNumberGenerator.GetInt32(1, 5);
                await Task.Delay(TimeSpan.FromSeconds(delay), cancelToken);
            }
        }
    }
}
