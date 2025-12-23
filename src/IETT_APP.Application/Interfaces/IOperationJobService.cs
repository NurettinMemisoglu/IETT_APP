namespace IETT_APP.Application.Interfaces
{
    public interface IOperationJobService
    {
        Task CheckDelayedTripsAsync(); // Mevcut
        Task AutoCloseShiftAsync();    // Mevcut

        // --- YENİ EKLENENLER ---
        Task CheckExpirationsAsync();  // Araç/Ehliyet Kontrolü
        Task SendWeeklyReportAsync();  // Haftalık Rapor
    }
}