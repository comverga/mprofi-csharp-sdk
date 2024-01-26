namespace Mprofi.API
{
    public interface IMessageReceiver
    {
        string Apikey { get; set; }

        public GetIncomingSMSResult GetIncomingSMSMessages(DateTime dateFrom, DateTime dateTo);
    }
}
