using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;
using System.Text;

using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputMessageContents;
using Telegram.Bot.Types.ReplyMarkups;

using NLog;

using AlikBot.Core;

namespace AlikBot.Telegram
{
	public class Program
	{
		public static TelegramBotClient Bot { get; private set; }

		public static WordBase Words { get; private set; }

		public static UserBase UserBase { get; set; } = new UserBase();

		private const int Vlad = 115533229;

		private static readonly Logger Log = LogManager.GetCurrentClassLogger();

		private static Task initbase;

		private static void Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

			using (var r = new StreamReader("api.txt"))
			{
				Bot = new TelegramBotClient(r.ReadLine());
			}

			Words = new WordBase(Directory.GetFiles("dictionaries"));
			Words.Files.AddRange(Directory.GetFiles("downloads"));

			Words.Init();
			//initbase = Words.InitAsync();
			//initbase.Wait();

			Bot.OnMessage += BotOnMessageReceived;
			Bot.OnMessageEdited += BotOnMessageReceived;
			Bot.OnReceiveError += BotOnReceiveError;
			Bot.OnCallbackQuery += BotOnCallbackQueryReceived;

			var me = Bot.GetMeAsync().Result;

			Console.Title = me.Username;

			Bot.StartReceiving();
			Send(Bot.SendTextMessageAsync(Vlad, $"Я проснулся {DateTime.Now}")).Wait();
			//initbase.Wait();
			Console.ReadLine();
			Bot.StopReceiving();
		}

		public static async Task Send2(long id, string message)
		{
			Log.Trace($"Out: {id}\r\n{message}");
			await Bot.SendTextMessageAsync(id, message);
		}

		public static async Task Send(Task<Message> func)
		{
			var m = await func;
			Log.Trace($"Out: {m.Chat.Id} {m.Chat.FirstName} {m.Chat.LastName}\r\n{m.Text}");
		}

