using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("Imagenes")]
    public class Imagen
    {
        public Imagen()
        {
            ImagenesProcesadas = new List<ImagenProcesada>();
            ComparacionesOriginal = new List<Comparacion>();
        }

        [Key]
        public int IdImagen { get; set; }

        [Required, MaxLength(255)]
        public string Nombre { get; set; }

        [Required]
        public byte[] DatosImagen { get; set; }

        public int AnchoOriginal { get; set; }
        public int AltoOriginal { get; set; }

        public DateTime FechaCarga { get; set; }

        // Una Imagen puede tener varias ImagenesProcesadas
        public ICollection<ImagenProcesada> ImagenesProcesadas { get; set; }

        // Una Imagen puede ser la "original" en varias Comparaciones
        [InverseProperty(nameof(Comparacion.ImagenOriginal))]
        public ICollection<Comparacion> ComparacionesOriginal { get; set; }
    }

    [Table("AlgoritmosCompresion")]
    public class AlgoritmoCompresion
    {
        public AlgoritmoCompresion()
        {
            ImagenesProcesadas = new List<ImagenProcesada>();
        }

        [Key]
        public int IdAlgoritmoCompresion { get; set; }

        [Required, MaxLength(50)]
        public string NombreAlgoritmo { get; set; }

        // Un Algoritmo puede aplicarse a varias ImagenesProcesadas
        public ICollection<ImagenProcesada> ImagenesProcesadas { get; set; }
    }

    [Table("ImagenesProcesadas")]
    public class ImagenProcesada
    {
        public ImagenProcesada()
        {
            Comparaciones = new List<Comparacion>();
        }

        [Key]
        public int IdImagenProcesada { get; set; }

        [ForeignKey(nameof(ImagenOriginal))]
        public int IdImagenOriginal { get; set; }

        // Navegación a la imagen original
        public Imagen ImagenOriginal { get; set; }

        public int AnchoResolucion { get; set; }
        public int AltoResolucion { get; set; }

        public byte ProfundidadBits { get; set; }

        [ForeignKey(nameof(AlgoritmoCompresion))]
        public int? IdAlgoritmoCompresion { get; set; }

        // Navegación al algoritmo de compresión (opcional)
        public AlgoritmoCompresion AlgoritmoCompresion { get; set; }

        public double? RatioCompresion { get; set; }

        [Required]
        public byte[] DatosProcesados { get; set; }

        public DateTime FechaProcesamiento { get; set; }

        // Una ImagenProcesada puede estar involucrada en varias Comparaciones
        [InverseProperty(nameof(Comparacion.ImagenProcesada))]
        public ICollection<Comparacion> Comparaciones { get; set; }
    }

    [Table("Comparaciones")]
    public class Comparacion
    {
        [Key]
        public int IdComparacion { get; set; }

        [ForeignKey(nameof(ImagenOriginal))]
        public int IdImagenOriginal { get; set; }

        // Navegación a la imagen original
        public Imagen ImagenOriginal { get; set; }

        [ForeignKey(nameof(ImagenProcesada))]
        public int IdImagenProcesada { get; set; }

        // Navegacion a la imagen procesada
        public ImagenProcesada ImagenProcesada { get; set; }

        public double? MSE { get; set; }
        public double? PSNR { get; set; }

        // VARBINARY(MAX) en la base de datos
        public byte[] ImagenDiferencias { get; set; }

        public DateTime FechaComparacion { get; set; }
    }
}
