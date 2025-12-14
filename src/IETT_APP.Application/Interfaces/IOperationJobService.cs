namespace IETT_APP.Application.Interfaces
{
    public interface IOperationJobService
    {
        Task CheckDelayedTripsAsync(); // Gecikme Kontrolü
        Task AutoCloseShiftAsync();    // Vardiya Kapanışı
    }
}
