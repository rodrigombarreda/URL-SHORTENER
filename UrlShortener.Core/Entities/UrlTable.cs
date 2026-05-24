namespace UrlShortener.Core.Entities

{

    public class UrlTable
    {
        public int Id { get; set; }
        public string LongUrl { get; set; }
        public string? ShortCode { get; set; }
        public DateTime CreatedOnUtc { get; set; }
    }
}
