using System.ComponentModel.DataAnnotations;

namespace cubichi.Models
{
    public class SkinUploading
    {
        [Required]
        public string UserName { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        public IFormFile File { get; set; } = null!;
    }
}