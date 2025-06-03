using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs
{
    public class ProcesarDto
    {
        [Required]
        public int AnchoResolucion { get; set; }

        [Required]
        public int AltoResolucion { get; set; }

        [Required]
        [Range(1, 24, ErrorMessage = "La profundidad de bits debe ser 1, 8 o 24.")]
        public byte ProfundidadBits { get; set; }

        [Required]
        public string Algoritmo { get; set; } = string.Empty;

        [Required]
        public int IdAlgoritmoCompresion { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "El nivel de compresión debe estar entre 0 y 100.")]
        public int NivelCompresion { get; set; }
    }
}
