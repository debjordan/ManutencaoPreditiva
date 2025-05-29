using ManutencaoPreditiva.Api.Application.Services;
using ManutencaoPreditiva.Api.Application.DTOs;
using ManutencaoPreditiva.Api.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace ManutencaoPreditiva.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SensorsController : ControllerBase
{
    private readonly ISensorService _sensorService;

    public SensorsController(ISensorService sensorService)
    {
        _sensorService = sensorService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _sensorService.GetSensorsAsync();
        return result.IsSuccess
            ? Ok(result.Value)
            : ResponseHandler.HandlerResult(result);
    }
}
