using Microsoft.AspNetCore.Mvc;
using IoTDataApi.Application.Interfaces;
using IoTDataApi.Domain.Entities;

namespace IoTDataApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class IoTController : ControllerBase
{
    private readonly IIoTDataService _iotDataService;

    public IoTController(IIoTDataService iotDataService)
    {
        _iotDataService = iotDataService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<IoTData>>> GetAllData()
    {
        var data = await _iotDataService.GetAllDataAsync();
        return Ok(data);
    }

    [HttpGet("machine/{machineId}")]
    public async Task<ActionResult<IEnumerable<IoTData>>> GetMachineData(string machineId)
    {
        var data = await _iotDataService.GetMachineDataAsync(machineId);
        return Ok(data);
    }
}