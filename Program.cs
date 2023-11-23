using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Microsoft.Data.Sqlite;

namespace UkraineBot 
{
    public class MessageCheck
    {
        public Message message;
        public string settlements;
        public int settlementsCost;
        public string defender;

        public MessageCheck(Message checkedMessage, string command)
        {
            string messageText = command;
            this.message = checkedMessage;

            settlements = messageText.Substring(messageText.IndexOf('{') + 1, messageText.IndexOf('}') - messageText.IndexOf('{') - 1);
            int cost = 0;

            int.TryParse(messageText.Substring(messageText.IndexOf('(') + 1, messageText.IndexOf(')') - messageText.IndexOf('(') - 1), out cost);
            settlementsCost = cost;

            defender = messageText.Substring(messageText.IndexOf('[') + 1, messageText.IndexOf(']') - messageText.IndexOf('[') - 1);
        }
    }

    public class Program
    {
        internal static string bot_id = "6328855105:AAE5s3r8OTwBcZTA0dJsWj8K3TWygu15KFY";

        private static string dbFileName = "bot.db";

        public static TelegramBotClient client = new TelegramBotClient(bot_id);
        private static CancellationTokenSource cts = new CancellationTokenSource();
        private static ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };
        public static MessageCheck checkedMessage;
        public static void Main(string[] args)
        {
            client.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
                );
            Console.ReadLine();
        }

        

        private static async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Type == UpdateType.Message)
                {
                    Message message = update.Message;
                    long msgId = update.Message.MessageId;
                    long chatId = update.Message.Chat.Id;
                    string msgText = update.Message.Text;

                    if (msgText != null)
                    {
                        if (msgText.Contains("/check"))
                        {
                            if (message.ReplyToMessage != null)
                            {
                                checkedMessage = new MessageCheck(message.ReplyToMessage, msgText);

                                int cost = 0;
                                int.TryParse(msgText.Substring(msgText.IndexOf('(') + 1, msgText.IndexOf(')') - msgText.IndexOf('(') - 1), out cost);

                                string settlements = msgText.Substring(msgText.IndexOf('{') + 1, msgText.IndexOf('}') - msgText.IndexOf('{') - 1);
                                string defender = msgText.Substring(msgText.IndexOf('[') + 1, msgText.IndexOf(']') - msgText.IndexOf('[') - 1);

                                using (var connection = new SqliteConnection($"Data Source={dbFileName}"))
                                {
                                    connection.Open();
                                    SqliteCommand command = new SqliteCommand();
                                    command.Connection = connection;
                                    command.CommandText = $"CREATE TABLE IF NOT EXISTS {ConvertLongToString(chatId)}" +
                                        $"(messageId INTEGER NOT NULL PRIMARY KEY, " +
                                        $"invader TEXT NOT NULL, " +
                                        $"settlements TEXT NOT NULL, " +
                                        $"settlementsCost INTEGER NOT NULL, " +
                                        $"defender TEXT NOT NULL" +
                                        $")";
                                    command.ExecuteNonQuery();

                                    command.CommandText = $"INSERT INTO {ConvertLongToString(chatId)} " +
                                        $"(messageId, invader, settlements, settlementsCost, defender) " +
                                        $"VALUES ( {chatId}, '{message.From.FirstName}', '{settlements}', {cost}, '{defender}')";
                                    command.ExecuteNonQuery();
                                }
                            }
                        }
                        if (msgText.Contains("/get_mp"))
                        {
                            Random r = new Random();
                            int points = r.Next(1, 70);

                            string text = $"#ВоенныеЗаводы\n\n" +
                                $"Военные заводы игрока {message.From.FirstName} произвели ему {points} очк.";

                            client.SendTextMessageAsync(chatId, text);
                        }
                    }
                    
                }
                else if (update.Type == UpdateType.EditedMessage)
                {
                    if ((checkedMessage != null) && (update.EditedMessage.MessageId == checkedMessage.message.MessageId))
                    {
                        Console.WriteLine($"Message update in {DateTime.Now}");
                        if (update.EditedMessage.Text.Contains("Победитель") || update.EditedMessage.Text.EndsWith("Ничья"))
                        {
                            string sendMessage = 
                                $"#Бой\n\n" +
                                $"⚔️ Атакующий: {checkedMessage.message.From.FirstName}\n" +
                                $"🛡 Обороняющийся: {checkedMessage.defender}\n\n" +
                                $"🗺 Населенные пункты: {checkedMessage.settlements} ({checkedMessage.settlementsCost})\n\n";

                            client.SendTextMessageAsync(update.EditedMessage.Chat, sendMessage);
                        }
                    }
                }
            } catch (Exception ex) { }
            
        }

        private static Task HandlePollingErrorAsync(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
        {
            return Task.CompletedTask;
        }

        public static string ConvertLongToString(long value)
        {
            string result = string.Empty;
            foreach (var c in value.ToString().ToCharArray())
            {
                if (c == '-')
                    result += 'k';
                else if (c == '1')
                    result += 'a';
                else if (c == '2')
                    result += 'b';
                else if (c == '3')
                    result += 'c';
                else if (c == '4')
                    result += 'd';
                else if (c == '5')
                    result += 'e';
                else if (c == '6')
                    result += 'f';
                else if (c == '7')
                    result += 'g';
                else if (c == '8')
                    result += 'h';
                else if (c == '9')
                    result += 'i';
                else if (c == '0')
                    result += 'j';
            }

            return result;
        }
    }
}