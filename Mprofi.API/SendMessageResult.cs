namespace Mprofi.API
{
    public class SendMessageResult : BaseResult
    {
        public List<int> MessageIDs { get; internal set; } = new List<int>();
    }
}
