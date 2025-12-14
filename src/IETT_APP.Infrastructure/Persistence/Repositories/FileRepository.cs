using IETT_APP.Domain.Entities;
using IETT_APP.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IETT_APP.Infrastructure.Persistence.Repositories
{
    public class FileRepository : IFileRepository
    {
        private readonly AppDbContext _context;

        public FileRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(FileRecorder fileRecorder)
        {
            await _context.FileRecorders.AddAsync(fileRecorder);
            await _context.SaveChangesAsync();
        }

        public async Task<FileRecorder?> GetByIdAsync(Guid id)
        {
            return await _context.FileRecorders.FindAsync(id);
        }

        public async Task<FileRecorder?> GetByPathAsync(string filePath)
        {
            // Web yoluna göre eşleşen kaydı bul
            return await _context.FileRecorders
                .FirstOrDefaultAsync(f => f.FilePath == filePath && !f.IsDeleted);
        }
        public async Task SoftDeleteAsync(FileRecorder entity)
        {
            // Interceptor devreye gireceği için Remove çağırıyoruz.
            // Interceptor bunu "Update IsDeleted = 1" sorgusuna çevirecek.
            _context.FileRecorders.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}