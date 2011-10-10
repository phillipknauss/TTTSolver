using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using TTTSolver_Lib;

namespace TTTSolver
{
	

	class Program
	{
		static void TakeTurn(TTTBoard game, int x, int y)
		{
			Console.WriteLine("Turn: " + game.CurrentPlayer.ToString() + " plays at (" + x.ToString() + "," + y.ToString() + ")");
			game.TakeTurn(x, y);
		}

		static void Main(string[] args)
		{
			TTTBoard game = new TTTBoard();
			Console.WriteLine("Began game.");
			List<Point> plays = new List<Point>()
			{
				new Point(1,1), // X
				new Point(0,1), // O
				new Point(1,0),
				new Point(1,2),
				new Point(2,0),
				new Point(0,0),
				new Point(2,1),
				new Point(2,2),
				new Point(0,2)
			};
			
			while (!game.GameOver)
			{
				Console.WriteLine("Game state: " + game.Serialize());
				Console.Write("Next play for " + game.CurrentPlayer.ToString() + ": ");
				string nextPlayStr = Console.ReadLine();
				//Point nextPlay = plays.First();
				int x = -1;
				int y = -1;
				int.TryParse(nextPlayStr.Substring(0, 1),out x);
				int.TryParse(nextPlayStr.Substring(1, 1), out y);
				if (x < 0 || y < 0) { throw new InvalidOperationException("Couldn't parse input."); }
				Point nextPlay = new Point(x,y);
				//plays.RemoveAt(0);
				TakeTurn(game, nextPlay.X, nextPlay.Y);
				//if (plays.Count == 0) { Console.WriteLine("Ran out of moves."); Console.WriteLine("Final state: " + game.Serialize()); break; }
			}
			Console.WriteLine("Game Ended.");
			if (game.XWins)
			{
				Console.WriteLine("X Wins!");
			}
			else if (game.OWins)
			{
				Console.WriteLine("O Wins!");
			}
			else
			{
				Console.WriteLine("Draw.");
			}

			Console.ReadKey();
		}
	}
}
