﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Mprofi.API
{    
    internal class Program
    {
        // DROP
        const string SEND_API_TOKEN = "155b4752199c47b8914904cab695ce4f";
        const string RECEIVE_API_TOKEN = "123";


        // Przykład wysłania pojedynczej wiadomości
        static void SendSingleMessageExample()
        {
            // Tworzenie instancji obiektu do wysyłania wiadomości SMS/MMS
            IMessageSender sender = Client.CreateMessageSender(SEND_API_TOKEN, APIVersion.VERSION_1_0);
            // Obecnie wspierana jest tylko jedna wersja API: Mprofi.API.APIVersion.VERSION_1_0
            // Jest to wartość domyślna więc obecnie można pominąć ten parametr:
            /*
            
            IMessageSender sender = Client.CreateMessageSender("API_KEY_FOR_SENDING_SMS");
            
            */

            // UWAGA! W mprofi API Key stworzony do wysyłania wiadomości SMS nie może być użyty do odbierania wiadomości przychodzących.
            // Do tego celu trzeba stworzyć osobny API Key.

            // Tworzymy listę wiadomości do wysłania
            List<SmsMessage> messages = new List<SmsMessage>();

            // Tworzymy prostą wiadomość SMS do wysłania
            messages.Add(new SmsMessage
            {
                Recipient = "48500100200",
                Message = "Testowa wiadomosc sms"
            });

            // Wysyłamy wiadomości
            SendMessageResult result = sender.SendSMS(messages);
            if (result.IsSuccess)
            {
                var msgId = result.MessageIDs.FirstOrDefault<int>();
                Console.WriteLine("Wiadomość została wysłana i otrzymała id: {0}", msgId);
            }
            else
            {
                Console.WriteLine("Błąd podczas wysyłania wiadomości - kod błędu: {0}, opis błędu: {1}", result.ErrorCode, result.ErrorMessage);
            }
        }

        // Przykład wysyłania wielu wiadomości w pojedynczym żądaniu oraz sprawdzania statusu
        static void SendMultipleMessagesExample()
        {
            // Tworzenie instancji obiektu do wysyłania wiadomości SMS/MSS     
            IMessageSender sender = Client.CreateMessageSender(SEND_API_TOKEN);

            // Tworzymy listę wiadomości do wysłania
            List<SmsMessage> messages = new List<SmsMessage>();

            // Tworzymy prostą wiadomość SMS do wysłania
            messages.Add(new SmsMessage
            {
                Recipient = "48500100200",
                Message = "Testowa wiadomosc sms"
            });

            // Tworzymy wiadomość, która ma być zaplanowana do wysyłki za 2 godziny
            messages.Add(new SmsMessage
            {
                Recipient = "48600100200",
                Message = "Wiadomosc ktorej wyslanie zostanie odroczone o 2h",
                Date = DateTime.Now.AddHours(2)
            });

            // Tworzymy wiadomość z własnym ID
            messages.Add(new SmsMessage
            {
                Recipient = "48500200200",
                Message = "Wiadomosc z przekazanym ID klienta",
                Reference = "my-internal-id"
            });

            // Tworzenie wiadomości która zawiera poleski znaki diakrytyczne lub inne znaki specjalne
            messages.Add(new SmsMessage
            {
                Recipient = "48500100200",
                Message = "Treść wiadomości sms",
                Encoding = "utf-8"
            });

            // Wysyłamy wiadomości
            SendMessageResult result = sender.SendSMS(messages);

            // Klasa SendMessageResult posiada właściwości:
            // - IsSuccess (bool) - zwraca true jeśli operacją się powiodła
            // - ErrorCode (string) - ustawiana gdy IsSuccess zwraca false. Zawiera kod błędu.
            // - ErrorMessage (string) - ustawiana gdy IsSuccess zwraca false. Zawiera opis błędu.
            // - MessageIDs (List<int>) - zwiera listę unikalnych identyfikatorów wiadomości (kolejność
            //   taka sama jak wiadomości w liście przekazanej do metody SendSMS)

            if (result.IsSuccess)
            {
                Console.WriteLine("Wiadomości zostały wysłane");
                // Czekamy chwilę aby dać mprofi czas na przetworzenie i wysłanie wiadomości
                Thread.Sleep(10000);

                // UWAGA!
                // W kodzie produkcyjnym nie jest zalecane aby sprawdzać status wiadomości zaraz po wysłaniu.
                // Sprawdzenie nie powiedzie się jeśli zrobimy to zbyt szybko po wysłaniu (mprofi nie zdąży przetworzyć
                // i wysłać wiadomości) albo dostaniemy informację, że wiadomość jest wysłana bo nie ma jeszcze informacji
                // od operatora co się stało z wiadomoscią. Taka informacja może wrócić nawet 72h po wysłaniu.
                // Pierwsze odpytanie zalecamy po kilku lub kilkunastu minutach. Jeśli nie uzyskamy finalnego statusu,
                // powtarzamy wydłużając czas między kolejnymi sprawdzeniami. Jeśli po upływie 72h nadal brak statusu
                // finalnego, można odczekać 2h, dalsze odpytyanie nie ma sensu. 
                // 
                // Mprofi posiada mechanizm, który pozwala wysyłać statusy wiadomości na wskazany adres URL.
                // Jeśli to tylko możliwe zalecamy używanie tego mechanizmu do sprawdzania statusów. Jest to
                // dużo lepsza metoda niż odpytywanie o status każdej wiadomości pojedynczo.
                // Opis konfiguracji tego mechanizmu znajduje się w dokumentacji API

                // Sprawdzenie statusu wiadomości
                foreach (var msgId in result.MessageIDs)
                {
                    CheckStatusResult checkResult = sender.CheckStatus(msgId);
                    if (checkResult.IsSuccess)
                    {                        
                        Console.WriteLine(
                            "ID: {0}, status: {1}, timestamp: {2}, reference: {3}",
                            msgId,
                            checkResult.Status,
                            checkResult.Ts,
                            checkResult.Reference
                        );
                    }
                    else
                    {
                        Console.WriteLine("Błąd podczas sprawdzania statusu - kod błędu: {0}, opis błędu: {1}", result.ErrorCode, result.ErrorMessage);
                    }
                }
            }
            else
            {
                Console.WriteLine("Błąd podczas wysyłania wiadomości - kod błędu: {0}, opis błędu: {1}", result.ErrorCode, result.ErrorMessage);
            }
        }

        static void GetStatusExample()
        {
            // {"result":[{"id":69080336},{"id":69080337},{"id":69080338},{"id":69080339}]}
            const int msgId = 69080336;

            // Tworzenie instancji obiektu do wysyłania wiadomości SMS/MMS
            IMessageSender sender = Client.CreateMessageSender(SEND_API_TOKEN);

            // sprawdzamy status
            CheckStatusResult checkResult = sender.CheckStatus(msgId);
            Console.WriteLine(
                "ID: {0}, status: {1}, timestamp: {2}, reference: {3}",
                msgId,
                checkResult.Status,
                checkResult.Ts,
                checkResult.Reference
            );
        }

        static void SendTheSameMessageToMultipleRecipientsExample()
        {
            // Tworzenie instancji obiektu do wysyłania wiadomości SMS/MMS
            IMessageSender sender = Client.CreateMessageSender(SEND_API_TOKEN);

            // Jeśli potrzebujemy wysłać tę samą treść wiadomości do wielu odbiorców możemy skożystać z metody BroadcastSMS
            // Jest to metoda znaczenie wydajniejsza ale ma pewne ograniczenia:
            // - wiadomość nie może zawierać polskich znaków diakrytycznych lub znaków specjalnych,
            // - nie da się przekazać własnego identyfikatora (reference),
            // - nie da się zaplanować czasu wysłania wiadomości

            List<string> recipients = new List<string> { "48500100100", "48500100200", "48500100300" };
            SendMessageResult result = sender.BroadcastSMS("Taka sama tresc wiadomosci dla wszystkich odbiorco", recipients);
            if (result.IsSuccess)
            {
                // robimy coś z otrzymanymi identyfikatorami
                foreach (var msgId in result.MessageIDs)
                    Console.WriteLine(msgId);
            }
            else
            {
                // obsługa błędów
            }

        }


        static void SendMMSMessagesExample()
        {
            // Tworzenie instancji obiektu do wysyłania wiadomości
            IMessageSender sender = Client.CreateMessageSender(SEND_API_TOKEN);
            
            // Tworzymy listę wiadomości do wysłania
            List<MmsMessage> messages = new List<MmsMessage>();

            // Dodajemy wiadomości do wysłania
            messages.Add(new MmsMessage
            {
                Recipient = "48500100200",
                Message = "Testowa wiadomość MMS",
                Reference = "my-mms-id",
                Encoding = "utf-8"
            });

            string pathToImageFile = "c:/tmp/cat.png";

            // Wysyłamy wiadomości
            SendMessageResult result = sender.SendMMS(messages, pathToImageFile);
            if (result.IsSuccess)
            {
                // robimy coś z otrzymanymi identyfikatorami
                foreach (var msgId in result.MessageIDs)
                    Console.WriteLine(msgId);
            }
            else
            {
                // obsługa błędów
                Console.WriteLine("Błąd podczas wysyłania wiadomości - kod błędu: {0}, opis błędu: {1}", result.ErrorCode, result.ErrorMessage);
            }

            // UWAGA! Istnieje również metoda BroadcastMMS(string message, List<string> recipients, string pathToFile)
            // Podobnie jak w przypadku SMS jest wydajniejsza i posiada te same ograniczenia. 
        }


        static void ReceiveSMSMessages()
        {
            // Tworzenie instancji obiektu do odbierania wiadomości
            IMessageReceiver receiver = Client.CreateMessageReceiver(RECEIVE_API_TOKEN);
            DateTime fromDate = new DateTime(2023, 1, 1, 0, 0, 0);
            DateTime toDate = new DateTime(2023, 12, 31, 23, 59, 59);
            GetIncomingSMSResult result = receiver.GetIncomingSMSMessages(fromDate, toDate);

            // Klasa GetIncomingSMSResult posiada właściwości:
            // - IsSuccess (bool) - zwraca true jeśli operacją się powiodła
            // - ErrorCode (string) - ustawiana gdy IsSuccess zwraca false. Zawiera kod błędu.
            // - ErrorMessage (string) - ustawiana gdy IsSuccess zwraca false. Zawiera opis błędu.
            // - Messages (List<IncomingMessage>) - lista pobranych wiadomości
            // 
            // Klasa IncomingMessage posiada właściwości:
            // - Message (string) - treść wiadomości
            // - Sender (string) - nadawca wiadomości
            // - Recipient (string) - odbiorca wiadomości (numer telefonu komórkowego)
            // - Ts (DateTime) - data i czas odebrania wiadomości

            if (result.IsSuccess)
            {
                foreach (var msg in result.Messages)
                {
                    Console.WriteLine(
                        "Timestamp: {0}, nadawca: {1}, odbiorca: {2}, treść: {3}",
                        msg.Ts,
                        msg.Sender,
                        msg.Recipient,
                        msg.Message
                    );
                }
            }
            else
            {
                // obsługa błędów
                Console.WriteLine("Błąd podczas pobierania wiadomości - kod błędu: {0}, opis błędu: {1}", result.ErrorCode, result.ErrorMessage);
            }
        }


        static void Main(string[] args)
        {
            /// using Mprofi.API;
            /// wymagania .NET 8, C# 11?

            // SendSingleMessageExample();

            // GetStatusExample();

            // SendMultipleMessagesExample();

            // SendTheSameMessageToMultipleRecipientsExample();

            // SendMMSMessagesExample();

            //TODO: ReceiveSMSMessages();            
        }
    }
}
