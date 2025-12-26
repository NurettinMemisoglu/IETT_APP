using IETT_APP.Domain.Entities;

namespace IETT_APP.Domain.Interfaces
{
    public interface ITripTaskRepository
    {
        Task<TripTask?> GetByIdAsync(Guid id);
        Task<IEnumerable<TripTask>> GetAllAsync(string? creatorName = null); Task AddAsync(TripTask tripTask);
        Task UpdateAsync(TripTask tripTask);
        Task SoftDeleteAsync(TripTask tripTask, string? reason = null);
        Task<bool> ExistsAsync(Guid id);
        Task<IEnumerable<TripTask>> SearchByTermAsync(string term);

        //Çakışan görevleri bul
        /// <summary>
        /// Belirtilen zaman dilimi, sürücü veya araç için çakışan görevleri çeker.
        /// </summary>
        Task<IEnumerable<TripTask>> GetConflictingTasksAsync(
            Guid? driverId,
            Guid? vehicleId,
            DateTime startTime,
            DateTime endTime,
            Guid? currentTaskId = null); // currentTaskId: Güncelleme yapılıyorsa mevcut görevi hariç tutmak için kullanılır.

        Task<IEnumerable<TripTask>> GetByDriverIdAsync(Guid driverId);
    }
}
