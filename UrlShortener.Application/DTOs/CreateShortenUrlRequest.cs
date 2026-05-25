namespace UrlShortener.Core.DTOs
{
    public class CreateShortenUrlRequest
    {
        public required string LongUrl { get; set; }
    }
}
