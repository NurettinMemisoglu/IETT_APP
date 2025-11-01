using AutoMapper;
using IETT_APP.Application.Dtos.Route;
using IETT_APP.Application.Interfaces;
using IETT_APP.Domain.Entities;
using IETT_APP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IETT_APP.Infrastructure.Services
{
    public class RouteService<T> : IRouteService<T>
    {
        private readonly IRouteRepository<T> _repo;
        private readonly IMapper _mapper;
        private readonly AppDbContext _db;

        public RouteService(IRouteRepository<T> repo, IMapper mapper, AppDbContext db)
        {
            _repo = repo;
            _mapper = mapper;
            _db = db;
        }

        // GET ALL
        public async Task<List<RouteDto<T>>> GetAllAsync()
        {
            var entities = await _repo.GetAllAsync();
            return entities.Select(MapToDto).ToList();
        }

        // GET BY ID
        public async Task<RouteDto<T>?> GetByIdAsync(T id)
        {
            var entity = await _repo.GetByIdAsync(id);
            return entity == null ? null : MapToDto(entity);
        }

        // CREATE
        public async Task<RouteDto<T>> CreateAsync(RouteCreateUpdateDto<T> dto)
        {
            if (dto.StopIds?.Any() == true)
            {
                var requested = dto.StopIds.Cast<Guid>().ToList();
                var existing = await _db.Stops
                    .Where(s => requested.Contains(s.Id))
                    .Select(s => s.Id)
                    .ToListAsync();

                var missing = requested.Except(existing).ToList();
                if (missing.Any())
                    throw new ArgumentException($"Some stops do not exist: {string.Join(", ", missing)}");
            }

            var entity = _mapper.Map<Route<T>>(dto);

            if (dto.StopIds?.Any() == true)
            {
                entity.RouteStops = dto.StopIds.Select((stopId, idx) => new RouteStop<T>
                {
                    StopId = (Guid)(object)stopId!,
                    Order = idx,
                    Route = entity
                }).ToList();
            }

            await _repo.AddAsync(entity);

            // _db'den değil _repo'dan al
            var createdEntity = await _repo.GetByIdAsync(entity.Id!);

            return MapToDto(createdEntity);
        }

        // UPDATE
        public async Task<bool> UpdateAsync(RouteCreateUpdateDto<T> dto)
        {
            if (dto.Id == null) return false;

            var entity = await _repo.GetByIdAsync(dto.Id);
            if (entity == null) return false;

            _mapper.Map(dto, entity);

            if (dto.StopIds?.Any() == true)
            {
                entity.RouteStops = dto.StopIds.Select((stopId, idx) => new RouteStop<T>
                {
                    StopId = (Guid)(object)stopId!,
                    Order = idx,
                    RouteId = entity.Id,
                    Route = entity
                }).ToList();
            }

            await _repo.UpdateAsync(entity);
            return true;
        }

        // DELETE
        public async Task<bool> DeleteAsync(T id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return false;

            await _repo.DeleteAsync(id);
            return true;
        }

        // SEARCH
        public async Task<List<RouteDto<T>>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<RouteDto<T>>();

            var term = query.Trim().ToLowerInvariant();
            var all = await _repo.GetAllAsync();

            var filtered = all
                .Where(l => !l.IsDeleted &&
                           ((l.Name?.ToLowerInvariant().Contains(term) ?? false) ||
                            (l.Code?.ToLowerInvariant().Contains(term) ?? false)))
                .ToList();

            return filtered.Select(MapToDto).ToList();
        }

        // SET ACTIVE
        public async Task<bool> SetActiveAsync(T id, bool isActive)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return false;

            entity.IsActive = isActive;
            await _repo.UpdateAsync(entity);
            return true;
        }

        // 🔹 Private helper to map entity -> DTO with StopInfo
        private RouteDto<T> MapToDto(Route<T> entity)
        {
            var dto = _mapper.Map<RouteDto<T>>(entity);
            dto.StopIds = entity.RouteStops?.Select(rs => rs.StopId).ToList() ?? new List<Guid>();
            dto.StopNames = entity.RouteStops?.Select(rs => rs.Stop?.Name ?? string.Empty).ToList() ?? new List<string>();
            dto.Stops = entity.RouteStops?.Select(rs => new StopInfoDto
            {
                Id = rs.StopId,
                Name = rs.Stop?.Name ?? "",
                Latitude = rs.Stop?.Location?.Latitude ?? 0,
                Longitude = rs.Stop?.Location?.Longitude ?? 0
            }).ToList() ?? new List<StopInfoDto>();

            return dto;
        }
    }
}
