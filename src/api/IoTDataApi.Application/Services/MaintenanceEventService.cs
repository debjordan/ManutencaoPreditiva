using IoTDataApi.Application.DTOs;
using IoTDataApi.Application.Interfaces;
using IoTDataApi.Domain.Entities;
using IoTDataApi.Domain.Interfaces;

namespace IoTDataApi.Application.Services;

public class MaintenanceEventService : IMaintenanceEventService
{
    private readonly IMaintenanceEventRepository _repo;

    public MaintenanceEventService(IMaintenanceEventRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<MaintenanceEventDto>> GetAllEventsAsync()
        => (await _repo.GetAllAsync()).Select(ToDto);

    public async Task<IEnumerable<MaintenanceEventDto>> GetMachineEventsAsync(string machineId)
        => (await _repo.GetByMachineIdAsync(machineId)).Select(ToDto);

    public async Task<MaintenanceEventDto?> GetEventByIdAsync(int id)
    {
        var ev = await _repo.GetByIdAsync(id);
        return ev is null ? null : ToDto(ev);
    }

    public async Task<MaintenanceEventDto> CreateEventAsync(CreateMaintenanceEventDto dto)
    {
        var ev = new MaintenanceEvent
        {
            MachineId      = dto.MachineId,
            MachineName    = dto.MachineName,
            Type           = dto.Type,
            StartedAt      = DateTime.UtcNow.ToString("o"),
            Notes          = dto.Notes,
            TechnicianName = dto.TechnicianName,
        };
        var created = await _repo.AddAsync(ev);
        return ToDto(created);
    }

    public async Task<MaintenanceEventDto?> ResolveEventAsync(int id, ResolveMaintenanceEventDto dto)
    {
        var ev = await _repo.GetByIdAsync(id);
        if (ev is null) return null;

        ev.ResolvedAt = DateTime.UtcNow.ToString("o");
        if (dto.Notes is not null) ev.Notes = dto.Notes;

        var updated = await _repo.UpdateAsync(ev);
        return updated is null ? null : ToDto(updated);
    }

    private static MaintenanceEventDto ToDto(MaintenanceEvent ev)
    {
        double? duration = null;
        if (ev.ResolvedAt is not null
            && DateTime.TryParse(ev.StartedAt,  null, System.Globalization.DateTimeStyles.RoundtripKind, out var start)
            && DateTime.TryParse(ev.ResolvedAt, null, System.Globalization.DateTimeStyles.RoundtripKind, out var end))
        {
            duration = Math.Round((end - start).TotalMinutes, 1);
        }

        return new MaintenanceEventDto
        {
            Id              = ev.Id,
            MachineId       = ev.MachineId,
            MachineName     = ev.MachineName,
            Type            = ev.Type,
            StartedAt       = ev.StartedAt,
            ResolvedAt      = ev.ResolvedAt,
            DurationMinutes = duration,
            Notes           = ev.Notes,
            TechnicianName  = ev.TechnicianName,
            IsOpen          = ev.ResolvedAt is null,
        };
    }
}
