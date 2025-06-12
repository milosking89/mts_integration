using mts_integration.DTO;
using mts_integration.RestAPI;
using Org.BouncyCastle.Tls;

namespace mts_integration.Application.DataService.CachingServirce.Redis
{
    public class RedisAutoBackupWorker : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly string _backupFilePath = @"C:\REDIS\DeviceList\redis_backup.json";
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(60);
        private readonly IApiDataProvider _apiDataProvider;
        private readonly ILogger<RedisAutoBackupWorker> _logger; // Dodato za logovanje

        public RedisAutoBackupWorker(IServiceProvider services, IApiDataProvider apiDataProvider, ILogger<RedisAutoBackupWorker> logger)
        {
            _services = services;
            _apiDataProvider = apiDataProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _services.CreateScope();
            var cachedService = scope.ServiceProvider.GetRequiredService<RedisBackupService>();

            var data = new List<BriefDto>();
            data = await cachedService.LoadFromFileAsync(_backupFilePath);

            var fetchData = new List<DtoDevicesData>();

            if (data.Count > 0)
            {
                _logger.LogInformation("Loading data from file: {FilePath}", _backupFilePath);
                _logger.LogInformation("Data count is: {Count}", data.Count);
            }
            else
            {                
                fetchData = await _apiDataProvider.GetDevicesData();
                data = fetchData.SelectMany(x => x.Briefs).ToList();
                data = data.GroupBy(x => x.EUI).Select(g => g.First()).ToList();
            }

            await cachedService.SaveListOfDevices(_backupFilePath, data);

            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    //await Task.Delay(_interval, stoppingToken);

            //    await cachedService.SaveAllToFileAsync(_backupFilePath, data);
            //}
        }
    }

}
