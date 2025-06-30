using ManutencaoPreditiva.Application.Common;
using ManutencaoPreditiva.Application.DTOs.Request;
using ManutencaoPreditiva.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ManutencaoPreditiva.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SectorController : ControllerBase
{
    private readonly ISectorApplicationService _sectorApplicationService;

    public SectorController(ISectorApplicationService sectorApplicationService)
    {
        _sectorApplicationService = sectorApplicationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllSectorsAsync()
    {
        var result = await _sectorApplicationService.GetAllSectorsAsync();

        if (!result.IsSuccess)
            return ResponseHandler.HandlerResult(result);

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetSectorByIdAsync(Guid id)
    {
        var result = await _sectorApplicationService.GetSectorByIdAsync(id);

        if (!result.IsSuccess)
            return ResponseHandler.HandlerResult(result);

        return Ok(result.Value);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveSectosAsync()
    {
        var result = await _sectorApplicationService.GetActiveSectorsAsync();

        if (!result.IsSuccess)
            return ResponseHandler.HandlerResult(result);

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSectorAsync([FromBody] CreateSectorDto dto)
    {
        var result = await _sectorApplicationService.CreateSectorAsync(dto);

        if (!result.IsSuccess)
            return ResponseHandler.HandlerResult(result);

        return CreatedAtAction(
            nameof(GetSectorByIdAsync),
            new { id = result.Value!.Id },
            result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateSectorByIdAsync(Guid id, [FromBody] UpdateSectorDto dto)
    {
        var result = await _sectorApplicationService.UpdateSectorByIdAsync(id, dto);

        if (!result.IsSuccess)
            return ResponseHandler.HandlerResult(result);

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteSectorByIdAsync(Guid id)
    {
        var result = await _sectorApplicationService.DeleteSectorByIdAsync(id);

        if (!result.IsSuccess)
            return ResponseHandler.HandlerResult(result);

        return NoContent();
    }
}
