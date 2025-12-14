using IETT_APP.Application.Dtos.TripTask;
using IETT_APP.Application.Wrappers;
using IETT_APP.Domain.Enums;

namespace IETT_APP.WebMVC.Services.Interfaces
{
    public interface ITripTaskApiService
    {
        // --- TEMEL CRUD ---
        Task<IEnumerable<TripTaskDto>> GetAllAsync();
        Task<TripTaskDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<TripTaskDto>> SearchAsync(string query);
        Task<ServiceResult<TripTaskDto>> CreateAsync(TripTaskCreateDto dto);
        Task<ServiceResult<TripTaskDto>> UpdateAsync(Guid id, TripTaskUpdateDto dto);
        Task<ServiceResult> DeleteAsync(Guid id);

        // --- YARDIMCI ---
        List<TaskState> GetAllowedStatesForRole(string role);

        // --- SÜRÜCÜ OPERASYONLARI (EKLENDİ) ---
        Task<IEnumerable<TripTaskDto>> GetMyTasksAsync();

        Task<ServiceResult> AcceptTripAsync(Guid id);
        Task<ServiceResult> RejectTripAsync(Guid id, RejectTripRequestDto dto);
        Task<ServiceResult> StartTripAsync(Guid id);
        Task<ServiceResult> CompleteTripAsync(Guid id, CompleteTripRequestDto dto);
        Task<ServiceResult> FailTripAsync(Guid id, FailTripRequestDto dto);

    }
}