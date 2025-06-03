namespace Backend.Data
{
    public class JwtSettings
    {

        public string SecretKey { get; set; }

        /// El emisor (“issuer”) que colocas en el claim "iss" dentro del JWT.
        public string Issuer { get; set; }

        /// El destinatario (“audience”) que colocas en el claim "aud" dentro del JWT.
        public string Audience { get; set; }

        public int ExpiryMinutes { get; set; }
    }
}
