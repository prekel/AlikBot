using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputMessageContents;
using Telegram.Bot.Types.ReplyMarkups;

using AlikBot.Core;

namespace AlikBot.Telegram
{
	public class Program
	{
		private static TelegramBotClient Bot;

		private static WordBase Words;

		public static UserBase UserBase { get; set; } = new UserBase();

		private static void Main(string[] args)
		{
			using (var r = new StreamReader("api.txt"))
			{
				Bot = new TelegramBotClient(r.ReadLine());
			}

			Words = new WordBase(@"C:\Users\vladislav\OneDrive\Projects\AlikBot\AlikBot.Core\pldf.txt");
			Words.Init();

			Bot.OnMessage += BotOnMessageReceived;
			Bot.OnMessageEdited += BotOnMessageReceived;
			Bot.OnReceiveError += BotOnReceiveError;

			var me = Bot.GetMeAsync().Result;

			Console.Title = me.Username;

			Bot.StartReceiving();
			Bot.SendTextMessageAsync(115533229, $"Я проснулся {DateTime.Now}");
			Console.ReadLine();
			Bot.StopReceiving();
		}

		private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
		{
			Debugger.Break();
		}

		private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
		{
			var message = messageEventArgs.Message;
			var id = message.From.Id;
			var text = message.Text;
			var chatid = message.Chat.Id;

			Console.WriteLine($"{id} {message.From.FirstName} {message.From.LastName} {text}");

			if (message.Type != MessageType.TextMessage) return;

			if (!UserBase.ContainsKey(id))
			{
				UserBase[id] = new UserInfo(id);
			}

			if (UserBase[id].QuantityRequest)
			{
				try
				{
					UserBase[id].Guesser = new Guesser(int.Parse(text), Words);
					UserBase[id].QuantityRequest = false;
					UserBase[id].InterviewRequest = true;

					var g = UserBase[id].Guesser;
					var guess = g.GuessAnswer();
					var l = guess.Letter;

					await Bot.SendTextMessageAsync(chatid, $"Попытка №1 Шаблон: {g.Matcher.Pattern}\nГде буква '{l}'?");
					UserBase[id].Previous = l;
				}
				catch (Exception e)
				{
					Console.WriteLine($" - {id} {message.From.FirstName} {message.From.LastName} Спровоцировал: {e.GetType()} {e.Message}");
					await Bot.SendTextMessageAsync(chatid, $"Что-то пошло не так: {e.GetType()} {e.Message}");
					UserBase[id].Guesser = null;
					UserBase[id].QuantityRequest = false;
					UserBase[id].InterviewRequest = false;
					UserBase[id].Previous = '0';
				}
			}
			else if (UserBase[id].InterviewRequest)
			{
				try
				{
					var g = UserBase[id].Guesser;
					var p = UserBase[id].Previous;

					var d = (from i in text.Split() select int.Parse(i)).ToArray();
					g.Hint(p, d);

					if (g.Matcher.Unknown == 0)
					{
						Console.WriteLine($" - {id} {message.From.FirstName} {message.From.LastName} Угадал слово '{g.Matcher.Pattern}' c {g.Attempts} попытки!");
						await Bot.SendTextMessageAsync(chatid, $"Угадано слово '{g.Matcher.Pattern}' c {g.Attempts} попытки!");
						UserBase[id].InterviewRequest = false;
					}
					else
					{
						var guess = g.GuessAnswer();
						var l = guess.Letter;

						await Bot.SendTextMessageAsync(chatid, $"Попытка №{g.Attempts} Шаблон: {g.Matcher.Pattern}\nГде буква '{l}'?");

						UserBase[id].Previous = l;
					}
				}
				catch (Exception e)
				{
					Console.WriteLine($" - {id} {message.From.FirstName} {message.From.LastName} Спровоцировал: {e.GetType()} {e.Message}");
					await Bot.SendTextMessageAsync(chatid, $"Что-то пошло не так: {e.GetType()} {e.Message}");
					UserBase[id].Guesser = null;
					UserBase[id].QuantityRequest = false;
					UserBase[id].InterviewRequest = false;
					UserBase[id].Previous = '0';
				}
			}
			else if (text.ToLower() == "где")
			{
				await Bot.SendLocationAsync(chatid, 56.055237f, 92.968446f);
			}
			else if (text.ToLower() == "убейся")
			{
				Environment.Exit(id);
			}
			else if (text.ToLower() == "/rules")
			{
				await Bot.SendTextMessageAsync(chatid, "Наберите /startgame для начала. Ответ боту — 0, если буквы нет, и номера букв через пробел, если есть");
			}
			else if (text.ToLower() == "/startgame")
			{
				//UserBase[id].Guesser = new Guesser(Words);
				UserBase[id].QuantityRequest = true;
				await Bot.SendTextMessageAsync(chatid, "Сколько букв в слове?");
			}
			else
			{
				await Bot.SendTextMessageAsync(chatid, message.Text);
			}
		}
	}
}
