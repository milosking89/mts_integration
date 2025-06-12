using System.Diagnostics;
using System.Text.Json;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using mts_integration.Application.DataService;
using mts_integration.Application.DataService.CachingServirce.Redis;
using mts_integration.DTO;
using mts_integration.RestAPI;

namespace mts_integration.Controllers
{
    [ApiController]
    [Route("api/data")]
    public class DataController : ControllerBase
    {
        private readonly ILogger<DataController> _logger;
        private readonly IApiDataProvider _apiDataProvider;
        private readonly IGenerateData _generateData;
        private readonly IDataCacheService _dtoCacheService;
        private readonly IServiceProvider _services;

        private readonly string _backupFilePath = @"C:\REDIS\DeviceList\redis_backup.json";


        public DataController(
            ILogger<DataController> logger, 
            IApiDataProvider apiDataProvider,
            IGenerateData generateData,
            IDataCacheService dtoCacheService,
            IServiceProvider services)
        {
            _logger = logger;
            _apiDataProvider = apiDataProvider;
            _generateData = generateData;
            _dtoCacheService = dtoCacheService;
            _services = services;
        }


        [HttpGet("getDeviceData")]
        public async Task<IActionResult> GetDeviceData()
        {

            var data = new List<BriefDto>();
            var fetchData = new List<DtoDevicesData>();

            fetchData = await _apiDataProvider.GetDevicesData();
            if (fetchData == null || !fetchData.Any())
            {
                return NotFound("No device data found.");
            }

            using var scope = _services.CreateScope();
            var redisService = scope.ServiceProvider.GetRequiredService<RedisBackupService>();

            data = fetchData.SelectMany(x => x.Briefs).ToList();

            var _data = data.GroupBy(x => x.EUI).Select(g => g.First()).ToList();

            await redisService.SaveListOfDevices(_backupFilePath, _data);

            return Ok();
        }


        [HttpGet("getDeviceDataLength")]
        public async Task<IActionResult> GetDeviceDataLength()
        {
            //var data = await _dtoCacheService.GetOrRefreshDevicesDataAsync();

            var data = await _apiDataProvider.GetDevicesDataLength();

            //var content = await _generateData.GenerateDevicesExcel(data);
            //var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            //var fileName = "DataFromApi.xlsx";

            return Ok(data);

            //return File(content, contentType, fileName);
        }


        [HttpGet("getDeviceDataDetails")]
        public async Task<IActionResult> GetDeviceDataDetails([FromQuery(Name = "from")] int from, [FromQuery(Name = "to")] int to)
        {

            using var scope = _services.CreateScope();
            var redisService = scope.ServiceProvider.GetRequiredService<RedisBackupService>();
            var data = await redisService.LoadFromFileAsync(@"C:\REDIS\DeviceList\redis_backup.json");

            bool isAuthenticated = false;
            const int ChangeThreshold = 2000;

            var initialLimiter = from;
            var rangeLimiter = to;

            var finalResult = new List<DtoDeviceData>();

            for (int i = initialLimiter; i < rangeLimiter; i++)
            {
                var responseBody = await _apiDataProvider.GetDevicesDataByUrl(data[i].href, isAuthenticated);

                isAuthenticated = true;

                var result = JsonSerializer.Deserialize<DtoDeviceData>(responseBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                finalResult.Add(result);

                if (i % ChangeThreshold == 0)
                {
                    isAuthenticated = false;
                }
            }
            ;

            var _backupFilePath = @"C:\REDIS\DeviceDetails\redis_backup.json";

            await redisService.SaveDeviceDetails(_backupFilePath, finalResult);

            return Ok(finalResult);
        }

       
        [HttpGet("getDeviceDataToExcel")]
        public async Task<IActionResult> GetDeviceDataToExcel()
        {

            using var scope = _services.CreateScope();
            var redisService = scope.ServiceProvider.GetRequiredService<RedisBackupService>();
            var data = await redisService.LoadDevicesFromFile(@"C:\REDIS\DeviceDetails\redis_backup.json");

            var content = await _generateData.GenerateDeviceExcelByName(data);
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = "DataFromApi.xlsx";

            return File(content, contentType, fileName);
        }


        [HttpGet("getDeviceDataByIMSI")]
        public async Task<IActionResult> GetDeviceDataByIMSI([FromQuery(Name = "imsi")] string imsi)
        {
            using var scope = _services.CreateScope();
            var redisService = scope.ServiceProvider.GetRequiredService<RedisBackupService>();
            var data = await redisService.LoadDevicesFromFile(@"C:\REDIS\DeviceDetails\redis_backup.json");

            var filteredData = data.Where(d => d.imsi == imsi).SingleOrDefault();

            if (filteredData == null)
            {
                return NotFound($"No devices found with IMSI: {imsi}");
            }

            var url = filteredData.NetworkSubscription.href;

            var _data =  await _apiDataProvider.GetDevicesDataByUrl(url, false);

            return Ok(_data);
        }
    }
}
