using IETT_APP.Application.Dtos;
using Microsoft.AspNetCore.Identity;

namespace IETT_APP.WebMVC.Areas.Admin.Models
{
    public class UserViewModel
    {
        public IEnumerable<UserDto> Users { get; set; } = new List<UserDto>();
        public IEnumerable<IdentityRole> Roles { get; set; } = new List<IdentityRole>();

    }
}
