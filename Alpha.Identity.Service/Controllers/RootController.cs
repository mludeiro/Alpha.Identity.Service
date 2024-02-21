using Microsoft.AspNetCore.Mvc;

namespace Alpha.Identity.Controllers;

[ApiExplorerSettings(IgnoreApi=true)]
[Route("/")]
public class RootController : ControllerBase
{

    [HttpGet]
    public ObjectResult Get()
    {
        return Ok("Identity Server");
    }
}