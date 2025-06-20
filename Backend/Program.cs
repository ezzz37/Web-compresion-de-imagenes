using Backend.Data;
using Backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1) DbContext
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
       .LogTo(Console.WriteLine, LogLevel.Information)
);

// 2) Servicios
builder.Services.AddScoped<IImageProcessorService, ImageProcessorService>();
builder.Services.AddScoped<UsuarioService>();

// 3) JWT
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration
                         .GetSection("JwtSettings")
                         .Get<JwtSettings>()
                 ?? throw new InvalidOperationException("Falta secci�n JwtSettings");
var keyBytes = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;  // en producci�n siempre HTTPS
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

// 4) CORS: Or�genes permitidos para el front React
builder.Services.AddCors(opts =>
{
    opts.AddPolicy("ReactPolicy", policy =>
    {
        policy
          .WithOrigins(
              "http://localhost:3000",                          // desarrollo local
              "https://web-compresion-de-imagenes.vercel.app"   // front desplegado en Vercel
          )
          .AllowAnyHeader()
          .AllowAnyMethod();
    });
});

// 5) Controladores & JSON
builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles
    );

// 6) Swagger (opcional)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- pipeline ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Si usas endpoint routing expl�cito:
app.UseRouting();

//  CORS debe ir antes de Authentication/Authorization
app.UseCors("ReactPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
