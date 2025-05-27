using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Backend.Services;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComparacionesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IImageProcessorService _processor;

        public ComparacionesController(
            AppDbContext db,
            IImageProcessorService processor
        )
        {
            _db = db;
            _processor = processor;
        }

        // GET: api/Comparaciones
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ComparacionResponseDto>>> GetTodas()
        {
            var lista = await _db.Comparaciones
                .AsNoTracking()
                .Select(c => new ComparacionResponseDto
                {
                    IdComparacion = c.IdComparacion,
                    IdImagenOriginal = c.IdImagenOriginal,
                    IdImagenProcesada = c.IdImagenProcesada,
                    Mse = (double)c.MSE,
                    Psnr = (double)c.PSNR,
                    ImagenDiferenciasBase64 = Convert.ToBase64String(c.ImagenDiferencias),
                    FechaComparacion = c.FechaComparacion
                })
                .ToListAsync();

            return Ok(lista);
        }

        // GET: api/Comparaciones/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ComparacionResponseDto>> GetPorId(int id)
        {
            var c = await _db.Comparaciones
                             .AsNoTracking()
                             .FirstOrDefaultAsync(x => x.IdComparacion == id);

            if (c == null)
                return NotFound();

            var dto = new ComparacionResponseDto
            {
                IdComparacion = c.IdComparacion,
                IdImagenOriginal = c.IdImagenOriginal,
                IdImagenProcesada = c.IdImagenProcesada,
                Mse = (double)c.MSE,
                Psnr = (double)c.PSNR,
                ImagenDiferenciasBase64 = Convert.ToBase64String(c.ImagenDiferencias),
                FechaComparacion = c.FechaComparacion
            };

            return Ok(dto);
        }

        // POST: api/Comparaciones/comparar
        [HttpPost("comparar")]
        public async Task<ActionResult<ComparacionResponseDto>> Comparar([FromBody] CompararDto dto)
        {
            // 1) Cargo las imágenes
            var orig = await _db.Imagenes.FindAsync(dto.IdImagenOriginal);
            var proc = await _db.ImagenesProcesadas.FindAsync(dto.IdImagenProcesada);
            if (orig == null || proc == null)
                return NotFound("La imagen original o la procesada no existen.");

            // 2) Calculo métricas y diff (deconstrucción con tipos explícitos)
            (double mse, double psnr, byte[] diffBytes) =
                await _processor.CompararAsync(orig.DatosImagen, proc.DatosProcesados);

            // 3) Persisto la comparación
            var entidad = new Comparacion
            {
                IdImagenOriginal = dto.IdImagenOriginal,
                IdImagenProcesada = dto.IdImagenProcesada,
                MSE = (float)mse,
                PSNR = (float)psnr,
                ImagenDiferencias = diffBytes,
                FechaComparacion = DateTime.UtcNow
            };
            _db.Comparaciones.Add(entidad);
            await _db.SaveChangesAsync();

            // 4) Preparo DTO de respuesta
            var resp = new ComparacionResponseDto
            {
                IdComparacion = entidad.IdComparacion,
                IdImagenOriginal = entidad.IdImagenOriginal,
                IdImagenProcesada = entidad.IdImagenProcesada,
                Mse = mse,
                Psnr = psnr,
                ImagenDiferenciasBase64 = Convert.ToBase64String(diffBytes),
                FechaComparacion = entidad.FechaComparacion
            };

            // 5) Devuelvo 201 Created con ubicación al GET {id}
            return CreatedAtAction(
                nameof(GetPorId),
                new { id = entidad.IdComparacion },
                resp
            );
        }
    }
}
