using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;


namespace Backend.Services
{
    public interface IImageProcessorService
    {
        /// <summary>
        /// Muestrea la imagen original a la resolución indicada.
        /// </summary>
        byte[] Muestrear(byte[] original, int ancho, int alto);

        /// <summary>
        /// Cuantiza la imagen (reduce profundidad de bits).
        /// </summary>
        byte[] Cuantizar(byte[] datos, byte profundidadBits);

        /// <summary>
        /// Comprime los datos con el algoritmo especificado ('JPEG','PNG','RLE', etc).
        /// </summary>
        byte[] Comprimir(byte[] datos, string algoritmo);

        /// <summary>
        /// Compara dos imagenes y devuelve tupla (mse, psnr, diffImage).
        /// </summary>
        Task<(double mse, double psnr, byte[] diff)> CompararAsync(
            byte[] original,
            byte[] procesada
        );
    }
}
