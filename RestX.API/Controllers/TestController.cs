using Microsoft.AspNetCore.Mvc;

namespace RestX.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        /// <summary>
        /// Test endpoint để kiểm tra API hoạt động
        /// </summary>
        /// <returns>Test response</returns>
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new 
            { 
                success = true, 
                message = "API is working!", 
                timestamp = DateTime.UtcNow,
                server = Environment.MachineName
            });
        }

        /// <summary>
        /// Test endpoint với parameter
        /// </summary>
        /// <param name="name">Tên để test</param>
        /// <returns>Greeting message</returns>
        [HttpGet("hello/{name}")]
        public IActionResult Hello(string name)
        {
            return Ok(new 
            { 
                success = true, 
                message = $"Hello, {name}!", 
                timestamp = DateTime.UtcNow 
            });
        }

        /// <summary>
        /// Test POST endpoint
        /// </summary>
        /// <param name="request">Test data</param>
        /// <returns>Echo response</returns>
        [HttpPost("echo")]
        public IActionResult Echo([FromBody] TestRequest request)
        {
            return Ok(new 
            { 
                success = true, 
                message = "Echo response", 
                receivedData = request,
                timestamp = DateTime.UtcNow 
            });
        }
    }

    public class TestRequest
    {
        public string? Message { get; set; }
        public int Number { get; set; }
    }
}

