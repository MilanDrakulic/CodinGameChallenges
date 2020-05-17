﻿using System;
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
		public static List<int> remainingPacs;

		//Used when we realize that there is a pac closer to already selected target that the pac it was selected for, and we want to switch these pacs - assing this already selected target to current/closer pac and keep searching for farther pac
		public static void SwapPacTargets(ref Pac previousOwner, ref Pac newOwner, Point target)
		{
			if (previousOwner.hasFixedTarget)
			{
				Console.Error.WriteLine("Cannot swap: Pac: " + previousOwner.id.ToString() + " has fixed target: " + previousOwner.currentTarget.ToString());
				return;
			}
			Console.Error.WriteLine("Swapping targets - Previous: " + previousOwner.id.ToString() + " New" + newOwner.id.ToString() + " Pellet:" + target.ToString());
			//if (newOwner.currentTarget == target)
			//{
			//	newOwner.isOnPath = false;
			//}
			//else
			//{
			//	newOwner.isOnPath = true;
			//}
			newOwner.currentTarget = target;
			previousOwner.currentTarget = null;
			//previousOwner.isOnPath = false;
			newOwner = previousOwner;
			return;
		}
	}
}
