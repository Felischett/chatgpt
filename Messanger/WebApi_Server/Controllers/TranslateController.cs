using Microsoft.AspNetCore.Mvc;
using WebApi_Server.Services;

namespace WebApi_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TranslateController : ControllerBase
    {
        private readonly DeeplTranslationService _deepl;

        public TranslateController(DeeplTranslationService deepl)
        {
            _deepl = deepl;
        }

        [HttpPost]
        public async Task<IActionResult> Translate([FromBody] TranslateRequest req)
        {
            if (req == null) return BadRequest(new { message = "Body fehlt." });
            if (string.IsNullOrWhiteSpace(req.Text)) return BadRequest(new { message = "text fehlt." });
            if (string.IsNullOrWhiteSpace(req.TargetLang)) return BadRequest(new { message = "targetLang fehlt." });

            var translated = await _deepl.TranslateAsync(req.Text, req.TargetLang);
            return Ok(new TranslateResponse { TranslatedText = translated });
        }

        public sealed class TranslateRequest
        {
            public string Text { get; set; } = "";
            public string TargetLang { get; set; } = "DE";
        }

        public sealed class TranslateResponse
        {
            public string TranslatedText { get; set; } = "";
        }
    }
}