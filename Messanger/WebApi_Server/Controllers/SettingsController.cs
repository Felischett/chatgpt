using Microsoft.AspNetCore.Mvc;
using ORM.Services;

namespace WebApi_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private readonly DbManager _db;

        public SettingsController(DbManager db)
        {
            _db = db;
        }

        // GET /api/settings/5
        [HttpGet("{userId:int}")]
        public async Task<IActionResult> Get(int userId)
        {
            if (userId <= 0) return BadRequest(new { message = "userId fehlt." });

            var settings = await _db.holeUserSettingsAsync(userId);
            return Ok(settings);
        }

        // PUT /api/settings/5
        [HttpPut("{userId:int}")]
        public async Task<IActionResult> Put(int userId, [FromBody] UpdateSettingsRequest req)
        {
            if (userId <= 0) return BadRequest(new { message = "userId fehlt." });
            if (req == null) return BadRequest(new { message = "Body fehlt." });

            var saved = await _db.speichereUserSettingsAsync(userId, req.TargetLang);
            return Ok(saved);
        }

        public class UpdateSettingsRequest
        {
            public string TargetLang { get; set; } = "DE";
        }
    }
}