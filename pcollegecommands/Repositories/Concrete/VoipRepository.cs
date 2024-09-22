using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Flyurdreamcommands.Models.Datafields;
using Flyurdreamcommands.Repositories.Abstract;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace Flyurdreamcommands.Repositories.Concrete
{

    public class VoipRepository : DataRepositoryBase, IVoipRepository
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;
        public VoipRepository(IConfiguration config, ILogger<AgentRepository> logger, HttpClient httpClient) : base(logger, config)
        {
            _config = config;
            _logger = logger;
            _httpClient = httpClient;
        }


        public async Task<JsonObject> GetRequestYay()
        {
            JsonObject jsonResponse = null;
            try
            {
                var requestUri = "https://sandbox.api.yay.com";
                var apiCredentials = new Dictionary<string, string>
         {{ "X-Auth-Reseller", "4176a4c8183e4953869c" },
           { "X-Auth-User", "sandbox_admin" },
           { "X-Auth-Password", "95798e4909" } };
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(requestUri);
                    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("ApiExample", "1.0"));

                    foreach (var key in apiCredentials.Keys)
                    {
                        client.DefaultRequestHeaders.Add(key, apiCredentials[key]);
                    }

                    var response = await client.GetAsync("/authenticated");

                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();
                        var jsonDocument = JsonDocument.Parse(responseString);
                        // Check if the root element is a JSON array
                        if (jsonDocument.RootElement.ValueKind == JsonValueKind.Object)
                        {
                            jsonResponse = JsonNode.Parse(responseString).AsObject();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return jsonResponse;
        }

        public async Task<string> InitiateCallAsync()
        {
            var displayName = "Yay Support Line";
            var dialCode = "123";
            var targetNumber = "++447752344143";

            var requestUri = "https://sandbox.api.yay.com";
            var apiCredentials = new Dictionary<string, string>
         {{ "X-Auth-Reseller", "4176a4c8183e4953869c" },
           { "X-Auth-User", "sandbox_admin" },
           { "X-Auth-Password", "95798e4909" } };

            // Clear existing headers and add new headers
            _httpClient.DefaultRequestHeaders.Clear();
            foreach (var key in apiCredentials.Keys)
            {
                _httpClient.DefaultRequestHeaders.Add(key, apiCredentials[key]);
            }
            // Add the User-Agent header
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("ApiExample/1.0");
            var callData = new
            {
                display_name = displayName,
                dial_code = dialCode,
                target_number = targetNumber
            };

            var content = new StringContent(JsonSerializer.Serialize(callData), System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(requestUri, content);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                return responseString; // Ideally parse and return a more structured result
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to initiate call: {response.StatusCode}, {errorResponse}");
            }
        }
    }
    }