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
		private readonly Logger Log = LogManager.GetCurrentClassLogger();

		public List<string> Files { get; set; } = new List<string>();

		public WordBase()
		{
		}

		public WordBase(params string[] files) => Files.AddRange(files);

		public WordBase(IEnumerable<string> files) => Files.AddRange(files);

		public async Task InitAsync()
		{
			var tasks1 = new Dictionary<Task<string>, string>();
			foreach (var f in Files)
			{
				var r = new StreamReader(f);
				Log.Debug($"              Загружается {f}");
				tasks1.Add(r.ReadToEndAsync(), f);
			}
			var tasks2 = new List<Task>();
			foreach (var i in tasks1)
			{
				var t = new Task(() =>
				{
					var spl = (i.Key.Result).Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
					var c = 0;
					foreach (var j in spl)
					{
						if (j == null)
							continue;
						if (Add(j))
							c++;
					}
					Log.Debug($"{c:D7} слов загружено из {i.Value}");
				});
				tasks2.Add(t);
				t.Start();
			}
			foreach (var i in tasks2)
			{
				await i;
			}
		}

		public void Init()
		{
			foreach (var f in Files)
			{
				using (var r = new StreamReader(f))
				{
					Log.Debug($"              Загружается {f}");
					var spl = r.ReadToEnd().Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
					var c = 0;
					foreach (var j in spl)
					{
						if (Add(j))
							c++;
					}
					Log.Debug($"{c:D7} слов загружено из {f}");
				}
			}
		}

		public new bool Contains(string item)
		{
			return base.Contains(ToLower(item));
		}

		public new bool Add(string item)
		{
			return base.Add(ToLower(item));
		}

		public static string ToLower(string s)
		{
			return s.ToLower().Replace('ё', 'е');
		}
	}
}
