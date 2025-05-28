using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace Backend.Services
{
    public class ImageProcessorService : IImageProcessorService
    {
        /// <summary>
        /// Muestrea (resample) la imagen a la resolución dada.
        /// </summary>
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

        /// <summary>
        /// Reduce la profundidad de bits por píxel: de 1 hasta 24 bits
        /// (máximo 8 bits por canal).
        /// </summary>
        public byte[] Cuantizar(byte[] datos, byte profundidadBits)
        {
            if (profundidadBits < 1 || profundidadBits > 24)
                throw new ArgumentOutOfRangeException(
                    nameof(profundidadBits),
                    "La profundidad debe estar entre 1 y 24 bits por píxel."
                );

            // Calcula cuántos bits usar por canal (clamp a 8)
            int bitsPorCanal = profundidadBits > 8 ? 8 : profundidadBits;
            int niveles = 1 << bitsPorCanal; // 2^bitsPorCanal

            using var msIn = new MemoryStream(datos);
            using var img = Image.FromStream(msIn);
            using var bmp = new Bitmap(img);

            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    var orig = bmp.GetPixel(x, y);
                    byte r = QuantizarCanal(orig.R, niveles);
                    byte g = QuantizarCanal(orig.G, niveles);
                    byte b = QuantizarCanal(orig.B, niveles);
                    bmp.SetPixel(x, y, Color.FromArgb(orig.A, r, g, b));
                }
            }

            using var msOut = new MemoryStream();
            bmp.Save(msOut, ImageFormat.Png);
            return msOut.ToArray();
        }

        /// <summary>
        /// Comprime en JPEG (calidad 75) o PNG.
        /// </summary>
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

        /// <summary>
        /// Compara dos imágenes (deben tener la misma resolución o se reescalará la original),
        /// calcula MSE, PSNR y devuelve un mapa de diferencias absoluto.
        /// </summary>
        public async Task<(double mse, double psnr, byte[] diff)> CompararAsync(
            byte[] original,
            byte[] procesada
        )
        {
            await Task.CompletedTask;

            using var msOrig0 = new MemoryStream(original);
            using var msProc = new MemoryStream(procesada);
            using var bmpProc = new Bitmap(Image.FromStream(msProc));

            // Reescalar original si difiere de tamaño
            using var bmpOrig0 = new Bitmap(Image.FromStream(msOrig0));
            Bitmap bmpOrig = bmpOrig0;
            if (bmpOrig0.Width != bmpProc.Width || bmpOrig0.Height != bmpProc.Height)
            {
                bmpOrig = new Bitmap(bmpProc.Width, bmpProc.Height);
                using var g = Graphics.FromImage(bmpOrig);
                g.DrawImage(bmpOrig0, 0, 0, bmpProc.Width, bmpProc.Height);
                bmpOrig0.Dispose();
            }

            int w = bmpProc.Width, h = bmpProc.Height;
            double sumErr = 0;
            var diffBmp = new Bitmap(w, h);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var c1 = bmpOrig.GetPixel(x, y);
                    var c2 = bmpProc.GetPixel(x, y);
                    int dr = c1.R - c2.R,
                        dg = c1.G - c2.G,
                        db = c1.B - c2.B;

                    sumErr += dr * dr + dg * dg + db * db;

                    diffBmp.SetPixel(
                        x, y,
                        Color.FromArgb(
                            Math.Abs(dr),
                            Math.Abs(dg),
                            Math.Abs(db)
                        )
                    );
                }
            }

            double mse = sumErr / (w * h * 3);

            // Evitar PSNR infinito o NaN
            double psnr = (mse == 0)
                ? 100.0
                : 10 * Math.Log10(255 * 255 / mse);
            if (double.IsInfinity(psnr) || double.IsNaN(psnr))
                psnr = 100.0;

            using var msDiff = new MemoryStream();
            diffBmp.Save(msDiff, ImageFormat.Png);

            return (mse, psnr, msDiff.ToArray());
        }

        #region Helpers

        private static byte QuantizarCanal(byte valor, int niveles)
        {
            int q = (int)Math.Round(valor * (niveles - 1) / 255.0);
            return (byte)(q * 255 / (niveles - 1));
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
            => Array.Find(
                ImageCodecInfo.GetImageEncoders(),
                c => c.FormatID == format.Guid
            );

        #endregion
    }
}
