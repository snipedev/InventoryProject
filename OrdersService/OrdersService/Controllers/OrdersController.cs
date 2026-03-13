using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace OrdersService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        [HttpGet]
        public IActionResult get()
        {
            return Ok("landed home");
        }
    }
}
