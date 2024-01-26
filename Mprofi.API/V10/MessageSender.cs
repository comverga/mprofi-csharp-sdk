using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Mprofi.API.V10
{
    public class MessageSender : IMessageSender
    {
        const string BASE_URL = "https://api.mprofi.pl/1.0";        

        public required string Apikey { get; set; }

        private HttpClient _httpClient = new HttpClient();

        public SendMessageResult SendSMS(List<SmsMessage> messages)
        {
            if (messages.Count == 0)
                throw new ArgumentException("Empty list of messages");

            var serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            string jsonStr;
            string method;
            if (messages.Count == 1)
            {
                var message = messages[0];
                var payload = new SmsPayload
                {
                    Apikey = Apikey,
                    Recipient = message.Recipient,
                    Message = message.Recipient,
                    Reference = message.Reference,
                    Encoding = message.Encoding,
                    Date = message.Date,
                };
                jsonStr = JsonSerializer.Serialize(payload, serializerOptions);
                method = "send";
            }
            else
            {
                var payload = new SmsBulkPayload { Apikey = Apikey, Messages = messages };
                jsonStr = JsonSerializer.Serialize(payload, serializerOptions);
                method = "sendbulk";
            }

            string url = $"{BASE_URL}/{method}/";
            SendMessageResult result = new SendMessageResult();
            var content = new StringContent(jsonStr, Encoding.UTF8, "application/json");
            try
            {                
                HttpResponseMessage response = _httpClient.PostAsync(url, content).Result;
                var body = response.Content.ReadAsStringAsync().Result;
                if (response.IsSuccessStatusCode)
                {                                        
                    if (method == "send")
                    {
                        var sendResponse = JsonSerializer.Deserialize<SendResponse>(body, serializerOptions);
                        result.MessageIDs.Add(sendResponse.Id);
                    }
                    else
                    {
                        var sendBulkResponse = JsonSerializer.Deserialize<SendBulkResponse>(body, serializerOptions);
                        foreach (var entry in sendBulkResponse.Result)
                            result.MessageIDs.Add(entry.Id);
                    }
                    result.IsSuccess = true;
                }
                else
                {
                    var sendErrorResponse = JsonSerializer.Deserialize<SendErrorResponse>(body, serializerOptions);
                    result.ErrorCode = sendErrorResponse.ErrorCode ?? "ERR400";
                    if (sendErrorResponse.Detail != null)
                        result.ErrorMessage = sendErrorResponse.Detail;
                    else
                        result.ErrorMessage = sendErrorResponse.ErrorMessage ?? "unknown error";
                }                
            }
            catch (Exception ex)
            {
                result.ErrorCode = "ERR666";
                result.ErrorMessage = ex.Message;
            }

            return result;
        }


        public SendMessageResult BroadcastSMS(string message, List<string> recipients)
        {
            var serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var payload = new SmsBulkPayload
            {
                Apikey = Apikey,
                DefaultMessage = message,
                Recipients = recipients
            };

            string jsonStr = JsonSerializer.Serialize(payload, serializerOptions);
            string url = $"{BASE_URL}/sendbulk/";
            SendMessageResult result = new SendMessageResult();
            var content = new StringContent(jsonStr, Encoding.UTF8, "application/json");
            
            try
            {
                HttpResponseMessage response = _httpClient.PostAsync(url, content).Result;
                var body = response.Content.ReadAsStringAsync().Result;
                if (response.IsSuccessStatusCode)
                {
                    var sendBulkResponse = JsonSerializer.Deserialize<SendBulkResponse>(body, serializerOptions);
                    foreach (var entry in sendBulkResponse.Result)
                        result.MessageIDs.Add(entry.Id);

                    result.IsSuccess = true;
                }
                else
                {
                    var sendErrorResponse = JsonSerializer.Deserialize<SendErrorResponse>(body, serializerOptions);
                    result.ErrorCode = sendErrorResponse.ErrorCode ?? "ERR400";
                    if (sendErrorResponse.Detail != null)
                        result.ErrorMessage = sendErrorResponse.Detail;
                    else
                        result.ErrorMessage = sendErrorResponse.ErrorMessage ?? "unknown error";
                }
            }
            catch (Exception ex)
            {
                result.ErrorCode = "ERR666";
                result.ErrorMessage = ex.Message;
            }

            return result;
        }


        public CheckStatusResult CheckStatus(int msgId)
        {            
            var url = $"{BASE_URL}/status/?apikey={Apikey}&id={msgId}";
            try
            {             
                HttpResponseMessage response = _httpClient.GetAsync(url).Result;                
                if (response.IsSuccessStatusCode)
                {
                    var serializerOptions = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                    };

                    string body = response.Content.ReadAsStringAsync().Result;
                    var result = JsonSerializer.Deserialize<CheckStatusResult>(body, serializerOptions);
                    result.IsSuccess = true;
                    return result;
                }
                else
                {
                    var result = new CheckStatusResult();
                    result.ErrorCode = "ERR400";
                    result.ErrorMessage = response.ReasonPhrase;
                    return result;
                }
            }
            catch (Exception ex)
            {
                var result = new CheckStatusResult();
                result.ErrorCode = "ERR666";
                result.ErrorMessage = ex.Message;
                return result;                
            }
        }


        public SendMessageResult SendMMS(List<MmsMessage> messages, string pathToFile)
        {
            throw new NotImplementedException();
        }


        public SendMessageResult BroadcastMMS(string message, List<string> recipients, string pathToFile)
        {
            throw new NotImplementedException();
        }
    }

    internal class SmsPayload : SmsMessage
    {
        public required string Apikey { get; set; }
    }

    internal class SmsBulkPayload
    {
        public required string Apikey { get; set; }
        public List<SmsMessage>? Messages { get; set; }
        public List<string>? Recipients { get; set; }        
        public string? DefaultMessage { get; set; }        

    }

    internal class SendResponse
    {
        public int Id { get; set; } = -1;
    }

    internal class SendBulkResponse
    {
        public List<SendResponse> Result { get; set; } = new List<SendResponse>();
    }

    internal class SendErrorResponse
    {
        public string? Detail { get; internal set; }
        public string? ErrorCode { get; internal set; }
        public string? ErrorMessage { get; internal set; }

    }
}
