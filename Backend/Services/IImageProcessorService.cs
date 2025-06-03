using System.Threading.Tasks;

namespace Backend.Services
{
    public interface IImageProcessorService
    {
        byte[] Muestrear(byte[] original, int ancho, int alto);
        byte[] Cuantizar(byte[] datos, byte profundidadBits);
        byte[] Comprimir(byte[] datos, string algoritmo, int nivel);
        Task<(double mse, double psnr, byte[] diff)> CompararAsync(
            byte[] original,
            byte[] procesada
        );
    }
}
