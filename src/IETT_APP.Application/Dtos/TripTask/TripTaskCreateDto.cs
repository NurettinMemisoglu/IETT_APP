using System.ComponentModel.DataAnnotations;

namespace IETT_APP.Application.Dtos.TripTask
{
    public class TripTaskCreateDto
    {
        // Title ve Description BaseTask'ten gelmiyor (DTO olduğu için elle yazmalısın)
        [Required(ErrorMessage = "Görev başlığı zorunludur.")]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        // Zamanlama
        [Required(ErrorMessage = "Planlanan kalkış saati zorunludur.")]
        public DateTime? ScheduledDeparture { get; set; }

        [Required(ErrorMessage = "Planlanan varış saati zorunludur.")]
        public DateTime? ScheduledArrival { get; set; }

        // İlişkiler (Line ve Route olmadan sefer olmaz)
        [Required(ErrorMessage = "Hat seçimi zorunludur.")]
        public Guid? LineId { get; set; }

        [Required(ErrorMessage = "Güzergah seçimi zorunludur.")]
        public Guid? RouteId { get; set; }

        // Not: Vehicle, Driver ve Garage opsiyonel olabilir (Taslak görev için).
        // Ama eğer atama anında zorunluysa onlara da [Required] ekle.
        public Guid? VehicleId { get; set; }
        public Guid? DriverId { get; set; }
        public Guid? GarageId { get; set; }
    }
}
