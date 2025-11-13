using IETT_APP.Domain.Common;

namespace IETT_APP.Domain.Entities
{
    public class TripTask : BaseTask<Guid>
    {
        public int? PassengerCount { get; set; } = 0;
        public int? DelayInMinutes { get; set; }
        public int? DelayOutMinutes { get; set; }

        // === Zaman Bilgileri ===
        public DateTime? ScheduledDeparture { get; set; }
        public DateTime? ScheduledArrival { get; set; }
        public DateTime? AdjustedDeparture { get; set; }
        public DateTime? AdjustedArrival { get; set; }
        public DateTime? ActualDeparture { get; set; }
        public DateTime? ActualArrival { get; set; }

        // === Foreign Key'ler ===  
        public Guid? VehicleId { get; set; }
        public Guid? OperatorId { get; set; }
        public Guid? LineId { get; set; }
        public Guid? RouteId { get; set; }
        public Guid? GarageId { get; set; } // İsteğe bağlı olabilir

        // === Navigation Property'ler ===
        public Vehicle<Guid>? Vehicle { get; set; } = null!;
        public Operator? Operator { get; set; } = null!;
        public Line<Guid>? Line { get; set; } = null!;
        public Route<Guid>? Route { get; set; } = null!;
        public Garage<Guid>? Garage { get; set; } // opsiyonel olabilir

        // Eğer ileride koleksiyon eklenirse (ör: görev geçmişi, notlar) buraya eklenebilir
        public ICollection<TripTaskHistory> TripTaskHistories { get; set; } = new List<TripTaskHistory>();
    }
}