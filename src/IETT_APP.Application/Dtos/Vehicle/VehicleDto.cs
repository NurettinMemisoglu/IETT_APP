using IETT_APP.Domain.Enums;

namespace IETT_APP.Application.Dtos.Vehicle
{
    public class VehicleDto<T>
    {
        public T Id { get; set; } = default!;

        public string DoorNumber { get; set; } = string.Empty;
        public string PlateNumber { get; set; } = string.Empty;
        public int Capacity { get; set; }

        public T GarageId { get; set; } = default!;


        public ServiceStatus ServiceStatus { get; set; }
        public VehicleType Driver { get; set; }
        public VehicleModel Model { get; set; }

        public int Year { get; set; }
        public int TotalKm { get; set; }

        public bool HasDisabilityAccess { get; set; } = true;
        public bool HasWiFi { get; set; } = true;
        public bool HasBikeRack { get; set; } = false;
        public bool HasMobileCharging { get; set; } = false;
        public bool HasPassengerInfoSystem { get; set; } = false;
        public bool HasCCTV { get; set; } = true;
        public bool IsActive { get; set; } = true;

        public bool IsAssigned { get; set; } = false;

        // Opsiyonel: Read-only alanlar (UI veya log amaçlı)
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
