using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Modelos;
using Modelos.Dto;
using Modelos.Modelos;

namespace MagicVilla_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VillaController : ControllerBase
    {
        private readonly ILogger<VillaController> _logger;
        private readonly ApplicationDBContext _dbContext;
        public VillaController(ILogger<VillaController> logger, ApplicationDBContext dbContext)
        {
            //Se inicializa
            _logger = logger;
            _dbContext = dbContext;
        }

        //Endponit tipo HTTP Get
        [HttpGet]   // Verbo HTTP
        [Route("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        // Tipo IEnumerable porque nos va retornar una lista de tipo <VillaDto>
        public async Task<ActionResult<IEnumerable<VillaDto>>> GetVillas()
        {
            // Logger
            _logger.LogInformation("Obtener las villas");
            return Ok(await _dbContext.Villas.ToListAsync());
        }

        [HttpGet("id:int", Name = "GetVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VillaDto>> GetVillaById(int id)
        {
            if (id == 0)
            {
                _logger.LogError("Error al traer Villa con Id :" + id);
                return BadRequest();
            }

            var villa = await _dbContext.Villas.FirstOrDefaultAsync(x => x.Id == id);

            if (villa == null) return NotFound();

            return Ok(villa);
        }

        [HttpPost]
        [Route("")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<VillaDto>> AddVilla([FromBody] VillaCreateDto villaDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (await _dbContext.Villas.FirstOrDefaultAsync(x => x.Nombre.ToLower() == villaDto.Nombre.ToLower()) != null)
            {
                ModelState.AddModelError("NombreExiste", "La villa con ese nombre ya existe");
                return BadRequest(ModelState);
            }

            if (villaDto == null) return BadRequest(villaDto);

            // Creamos un nuevo modelo en base a la villa
            Villa modelo = new()
            {
                Nombre = villaDto.Nombre,
                Detalle = villaDto.Detalle,
                ImagenUrl = villaDto.ImagenUrl,
                Ocupantes = villaDto.Ocupantes,
                MetrosCuadrados = villaDto.MetrosCuadraros,
                Tarifa = villaDto.Tarifa,
                Amenidad = villaDto.Amenidad
            };
            await _dbContext.AddAsync(modelo);
            await _dbContext.SaveChangesAsync();

            return CreatedAtRoute("GetVilla", new { id = modelo.Id }, modelo);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteVillaById(int id)
        {
            if (id == 0) return BadRequest();

            var villa = await _dbContext.Villas.FirstOrDefaultAsync(x => x.Id == id);
            if (villa == null) return NotFound();

            _dbContext.Villas.Remove(villa);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]

        public async Task<IActionResult> UpdateVillaById(int id, [FromBody] VillaUpdateDto villaDto)
        {
            if (villaDto == null || id != villaDto.Id) return BadRequest();

            Villa modelo = new()
            {
                Id = villaDto.Id,
                Nombre = villaDto.Nombre,
                Detalle = villaDto.Detalle,
                ImagenUrl = villaDto.ImagenUrl,
                Ocupantes = villaDto.Ocupantes,
                MetrosCuadrados = villaDto.MetrosCuadraros,
                Tarifa = villaDto.Tarifa,
                Amenidad = villaDto.Amenidad
            };

            _dbContext.Villas.Update(modelo);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("{id:int}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]

        public async Task<IActionResult> UpdatePartialVillaById(int id, JsonPatchDocument<VillaUpdateDto> patchDto)
        {
            if (patchDto == null || id == 0) return BadRequest();

            var villa = await _dbContext.Villas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

            if (villa == null) return BadRequest();

            VillaUpdateDto villaDto = new()
            {
                Id = villa.Id,
                Nombre = villa.Nombre,
                Detalle = villa.Detalle,
                ImagenUrl = villa.ImagenUrl,
                Ocupantes = villa.Ocupantes,
                Tarifa = villa.Tarifa,
                MetrosCuadraros = villa.MetrosCuadrados,
                Amenidad = villa.Amenidad
            };

            patchDto.ApplyTo(villaDto, ModelState);
            if (!ModelState.IsValid) return BadRequest(ModelState);

            Villa modelo = new()
            {
                Id = villaDto.Id,
                Nombre = villaDto.Nombre,
                Detalle = villaDto.Detalle,
                ImagenUrl = villaDto.ImagenUrl,
                Ocupantes = villaDto.Ocupantes,
                MetrosCuadrados = villaDto.MetrosCuadraros,
                Tarifa = villaDto.Tarifa,
                Amenidad = villaDto.Amenidad
            };
            _dbContext.Villas.Update(modelo);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
