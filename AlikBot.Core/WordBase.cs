using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AlikBot.Core
{
	public class WordBase : List<string>
	{
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
			foreach (var f in Files)
			{
				using (var r = new StreamReader(f.Key))
				{
					AddRange((await r.ReadToEndAsync()).Split());
				}
			}
		}
	}
}
