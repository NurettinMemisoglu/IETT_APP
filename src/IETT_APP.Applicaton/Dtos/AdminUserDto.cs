namespace IETT_APP.Application.Dtos
{
    public class AdminUserDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }

        // Admin oluştururken şifre de alınacağı için ekliyoruz
        public string Password { get; set; }

        // Admin'in atanacağı rol ismi (örneğin: "Admin")
        public string RoleName { get; set; } = "Admin";
    }
}
