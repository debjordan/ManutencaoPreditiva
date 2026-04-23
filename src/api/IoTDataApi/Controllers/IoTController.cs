using Microsoft.AspNetCore.Mvc;
using IoTDataApi.Application.Interfaces;
using IoTDataApi.Application.DTOs;
using IoTDataApi.Domain.Entities;

namespace IoTDataApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class IoTController(
    IIoTDataService iotDataService,
    IRulService     rulService,
    ITrendService   trendService,
    IDowntimeService downtimeService) : ControllerBase
{
    private static readonly System.Text.RegularExpressions.Regex SafeId =
        new(@"^[A-Za-z0-9_-]{1,20}$", System.Text.RegularExpressions.RegexOptions.Compiled);

    // ── raw records ──────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<ActionResult<IEnumerable<IoTData>>> GetAllData()
        => Ok(await iotDataService.GetAllDataAsync());

    [HttpGet("machine/{machineId}")]
    public async Task<ActionResult<IEnumerable<IoTData>>> GetMachineData(string machineId)
    {
        if (!SafeId.IsMatch(machineId)) return BadRequest("machineId inválido.");
        return Ok(await iotDataService.GetMachineDataAsync(machineId));
    }

    // ── typed readings ───────────────────────────────────────────────────────

    [HttpGet("machine/{machineId}/readings")]
    public async Task<ActionResult<IEnumerable<SensorReadingDto>>> GetMachineReadings(
        string machineId, [FromQuery] int limit = 50)
    {
        if (!SafeId.IsMatch(machineId)) return BadRequest("machineId inválido.");
        limit = Math.Clamp(limit, 1, 200);
        return Ok(await iotDataService.GetMachineReadingsAsync(machineId, limit));
    }

    // ── aggregated stats ─────────────────────────────────────────────────────

    [HttpGet("stats")]
    public async Task<ActionResult<IEnumerable<MachineStatsDto>>> GetAllStats()
        => Ok(await iotDataService.GetAllMachineStatsAsync());

    [HttpGet("stats/{machineId}")]
    public async Task<ActionResult<MachineStatsDto>> GetMachineStats(string machineId)
    {
        if (!SafeId.IsMatch(machineId)) return BadRequest("machineId inválido.");
        var stats = await iotDataService.GetMachineStatsAsync(machineId);
        return stats is null ? NotFound() : Ok(stats);
    }

    // ── active alerts ────────────────────────────────────────────────────────

    [HttpGet("alerts")]
    public async Task<ActionResult<IEnumerable<AlertDto>>> GetActiveAlerts()
        => Ok(await iotDataService.GetActiveAlertsAsync());

    // ── RUL — Remaining Useful Life ──────────────────────────────────────────

    [HttpGet("rul")]
    public async Task<ActionResult<IEnumerable<RulDto>>> GetAllRul()
        => Ok(await rulService.GetForAllMachinesAsync());

    [HttpGet("machine/{machineId}/rul")]
    public async Task<ActionResult<RulDto>> GetMachineRul(string machineId)
    {
        if (!SafeId.IsMatch(machineId)) return BadRequest("machineId inválido.");
        var rul = await rulService.GetForMachineAsync(machineId);
        return rul is null ? NotFound() : Ok(rul);
    }

    // ── sensor trends ────────────────────────────────────────────────────────

    [HttpGet("trends")]
    public async Task<ActionResult<IEnumerable<MachineTrendsDto>>> GetAllTrends()
        => Ok(await trendService.GetForAllMachinesAsync());

    [HttpGet("machine/{machineId}/trends")]
    public async Task<ActionResult<MachineTrendsDto>> GetMachineTrends(string machineId)
    {
        if (!SafeId.IsMatch(machineId)) return BadRequest("machineId inválido.");
        var trends = await trendService.GetForMachineAsync(machineId);
        return trends is null ? NotFound() : Ok(trends);
    }

    // ── downtime / MTBF / MTTR ───────────────────────────────────────────────

    [HttpGet("downtime")]
    public async Task<ActionResult<IEnumerable<DowntimeSummaryDto>>> GetAllDowntime()
        => Ok(await downtimeService.GetForAllMachinesAsync());

    [HttpGet("machine/{machineId}/downtime")]
    public async Task<ActionResult<DowntimeSummaryDto>> GetMachineDowntime(string machineId)
    {
        if (!SafeId.IsMatch(machineId)) return BadRequest("machineId inválido.");
        var summary = await downtimeService.GetForMachineAsync(machineId);
        return summary is null ? NotFound() : Ok(summary);
    }
}
