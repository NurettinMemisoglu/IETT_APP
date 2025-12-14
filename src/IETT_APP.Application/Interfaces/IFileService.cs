using IETT_APP.Domain.Entities;
using IETT_APP.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace IETT_APP.Application.Interfaces
{
    public interface IFileService
    {
        // Dosyayı fiziksel olarak kaydeder ve DB kaydını döner
        Task<FileRecorder> SaveFileAsync(IFormFile file, FileCategory category, string createdBy);

        // Dosyayı fiziksel olarak siler (Eski resmi temizlemek için)
        Task DeleteFileAsync(string filePath);
    }
}