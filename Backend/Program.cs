using System;
using Backend.Data;
using Backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Registrar el DbContext con la cadena de conexión y logging de EF Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConexionPrincipal"))
           .LogTo(Console.WriteLine, LogLevel.Information)
);

// Registrar el servicio de procesamiento de imágenes
builder.Services.AddScoped<IImageProcessorService, ImageProcessorService>();

// Configurar CORS para permitir peticiones desde el frontend React (puerto 3000)
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
        policy.WithOrigins("http://localhost:3000")  // Cambia aquí si React corre en otro puerto
              .AllowAnyMethod()
              .AllowAnyHeader()
    );
});

// Añadir controladores y ajustar JSON para ignorar ciclos de referencia
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Aplicar CORS con la política nombrada antes de Authorization y MapControllers
app.UseCors("ReactPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();
