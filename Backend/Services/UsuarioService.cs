using Backend.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Backend.Services
{
    public class UsuarioService
    {
        private readonly AppDbContext _context;

        public UsuarioService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int?> ValidarCredencialesAsync(string username, string passwordPlano)
        {
            var connection = _context.Database.GetDbConnection();

            try
            {
                using var cmd = connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "dbo.sp_ValidarUsuario";

                cmd.Parameters.Add(new SqlParameter("@Username", SqlDbType.NVarChar, 50) { Value = username });
                cmd.Parameters.Add(new SqlParameter("@PasswordInput", SqlDbType.NVarChar, -1) { Value = passwordPlano });

                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                var resultado = await cmd.ExecuteScalarAsync();

                if (resultado != null && int.TryParse(resultado.ToString(), out int idUsuario))
                    return idUsuario;

                return null;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    await connection.CloseAsync();
            }
        }
    }
}
