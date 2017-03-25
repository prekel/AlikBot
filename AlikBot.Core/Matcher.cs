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

		public int Known
		{
			get
			{
				var c = 0;
				foreach (var i in Pattern)
				{
					if (i != '_') 
						c++;
				}
				return c;
			}
		}

		public int Unknown => Pattern.Length - Known;

		public Matcher(string pattern) => Pattern = pattern;

		public Matcher(int n) => Pattern = new String('_', n);

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

		public override string ToString() => 
			$"Pattern:{Pattern} Length:{Pattern.Length} Known:{Known} Unknown:{Unknown}";
	}
}
