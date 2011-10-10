using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TTTSolver_Lib;
using TTTSolver_Solver;

namespace WinTTT
{
	public partial class TTTGui : Form
	{
		TTTBoard Game;
		TTTSolver Solver;
		
		Dictionary<Point,PictureBox> locations;

		public TTTGui()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			locations = new Dictionary<Point,PictureBox>()
			{
				{ new Point(0,0), pic00 }, { new Point(0,1), pic01 }, { new Point(0,2), pic02 },
				{ new Point(1,0), pic10 }, { new Point(1,1), pic11 }, { new Point(1,2), pic12 },
				{ new Point(2,0), pic20 }, { new Point(2,1), pic21 }, { new Point(2,2), pic22 }
			};
			Normalize();
			NewGame();

			MessageBox.Show("Instructions: You know the rules. Current turn is displayed in the status bar.\nYou can use the menu item Solver > Take Next Turn to have the solver calculate the next turn.\nUse this to make the computer play itself, or play your choice of X or O");
		}

		void Normalize()
		{
			foreach (var l in locations)
			{
				l.Value.Tag = "";
				l.Value.Image = Image.FromFile("empty_label.gif");
				l.Value.Enabled = true;
			}
		}
		void NewGame()
		{
			Game = new TTTBoard();
			Game.TurnTaken += new TTTBoard.TurnHandler(Game_TurnTaken);
			Solver = new BruteForceTTTSolver(Game);
			//Solver = new MiniMaxSolver(Game);
			mainLabel.Text = Game.CurrentPlayer + "'s turn.";
		}

		void Game_TurnTaken(TTTBoard board)
		{
			UpdateFromGame(board.LastPlay.X, board.LastPlay.Y, Game.LastPlayer);
		}
		void Disable()
		{
			foreach (var l in locations)
			{
				l.Value.Enabled = false;
			}
		}
		void Enable()
		{
			foreach (var l in locations)
			{
				l.Value.Enabled = true;
			}
		}

		public void TakeTurn(int x, int y, PictureBox sender)
		{
			string tag = (((PictureBox)sender).Tag == null ? "" : ((PictureBox)sender).Tag.ToString());
			if (String.Equals(tag, "X") || String.Equals(tag, "O"))
			{
				MessageBox.Show("That position has already been taken.");
				return;
			}
			Game.TakeTurn(x, y);
		}

		public PictureBox BoxFromPlay(int x, int y)
		{
			string locStr = x.ToString() + y.ToString();
			switch (locStr)
			{
				case "00":
					return pic00;
				case "01":
					return pic01;
				case "02":
					return pic02;
				case "10":
					return pic10;
				case "11":
					return pic11;
				case "12":
					return pic12;
				case "20":
					return pic20;
				case "21":
					return pic21;
				case "22":
					return pic22;
				default:
					return null;
			}
		}

		public void UpdateBoardImages()
		{
			for (int x = 0; x < 3; x++)
			{
				for (int y = 0; y < 3; y++)
				{
					var loc = Game.GetLocation(x, y);
					switch (loc.State)
					{
						case LocationState.X:
							BoxFromPlay(x, y).Image = Image.FromFile("x.png");
							break;
						case LocationState.O:
							BoxFromPlay(x, y).Image = Image.FromFile("o.gif");
							break;
						default:
							BoxFromPlay(x, y).Image = Image.FromFile("empty_label.gif");
							break;
					}
				}
			}
			
		}

		public void UpdateFromGame(int x, int y, Player actingPlayer)
		{
			var sender = BoxFromPlay(x, y);
			
			if (sender == null)
			{
				sender = new PictureBox();
			}
			
			UpdateBoardImages();
			((PictureBox)sender).Tag = actingPlayer.ToString();
			if (Game.GameOver)
			{
				string endMessage = "Game Over.";
				if (Game.XWins)
				{
					endMessage += " X Wins!";
				}
				if (Game.OWins)
				{
					endMessage += " O Wins!";
				}
				mainLabel.Text = endMessage;
				MessageBox.Show(endMessage);
				Disable();
				return;
			}

			mainLabel.Text = Game.CurrentPlayer + "'s turn.";
			serializedText.Text = Game.SerializeWeights();
		}
		PictureBox lastBox = null;
		Player lastPlayer = Player.X;
		private void pic_Click(object sender, EventArgs e)
		{
			string boxName = ((PictureBox)sender).Name;
			int x = -1;
			int y = -1;
			Int32.TryParse(boxName[boxName.Length - 2].ToString(), out x);
			Int32.TryParse(boxName[boxName.Length - 1].ToString(), out y);
			if (x < 0 || y < 0) { return; }
			lastPlayer = Game.CurrentPlayer;
			lastBox = (PictureBox)sender;
			TakeTurn(x, y, lastBox);
		}

		private void toolStripTextBox1_Click(object sender, EventArgs e)
		{

		}

		public void NormalizeAndNewGame()
		{
			Normalize();
			NewGame();
		}

		private void newGameToolStripMenuItem_Click(object sender, EventArgs e)
		{
			NormalizeAndNewGame();
		}

		private void takeNextTurnToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (!Game.GameOver)
			{
				Solver.PlayNextMove();
			}
		}
	}
}
