using IETT_APP.Application.Dtos;
using IETT_APP.Application.Interfaces;
using IETT_APP.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IETT_APP.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(IUserService userService, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userService = userService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: api/users
        [HttpGet]
        public async Task<IEnumerable<UserDto>> GetAll()
        {
            return await _userService.GetAllAsync();
        }

        // GET: api/users/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetById(string id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound();
            return user;
        }

        // POST: api/users
        [HttpPost]
        public async Task<ActionResult<UserDto>> Create([FromBody] UserDto userDto, [FromQuery] string password)
        {
            var user = await _userService.CreateUserAsync(userDto, password);

            // Kullanıcı başarıyla oluşturulduysa otomatik 'User' rolü ata
            if (user != null)
            {
                // Eğer 'User' rolü yoksa oluştur
                if (!await _roleManager.RoleExistsAsync("User"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("User"));
                }

                var createdUser = await _userManager.FindByIdAsync(user.Id);
                if (createdUser != null && !await _userManager.IsInRoleAsync(createdUser, "User"))
                {
                    await _userManager.AddToRoleAsync(createdUser, "User");
                }
            }

            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }


        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _userService.DeleteUserAsync(id);
            return NoContent();
        }

        // GET: api/roles
        // UsersController
        [HttpGet("roles")]
        public ActionResult<IEnumerable<IdentityRole>> GetRoles()
        {
            var roles = _roleManager.Roles.ToList();
            return Ok(roles);
        }


        // POST: api/users/{id}/roles/add
        [HttpPost("{id}/roles/add")]
        public async Task<IActionResult> AddRoleToUser(string id, [FromQuery] string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return BadRequest("Rol ismi geçerli değil.");

            var identityUser = await _userManager.FindByIdAsync(id);
            if (identityUser == null)
                return NotFound("Kullanıcı bulunamadı.");

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole(roleName));
                if (!roleResult.Succeeded)
                    return BadRequest(string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            }

            if (await _userManager.IsInRoleAsync(identityUser, roleName))
                return BadRequest("Kullanıcı zaten bu role sahip.");

            var result = await _userManager.AddToRoleAsync(identityUser, roleName);
            if (!result.Succeeded)
                return BadRequest(string.Join(", ", result.Errors.Select(e => e.Description)));

            return Ok(new { Message = $"Rol '{roleName}' kullanıcıya başarıyla eklendi." });
        }

        // POST: api/users/{id}/roles/remove
        [HttpPost("{id}/roles/remove")]
        public async Task<IActionResult> RemoveRoleFromUser(string id, [FromQuery] string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return BadRequest("Rol ismi geçerli değil.");

            var identityUser = await _userManager.FindByIdAsync(id);
            if (identityUser == null)
                return NotFound("Kullanıcı bulunamadı.");

            if (!await _userManager.IsInRoleAsync(identityUser, roleName))
                return BadRequest("Kullanıcı bu role sahip değil.");

            var result = await _userManager.RemoveFromRoleAsync(identityUser, roleName);
            if (!result.Succeeded)
                return BadRequest(string.Join(", ", result.Errors.Select(e => e.Description)));

            return Ok(new { Message = $"Rol '{roleName}' kullanıcıdan başarıyla kaldırıldı." });
        }

        // POST: api/roles/create
        [HttpPost("/api/roles/create")]
        public async Task<IActionResult> CreateRole([FromQuery] string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return BadRequest("Rol ismi boş olamaz.");

            if (await _roleManager.RoleExistsAsync(roleName))
                return BadRequest("Rol zaten mevcut.");

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (!result.Succeeded)
                return BadRequest(string.Join(", ", result.Errors.Select(e => e.Description)));

            return Ok(new { Message = $"Rol '{roleName}' başarıyla oluşturuldu." });
        }

        // DELETE: api/roles/{roleId}
        [HttpDelete("/api/roles/{roleId}")]
        public async Task<IActionResult> DeleteRole(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return NotFound("Rol bulunamadı.");

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
                return BadRequest(string.Join(", ", result.Errors.Select(e => e.Description)));

            return Ok(new { Message = $"Rol '{role.Name}' başarıyla silindi." });
        }

    }
}