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
	}
}
