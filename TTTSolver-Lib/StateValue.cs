using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TTTSolver_Lib
{
	public class StateValue
	{
		public int X { get; set; }
		public int Y { get; set; }
		public LocationState State { get; set; }

		public int Weight { get; set; }

		public override string ToString()
		{
			return X.ToString() + Y.ToString() + State.ToString();
		}
	}
}
