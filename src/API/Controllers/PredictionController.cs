// using ManutencaoPreditiva.Api.Application.Services;
// using Microsoft.AspNetCore.Mvc;

// namespace ManutencaoPreditiva.Api.Controllers;

// [ApiController]
// [Route("api/[controller]")]
// public class PredictionsController : ControllerBase
// {
//     private readonly PredictionService _predictionService;
//     public PredictionsController(PredictionService predictionService)
//     {
//         _predictionService = predictionService;
//     }
//     [HttpGet]
//     public async Task<IActionResult> Get()
//     {
//         try
//         {
//             var probability = await _predictionService.PredictFailureAsync();
//             return Ok(new { FailureProbability = probability });
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine($"Error in /api/predict: {ex.Message}");
//             return Problem("Error predicting failure");
//         }
//     }
// }
