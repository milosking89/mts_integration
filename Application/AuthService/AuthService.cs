namespace mts_integration.Application.AuthService
{
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly string _clientId = "sub-1100000405/eds-mc";
        private readonly string _clientSecret = "kwZTiXMslwUaCeYoj2XgUynMYXxAd1LP";
        private readonly string _tokenUrl = "https://iot.mts.rs/users-auth/protocol/openid-connect/token"; // URL za dobijanje tokena

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            var requestBody = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", _clientId },
            { "client_secret", _clientSecret }
        };

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, _tokenUrl)
            {
                Content = new FormUrlEncodedContent(requestBody)
            };

            var response = await _httpClient.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

            return data["access_token"].ToString();
        }
    }

}
