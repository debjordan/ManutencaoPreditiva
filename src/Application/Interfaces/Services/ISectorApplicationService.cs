using ManutencaoPreditiva.Application.DTOs.Request;
using ManutencaoPreditiva.Application.DTOs.Response;
using ManutencaoPreditiva.Domain.Common;

namespace ManutencaoPreditiva.Application.Interfaces.Services;

public interface ISectorApplicationService
{
    Task<Result<SectorDto>> GetSectorByIdAsync(Guid id);
    Task<Result<IEnumerable<SectorDto>>> GetAllSectorsAsync();
    Task<Result<IEnumerable<SectorDto>>> GetActiveSectorsAsync();
    Task<Result<SectorDto>> CreateSectorAsync(CreateSectorDto dto);
    Task<Result<SectorDto>> UpdateSectorByIdAsync(Guid id, UpdateSectorDto dto);
    Task<Result<bool>> DeleteSectorByIdAsync(Guid id);
}
