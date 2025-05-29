using Microsoft.AspNetCore.Mvc;

namespace ManutencaoPreditiva.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MachinesController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { Message = "API funcionando!" });
    }
}
