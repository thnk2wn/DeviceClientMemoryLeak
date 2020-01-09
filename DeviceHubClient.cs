using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using RenewDeviceClientMemoryLeak.Config;
using Serilog;

namespace RenewDeviceClientMemoryLeak
{
    internal class DeviceHubClient
    {
        private readonly DeviceConfiguration _deviceConfig;
        private CancellationToken _cancellationToken;
        private DeviceClient _currentDeviceClient;
        private MethodCallback _directMethodCallback;
        private static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1,1);

        public bool IsConnInProgress { get; private set; }

        public DeviceHubClient(DeviceConfiguration deviceConfig)
        {
            _deviceConfig = deviceConfig;
        }

        public async Task EnsureHubConnectionAsync(MethodCallback callback, CancellationToken cancellationToken)
        {
            IsConnInProgress = false;
            Log.Information("Establishing initial connection to IoT Hub.");

            _cancellationToken = cancellationToken;
            _directMethodCallback = callback ?? throw new ArgumentNullException(nameof(callback));

            await ConnectToHubAsync(cancellationToken);
        }

        [Obsolete("Remove this if only reconnecting when health checks fail is sufficient. Forcing creates memory leaks")]
        public async Task RenewClientAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // 15 in real app
                    await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);

                    if (IsConnInProgress) continue;

                    Log.Information("Renewing hub connection to {host}", _deviceConfig.HubHostname);
                    await ConnectToHubAsync(_cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // ignore
                }
                catch (Exception ex)
                {
                    Log.Error("Error renewing connection to hub", ex);
                }
            }
        }

        public async Task SendData(byte[] contents, CancellationToken cancelToken)
        {
            Log.Information(
                "Sending {count:###,###,##0} bytes of data to hub",
                contents.Length);

            using (Message hubMessage = CreateMessage(contents))
            {
                await _currentDeviceClient.SendEventAsync(hubMessage, cancelToken);
            }
        }

        private static Message CreateMessage(byte[] contents)
        {
            var message = new Message(contents)
            {
                ContentType = "application/json",
                ContentEncoding = Encoding.UTF8.EncodingName
            };

            message.Properties.Add("context", "logging-sample");
            return message;
        }

        private async Task<DeviceClient> CreateConnection(
            DeviceConfiguration deviceConfig,
            CancellationToken cancellationToken)
        {
            const TransportType transportType = TransportType.Amqp_Tcp_Only;

            var authentication = new DeviceAuthenticationWithRegistrySymmetricKey(
                deviceConfig.DeviceId,
                deviceConfig.DeviceKey);

            DeviceClient deviceClient = DeviceClient.Create(
                deviceConfig.HubHostname,
                authentication,
                transportType);

            deviceClient.SetConnectionStatusChangesHandler(MonitorForDisabledConnection);

            await deviceClient.OpenAsync(cancellationToken);

            Log.Information(
                "Connection to hub {host} opened with transport {transportType}",
                deviceConfig.HubHostname,
                transportType);

            return deviceClient;
        }

        private async void MonitorForDisabledConnection(
            ConnectionStatus status,
            ConnectionStatusChangeReason reason)
        {
            Log.Information(
                "Client connection state changed. Status: {status}, Reason: {reason}",
                status,
                reason);

            if (status == ConnectionStatus.Disabled && !IsConnInProgress)
            {
                await ConnectToHubAsync(_cancellationToken);
            }
        }

        public async Task ConnectToHubAsync(CancellationToken cancellationToken)
        {
            await semaphoreSlim.WaitAsync(cancellationToken);

            try
            {
                IsConnInProgress = true;
                bool isReconnect = _currentDeviceClient != null;

                if (isReconnect)
                {
                    _currentDeviceClient.SetConnectionStatusChangesHandler(null);
                    _currentDeviceClient.Dispose();
                }

                Log.Information(
                    "{operation} to {host}",
                    isReconnect ? "Reconnecting" : "Connecting",
                    _deviceConfig.HubHostname);

                _currentDeviceClient = await CreateConnection(_deviceConfig, cancellationToken);

                await RegisterDesiredPropertyHandler(_currentDeviceClient, cancellationToken);
                await _currentDeviceClient.SetMethodDefaultHandlerAsync(_directMethodCallback, null, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error connecting or subscribing to {hub}", _deviceConfig.HubHostname);
            }
            finally
            {
                IsConnInProgress = false;
                semaphoreSlim.Release();
            }
        }

        private static Task RegisterDesiredPropertyHandler(DeviceClient deviceClient, CancellationToken cancellationToken)
        {
            Log.Information("Registering callback for Desired Property Changes");

            return deviceClient.SetDesiredPropertyUpdateCallbackAsync((properties, context) =>
            {
                Log.Information("{count} desired properties changed", properties.Count);
                return Task.CompletedTask;
            }, null, cancellationToken);
        }
    }
}
