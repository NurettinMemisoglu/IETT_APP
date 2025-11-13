using IETT_APP.Domain.Entities;

namespace IETT_APP.Domain.Interfaces
{
    public interface ILineRepository<T>
    {
        Task<List<Line<T>>> GetAllAsync();
        Task<Line<T>?> GetByIdAsync(T id);
        Task AddAsync(Line<T> entity);
        Task UpdateAsync(Line<T> entity);
        Task DeleteAsync(T id); // Soft delete
    }
}
