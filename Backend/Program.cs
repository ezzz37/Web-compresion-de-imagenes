using Backend.Data;
using Backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// 1) Configurar DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConexionPrincipal"))
           .LogTo(Console.WriteLine, LogLevel.Information)
);

// 2) Registrar servicios propios
builder.Services.AddScoped<IImageProcessorService, ImageProcessorService>();
builder.Services.AddScoped<UsuarioService>();

// 3) Leer JwtSettings desde appsettings.json
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings == null)
{
    throw new InvalidOperationException("La sección 'JwtSettings' no está configurada en appsettings.json");
}
var keyBytes = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

// 4) Configurar JWT Bearer Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
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

// 5) Registrar Authorization (requerido para [Authorize])
builder.Services.AddAuthorization();

// 6) Configurar CORS para permitir llamadas desde React en localhost:3000
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
    //.AllowCredentials() // Descomentar SOLO si usás cookies o withCredentials:true en Axios
    );
});

// 7) Agregar controladores + JSON
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// 8) Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 9) En desarrollo, habilitar Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 10) Habilitar CORS (antes de Auth)
app.UseCors("ReactPolicy");

// 11) Habilitar Autenticación y Autorización
app.UseAuthentication();
app.UseAuthorization();

// 12) Mapear controllers
app.MapControllers();

app.Run();
