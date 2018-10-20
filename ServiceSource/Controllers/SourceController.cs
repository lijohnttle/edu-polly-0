using System;
using System.Diagnostics;
using System.Net;
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
        private readonly IAsyncPolicy<HttpResponseMessage> _httpRequestFallbackPolicy;


        public SourceController()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5001")
            };

            _httpRetryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(
                    3,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) / 2),
                    onRetry: (response, retryMarker) =>
                    {
                        Trace.WriteLine($"Retry #{retryMarker}...");
                    });

            _httpRequestFallbackPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .FallbackAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("Fallback")
                });
        }


        [HttpGet("Retry")]
        public async Task<IActionResult> Retry()
        {
            const string requestEndpoint = "retry";

            var response = await _httpRetryPolicy.ExecuteAsync(() =>
                _httpClient.GetAsync(requestEndpoint));

            if (response.IsSuccessStatusCode)
            {
                return Ok(await response.Content.ReadAsStringAsync());
            }

            return StatusCode((int) response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        [HttpGet("Fallback")]
        public async Task<IActionResult> Fallback()
        {
            const string requestEndpoint = "retry";

            var response = await _httpRequestFallbackPolicy.ExecuteAsync(() =>
                _httpClient.GetAsync(requestEndpoint));

            if (response.IsSuccessStatusCode)
            {
                return Ok(await response.Content.ReadAsStringAsync());
            }

            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }
    }
}
