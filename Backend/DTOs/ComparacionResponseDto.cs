using System;

namespace Backend.DTOs
{
    public class ComparacionResponseDto
    {
        public int IdComparacion { get; set; }
        public int IdImagenOriginal { get; set; }
        public int IdImagenProcesada { get; set; }
        public double Mse { get; set; }
        public double Psnr { get; set; }
        public string ImagenDiferenciasBase64 { get; set; }
        public DateTime FechaComparacion { get; set; }
    }
}
