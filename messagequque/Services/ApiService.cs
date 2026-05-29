using messagequque.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace messagequque.Services
{
    public class ApiService : IApiService
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<ApiService> _logger;

        public ApiService(
            IHttpClientFactory httpFactory,
            IConfiguration config,
            ILogger<ApiService> logger)
        {
            _httpFactory = httpFactory;
            _config = config;
            _logger = logger;
        }

        public async Task<bool> SendAsync(IEnumerable<TelemetryData> rows, CancellationToken cancellationToken)
        {
            try
            {
                var client = _httpFactory.CreateClient();

                var response = await client.PostAsJsonAsync(
                    _config["Api:Url"],
                    rows,
                    cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                _logger.LogWarning(
                    "API Error: {StatusCode}",
                    response.StatusCode);

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API Send Error");

                return false;
            }
        }
    }
}
