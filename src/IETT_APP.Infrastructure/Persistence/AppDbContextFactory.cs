using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace IETT_APP.Infrastructure.Persistence
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // 1. Çalışma dizinini al
            var basePath = Directory.GetCurrentDirectory();

            // 2. Eğer Infrastructure klasöründeysek, WebAPI klasörüne geçiş yap
            if (!File.Exists(Path.Combine(basePath, "appsettings.json")))
            {
                var webApiPath = Path.Combine(Directory.GetParent(basePath)!.FullName, "IETT_APP.WebAPI");
                if (Directory.Exists(webApiPath))
                {
                    basePath = webApiPath;
                }
            }

            // 3. Konfigürasyonu yükle
            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath) // Paket yüklendikten sonra burası çalışacak
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // 4. Connection String'i al
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException($"HATA: 'DefaultConnection' bulunamadı. Bakılan yer: {Path.Combine(basePath, "appsettings.json")}");
            }

            // 5. DbContext'i oluştur
            var builder = new DbContextOptionsBuilder<AppDbContext>();
            builder.UseSqlServer(connectionString);

            return new AppDbContext(builder.Options);
        }
    }
}