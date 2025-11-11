using AutoMapper;
using IETT_APP.Application.Dtos.Vehicle;
using IETT_APP.Application.Interfaces;
using IETT_APP.Domain.Entities;

namespace IETT_APP.Infrastructure.Services
{
    public class VehicleService<T> : IVehicleService<T>
    {
        private readonly IVehicleRepository<T> _repository;
        private readonly IMapper _mapper;

        public VehicleService(IVehicleRepository<T> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<VehicleDto<T>>> GetAllAsync()
        {
            var vehicles = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<VehicleDto<T>>>(vehicles);
        }

        public async Task<VehicleDto<T>?> GetByIdAsync(T id)
        {
            var vehicle = await _repository.GetByIdAsync(id);
            return vehicle == null ? null : _mapper.Map<VehicleDto<T>>(vehicle);
        }

        public async Task<T> AddAsync(VehicleCreateDto<T> dto)
        {
            var entity = _mapper.Map<Vehicle<T>>(dto);
            entity.Id = (T)Convert.ChangeType(Guid.NewGuid(), typeof(T)); // T Guid ise sorun olmaz, Guid.NewGuid() cast edilir
            await _repository.AddAsync(entity);
            return entity.Id;
        }

        public async Task UpdateAsync(VehicleUpdateDto<T> dto)
        {
            var entity = _mapper.Map<Vehicle<T>>(dto);
            await _repository.UpdateAsync(entity);
        }

        // DELETE
        public async Task<bool> DeleteAsync(T id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return false;

            await _repository.DeleteAsync(id);
            return true;
        }

        public async Task<List<VehicleDto<T>>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<VehicleDto<T>>();

            var term = query.Trim().ToLowerInvariant();
            var all = await _repository.GetAllAsync();

            var filtered = all
                .Where(l => !l.IsDeleted &&
                           ((l.DoorNumber?.ToLowerInvariant().Contains(term) ?? false) ||
                            (l.PlateNumber?.ToLowerInvariant().Contains(term) ?? false)))
                .ToList();

            return _mapper.Map<List<VehicleDto<T>>>(filtered);
        }

        public async Task UnassignFromLineAsync(T vehicleId)
        {
            var vehicle = await _repository.GetByIdAsync(vehicleId);
            if (vehicle == null) throw new Exception("Araç bulunamadı.");

            vehicle.IsAssigned = false;

            await _repository.UpdateAsync(vehicle);
        }

        public async Task<List<VehicleDto<T>>> GetUnassignedVehiclesAsync()
        {
            var allVehicles = await _repository.GetAllAsync();
            var unassigned = allVehicles.Where(v => !v.IsAssigned && !v.IsDeleted).ToList();
            return _mapper.Map<List<VehicleDto<T>>>(unassigned);
        }
    }

}
