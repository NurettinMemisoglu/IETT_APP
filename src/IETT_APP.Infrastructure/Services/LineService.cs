using AutoMapper;
using IETT_APP.Application.Dtos.Line;
using IETT_APP.Application.Interfaces;
using IETT_APP.Domain.Entities;

namespace IETT_APP.Infrastructure.Services
{
    public class LineService<T> : ILineService<T>
    {
        private readonly ILineRepository<T> _repo;
        private readonly IMapper _mapper;

        public LineService(ILineRepository<T> repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<List<LineDto<T>>> GetAllAsync()
        {
            var entities = await _repo.GetAllAsync();
            // Soft delete filtre
            var activeEntities = entities.Where(x => !x.IsDeleted).ToList();
            return _mapper.Map<List<LineDto<T>>>(activeEntities);
        }

        public async Task<LineDto<T>?> GetByIdAsync(T id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted) return null;
            return _mapper.Map<LineDto<T>>(entity);
        }

        public async Task<LineDto<T>> CreateAsync(LineCreateUpdateDto<T> dto)
        {
            var entity = _mapper.Map<Line<T>>(dto);
            entity.IsDeleted = false;
            await _repo.AddAsync(entity);
            return _mapper.Map<LineDto<T>>(entity);
        }

        public async Task<bool> UpdateAsync(LineCreateUpdateDto<T> dto)
        {
            if (dto.Id == null) return false;

            var entity = await _repo.GetByIdAsync(dto.Id);
            if (entity == null || entity.IsDeleted) return false;

            _mapper.Map(dto, entity);
            await _repo.UpdateAsync(entity);
            return true;
        }

        public async Task<LineDto<T>> CreateOrUpdateAsync(LineCreateUpdateDto<T> dto)
        {
            if (dto.Id == null || dto.Id.Equals(default(T)) || dto.Id.Equals(Guid.Empty))
                return await CreateAsync(dto);

            var updated = await UpdateAsync(dto);
            if (!updated) throw new ArgumentException("Güncellenecek hat bulunamadı.");

            var entity = await GetByIdAsync(dto.Id);
            return entity!;
        }

        public async Task<bool> SoftDeleteAsync(T id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted) return false;

            entity.IsDeleted = true;
            await _repo.UpdateAsync(entity);
            return true;
        }

        public async Task<bool> DeleteAsync(T id)
        {
            // Direkt delete isteğe bağlı, soft delete yerine SoftDeleteAsync kullan
            return await SoftDeleteAsync(id);
        }

        public async Task<List<LineDto<T>>> SearchAsync(string query)
        {
            var all = await _repo.GetAllAsync();
            var filtered = all
                .Where(l => !l.IsDeleted &&
                       (!string.IsNullOrEmpty(l.Name) && l.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        !string.IsNullOrEmpty(l.Code) && l.Code.Contains(query, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            return _mapper.Map<List<LineDto<T>>>(filtered);
        }

        public async Task<bool> SetActiveAsync(T id, bool isActive)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted) return false;

            entity.IsActive = isActive;
            await _repo.UpdateAsync(entity);
            return true;
        }
    }
}
