using IETT_APP.Application.Interfaces;
using IETT_APP.Application.Validators;
using IETT_APP.Domain.Entities;
using IETT_APP.Domain.Enums;
using IETT_APP.Domain.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace IETT_APP.Infrastructure.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IFileRepository _fileRepository;

        public FileService(IWebHostEnvironment env, IFileRepository fileRepository)
        {
            _env = env;
            _fileRepository = fileRepository;
        }

        public async Task<FileRecorder> SaveFileAsync(IFormFile file, FileCategory category, string createdBy)
        {
            // 1. VALIDASYON (Türe göre kontrol)
            bool isValid = false;

            switch (category)
            {
                case FileCategory.ProfileImage:
                    isValid = file.IsValidImage(); // Resim mi? (JPG, PNG)
                    break;

                case FileCategory.Document:
                    isValid = file.IsValidDocument(); // Belge mi? (PDF, DOC)
                    break;

                default:
                    isValid = file.IsValidFile();
                    break;
            }

            if (!isValid)
            {
                var ext = Path.GetExtension(file.FileName);
                throw new Exception($"Geçersiz dosya formatı. ({category} için '{ext}' kabul edilmiyor)");
            }

            // 2. KLASÖR BELİRLEME (Dinamik Kısım)
            // Burası dosyanın nereye gideceğini seçer.
            string folderName = category switch
            {
                FileCategory.ProfileImage => "profile_images", // Resimse buraya
                FileCategory.Document => "documents",          // Belgeyse buraya
                _ => "others"
            };

            // Yol Oluşturma
            string webRootPath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            string uploadPath = Path.Combine(webRootPath, "uploads", folderName);

            // Klasör yoksa oluştur (Örn: wwwroot/uploads/documents)
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            // 3. FİZİKSEL KAYIT (Ortak Kısım)
            // Burası hem resim hem de belge için çalışır.

            string fileExt = Path.GetExtension(file.FileName).ToLowerInvariant();
            string uniqueFileName = $"{Guid.NewGuid()}{fileExt}";
            string fullPath = Path.Combine(uploadPath, uniqueFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream); // Dosyayı diske yazar
            }

            // 4. VERİTABANI KAYDI
            var fileRecorder = new FileRecorder
            {
                Id = Guid.NewGuid(),
                OriginalFileName = file.FileName,
                FileName = uniqueFileName,
                FileExtension = fileExt,
                ContentType = file.ContentType,
                FileSize = file.Length,
                FilePath = $"/uploads/{folderName}/{uniqueFileName}", // Web URL'i
                FileCategory = category,
                CreatedBy = createdBy
            };

            await _fileRepository.AddAsync(fileRecorder);

            return fileRecorder;
        }

        public async Task DeleteFileAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            // 1. VERİTABANI İŞLEMİ (Soft Delete)
            var fileRecord = await _fileRepository.GetByPathAsync(filePath);

            if (fileRecord != null)
            {
                // Repository'deki SoftDelete metodunu çağır (Interceptor IsDeleted=1 yapar)
                await _fileRepository.SoftDeleteAsync(fileRecord);
            }

            // 2. FİZİKSEL SİLME İŞLEMİ
            string webRootPath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");

            // URL yolunu fiziksel yola çevir
            string relativePath = filePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            string physicalPath = Path.Combine(webRootPath, relativePath);

            if (File.Exists(physicalPath))
            {
                try
                {
                    File.Delete(physicalPath);
                }
                catch (Exception ex)
                {
                    // Fiziksel silme başarısız olsa bile DB'den silindiği için sorun yok.
                    // Log atılabilir.
                    Console.WriteLine($"Fiziksel dosya silinemedi: {ex.Message}");
                }
            }
        }
    }
}