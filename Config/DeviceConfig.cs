using Newtonsoft.Json;

namespace RenewDeviceClientMemoryLeak.Config
{
    public class DeviceConfiguration
    {
        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }

        [JsonProperty("deviceKey")]
        public string DeviceKey { get; set; }

        [JsonProperty("hubHostname")]
        public string HubHostname { get; set; }
    }
}
