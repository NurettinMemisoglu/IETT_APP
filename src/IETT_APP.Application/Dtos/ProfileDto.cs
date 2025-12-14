namespace IETT_APP.Application.Dtos
{
    namespace IETT_APP.Application.Dtos
    {
        public class ProfileDto
        {
            public string Email { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;    // FullName yerine
            public string Surname { get; set; } = string.Empty; // FullName yerine

            // Opsiyonel: Ekranda tek satırda göstermek istersen bir helper property koyabilirsin
            public string FullName => $"{Name} {Surname}";
        }
    }
}
