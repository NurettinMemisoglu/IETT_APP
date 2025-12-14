using AutoMapper;
using IETT_APP.Application.Dtos;
using IETT_APP.Application.Interfaces;
using IETT_APP.Domain.Entities;
using IETT_APP.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;    // Eklendi

namespace IETT_APP.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IDriverRepository _driverRepository;
        private readonly IUserRepository _userRepository;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor; // 1. Eklendi

        public UserService(
            IDriverRepository driverRepository,
            IUserRepository userRepository,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor) // Constructor'a eklendi
        {
            _driverRepository = driverRepository;
            _userRepository = userRepository;
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        // Helper: O anki kullanıcının ID'sini veya ismini getirir
        private string GetCurrentUserIdOrName()
        {
            // Kullanıcı login olmuşsa ID'sini, olmamışsa (örn: sistem job'ı) "System" döner
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = _httpContextAccessor.HttpContext?.User?.Identity?.Name;

            return userId ?? userName ?? "System";
        }

        public async Task<UserDto> CreateUserAsync(UserDto userDto, string password)
        {
            var user = _mapper.Map<User>(userDto);

            user.UserName = userDto.Email; // Username genelde Email ile aynıdır
            user.EmailConfirmed = true;

            // 2. Gerçek kullanıcı bilgisini alıyoruz (Audit)
            string createdBy = GetCurrentUserIdOrName();

            // Repository üzerinden kayıt (Interceptor tarihleri halledecek)
            var result = await _userRepository.AddAsync(user, password, createdBy: createdBy);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Kullanıcı oluşturulamadı: {errors}");
            }

            // ============================================================
            // 🔥 GÜNCELLENEN KISIM: OTOMATİK ROL ATAMA
            // ============================================================

            // 1. Liste null ise başlat (Hata almamak için)
            if (userDto.RoleNames == null)
            {
                userDto.RoleNames = new List<string>();
            }

            // 2. Listede "User" yoksa ekle.
            // Böylece kullanıcı "Driver" veya "Admin" olsa bile, temel "User" haklarına da sahip olur.
            if (!userDto.RoleNames.Contains("User"))
            {
                userDto.RoleNames.Add("User");
            }

            try
            {
                foreach (var role in userDto.RoleNames)
                {
                    // Sistemde bu rol henüz tanımlanmamışsa oluştur (Güvenlik ağı)
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(role));
                    }

                    var roleResult = await _userRepository.AddToRoleAsync(user, role);

                    if (!roleResult.Succeeded)
                    {
                        var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                        throw new Exception($"Rol '{role}' atanamadı: {errors}");
                    }
                }
            }
            catch (Exception)
            {
                // 3. ROLLBACK (Telafi İşlemi):
                // Kullanıcı oluştu ama rollerde patladıysak, yarım yamalak veri kalmasın.
                // Soft delete ile kullanıcıyı hemen siliyoruz.
                await _userRepository.SoftDeleteAsync(user.Id, deletedBy: "System_Rollback");
                throw; // Hatayı yukarı fırlat
            }

            userDto.Id = user.Id;
            return userDto;
        }

        public async Task<UserDto> UpdateUserAsync(UserDto userDto)
        {
            // 1. Kullanıcıyı bul
            var user = await _userRepository.GetByIdAsync(userDto.Id);
            if (user == null) throw new Exception("Kullanıcı bulunamadı.");

            // 2. Alanları güncelle (Manuel mapping daha güvenlidir)
            user.Name = userDto.Name;
            user.Surname = userDto.Surname;

            // Email/UserName değişimi
            if (user.Email != userDto.Email)
            {
                user.Email = userDto.Email;
                user.UserName = userDto.Email; // Email ile Username aynı ise
            }

            // 3. Güncelleyen kişiyi al ve Update işlemini yap
            string updatedBy = GetCurrentUserIdOrName();

            var result = await _userRepository.UpdateAsync(user, updatedBy: updatedBy);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Kullanıcı güncellenemedi: {errors}");
            }

            // 4. Rol Güncelleme Mantığı
            if (userDto.RoleNames != null)
            {
                var currentRoles = await _userRepository.GetUserRolesAsync(user);

                // Eklenecekler: Yeni listede var ama eskide yok
                var addedRoles = userDto.RoleNames.Except(currentRoles).ToList();

                // Silinecekler: Eskide var ama yeni listede yok
                var removedRoles = currentRoles.Except(userDto.RoleNames).ToList();

                if (addedRoles.Any())
                {
                    // Olmayan rolleri önce oluştur (Opsiyonel güvenlik)
                    foreach (var role in addedRoles)
                    {
                        if (!await _roleManager.RoleExistsAsync(role))
                            await _roleManager.CreateAsync(new IdentityRole(role));
                    }
                    await _userManager.AddToRolesAsync(user, addedRoles);
                }

                if (removedRoles.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, removedRoles);
                }
            }

            // 5. Geriye güncel DTO dön (Interface Task<UserDto> istediği için)
            // Rolleri son haliyle tekrar çekip DTO'ya koyuyoruz
            var finalRoles = await _userRepository.GetUserRolesAsync(user);
            userDto.RoleNames = finalRoles.ToList();

            return userDto;
        }

        public async Task<UserDto?> GetByIdAsync(string id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return null;

            var userDto = _mapper.Map<UserDto>(user);

            var roles = await _userRepository.GetUserRolesAsync(user);
            userDto.RoleNames = roles.ToList();

            return userDto;
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();
            var userDtos = new List<UserDto>();

            // Mapper ile toplu dönüşüm de yapılabilir: _mapper.Map<List<UserDto>>(users);
            // Ancak Rolleri tek tek çekmek gerektiği için döngü şart (Identity yapısı gereği)
            foreach (var user in users)
            {
                var dto = _mapper.Map<UserDto>(user);
                var roles = await _userRepository.GetUserRolesAsync(user);
                dto.RoleNames = roles.ToList();
                userDtos.Add(dto);
            }

            return userDtos;
        }

        public async Task<IdentityResult> DeleteUserAsync(string userId)
        {
            // 1. Kullanıcıyı bul
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return IdentityResult.Failed(new IdentityError { Description = "Kullanıcı bulunamadı." });

            // 2. Varsa Driver profilini bul ve sil (Cascade Soft Delete)
            var driver = await _driverRepository.GetByUserIdAsync(userId);
            if (driver != null)
            {
                await _driverRepository.SoftDeleteAsync(driver);
            }

            // 3. Kullanıcıyı sil (Repository'deki SoftDeleteAsync metodunu çağırıyoruz)
            // Not: Repository metodunun adı SoftDeleteAsync ise onu kullan.
            string deletedBy = GetCurrentUserIdOrName();

            // Repository'deki metodun IdentityResult döndüğünden emin ol. 
            // Eğer repository void dönüyorsa IdentityResult.Success dönebiliriz.
            var result = await _userRepository.SoftDeleteAsync(userId, deletedBy);

            return result;
        }

        // --- ROL METODLARI ---

        public async Task<List<string>> GetAllRolesAsync()
        {
            return await _roleManager.Roles.Select(r => r.Name).ToListAsync();
        }

        public async Task AssignRoleToUserAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new Exception("Kullanıcı bulunamadı.");

            if (!await _roleManager.RoleExistsAsync(roleName))
                await _roleManager.CreateAsync(new IdentityRole(roleName));

            if (!await _userManager.IsInRoleAsync(user, roleName))
                await _userManager.AddToRoleAsync(user, roleName);
        }

        public async Task RemoveRoleFromUserAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new Exception("Kullanıcı bulunamadı.");

            if (await _userManager.IsInRoleAsync(user, roleName))
                await _userManager.RemoveFromRoleAsync(user, roleName);
        }

        public async Task CreateRoleAsync(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName)) throw new Exception("Rol ismi boş olamaz.");
            if (await _roleManager.RoleExistsAsync(roleName)) throw new Exception("Rol zaten mevcut.");

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Rol oluşturulamadı: {errors}");
            }
        }
        // IETT_APP.Infrastructure/Services/UserService.cs

        public async Task DeleteRoleAsync(string roleName)
        {
            // 1. Rol var mı kontrolü
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
                throw new Exception("Silinecek rol bulunamadı.");

            // 2. Kritik Rol Koruması (Admin silinemez)
            if (roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                roleName.Equals("User", StringComparison.OrdinalIgnoreCase)) // Opsiyonel: Varsayılan user da silinmesin
            {
                throw new Exception($"'{roleName}' sistemin temel rolüdür ve silinemez.");
            }

            // 3. İlişki Kontrolü: Bu role sahip kullanıcı var mı?
            // (Eğer varsa silmek veri tutarsızlığı yaratır)
            var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
            if (usersInRole.Any())
            {
                throw new Exception($"Bu role atanmış {usersInRole.Count} adet kullanıcı bulunmaktadır. Önce kullanıcıları bu rolden çıkarınız.");
            }

            // 4. Silme İşlemi
            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Rol silinemedi: {errors}");
            }
        }
    }
}