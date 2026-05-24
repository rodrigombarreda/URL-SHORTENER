using Microsoft.AspNetCore.Mvc;
using UrlShortener.Core.DTOs;
using UrlShortener.Core.Services;
using Microsoft.AspNetCore.Authorization;

namespace UrlShortener.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class UrlController : ControllerBase
    {
        private readonly IUrlService _urlService;
        private readonly ILogger<UrlController> _logger;

        public UrlController(IUrlService urlService, ILogger<UrlController> logger)
        {
            _urlService = urlService;
            _logger = logger;
        }

        [HttpPost("shorten")]
        public async Task<IActionResult> CreateShortUrl([FromBody] CreateShortenUrlRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.LongUrl))
            {
                return BadRequest(new { message = "LongUrl is required." });
            }

            string shortCode = await _urlService.CreateShortUrlAsync(request.LongUrl);

            var response = new CreateShortenUrlResponse
            {
                ShortUrl = shortCode
            };
            return Ok(response);
        }

        [HttpGet]
        public IActionResult GetLongUrlEmpty()
        {
            return BadRequest(new { message = "ShortCode is required." });
        }

        [HttpGet("{shortCode}")]
        public async Task<IActionResult> GetLongUrl(string shortCode)
        {
            string longUrl = await _urlService.GetLongUrlAsync(shortCode);

            _logger.LogInformation("Redirecting {ShortCode} -> {LongUrl}", shortCode, longUrl);

            return Redirect(longUrl);
        }

        [HttpGet("throw")]
        public IActionResult ThrowError()
        {
            throw new InvalidOperationException("Test exception");
        }

    }

}
