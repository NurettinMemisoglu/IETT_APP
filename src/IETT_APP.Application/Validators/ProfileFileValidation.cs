using Microsoft.AspNetCore.Http;

namespace IETT_APP.Application.Validators
{
    public static class ProfileFileValidation
    {
        private static readonly Dictionary<string, List<byte[]>> _signatures = new()
        {
            // --- RESİM FORMATLARI ---
            { ".jpg", new List<byte[]> { new byte[]{0xFF,0xD8,0xFF,0xE0}, new byte[]{0xFF,0xD8,0xFF,0xE1}, new byte[]{0xFF,0xD8,0xFF,0xE8} } },
            { ".jpeg", new List<byte[]> { new byte[]{0xFF,0xD8,0xFF,0xE0}, new byte[]{0xFF,0xD8,0xFF,0xE2}, new byte[]{0xFF,0xD8,0xFF,0xE3} } },
            { ".png", new List<byte[]> { new byte[]{0x89,0x50,0x4E,0x47,0x0D,0x0A,0x1A,0x0A} } },
            
            // --- BELGE FORMATLARI (YENİ EKLENENLER) ---
            
            // PDF: %PDF-
            { ".pdf", new List<byte[]> { new byte[]{0x25, 0x50, 0x44, 0x46} } },

            // DOC (Eski Office): D0 CF 11 E0
            { ".doc", new List<byte[]> { new byte[]{0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1} } },

            // DOCX (Yeni Office - XML tabanlı): PK..
            // Not: .docx, .xlsx, .pptx hepsi aynı imzayı (PK..) kullanır (Zip formatı olduğu için).
            // Ayırt etmek zordur ama güvenlik için en azından zip header'ı kontrol edilebilir.
            { ".docx", new List<byte[]> { new byte[]{0x50, 0x4B, 0x03, 0x04} } }
        };

        // Genel Dosya Kontrolü (Her şeyi kabul eder: Resim + Belge)
        public static bool IsValidFile(this IFormFile file)
        {
            return ValidateSignature(file, _signatures.Keys.ToArray());
        }

        // Sadece Resim Kontrolü
        public static bool IsValidImage(this IFormFile file)
        {
            return ValidateSignature(file, new[] { ".jpg", ".jpeg", ".png" });
        }

        // Sadece Belge Kontrolü (Word/PDF)
        public static bool IsValidDocument(this IFormFile file)
        {
            return ValidateSignature(file, new[] { ".pdf", ".doc", ".docx" });
        }

        // --- Ortak Doğrulama Motoru ---
        private static bool ValidateSignature(IFormFile file, string[] allowedExtensions)
        {
            if (file == null || file.Length == 0) return true; // Boş dosya validasyon dışı (Required attribute bakar)

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            // 1. Uzantı listemizde var mı?
            if (!allowedExtensions.Contains(ext)) return false;

            // 2. İmza listemizde bu uzantının tanımı var mı?
            if (!_signatures.ContainsKey(ext)) return false;

            // 3. Bayt okuma ve kontrol
            using var reader = new BinaryReader(file.OpenReadStream());
            var signatures = _signatures[ext];
            var header = reader.ReadBytes(signatures.Max(s => s.Length));

            // Dosya imzasını kontrol et
            bool isValid = signatures.Any(sig => header.Take(sig.Length).SequenceEqual(sig));

            // Stream pozisyonunu başa al (Çok önemli! Yoksa dosya boş kaydedilir)
            file.OpenReadStream().Position = 0;

            return isValid;
        }
    }
}