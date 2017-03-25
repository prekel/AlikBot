using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlikBot.Core
{
    public class Matcher
    {
		public string Pattern { get; set; }

		public Matcher(string pattern) => Pattern = pattern;

		public Matcher() => Pattern = "___";

		public bool Match(string s)
		{
			if (s.Length != Pattern.Length)
				return false;
			for (var i = 0; i < s.Length; i++)
			{
				if (s[i] != Pattern[i] && Pattern[i] != '_')
					return false;
			}
			return true;
		}
	}
}
