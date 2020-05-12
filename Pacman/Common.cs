using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman
{
	public static class Common
	{
		public static readonly int SpeedCooldownDuration = 10;
		public static readonly int SwitchCooldownDuration = 10;

		public static int CurrentTurn = 0;
		public static Dictionary<int, Point> currentTargets = new Dictionary<int, Point>();
		public static List<int> remainingPacs;
	}
}
