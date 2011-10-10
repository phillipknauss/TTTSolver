using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TTTSolver_Lib
{
	public enum LocationState
	{
		U = 0,
		X = 1,
		O = 2,
		S = 3 // Used to specify a set value with an unknown X/O state
	}

	public enum Player
	{
		X = 1,
		O = 2
	}

	public static class EnumConverter
	{
		public static LocationState From(Player player)
		{
			switch (player)
			{
				case Player.X:
					return LocationState.X;
				case Player.O:
					return LocationState.O;
				default:
					return LocationState.U;
			}
		}
		public static Player? From(LocationState state)
		{
			switch (state)
			{
				case LocationState.X:
					return Player.X;
				case LocationState.O:
					return Player.O;
				default:
					return null;
			}
		}
	}
}
