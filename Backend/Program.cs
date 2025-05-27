using System;
using Backend.Data;
using Backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Registrar el DbContext con la cadena de conexion y logging de EF Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConexionPrincipal"))
           .LogTo(Console.WriteLine, LogLevel.Information)
);

//Registrar el servicio de procesamiento de imagenes
builder.Services.AddScoped<IImageProcessorService, ImageProcessorService>();

//Configurar CORS (opcional, para llamadas desde un front en otro origen)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

//Añadir controladores y ajustar JSON para ignorar ciclos de referencia
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//Pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// aplicar CORS antes de Authorization
app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
