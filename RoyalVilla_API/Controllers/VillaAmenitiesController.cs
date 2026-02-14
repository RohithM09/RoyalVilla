using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla_API.Models.DTO;

namespace RoyalVilla_API.Controllers
{
    [ApiController]
    [Route("api/VillaAmenities")]
    public class VillaAmenitiesController:ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _dbContext; 
        public VillaAmenitiesController(IMapper mapper,ApplicationDbContext dbcontext)
        {
            _dbContext = dbcontext;
            _mapper = mapper;   
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VillaAmenitiesDTO>>> GetVillaAmenitites()
        {
            return Ok(await _dbContext.VillaAmenities.ToListAsync());
        }
        [HttpGet("{id:int}")]
        public async Task<ActionResult<VillaAmenitiesDTO>> GetVillaAmenity(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest($"villa with {id} not found");
                }
                var villaAmenity = await _dbContext.VillaAmenities.FirstOrDefaultAsync(i => i.Id == id);
                VillaAmenitiesDTO villaAmenitiesDTO = _mapper.Map<VillaAmenitiesDTO>(villaAmenity);

                return Ok(villaAmenitiesDTO);


            }
            catch (Exception ex)
            {
                return StatusCodes(StatusCode.Status500InternalServerError,
                    $"exception occured while retrieving record with {ex.Message}");

            }
        }

        
    }
}
