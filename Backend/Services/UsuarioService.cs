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
            using var cmd = _context.Database.GetDbConnection().CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "dbo.sp_ValidarUsuario";

            cmd.Parameters.Add(new SqlParameter("@Username", SqlDbType.NVarChar, 50) { Value = username });
            cmd.Parameters.Add(new SqlParameter("@PasswordInput", SqlDbType.NVarChar, -1) { Value = passwordPlano });

            if (cmd.Connection.State != ConnectionState.Open)
                await cmd.Connection.OpenAsync();

            var resultado = await cmd.ExecuteScalarAsync();
            await cmd.Connection.CloseAsync();

            if (resultado != null && int.TryParse(resultado.ToString(), out int idUsuario))
                return idUsuario;

            return null;
        }
    }
}
