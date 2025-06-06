using System.Text.Json.Serialization;

namespace mts_integration.DTO
{
    public class DtoDevicesData
    {
        public List<DeviceDto> Devices { get; set; }

        public List<BriefDto> Briefs { get; set; }

        public bool More { get; set; }

        public DateTime Now { get; set; }

        public int Count { get; set; }

    }

    public class DeviceDto
    {

    }

    public class BriefDto
    {
        public string name { get; set; }
        public string EUI { get; set; }
        public string nwAddress { get; set; }

        public string connectivity { get; set; }

        public string href { get; set; }

        public Dictionary<string, string> appServersRoutingProfile { get; set; } = new();
    }

}
