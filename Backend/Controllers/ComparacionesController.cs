using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using Backend.DTOs;
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
        public async Task<ActionResult<IEnumerable<Comparacion>>> GetTodas()
        {
            var lista = await _db.Comparaciones
                                 .Include(c => c.ImagenOriginal)
                                 .Include(c => c.ImagenProcesada)
                                 .ToListAsync();
            return Ok(lista);
        }

        // GET: api/Comparaciones/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Comparacion>> GetPorId(int id)
        {
            var comp = await _db.Comparaciones
                                .Include(c => c.ImagenOriginal)
                                .Include(c => c.ImagenProcesada)
                                .FirstOrDefaultAsync(c => c.IdComparacion == id);

            if (comp == null) return NotFound();
            return Ok(comp);
        }

        // POST: api/Comparaciones
        [HttpPost]
        public async Task<ActionResult<Comparacion>> Crear(Comparacion modelo)
        {
            _db.Comparaciones.Add(modelo);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPorId),
                new { id = modelo.IdComparacion }, modelo);
        }

        // POST: api/Comparaciones/comparar
        [HttpPost("comparar")]
        public async Task<ActionResult<Comparacion>> Comparar([FromBody] CompararDto dto)
        {
            // Cargar entidades
            var orig = await _db.Imagenes.FindAsync(dto.IdImagenOriginal);
            var proc = await _db.ImagenesProcesadas.FindAsync(dto.IdImagenProcesada);
            if (orig == null || proc == null)
                return NotFound("La imagen original o la procesada no existen.");

            //Calcular metricas y diff
            var (mse, psnr, diffBytes) = await _processor.CompararAsync(
                orig.DatosImagen,
                proc.DatosProcesados
            );

            //Crear y guardar comparacion
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

            return CreatedAtAction(nameof(GetPorId),
                new { id = entidad.IdComparacion }, entidad);
        }

        // PUT: api/Comparaciones/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(int id, Comparacion modelo)
        {
            if (id != modelo.IdComparacion) return BadRequest();
            _db.Entry(modelo).State = EntityState.Modified;
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_db.Comparaciones.Any(e => e.IdComparacion == id))
                    return NotFound();
                throw;
            }
            return NoContent();
        }

        // DELETE: api/Comparaciones/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Borrar(int id)
        {
            var comp = await _db.Comparaciones.FindAsync(id);
            if (comp == null) return NotFound();
            _db.Comparaciones.Remove(comp);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
