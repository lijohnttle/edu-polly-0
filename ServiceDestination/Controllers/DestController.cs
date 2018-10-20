using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ServiceDestination.Controllers
{
    [ApiController]
    public class DestController : ControllerBase
    {
        private static int _requestCouter;

        [HttpGet("Retry")]
        public async Task<IActionResult> Retry()
        {
            await Task.Delay(100);

            if (Interlocked.Increment(ref _requestCouter) % 4 == 0)
            {
                return Ok(15);
            }

            return StatusCode((int) HttpStatusCode.InternalServerError, "Something went wrong");
        }
    }
}
