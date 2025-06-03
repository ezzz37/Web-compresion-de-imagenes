using System.Text.Json.Serialization;

public class ComparacionResponseDto
{
    [JsonPropertyName("idComparacion")]
    public int IdComparacion { get; set; }

    [JsonPropertyName("idImagenOriginal")]
    public int IdImagenOriginal { get; set; }

    [JsonPropertyName("idImagenProcesada")]
    public int IdImagenProcesada { get; set; }

    [JsonPropertyName("mse")]
    public double Mse { get; set; }

    [JsonPropertyName("psnr")]
    public double Psnr { get; set; }

    [JsonPropertyName("imagenDiferenciasBase64")]
    public string? ImagenDiferenciasBase64 { get; set; }

    [JsonPropertyName("fechaComparacion")]
    public DateTime FechaComparacion { get; set; }
}
