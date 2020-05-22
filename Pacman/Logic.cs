using System;
using System.CodeDom;
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
			Point result;

			result = new AttackStrategy().GetTarget(ref pac);
			if (result != null && pac.inPursuit)
			{
				return result;
			}
			else
			{
				pac.inPursuit = false;
			}

			result = new MimicStrategy().GetTarget(ref pac);

			if ((result == null) && (PelletController.BigPellets.Count > 0))
			{
				result = new GreedyStrategy().GetTarget(ref pac);
				result = pac.ResetTargetIfStationary(result);
			}

			if ((result == null) && (PelletController.Pellets.Count > 0))
			{
				result = new VisibleStrategy().GetTarget(ref pac);
				result = pac.ResetTargetIfStationary(result);
			}

			if ((result == null) && (Level.junctions.Count > 0))
			{
				result = new JunctionsStrategy().GetTarget(ref pac);
				result = pac.ResetTargetIfStationary(result);
			}

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

	public class AttackStrategy : IPacStrategy
	{
		public Point GetTarget(ref Pac pac)
		{
			Point target = null;
			Pac enemyPac = PacController.GetClosestEnemy(pac);
			//Pac enemyPac = PacController.GetClosestVisibleEnemy(pac, true);
			int distanceToEnemy;

			if (enemyPac != null)
			{
				distanceToEnemy = pac.origin.GetDistanceTo(enemyPac.origin);
				PacType strongerThanMe = Common.GetStrongerPacType(pac.pacType);
				PacType strongerThanHim = Common.GetStrongerPacType(enemyPac.pacType);

				Console.Error.WriteLine("ATTACK! pacType" + pac.pacType.ToString() + " strongerThanHim:" + strongerThanHim.ToString() + " distance:" + distanceToEnemy.ToString());
				if (pac.pacType == strongerThanHim && distanceToEnemy <= 6)
				{
					pac.shouldActivateSpeed = true;
					pac.inPursuit = true;
					target = enemyPac.origin;
				}
				else
				{
					if (pac.cooldown > (enemyPac.speedTurnsLeft > 0 ? distanceToEnemy / 2 : distanceToEnemy))
					{ 
						//Bezanija!!!
						//Level.GetClosestJunctionInDirection();
					}
				}


			}
			Console.Error.WriteLine("PacId:" + pac.id + " Strategy: Attack " + ((target == null) ? "null" : target.ToString()) + " inPursuit:" + pac.inPursuit.ToString());
			return target;
		}
	}

	public class MimicStrategy : IPacStrategy
	{
		public Point GetTarget(ref Pac pac)
		{
			Console.Error.WriteLine("Mimic strategy start");
			Point target = null;
			Pac enemyPac = PacController.GetClosestVisibleEnemy(pac, true);

			int distance = Common.minDistanceForSwitch;
			if (enemyPac != null)
			{
				if (enemyPac.speedTurnsLeft > 0)
				{
					if (pac.speedTurnsLeft > 0)
					{
						distance = distance + 2;
					}
					else
					{
						distance = distance + 1;
					}
				}
			}

			if (enemyPac != null && pac.origin.GetDistanceTo(enemyPac.origin) <= distance)
			{
				PacType strongerType = Common.GetStrongerPacType(enemyPac.pacType);
				if (pac.pacType != strongerType)
				{
					pac.shouldActivateSwitch = true;
					pac.switchTo = strongerType;
				}
			}
			else 
			{
				return null;
			}

			target = enemyPac.origin;

			Console.Error.WriteLine("PacId:" + pac.id + " Strategy: Mimic " + ((target == null) ? "null" : target.ToString()));
			return target;
		}
	}

	public static class Logic
	{
		public static void SetTargets()
		{
			Console.Error.WriteLine("SetTargets, mypacs count:" + PacController.myPacs.Count().ToString());
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
			pac.currentTarget = target;

			if (pac.currentTarget == null)
			{
				pac.currentTarget = new Point(pac.origin);
				pac.isOnHold = true;
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
				pac.currentTarget = null;
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
			for (int i = 0; i < PacController.myPacs.Count; i++)
			{
				Pac pac = PacController.myPacs[i];
				if (pac.isAlive)
				{
					//if it is on path, target hasn't changed and there is no collision, just proceed
					if (pac.isOnPath && (pac.currentTarget == pac.previousTarget) && (pac.origin != pac.previousOrigin))
					{
						pac.indexOnPath++;
					}
					else
					{
						//if (pac.isOnHold || pac.isInCollision)
						//{
						//	pac.isOnPath = false;
						//	pac.previousTarget = pac.currentTarget;
						//	//pac.currentTarget = null;
						//	continue;
						//}

						List<Point> path;
						if (FindPath(pac, new Node(pac.origin), new Node(pac.currentTarget), out path))
						{
							if (path.Count > 0)
							{
								SetPath(pac, path);
							}
							else
							{
								pac.isOnPath = false;
								pac.isOnHold = true;
							}
						}
						else
						{
							pac.isOnPath = false;
							pac.isOnHold = true;							
						}
					}

				}
			}
		}

		public static bool FindPath(Pac pac, Node start, Node end, out List<Point> path)
		{
			Console.Error.WriteLine("Pathfinding started! pac:" + pac.id.ToString());
			List <Point> obstacles = MarkObstacles(pac, end);
			path = null;

			try
			{
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
					//Console.Error.WriteLine("Pathfinding currentNode: " + currentNode.ToString());

					openSet.Remove(currentNode);
					closedSet.Add(currentNode);

					if (currentNode == end)
					{
						path = CalculatePath(ref pac, start, currentNode);
						Console.Error.WriteLine("Found path: pac:" + pac.id + " start:" + start.ToString() + " end:" + currentNode.ToString() + " distance: " + path.Count.ToString());
						return true;
					}

					List<Point> neighbours = Level.GetNeighbours(currentNode.x, currentNode.y);
					foreach (Point neighbour in neighbours)
					{
						//Console.Error.WriteLine("Pathfinding neighbour: " + neighbour.ToString());
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

				return false;
			}
			finally
			{
				UnmarkObstacles(obstacles);
			}
		}

		public static void SetPath(Pac pac, List<Point> path)
		{
			pac.previousTarget = pac.currentTarget;
			pac.isOnPath = true;
			pac.isOnHold = false;
			pac.indexOnPath = 0;
			pac.distanceToTarget = path.Count;
			pac.path = path;
		}
		
		public static List<Point> CalculatePath(ref Pac pac, Node start, Node end)
		{
			List<Point> path = new List<Point>();

			Node currentNode = end;
			while (currentNode != start)
			{
				path.Add(currentNode as Point);
				currentNode = currentNode.parent;
				//Console.Error.WriteLine("Added to path: " + currentNode.ToString());
			}

			path.Reverse();
			return path;
		}

		//Not thread safe
		public static List<Point> MarkObstacles(Pac pac, Point target)
		{
			List<Point> result = new List<Point>();
			Point pacDirection = PacController.GetDirectionSigns(pac.origin, target);

			foreach (Pac otherPac in PacController.myPacs)
			{
				if (pac.id == otherPac.id || !otherPac.isAlive || otherPac.previousOrigin == null)
				{
					continue;
				}
				Point otherPacDirection = PacController.GetDirectionSigns(otherPac.previousOrigin, otherPac.origin);
				if (PacController.HaveOppositeDirection(pac, otherPac, target))
				{
					Console.Error.WriteLine("Obstacle found:" + otherPac.origin.ToString());
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
	}
}
