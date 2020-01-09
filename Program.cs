using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using RenewDeviceClientMemoryLeak.Config;
using RenewDeviceClientMemoryLeak.Data;
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

            CancellationToken cancelToken = cancellationTokenSource.Token;

            var deviceHubClient = new DeviceHubClient(config);
            await deviceHubClient.EnsureHubConnectionAsync(DispatchDirectCall, cancelToken);

            var tasksModule = new TasksModule(deviceHubClient);
            List<Task> tasks = tasksModule.GetTasks(cancelToken).ToList();

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
            cancellationTokenSource.Cancel();
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
