namespace IETT_APP.Application.Dtos
{
    public class UserDto
    {
        public string Id { get; set; } = null!;
        public string? FullName { get; set; }
        public string Email { get; set; } = null!;
        public List<string> RoleNames { get; set; } = new();
        public string? UserName { get; set; }
    }
}
