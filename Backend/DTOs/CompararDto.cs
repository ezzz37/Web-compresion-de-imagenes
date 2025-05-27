using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs
{
    public class CompararDto
    {
        [Required]
        public int IdImagenOriginal { get; set; }

        [Required]
        public int IdImagenProcesada { get; set; }
    }
}
