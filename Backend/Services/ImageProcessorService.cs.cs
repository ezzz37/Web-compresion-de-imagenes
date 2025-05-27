using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace Backend.Services
{
    public class ImageProcessorService : IImageProcessorService
    {
        public byte[] Muestrear(byte[] original, int ancho, int alto)
        {
            using var msOrig = new MemoryStream(original);
            using var imgOrig = Image.FromStream(msOrig);
            using var bmp = new Bitmap(ancho, alto);
            using var g = Graphics.FromImage(bmp);
            g.DrawImage(imgOrig, 0, 0, ancho, alto);
            using var msOut = new MemoryStream();
            bmp.Save(msOut, ImageFormat.Png);
            return msOut.ToArray();
        }

        public byte[] Cuantizar(byte[] datos, byte profundidadBits)
        {
            // terminar imple
            return datos;
        }

        public byte[] Comprimir(byte[] datos, string algoritmo)
        {
            using var msIn = new MemoryStream(datos);
            using var img = Image.FromStream(msIn);
            using var msOut = new MemoryStream();

            if (algoritmo.Equals("JPEG", StringComparison.OrdinalIgnoreCase))
            {
                var codec = GetEncoder(ImageFormat.Jpeg);
                var encParams = new EncoderParameters(1);
                encParams.Param[0] = new EncoderParameter(
                    Encoder.Quality,
                    75L
                );
                img.Save(msOut, codec, encParams);
            }
            else
            {
                img.Save(msOut, ImageFormat.Png);
            }

            return msOut.ToArray();
        }

        public async Task<(double mse, double psnr, byte[] diff)> CompararAsync(
            byte[] original,
            byte[] procesada
        )
        {
            await Task.CompletedTask;
            return (0, 0, null);
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
            => Array.Find(
                ImageCodecInfo.GetImageEncoders(),
                c => c.FormatID == format.Guid
            );
    }
}
