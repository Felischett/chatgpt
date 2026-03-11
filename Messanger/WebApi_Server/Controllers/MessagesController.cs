using Microsoft.AspNetCore.Mvc;
using Models;
using ORM.Services;
using WebApi_Server.Services;

namespace WebApi_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly DbManager _db;
        private readonly MessageCryptoService _crypto;

        public MessagesController(DbManager db, MessageCryptoService crypto)
        {
            _db = db;
            _crypto = crypto;
        }

        // GET: /api/Messages/conversation?userId=1&friendUserId=2&take=200
        [HttpGet("conversation")]
        public async Task<IActionResult> Conversation([FromQuery] int userId, [FromQuery] int friendUserId, [FromQuery] int take = 200)
        {
            if (userId <= 0 || friendUserId <= 0) return BadRequest("userId/friendUserId fehlt.");

            var msgs = await _db.holeChatAsync(userId, friendUserId, take);

            foreach (var m in msgs)
            {
                try
                {
                    m.Message = _crypto.Decrypt(m.Message);
                }
                catch
                {
                    m.Message = "(nicht lesbar)";
                }
            }

            return Ok(msgs);
        }

        // POST: /api/Messages/send
        // Body: Models.Messages (SenderId, EmpfaengerId, Message)
        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] Messages msg)
        {
            if (msg == null) return BadRequest("Body fehlt.");
            if (msg.SenderId <= 0 || msg.EmpfaengerId <= 0) return BadRequest("SenderId/EmpfaengerId fehlt.");
            if (string.IsNullOrWhiteSpace(msg.Message)) return BadRequest("Message fehlt.");

            var encrypted = _crypto.Encrypt(msg.Message.Trim());
            var saved = await _db.sendeMessageAsync(msg.SenderId, msg.EmpfaengerId, encrypted);

            return Ok(saved.Id);
        }
    }
}
