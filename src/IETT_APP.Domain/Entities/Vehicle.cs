using IETT_APP.Domain.Common;
using IETT_APP.Domain.Enums;

namespace IETT_APP.Domain.Entities
{
    public class Vehicle<T> : BaseEntity<T>
    {
        public string DoorNumber { get; set; } = string.Empty;
        public string PlateNumber { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public T GarageId { get; set; } = default!;
        public Garage<T> Garage { get; set; } = null!;
        public ServiceStatus ServiceStatus { get; set; } = ServiceStatus.InService;
        public VehicleOperator Operator { get; set; }
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
    }
}
