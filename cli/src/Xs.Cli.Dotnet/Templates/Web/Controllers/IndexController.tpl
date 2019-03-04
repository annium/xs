using Microsoft.AspNetCore.Mvc;

namespace {{name}}.Controllers
{
    [Route("/")]
    public class IndexController : ControllerBase
    {
        public IndexController()
        {

        }

        [HttpGet]
        public IActionResult Index()
        {
            return Ok("Hello World from {{name}}");
        }
    }
}