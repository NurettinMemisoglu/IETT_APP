using IETT_APP.Domain.Entities;

namespace IETT_APP.Domain.Interfaces
{
    public interface IDriverRepository
    {
        // Temel Okuma İşlemleri
        Task<IEnumerable<Driver>> GetAllWithDetailsAsync();
        Task<Driver?> GetByIdWithDetailsAsync(Guid id);

        // Özel Sorgular
        Task<Driver?> GetByUserIdAsync(string userId);
        Task<IEnumerable<Driver>> GetUnassignedDriversAsync(); // Garajı olmayanlar

        // Yazma İşlemleri
        Task AddAsync(Driver entity);
        Task UpdateAsync(Driver entity);
        Task SoftDeleteAsync(Driver entity);


    }
}