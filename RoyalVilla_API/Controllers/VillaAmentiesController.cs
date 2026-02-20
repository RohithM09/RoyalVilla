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
    [Route("api/villa-amenities")]
    [ApiController]
    //[Authorize(Roles ="Customer,Admin")]
    public class VillaAmentiesController:ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _db;
        public VillaAmentiesController(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        [HttpGet]
        //[Authorize(Roles ="Admin")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<VillaAmentiesDTO>>),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        
        public async Task<ActionResult<ApiResponse<IEnumerable<VillaAmentiesDTO>>>> GetVillaAmenities()
        {
            var villas = await _db.VillaAmenities.ToListAsync();
            var response = ApiResponse<IEnumerable<VillaAmentiesDTO>>.Ok("Villas retrieved successfully", _mapper.Map<List<VillaAmentiesDTO>>(villas));
            return Ok(response);
        }
        [HttpGet("{id:int}")]
        //[AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<VillaAmentiesDTO>),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<ApiResponse<VillaAmentiesDTO>>> GetVillaAmenitiesById(int? id)
        {
            try
            {
                if(id<=0)
                {

                    return NotFound(ApiResponse<Object>.NotFound("VillaAmenities ID must be greater than 0"));
                }
                var villaAmenities = await _db.VillaAmenities.FirstOrDefaultAsync(i => i.Id == id);
                if (villaAmenities == null)
                {
                    return NotFound(ApiResponse<Object>.NotFound($"VillaAmenities with ID {id} was not found"));
                }
                return Ok(ApiResponse<VillaAmentiesDTO>.Ok("Record retrieved successfuly",_mapper.Map<VillaAmentiesDTO>(villaAmenities)));
            }
            catch(Exception ex) 
            {
                var errorResponse = ApiResponse<Object>.Error(500, $"An error occured while retrieving villa with ID {id}:{ex.Message}", ex.Message);
                return StatusCode(500,errorResponse);
            }
        }
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<VillaAmentiesDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<VillaAmentiesDTO>>> CreateVillaAmenities(VillaAmentiesCreateDTO villaAmentiesDTO)
        {
            try
            {
                if (villaAmentiesDTO == null)
                {
                    return NotFound(ApiResponse<object>.BadRequest("Villa Amenities data is required"));
                }
                var duplicateVilla = await _db.Villa.FirstOrDefaultAsync(i => i.Id == villaAmentiesDTO.VillaId);
                if (duplicateVilla != null)
                {
                    return Conflict(ApiResponse<object>.Conflict($"A villa with the id {villaAmentiesDTO.VillaId} already exists"));
                }
                VillaAmenities villaAmenities = _mapper.Map<VillaAmenities>(villaAmentiesDTO);
                villaAmenities.CreatedDate = DateTime.Now;
                await _db.VillaAmenities.AddAsync(villaAmenities);
                await _db.SaveChangesAsync();
                var response = ApiResponse<VillaAmentiesDTO>.CreatedAt("VillaAmenities created successfully", _mapper.Map<VillaAmentiesDTO>(villaAmenities));
                return CreatedAtAction(nameof(CreateVillaAmenities),new {id=villaAmenities.Id},response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<Object>.Error(500, $"An error occured while creating villa :{ex.Message}", ex.Message);
                return StatusCode(500, errorResponse);

            }

        }
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<VillaAmentiesDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<VillaAmentiesDTO>>>UpdateVilla(int id,VillaAmentiesUpdateDTO villaAmentiesDTO)
        {
            try
            {
                
                if (villaAmentiesDTO == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("VillaAmenities data is required"));
                }
                if (id != villaAmentiesDTO.Id)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("VillaAmenities ID in url doesn't match villa id in request body"));
                }
                var existingVilla = await _db.VillaAmenities.FirstOrDefaultAsync(x => x.Id == id);
                if (existingVilla == null)
                {
                    return NotFound(ApiResponse<Object>.NotFound($"VillaAmenities with {id} was not found"));
                }

                var duplicateVilla = await _db.VillaAmenities.FirstOrDefaultAsync(i=>i.Name.ToLower() == villaAmentiesDTO.Name.ToLower() && i.Id!=id);
                if(duplicateVilla != null)
                {
                    return Conflict(ApiResponse<object>.Conflict($"A villa with the name {villaAmentiesDTO.Name} already exists"));
                }
                _mapper.Map(villaAmentiesDTO,existingVilla);
                existingVilla.UpdatedDate = DateTime.Now;
                
                await _db.SaveChangesAsync();
                var response = ApiResponse<VillaAmentiesDTO>.Ok("VillaAmenities updated successfully", _mapper.Map<VillaAmentiesDTO>(villaAmentiesDTO));
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
                    return NotFound(ApiResponse<Object>.NotFound("VillaAmenities ID must be greater than 0"));
                }
                var existingVilla = await _db.VillaAmenities.FirstOrDefaultAsync(i => i.Id == id);

                if (existingVilla == null)
                {
                    return NotFound(ApiResponse<Object>.NotFound($"VillaAmenities with {id} not found"));
                }
                _db.VillaAmenities.Remove(existingVilla);
                await _db.SaveChangesAsync();

                return Ok(ApiResponse<object>.NoContent("VillaAmenities deleted Successfully"));
            }
            catch (Exception ex)
            {

                var errorResponse = ApiResponse<Object>.Error(500, $"An error occured while retrieving villa with ID {id}:{ex.Message}", ex.Message);
                return StatusCode(500, errorResponse);

            }

        }

    }
}
