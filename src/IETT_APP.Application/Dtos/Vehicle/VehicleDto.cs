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
        public VehicleOperator Operator { get; set; }
        public VehicleModel Model { get; set; }

        public int Year { get; set; }
        public int TotalKm { get; set; }

        public bool HasDisabilityAccess { get; set; }
        public bool HasWiFi { get; set; }
        public bool HasBikeRack { get; set; }
        public bool HasMobileCharging { get; set; }
        public bool HasPassengerInfoSystem { get; set; }
        public bool HasCCTV { get; set; }

        public bool IsActive { get; set; } = true;

        // Opsiyonel: Read-only alanlar (UI veya log amaçlı)
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
