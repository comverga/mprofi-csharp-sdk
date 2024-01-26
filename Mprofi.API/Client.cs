using Mprofi.API.V10;

namespace Mprofi.API
{
    public enum APIVersion
    {
        VERSION_1_0,
    }

    public class Client
    {
        private static IMessageSender? _messageSender;
        private static IMessageReceiver? _messageReceiver;

        public static IMessageSender CreateMessageSender(string apiKey, APIVersion apiVersion = APIVersion.VERSION_1_0)
        {
            if (_messageSender == null)
            {
                if (apiVersion == APIVersion.VERSION_1_0)
                {
                    _messageSender = new Mprofi.API.V10.MessageSender { Apikey = apiKey };
                }
                else
                {
                    throw new ArgumentException("Unsupported API version");
                }
            }

            return _messageSender;
        }

        public static IMessageReceiver CreateMessageReceiver(string apiKey, APIVersion apiVersion = APIVersion.VERSION_1_0)
        {
            if (_messageReceiver == null)
            {
                if (apiVersion == APIVersion.VERSION_1_0)
                {
                    _messageReceiver = new Mprofi.API.V10.MessageReceiver { Apikey = apiKey };
                }
                else
                {
                    throw new ArgumentException("Unsupported API version");
                }
            }

            return _messageReceiver;
        }
    }
}