		private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Log.Fatal(e.ExceptionObject.ToString());
			Debugger.Break();
			Environment.Exit(1);
		}

		private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
		{
			Log.Error(receiveErrorEventArgs.ApiRequestException.ToString());
			Debugger.Break();
		}

		private static async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
		{
			var id = callbackQueryEventArgs.CallbackQuery.Message.Chat.Id;
			await Send(Bot.SendTextMessageAsync(id, callbackQueryEventArgs.CallbackQuery.Data));
		}

		private InlineKeyboardMarkup CreateKeyboard(Matcher m)
		{
			var k = new InlineKeyboardButton[m.Length];
			for (var i = 0; i < k.Length; i++)
			{
				k[i] = new InlineKeyboardButton(m.Pattern[i] == '_' ? (i + 1).ToString() : m.Pattern[i].ToString());
			}
			return new InlineKeyboardMarkup(new[] { k, new[] { new InlineKeyboardButton("Всё") } });
		}

		private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
		{
			try
			{
				var message = messageEventArgs.Message;
				var id = message.From.Id;
				var text = message.Text;
				var chatid = message.Chat.Id;

				Log.Trace($" In: {id} {message.From.FirstName} {message.From.LastName}\r\n{text}");

				if (!UserBase.ContainsKey(id))
				{
					UserBase[id] = new UserInfo(id);
				}

				if (message.Type != MessageType.TextMessage) return;

				if (text.Contains("/download"))
				{
					var url = text.Substring(text.IndexOf(' ') + 1);
					Uri uri;
					try
					{
						uri = new Uri(url);
					}
					catch
					{
						uri = new Uri("http://" + url);
					}
					var reply = "";
					using (var client = new WebClient())
					{
						switch (text[text.IndexOf(' ') - 1])
						{
							case '0':
								client.Encoding = Encoding.Default;
								break;
							case '1':
								client.Encoding = Encoding.ASCII;
								break;
							case '2':
								client.Encoding = Encoding.BigEndianUnicode;
								break;
							case '3':
								client.Encoding = Encoding.Default;
								break;
							case '4':
								client.Encoding = Encoding.Unicode;
								break;
							case '5':
								client.Encoding = Encoding.UTF32;
								break;
							case '6':
								client.Encoding = Encoding.UTF7;
								break;
							case '7':
								client.Encoding = Encoding.UTF8;
								break;
							default:
								client.Encoding = Encoding.UTF8;
								break;
						};
						Log.Debug($"Начато скачивание и анализ {uri}");
						reply = client.DownloadString(uri);
						Log.Debug($"Скачано {reply.Length} символов из {uri}");
					}

					var regex1 = new Regex("[А-Яа-яЁё]{1,30}");
					var regex2 = new Regex("[А-Яа-яЁё-]{1,30}");
					var list = new List<string>();

					foreach (Match i in regex1.Matches(reply))
					{
						if (Words.Add(i.Value))
						{
							list.Add(WordBase.ToLower(i.Value));
						}
					}
					foreach (Match i in regex2.Matches(reply))
					{
						if (i.Value[0] == '-' || i.Value[i.Length - 1] == '-')
							continue;
						if (Words.Add(i.Value))
						{
							list.Add(WordBase.ToLower(i.Value));
						}
					}
					Log.Debug($"Найдено {list.Count} слов в {uri}");

					if (list.Count == 0)
					{
						await Send(Bot.SendTextMessageAsync(chatid, $"Добавлено {list.Count} слов"));
					}
					else
					{
						list.Sort();
						var str = list.Aggregate("", (current, i) => current + (i + "\r\n"));

						new Task(async () =>
						{
							var o = $"downloads\\{id} {DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")}.txt";
							using (var w = new StreamWriter(o))
							{
								await w.WriteAsync(str);
							}
							Log.Debug($"Записаны в {o} слова из {uri}");
						}).Start();

						if (list.Count < 500)
						{
							await Send(Bot.SendTextMessageAsync(chatid, $"Добавлено {list.Count} слов:\n{str}"));
						}
						else
						{
							await Send(Bot.SendTextMessageAsync(chatid, $"Добавлено {list.Count} слов"));
						}
					}
				}
				else if (text == "/info" && UserBase[id].Guesser != null)
				{
					await Send(Bot.SendTextMessageAsync(chatid, $"{UserBase[id].Guesser}"));
				}
				else if (text == "/wordscount" && UserBase[id].Guesser != null)
				{
					await Send(Bot.SendTextMessageAsync(chatid, $"Количество возможных слов: {UserBase[id].Guesser.Answer.PossibleWords.Count}"));
				}
				else if (text == "/words" && UserBase[id].Guesser != null)
				{
					if (UserBase[id].Guesser.Answer.PossibleWords.Count < 300)
					{
						var s = UserBase[id].Guesser.Answer.PossibleWords.Aggregate("", (current, i) => current + (i + '\n'));
						await Send(Bot.SendTextMessageAsync(chatid, $"Возможные слова:\n{s}"));
					}
					else
					{
						await Send(Bot.SendTextMessageAsync(chatid, $"{UserBase[id].Guesser.Answer.PossibleWords.Count} Слишком много слов для вывода"));
					}
				}
				else if (UserBase[id].QuantityRequest)
				{
					try
					{
						UserBase[id].Guesser = new Guesser(int.Parse(text), Words);

						UserBase[id].QuantityRequest = false;
						UserBase[id].InterviewRequest = true;

						var g = UserBase[id].Guesser;
						await Task.Delay(3000);
						var guess = g.GuessAnswer();
						var l = guess.Letter;

						await Send(Bot.SendTextMessageAsync(chatid, $"Попытка №1\nГде буква '{l}'?"));
						UserBase[id].Previous = l;
					}
					catch (Exception e)
					{
						Log.Warn($"{id} {message.From.FirstName} {message.From.LastName} Спровоцировал:\r\n{e.GetType()} {e.Message}");
						await Send(Bot.SendTextMessageAsync(chatid, $"Что-то пошло не так: {e.GetType()} {e.Message}"));
						UserBase.Remove(id);
					}
				}
				else if (UserBase[id].InterviewRequest)
				{
					try
					{
						var g = UserBase[id].Guesser;
						var p = UserBase[id].Guesser.Answer.Letter;

						var d = (from i in text.Split() select int.Parse(i)).ToArray();
						g.Hint(p, d);

						if (g.Matcher.Unknown == 0)
						{
							Log.Info(
								$"{id} {message.From.FirstName} {message.From.LastName} Угадал слово '{g.Matcher.Pattern}' c {g.Attempts} попытки!");
							await Send(Bot.SendTextMessageAsync(chatid, $"Угадано слово '{g.Matcher.Pattern}' c {g.Attempts} попытки!"));
							UserBase[id].InterviewRequest = false;
						}
						else
						{
							var guess = g.GuessAnswer();
							var l = guess.Letter;

							await Send(Bot.SendTextMessageAsync(chatid, $"Попытка №{g.Attempts + 1}\nГде буква '{l}'?"));

							UserBase[id].Previous = l;
						}
					}
					catch (Exception e)
					{
						Log.Warn($"{id} {message.From.FirstName} {message.From.LastName} Спровоцировал:\r\n{e.GetType()} {e.Message}");
						await Send(Bot.SendTextMessageAsync(chatid, $"Что-то пошло не так: {e.GetType()} {e.Message}"));
						UserBase.Remove(id);
					}
				}
				else if (text.ToLower() == "где")
				{
					await Bot.SendLocationAsync(chatid, 56.055237f, 92.968446f);
				}
				else if (text.ToLower() == "убейся")
				{
					new Task(async () =>
					{
						Log.Fatal($"{id} Сказал убиться((");
						await Task.Delay(1000);
						Environment.Exit(id);
					}).Start();
				}
				else if (text.ToLower() == "/rules")
				{
					await Send(Bot.SendTextMessageAsync(chatid,
						"Наберите /startgame для начала. Ответ боту — 0, если буквы нет, и номера букв через пробел, если есть"));
				}
				else if (text.ToLower() == "/startgame")
				{
					//initbase.Wait();
					if (!UserBase.ContainsKey(id))
					{
						UserBase[id] = new UserInfo(id);
					}
					UserBase[id].QuantityRequest = true;
					await Send(Bot.SendTextMessageAsync(chatid, "Сколько букв в слове?"));
				}
				else if (text.ToLower() == "/test")
				{
					var keyboard = new InlineKeyboardMarkup(new[]
					{
						new[]
						{
							new InlineKeyboardButton("1"),
							new InlineKeyboardButton("2"),
							new InlineKeyboardButton("3"),
							new InlineKeyboardButton("4"),
							new InlineKeyboardButton("5"),
							new InlineKeyboardButton("6"),
							new InlineKeyboardButton("7"),
							new InlineKeyboardButton("8"),
							new InlineKeyboardButton("9"),
						},
						new[]
						{
							new InlineKeyboardButton("Next")
						}
					});
					await Bot.SendTextMessageAsync(chatid, "Choose", replyMarkup: keyboard);
				}
				else
				{
					await Send(Bot.SendTextMessageAsync(chatid, message.Text));
				}
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
				await Send(Bot.SendTextMessageAsync(messageEventArgs.Message.From.Id, $"Что-то пошло не так: {e.GetType()} {e.Message}"));
			}
		}
	}
}
