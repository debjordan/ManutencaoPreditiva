// using AutoMapper;
// using ManutencaoPreditiva.Application.DTOs.Request;
// using ManutencaoPreditiva.Application.DTOs.Response;
// using ManutencaoPreditiva.Application.Interfaces.Services;
// using ManutencaoPreditiva.Domain.Common;
// using ManutencaoPreditiva.Domain.Entities;
// using ManutencaoPreditiva.Domain.Interfaces.Services;

// namespace ManutencaoPreditiva.Application.Services;

// public class SectorApplicationService : ISectorApplicationService
// {
//     private readonly ISectorService _sectorService;
//     private readonly IMapper _mapper;

//     public SectorApplicationService(ISectorService sectorService, IMapper mapper)
//     {
//         _sectorService = sectorService;
//         _mapper = mapper;
//     }

//     public async Task<Result<SectorDto>> GetSectorByIdAsync(Guid id)
//     {
//         var result = await _sectorService.GetSectorByIdAsync(id);

//         if (!result.IsSuccess)
//             return Result<SectorDto>.Failure(result.ErrorMessage!);

//         var sectorDto = _mapper.Map<SectorDto>(result.Value);
//         return Result<SectorDto>.Success(sectorDto);
//     }

//     public async Task<Result<IEnumerable<SectorDto>>> GetAllSectorsAsync()
//     {
//         var result = await _sectorService.GetAllSectorsAsync();

//         if (!result.IsSuccess)
//             return Result<IEnumerable<SectorDto>>.Failure(result.ErrorMessage!);

//         var SectorsDto = _mapper.Map<IEnumerable<SectorDto>>(result.Value);
//         return Result<IEnumerable<SectorDto>>.Success(SectorsDto);
//     }

//     public async Task<Result<IEnumerable<SectorDto>>> GetActiveSectorsAsync()
//     {
//         var result = await _sectorService.GetActiveSectorsAsync();

//         if (!result.IsSuccess)
//             return Result<IEnumerable<SectorDto>>.Failure(result.ErrorMessage!);

//         var SectorsDto = _mapper.Map<IEnumerable<SectorDto>>(result.Value);
//         return Result<IEnumerable<SectorDto>>.Success(SectorsDto);
//     }

//     public async Task<Result<SectorDto>> CreateSectorAsync(CreateSectorDto dto)
//     {
//         var sector = new Sector(
//             dto.Name, // trocar aqui as propertys
//             dto.CostCenter);

//         var result = await _sectorService.CreateSectorAsync(sector);

//         if (!result.IsSuccess)
//             return Result<SectorDto>.Failure(result.ErrorMessage!);

//         var SectorDto = _mapper.Map<SectorDto>(result.Value);
//         return Result<SectorDto>.Success(SectorDto);
//     }

//     public async Task<Result<SectorDto>> UpdateSectorByIdAsync(Guid id, UpdateSectorDto dto)
//     {
//         var existingSectorResult = await _sectorService.GetSectorByIdAsync(id);

//         if (!existingSectorResult.IsSuccess)
//             return Result<SectorDto>.Failure(existingSectorResult.ErrorMessage!);

//         var sector = existingSectorResult.Value!;
//         sector.UpdateInfo(dto.Name, dto.CostCenter); // mudar o nome das propertys

//         var result = await _sectorService.UpdateSectorAsync(sector);

//         if (!result.IsSuccess)
//             return Result<SectorDto>.Failure(result.ErrorMessage!);

//         var SectorDto = _mapper.Map<SectorDto>(result.Value);
//         return Result<SectorDto>.Success(SectorDto);
//     }

//     public async Task<Result<bool>> DeleteSectorByIdAsync(Guid id)
//     {
//         return await _sectorService.DeleteSectorAsync(id);
//     }
// }
