using System;
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

namespace AlikBot.Telegram
{
	class Program
	{
		private static TelegramBotClient Bot;

		static void Main(string[] args)
		{
			using (var r = new StreamReader("api.txt"))
			{
				Bot = new TelegramBotClient(r.ReadLine());
			}

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
			Console.WriteLine($"{message.From.Id} {message.From.FirstName} {message.From.LastName} {message.Text}");

			if (message == null || message.Type != MessageType.TextMessage) return;

			if (message.Text == "где")
			{
				await Bot.SendLocationAsync(message.Chat.Id, 56.055237f, 92.968446f, replyMarkup: new ReplyKeyboardHide());
			}
			else
			{
				await Bot.SendTextMessageAsync(message.Chat.Id, message.Text, replyMarkup: new ReplyKeyboardHide());
			}
		}
	}
}
