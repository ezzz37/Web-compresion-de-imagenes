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
        // Devuelve todas las comparaciones almacenadas
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
                    ImagenDiferenciasBase64 =
                        c.ImagenDiferencias == null
                            ? null
                            : Convert.ToBase64String(c.ImagenDiferencias),
                    FechaComparacion = c.FechaComparacion
                })
                .ToListAsync();

            return Ok(lista);
        }

        // GET: api/Comparaciones/{id}
        // Devuelve una comparación por su IdComparacion
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ComparacionResponseDto>> GetPorId(int id)
        {
            var entidad = await _db.Comparaciones
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdComparacion == id);

            if (entidad == null)
                return NotFound(new { mensaje = "No se encontró ninguna comparación con ese ID." });

            var dto = new ComparacionResponseDto
            {
                IdComparacion = entidad.IdComparacion,
                IdImagenOriginal = entidad.IdImagenOriginal,
                IdImagenProcesada = entidad.IdImagenProcesada,
                Mse = (double)entidad.MSE,
                Psnr = (double)entidad.PSNR,
                ImagenDiferenciasBase64 = Convert.ToBase64String(entidad.ImagenDiferencias),
                FechaComparacion = entidad.FechaComparacion
            };

            return Ok(dto);
        }

        // GET: api/Comparaciones/porImagenes?imgOriginalId=2&imgProcesadaId=3
        // Devuelve la comparación para un par específico de IDs de imágenes
        [HttpGet("porImagenes")]
        public async Task<ActionResult<ComparacionResponseDto>> GetPorIds(
            [FromQuery] int imgOriginalId,
            [FromQuery] int imgProcesadaId)
        {
            if (imgOriginalId <= 0 || imgProcesadaId <= 0)
                return BadRequest(new { mensaje = "IDs de imágenes inválidos." });

            var entidad = await _db.Comparaciones
                .AsNoTracking()
                .Where(x => x.IdImagenOriginal == imgOriginalId && x.IdImagenProcesada == imgProcesadaId)
                .Select(x => new ComparacionResponseDto
                {
                    IdComparacion = x.IdComparacion,
                    IdImagenOriginal = x.IdImagenOriginal,
                    IdImagenProcesada = x.IdImagenProcesada,
                    Mse = (double)x.MSE,
                    Psnr = (double)x.PSNR,
                    ImagenDiferenciasBase64 = Convert.ToBase64String(x.ImagenDiferencias),
                    FechaComparacion = x.FechaComparacion
                })
                .FirstOrDefaultAsync();

            if (entidad == null)
                return NotFound(new { mensaje = "No se encontró una comparación para esos IDs de imágenes." });

            return Ok(entidad);
        }

        // POST: api/Comparaciones/comparar
        // Recibe un DTO con IDs de imagen original y procesada, calcula métricas,
        // guarda la diferencia y devuelve la nueva entidad
        [HttpPost("comparar")]
        public async Task<ActionResult<ComparacionResponseDto>> Comparar([FromBody] CompararDto dto)
        {
            if (dto == null
                || dto.IdImagenOriginal <= 0
                || dto.IdImagenProcesada <= 0)
            {
                return BadRequest(new { mensaje = "Los IDs de imagen enviados son inválidos." });
            }

            // Verificar que ambas imágenes existen
            var orig = await _db.Imagenes.FindAsync(dto.IdImagenOriginal);
            var proc = await _db.ImagenesProcesadas.FindAsync(dto.IdImagenProcesada);

            if (orig == null || proc == null)
                return NotFound(new { mensaje = "La imagen original o la procesada no existen." });

            // Calcular MSE, PSNR y la imagen de diferencias en bytes
            var (mse, psnr, diffBytes) =
                await _processor.CompararAsync(orig.DatosImagen, proc.DatosProcesados);

            // Persistir la entidad Comparacion
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

            // Preparar DTO de respuesta
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

            // Devolver CreatedAtAction para que el frontend conozca la URL /api/Comparaciones/{id}
            return CreatedAtAction(
                nameof(GetPorId),
                new { id = entidad.IdComparacion },
                resp
            );
        }
    }
}