using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman
{
	public class Point
	{
		public int x;
		public int y;

		public Point()
		{
			this.x = -1;
			this.y = -1;
		}

		public Point(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public double GetDistanceTo(Point target)
		{
			int a = target.x - x;
			int b = target.y - y;
			return Math.Sqrt(a*a + y*y);
		}

		public bool IsValid()
		{
			return (x != -1) && (y != -1);
		}

		public override string ToString()
		{
			return x.ToString() + " " + y.ToString();
		}
	}
}
