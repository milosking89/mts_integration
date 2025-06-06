using System.Text.Json;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mts_integration.Application.DataService;
using mts_integration.RestAPI;

namespace mts_integration.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataController : ControllerBase
    {
        private readonly ILogger<DataController> _logger;
        private readonly IApiDataProvider _apiDataProvider;
        private readonly IGenerateData _generateData;

        public DataController(ILogger<DataController> logger, IApiDataProvider apiDataProvider, IGenerateData generateData)
        {
            _logger = logger;
            _apiDataProvider = apiDataProvider;
            _generateData = generateData;
        }

        [HttpGet("getDeviceDataResult")]
        public async Task<IActionResult> GetDeviceDataResult()
        {
            var data = await _apiDataProvider.GetDevicesData();

            var content = await _generateData.GenerateDevicesExcel(data);
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = "DataFromApi.xlsx";

            //return Ok(data);

            return File(content, contentType, fileName);
        }

        [HttpGet("getDeviceDataResultByName")]
        public async Task<IActionResult> getDeviceDataResultByName([FromQuery(Name = "elem")] string item)
        {
            var data = await _apiDataProvider.GetDevicesDataName(item);

            //var content = await _generateData.GenerateDeviceExcelByName(data);
            //var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            //var fileName = "DataFromApi.xlsx";

            //return File(content, contentType, fileName); // Ensure content is awaited

            return Ok(data);
        }
    }
}
