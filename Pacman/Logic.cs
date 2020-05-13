using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman
{
	public interface IPacStrategy
	{
		Point GetTarget(ref Pac pac);
	}

	public static class StrategyPicker
	{
		public static Point GetTargetFromStrategy(ref Pac pac)
		{
			Point result = null;

			if (PelletController.BigPellets.Count > 0)
			{
				result = new GreedyStrategy().GetTarget(ref pac);
			}

			if ((result == null) && (PelletController.Pellets.Count > 0))
			{
				result = new VisibleStrategy().GetTarget(ref pac);
			}

			if ((result == null) && (Level.junctions.Count > 0))
			{
				result = new JunctionsStrategy().GetTarget(ref pac);
			}

			//if (pac.isOnHold)
			//{
			//	result = new JunctionsStrategy().GetTarget(ref pac);
			//}

			if (result == null)
			{
				result = new WaitStrategy().GetTarget(ref pac);
			}

			return result;
		}

		//public static IPacStrategy ChooseStrategy(Pac pac)
		//{
		//	IPacStrategy result = null;

		//	//TODO - add aditional strategy choices
		//	if (PelletController.BigPellets.Count > 0)
		//	{
		//		result = new GreedyStrategy();
		//	}

		//	if ((result == null) && (PelletController.Pellets.Count > 0))
		//	{
		//		result = new VisibleStrategy();
		//	}

		//	if ((result == null) && (Level.junctions.Count > 0))
		//	{
		//		result = new JunctionsStrategy();
		//	}

		//	if (pac.isOnHold)
		//	{ 
		//		result = new JunctionsStrategy();
		//	}

		//	if (result == null)
		//	{
		//		result = new WaitStrategy();
		//	}

		//	return result;
		//}
	}

	public class GreedyStrategy : IPacStrategy
	{
		public Point GetTarget(ref Pac pac)
		{
			if (pac.currentTarget != null && PelletController.ExistsAtPosition(pac.currentTarget) && !PacController.GetCurrentTargets().Values.Contains(pac.currentTarget))
			{
				return pac.currentTarget;
			}

			Point target = null;
			double minDistance = Double.MaxValue;
			double distance;

			if (PelletController.BigPellets.Count > 0)
			{

				foreach (Point bigPellet in PelletController.BigPellets)
				{
					if (PacController.GetCurrentTargets().Values.Contains(bigPellet))
					{
						Pac previousTargetOwner = PacController.GetPacWithCurrentTarget(bigPellet);
						if ((pac.origin.GetDistanceTo(bigPellet) < previousTargetOwner.origin.GetDistanceTo(bigPellet)))
						{
							Common.SwapPacTargets(ref previousTargetOwner, ref pac, bigPellet);
						}
						else
						{
							Console.Error.WriteLine("Pellet to skip:" + bigPellet.ToString());
						}
						continue;
					}
					distance = bigPellet.GetDistanceTo(pac.origin);
					if (distance < minDistance)
					{
						minDistance = distance;
						target = new Point(bigPellet);
					}
				}
			}

			Console.Error.WriteLine("PacId:" + pac.id + " Strategy: Greedy" + ((target == null) ? "null" : target.ToString()));
			return target;
		}
	}

	public class VisibleStrategy : IPacStrategy
	{
		public Point GetTarget(ref Pac pac)
		{
			if (pac.currentTarget != null && PelletController.ExistsAtPosition(pac.currentTarget) && !PacController.GetCurrentTargets().Values.Contains(pac.currentTarget))
			{
				Console.Error.WriteLine("Keeping current target:" + pac.currentTarget.ToString());
				return pac.currentTarget;
			}

			Point target = null;
			double minDistance = Double.MaxValue;
			double distance;

			if (PelletController.Pellets.Count > 0)
			{
				foreach (Point pellet in PelletController.Pellets)
				{
					if (PacController.GetCurrentTargets().Values.Contains(pellet))
					{
						Pac previousTargetOwner = PacController.GetPacWithCurrentTarget(pellet);
						if ((pac.origin.GetDistanceTo(pellet) < previousTargetOwner.origin.GetDistanceTo(pellet)))
						{
							Common.SwapPacTargets(ref previousTargetOwner, ref pac, pellet);
						}
						else
						{
							Console.Error.WriteLine("Pellet to skip:" + pellet.ToString());
						}
						continue;
					}
					distance = pellet.GetDistanceTo(pac.origin);
					if (distance < minDistance)
					{
						minDistance = distance;
						target = pellet;
					}
				}
			}

			Console.Error.WriteLine("PacId:" + pac.id + " Strategy: Visible " + ((target == null)? "null": target.ToString()));
			return target;
		}
	}

	public class JunctionsStrategy : IPacStrategy
	{
		public Point GetTarget(ref Pac pac)
		{
			Point target = null;
			double minDistance = Double.MaxValue;
			double distance;

			foreach (KeyValuePair<Point, int> junction in Level.junctions)
			{
				if (PacController.GetCurrentTargets().Values.Contains(junction.Key))
				{
					Pac previousTargetOwner = PacController.GetPacWithCurrentTarget(junction.Key);
					if ((pac.origin.GetDistanceTo(junction.Key) < previousTargetOwner.origin.GetDistanceTo(junction.Key)))
					{
						Common.SwapPacTargets(ref previousTargetOwner, ref pac, junction.Key);
					}
					else
					{
						Console.Error.WriteLine("Junction to skip:" + junction.Key.ToString());
					}
					continue;
				}

				if (junction.Key.hasVisiblePellets)
				{
					distance = pac.origin.GetDistanceTo(junction.Key);
					if ((distance < minDistance) && (distance > 0))
					{
						minDistance = distance;
						target = junction.Key;
					}
				}
			}

			Console.Error.WriteLine("PacId:" + pac.id + " Strategy: Junctions " + ((target == null) ? "null" : target.ToString()));
			return target;
		}
	}

	public class WaitStrategy : IPacStrategy
	{
		public Point GetTarget(ref Pac pac)
		{
			Console.Error.WriteLine("PacId:" + pac.id + " Strategy: Wait");
			return pac.origin;
		}
	}

	public static class Logic
	{

		public static void SetTarget(ref Pac pac)
		{
			Point target = StrategyPicker.GetTargetFromStrategy(ref pac);
			pac.currentTarget = target;
			//pac.currentTarget = StrategyPicker.ChooseStrategy(pac).GetTarget(pac);

			if (pac.currentTarget == null)
			{
				pac.currentTarget = new Point(pac.origin);
				Console.Error.WriteLine("Waiting!");
			}

			Console.Error.WriteLine("Pac: " + pac.id + " target: " + pac.currentTarget.ToString());
			return;
		}

		public static void MarkEmptyTiles()
		{
			Console.Error.WriteLine("Deleting junction usao");
			foreach (Pac pac in PacController.myPacs)
			{
				Console.Error.WriteLine("Deleting junction pac: " + pac.id.ToString() + pac.origin.ToString());
				if (Level.junctions.ContainsKey(pac.origin))
				{
					Console.Error.WriteLine("Deleting junction beore delete");
					Level.junctions.Remove(pac.origin);
					Console.Error.WriteLine("Deleted junction: " + pac.origin.ToString());
				}
			}
		}
	}
}
