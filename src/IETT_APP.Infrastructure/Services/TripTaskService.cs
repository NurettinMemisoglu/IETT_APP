using AutoMapper;
using IETT_APP.Application.Dtos.TripTask;
using IETT_APP.Application.Interfaces;
using IETT_APP.Domain.Entities;
using IETT_APP.Domain.Interfaces;
using IETT_APP.Domain.Services;

namespace IETT_APP.Infrastructure.Services
{
    public class TripTaskService : ITripTaskService
    {
        private readonly ITripTaskRepository _repository;
        private readonly IMapper _mapper;
        private readonly TripTaskDomainService _tripTaskDomainService;
        private static readonly TimeZoneInfo TurkeyZone =
            TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");

        public TripTaskService(
            ITripTaskRepository repository,
            IMapper mapper,
            TripTaskDomainService tripTaskDomainService)
        {
            _repository = repository;
            _mapper = mapper;
            _tripTaskDomainService = tripTaskDomainService;
        }

        public async Task<Guid> AddAsync(TripTaskCreateDto dto)
        {
            // Route ve Line eşleşmesini kontrol et
            await _tripTaskDomainService.ValidateRouteLineMatchAsync(dto.RouteId!.Value, dto.LineId);

            // Vehicle ve Garage eşleşmesini kontrol et
            await _tripTaskDomainService.ValidateVehicleGarageMatchAsync(dto.VehicleId!.Value, dto.GarageId);

            var entity = _mapper.Map<TripTask>(dto);
            entity.Id = Guid.NewGuid();

            var nowTr = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TurkeyZone);
            entity.CreatedAt = nowTr;
            entity.UpdatedAt = nowTr;
            entity.CreatedBy = "System"; // ileride context’ten gelen kullanıcı bilgisi

            await _repository.AddAsync(entity);
            return entity.Id;
        }

        public async Task UpdateAsync(TripTaskUpdateDto dto)
        {
            var entity = await _repository.GetByIdAsync(dto.Id);
            if (entity == null)
                throw new Exception("TripTask not found.");

            // Route ve Line eşleşmesini kontrol et
            if (dto.RouteId.HasValue && dto.LineId.HasValue)
            {
                await _tripTaskDomainService.ValidateRouteLineMatchAsync(dto.RouteId.Value, dto.LineId);
            }

            // Vehicle ve Garage eşleşmesini kontrol et
            if (dto.VehicleId.HasValue && dto.GarageId.HasValue)
            {
                await _tripTaskDomainService.ValidateVehicleGarageMatchAsync(dto.VehicleId.Value, dto.GarageId);
            }

            // Mapping: sadece dolu alanları entity'ye kopyala
            _mapper.Map(dto, entity);

            // Güncelleme zamanı ve kullanıcı
            var nowTr = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TurkeyZone);
            entity.UpdatedAt = nowTr;
            entity.UpdatedBy = "System"; // ileride oturum açan kullanıcı

            // Repository update
            await _repository.UpdateAsync(entity);
            // History otomatik olarak AppDbContext.ApplyEntityHistory() ile eklenir
        }

        public async Task DeleteAsync(Guid id, string? reason = null)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                throw new Exception("TripTask not found.");

            await _repository.SoftDeleteAsync(entity, reason);
        }

        public async Task<IEnumerable<TripTaskDto>> GetAllAsync()
        {
            var list = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<TripTaskDto>>(list);
        }

        public async Task<TripTaskDto?> GetByIdAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<TripTaskDto>(entity);
        }

        // SEARCH
        public async Task<List<TripTaskDto>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<TripTaskDto>();

            var term = query.Trim().ToLowerInvariant();
            var all = await _repository.GetAllAsync();

            var filtered = all
                .Where(l => !l.IsDeleted &&
                           ((l.Title?.ToLowerInvariant().Contains(term) ?? false) ||
                            (l.Description?.ToLowerInvariant().Contains(term) ?? false)))
                .ToList();

            return _mapper.Map<List<TripTaskDto>>(filtered);
        }

    }
}
