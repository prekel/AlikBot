using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using NLog;

namespace AlikBot.Core
{
	public class WordBase : List<string>
	{
		private Logger Log = LogManager.GetCurrentClassLogger();

		public Dictionary<string, int> Files;

		public WordBase(params string[] files)
		{
			Files = new Dictionary<string, int>();
			foreach (var i in files)
			{
				Files[i] = 1;
			}
		}

		public async Task InitAsync()
		{
			var l = new Dictionary<Task<string>, string>();
			foreach (var f in Files)
			{
				var r = new StreamReader(f.Key);
				Log.Debug($"Загружается {f.Key}");
				l.Add(r.ReadToEndAsync(), f.Key);
			}
			foreach (var i in l)
			{
				AddRange((await i.Key).Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));
				Log.Debug($"Загружено {i.Value}");
			}
		}

		public void Init()
		{
			foreach (var f in Files)
			{
				using (var r = new StreamReader(f.Key))
				{
					Log.Debug($"Загружается {f.Key}");
					AddRange(r.ReadToEnd().Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));
					Log.Debug($"Загружено {f.Key}");
				}
			}
		}
	}
}
