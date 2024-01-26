namespace Mprofi.API
{
    public class IncomingMessage
    {
        public string Message { get; set; }
        public string Sender { get; set; }
        public string Recipient { get; set; }       
        public DateTime Ts { get; set; }
    }
}
