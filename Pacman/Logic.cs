using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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
	}

	public class GreedyStrategy : IPacStrategy
	{
		public Point GetTarget(ref Pac pac)
		{
			if (pac.currentTarget != null && PelletController.ExistsAtPosition(pac.currentTarget))// && !PacController.GetCurrentTargets().Values.Contains(pac.currentTarget))
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
			if (pac.currentTarget != null && PelletController.ExistsAtPosition(pac.currentTarget))// && !PacController.GetCurrentTargets().Values.Contains(pac.currentTarget))
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

			if (pac.currentTarget != null && pac.currentTarget.hasVisiblePellets)// && !PacController.GetCurrentTargets().Values.Contains(pac.currentTarget))
			{
				Console.Error.WriteLine("Keeping current target:" + pac.currentTarget.ToString());
				return pac.currentTarget;
			}

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
					if ((distance < minDistance) && (distance > 0.1))
					{
						minDistance = distance;
						target = junction.Key;
						Console.Error.WriteLine("Junction target:" + junction.Key.ToString() + " distance: " + distance.ToString());
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
			pac.isOnHold = true;
			return pac.origin;
		}
	}

	public static class Logic
	{
		public static void SetTargets()
		{
			for (int i = 0; i < PacController.myPacs.Count; i++)
			{
				Pac pac = PacController.myPacs[i];
				if (pac.isAlive)
				{
					Logic.SetTarget(ref pac);
				}
			}
		}

		public static void SetTarget(ref Pac pac)
		{
			CheckArrivalToTarget(ref pac);

			Point target = StrategyPicker.GetTargetFromStrategy(ref pac);
			if (pac.currentTarget == null)
			{
				Console.Error.WriteLine("Current target is null: pac id" + pac.id.ToString());

			}
			else
			{
				Console.Error.WriteLine("Current target:" + pac.currentTarget.ToString() + " target:" + target.ToString());

			}

			if (pac.currentTarget != null && target.Equals(pac.currentTarget))
			{
				Console.Error.WriteLine("Targets are EQUAL!!!");
				if (pac.isInCollision)
				{
					Console.Error.WriteLine("Collision! pac id:" + pac.id.ToString());
					FindPath(pac, new Node(pac.origin), new Node(target));
				}
				else
				{
					if (pac.isOnPath)
					{
						Console.Error.WriteLine("Incrementing index! pac id:" + pac.id.ToString());
						pac.indexOnPath++;
					}
				}
			}
			else
			{
				FindPath(pac, new Node(pac.origin), new Node(target));
				pac.previousTarget = pac.currentTarget;
				pac.currentTarget = target;
			}

			if (pac.currentTarget == null)
			{
				pac.currentTarget = new Point(pac.origin);
				Console.Error.WriteLine("Waiting!");
			}

			Console.Error.WriteLine("Pac: " + pac.id + " target: " + pac.currentTarget.ToString());
			return;
		}

		public static void CheckArrivalToTarget(ref Pac pac)
		{
			if (pac.isOnPath && pac.origin.Equals(pac.currentTarget))
			{
				Console.Error.WriteLine("Arrival! Pac: " + pac.id + " target: " + pac.currentTarget.ToString());
				pac.isOnPath = false;
				pac.hasFixedTarget = false;
				pac.indexOnPath = -1;
				pac.distanceToTarget = 0;
			}
		}

		public static void MarkEmptyTiles()
		{
			foreach (Pac pac in PacController.myPacs)
			{
				if (!pac.isAlive)
				{
					continue;
				}
				if (Level.junctions.ContainsKey(pac.origin))
				{
					Level.junctions.Remove(pac.origin);
					Console.Error.WriteLine("Deleted junction: " + pac.origin.ToString());
				}
			}
		}

		public static void FindPaths()
		{ 
		
		}

		public static void FindPath(Pac pac, Node start, Node end)
		{
			List<Point> obstacles = MarkObstacles(pac, end);

			List<Node> openSet = new List<Node>();
			HashSet<Node> closedSet = new HashSet<Node>();

			start.gCost = 0;
			start.hCost = start.GetDistanceTo(end);
			openSet.Add(start);

			while (openSet.Count() > 0)
			{
				Node currentNode = openSet[0];
				for (int i = 1; i < openSet.Count(); i++)
				{
					if (openSet[i].fCost < currentNode.fCost || (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
					{
						currentNode = openSet[i];
					}
				}

				openSet.Remove(currentNode);
				closedSet.Add(currentNode);

				if (currentNode.Equals(end))
				{
					MemorizePath(ref pac, start, currentNode);
					Console.Error.WriteLine("Found path: pac:" + pac.id + " start:" + start.ToString() + " end:" + currentNode.ToString() + " distance: " + pac.distanceToTarget.ToString());
					return;
				}

				List<Point> neighbours = Level.GetNeighbours(currentNode.x, currentNode.y);
				foreach (Point neighbour in neighbours)
				{
					//Check if this works - Equals on point comparing only x and y so that this works when Point passed as a parameter although collection contains nodes
					if (!closedSet.Contains(neighbour))
					{

						Node node;
						if (!openSet.Contains(neighbour))
						{
							node = new Node(neighbour);
							openSet.Add(node);
						}
						else 
						{
							node = openSet.Find(a => a.x == neighbour.x && a.y == neighbour.y);
						}
						node.gCost = currentNode.gCost + 1;
						node.hCost = node.GetDistanceTo(end);
						node.parent = currentNode;
					}
				}

			}

			UnmarkObstacles(obstacles);
			return;
		}

		public static void MemorizePath(ref Pac pac, Node start, Node end)
		{
			List<Point> path = new List<Point>();
			int distance = 0;

			Node currentNode = end;
			while (currentNode != start)
			{
				distance++;
				path.Add(currentNode as Point);
				currentNode = currentNode.parent;
				Console.Error.WriteLine("Added to path: " + currentNode.ToString());
			}

			path.Reverse();
			pac.isOnPath = true;
			pac.indexOnPath = 0;
			pac.distanceToTarget = distance;
			pac.path = path;
		}

		//Not thread safe
		public static List<Point> MarkObstacles(Pac pac, Point target)
		{
			List<Point> result = new List<Point>();
			Point pacDirection = GetDirectionSigns(pac.origin, target);

			foreach (Pac otherPac in PacController.myPacs)
			{
				if (pac.id == otherPac.id || !otherPac.isAlive || otherPac.previousOrigin == null)
				{
					continue;
				}
				Point otherPacDirection = GetDirectionSigns(otherPac.previousOrigin, otherPac.origin);
				if (ShouldTreatPacAsObstacle(pac, otherPac, target))
				{
					result.Add(otherPac.origin);
					Level.map[otherPac.origin.y, otherPac.origin.x] = -1;
				}
			}

			return result;
		}

		//Not thread safe
		public static void UnmarkObstacles(List<Point> obstacles)
		{
			foreach (Point tile in obstacles)
			{
				Level.map[tile.y, tile.x] = 1;
			}
		}

		public static Point GetDirectionSigns(Point start, Point end)
		{
			int xSign = Math.Sign(end.x - start.x);
			int ySign = Math.Sign(end.y - start.y);
			return new Point(xSign, ySign);
		}

		public static bool ShouldTreatPacAsObstacle(Pac myPac, Pac otherPac, Point myPacTarget = null)
		{
			Point myPacDirection;
			Point otherPacDirection;
			if (myPacTarget != null)
			{
				myPacDirection = GetDirectionSigns(myPac.origin, myPacTarget);
			}
			else
			{
				//Special case for first turn
				if (myPac.previousOrigin == null)
				{
					return false;
					//myPacDirection = new Point(0, 0);
				}
				else
				{ 
					myPacDirection = GetDirectionSigns(myPac.previousOrigin, myPac.origin);
				}
			}

			//Special case for first turn
			if (otherPac.previousOrigin == null)
			{
				return false;
				//otherPacDirection = new Point(0, 0);
			}
			else
			{
				otherPacDirection = GetDirectionSigns(otherPac.previousOrigin, otherPac.origin);
			}

			//either other pac is stationary, or distance in x or y between two pacs is getting smaller
			bool result =	(otherPacDirection.x == 0 && otherPacDirection.y == 0) || 
							((myPacDirection.x == -otherPacDirection.x) && Math.Abs(myPac.origin.x + myPacDirection.x - otherPac.origin.x - otherPacDirection.x) < Math.Abs(myPac.origin.x - otherPac.origin.x)) ||
							((myPacDirection.y == -otherPacDirection.y) && Math.Abs(myPac.origin.y + myPacDirection.y - otherPac.origin.y - otherPacDirection.y) < Math.Abs(myPac.origin.y - otherPac.origin.y));
			return result;
		}
	}
}
