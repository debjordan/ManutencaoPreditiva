using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using ManutencaoPreditiva.Application.DTOs.Request;
using ManutencaoPreditiva.Application.DTOs.Response;
using ManutencaoPreditiva.Application.Interfaces.Services;
using ManutencaoPreditiva.Domain.Common;
using ManutencaoPreditiva.Domain.Entities;
using ManutencaoPreditiva.Domain.Interfaces.Services;
using ManutencaoPreditiva.Domain.ValueObjects;

namespace ManutencaoPreditiva.Application.Services
{
    public class MachineApplicationService : IMachineApplicationService
    {
        private readonly IMachineService _machineService;
        private readonly IMapper _mapper;

        public MachineApplicationService(IMachineService machineService, IMapper mapper)
        {
            _machineService = machineService;
            _mapper = mapper;
        }

        public async Task<Result<MachineDto>> GetMachineByIdAsync(Guid id)
        {
            var result = await _machineService.GetMachineByIdAsync(id);
            if (!result.IsSuccess)
                return Result<MachineDto>.Failure(result.ErrorMessage, result.ErrorType);

            var machineDto = _mapper.Map<MachineDto>(result.Value);
            return Result<MachineDto>.Success(machineDto);
        }

        public async Task<Result<IEnumerable<MachineDto>>> GetAllMachinesAsync()
        {
            var result = await _machineService.GetAllMachinesAsync();
            if (!result.IsSuccess)
                return Result<IEnumerable<MachineDto>>.Failure(result.ErrorMessage, result.ErrorType);

            var machinesDto = _mapper.Map<IEnumerable<MachineDto>>(result.Value);
            return Result<IEnumerable<MachineDto>>.Success(machinesDto);
        }

        public async Task<Result<IEnumerable<MachineDto>>> GetActiveMachinesAsync()
        {
            var result = await _machineService.GetActiveMachinesAsync();
            if (!result.IsSuccess)
                return Result<IEnumerable<MachineDto>>.Failure(result.ErrorMessage, result.ErrorType);

            var machinesDto = _mapper.Map<IEnumerable<MachineDto>>(result.Value);
            return Result<IEnumerable<MachineDto>>.Success(machinesDto);
        }

        public async Task<Result<IEnumerable<MachineDto>>> GetMachinesByLocationAsync(string location)
        {
            var result = await _machineService.GetMachinesByLocationAsync(location);
            if (!result.IsSuccess)
                return Result<IEnumerable<MachineDto>>.Failure(result.ErrorMessage, result.ErrorType);

            var machinesDto = _mapper.Map<IEnumerable<MachineDto>>(result.Value);
            return Result<IEnumerable<MachineDto>>.Success(machinesDto);
        }

        public async Task<Result<IEnumerable<MachineDto>>> GetMachinesRequiringMaintenanceAsync()
        {
            var result = await _machineService.GetMachinesRequiringMaintenanceAsync();
            if (!result.IsSuccess)
                return Result<IEnumerable<MachineDto>>.Failure(result.ErrorMessage, result.ErrorType);

            var machinesDto = _mapper.Map<IEnumerable<MachineDto>>(result.Value);
            return Result<IEnumerable<MachineDto>>.Success(machinesDto);
        }

        public async Task<Result<MachineDto>> CreateMachineAsync(CreateMachineDto dto)
        {
            var serialNumber = new SerialNumber(dto.SerialNumber);
            var machine = new Machine(
                dto.Name,
                serialNumber,
                dto.Model,
                dto.Manufacturer,
                dto.Location,
                dto.InstallationDate,
                dto.Description);

            var result = await _machineService.CreateMachineAsync(machine);
            if (!result.IsSuccess)
                return Result<MachineDto>.Failure(result.ErrorMessage, result.ErrorType);

            var machineDto = _mapper.Map<MachineDto>(result.Value);
            return Result<MachineDto>.Success(machineDto);
        }

        public async Task<Result<MachineDto>> UpdateMachineByIdAsync(Guid id, UpdateMachineDto dto)
        {
            var existingMachineResult = await _machineService.GetMachineByIdAsync(id);
            if (!existingMachineResult.IsSuccess)
                return Result<MachineDto>.Failure(existingMachineResult.ErrorMessage, existingMachineResult.ErrorType);

            var machine = existingMachineResult.Value;
            machine.UpdateInfo(dto.Name, dto.Model, dto.Manufacturer, dto.Location, dto.Description);

            var result = await _machineService.UpdateMachineAsync(machine);
            if (!result.IsSuccess)
                return Result<MachineDto>.Failure(result.ErrorMessage, result.ErrorType);

            var machineDto = _mapper.Map<MachineDto>(result.Value);
            return Result<MachineDto>.Success(machineDto);
        }

        public async Task<Result<bool>> DeleteMachineByIdAsync(Guid id)
        {
            return await _machineService.DeleteMachineAsync(id);
        }

        public async Task<Result<bool>> ScheduleMaintenanceAsync(Guid machineId, DateTime maintenanceDate)
        {
            return await _machineService.ScheduleMaintenanceAsync(machineId, maintenanceDate);
        }

        public async Task<Result<bool>> UpdateMaintenanceAsync(Guid machineId, DateTime maintenanceDate)
        {
            return await _machineService.UpdateMaintenanceAsync(machineId, maintenanceDate);
        }
    }
}
