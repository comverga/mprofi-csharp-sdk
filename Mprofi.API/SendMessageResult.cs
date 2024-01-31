namespace Mprofi.API
{
    public class SendMessageResult : BaseResult
    {
        public List<int> MessageIDs { get; set; } = new List<int>();
    }
}
