using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman
{
	public interface IPacStrategy
	{
		Point GetTarget(Pac pac, CurrentTarget currentTarget);
	}

	public static class StrategyPicker
	{
		public static IPacStrategy ChooseStrategy(Pac pac, CurrentTarget currentTarget)
		{
			IPacStrategy result = null;

			//TODO - add aditional strategy choices
			if (PelletController.BigPellets.Count > 0)
			{
				result = new GreedyStrategy();
			}

			if ((result == null) && (PelletController.Pellets.Count > 0))
			{
				result = new VisibleStrategy();
			}

			if ((result == null) && (Level.junctions.Count > 0))
			{
				result = new JunctionsStrategy();
			}

			if (currentTarget.onHold)
			{ 
				result = new JunctionsStrategy();
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
		public Point GetTarget(Pac pac, CurrentTarget currentTarget)
		{
			if (currentTarget.point.IsValid() && PelletController.ExistsAtPosition(currentTarget.point) && !Common.currentTargets.Values.Contains(currentTarget.point))
			{
				return currentTarget.point;
			}

			Point target = null;
			double minDistance = Double.MaxValue;
			double distance;

			if (PelletController.BigPellets.Count > 0)
			{

				foreach (Point bigPellet in PelletController.BigPellets)
				{
					if (Common.currentTargets.Values.Contains(bigPellet))
					{
						Console.Error.WriteLine("Pellet to skip:" + bigPellet.ToString());
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

			Console.Error.WriteLine("PacId:" + pac.Id + " Strategy: Greedy" + ((target == null) ? "null" : target.ToString()));
			return target;
		}
	}

	public class VisibleStrategy : IPacStrategy
	{
		public Point GetTarget(Pac pac, CurrentTarget currentTarget)
		{
			if (currentTarget.point.IsValid() && PelletController.ExistsAtPosition(currentTarget.point) && !Common.currentTargets.Values.Contains(currentTarget.point))
			{
				Console.Error.WriteLine("Keeping current target:" + currentTarget.ToString());
				return currentTarget.point;
			}

			Point target = null;
			double minDistance = Double.MaxValue;
			double distance;

			if (PelletController.Pellets.Count > 0)
			{
				foreach (Point pellet in PelletController.Pellets)
				{
					if (Common.currentTargets.Values.Contains(pellet))
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

	public class JunctionsStrategy : IPacStrategy
	{
		public Point GetTarget(Pac pac, CurrentTarget currentTarget)
		{
			Console.Error.WriteLine("PacId:" + pac.Id + " Strategy: Junctions");

			return GetNearestJunction(pac.Origin);
		}

		public Point GetNearestJunction(Point origin)
		{
			Point result = null;
			double minDistance = Double.MaxValue;
			double distance;

			foreach (KeyValuePair<Point, int> junction in Level.junctions)
			{
				if (Common.currentTargets.Values.Contains(junction.Key))
				{
					Console.Error.WriteLine("Pellet to skip:" + junction.Key.ToString());
					continue;
				}

				if (junction.Key.hasVisiblePellets)
				{
					distance = origin.GetDistanceTo(junction.Key);
					if (distance < minDistance)
					{
						minDistance = distance;
						result = junction.Key;
					}
				}
			}

			return result;
		}
	}

	public class WaitStrategy : IPacStrategy
	{
		public Point GetTarget(Pac pac, CurrentTarget currentTarget)
		{
			Console.Error.WriteLine("PacId:" + pac.Id + " Strategy: Wait");
			return pac.Origin;
		}
	}

	public class CurrentTarget
	{
		public Point point;
		public bool onHold;

		public CurrentTarget()
		{
			this.point = new Point();
		}

		public CurrentTarget(Point point)
		{
			this.point = point;
		}

		public CurrentTarget(Point point, bool onHold)
		{
			this.point = point;
			this.onHold = onHold;
		}
	}

	public static class Logic
	{
		public static Dictionary<int, CurrentTarget> CurrentTargets = new Dictionary<int, CurrentTarget>();

		public static void AddCurrentTargetIfNeeded(Pac pac)
		{
			if (!CurrentTargets.ContainsKey(pac.Id))
			{
				CurrentTargets.Add(pac.Id, new CurrentTarget());
				Console.Error.WriteLine("Added Target! Id: " + pac.Id.ToString());
			}
		}

		public static void SetTarget(Pac pac)
		{
			AddCurrentTargetIfNeeded(pac);
			Point targetPoint = StrategyPicker.ChooseStrategy(pac, CurrentTargets[pac.Id]).GetTarget(pac, CurrentTargets[pac.Id]);
			if (targetPoint == null)
			{
				targetPoint = new JunctionsStrategy().GetTarget(pac, CurrentTargets[pac.Id]);
			}

			CurrentTarget currentTarget = null;

			if (targetPoint == null)
			{
				targetPoint = new Point(pac.Origin.x, pac.Origin.y);
				currentTarget = new CurrentTarget(targetPoint, true);
				Console.Error.WriteLine("Waiting!");
			}
			else
			{
				currentTarget = new CurrentTarget(targetPoint, false);
			}

			pac.currentTarget = currentTarget.point;

			CurrentTargets[pac.Id] = currentTarget;
			Common.currentTargets.Add(pac.Id, targetPoint);
			Console.Error.WriteLine("New target: " + CurrentTargets[pac.Id].point.ToString());

			return;
		}

		public static void MarkEmptyTiles()
		{
			Console.Error.WriteLine("Deleting junction usao");
			foreach (Pac pac in PacController.myPacs)
			{
				Console.Error.WriteLine("Deleting junction pac: " + pac.Id.ToString() + pac.Origin.ToString());
				if (Level.junctions.ContainsKey(pac.Origin))
				{
					Console.Error.WriteLine("Deleting junction beore delete");
					Level.junctions.Remove(pac.Origin);
					Console.Error.WriteLine("Deleted junction: " + pac.Origin.ToString());
				}
			}
		}

		//Used when we realize that there is a pac closer to already selected target that the pac it was selected for, and we want to switch these pacs - assing this already selected target to current/closer pac and keep searching for farther pac
		public static void SwapPacTargets(Pac closer, Pac farther, Point target)
		{ 
			
		}
	}
}
