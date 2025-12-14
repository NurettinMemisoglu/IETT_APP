using IETT_APP.Domain.Common;
using IETT_APP.Domain.Enums;

namespace IETT_APP.Domain.Entities
{
    public class FileRecorder : BaseEntity<Guid>
    {
        // --- Dosya Kimlik Bilgileri ---

        // Sunucuda saklanan benzersiz isim (Örn: 550e84...jpg)
        public string FileName { get; set; } = null!;

        // Kullanıcının yüklediği orijinal isim (Örn: vesikalik_foto.jpg)
        public string OriginalFileName { get; set; } = null!;

        // Dosya uzantısı (Örn: .jpg, .png) - Filtreleme için çok yararlı
        public string FileExtension { get; set; } = null!;

        // Tarayıcının dosyayı tanıması için (Örn: image/jpeg, application/pdf)
        public string ContentType { get; set; } = null!;


        // --- Fiziksel Bilgiler ---

        // Dosyanın diskteki tam yolu veya klasör yolu (Örn: /uploads/profiles/)
        public string FilePath { get; set; } = null!;

        // Dosya boyutu (Byte cinsinden) - Kota kontrolleri için
        public long FileSize { get; set; }


        // --- Kategorizasyon ---

        // Bu dosya ne dosyası? (Profil, Belge, Rapor vs.)
        public FileCategory FileCategory { get; set; }


        // --- Opsiyonel ---
        public string? Description { get; set; } // Kullanıcıdan alınan ek açıklama

        // NOT: UploadedAt, UploadedBy, IsDeleted alanları BaseEntity'den geliyor.
        // Tekrar yazmana gerek yok.
    }
}