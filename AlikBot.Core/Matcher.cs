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

		public int Known => Pattern.Count(i => i != '_');

		public int Unknown => Pattern.Length - Known;

		public Matcher(string pattern) => Pattern = pattern;

		public Matcher(int n) => Pattern = new String('_', n);

		public Matcher() => Pattern = "___";

		public class Letters : HashSet<char>
		{
			public override string ToString()
			{
				var s = "";
				foreach (var i in this)
					s += i;
				return s;
			}

			public string ToString(string name) => $"[{Count} {name}: '{ToString()}']";
		}

		public Letters Guessed { get; set; } = new Letters();

		public Letters WrongGuessed { get; set; } = new Letters();

		public bool Match(string s)
		{
			if (s.Length != Pattern.Length)
				return false;
			for (var i = 0; i < s.Length; i++)
			{
				if (Pattern[i] == '_' && Guessed.Contains(s[i]))
					return false;
				if (WrongGuessed.Contains(s[i]))
					return false;
				if (Pattern[i] != s[i] && Pattern[i] != '_')
					return false;
			}
			return true;
			//return !s.Where((t, i) => t != Pattern[i] && Pattern[i] != '_').Any();
		}

		public override string ToString() =>
			$"Pattern: {Pattern} {Guessed.ToString("Guessed")} {WrongGuessed.ToString("WrongGuessed")} Length: {Pattern.Length} Known: {Known} Unknown: {Unknown}";
	}
}
