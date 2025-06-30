// using ManutencaoPreditiva.Application.Common;
// using ManutencaoPreditiva.Application.Interfaces.Services;
// using ManutencaoPreditiva.Domain.Common;
// using Microsoft.AspNetCore.Mvc;

// namespace ManutencaoPreditiva.Api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public abstract class BaseController<TService, TDto, TCreateDto, TUpdateDto> : ControllerBase
//         where TService : IBaseApplicationService<TDto, TCreateDto, TUpdateDto>
//     {
//         protected readonly TService _service;

//         public BaseController(TService service)
//         {
//             _service = service;
//         }

//         [HttpGet]
//         public virtual async Task<IActionResult> GetAllAsync()
//         {
//             var result = await _service.GetAllAsync();
//             return HandleResult(result);
//         }

//         [HttpGet("{id:guid}")]
//         public virtual async Task<IActionResult> GetByIdAsync(Guid id)
//         {
//             var result = await _service.GetByIdAsync(id);
//             return HandleResult(result);
//         }

//         [HttpGet("active")]
//         public virtual async Task<IActionResult> GetActiveAsync()
//         {
//             var result = await _service.GetActiveAsync();
//             return HandleResult(result);
//         }

//         [HttpPost]
//         public virtual async Task<IActionResult> CreateAsync([FromBody] TCreateDto dto)
//         {
//             var result = await _service.CreateAsync(dto);

//             if (!result.IsSuccess)
//                 return HandleResult(result);

//             return CreatedAtAction(
//                 nameof(GetByIdAsync),
//                 new { id = result.Value!.Id },
//                 result.Value);
//         }

//         [HttpPut("{id:guid}")]
//         public virtual async Task<IActionResult> UpdateAsync(Guid id, [FromBody] TUpdateDto dto)
//         {
//             var result = await _service.UpdateAsync(id, dto);
//             return HandleResult(result);
//         }

//         [HttpDelete("{id:guid}")]
//         public virtual async Task<IActionResult> DeleteAsync(Guid id)
//         {
//             var result = await _service.DeleteAsync(id);

//             if (result.IsSuccess)
//                 return NoContent();

//             return HandleResult(result);
//         }

//         protected IActionResult HandleResult<T>(Result<T> result)
//         {
//             if (!result.IsSuccess)
//                 return ResponseHandler.HandlerResult(result);

//             return Ok(result.Value);
//         }
//     }
// }
