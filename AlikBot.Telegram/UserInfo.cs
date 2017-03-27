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
	public class UserInfo
	{
		public int ID { get; set; }
		public Guesser Guesser { get; set; }
		public bool QuantityRequest { get; set; }
		public bool InterviewRequest { get; set; }
		public char Previous { get; set; }

		public UserInfo(int id) => ID = id;
	}
}