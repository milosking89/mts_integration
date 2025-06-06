using System.Text.Json.Serialization;
using DocumentFormat.OpenXml.Office.CoverPageProps;

namespace mts_integration.DTO
{
    public class DtoDeviceData
    {
        public string name { get; set; }

        public string EUI { get; set; }

        public string nwAddress { get; set; }
        public string imsi { get; set; }

        public string lastCellID { get; set; }

    }
}