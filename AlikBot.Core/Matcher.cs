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
			public override string ToString() => this.Aggregate("", (current, i) => current + i);

			public string ToString(string name) => $"[{Count} {name}: '{ToString()}']";
		}

		public Letters Guessed { get; set; } = new Letters();

		public Letters WrongGuessed { get; set; } = new Letters();

		public bool Match(string s)
		{
			return s.Length == Pattern.Length
			       && !s.Where((t, i) =>
					       Pattern[i] == '_' && Guessed.Contains(t) ||
					       WrongGuessed.Contains(t) ||
					       Pattern[i] != t && Pattern[i] != '_')
				       .Any();
		}

		public override string ToString() =>
			$"Pattern: {Pattern} {Guessed.ToString("Guessed")} {WrongGuessed.ToString("WrongGuessed")} Length: {Pattern.Length} Known: {Known} Unknown: {Unknown}";
	}
}