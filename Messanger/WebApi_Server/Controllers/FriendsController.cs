using Microsoft.AspNetCore.Mvc;
using Models;
using ORM.Services;

namespace WebApi_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FriendsController : ControllerBase
    {
        private readonly DbManager dbManager;

        public FriendsController(DbManager dbManager)
        {
            this.dbManager = dbManager;
        }

        // GET: /api/Friends/{userId}/accepted
        [HttpGet("{userId}/accepted")]
        public async Task<ActionResult<List<User>>> holeFreunde(int userId)
        {
            var users = await dbManager.holeFreundeAsync(userId);

            foreach (var u in users)
            {
                u.Password = "";
            }

            return Ok(users);
        }

        // GET: /api/Friends/{userId}/requests
        [HttpGet("{userId}/requests")]
        public async Task<ActionResult<List<User>>> holeAnfragen(int userId)
        {
            var users = await dbManager.holeAnfragenAsync(userId);

            foreach (var u in users)
            {
                u.Password = "";
            }

            return Ok(users);
        }

        public class FriendRequestDto
        {
            public int UserId { get; set; }
            public string FriendKey { get; set; } = "";
        }

        // POST: /api/Friends/request
        [HttpPost("request")]
        public async Task<IActionResult> sendeAnfrage([FromBody] FriendRequestDto dto)
        {
            await dbManager.sendeFreundschaftsanfrageAsync(dto.UserId, dto.FriendKey);
            return Ok();
        }

        public class FriendActionDto
        {
            public int UserId { get; set; }
            public int FriendUserId { get; set; }
        }

        // POST: /api/Friends/accept
        [HttpPost("accept")]
        public async Task<IActionResult> annehmen([FromBody] FriendActionDto dto)
        {
            await dbManager.bestaetigeFreundschaftAsync(dto.UserId, dto.FriendUserId);
            return Ok();
        }

        // POST: /api/Friends/reject
        [HttpPost("reject")]
        public async Task<IActionResult> ablehnen([FromBody] FriendActionDto dto)
        {
            await dbManager.lehneFreundschaftAsync(dto.UserId, dto.FriendUserId);
            return Ok();
        }
    }
}
