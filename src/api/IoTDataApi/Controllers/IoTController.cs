using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IoTDataApi.Data;

namespace IoTDataApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class IoTController : ControllerBase
{
    private readonly IoTDataContext _context;

    public IoTController(IoTDataContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllData()
    {
        var data = await _context.IoTData
            .OrderByDescending(d => d.ReceivedAt)
            .Take(100)
            .ToListAsync();
        return Ok(data);
    }

    [HttpGet("machine/{machineId}")]
    public async Task<IActionResult> GetMachineData(string machineId)
    {
        var data = await _context.IoTData
            .Where(d => d.Topic.Contains(machineId))
            .OrderByDescending(d => d.ReceivedAt)
            .Take(100)
            .ToListAsync();
        return Ok(data);
    }
}
