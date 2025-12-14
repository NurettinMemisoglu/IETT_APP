using IETT_APP.Domain.Entities;
using IETT_APP.Domain.Enums;
using IETT_APP.Domain.Interfaces;

namespace IETT_APP.Domain.Services
{
    // Bu Domain Service, TripTaskService'in görev atamadan önce çağırdığı merkezi validasyon noktasıdır.
    public class DriverDomainService
    {
        private readonly IDriverRepository _driverRepository;

        public DriverDomainService(IDriverRepository driverRepository)
        {
            _driverRepository = driverRepository;
        }

        /// <summary>
        /// Bir operatörün (şoförün) yasal ve operasyonel olarak göreve çıkmaya uygun olup olmadığını denetler.
        /// </summary>
        /// <param name="driverId">Kontrol edilecek sürücü ID'si</param>
        /// <param name="taskDate">Görevin yapılacağı tarih</param>
        public async Task ValidateOperatorEligibilityAsync(Guid driverId, DateTime taskDate)
        {
            // 1. Sürücüyü detayları ile çek (User, Belgeler, Durum)
            // GetByIdWithDetailsAsync metodu User tablosunu da joinlediği için driver.User'a erişebiliriz.
            var driver = await _driverRepository.GetByIdWithDetailsAsync(driverId);

            if (driver == null)
                throw new Exception("Belirtilen sürücü sistemde bulunamadı.");

            // 2. Hesap Aktif mi? (Soft Delete kontrolü)
            if (!driver.IsActive || driver.IsDeleted) // IsDeleted kontrolü de eklendi (Gerekli olmasa da güvenli)
                throw new Exception($"'{driver.User?.Name} {driver.User?.Surname}' isimli personel pasif durumda/silinmiş olduğu için görevlendirilemez.");

            // 3. Çalışma Durumu (WorkStatus) Uygun mu? (İzin, Rapor vb.)
            ValidateWorkStatus(driver);

            // 4. Yasal Belgeler (Ehliyet & Psikoteknik) Geçerli mi?
            ValidateDocuments(driver, taskDate);

            // 5. Dinlenme Süresi Kontrolü (Opsiyonel: Eğer bu kontrol gerekiyorsa buraya eklenmeli)
            // Şu an sadece tek göreve atama validasyonu yapılıyor.
        }

        // Yardımcı Metot: Durum Kontrolü
        private void ValidateWorkStatus(Driver driver)
        {
            // KURAL: Sadece "Available" (Müsait) olan personele görev atanabilir.
            if (driver.WorkStatus != WorkStatus.Available)
            {
                // Hatanın sebebini kullanıcıya net açıklayalım
                string reason = driver.WorkStatus switch
                {
                    WorkStatus.OffDuty => "Görev Dışı (Henüz garaj atanmamış veya mesai dışı)",
                    WorkStatus.Suspended => "Açığa Alınmış / Cezalı",
                    WorkStatus.MedicalLeave => "Raporlu / Hasta",
                    WorkStatus.OnVacation => "Yıllık İzinde",
                    WorkStatus.AdministrativeLeave => "İdari İzinli",
                    WorkStatus.Working => "Şu an Aktif Görevde (Working)",
                    WorkStatus.Resting => "İstirahatte (Mola)",
                    _ => "Uygun Değil"
                };

                throw new Exception($"Personel ({driver.EmployeeNumber}) şu an '{reason}' durumunda olduğu için görev atanamaz. Sadece 'Müsait' (Available) durumdaki personele görev verilebilir.");
            }
        }

        // Yardımcı Metot: Belge Kontrolü
        private void ValidateDocuments(Driver driver, DateTime taskDate)
        {
            // Ehliyet Süresi
            if (driver.LicenseExpiryDate.HasValue && driver.LicenseExpiryDate.Value.Date < taskDate.Date)
            {
                throw new Exception($"Personelin ehliyet süresi dolmuş! (Son Tarih: {driver.LicenseExpiryDate.Value.ToShortDateString()})");
            }

            // Psikoteknik Belgesi
            if (driver.PsychotechnicExpiryDate.HasValue && driver.PsychotechnicExpiryDate.Value.Date < taskDate.Date)
            {
                throw new Exception($"Personelin psikoteknik belgesinin süresi dolmuş! (Son Tarih: {driver.PsychotechnicExpiryDate.Value.ToShortDateString()})");
            }

            // Ehliyet Sınıfı Kontrolü (Opsiyonel)
            // Bu kısım şu an commentlenmiş vaziyette, eğer aktif edilirse validasyon çalışır.
        }
    }
}