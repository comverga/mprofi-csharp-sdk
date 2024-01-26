namespace Mprofi.API
{
    public class GetIncomingSMSResult : BaseResult
    {
        public List<IncomingMessage> Messages { get; set; } = new List<IncomingMessage>();
    }
}
