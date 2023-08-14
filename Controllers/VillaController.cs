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
        public ActionResult<IEnumerable<VillaDto>> GetVillas()
        {
            // Logger
            _logger.LogInformation("Obtener las villas");
            return Ok(_dbContext.Villas.ToList());
        }

        [HttpGet("id:int", Name = "GetVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<VillaDto> GetVillaById(int id)
        {
            if (id == 0)
            {
                _logger.LogError("Error al traer Villa con Id :" + id);
                return BadRequest();
            }

            var villa = _dbContext.Villas.FirstOrDefault(x => x.Id == id);

            if (villa == null) return NotFound();

            return Ok(villa);
        }

        [HttpPost]
        [Route("")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<VillaDto> AddVilla([FromBody] VillaDto villaDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (_dbContext.Villas.FirstOrDefault(x => x.Nombre.ToLower() == villaDto.Nombre.ToLower()) != null)
            {
                ModelState.AddModelError("NombreExiste", "La villa con ese nombre ya existe");
                return BadRequest(ModelState);
            }

            if (villaDto == null) return BadRequest(villaDto);
            if (villaDto.Id > 0) return StatusCode(StatusCodes.Status500InternalServerError);

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
            _dbContext.Add(modelo);
            _dbContext.SaveChanges();

            return CreatedAtRoute("GetVilla", new { id = villaDto.Id }, villaDto);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult DeleteVillaById(int id)
        {
            if (id == 0) return BadRequest();

            var villa = _dbContext.Villas.FirstOrDefault(x => x.Id == id);
            if (villa == null) return NotFound();

            _dbContext.Villas.Remove(villa);
            _dbContext.SaveChanges();

            return NoContent();
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]

        public IActionResult UpdateVillaById(int id, [FromBody] VillaDto villaDto)
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
            _dbContext.SaveChanges();

            return NoContent();
        }

        [HttpPatch("{id:int}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]

        public IActionResult UpdatePartialVillaById(int id, JsonPatchDocument<VillaDto> patchDto)
        {
            if (patchDto == null || id == 0) return BadRequest();

            var villa = _dbContext.Villas.AsNoTracking().FirstOrDefault(x => x.Id == id);

            if (villa == null) return BadRequest();

            VillaDto villaDto = new()
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
            _dbContext.SaveChanges();

            return NoContent();
        }
    }
}
