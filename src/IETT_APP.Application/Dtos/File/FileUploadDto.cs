using Microsoft.AspNetCore.Http;

namespace IETT_APP.Application.Dtos.File
{
    public class FileUploadDto
    {
        IFormFile File { get; set; } = null!;
    }
}
