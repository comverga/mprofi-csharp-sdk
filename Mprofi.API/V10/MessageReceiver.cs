
namespace Mprofi.API.V10
{
    public class MessageReceiver : IMessageReceiver
    {
        const string BASE_URL = "http://192.168.0.65:9000/1.0";

        public required string Apikey { get; set; }

        private HttpClient _httpClient = new HttpClient();

        public GetIncomingSMSResult GetIncomingSMSMessages(DateTime dateFrom, DateTime dateTo)
        {
            throw new NotImplementedException();
        }
    }
}
