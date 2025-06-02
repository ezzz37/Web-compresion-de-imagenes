using System;
using Backend.Data;
using Backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// 1. Registrar el DbContext con la cadena de conexión y habilitar logging de EF Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConexionPrincipal"))
           .LogTo(Console.WriteLine, LogLevel.Information)
);

// 2. Registrar el servicio de procesamiento de imágenes (interfaz + implementación)
builder.Services.AddScoped<IImageProcessorService, ImageProcessorService>();

// 3. Configurar CORS para permitir peticiones desde el frontend React (por ejemplo, http://localhost:3000)
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
        policy.WithOrigins("http://localhost:3000") // Ajusta el origen si React corre en otro puerto or dominio
              .AllowAnyMethod()
              .AllowAnyHeader()
    );
});

// 4. Añadir controladores y ajustar JSON para ignorar ciclos de referencia
builder.Services.AddControllers()
                .AddJsonOptions(opts =>
                {
                    opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                });

// 5. Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 6. Pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 7. Aplicar CORS antes de cualquier middleware que maneje peticiones
app.UseCors("ReactPolicy");

app.UseAuthorization();

// 8. Mapear controladores
app.MapControllers();

app.Run();
