using mts_integration.DTO;

namespace mts_integration.Application.DataService
{
    public interface IGenerateData
    {
        /// <summary>
        /// Gets the export result as a byte array.
        /// </summary>
        /// <returns>A byte array representing the export data.</returns>
        Task<byte[]> GenerateDevicesExcel(List<DtoDevicesData> data);

        Task<byte[]> GenerateDeviceExcelByName(List<DtoDeviceData> data);
    }
}
