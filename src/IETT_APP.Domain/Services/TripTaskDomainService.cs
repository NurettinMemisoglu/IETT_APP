using IETT_APP.Domain.Interfaces;

namespace IETT_APP.Domain.Services
{
    public class TripTaskDomainService
    {
        private readonly IRouteRepository<Guid> _routeRepository;
        private readonly IVehicleRepository<Guid> _vehicleRepository;

        public TripTaskDomainService(
            IRouteRepository<Guid> routeRepository,
            IVehicleRepository<Guid> vehicleRepository)
        {
            _routeRepository = routeRepository;
            _vehicleRepository = vehicleRepository;
        }

        // Route-Line kontrolü
        public async Task ValidateRouteLineMatchAsync(Guid? routeId, Guid? lineId)
        {
            if (routeId == null)
                throw new Exception("RouteId boş olamaz.");

            if (!lineId.HasValue)
                throw new Exception("LineId boş olamaz.");

            var route = await _routeRepository.GetByIdAsync(routeId.Value);
            if (route == null)
                throw new Exception("Route bulunamadı.");

            if (route.LineId != lineId.Value)
                throw new Exception("Seçilen Route, Line ile eşleşmiyor.");
        }


        // Vehicle-Garage kontrolü
        public async Task ValidateVehicleGarageMatchAsync(Guid? vehicleId, Guid? garageId)
        {
            if (!vehicleId.HasValue)
                throw new Exception("VehicleId boş olamaz.");

            if (!garageId.HasValue)
                throw new Exception("GarageId boş olamaz.");

            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId.Value);
            if (vehicle == null)
                throw new Exception("Vehicle bulunamadı.");

            if (vehicle.GarageId != garageId.Value)
                throw new Exception("Seçilen Vehicle, belirtilen Garage ile eşleşmiyor.");
        }
    }
}
