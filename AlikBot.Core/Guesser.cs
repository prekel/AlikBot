﻿using System;
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

		public int Attempts => Matcher.Guessed.Count + Matcher.WrongGuessed.Count;

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

		public class GuesserAnswer
		{
			public char Letter { get; set; }

			public List<string> PossibleWords { get; set; } = new List<string>();

			public GuesserAnswer()
			{
			}

			public override string ToString() => $"Letter: '{Letter}' WordsCount: {PossibleWords.Count}";
		}

		public GuesserAnswer Answer { get; set; } = new GuesserAnswer();

		public GuesserAnswer GuessAnswer()
		{
			Guess();
			return Answer;
		}

		public void Guess()
		{
			var d = new Dictionary<char, int>();
			Answer = new GuesserAnswer();
			foreach (var i in Words)
			{
				if (!Matcher.Match(i)) continue;
				Answer.PossibleWords.Add(i);
				foreach (var j in i)
				{
					if (Matcher.Guessed.Contains(j)) continue;
					if (d.ContainsKey(j)) d[j]++;
					else d[j] = 1;
				}
			}
			var l = d.ToList();
			l.Sort((a, b) => -a.Value.CompareTo(b.Value));
			Answer.Letter = l[0].Key;
		}

		public void Hint(char letter, params int[] indexes)
		{
			if (indexes[0] == 0)
			{
				Matcher.WrongGuessed.Add(letter);
				return;
			}
			Matcher.Guessed.Add(letter);
			var p = new StringBuilder(Matcher.Pattern);
			foreach (var i in indexes)
			{
				p[i - 1] = letter;
			}
			Matcher.Pattern = p.ToString();
		}

		public override string ToString() => $"Matcher: [{Matcher}] Answer: [{Answer}] Attempts: {Attempts}";
	}
}