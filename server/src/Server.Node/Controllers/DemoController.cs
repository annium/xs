using Microsoft.AspNetCore.Mvc;

namespace Server.Node.Controllers;

[Area(Constants.Project)]
[Route("[area]/[controller]")]
public class DemoController : ControllerBase
{
    [HttpGet]
    public IActionResult Demo()
    {
        return Ok("node");
    }
}