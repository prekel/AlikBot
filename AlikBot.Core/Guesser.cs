using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlikBot.Core
{
	public class Guesser
	{
		public Matcher Matcher { get; set; }

		public WordBase Words { get; set; }

		private HashSet<char> NotAllowedLetters = new HashSet<char>();

		public int Attempts => NotAllowedLetters.Count;

		public Guesser()
		{
		}

		public Guesser(int n) => Matcher = new Matcher(n);

		public Guesser(WordBase b) => Words = b;

		public Guesser(int n, WordBase b)
		{
			Matcher = new Matcher(n);
			Words = b;
		}

		public Guesser(Matcher m, WordBase b)
		{
			Matcher = m;
			Words = b;
		}

		public char Guess()
		{
			var d = new Dictionary<char, int>();
			foreach (var i in Words)
			{
				if (!Matcher.Match(i)) continue;
				foreach (var j in i)
				{
					if (NotAllowedLetters.Contains(j)) continue;
					if (d.ContainsKey(j)) d[j]++;
					else d[j] = 1;
				}
			}
			var l = d.ToList();
			l.Sort((a, b) => -a.Value.CompareTo(b.Value));
			NotAllowedLetters.Add(l[0].Key);
			return l[0].Key;
		}

		public void Hint(char letter, params int[] indexes)
		{
			if (indexes[0] == 0) return;
			var p = new StringBuilder(Matcher.Pattern);
			foreach (var i in indexes)
			{
				p[i - 1] = letter;
			}
			Matcher.Pattern = p.ToString();
		}

		public override string ToString() => $"{Matcher} Attempts: {Attempts}";
	}
}