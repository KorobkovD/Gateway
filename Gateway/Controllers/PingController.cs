using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers;

[Route("[controller]")]
public class PingController : Controller
{
    // GET
    public IActionResult Index()
    {
        return Ok("gateway pong");
    }
}