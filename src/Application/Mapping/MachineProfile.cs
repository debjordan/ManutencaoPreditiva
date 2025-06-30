using AutoMapper;
using ManutencaoPreditiva.Application.DTOs.Response;
using ManutencaoPreditiva.Domain.Entities;

namespace ManutencaoPreditiva.Application.Mappings;

public class MachineProfile : Profile
{
    public MachineProfile()
    {
        CreateMap<Machine, MachineDto>()
            .ForMember(dest => dest.RequiresMaintenance, opt => opt.MapFrom(src => src.RequiresMaintenance()))
            .ForMember(dest => dest.DaysSinceLastMaintenance, opt => opt.MapFrom(src => src.DaysSinceLastMaintenance()));
    }
}
