using Microsoft.AspNetCore.Mvc;
using Server.Node.Internal;

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