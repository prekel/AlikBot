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

		public HashSet<char> GuessedLetters = new HashSet<char>();

		public string GuessedLettersList => (from i in GuessedLetters select i.ToString()).ToString();

		public bool Match(string s)
		{
			if (s.Length != Pattern.Length)
				return false;
			if (s.ToCharArray().Count(i => GuessedLetters.Contains(i)) == 0 && GuessedLetters.Count > 0)
				return false;
			return !s.Where((t, i) => t != Pattern[i] && Pattern[i] != '_').Any();
		}

		public override string ToString() =>
			$"Pattern: {Pattern} Length: {Pattern.Length} Known: {Known} Unknown: {Unknown}";
	}
}
