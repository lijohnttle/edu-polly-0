using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Polly;

namespace ServiceSource.Controllers
{
    [ApiController]
    public class SourceController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IAsyncPolicy<HttpResponseMessage> _httpRetryPolicy;


        public SourceController()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5001")
            };

            _httpRetryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) / 2));
        }


        [HttpGet("Retry")]
        public async Task<IActionResult> Retry()
        {
            const string requestEndpoint = "retry";

            var response = await _httpRetryPolicy.ExecuteAsync(() =>
                _httpClient.GetAsync(requestEndpoint));

            if (response.IsSuccessStatusCode)
            {
                return Ok("Success");
            }

            return StatusCode((int) response.StatusCode, response.Content.ReadAsStringAsync());
        }
    }
}
