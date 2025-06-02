namespace Backend.Models
{
    public class Comparaciones
    {
        public int IdComparacion { get; set; }
        public int IdImagenOriginal { get; set; }
        public Imagen ImagenOriginal { get; set; }
        public int IdImagenProcesada { get; set; }
        public ImagenProcesada ImagenProcesada { get; set; }
        public double? MSE { get; set; }
        public double? PSNR { get; set; }
        public byte[] ImagenDiferencias { get; set; }
        public DateTime FechaComparacion { get; set; }
    }
}
