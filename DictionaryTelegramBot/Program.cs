using System;
using Telegram.Bot;

namespace DictionaryTelegramBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var botClient = new TelegramBotClient("784574297:AAHU6DEHTHaTf8UrkSaAir9oPXPm5oVU0pc");
            var me = botClient.GetMeAsync().Result;
            Console.WriteLine($"Hello! I'm user {me.Id} and my name is {me.FirstName}.");
            Console.ReadLine();
        }
    }
}
