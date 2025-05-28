using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using Backend.Services;
using Backend.DTOs;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImagenesProcesadasController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IImageProcessorService _processor;

        public ImagenesProcesadasController(
            AppDbContext db,
            IImageProcessorService processor
        )
        {
            _db = db;
            _processor = processor;
        }

        // GET: api/ImagenesProcesadas
        [HttpGet]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<ImagenProcesada>>> GetTodas()
        {
            var lista = await _db.ImagenesProcesadas
                                 .Include(ip => ip.ImagenOriginal)
                                 .Include(ip => ip.AlgoritmoCompresion)
                                 .Include(ip => ip.Comparaciones)
                                 .ToListAsync();
            return Ok(lista);
        }

        // GET: api/ImagenesProcesadas/5
        [HttpGet("{id}")]
        [Produces("application/json")]
        public async Task<ActionResult<ImagenProcesada>> GetPorId(int id)
        {
            var item = await _db.ImagenesProcesadas
                                .Include(ip => ip.ImagenOriginal)
                                .Include(ip => ip.AlgoritmoCompresion)
                                .Include(ip => ip.Comparaciones)
                                .FirstOrDefaultAsync(ip => ip.IdImagenProcesada == id);
            if (item == null)
                return NotFound();
            return Ok(item);
        }

        // POST: api/ImagenesProcesadas/procesar/{idImagen}
        [HttpPost("procesar/{idImagen}")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<ActionResult<ImagenProcesada>> Procesar(
            int idImagen,
            [FromBody] ProcesarDto dto
        )
        {
            // Cargar la imagen original
            var orig = await _db.Imagenes.FindAsync(idImagen);
            if (orig == null)
                return NotFound($"No existe imagen con Id {idImagen}");

            // Validaciones basicas
            if (dto.AnchoResolucion <= 0 || dto.AltoResolucion <= 0)
                return BadRequest("AnchoResolucion y AltoResolucion deben ser mayores que cero.");

            if (dto.ProfundidadBits is not (1 or 8 or 24))
                return BadRequest("ProfundidadBits debe ser 1, 8 o 24.");

            // Validar algoritmo de compresión
            var formato = dto.Algoritmo?.ToUpperInvariant();
            if (formato != "JPEG" && formato != "PNG")
                return BadRequest("Algoritmo desconocido. Use 'JPEG' o 'PNG'.");

            // Validar que el IdAlgoritmoCompresion exista
            var existeAlgo = await _db.AlgoritmosCompresion
                .AnyAsync(a => a.IdAlgoritmoCompresion == dto.IdAlgoritmoCompresion);
            if (!existeAlgo)
                return BadRequest($"No existe AlgoritmoCompresion con Id {dto.IdAlgoritmoCompresion}.");

            // Procesamiento
            var muestreado = _processor.Muestrear(
                orig.DatosImagen, dto.AnchoResolucion, dto.AltoResolucion
            );
            var cuantizado = _processor.Cuantizar(
                muestreado, dto.ProfundidadBits
            );
            var comprimido = _processor.Comprimir(
                cuantizado, formato
            );

            //Persistir resultado
            var procEnt = new ImagenProcesada
            {
                IdImagenOriginal = idImagen,
                AnchoResolucion = dto.AnchoResolucion,
                AltoResolucion = dto.AltoResolucion,
                ProfundidadBits = dto.ProfundidadBits,
                IdAlgoritmoCompresion = dto.IdAlgoritmoCompresion,
                DatosProcesados = comprimido,
                FechaProcesamiento = DateTime.UtcNow
            };

            _db.ImagenesProcesadas.Add(procEnt);
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return BadRequest("No se pudo guardar la imagen procesada. Verifica los datos enviados.");
            }

            return CreatedAtAction(nameof(GetPorId),
                new { id = procEnt.IdImagenProcesada }, procEnt);
        }

        // PUT: api/ImagenesProcesadas/5
        [HttpPut("{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ImagenProcesada modelo)
        {
            if (id != modelo.IdImagenProcesada)
                return BadRequest();

            _db.Entry(modelo).State = EntityState.Modified;
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _db.ImagenesProcesadas.AnyAsync(e => e.IdImagenProcesada == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/ImagenesProcesadas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Borrar(int id)
        {
            var item = await _db.ImagenesProcesadas.FindAsync(id);
            if (item == null)
                return NotFound();

            _db.ImagenesProcesadas.Remove(item);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
