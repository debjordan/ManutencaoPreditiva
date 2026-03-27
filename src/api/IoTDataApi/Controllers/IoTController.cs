using Microsoft.AspNetCore.Mvc;
using IoTDataApi.Application.Interfaces;
using IoTDataApi.Application.DTOs;
using IoTDataApi.Domain.Entities;

namespace IoTDataApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class IoTController : ControllerBase
{
    private readonly IIoTDataService _iotDataService;
    private static readonly System.Text.RegularExpressions.Regex SafeId =
        new(@"^[A-Za-z0-9_-]{1,20}$", System.Text.RegularExpressions.RegexOptions.Compiled);

    public IoTController(IIoTDataService iotDataService)
    {
        _iotDataService = iotDataService;
    }

    // ── raw records (legacy) ─────────────────────────────────────────────────

    [HttpGet]
    public async Task<ActionResult<IEnumerable<IoTData>>> GetAllData()
    {
        var data = await _iotDataService.GetAllDataAsync();
        return Ok(data);
    }

    [HttpGet("machine/{machineId}")]
    public async Task<ActionResult<IEnumerable<IoTData>>> GetMachineData(string machineId)
    {
        if (!SafeId.IsMatch(machineId)) return BadRequest("machineId inválido.");
        var data = await _iotDataService.GetMachineDataAsync(machineId);
        return Ok(data);
    }

    // ── typed readings ───────────────────────────────────────────────────────

    [HttpGet("machine/{machineId}/readings")]
    public async Task<ActionResult<IEnumerable<SensorReadingDto>>> GetMachineReadings(
        string machineId, [FromQuery] int limit = 50)
    {
        if (!SafeId.IsMatch(machineId)) return BadRequest("machineId inválido.");
        limit = Math.Clamp(limit, 1, 200);
        var readings = await _iotDataService.GetMachineReadingsAsync(machineId, limit);
        return Ok(readings);
    }

    // ── aggregated stats ─────────────────────────────────────────────────────

    [HttpGet("stats")]
    public async Task<ActionResult<IEnumerable<MachineStatsDto>>> GetAllStats()
    {
        var stats = await _iotDataService.GetAllMachineStatsAsync();
        return Ok(stats);
    }

    [HttpGet("stats/{machineId}")]
    public async Task<ActionResult<MachineStatsDto>> GetMachineStats(string machineId)
    {
        if (!SafeId.IsMatch(machineId)) return BadRequest("machineId inválido.");
        var stats = await _iotDataService.GetMachineStatsAsync(machineId);
        return stats is null ? NotFound() : Ok(stats);
    }

    // ── active alerts ────────────────────────────────────────────────────────

    [HttpGet("alerts")]
    public async Task<ActionResult<IEnumerable<AlertDto>>> GetActiveAlerts()
    {
        var alerts = await _iotDataService.GetActiveAlertsAsync();
        return Ok(alerts);
    }
}
