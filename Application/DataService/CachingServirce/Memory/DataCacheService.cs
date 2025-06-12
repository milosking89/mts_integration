using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System; // Za TimeSpan
using Microsoft.Extensions.Logging; // Dodato za bolje logovanje

// Pretpostavljam da su DTO i API klase ovde
using mts_integration.DTO;
using mts_integration.RestAPI;

public class DataCacheService : IDataCacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IApiDataProvider _apiDataProvider; // Koristiš samo ovo za poziv API 1
    private readonly ILogger<DataCacheService> _logger; // Dodato za logovanje

    private const string CacheKey = "AllDevicesDataCache"; // Jasniji ključ ako keširaš samo DevicesData
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1); // Za sinhronizaciju

    public DataCacheService(IMemoryCache memoryCache, IApiDataProvider apiDataProvider, ILogger<DataCacheService> logger)
    {
        _memoryCache = memoryCache;
        _apiDataProvider = apiDataProvider;
        _logger = logger; // Injektovanje loggera
    }

    // Metoda za dohvatanje (ili osvežavanje) keširane liste osnovnih DTO-a
    public async Task<List<DtoDevicesData>> GetOrRefreshDevicesDataAsync()
    {
        // Pokušaj da dohvatiš iz keša
        if (_memoryCache.TryGetValue(CacheKey, out List<DtoDevicesData> cachedData))
        {
            _logger.LogInformation("Dohvaćena lista DTO-a iz keša. Ključ: {CacheKey}", CacheKey);
            return cachedData;
        }

        // Ako nije u kešu ili je istekao, dohvati nove podatke
        _logger.LogInformation("Keš za listu DTO-a je prazan ili istekao. Dohvatam nove podatke. Ključ: {CacheKey}", CacheKey);
        return await RefreshAndCacheDevicesDataAsync();
    }

    // Metoda za prinudno osvežavanje keša
    public async Task<List<DtoDevicesData>> RefreshAndCacheDevicesDataAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            // Dvostruka provera (Double-checked locking) u slučaju trke
            if (_memoryCache.TryGetValue(CacheKey, out List<DtoDevicesData> existingCachedData) && existingCachedData != null)
            {
                _logger.LogInformation("Neko je već osvežio keš za listu DTO-a, vraćam postojeće podatke. Ključ: {CacheKey}", CacheKey);
                return existingCachedData;
            }

            _logger.LogInformation("Pozivam API za osnovnu listu DTO-a (API 1)...");
            List<DtoDevicesData> freshData = await _apiDataProvider.GetDevicesData();
            _logger.LogInformation("Dohvaćeno {Count} osnovnih DTO-a iz API 1.", freshData.Count);

            // KORAK: Keširaj rezultate
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(60)); // Keš ističe nakon 30 minuta

            _memoryCache.Set(CacheKey, freshData, cacheEntryOptions);
            _logger.LogInformation("Keširana osnovna lista DTO-a. Broj elemenata: {Count}. Keš ističe za {ExpirationMinutes} minuta. Ključ: {CacheKey}",
                                    freshData.Count, cacheEntryOptions.AbsoluteExpiration?.Minute, CacheKey);

            return freshData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška prilikom osvežavanja keša za listu DTO-a.");
            throw; // Re-throw exception da bi se mogao obraditi više
        }
        finally
        {
            _semaphore.Release();
        }
    }
}