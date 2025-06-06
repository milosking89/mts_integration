using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Intrinsics.X86;
using System.Text.Json;
using System.Xml.Serialization;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.WebUtilities;
using mts_integration.Application.AuthService;
using mts_integration.DTO;

namespace mts_integration.RestAPI
{
    public class ApiDataProvider : IApiDataProvider
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly AuthService _authService;

        private const int PageSize = 100;

        public ApiDataProvider(HttpClient httpClient, IConfiguration configuration, AuthService authService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _authService = authService;
        }
        public async Task<List<DtoDevicesData>> GetDevicesData()
        {

            var token = await _authService.GetAccessTokenAsync();

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var allDevices = new List<DtoDevicesData>();
            int pageIndex = 1;
            int pageSize = 5000; ///5000 maximum allowed by the API
            var name = "MAC";
            bool more = true;

            string url = $"https://iot.mts.rs/thingpark/wireless/rest/subscriptions/mine/devices";

            while (more)
            {
                var newUrl = $"{url}?pageIndex={pageIndex}&pageSize={pageSize}";

                string responseBody = "";

                try
                {
                    HttpResponseMessage response = await _httpClient.GetAsync(newUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        response.EnsureSuccessStatusCode();
                        responseBody = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        Console.WriteLine(response);
                        return allDevices;
                    }
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"HTTP error: {ex.Message}", responseBody);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error: {ex.Message}");
                }
 
                var result = JsonSerializer.Deserialize<DtoDevicesData>(responseBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result == null || result.Briefs.Count == null || result.Briefs.Count == 0)
                {
                    break; 
                }

                allDevices.Add(result);

                more = result.More;
                pageIndex++;

                if (pageIndex == 15)
                {
                    pageSize = result.Count % pageSize;
                }
            }

            //var distinctList = allDevices.GroupBy(x => x.Briefs).Select(g => g.First()).ToList();

            return allDevices;
        }

        public async Task<List<string>> GetDevicesDataName(string item)
        {

            var token = await _authService.GetAccessTokenAsync();

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var allDevices = new List<string>();

            string url = $"https://iot.mts.rs" + item;
   
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            allDevices.Add(content);


            return allDevices;
        }
    }
}
