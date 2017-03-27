using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using AlikBot.Core;

namespace AlikBot.Console
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var wb = new WordBase(@"C:\Users\vladislav\OneDrive\Projects\AlikBot\AlikBot.Core\pldf.txt");
			wb.Init();
			System.Console.Write("Количество букв: ");
			var n = int.Parse(System.Console.ReadLine());
			var g = new Guesser(n, wb);
			while (true)
			{
				var guess = g.GuessAnswer();
				var l = guess.Letter;
				var c = g.Attempts;
				System.Console.WriteLine($"Попытка №{c} Шаблон: {g.Matcher.Pattern}");
				System.Console.WriteLine($"Где буква '{l}'?");
				var d = (from i in System.Console.ReadLine().Split() select int.Parse(i)).ToArray();
				g.Hint(l, d);
				if (g.Matcher.Unknown == 0)
				{
					System.Console.WriteLine($"Угадано слово '{g.Matcher.Pattern}' c {c} попытки!");
					break;
				}
			}
		}
	}
}
