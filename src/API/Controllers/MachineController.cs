using ManutencaoPreditiva.Application.Common;
using ManutencaoPreditiva.Application.DTOs.Request;
using ManutencaoPreditiva.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ManutencaoPreditiva.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MachinesController : ControllerBase
{
    private readonly IMachineApplicationService _machineApplicationService;

    public MachinesController(IMachineApplicationService machineApplicationService)
    {
        _machineApplicationService = machineApplicationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllMachinesAsync()
    {
        var result = await _machineApplicationService.GetAllMachinesAsync();

        if (!result.IsSuccess)
            return ResponseHandler.HandlerResult(result);

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetMachineByIdAsync(Guid id)
    {
        var result = await _machineApplicationService.GetMachineByIdAsync(id);

        if (!result.IsSuccess)
            return ResponseHandler.HandlerResult(result);

        return Ok(result.Value);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveMachinesAsync()
    {
        var result = await _machineApplicationService.GetActiveMachinesAsync();

        if (!result.IsSuccess)
            return ResponseHandler.HandlerResult(result);

        return Ok(result.Value);
    }

    [HttpGet("location/{location}")]
    public async Task<IActionResult> GetMachinesByLocationAsync(string location)
    {
        var result = await _machineApplicationService.GetMachinesByLocationAsync(location);

        if (!result.IsSuccess)
            return ResponseHandler.HandlerResult(result);

        return Ok(result.Value);
    }

    [HttpGet("requiring-maintenance")]
    public async Task<IActionResult> GetMachinesRequiringMaintenanceAsync()
    {
        var result = await _machineApplicationService.GetMachinesRequiringMaintenanceAsync();

        if (!result.IsSuccess)
            return ResponseHandler.HandlerResult(result);

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> CreateMachineAsync([FromBody] CreateMachineDto dto)
    {
        var result = await _machineApplicationService.CreateMachineAsync(dto);

        if (!result.IsSuccess)
            return ResponseHandler.HandlerResult(result);

        return CreatedAtAction(
            nameof(GetMachineByIdAsync),
            new { id = result.Value!.Id },
            result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateMachineByIdAsync(Guid id, [FromBody] UpdateMachineDto dto)
    {
        var result = await _machineApplicationService.UpdateMachineByIdAsync(id, dto);

        if (!result.IsSuccess)
            return ResponseHandler.HandlerResult(result);

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteMachineByIdAsync(Guid id)
    {
        var result = await _machineApplicationService.DeleteMachineByIdAsync(id);

        if (!result.IsSuccess)
            return ResponseHandler.HandlerResult(result);

        return NoContent();
    }

    [HttpPost("{id:guid}/schedule-maintenance")]
    public async Task<IActionResult> ScheduleMaintenanceAsync(Guid id, [FromBody] DateTime maintenanceDate)
    {
        var result = await _machineApplicationService.ScheduleMaintenanceAsync(id, maintenanceDate);

        if (!result.IsSuccess)
            return ResponseHandler.HandlerResult(result);

        return Ok(new { message = "Manutenção agendada com sucesso" });
    }

    [HttpPut("{id:guid}/update-maintenance")]
    public async Task<IActionResult> UpdateMaintenanceAsync(Guid id, [FromBody] DateTime maintenanceDate)
    {
        var result = await _machineApplicationService.UpdateMaintenanceAsync(id, maintenanceDate);

        if (!result.IsSuccess)
            return ResponseHandler.HandlerResult(result);

        return Ok(new { message = "Manutenção atualizada com sucesso" });
    }
}
