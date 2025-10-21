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

        public async Task<List<RouteDto<T>>> GetAllAsync()
        {
            var entities = await _repo.GetAllAsync();
            var dtos = entities.Select(e =>
            {
                var dto = _mapper.Map<RouteDto<T>>(e);
                dto.StopIds = e.RouteStops?.Select(ls => ls.StopId).ToList() ?? new List<Guid>();
                return dto;
            }).ToList();

            return dtos;
        }

        public async Task<RouteDto<T>?> GetByIdAsync(T id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return null;

            var dto = _mapper.Map<RouteDto<T>>(entity);
            dto.StopIds = entity.RouteStops?.Select(ls => ls.StopId).ToList() ?? new List<Guid>();
            return dto;
        }

        public async Task<RouteDto<T>> CreateAsync(RouteCreateUpdateDto<T> dto)
        {
            // validate stop ids exist
            if (dto.StopIds != null && dto.StopIds.Any())
            {
                var requested = dto.StopIds.Select(s => (Guid)(object)s!).ToList();
                var existing = await _db.Stops
                                        .Where(s => requested.Contains(s.Id))
                                        .Select(s => s.Id)
                                        .ToListAsync();

                var missing = requested.Except(existing).ToList();
                if (missing.Any())
                    throw new ArgumentException($"Some stops do not exist: {string.Join(", ", missing)}");
            }

            var entity = _mapper.Map<Route<T>>(dto);

            if (dto.StopIds != null && dto.StopIds.Any())
            {
                entity.RouteStops = dto.StopIds
                    .Select((stopId, idx) => new RouteStop<T>
                    {
                        StopId = (Guid)(object)stopId!,
                        Order = idx,
                        Route = entity
                    })
                    .ToList();
            }

            await _repo.AddAsync(entity);

            var created = _mapper.Map<RouteDto<T>>(entity);
            created.StopIds = entity.RouteStops?.Select(ls => ls.StopId).ToList() ?? new List<Guid>();
            return created;
        }

        public async Task<bool> UpdateAsync(RouteCreateUpdateDto<T> dto)
        {
            if (dto.Id == null) return false;

            // validate stop ids exist
            if (dto.StopIds != null && dto.StopIds.Any())
            {
                var requested = dto.StopIds.Select(s => (Guid)(object)s!).ToList();
                var existing = await _db.Stops
                                        .Where(s => requested.Contains(s.Id))
                                        .Select(s => s.Id)
                                        .ToListAsync();

                var missing = requested.Except(existing).ToList();
                if (missing.Any())
                    throw new ArgumentException($"Some stops do not exist: {string.Join(", ", missing)}");
            }

            var entity = await _repo.GetByIdAsync(dto.Id);
            if (entity == null) return false;

            _mapper.Map(dto, entity);

            entity.RouteStops = dto.StopIds?
                .Select((stopId, idx) => new RouteStop<T>
                {
                    StopId = (Guid)(object)stopId!,
                    Order = idx,
                    RouteId = entity.Id,
                    Route = entity
                })
                .ToList() ?? new List<RouteStop<T>>();

            await _repo.UpdateAsync(entity);
            return true;
        }

        public async Task<bool> DeleteAsync(T id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return false;

            await _repo.DeleteAsync(id);
            return true;
        }

        public async Task<List<RouteDto<T>>> SearchAsync(string query)
        {
            var all = await _repo.GetAllAsync();
            var filtered = all.Where(l => l.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                                          l.Code.Contains(query, StringComparison.OrdinalIgnoreCase))
                              .ToList();

            var dtos = filtered.Select(e =>
            {
                var dto = _mapper.Map<RouteDto<T>>(e);
                dto.StopIds = e.RouteStops?.Select(ls => ls.StopId).ToList() ?? new List<Guid>();
                return dto;
            }).ToList();

            return dtos;
        }
    }
}
