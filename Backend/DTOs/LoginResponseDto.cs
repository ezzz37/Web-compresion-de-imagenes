namespace Backend.DTOs
{
    /// DTO que representa la respuesta al login: token JWT y fecha de expiración.
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
    }
}
