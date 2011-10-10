using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TTTSolver_Lib;
using System.Drawing;

namespace TTTSolver_Solver
{
	public abstract class TTTSolver
	{
		protected TTTBoard Game { get; set; }
		
		public TTTSolver(TTTBoard board)
		{
			Game = board;			
		}

		public abstract Point GetNextMove();
		public void PlayNextMove()
		{
			Point move = GetNextMove();
			Game.TakeTurn(move.X, move.Y);
		}
		public void PlayRestOfGame()
		{
			while (!Game.GameOver)
			{
				PlayNextMove();
			}
		}
	}

	public class BruteForceTTTSolver : TTTSolver
	{
		public override Point GetNextMove()
		{
			return new Point(CalculateNextPlay().X, CalculateNextPlay().Y);
		}
		public class WeightedStateValue : List<StateValue>
		{
			public const int ONEMOVEAWAY = 10;
			public const int BLOCKOPPONENTWIN = 5;
			public const int TWOMOVESAWAY = 2;
			public int GetWeight(TTTBoard game, Player player)
			{
				var opponent = (player == Player.X ? Player.O : Player.X);
				int intermediate = 0;
				foreach (StateValue val in this)
				{
					if (game.GetLocation(val.X, val.Y).State == EnumConverter.From(player))
					{
						intermediate++;
					}
					else if (game.GetLocation(val.X, val.Y).State == EnumConverter.From(opponent))
					{
						intermediate--;
					}
				}

				if (intermediate == 2) // We already own two blocks in this winning condition
				{
					return ONEMOVEAWAY;
				}

				int opponentIntermediate = 0;
				foreach (StateValue val in this)
				{
					if (game.GetLocation(val.X, val.Y).State == EnumConverter.From(opponent))
					{
						opponentIntermediate++;
					}
				}

				if (opponentIntermediate == 2) // Opponent has two in a row, we can block!
				{
					return BLOCKOPPONENTWIN;
				}

				// Just return 0 for now if we can't win or block.
				return 0;
			}

			public WeightedStateValue() { }
			public WeightedStateValue(List<StateValue> state) { AddRange(state); }
		}
		public class WeightedStateCollection : List<WeightedStateValue>
		{
			public WeightedStateCollection() { }
			public WeightedStateCollection(List<List<StateValue>> orig)
			{
				foreach (var o in orig)
				{
					this.Add(new WeightedStateValue(o));
				}
			}
		}
		StateValue CalculateNextPlay()
		{
			var plays = Game.GetWinningPlays();
			WeightedStateCollection topValues = new WeightedStateCollection(Game.GetWinningPlays());
			
			var all = (from v in topValues
						 orderby v.GetWeight(Game, Game.CurrentPlayer) descending
						 select v).ToList();

			StateValue mostDegreesOfFreedom = null;
			int mostDegreesOfFreedomAmount = 0;
			foreach (var val in all)
			{
				int weight = val.GetWeight(Game, Game.CurrentPlayer);
				var openGameStates = val.Where(n => Game.GetLocation(n.X, n.Y).State == LocationState.U).ToList();
				foreach (var top in openGameStates)
				{
					var gameState = Game.GetLocation(top.X, top.Y);
					// todo: This needs to do some cell-level calculation of play strength
					// It currently takes the first available space in the heighted-weighted victory condition row,
					// which makes for some useless plays. It blocks well, though

					// If we're at row-weight 0, calculate degrees of freedom for this cell. 
					if (weight > 0 )
					{
						return gameState;
					}
					if (weight == 0)
					{
						// This works for most cases, but is too naive to deal with the case where a position will give the opponent a two-direction winning condition
						// todo: It needs to get the weighted values for its following turn to determine if a play is dangerous.
						int degreesOfFreedom = 0;
						if (Game.GetAbove(top.X, top.Y) != null && Game.GetAbove(top.X, top.Y).State == LocationState.U) { degreesOfFreedom++; }
						if (Game.GetBelow(top.X, top.Y) != null && Game.GetBelow(top.X, top.Y).State == LocationState.U) { degreesOfFreedom++; }
						if (Game.GetLeft(top.X, top.Y) != null && Game.GetLeft(top.X, top.Y).State == LocationState.U) { degreesOfFreedom++; }
						if (Game.GetRight(top.X, top.Y) != null && Game.GetRight(top.X, top.Y).State == LocationState.U) { degreesOfFreedom++; }
						if (degreesOfFreedom == 3)
						{
							mostDegreesOfFreedom = top;
							mostDegreesOfFreedomAmount = degreesOfFreedom;
							break;
						}
						if (mostDegreesOfFreedom == null || degreesOfFreedom > mostDegreesOfFreedomAmount)
						{
							mostDegreesOfFreedom = top;
							mostDegreesOfFreedomAmount = degreesOfFreedom;
						}
					}
				}
			}
			return mostDegreesOfFreedom;
		}

		public BruteForceTTTSolver(TTTBoard board) : base(board) { }
	}

	public class MiniMaxSolver : TTTSolver
	{
		public MiniMaxSolver(TTTBoard board) : base(board) { }

		void SetRowScores(List<StateValue> row)
		{
			int scoreX = 0, scoreO = 0;
			int rowIdx = row[0].Y;
			for (int i = 0; i < 3; i++)
			{
				var state = Game.GetLocation(i, rowIdx);
				if (state.State == LocationState.X)
				{
					scoreX++;
				}
				if (state.State == LocationState.O)
				{
					scoreO++;
				}
				
			}

			int score = 0;
			int advantage = 1;
			switch (Game.CurrentPlayer)
			{
				case Player.X:
					score = (int)Math.Pow(10, scoreX) * advantage;
					break;
				case Player.O:
					score = -(int)Math.Pow(10, scoreO) * advantage;
					break;
			}
			for (int i = 0; i < 3; i++)
			{
				var state = Game.GetLocation(i, rowIdx);
				state.Weight += score;
			}
		}

		void ResetRowScores()
		{
			for (int x = 0; x < 3; x++)
			{
				for (int y = 0; y < 3; y++)
				{
					var location = Game.GetLocation(x, y);
					location.Weight = 0;
				}
			}
		}

		void SetRowScores()
		{
			var winningMoves = Game.GetWinningPlays();
			
			foreach (var rows in winningMoves)
			{
				SetRowScores(rows);
			}
		}

		public override Point GetNextMove()
		{
			ResetRowScores();
			SetRowScores();

			List<StateValue> locations = (from ls in Game.GetSingleDimension()
										 where ls.State == LocationState.U
										 select ls).ToList();
			StateValue location = null;
			switch (Game.CurrentPlayer)
			{
				case Player.X:
					location = (from f in locations
					 where f.State == LocationState.U
					 orderby f.Weight descending
					 select f).FirstOrDefault();
					break;
				case Player.O:
					location = (from f in locations
					 where f.State == LocationState.U
					 orderby f.Weight
					 select f).FirstOrDefault();
					break;
			}

			return new Point(location.X, location.Y);
		}
	}

}
