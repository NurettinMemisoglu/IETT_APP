using IETT_APP.Domain.Enums;
using IETT_APP.Domain.Interfaces;

namespace IETT_APP.Domain.Services
{
    public class TripTaskDomainService
    {
        // Gerekli bağımlılıklar
        private readonly IRouteRepository<Guid> _routeRepository;
        private readonly IVehicleRepository<Guid> _vehicleRepository;

        // EKSİK OLAN REPOSITORY EKLENDİ (TripTask işlemleri için)
        private readonly ITripTaskRepository _tripTaskRepository;

        public TripTaskDomainService(
            IRouteRepository<Guid> routeRepository,
            IVehicleRepository<Guid> vehicleRepository,
            ITripTaskRepository tripTaskRepository) // Constructor'a ekle
        {
            _routeRepository = routeRepository;
            _vehicleRepository = vehicleRepository;
            _tripTaskRepository = tripTaskRepository; // Atama yapıldı
        }

        // ============================================================
        // 1. Route-Line Kontrolü
        // ============================================================
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


        // ============================================================
        // 2. Vehicle-Garage Kontrolü
        // ============================================================
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

        // ============================================================
        // 3. Concurrency (Çakışma) Kontrolü
        // ============================================================
        public async Task ValidateAssignmentConflictAsync(
            Guid? driverId, Guid? vehicleId, DateTime departure, DateTime arrival, Guid? currentTaskId = null)
        {
            if (!driverId.HasValue && !vehicleId.HasValue) return;

            // Concurrency kontrolü yap
            var conflicts = await _tripTaskRepository.GetConflictingTasksAsync(
                driverId,
                vehicleId,
                departure,
                arrival,
                currentTaskId);

            if (conflicts.Any())
            {
                var conflictType = driverId.HasValue ? "Sürücü" : "Araç";
                throw new Exception($"{conflictType} aynı anda başka bir göreve atanmıştır. Çakışma!");
            }
        }

        // ============================================================
        // 4. Durum Değişikliği Validasyonu (Status Reason Rule)
        // ============================================================
        public void ValidateStatusChange(TaskState status, string? reason)
        {
            // İptal veya Yarım Kalma durumlarında açıklama zorunludur.
            if ((status == TaskState.Cancelled || status == TaskState.Incomplete) &&
                string.IsNullOrWhiteSpace(reason))
            {
                throw new Exception("Görevi iptal ederken veya yarım bırakırken 'Durum Açıklaması' (Reason) girilmesi zorunludur.");
            }
        }

        // ============================================================
        // 5. Durum Geçiş Kontrolü (State Transition Rules)
        // ============================================================
        public void ValidateStatusTransition(TaskState oldStatus, TaskState newStatus, bool isDriver)
        {
            // Eğer durum değişmiyorsa kontrole gerek yok
            if (oldStatus == newStatus) return;

            // --- A) ŞOFÖR KURALLARI ---
            if (isDriver)
            {
                // Şoför Görevi İptal Edemez (Genelde Amir yapar) veya Beklemeye alamaz.
                // Şoför akışı: Pending -> Accepted -> InProgress -> Completed (veya Incomplete)
                if (newStatus == TaskState.Pending || newStatus == TaskState.Cancelled)
                {
                    throw new Exception("Şoförler görevi 'Beklemede' veya 'İptal' durumuna getiremez. Lütfen amirinizle görüşün.");
                }

                // Mantıksız atlamalar engellenebilir (Örn: Pending'den direkt Completed'a geçiş)
                if (oldStatus == TaskState.Pending && newStatus == TaskState.Completed)
                {
                    throw new Exception("Henüz başlamamış bir görevi tamamlayamazsınız.");
                }
            }
            // --- B) AMİR (CHIEF/ADMIN) KURALLARI ---
            else
            {
                // AMİR SADECE: Oluşturabilir (Pending) veya İptal Edebilir (Cancelled).
                // Amir "Kabul Edildi", "Devam Ediyor" veya "Tamamlandı" yapamaz. Çünkü sahada değil.

                if (newStatus == TaskState.Accepted ||
                    newStatus == TaskState.InProgress ||
                    newStatus == TaskState.Completed ||
                    newStatus == TaskState.Incomplete)
                {
                    throw new Exception($"Amir olarak görevi '{newStatus}' durumuna getiremezsiniz. Bu durumlar saha operasyonu (şoför) tarafından tetiklenir.");
                }

                // Amir sadece şu geçişleri yapabilir:
                // Herhangi bir durum -> Cancelled (İptal)
                // Herhangi bir durum -> Pending (Sıfırlama/Geri Çekme)
            }
        }

        // ============================================================
        // 6. Araç Uygunluk Kontrolü (YENİ)
        // ============================================================
        public async Task ValidateVehicleAvailabilityAsync(Guid vehicleId)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);

            if (vehicle == null)
                throw new Exception("Seçilen araç sistemde bulunamadı.");

            // Sadece "InService" (Servise Hazır) olan araçlara görev verilebilir.
            if (vehicle.ServiceStatus != ServiceStatus.InService)
            {
                // Enum'un DisplayName özelliğini kullanmak daha şık olur ama şimdilik string çevirelim
                string statusTr = vehicle.ServiceStatus switch
                {
                    ServiceStatus.OutOfService => "Servis Dışı",
                    ServiceStatus.UnderMaintenance => "Bakımda",
                    ServiceStatus.Damaged => "Hasarlı",
                    ServiceStatus.OutOfDuty => "Görev Dışı",
                    _ => vehicle.ServiceStatus.ToString()
                };

                throw new Exception($"Araç ({vehicle.DoorNumber} - {vehicle.PlateNumber}) şu an '{statusTr}' durumunda olduğu için sefere atanamaz.");
            }
        }
    }
}