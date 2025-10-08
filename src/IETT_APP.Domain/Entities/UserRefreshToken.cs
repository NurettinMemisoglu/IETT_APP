namespace IETT_APP.Domain.Entities
{
    public class UserRefreshToken
    {
        public int Id { get; set; }
        public string UserId { get; set; }          // FK: AspNetUsers tablosundaki Id
        public User User { get; set; }              // Navigation property
        public string Token { get; set; }
        public DateTime ExpiryTime { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
