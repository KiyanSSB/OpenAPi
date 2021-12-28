using api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using api.Models;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : Controller
    {
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IJwtAuthenticationService _authService;

        public AuthenticationController(ILogger<AuthenticationController> logger, IJwtAuthenticationService authenticationService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _authService = authenticationService;
        }

        [AllowAnonymous]
        [HttpGet]
        public object Get()
        {
            var responseObject = new { Status = "Running" };
            _logger.LogInformation($"Status: {responseObject.Status}");

            return responseObject;
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Authenticate([FromBody] usuario usuario)
        {
            var token = _authService.Authenticate(usuario.email, usuario.password);
            if(token == null)
            {
                return Unauthorized();
            }
            return Ok(token); ;
        }



    }
}
