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

		private static Dictionary<int, Guesser> Guessers = new Dictionary<int, Guesser>();
		private static Dictionary<int, bool> QuantityRequest = new Dictionary<int, bool>();
		private static Dictionary<int, bool> InterviewRequest = new Dictionary<int, bool>();
		private static Dictionary<int, char> Previous = new Dictionary<int, char>();

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

			if (QuantityRequest.ContainsKey(id) && QuantityRequest[id])
			{
				try
				{
					Guessers[id].Matcher = new Matcher(int.Parse(text));
					QuantityRequest[id] = false;
					InterviewRequest[id] = true;

					var g = Guessers[id];
					var l = g.Guess();

					await Bot.SendTextMessageAsync(chatid, $"Попытка №1 Шаблон: {g.Matcher.Pattern}\nГде буква '{l}'?");
					Previous[id] = l;
				}
				catch (Exception e)
				{
					Console.WriteLine($" - {id} {message.From.FirstName} {message.From.LastName} Спровоцировал: {e.Message}");
					await Bot.SendTextMessageAsync(chatid, $"Что-то пошло не так: {e.Message}");
					Guessers[id] = null;
					QuantityRequest[id] = false;
					InterviewRequest[id] = false;
					Previous[id] = '0';
				}
			}
			else if (InterviewRequest.ContainsKey(id) && InterviewRequest[id])
			{
				try
				{
					var g = Guessers[id];
					var p = Previous[id];

					var d = (from i in text.Split() select int.Parse(i)).ToArray();
					g.Hint(p, d);

					if (g.Matcher.Unknown == 0)
					{
						Console.WriteLine($" - {id} {message.From.FirstName} {message.From.LastName} Угадал слово '{g.Matcher.Pattern}' c {g.Attempts} попытки!");
						await Bot.SendTextMessageAsync(chatid, $"Угадано слово '{g.Matcher.Pattern}' c {g.Attempts} попытки!");
						InterviewRequest[id] = false;
					}
					else
					{
						var l = g.Guess();

						await Bot.SendTextMessageAsync(chatid, $"Попытка №{g.Attempts} Шаблон: {g.Matcher.Pattern}\nГде буква '{l}'?");

						Previous[id] = l;
					}
				}
				catch (Exception e)
				{
					Console.WriteLine($" - {id} {message.From.FirstName} {message.From.LastName} Спровоцировал: {e.Message}");
					await Bot.SendTextMessageAsync(chatid, $"Что-то пошло не так: {e.Message}");
					Guessers[id] = null;
					QuantityRequest[id] = false;
					InterviewRequest[id] = false;
					Previous[id] = '0';
				}
			}
			else if (message.Text == "где")
			{
				await Bot.SendLocationAsync(chatid, 56.055237f, 92.968446f);
			}
			else if (message.Text == "/rules")
			{
				await Bot.SendTextMessageAsync(chatid, "Наберите /startgame для начала. Ответ боту — 0, если буквы нет, и номера букв через пробел, если есть");
			}
			else if (message.Text == "/startgame")
			{
				Guessers[id] = new Guesser(Words);
				QuantityRequest[id] = true;
				await Bot.SendTextMessageAsync(chatid, "Сколько букв в слове?");
			}
			else
			{
				await Bot.SendTextMessageAsync(chatid, message.Text);
			}
		}
	}
}
