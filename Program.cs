using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using App.Metrics;
using Microsoft.Azure.Devices.Client;
using RenewDeviceClientMemoryLeak.Config;
using RenewDeviceClientMemoryLeak.Data;
using RenewDeviceClientMemoryLeak.Diagnostics;
using RenewDeviceClientMemoryLeak.Tasks;
using Serilog;

namespace RenewDeviceClientMemoryLeak
{
    internal class Program
    {
        private static readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private static async Task<int> Main(string[] args)
        {
            Console.WriteLine("App starting, press Ctrl+C to cancel.");
            Console.CancelKeyPress += Console_CancelKeyPress;

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            DeviceConfiguration config = DeviceConfigReader.GetDeviceConfig();

            if (!config.Validate(out string error))
            {
                Log.Error("Device config could not be loaded; {error}", error);
                return -1;
            }

            Console.WriteLine();

            Console.Write("Renew IoT hub connection periodically (y/n)? ");
            ConsoleKeyInfo key = Console.ReadKey();

            Console.WriteLine();
            Console.WriteLine();

            bool renewHubConnection = key.Key == ConsoleKey.Y;
            Console.WriteLine();

            CancellationToken cancelToken = cancellationTokenSource.Token;

            IMetricsRoot metrics = MetricsFactory.Create();

            var deviceHubClient = new DeviceHubClient(config, metrics);
            await deviceHubClient.EnsureHubConnectionAsync(DispatchDirectCall, cancelToken);

            var tasksModule = new TasksModule(deviceHubClient, metrics);
            List<Task> tasks = tasksModule.GetTasks(cancelToken).ToList();

            if (renewHubConnection)
            {
                const int intervalMinutes = 5;
                Log.Information(
                    "IoT hub connection will be recreated every {interval} minutes regardless of status",
                    intervalMinutes);
                tasks.Add(deviceHubClient.RenewClientAsync(cancelToken, intervalMinutes));
            }
            else
            {
                Log.Information("IoT hub connection will not be periodically recreated");
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (TaskCanceledException)
            {
                Log.Information("App run cancelled");
            }
            catch (AggregateException)
            {
                Log.Information("Error running tasks");
            }

            return 0;
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("Cancellation detected, stopping execution");
            cancellationTokenSource.Cancel(throwOnFirstException: false);
            e.Cancel = true;

            //Environment.Exit(-2);
        }

        private static async Task<MethodResponse> DispatchDirectCall(MethodRequest methodRequest, object userContext)
        {
            await Task.Delay(100, cancellationTokenSource.Token);
            return new MethodResponse(SampleDeviceData.GetBytes(), 0);
        }
    }
}
