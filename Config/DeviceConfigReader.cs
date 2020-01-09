using System;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace RenewDeviceClientMemoryLeak.Config
{
    public static class DeviceConfigReader
    {
        private static IConfigurationRoot LoadConfiguration()
        {
            string basePath = Environment.CurrentDirectory;
            Log.Information("Reading appSettings configuration in {basePath}", basePath);

            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appSettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile("appSettings.private.json", optional: true, reloadOnChange: false);

            return builder.Build();
        }

        public static DeviceConfiguration GetDeviceConfig()
        {
            IConfigurationRoot configuration = LoadConfiguration();
            var deviceConfig = new DeviceConfiguration();
            configuration.Bind(deviceConfig);
            return deviceConfig;
        }
    }
}
