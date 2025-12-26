namespace IETT_APP.Application.Dtos
{
    public class UserDto
    {
        public string Id { get; set; } = null!;
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public List<string> RoleNames { get; set; } = new();
        public string? UserName { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}
