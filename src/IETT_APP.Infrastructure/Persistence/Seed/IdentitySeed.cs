using IETT_APP.Domain.Entities;
using IETT_APP.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IETT_APP.Infrastructure.Persistence.Seed
{
    public class IdentitySeed
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // DbContext'i direct alıyoruz ki hem Soft Delete kontrolü hem de Driver kaydı yapabilelim
            var context = serviceProvider.GetRequiredService<AppDbContext>();

            // Parametreler: (Role, Name, Surname, Email, Password, isDriver?)
            await CreateUserAndRoleAsync(context, userManager, roleManager, "Admin", "Admin", "System", "admin@gmail.com", "Admin123*");
            await CreateUserAndRoleAsync(context, userManager, roleManager, "Planner", "Planner", "System", "planner@gmail.com", "Planner123*");
            await CreateUserAndRoleAsync(context, userManager, roleManager, "Chief", "Chief", "System", "chief@gmail.com", "Chief123*");

            // Driver için isDriver: true gönderiyoruz
            await CreateUserAndRoleAsync(context, userManager, roleManager, "Driver", "Driver", "System", "driver@gmail.com", "Driver123*", isDriver: true);

            await CreateUserAndRoleAsync(context, userManager, roleManager, "User", "User", "System", "user@gmail.com", "User123*");
        }

        private static async Task CreateUserAndRoleAsync(
            AppDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            string roleName,
            string name,
            string surname,
            string email,
            string password,
            string phoneNumber = "5550000000",
            bool isDriver = false) // Driver parametresi eklendi
        {
            // 1. Rol Kontrolü
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }

            // 2. Kullanıcı Kontrolü (Silinmişleri de kontrol et)
            var normalizedEmail = email.ToUpperInvariant();
            var existingUser = await context.Users
                                            .IgnoreQueryFilters()
                                            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);

            User? user = existingUser; // Mevcut kullanıcıyı referans al

            if (user == null)
            {
                // Kullanıcı Yoksa Oluştur
                user = new User
                {
                    UserName = email.ToLowerInvariant(),
                    Name = name,
                    Surname = surname,
                    Email = email.ToLowerInvariant(),
                    PhoneNumber = phoneNumber,
                    EmailConfirmed = true,
                    IsActive = true,
                    IsDeleted = false
                };

                var result = await userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    Console.WriteLine($"[SEED] Kullanıcı oluşturuldu: {email}");
                    await userManager.AddToRoleAsync(user, roleName);
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    Console.WriteLine($"[SEED ERROR] Kullanıcı ({email}) oluşturulamadı: {errors}");
                    return; // Kullanıcı oluşmadıysa operatör kaydına geçme
                }
            }
            else
            {
                // Kullanıcı Varsa ve Silinmişse Bilgi Ver
                if (user.IsDeleted)
                {
                    Console.WriteLine($"[SEED INFO] Kullanıcı zaten var ama silinmiş: {email}");
                }
            }

            // 3. EĞER DRIVER İSE OPERATOR KAYDINI KONTROL ET VE OLUŞTUR
            if (isDriver && user != null)
            {
                await EnsureDriverProfileAsync(context, user);
            }
        }

        private static async Task EnsureDriverProfileAsync(AppDbContext context, User user)
        {
            // Operatör kaydı var mı? (Silinmiş olsa bile kontrol et)
            var operatorExists = await context.Drivers
                                              .IgnoreQueryFilters()
                                              .AnyAsync(o => o.UserId == user.Id);

            if (!operatorExists)
            {
                var newDriver = new Driver
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    EmployeeNumber = "DRV-SEED-001", // Örnek Sicil No
                    LicenseClass = "E",
                    LicenseNumber = "TR-SEED-12345",
                    WorkStatus = WorkStatus.Available,
                    IsActive = true,
                    IsDeleted = false
                };

                await context.Drivers.AddAsync(newDriver);
                await context.SaveChangesAsync();

                Console.WriteLine($"[SEED] Kullanıcı ({user.Email}) için Driver profili oluşturuldu.");
            }
        }
    }
}