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
		public bool hasVisiblePellets = true;

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

		public Point(Point point)
		{
			this.x = point.x;
			this.y = point.y;
		}

		public override bool Equals(object obj)
		{
			Point point = obj as Point;
			return point.x == this.x && point.y == this.y;
		}

		public override int GetHashCode()
		{
			unchecked // Overflow is fine, just wrap
			{
				int hash = 17;
				// Suitable nullity checks etc, of course :)
				hash = hash * 23 + x.GetHashCode();
				hash = hash * 23 + y.GetHashCode();
				return hash;
			}
		}

		public double GetDistanceTo(Point target)
		{
			int a = target.x - x;
			int b = target.y - y;
			//Euclidian
			return Math.Sqrt(a*a + b*b);

			////Manhattan
			//return Math.Abs(a) + Math.Abs(b);
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
