using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IAuthRepository _repo;
        public AuthController(IAuthRepository repo)
        {
            _repo = repo;

        }

        
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto) {
            
            userForRegisterDto.Username = userForRegisterDto.Username.ToLower();

            /*Check for duplicate username*/
            if(await _repo.UserExists(userForRegisterDto.Username))
                return BadRequest("Username already exists");

            
            /*Create new username*/
            var newUser = new User {
                Username = userForRegisterDto.Username
            };

            /*Calls Register method to create/store non-Hash/Hash password in the database*/
            var createdUser = await _repo.Register(newUser, userForRegisterDto.Password);

            /*Returns status 201 (Created)*/
            return StatusCode(201);

        }

    }
}