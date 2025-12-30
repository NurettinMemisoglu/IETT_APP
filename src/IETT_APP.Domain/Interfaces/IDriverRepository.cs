using IETT_APP.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage; // Bunu eklemeyi unutma

namespace IETT_APP.Domain.Interfaces
{
    public interface IDriverRepository
    {
        // ... Mevcut metotların ...
        Task<IEnumerable<Driver>> GetAllWithDetailsAsync();
        Task<Driver?> GetByIdWithDetailsAsync(Guid id);
        Task<Driver?> GetByUserIdAsync(string userId);
        Task<IEnumerable<Driver>> GetUnassignedDriversAsync();

        Task AddAsync(Driver entity);
        Task UpdateAsync(Driver entity);
        Task SoftDeleteAsync(Driver entity);

        // ✅ YENİ EKLENEN: Transaction Başlatma Metodu
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}