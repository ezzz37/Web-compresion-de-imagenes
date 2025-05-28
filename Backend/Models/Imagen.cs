using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("Imagenes")]
    public class Imagen
    {
        [Key]
        public int IdImagen { get; set; }

        [Required, MaxLength(255)]
        public string Nombre { get; set; }

        [Required]
        public byte[] DatosImagen { get; set; }

        public int AnchoOriginal { get; set; }
        public int AltoOriginal { get; set; }

        public DateTime FechaCarga { get; set; }

        public ICollection<ImagenProcesada> ImagenesProcesadas { get; set; }
        public ICollection<Comparacion> ComparacionesOriginal { get; set; }
    }

    [Table("AlgoritmosCompresion")]
    public class AlgoritmoCompresion
    {
        [Key]
        public int IdAlgoritmoCompresion { get; set; }

        [Required, MaxLength(50)]
        public string NombreAlgoritmo { get; set; }

        public ICollection<ImagenProcesada> ImagenesProcesadas { get; set; }
    }

    [Table("ImagenesProcesadas")]
    public class ImagenProcesada
    {
        [Key]
        public int IdImagenProcesada { get; set; }

        [ForeignKey("ImagenOriginal")]
        public int IdImagenOriginal { get; set; }
        public Imagen ImagenOriginal { get; set; }

        public int AnchoResolucion { get; set; }
        public int AltoResolucion { get; set; }

        public byte ProfundidadBits { get; set; }

        [ForeignKey("AlgoritmoCompresion")]
        public int? IdAlgoritmoCompresion { get; set; }
        public AlgoritmoCompresion AlgoritmoCompresion { get; set; }

        public float? RatioCompresion { get; set; }

        [Required]
        public byte[] DatosProcesados { get; set; }

        public DateTime FechaProcesamiento { get; set; }

        public ICollection<Comparacion> Comparaciones { get; set; }
    }

    [Table("Comparaciones")]
    public class Comparacion
    {
        [Key]
        public int IdComparacion { get; set; }

        [ForeignKey("ImagenOriginal")]
        public int IdImagenOriginal { get; set; }
        public Imagen ImagenOriginal { get; set; }

        [ForeignKey("ImagenProcesada")]
        public int IdImagenProcesada { get; set; }
        public ImagenProcesada ImagenProcesada { get; set; }

        public float? MSE { get; set; }
        public float? PSNR { get; set; }

        public byte[] ImagenDiferencias { get; set; }

        public DateTime FechaComparacion { get; set; }
    }
}
