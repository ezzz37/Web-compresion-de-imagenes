using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs
{
    public class ImagenUploadDto
    {
        [Required]
        public IFormFile Archivo { get; set; }

        [Required, MaxLength(255)]
        public string Nombre { get; set; }
    }
}
