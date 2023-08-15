using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Modelos;
using Modelos.Dto;
using Modelos.Map;
using Modelos.Modelos;

namespace MagicVilla_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VillaController : ControllerBase
    {
        private readonly ILogger<VillaController> _logger;
        private readonly ApplicationDBContext _dbContext;
        private readonly IMapper _mapper;
        public VillaController(ILogger<VillaController> logger, ApplicationDBContext dbContext, IMapper mapper)
        {
            //Se inicializa
            _logger = logger;
            _dbContext = dbContext;
            _mapper = mapper;
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
            IEnumerable<Villa> villaList = await _dbContext.Villas.ToListAsync();

            return Ok(_mapper.Map<IEnumerable<VillaDto>>(villaList));
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

            return Ok(_mapper.Map<VillaDto>(villa));
        }

        [HttpPost]
        [Route("")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<VillaDto>> AddVilla([FromBody] VillaCreateDto createDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (await _dbContext.Villas.FirstOrDefaultAsync(x => x.Nombre.ToLower() == createDto.Nombre.ToLower()) != null)
            {
                ModelState.AddModelError("NombreExiste", "La villa con ese nombre ya existe");
                return BadRequest(ModelState);
            }

            if (createDto == null) return BadRequest(createDto);

            // Creamos un nuevo modelo en base a la villa
            Villa modelo = _mapper.Map<Villa>(createDto);

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

        public async Task<IActionResult> UpdateVillaById(int id, [FromBody] VillaUpdateDto updateDto)
        {
            if (updateDto == null || id != updateDto.Id) return BadRequest();

            Villa modelo = _mapper.Map<Villa>(updateDto);

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

            VillaUpdateDto villaDto = _mapper.Map<VillaUpdateDto>(villa);

            patchDto.ApplyTo(villaDto, ModelState);
            if (!ModelState.IsValid) return BadRequest(ModelState);

            Villa modelo = _mapper.Map<Villa>(villaDto);

            _dbContext.Villas.Update(modelo);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
