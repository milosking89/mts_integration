using System.Text.Json;
using mts_integration.DTO;
using StackExchange.Redis;

namespace mts_integration.Application.DataService.CachingServirce.Redis
{
    public class RedisBackupService
    {
        private readonly IDatabase _db;
        private readonly IConnectionMultiplexer _redis;

        public RedisBackupService(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _db = redis.GetDatabase();
        }

        public async Task SaveListOfDevices(string filePath, List<BriefDto> data)
        {
            var server = _redis.GetServer(_redis.GetEndPoints()[0]);
            var keys = server.Keys();

            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);
        }

        public async Task<List<BriefDto>> LoadFromFileAsync(string filePath)
        {
            if (File.Exists(filePath))
            {
                var json = await File.ReadAllTextAsync(filePath);

                if (string.IsNullOrWhiteSpace(json))
                {
                    return new List<BriefDto>();
                }

                var data = JsonSerializer.Deserialize<List<BriefDto>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (data != null && data.Any())
                {
                    return data;
                }
            }

            return new List<BriefDto>();
        }

        public async Task SaveDeviceDetails(string filePath, List<DtoDeviceData> data)
        {
            var server = _redis.GetServer(_redis.GetEndPoints()[0]);
            var keys = server.Keys();

            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);
        }

        public async Task<List<DtoDeviceData>> LoadDevicesFromFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                var json = await File.ReadAllTextAsync(filePath);

                if (string.IsNullOrWhiteSpace(json))
                {
                    return new List<DtoDeviceData>();
                }

                var data = JsonSerializer.Deserialize<List<DtoDeviceData>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (data != null && data.Any())
                {
                    return data;
                }
            }

            return new List<DtoDeviceData>();
        }
    }

}
