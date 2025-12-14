using IETT_APP.Domain.Entities;

namespace IETT_APP.Domain.Interfaces
{
    public interface IFileRepository
    {
        Task AddAsync(FileRecorder fileRecorder);
        Task<FileRecorder?> GetByIdAsync(Guid id);
        Task<FileRecorder?> GetByPathAsync(string filePath);
        Task SoftDeleteAsync(FileRecorder entity);
    }
}