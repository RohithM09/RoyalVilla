using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models.DTO;
using RoyalVilla_API.Services;

namespace RoyalVilla_API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<UserDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]

        public async Task<ActionResult<ApiResponse<UserDTO>>> Register([FromBody]RegisterationRequestDTO registerationRequestDTO)
        {
            try
            {
                if (registerationRequestDTO == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Registeration data is required"));
                }
                if (await _authService.IsEmailExistsAsync(registerationRequestDTO.Email))
                {
                    return Conflict(ApiResponse<object>.Conflict($"User with email {registerationRequestDTO.Email} already exists"));
                }
                var user = await _authService.RegisterAsync(registerationRequestDTO);
                if (user == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Registeration failed"));
                }

                var response = ApiResponse<UserDTO>.CreatedAt("User registered successfully", user);
                return CreatedAtAction(nameof(Register), response);
            }
            catch (Exception ex)
            {

                var errorResponse = ApiResponse<Object>.Error(500, $"An error occured registeration", ex.Message);
                return StatusCode(500, errorResponse);

            }
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
       

        public async Task<ActionResult<ApiResponse<UserDTO>>> Login([FromBody]LoginRequestDTO loginRequestDTO)
        {
            try
            {
                if (loginRequestDTO == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Login data is required"));
                }
                
                var loginResponse = await _authService.LoginAsync(loginRequestDTO);
                if (loginResponse == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("login failed"));
                }

                var response = ApiResponse<LoginResponseDTO>.Ok("login successfull", loginResponse);
                return Ok(response);
            }
            catch (Exception ex)
            {

                var errorResponse = ApiResponse<Object>.Error(500, $"An error occured during login", ex.Message);
                return StatusCode(500, errorResponse);

            }
        }
    }
}
