
namespace Mprofi.API
{
    public interface IMessageSender
    {
        string Apikey { get; set; }
        SendMessageResult SendSMS(List<SmsMessage> messages);
        public SendMessageResult BroadcastSMS(string message, List<string> recipients);
        public CheckStatusResult CheckStatus(int msgId);
        public SendMessageResult SendMMS(List<MmsMessage> messages, string pathToFile);
        public SendMessageResult BroadcastMMS(string message, List<string> recipients, string pathToFile);
    }
}
