using System.Collections.Generic;
using System.Linq;

namespace RenewDeviceClientMemoryLeak.Config
{
    public static class DeviceConfigExtensions
    {
        public static bool Validate(this DeviceConfiguration deviceConfig, out string message)
        {
            var errors = new List<string>();

            if (deviceConfig == null)
            {
                errors.Add("Device configuration is null");
            }
            else
            {
                if (string.IsNullOrEmpty(deviceConfig.HubHostname))
                {
                    errors.Add("HubHostName is missing");
                }

                if (string.IsNullOrEmpty(deviceConfig.DeviceId))
                {
                    errors.Add("DeviceId is missing");
                }

                if (string.IsNullOrEmpty(deviceConfig.DeviceKey))
                {
                    errors.Add("DeviceKey is missing");
                }
            }

            message = string.Join(", ", errors);
            return !errors.Any();
        }
    }
}
