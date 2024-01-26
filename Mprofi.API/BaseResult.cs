namespace Mprofi.API
{
    public class BaseResult
    {
        public string? ErrorCode { get; internal set; } = "OK";
        public string? ErrorMessage { get; internal set; }        
        public bool IsSuccess { get; internal set; }
    }
}
