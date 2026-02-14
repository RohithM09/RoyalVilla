using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.EntityFrameworkCore;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla_API.Models.DTO;
using System.Runtime.InteropServices;

namespace RoyalVilla_API.Controllers
{
    [Route("api/Villa")]
    [ApiController]
    [Authorize(Roles ="Customer,Admin")]
    public class VillaController:ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _db;
        public VillaController(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        [HttpGet]
        [Authorize(Roles ="Admin")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<VillaDTO>>),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        
        public async Task<ActionResult<ApiResponse<IEnumerable<VillaDTO>>>> GetVillas()
        {
            var villas = await _db.Villa.ToListAsync();
            var response = ApiResponse<IEnumerable<VillaDTO>>.Ok("Villas retrieved successfully", _mapper.Map<List<VillaDTO>>(villas));
            return Ok(response);
        }
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<VillaDTO>),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<ApiResponse<VillaDTO>>> GetVillaById(int? id)
        {
            try
            {
                if(id<=0)
                {

                    return NotFound(ApiResponse<Object>.NotFound("Villa ID must be greater than 0"));
                }
                var villa = await _db.Villa.FirstOrDefaultAsync(i => i.Id == id);
                if (villa == null)
                {
                    return NotFound(ApiResponse<Object>.NotFound($"Villa with ID {id} was not found"));
                }
                return Ok(ApiResponse<VillaDTO>.Ok("Record retrieved successfuly",_mapper.Map<VillaDTO>(villa)));
            }
            catch(Exception ex) 
            {
                var errorResponse = ApiResponse<Object>.Error(500, $"An error occured while retrieving villa with ID {id}:{ex.Message}", ex.Message);
                return StatusCode(500,errorResponse);
            }
        }
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<VillaDTO>>> CreateVilla(VillaCreateDTO villaDTO)
        {
            try
            {
                if (villaDTO == null)
                {
                    return NotFound(ApiResponse<object>.BadRequest("Villa data is required"));
                }
                var duplicateVilla = await _db.Villa.FirstOrDefaultAsync(i => i.Name.ToLower() == villaDTO.Name.ToLower());
                if (duplicateVilla != null)
                {
                    return Conflict(ApiResponse<object>.Conflict($"A villa with the name {villaDTO.Name} already exists"));
                }
                Villa villa = _mapper.Map<Villa>(villaDTO);
                await _db.Villa.AddAsync(villa);
                await _db.SaveChangesAsync();
                var response = ApiResponse<VillaDTO>.CreatedAt("Villa created successfully", _mapper.Map<VillaDTO>(villa));
                return CreatedAtAction(nameof(CreateVilla),new {id=villa.Id},response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<Object>.Error(500, $"An error occured while creating villa :{ex.Message}", ex.Message);
                return StatusCode(500, errorResponse);

            }

        }
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<VillaDTO>>>UpdateVilla(int id,VillaUpdateDTO villaDTO)
        {
            try
            {
                
                if (villaDTO == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Villa data is required"));
                }
                if (id != villaDTO.Id)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Villa ID in url doesn't match villa id in request body"));
                }
                var existingVilla = await _db.Villa.FirstOrDefaultAsync(x => x.Id == id);
                if (existingVilla == null)
                {
                    return NotFound(ApiResponse<Object>.NotFound($"Villa with {id} was not found"));
                }

                var duplicateVilla = await _db.Villa.FirstOrDefaultAsync(i=>i.Name.ToLower() == villaDTO.Name.ToLower() && i.Id!=id);
                if(duplicateVilla != null)
                {
                    return Conflict(ApiResponse<object>.Conflict($"A villa with the name {villaDTO.Name} already exists"));
                }
                _mapper.Map(villaDTO,existingVilla);
                existingVilla.UpdatedDate = DateTime.Now;
                
                await _db.SaveChangesAsync();
                var response = ApiResponse<VillaDTO>.Ok("Villa updated successfully", _mapper.Map<VillaDTO>(villaDTO));
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<Object>.Error(500, $"An error occured while retrieving villa with ID {id}:{ex.Message}", ex.Message);
                return StatusCode(500, errorResponse);

            }

        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteVilla(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return NotFound(ApiResponse<Object>.NotFound("Villa ID must be greater than 0"));
                }
                var existingVilla = await _db.Villa.FirstOrDefaultAsync(i => i.Id == id);

                if (existingVilla == null)
                {
                    return NotFound(ApiResponse<Object>.NotFound($"Villa with {id} not found"));
                }
                _db.Villa.Remove(existingVilla);
                await _db.SaveChangesAsync();

                return Ok(ApiResponse<object>.NoContent("Villa deleted Successfully"));
            }
            catch (Exception ex)
            {

                var errorResponse = ApiResponse<Object>.Error(500, $"An error occured while retrieving villa with ID {id}:{ex.Message}", ex.Message);
                return StatusCode(500, errorResponse);

            }

        }

    }
}
