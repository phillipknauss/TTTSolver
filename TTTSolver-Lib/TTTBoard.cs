using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TTTSolver_Lib
{
	public class BoardModel
	{
		int XSize { get; set; }
		int YSize { get; set; }
		int locationsFilled = 0;
		public int LocationsFilled 
		{ 
			get
			{
				return locationsFilled;
			}
			set
			{
				locationsFilled = value;
			}
		}
		StateValue[,] Locations = null;
		public BoardModel(int xSize, int ySize)
		{
			XSize = xSize;
			YSize = ySize;
			Locations = new StateValue[XSize,YSize];
			for (int x = 0; x < XSize; x++)
			{
				for (int y = 0; y < YSize; y++)
				{
					Locations[x, y] = new StateValue() { X = x, Y = y, State = LocationState.U };
				}
			}
		}

		public void SetLocation(int x, int y, LocationState state, int weight=0)
		{
			Locations[x, y].X = x;
			Locations[x, y].Y = y;
			Locations[x, y].State = state;
			Locations[x, y].Weight = weight;
			LocationsFilled++;
		}
		public StateValue GetLocation(int x, int y)
		{
			return Locations[x, y];
		}
		public List<StateValue> GetSingleDimension()
		{
			List<StateValue> locations = new List<StateValue>();
			for (int x = 0; x < XSize; x++)
			{
				for (int y = 0; y < YSize; y++)
				{
					locations.Add(GetLocation(x, y));
				}
			}
			return locations;
		}
		public StateValue GetAbove(int x, int y)
		{
			if (y == 0)
			{
				return null;
			}
			return Locations[x, y - 1];
		}
		public StateValue GetBelow(int x, int y)
		{
			if (y == 2) { return null; }
			return Locations[x, y + 1];
		}
		public StateValue GetLeft(int x, int y)
		{
			if (x == 0) { return null; }
			return Locations[x - 1, y];
		}
		public StateValue GetRight(int x, int y)
		{
			if (x == 2) { return null; }
			return Locations[x + 1, y];
		}
	}

	public class GameModel
	{
		private List<Player> Players { get; set; }
	}

	public class TTTBoard : BoardModel
	{
		public StateValue LastPlay = null;
		public Player LastPlayer = Player.X;

		public TTTBoard() : base(3,3)
		{
		}

		int locationsFilled = 0;
		public bool GameOver
		{
			get
			{
				if (locationsFilled == 9) { return true; }
				if (XWins) { return true; }
				if (OWins) { return true; }
				return false;
			}
		}

		bool PlayerWins(LocationState stateToCheck)
		{
			string currentState = Serialize();
			foreach (List<StateValue> gameState in WinningPlays)
			{
				bool stateMatched = true;
				foreach (StateValue pos in gameState)
				{
					string posState = pos.State.ToString();
					string checkState = stateToCheck.ToString();
					if (GetLocation(pos.X, pos.Y).State != stateToCheck)
					{
						stateMatched = false;
					}
				}
				if (stateMatched)
				{
					return true;
				}
			}
			return false;
		}

		public bool XWins
		{
			get
			{
				return PlayerWins(LocationState.X);
			}
		}
		public bool OWins
		{
			get
			{
				return PlayerWins(LocationState.O);
			}
		}

		public string Serialize()
		{
			StringBuilder value = new StringBuilder();
			for (int x = 0; x < 3; x++)
			{
				for (int y = 0; y < 3; y++)
				{
					value.Append(GetLocation(x,y).ToString()).Append(",");
				}
			}
			return value.ToString().Substring(0, value.Length - 1);
		}
		public string SerializeWeights()
		{
			StringBuilder value = new StringBuilder();
			for (int x = 0; x < 3; x++)
			{
				for (int y = 0; y < 3; y++)
				{
					value.Append(GetLocation(x, y).Weight.ToString()).Append("|");
				}
				value.AppendLine();
			}
			return value.ToString().Substring(0, value.Length - 1);
		}
		static List<List<StateValue>> WinningPlays = new List<List<StateValue>>()
		{
			new List<StateValue>() { new StateValue() { X=0,Y=0,State=LocationState.S }, new StateValue() { X=0,Y=1,State=LocationState.S }, new StateValue() { X=0,Y=2,State=LocationState.S } },
			new List<StateValue>() { new StateValue() { X=1,Y=0,State=LocationState.S }, new StateValue() { X=1,Y=1,State=LocationState.S }, new StateValue() { X=1,Y=2,State=LocationState.S } },
			new List<StateValue>() { new StateValue() { X=2,Y=0,State=LocationState.S }, new StateValue() { X=2,Y=1,State=LocationState.S }, new StateValue() { X=2,Y=2,State=LocationState.S } },
			new List<StateValue>() { new StateValue() { X=0,Y=0,State=LocationState.S }, new StateValue() { X=1,Y=1,State=LocationState.S }, new StateValue() { X=2,Y=2,State=LocationState.S } },
			new List<StateValue>() { new StateValue() { X=2,Y=0,State=LocationState.S }, new StateValue() { X=1,Y=1,State=LocationState.S }, new StateValue() { X=0,Y=2,State=LocationState.S } },
			new List<StateValue>() { new StateValue() { X=0,Y=0,State=LocationState.S }, new StateValue() { X=1,Y=0,State=LocationState.S }, new StateValue() { X=2,Y=0,State=LocationState.S } },
			new List<StateValue>() { new StateValue() { X=0,Y=1,State=LocationState.S }, new StateValue() { X=1,Y=1,State=LocationState.S }, new StateValue() { X=2,Y=1,State=LocationState.S } },
			new List<StateValue>() { new StateValue() { X=0,Y=2,State=LocationState.S }, new StateValue() { X=1,Y=2,State=LocationState.S }, new StateValue() { X=2,Y=2,State=LocationState.S } }
		};
		public List<List<StateValue>> GetWinningPlays() { return WinningPlays; }
		

		Player _currentPlayer = Player.X;
		public Player CurrentPlayer
		{
			get
			{
				return _currentPlayer;
			}
		}
		public delegate void TurnHandler(TTTBoard board);
		public event TurnHandler TurnTaken;
		public void TakeTurn(int x, int y)
		{
			SetLocation(x, y, (CurrentPlayer == Player.X ? LocationState.X : LocationState.O));
			_currentPlayer = (CurrentPlayer == Player.X ? Player.O : Player.X);
			LastPlay = GetLocation(x, y);
			LastPlayer = (CurrentPlayer == Player.X ? Player.O : Player.X);
			TurnTaken(this);
		}
	}
}
