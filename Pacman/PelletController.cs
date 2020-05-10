using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman
{
	public static class PelletController
	{
		public static List<Point> Pellets = new List<Point>();
		public static List<Point> BigPellets = new List<Point>();

		public static void AddPellet(Point pellet)
		{
			Pellets.Add(pellet);
		}

		public static void AddPellet(int x, int y)
		{
			Point pellet = new Point(x, y);
			Pellets.Add(pellet);
		}


		public static void AddBigPellet(Point bigPellet)
		{
			BigPellets.Add(bigPellet);
		}

		public static void AddBigPellet(int x, int y)
		{
			Point bigPellet = new Point(x, y);
			BigPellets.Add(bigPellet);
		}

		public static void ClearPellets()
		{
			if (Pellets != null && Pellets.Count > 0)
			{
				Pellets.Clear();
			}
			if (BigPellets != null && BigPellets.Count > 0)
			{
				BigPellets.Clear();
			}
		}

		public static bool ExistsAtPosition(Point position)
		{
			return BigPellets.Contains(position);
			//return BigPellets.Where(a => a.x == position.x && a.y == position.y).Any() || Pellets.Where(a => a.x == position.x && a.y == position.y).Any();
		}

		//For later use - calculation of quadrant densities relative to origin 
		public static float[] GetDensityScores(Point origin)
		{
			float[] densities = new float[4];



			return densities;
		}
	}
}
