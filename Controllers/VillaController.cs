using Datos.Store;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Modelos.Dto;
using Modelos.Modelos;

namespace MagicVilla_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VillaController : ControllerBase
    {
        //Endponit tipo HTTP Get
        [HttpGet]   // Verbo HTTP
        [Route("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        // Tipo IEnumerable porque nos va retornar una lista de tipo <VillaDto>
        public ActionResult<IEnumerable<VillaDto>> GetVillas()
        {
            //Returnamos una lista de VillaDto de tipo <VillaDto> creamos un objeto con datos ficticios
            return Ok(VillaStore.villaList);
        }

        [HttpGet("id:int", Name = "GetVilla")]
        //[Route("id")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<VillaDto> GetVillaById(int id)
        {
            if (id == 0) return BadRequest();

            var villa = VillaStore.villaList.FirstOrDefault(x => x.Id == id);

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
            if (VillaStore.villaList.FirstOrDefault(x => x.Nombre.ToLower() == villaDto.Nombre.ToLower()) != null)
            {
                ModelState.AddModelError("NombreExiste", "La villa con ese nombre ya existe"); 
                    return BadRequest(ModelState);
            }

            if (villaDto == null) return BadRequest(villaDto);
            if (villaDto.Id > 0) return StatusCode(StatusCodes.Status500InternalServerError);

            // se asigna el id nuevo al villaDto.Id porque es datos no son persistente por no tener base de datos
            villaDto.Id = VillaStore.villaList.OrderByDescending(x => x.Id).FirstOrDefault().Id + 1;

            VillaStore.villaList.Add(villaDto);

            return CreatedAtRoute("GetVilla", new {id = villaDto.Id}, villaDto);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult DeleteVillaById(int id)
        {
            if(id == 0) return BadRequest();

            var villa = VillaStore.villaList.FirstOrDefault(x => x.Id == id);
            if(villa == null) return NotFound();

            VillaStore.villaList.Remove(villa);

            return NoContent();
        }
    }
}
