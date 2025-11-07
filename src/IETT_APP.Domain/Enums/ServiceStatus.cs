namespace IETT_APP.Domain.Enums
{
    public enum ServiceStatus
    {
        InService = 1,          // Servise hazır
        OutOfService = 2,       // Servis dışı
        UnderMaintenance = 3,   // Bakımda
        Damaged = 4,            // Hasarlı
        OutOfDuty = 5           // Görev dışı
    }
}
