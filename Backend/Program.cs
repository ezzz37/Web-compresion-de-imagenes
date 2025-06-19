using Backend.Data;
using Backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

// 1) Crear el builder
var builder = WebApplication.CreateBuilder(args);

// 2) Configurar DbContext con la cadena "DefaultConnection"
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .LogTo(Console.WriteLine, LogLevel.Information)
);

// 3) Registrar servicios propios
builder.Services.AddScoped<IImageProcessorService, ImageProcessorService>();
builder.Services.AddScoped<UsuarioService>();

// 4) Leer y configurar JWT desde appsettings.json
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings == null)
{
    throw new InvalidOperationException("La sección 'JwtSettings' no está configurada en appsettings.json");
}
var keyBytes = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

// 5) Configurar autenticación JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // solo para desarrollo en local
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// 6) Registrar autorización
builder.Services.AddAuthorization();

// 7) Configurar CORS para React en localhost:3000
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
    // .AllowCredentials() // Descomentar SOLO si usás cookies o withCredentials:true en Axios
    );
});

// 8) Configurar controladores y JSON
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// 9) Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 10) Habilitar Swagger en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 11) Habilitar HTTPS Redirection (puede mostrar warning si no está configurado en dev, pero no es grave)
app.UseHttpsRedirection();

// 12) Habilitar CORS antes de autenticación
app.UseCors("ReactPolicy");

// 13) Habilitar autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// 14) Mapear controladores
app.MapControllers();

app.Run();
