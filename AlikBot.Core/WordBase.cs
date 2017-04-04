using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using NLog;

namespace AlikBot.Core
{
	public class WordBase : HashSet<string>
	{
		private Logger Log = LogManager.GetCurrentClassLogger();

		public Dictionary<string, int> Files;

		public WordBase()
		{
		}

		public WordBase(params string[] files)
		{
			Files = new Dictionary<string, int>();
			foreach (var i in files)
			{
				Files[i] = 1;
			}
		}

		public WordBase(IEnumerable<string> files)
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
			var l2 = new List<Task>();
			foreach (var i in l)
			{
				var t = new Task(() =>
				{
					var spl = (i.Key.Result).Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
					foreach (var j in spl)
					{
						if (j == null)
							continue;
						Add(j);
					}
					//		AddRange((await i.Key).Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));
					Log.Debug($"Загружено {i.Value}");
				});
				l2.Add(t);
				t.Start();
			}
			foreach (var i in l2)
				await i;
			//Task.WaitAll(l2.ToArray());
		}

		public void Init()
		{
			foreach (var f in Files)
			{
				using (var r = new StreamReader(f.Key))
				{
					Log.Debug($"Загружается {f.Key}");
					var spl = r.ReadToEnd().Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
					foreach (var j in spl)
					{
						Add(j);
					}
					//AddRange(r.ReadToEnd().Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));
					Log.Debug($"Загружено {f.Key}");
				}
			}
		}
	}
}
