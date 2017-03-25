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
			System.Console.WriteLine("Количество: ");
			var n = int.Parse(System.Console.ReadLine());
			var g = new Guesser(n, wb);
			g.Guess
		}
	}
}
