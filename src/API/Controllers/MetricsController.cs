// using ManutencaoPreditiva.Api.Application.Services;
// using Microsoft.AspNetCore.Mvc;

// namespace ManutencaoPreditiva.Api.Controllers;

// [ApiController]
// [Route("api/[controller]")]
// public class MetricsController : ControllerBase
// {
//     private readonly ProductionService _productionService;
//     public MetricsController(ProductionService productionService)
//     {
//         _productionService = productionService;
//     }
//     [HttpGet]
//     public async Task<IActionResult> Get()
//     {
//         try
//         {
//             var metrics = await _productionService.GetMetricsAsync();
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine($"Error in /api/metrics: {ex.Message}");
//             return Problem("Error processing metrics");
//         }
//     }
// }
