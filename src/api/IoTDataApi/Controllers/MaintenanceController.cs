using Microsoft.AspNetCore.Mvc;
using IoTDataApi.Application.DTOs;
using IoTDataApi.Application.Interfaces;

namespace IoTDataApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MaintenanceController : ControllerBase
{
    private readonly IMaintenanceEventService _service;
    private static readonly System.Text.RegularExpressions.Regex SafeId =
        new(@"^[A-Za-z0-9_-]{1,20}$", System.Text.RegularExpressions.RegexOptions.Compiled);

    public MaintenanceController(IMaintenanceEventService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MaintenanceEventDto>>> GetAll()
        => Ok(await _service.GetAllEventsAsync());

    [HttpGet("machine/{machineId}")]
    public async Task<ActionResult<IEnumerable<MaintenanceEventDto>>> GetByMachine(string machineId)
    {
        if (!SafeId.IsMatch(machineId)) return BadRequest("machineId inválido.");
        return Ok(await _service.GetMachineEventsAsync(machineId));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MaintenanceEventDto>> GetById(int id)
    {
        var ev = await _service.GetEventByIdAsync(id);
        return ev is null ? NotFound() : Ok(ev);
    }

    [HttpPost]
    public async Task<ActionResult<MaintenanceEventDto>> Create([FromBody] CreateMaintenanceEventDto dto)
    {
        if (!SafeId.IsMatch(dto.MachineId)) return BadRequest("machineId inválido.");
        var created = await _service.CreateEventAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPatch("{id:int}/resolve")]
    public async Task<ActionResult<MaintenanceEventDto>> Resolve(int id, [FromBody] ResolveMaintenanceEventDto dto)
    {
        var resolved = await _service.ResolveEventAsync(id, dto);
        return resolved is null ? NotFound() : Ok(resolved);
    }
}
