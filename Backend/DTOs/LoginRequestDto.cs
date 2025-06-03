namespace Backend.DTOs
{
    /// DTO que representa la petición de login.
    /// Contiene Username y Password en texto plano.
    public class LoginRequestDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
