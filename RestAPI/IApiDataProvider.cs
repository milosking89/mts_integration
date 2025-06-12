using mts_integration.DTO;

namespace mts_integration.RestAPI
{
    public interface IApiDataProvider
    {
        /// <summary>
        /// Gets the export result as a byte array.
        /// </summary>
        /// <returns>A byte array representing the export data.</returns>
        /// 
        Task<int> GetDevicesDataLength();

        Task<List<DtoDevicesData>> GetDevicesData();

        Task<string> GetDevicesDataByUrl(string item, bool IsAuthenticated);
    }

}
