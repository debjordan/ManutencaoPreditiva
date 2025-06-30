using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ManutencaoPreditiva.Domain.Common;
using ManutencaoPreditiva.Domain.Entities;
using ManutencaoPreditiva.Domain.Exceptions;
using ManutencaoPreditiva.Domain.Interfaces.Repositories;
using ManutencaoPreditiva.Domain.Interfaces.Services;

namespace ManutencaoPreditiva.Domain.Services
{
    public class MachineService : IMachineService
    {
        private readonly IMachineRepository _machineRepository;

        public MachineService(IMachineRepository machineRepository)
        {
            _machineRepository = machineRepository;
        }

        public async Task<Result<Machine>> GetMachineByIdAsync(Guid id)
        {
            var machine = await _machineRepository.GetByIdAsync(id);
            if (machine == null)
                return Result<Machine>.Failure("Máquina não encontrada", ResultErrorType.NotFound);
            return Result<Machine>.Success(machine);
        }

        public async Task<Result<IEnumerable<Machine>>> GetAllMachinesAsync()
        {
            var machines = await _machineRepository.GetAllAsync();
            return Result<IEnumerable<Machine>>.Success(machines);
        }

        public async Task<Result<IEnumerable<Machine>>> GetActiveMachinesAsync()
        {
            var machines = await _machineRepository.GetActiveAsync();
            return Result<IEnumerable<Machine>>.Success(machines);
        }

        public async Task<Result<IEnumerable<Machine>>> GetMachinesByLocationAsync(string location)
        {
            if (string.IsNullOrWhiteSpace(location))
                return Result<IEnumerable<Machine>>.Failure("Localização não pode ser vazia", ResultErrorType.Invalid);

            var machines = await _machineRepository.GetByLocationAsync(location);
            return Result<IEnumerable<Machine>>.Success(machines);
        }

        public async Task<Result<IEnumerable<Machine>>> GetMachinesRequiringMaintenanceAsync()
        {
            var machines = await _machineRepository.GetRequiringMaintenanceAsync();
            return Result<IEnumerable<Machine>>.Success(machines);
        }

        public async Task<Result<Machine>> CreateMachineAsync(Machine machine)
        {
            if (await _machineRepository.SerialNumberExistsAsync(machine.SerialNumber.Value))
                return Result<Machine>.Failure("Já existe uma máquina com este número de série", ResultErrorType.Conflict);

            var createdMachine = await _machineRepository.AddAsync(machine);
            return Result<Machine>.Success(createdMachine);
        }

        public async Task<Result<Machine>> UpdateMachineAsync(Machine machine)
        {
            var existingMachine = await _machineRepository.GetByIdAsync(machine.Id);
            if (existingMachine == null)
                return Result<Machine>.Failure("Máquina não encontrada", ResultErrorType.NotFound);

            var machineWithSerial = await _machineRepository.GetBySerialNumberAsync(machine.SerialNumber.Value);
            if (machineWithSerial != null && machineWithSerial.Id != machine.Id)
                return Result<Machine>.Failure("Já existe uma máquina com este número de série", ResultErrorType.Conflict);

            var updatedMachine = await _machineRepository.UpdateAsync(machine);
            return Result<Machine>.Success(updatedMachine);
        }

        public async Task<Result<bool>> DeleteMachineAsync(Guid id)
        {
            if (!await _machineRepository.ExistsAsync(id))
                return Result<bool>.Failure("Máquina não encontrada", ResultErrorType.NotFound);

            await _machineRepository.DeleteAsync(id);
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> ScheduleMaintenanceAsync(Guid machineId, DateTime maintenanceDate)
        {
            var machine = await _machineRepository.GetByIdAsync(machineId);
            if (machine == null)
                return Result<bool>.Failure("Máquina não encontrada", ResultErrorType.NotFound);

            machine.ScheduleNextMaintenance(maintenanceDate);
            await _machineRepository.UpdateAsync(machine);
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> UpdateMaintenanceAsync(Guid machineId, DateTime maintenanceDate)
        {
            var machine = await _machineRepository.GetByIdAsync(machineId);
            if (machine == null)
                return Result<bool>.Failure("Máquina não encontrada", ResultErrorType.NotFound);

            machine.UpdateMaintenanceDate(maintenanceDate);
            await _machineRepository.UpdateAsync(machine);
            return Result<bool>.Success(true);
        }
    }
}
