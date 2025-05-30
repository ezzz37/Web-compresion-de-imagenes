using System;
using System.Text.Json.Serialization;

public class ImagenProcesadaResponseDto
{
    public int IdImagenProcesada { get; set; }
    public int AnchoResolucion { get; set; }
    public int AltoResolucion { get; set; }
    public byte ProfundidadBits { get; set; }
    public DateTime FechaProcesamiento { get; set; }
    public int IdAlgoritmoCompresion { get; set; }
    public int IdImagenOriginal { get; set; }

    public double? RatioCompresion { get; set; } // ✅ Corregido

    public string ImagenOriginal { get; set; } = string.Empty;
    public string Algoritmo { get; set; } = string.Empty;

    [JsonPropertyName("DatosProcesadosBase64")]
    public string DatosProcesadosBase64 { get; set; }
}
