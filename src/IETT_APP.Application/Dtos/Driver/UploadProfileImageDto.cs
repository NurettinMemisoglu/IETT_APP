using IETT_APP.Application.Attributes;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace IETT_APP.Application.Dtos.Driver
{
    public class UploadProfileImageDto
    {
        [Required]
        [AllowedExtensions(new[] { ".jpg", ".jpeg", ".png" })]
        public IFormFile Photo { get; set; } = null!;
    }
}