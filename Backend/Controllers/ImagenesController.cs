using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.DTOs;
using Backend.Models;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImagenesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ImagenesController(AppDbContext db) => _db = db;

        // GET: api/Imagenes
        [HttpGet]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<ImagenDto>>> GetTodas()
        {
            var lista = await _db.Imagenes.ToListAsync();

            var resultado = lista.Select(i => new ImagenDto
            {
                IdImagen = i.IdImagen,
                Nombre = i.Nombre,
                DatosImagenBase64 = Convert.ToBase64String(i.DatosImagen)
            });

            return Ok(resultado);
        }

        // GET: api/Imagenes/5
        [HttpGet("{id}")]
        [Produces("application/json")]
        public async Task<ActionResult<Imagen>> GetPorId(int id)
        {
            var img = await _db.Imagenes
                               .Include(i => i.ImagenesProcesadas)
                               .Include(i => i.ComparacionesOriginal)
                               .FirstOrDefaultAsync(i => i.IdImagen == id);
            if (img == null) return NotFound();
            return Ok(img);
        }

        // POST: api/Imagenes
        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<ActionResult<Imagen>> Crear([FromBody] Imagen modelo)
        {
            modelo.FechaCarga = DateTime.UtcNow;
            _db.Imagenes.Add(modelo);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPorId), new { id = modelo.IdImagen }, modelo);
        }

        // POST: api/Imagenes/upload
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [Produces("application/json")]
        public async Task<ActionResult<Imagen>> Upload([FromForm] ImagenUploadDto dto)
        {
            byte[] datos;
            using (var ms = new MemoryStream())
            {
                await dto.Archivo.CopyToAsync(ms);
                datos = ms.ToArray();
            }

            int ancho = 0, alto = 0;
            try
            {
                using var img = Image.FromStream(new MemoryStream(datos));
                ancho = img.Width;
                alto = img.Height;
            }
            catch
            {
                // Si falla, deja 0
            }

            var entidad = new Imagen
            {
                Nombre = dto.Nombre,
                DatosImagen = datos,
                AnchoOriginal = ancho,
                AltoOriginal = alto,
                FechaCarga = DateTime.UtcNow
            };

            _db.Imagenes.Add(entidad);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPorId), new { id = entidad.IdImagen }, entidad);
        }

        // PUT: api/Imagenes/5
        [HttpPut("{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] Imagen modelo)
        {
            if (id != modelo.IdImagen) return BadRequest();
            _db.Entry(modelo).State = EntityState.Modified;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_db.Imagenes.Any(e => e.IdImagen == id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Imagenes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Borrar(int id)
        {
            var img = await _db.Imagenes.FindAsync(id);
            if (img == null) return NotFound();

            _db.Imagenes.Remove(img);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
