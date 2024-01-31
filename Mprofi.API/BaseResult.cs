namespace Mprofi.API
{
    public class BaseResult
    {
        public string? ErrorCode { get; set; } = "OK";
        public string? ErrorMessage { get; set; }        
        public bool IsSuccess { get; set; }
    }
}
