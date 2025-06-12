using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Intrinsics.X86;
using System.Text.Json;
using System.Xml.Serialization;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Spreadsheet;
using iTextSharp.text;
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

        public async Task<int> GetDevicesDataLength()
        {
            var token = await _authService.GetAccessTokenAsync();

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var allDevices = new List<DtoDevicesData>();
            int pageIndex = 1;
            int pageSize = 1; ///5000 maximum allowed by the API
            var name = "";
            bool more = true;

            int contentLength = 5000;  //pageSize = contentLength % pageSize;

            string url = $"https://iot.mts.rs/thingpark/wireless/rest/subscriptions/mine/devices";

            var urlWithParams = $"{url}?pageIndex={pageIndex}&pageSize={pageSize}";

            string responseBody = "";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(urlWithParams);

                if (response.IsSuccessStatusCode)
                {
                    response.EnsureSuccessStatusCode();
                    responseBody = await response.Content.ReadAsStringAsync();
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


            GenerateChunks(result.Count, 5000);

            return result.Count;

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
            var name = "";
            bool more = true;

            int contentLength = await GetDevicesDataLength();
            List<int> chunks = GenerateChunks(contentLength, pageSize);

            string url = $"https://iot.mts.rs/thingpark/wireless/rest/subscriptions/mine/devices";

            string responseBody = "";

            for (var i = 0; i < chunks.Count; i++)
            {
                string urlWithParams = $"{url}?pageIndex={i}&pageSize={chunks[i]}";

                try
                {
                    HttpResponseMessage response = await _httpClient.GetAsync(urlWithParams);

                    if (response.IsSuccessStatusCode)
                    {
                        response.EnsureSuccessStatusCode();
                        responseBody = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        Console.WriteLine(response);
                        Console.WriteLine(pageIndex);

                        if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                        {
                            pageSize = contentLength % pageSize;

                            var newUrl = $"{url}?&pageSize={pageSize}";
                            HttpResponseMessage responseNew = await _httpClient.GetAsync(newUrl);
                            if (responseNew.IsSuccessStatusCode)
                            {
                                responseNew.EnsureSuccessStatusCode();
                                responseBody = await responseNew.Content.ReadAsStringAsync();
                                more = false;
                            }
                        }
                        else
                        {
                            return allDevices;
                        }
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

                allDevices.Add(result);
            }

            return allDevices;
        }

        public async Task<string> GetDevicesDataByUrl(string item, bool IsAuthenticated)
        {
            if (!IsAuthenticated)
            {
                var token = await _authService.GetAccessTokenAsync();

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            string url = $"https://iot.mts.rs" + item;
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            return content;
        }
        public static List<int> GenerateChunks(int listLength, int pageSize)
        {
            var chunkSize = listLength / pageSize;
            var remainder = listLength % pageSize;

            var result = new List<int>();

            for (int i = 0; i < chunkSize; i++)
            {
                result.Add(pageSize);
            }

            if (remainder > 0)
            {
                result.Add(remainder);
            }

            return result;
        }

    }
}
