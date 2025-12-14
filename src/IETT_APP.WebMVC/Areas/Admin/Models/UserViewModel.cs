namespace IETT_APP.WebMVC.Areas.Admin.Models
{
    public class UserViewModel
    {
        // DEĞİŞİKLİK: Artık UserDto değil, ViewModel listesi tutuyoruz.
        public IEnumerable<UserUpdateViewModel> Users { get; set; } = new List<UserUpdateViewModel>();

        public IEnumerable<RoleViewModel> Roles { get; set; } = new List<RoleViewModel>();
    }
}