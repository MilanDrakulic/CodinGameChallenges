using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman
{
	public interface IPacStrategy
	{
		Point GetTarget(Pac pac, Point currentTarget);
	}

	public static class StrategyPicker
	{
		public static IPacStrategy ChooseStrategy(Pac pac)
		{
			IPacStrategy result = null;

			//TODO - add aditional strategy choices
			if (PelletController.BigPellets.Count > 0)
			{
				result = new GreedyStrategy();
			}
			else
			{
				if (PelletController.Pellets.Count > 0)
				{
					result = new VisibleStrategy();
				}
			}

			if (result == null)
			{
				result = new WaitStrategy();
			}

			return result;
		}
	}

	public class GreedyStrategy : IPacStrategy
	{
		public Point GetTarget(Pac pac, Point currentTarget)
		{
			if (currentTarget.IsValid() && PelletController.ExistsAtPosition(currentTarget) && !Common.selectedTargets.Contains(currentTarget))
			{
				return currentTarget;
			}

			Point target = null;
			double minDistance = Double.MaxValue;
			double distance;

			if (PelletController.BigPellets.Count > 0)
			{

				foreach (Point bigPellet in PelletController.BigPellets)
				{
					if (Common.selectedTargets.Contains(bigPellet))
					{
						continue;
					}
					distance = bigPellet.GetDistanceTo(pac.Origin);
					if (distance < minDistance)
					{
						minDistance = distance;
						target = new Point(bigPellet);
					}
				}
			}

			Console.Error.WriteLine("PacId:" + pac.Id + " Strategy: Greedy");
			return target;
		}
	}

	public class VisibleStrategy : IPacStrategy
	{
		public Point GetTarget(Pac pac, Point currentTarget)
		{
			if (currentTarget.IsValid() && PelletController.ExistsAtPosition(currentTarget) && !Common.selectedTargets.Contains(currentTarget))
			{
				Console.Error.WriteLine("Keeping current target:" + currentTarget.ToString());
				return currentTarget;
			}

			Point target = null;
			double minDistance = Double.MaxValue;
			double distance;

			if (PelletController.Pellets.Count > 0)
			{
				foreach (Point pellet in PelletController.Pellets)
				{
					if (Common.selectedTargets.Contains(pellet))
					{
						Console.Error.WriteLine("Pellet to skip:" + pellet.ToString());
						continue;
					}
					distance = pellet.GetDistanceTo(pac.Origin);
					if (distance < minDistance)
					{
						minDistance = distance;
						target = pellet;
					}
				}
			}

			Console.Error.WriteLine("PacId:" + pac.Id + " Strategy: Visible " + ((target == null)? "null": target.ToString()));
			return target;
		}
	}

	public class WaitStrategy : IPacStrategy
	{
		public Point GetTarget(Pac pac, Point currentTarget)
		{
			Console.Error.WriteLine("PacId:" + pac.Id + " Strategy: Wait");
			return pac.Origin;
		}
	}

	public static class Logic
	{
		public static Dictionary<int, Point> CurrentTargets = new Dictionary<int, Point>();

		public static void AddCurrentTargetIfNeeded(Pac pac)
		{
			if (!CurrentTargets.ContainsKey(pac.Id))
			{
				CurrentTargets.Add(pac.Id, new Point());
				Console.Error.WriteLine("Added Target! Id: " + pac.Id.ToString());
			}
		}

		public static void SetTarget(Pac pac)
		{
			AddCurrentTargetIfNeeded(pac);
			Point target = StrategyPicker.ChooseStrategy(pac).GetTarget(pac, CurrentTargets[pac.Id]);

			if (target == null)
			{
				target = new Point(pac.Origin.x, pac.Origin.y);
				Console.Error.WriteLine("Waiting!");
			}

			if (target.IsValid())
			{
				foreach (Point selectedTarget in Common.selectedTargets)
				{
					Console.Error.WriteLine("Already selected: " + selectedTarget.ToString());
				}
				Common.selectedTargets.Add(target);
				CurrentTargets[pac.Id] = target;
				Console.Error.WriteLine("New target: " + CurrentTargets[pac.Id].ToString());
			}

			return;
		}

		public static void MarkEmptyTiles()
		{
			foreach (Pac pac in PacController.myPacs)
			{
				Level.junctions.Remove(pac.Origin);
			}
		}
	}
}
