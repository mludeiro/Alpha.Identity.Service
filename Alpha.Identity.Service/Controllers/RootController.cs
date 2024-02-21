using Microsoft.AspNetCore.Mvc;

namespace Alpha.Identity.Controllers;

[Route("/")]
public class RootController : ControllerBase
{

    [HttpGet]
    public ObjectResult Get()
    {
        return Ok("Identity Server");
    }
}