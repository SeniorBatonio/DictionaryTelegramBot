using System;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Args;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Html5;
using OpenQA.Selenium;
using System.Collections.Generic;
using Telegram.Bot.Types.Enums;
using System.Linq;
using System.IO;

namespace DictionaryTelegramBot
{
    class Program
    {
        static ITelegramBotClient botClient;

        static void Main()
        {
            botClient = new TelegramBotClient("YOUR_ACCESS_TOKEN_HERE");
            var me = botClient.GetMeAsync().Result;
            Console.WriteLine(
              $"Hello, World! I am user {me.Id} and my name is {me.FirstName}."
            );

            botClient.OnMessage += Bot_OnMessage;
            botClient.StartReceiving();
            Thread.Sleep(int.MaxValue);
        }

        static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Text != null)
            {
                List<string> result = new List<string>();
                if (e.Message.Text == @"/start")
                {
                    result.Add("Введіть слово, тлумачення якого хочете знайти.");
                }
                else
                {
                    result = GetDefinition(e.Message.Text.ToLower());
                }
                Console.WriteLine($"Received a text message in chat {e.Message.Chat.Id}.");
                foreach (var mes in result)
                {
                    await botClient.SendTextMessageAsync(
                      chatId: e.Message.Chat,
                      text: mes
                    );
                }
            }
        }

        private static List<string> GetDefinition(string word)
        {
            List<string> result = new List<string>();
            var driverService = ChromeDriverService.CreateDefaultService(Directory.GetCurrentDirectory());
            driverService.HideCommandPromptWindow = true;
            ChromeOptions options = new ChromeOptions();
            options.LeaveBrowserRunning = true;
            ChromeDriver driver = new ChromeDriver(driverService, options);

            driver.Navigate().GoToUrl("http://ukrlit.org/slovnyk/" + word);
            try
            {
                var el = driver.FindElementByClassName("word__description");
                var ps = el.FindElements(By.TagName("p"));
                foreach (var p in ps)
                {
                    result.AddRange(CheckTextLength(p.Text));
                }
            }
            catch(Exception)
            {
                result.Add("Визначення не знайдено. Перевірте коректність введених даних.");
            }

            driver.Close();
            return result;
        }

        /// <summary>
        /// Функция для проверки строки больше ли размер дозволеного в Telegram. Эсли да, то делит строку пополам(по словам) и возвращает список строк у которых размер меньше критического.  
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static List<string> CheckTextLength(string text)
        {
            List<string> result = new List<string>();
            result.Add(text);
            while (result.Any(txt => txt.Length >=4096))
            {
                foreach (var txt in result)
                {
                    if (txt.Length > 4096)
                    {
                        result.InsertRange(result.IndexOf(txt) + 1, DivideTextToHalf(txt));
                        result.Remove(txt);
                        break;
                    }
                }
            }
            return result;
        }

        private static List<string> DivideTextToHalf(string text)
        {
            List<string> result = new List<string>();
            var wordArray = text.Split(' ');
            var text1 = wordArray.Take(wordArray.Length / 2);
            var text2 = wordArray.Skip(wordArray.Length / 2);
            string txt1 = "";
            string txt2 = "";
            foreach (var word in text1)
            {
                txt1 += word + " ";
            }
            foreach (var word in text2)
            {
                txt2 += word + " ";
            }
            result.Add(txt1);
            result.Add(txt2);
            return result;
        }
    }
}
