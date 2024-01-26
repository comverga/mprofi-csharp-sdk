using System.Data;

namespace Mprofi.API
{
    public class CheckStatusResult : BaseResult
    {
        public string? Status { get; set; }
        public int Id { get; set; } = -1;
        public string? Reference { get; set; }
        public DateTime? Ts { get; set; }
    }
}
