using IETT_APP.Application.Dtos.TripTask;
using IETT_APP.Domain.Enums;

namespace IETT_APP.Application.Interfaces
{
    public interface ITripTaskService
    {
        // --- OKUMA ---
        Task<TripTaskDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<TripTaskDto>> GetAllAsync();
        Task<List<TripTaskDto>> SearchAsync(string query);
        Task<IEnumerable<TripTaskDto>> GetByDriverIdAsync(Guid driverId);

        // --- YÖNETİM (AMİR) ---
        Task<Guid> AddAsync(TripTaskCreateDto dto);
        Task UpdateAsync(TripTaskUpdateDto dto);
        Task DeleteAsync(Guid id, string? reason = null);
        List<TaskState> GetAllowedStatesForRole(string role);

        // --- OPERASYON (SÜRÜCÜ) ---
        Task AcceptTripAsync(Guid taskId);
        Task RejectTripAsync(Guid taskId, RejectTripRequestDto dto);
        Task StartTripAsync(Guid taskId);
        Task CompleteTripAsync(Guid taskId, CompleteTripRequestDto dto);
        Task FailTripAsync(Guid taskId, FailTripRequestDto dto);
    }
}