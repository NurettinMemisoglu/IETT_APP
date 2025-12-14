using IETT_APP.Application.Dtos;
using IETT_APP.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IETT_APP.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // ==================================================================================
        // KULLANICI İŞLEMLERİ (CRUD)
        // ==================================================================================

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        // GET: api/users/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetById(string id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound(new { Message = "Kullanıcı bulunamadı." });
            return Ok(user);
        }

        // POST: api/users
        [HttpPost]
        public async Task<ActionResult<UserDto>> Create([FromBody] UserDto userDto, [FromQuery] string password)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var createdUser = await _userService.CreateUserAsync(userDto, password);
                return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // PUT: api/users/{id}  <-- EKSİKTİ, EKLENDİ
        [HttpPut("{id}")]
        public async Task<ActionResult<UserDto>> Update(string id, [FromBody] UserDto userDto)
        {
            if (id != userDto.Id)
                return BadRequest(new { Message = "ID uyuşmazlığı." });

            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var updatedUser = await _userService.UpdateUserAsync(userDto);
                return Ok(updatedUser);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _userService.DeleteUserAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // ==================================================================================
        // ROL İŞLEMLERİ
        // ==================================================================================

        // GET: api/users/roles
        [HttpGet("roles")]
        public async Task<ActionResult<IEnumerable<string>>> GetAllRoles()
        {
            var roles = await _userService.GetAllRolesAsync();
            return Ok(roles);
        }

        // POST: api/users/roles/create
        [HttpPost("roles/create")]
        public async Task<IActionResult> CreateRole([FromQuery] string roleName)
        {
            try
            {
                await _userService.CreateRoleAsync(roleName);
                return Ok(new { Message = $"Rol '{roleName}' başarıyla oluşturuldu." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // DELETE: api/users/roles/{roleName}  <-- İSTEĞİN ÜZERİNE EKLENDİ
        [HttpDelete("roles/{roleName}")]
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            try
            {
                await _userService.DeleteRoleAsync(roleName);
                return Ok(new { Message = $"Rol '{roleName}' başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // POST: api/users/{id}/roles/add
        [HttpPost("{id}/roles/add")]
        public async Task<IActionResult> AddRoleToUser(string id, [FromQuery] string roleName)
        {
            try
            {
                await _userService.AssignRoleToUserAsync(id, roleName);
                return Ok(new { Message = $"Rol '{roleName}' kullanıcıya başarıyla eklendi." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // POST: api/users/{id}/roles/remove
        [HttpPost("{id}/roles/remove")]
        public async Task<IActionResult> RemoveRoleFromUser(string id, [FromQuery] string roleName)
        {
            try
            {
                await _userService.RemoveRoleFromUserAsync(id, roleName);
                return Ok(new { Message = $"Rol '{roleName}' kullanıcıdan başarıyla kaldırıldı." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}