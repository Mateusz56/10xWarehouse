using Microsoft.AspNetCore.Mvc;

namespace _10xWarehouseNet.Controllers
{
    [ApiController]
    [Route("api/")]
    public class HealthController : ControllerBase
    {
        [HttpGet("up")]
        public IActionResult Up()
        {
            return Ok(new { status = "up" });
        }
    }
}

