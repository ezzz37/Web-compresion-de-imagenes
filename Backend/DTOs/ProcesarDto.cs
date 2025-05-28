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
        public byte ProfundidadBits { get; set; }
        [Required]
        public string Algoritmo { get; set; }
        [Required]
        public int IdAlgoritmoCompresion { get; set; }
    }
}