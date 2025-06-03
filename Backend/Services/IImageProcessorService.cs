using System.Threading.Tasks;

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
        /// Comprime los datos con el algoritmo especificado ('JPEG', 'PNG', 'RLE', etc.)
        /// utilizando el nivel de compresión proporcionado.
        /// </summary>
        /// <param name="datos">Bytes de la imagen a comprimir.</param>
        /// <param name="algoritmo">Algoritmo de compresión (por ejemplo, "JPEG" o "PNG").</param>
        /// <param name="nivel">
        /// Para JPEG: calidad de 1 a 100 (1 = peor calidad / menor tamaño, 100 = mejor calidad / mayor tamaño).
        /// Para PNG: nivel de compresión de 0 a 9 (0 = sin compresión, 9 = máxima compresión).
        /// </param>
        byte[] Comprimir(byte[] datos, string algoritmo, int nivel);

        /// <summary>
        /// Compara dos imágenes y devuelve una tupla (mse, psnr, diffImage).
        /// </summary>
        Task<(double mse, double psnr, byte[] diff)> CompararAsync(
            byte[] original,
            byte[] procesada
        );
    }
}
