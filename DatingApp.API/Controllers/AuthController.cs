using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{   
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        public AuthController(IAuthRepository repo, IConfiguration config)
        {
            _config = config;
            _repo = repo;

        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {

            userForRegisterDto.Username = userForRegisterDto.Username.ToLower();

            /*Check for duplicate username*/
            if (await _repo.UserExists(userForRegisterDto.Username))
                return BadRequest("Username already exists");


            /*Create new username*/
            var newUser = new User
            {
                Username = userForRegisterDto.Username
            };

            /*Calls Register method to create/store non-Hash/Hash password in the database*/
            var createdUser = await _repo.Register(newUser, userForRegisterDto.Password);

            /*Returns status 201 (Created)*/
            return StatusCode(201);

        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            /*check if user is valid*/
            var userFromRepo = await _repo.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);

            
            if (userForLoginDto == null) return Unauthorized();

            /*create claims */
            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.Username)
            };
            
            /*create a key*/
            /*"AppSettings:Token" is created within the appsettings.json file*/
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));

            /*key will be encrypted with a hashing algorithm*/
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            /*token will be created. passing the claims as the subject with an expiry date of 1 day*/
            /*Encrypted singing credential created above will also be passed in*/
            var tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds

            };

            /*initialize Jwt token handler*/
            var tokenHandler = new JwtSecurityTokenHandler();
            
            /*create a token*/
            var token = tokenHandler.CreateToken(tokenDescriptor);

            /*return the created token*/
            return Ok(new {
                token = tokenHandler.WriteToken(token)
            });

        }
    }
}