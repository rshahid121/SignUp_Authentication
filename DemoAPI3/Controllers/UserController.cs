using DemoAPI3.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Dapper;
using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;
using System.Text;
using System;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;

namespace DemoAPI3.Controllers
{
    //[Route("api/[controller]")]
    [Route("users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        //private string _connectionString = ""; 
        //private readonly IConfiguration _configuration;
        private IUserService _usersService;

        public UserController(IConfiguration configuration, IUserService usersService)
        {
            //_configuration = configuration;
            //_connectionString = _configuration.GetConnectionString("Percept");
            _usersService = usersService;
        }

        [HttpPost("signup")]
        public IActionResult SignUp(User user)
        {
            if (_usersService.SignUp(user))
                return Ok("User created successfully.");
            else
                return BadRequest("User created successfully.");
        }

        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody] UserCredentials credentials)
        {
            // Authenticate the user using the provided credentials
            var user = _usersService.AuthenticateUser(credentials.Username, credentials.Password);

            if (user != null)
            {
                // Generate JWT token
                var token = _usersService.GenerateJwtToken(user);

                _usersService.SaveLoginDetails(user.UserID, credentials.IPAddress, credentials.Device, credentials.Browser, DateTime.UtcNow, token);

                // Check if it's the user's first successful login
                if (_usersService.IsFirstSuccessfulLogin(user.UserID))
                {
                    // Add gift balance to the user's account
                    _usersService.AddGiftBalance(user.UserID);
                }
                
                var response = new
                {
                    firstname = user.FirstName,
                    lastname = user.LastName,
                    token
                };

                return Ok(response);
            }
            else
            {
                return Unauthorized();
            }
        }

        [Authorize]
        [HttpPost("balance")]        
        public ActionResult<BalanceResponse> GetBalance(BalanceRequest request)
        {
            return _usersService.GetBalance(request);
                       
        }        
    }
}
