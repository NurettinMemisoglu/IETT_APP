namespace IETT_APP.Domain.Entities
{
    public class UserRefreshToken
    {
        public int Id { get; set; }
        public string UserId { get; set; } = default!;       // FK: AspNetUsers tablosundaki Id
        public User User { get; set; } = default!;              // Navigation property
        public string Token { get; set; } = default!;
        public DateTime ExpiryTime { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
