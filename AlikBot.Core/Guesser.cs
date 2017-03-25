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

		public Guesser()
		{
		}

		public Guesser(int n) => Matcher = new Matcher(n);

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
			throw new NotImplementedException();
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
	}
}
