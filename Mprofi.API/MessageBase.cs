namespace Mprofi.API
{
    public class MessageBase
    {
        public string? Encoding { get; set; }
        public required string Message { get; set; }
        public required string Recipient { get; set; }
        public string? Reference { get; set; }
    }
}
