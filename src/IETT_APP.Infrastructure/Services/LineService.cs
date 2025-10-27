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
            return _mapper.Map<List<LineDto<T>>>(entities);
        }

        public async Task<LineDto<T>?> GetByIdAsync(T id)
        {
            var entity = await _repo.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<LineDto<T>>(entity);
        }

        public async Task<LineDto<T>> CreateAsync(LineCreateUpdateDto<T> dto)
        {
            var entity = _mapper.Map<Line<T>>(dto);

            // created flags handled by repository
            await _repo.AddAsync(entity);

            return _mapper.Map<LineDto<T>>(entity);
        }

        public async Task<bool> UpdateAsync(LineCreateUpdateDto<T> dto)
        {
            if (dto.Id == null) return false;

            var entity = await _repo.GetByIdAsync(dto.Id);
            if (entity == null) return false;

            _mapper.Map(dto, entity);
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

        public async Task<List<LineDto<T>>> SearchAsync(string query)
        {
            var all = await _repo.GetAllAsync();
            var filtered = all.Where(l =>
                    (!string.IsNullOrEmpty(l.Name) && l.Name.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(l.Code) && l.Code.Contains(query, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            return _mapper.Map<List<LineDto<T>>>(filtered);
        }

        // NEW: toggle active flag on a Line entity
        public async Task<bool> SetActiveAsync(T id, bool isActive)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return false;

            entity.IsActive = isActive;
            await _repo.UpdateAsync(entity);
            return true;
        }
    }
}
