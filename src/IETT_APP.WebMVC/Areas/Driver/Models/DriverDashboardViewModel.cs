using IETT_APP.Application.Dtos.Driver;

namespace IETT_APP.WebMVC.Areas.Driver.Models
{
    public class DriverDashboardViewModel
    {
        public bool HasProfile { get; set; }
        public DriverDto? Profile { get; set; }

        // İleride eklenecek özellikler:
        // public int CompletedTrips { get; set; }
        // public TripTaskDto? NextTrip { get; set; }
    }
}