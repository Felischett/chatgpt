using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Models;
using ORM.Services;
using WebApi_Server.Models;

namespace WebApi_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DbManager _dbManager;
        private readonly IPasswordHasher<User> passwordHasher;

        public UsersController(DbManager dbManager, IPasswordHasher<User> passwordHasher)
        {
            _dbManager = dbManager;
            this.passwordHasher = passwordHasher;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _dbManager.findeBenutzerNachIdAsync(id);
            if (user == null) return NotFound(new { message = $"User mit ID {id} nicht gefunden." });
            return Ok(user);
        }

        [HttpGet("byKey/{key}")]
        public async Task<IActionResult> GetUserByKey(string key)
        {
            var user = await _dbManager.findeBenutzerNachSchluesselAsync(key);
            if (user == null) return NotFound(new { message = $"Kein User mit Key '{key}' gefunden." });
            return Ok(user);
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] User user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Password))
                return BadRequest(new { message = "Username und Passwort dürfen nicht leer sein." });

            var existingUser = await _dbManager.findeBenutzerNachNameAsync(user.Username);
            if (existingUser != null)
                return Conflict(new { message = "Dieser Username ist bereits vergeben." });

            var hashedPassword = passwordHasher.HashPassword(user, user.Password);
            user.Password = hashedPassword;

            if (string.IsNullOrWhiteSpace(user.Key))
                user.Key = ErzeugeUserKey(user.Username);

            var createdUser = await _dbManager.registriereBenutzerAsync(user);
            return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginDaten)
        {
            if (loginDaten == null || string.IsNullOrWhiteSpace(loginDaten.Email) || string.IsNullOrWhiteSpace(loginDaten.Password))
                return BadRequest(new { message = "E-Mail und Passwort sind Pflicht." });

            var user = await _dbManager.findeBenutzerNachEmailAsync(loginDaten.Email);

            if (user == null)
                return Unauthorized(new { message = "Benutzer existiert nicht." });

            var result = passwordHasher.VerifyHashedPassword(user, user.Password, loginDaten.Password);

            if (result == PasswordVerificationResult.Failed)
                return Unauthorized(new { message = "Passwort ist falsch." });

            return Ok(user);
        }

        private string ErzeugeUserKey(string username)
        {
            var random = new System.Random();
            int number = random.Next(1000, 9999);
            string prefix = username.Length >= 2 ? username.Substring(0, 2).ToUpper() : "US";
            return $"{prefix}{number}";
        }
    }
}
