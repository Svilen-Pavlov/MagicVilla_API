using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Logging;
using MagicVilla_VillaAPI.Models.DTO;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace MagicVilla_VillaAPI.Controllers
{
    [Route("api/VillaAPI")]
    [ApiController]
    public class VillaAPIController : ControllerBase
    {
        private readonly ILogging _logger;

        public VillaAPIController(ILogging logger)
        {
            this._logger = logger;
        }

        [HttpGet]
        public ActionResult<IEnumerable<VillaDTO>> GetVillas() // All villas
        {
            _logger.Log("Getting all Villas","");
            return Ok(VillaStore.villaList);
        }
        
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id:int}", Name = "GetVilla")]
        public ActionResult<VillaDTO> GetVilla(int id)
        {
            if (id <= 0)
            {
                _logger.Log("Get Villa Error Id " + id,"error");
                return BadRequest();
            }

            var villa = VillaStore.villaList.FirstOrDefault(x => x.Id == id);
            if (villa == null)
                return NotFound();

            return Ok(villa);
        } // Individual Villa

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        public ActionResult<VillaDTO> CreateVilla([FromBody] VillaDTO villaDTO)
        {
            //if (!ModelState.IsValid)
            //    return BadRequest(ModelState);
           
            if (VillaStore.villaList.FirstOrDefault(x => x.Name.ToLower() == villaDTO.Name.ToLower()) != null)
            {
                ModelState.AddModelError("CustomError", "Villa already Exists!");
                return BadRequest(ModelState);
            }

            if (villaDTO == null)
                return BadRequest(villaDTO);
            if (villaDTO.Id > 0)
                return StatusCode(StatusCodes.Status500InternalServerError);

            villaDTO.Id = VillaStore.villaList.OrderByDescending(x => x.Id).First().Id + 1;
            VillaStore.villaList.Add(villaDTO);

            return CreatedAtRoute("GetVilla", new { id = villaDTO.Id }, (villaDTO));
        }

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        [HttpDelete("{id:int}", Name = "DeleteVilla")]
        public ActionResult DeleteVilla(int id)
        {
            if (id <= 0)
                return BadRequest();
            var villa = VillaStore.villaList.FirstOrDefault(x => x.Id == id);
            if (villa == null)
            {
                return NotFound();
            }
            VillaStore.villaList.Remove(villa);
            return NoContent();
        }

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        [HttpPut("{id:int}", Name = "UpdateVilla")]
        public ActionResult<VillaDTO> UpdateVilla(int id, [FromBody] VillaDTO villaDTO)
        {
            if (id <= 0 || villaDTO.Id != id)
                return BadRequest();

            var villa = VillaStore.villaList.FirstOrDefault(x => x.Id == id);
            if (villa == null)
            {
                return NotFound();
            }
            villa.Id = villaDTO.Id;
            villa.Name = villaDTO.Name;
            villa.Sqft = villaDTO.Sqft;
            villa.Occupancy = villaDTO.Occupancy;

            return NoContent();
        }

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPatch("{id:int}", Name = "UpdatePartialVilla")] //{"op": "replace", "path": /name", "value": "Chocolate Digestive"}
        public IActionResult UpdatePartialVilla(int id, JsonPatchDocument<VillaDTO> patchDTO) //currently does not work
        {
            if (id <= 0 || patchDTO == null)
                return BadRequest();

            var villa = VillaStore.villaList.FirstOrDefault(x => x.Id == id);
            if (villa == null)
            {
                return NotFound();
            }
            patchDTO.ApplyTo(villa, ModelState);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return NoContent();
        }


    }
}
