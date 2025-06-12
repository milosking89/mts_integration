using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent; // Za ConcurrentDictionary ako se kešira pojedinačno unutar servisa
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using mts_integration.DTO;
using mts_integration.RestAPI;

public interface IDataCacheService
{
    /// <summary>
    /// Gets the export result as a byte array.
    /// </summary>
    /// <returns>A byte array representing the export data.</returns>
    Task<List<DtoDevicesData>> GetOrRefreshDevicesDataAsync();

    Task<List<DtoDevicesData>> RefreshAndCacheDevicesDataAsync();
}