using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Player
{
        static void Main(string[] args)
    {
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        int width = int.Parse(inputs[0]); // size of the grid
        int height = int.Parse(inputs[1]); // top left corner is (x=0, y=0)
        string[] levelRows = new string[height];

        for (int i = 0; i < height; i++)
        {
            string row = Console.ReadLine(); // one line of the grid: space " " is floor, pound "#" is wall
            levelRows[i] = row;
        }
        Level.InitializeLevel(width, height);
        Level.StringsToMatrix(levelRows);
        Level.CalculateJunctions();
        //Level.PrintJunctions();

        // game loop
        while (true)
        {
            Common.CurrentTurn++;
            //PacController.ClearCurrentTargets();
            //Common.currentTargets.Clear();

            inputs = Console.ReadLine().Split(' ');
            int myScore = int.Parse(inputs[0]);
            int opponentScore = int.Parse(inputs[1]);
            int visiblePacCount = int.Parse(Console.ReadLine()); // all your pacs and enemy pacs in sight

            PacController.ClearPacs();
            for (int i = 0; i < visiblePacCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int pacId = int.Parse(inputs[0]); // pac number (unique within a team)
                bool mine = inputs[1] != "0"; // true if this pac is yours
                int x = int.Parse(inputs[2]); // position in the grid
                int y = int.Parse(inputs[3]); // position in the grid
                string typeId = inputs[4]; // unused in wood leagues
                int speedTurnsLeft = int.Parse(inputs[5]); // unused in wood leagues
                int abilityCooldown = int.Parse(inputs[6]); // unused in wood leagues

                PacController.AddPac(pacId, x, y, typeId, mine, speedTurnsLeft, abilityCooldown);
            }
            PacController.SyncPacs();
            //PacController.DetectCollisions();

            PelletController.ClearPellets();

            int visiblePelletCount = int.Parse(Console.ReadLine()); // all pellets in sight
            Common.WriteLine(9, "Visible pellets: " + visiblePelletCount.ToString());
            if (visiblePelletCount == 0)
            {
                Logic.MarkEmptyTiles();
            }
                
            for (int i = 0; i < visiblePelletCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int x = int.Parse(inputs[0]);
                int y = int.Parse(inputs[1]);
                int value = int.Parse(inputs[2]); // amount of points this pellet is worth

                if (value > 1)
                {
                    PelletController.AddBigPellet(x, y);
                }
                else
                {
                    PelletController.AddPellet(x, y);
                }
            }

            Logic.SetTargets();
            Logic.FindPaths();

            List<Point> targets = new List<Point>();

            string output = "";
            for (int i = 0; i < PacController.myPacs.Count; i++)
            {
                Pac pac = PacController.myPacs[i];
                if (!pac.isAlive)
                {
                    continue;
                }

                Point target;
                if (pac.isOnPath)
                {
                    Common.WriteLine(9, "Pac on path: id: " + pac.id.ToString() + " index:" + pac.indexOnPath + " target:" + pac.currentTarget.ToString() + " distance:" + pac.distanceToTarget.ToString());
                    target = pac.path[pac.indexOnPath];
                }
                else
                {
                    target = pac.currentTarget;
                }

                if (targets.Contains(target))
                {
                    Pac otherPac = PacController.GetPacWithCurrentTarget(target);
                    if (otherPac != null && PacController.myPacs[i].origin.GetDistanceTo(otherPac.origin) <= 2)
                    {
                        target = PacController.myPacs[i].previousTarget;
                    }
				}
                else
                {
                    targets.Add(target);
                }

                string command = "";
                if (pac.cooldown == 0 && pac.shouldActivateSwitch)
                {
                    command = "SWITCH " + pac.id.ToString() + " " + pac.switchTo.ToString();
                }
                else
                {
                    if (pac.cooldown == 0)// && pac.shouldActivateSpeed)
                    {
                        command = "SPEED " + pac.id.ToString();
                    }
                    else
                    {
                        if (target != null)
                        {
                            command = "MOVE " + pac.id.ToString() + " " + target.ToString();
                        }
                    }
                }
                output += (output == "") ? "" : "|";
                output += command;
            }

            Console.WriteLine(output);

        }
    }
}

public interface IPacStrategy
{
	Point GetTarget(ref Pac pac);
}

public static class StrategyPicker
{
	public static Point GetTargetFromStrategy(ref Pac pac)
	{
		Point result = null;

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
			Common.WriteLine(7, "Greedy - Keeping current target:" + pac.currentTarget.ToString());
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
						Common.WriteLine(6, "Pellet to skip:" + bigPellet.ToString());
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

		if (target != null)
		{
			Common.WriteLine(8, "PacId:" + pac.id + " Strategy: Greedy" + ((target == null) ? "null" : target.ToString()));
		}
		return target;
	}
}

public class VisibleStrategy : IPacStrategy
{
	public Point GetTarget(ref Pac pac)
	{
		if (pac.currentTarget != null && PelletController.ExistsAtPosition(pac.currentTarget))// && !PacController.GetCurrentTargets().Values.Contains(pac.currentTarget))
		{
			Common.WriteLine(7, "Visible - Keeping current target:" + pac.currentTarget.ToString());
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
						Common.WriteLine(6, "Pellet to skip:" + pellet.ToString());
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
		if (target != null)
		{
			Common.WriteLine(8, "PacId:" + pac.id + " Strategy: Visible " + ((target == null) ? "null" : target.ToString()));
		}
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
			Common.WriteLine(7, "Junction - Keeping current target:" + pac.currentTarget.ToString());
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
					Common.WriteLine(3, "Junction to skip:" + junction.Key.ToString());
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
					Common.WriteLine(4, "Junction target:" + junction.Key.ToString() + " distance: " + distance.ToString());
				}
			}
		}

		if (target != null)
		{
			Common.WriteLine(8, "PacId:" + pac.id + " Strategy: Junctions " + ((target == null) ? "null" : target.ToString()));
		}
		return target;
	}
}

public class WaitStrategy : IPacStrategy
{
	public Point GetTarget(ref Pac pac)
	{
		Common.WriteLine(8, "PacId:" + pac.id + " Strategy: Wait");
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
		int distanceToEnemy = 0;

		if (enemyPac != null)
		{
			distanceToEnemy = pac.origin.GetDistanceTo(enemyPac.origin);
			PacType strongerThanMe = Common.GetStrongerPacType(pac.pacType);
			PacType strongerThanHim = Common.GetStrongerPacType(enemyPac.pacType);
			if (pac.pacType == strongerThanHim && distanceToEnemy <= 6)
			{
				Common.WriteLine(7, "ATTACK! pacType" + pac.pacType.ToString() + " enemyPacType:" + enemyPac.pacType.ToString() + " distance:" + distanceToEnemy.ToString());
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
		if (target != null)
		{
			Common.WriteLine(8, "PacId:" + pac.id + " Strategy: Attack " + ((target == null) ? "null" : target.ToString()) + " inPursuit:" + pac.inPursuit.ToString());
		}
		return target;
	}
}

public class MimicStrategy : IPacStrategy
{
	public Point GetTarget(ref Pac pac)
	{
		Common.WriteLine(2, "Mimic strategy start");
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

		if (enemyPac != null && pac.origin.GetDistanceTo(enemyPac.origin) <= distance && PacController.HaveOppositeDirection(pac, enemyPac, pac.currentTarget))
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

		if (pac.currentTarget == null || pac.currentTarget == pac.origin)
		{
			target = enemyPac.origin;
		}
		else
		{
			target = pac.currentTarget;
		}

		if (target != null)
		{
			Common.WriteLine(8, "PacId:" + pac.id + " Strategy: Mimic " + ((target == null) ? "null" : target.ToString()));
		}
		return target;
	}
}

public static class Logic
{
	public static void SetTargets()
	{
		Common.WriteLine(3, "SetTargets, myPacs count:" + PacController.myPacs.Count().ToString());
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
			Common.WriteLine(8, "Waiting!");
		}

		Common.WriteLine(8, "Pac: " + pac.id + " target: " + pac.currentTarget.ToString());
		return;
	}

	public static void CheckArrivalToTarget(ref Pac pac)
	{
		if (pac.isOnPath && pac.origin.Equals(pac.currentTarget))
		{
			Common.WriteLine(7, "Arrival! Pac: " + pac.id + " target: " + pac.currentTarget.ToString());
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
				Common.WriteLine(6, "Deleted junction: " + pac.origin.ToString());
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
				//if (pac.isOnPath && (pac.currentTarget == pac.previousTarget) && !(pac.isInCollision || pac.isOnHold))
				{
					int step = 1;
					if (pac.speedTurnsLeft > 0)
					{
						step = 2;
					}
					pac.indexOnPath += step;
					if (pac.indexOnPath > pac.path.Count - 1)
					{
						pac.indexOnPath = pac.path.Count - 1;
					}
				}
				else
				{
  					if (pac.isOnHold || pac.isInCollision)
					{
						pac.isOnPath = false;
						//pac.previousTarget = pac.currentTarget;
						//pac.currentTarget = null;
						continue;

					}

					List<Point> path = null;
					if (FindPath(pac, new Node(pac.origin), new Node(pac.currentTarget), out path))
					{
							if (path.Count > 0)
							{
								SetPath(pac, path);
							}
							else
							{
								pac.isOnPath = false;
								//pac.isOnHold = true;
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
		Common.WriteLine(4, "Pathfinding started! pac:" + pac.id.ToString());
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
				//Common.WriteLine("Pathfinding currentNode: " + currentNode.ToString());

				openSet.Remove(currentNode);
				closedSet.Add(currentNode);

				if (currentNode == end)
				{
					path = CalculatePath(ref pac, start, currentNode);
					Common.WriteLine(8, "Found path: pac:" + pac.id + " start:" + start.ToString() + " end:" + currentNode.ToString() + " distance: " + path.Count.ToString());
					return true;
				}

				List<Point> neighbours = Level.GetNeighbours(currentNode.x, currentNode.y);
				foreach (Point neighbour in neighbours)
				{
					//Common.WriteLine("Pathfinding neighbour: " + neighbour.ToString());
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
			//Common.WriteLine("Added to path: " + currentNode.ToString());
		}

		path.Reverse();
		return path;
	}

	//Not thread safe
	public static List<Point> MarkObstacles(Pac pac, Point target)
	{
		List<Point> result = new List<Point>();
		Point pacDirection = PacController.GetDirectionSigns(pac.origin, target);

		//avoid collisions by treating own pacs as obstacles
		foreach (Pac otherPac in PacController.myPacs)
		{
			if (pac.id == otherPac.id || !otherPac.isAlive || otherPac.previousOrigin == null)
			{
				continue;
			}
			Point otherPacDirection = PacController.GetDirectionSigns(otherPac.previousOrigin, otherPac.origin);
			if (PacController.HaveOppositeDirection(pac, otherPac, target))
			{
				Common.WriteLine(5, "My pac obstacle found:" + otherPac.origin.ToString());
				result.Add(otherPac.origin);
				Level.map[otherPac.origin.y, otherPac.origin.x] = -1;
			}
		}

		//Treat enemy pacs that are stronger (and mine cannot switch) as obstacles
		foreach (Pac enemyPac in PacController.enemyPacs)
		{
			Point enemyOrigin = TreatEnemyAsDanger(pac, enemyPac, target);
			if (enemyOrigin != null)
			{
				result.Add(enemyOrigin);
				Level.map[enemyPac.origin.y, enemyPac.origin.x] = -1;
			}
		}

		return result;
	}

	//Treat enemy pacs that are stronger (and mine cannot switch) as danger
	public static Point TreatEnemyAsDanger(Pac myPac, Pac enemyPac, Point target)
	{
		Point result = null;
		PacType strongerThanMe = Common.GetStrongerPacType(myPac.pacType);
		if (!PacController.HaveOppositeDirection(myPac, enemyPac, target) ||
			(myPac.cooldown > 0 && (enemyPac.pacType == strongerThanMe || enemyPac.cooldown == 0)))
		{
			return null;
		}

		if (PacController.HaveOppositeDirection(myPac, enemyPac, target))
		{
			Common.WriteLine(5, "My pac obstacle found:" + enemyPac.origin.ToString());
			result = enemyPac.origin;
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

public static class Level
{
    public static int[,] map;
    //public List<Point> crossroads;
    public static Dictionary<Point, int> junctions = new Dictionary<Point, int>();
    public static Dictionary<Point, List<Point>> visibleTiles = new Dictionary<Point, List<Point>>();

    public static void InitializeLevel(int width, int height)
    {
        map = new int[height, width];
    }

    //public void InitializeLevel()
    //{ }

    public static void StringsToMatrix(string[] levelRows)
    {
        for (int i = 0; i < levelRows.Length; i++)
        {
            for (int j = 0; j < levelRows[i].Length; j++)
            {
                map[i, j] = levelRows[i][j] == ' '? 1: 0;
            }
        }
        //PrintMatrix(levelRows);
    }

    public static void CalculateJunctions()
    {
        for (int i = 0; i < map.GetLength(0); i++)
        {
            for (int j = 0; j < map.GetLength(1); j++)
            {
                if (map[i, j] > 0)
                {
                    List<Point> neighbours = GetNeighbours(j, i);
                    if (neighbours.Count >= 2)
                    {
                        if (neighbours.Count == 2)
                        {
                            //besides true crossroads, we're also adding elbows
                            if ((neighbours[0].x != neighbours[1].x) && (neighbours[0].y != neighbours[1].y))
                            {
                                junctions.Add(new Point(j, i), neighbours.Count);
                            }
                        }
                        else
                        {
                            junctions.Add(new Point(j, i), neighbours.Count);
                        }
                    }
                }
            }
        }
        junctions = (from entry in junctions orderby entry.Value descending select entry).ToDictionary(x => x.Key, x => x.Value);
        //junctions = (Dictionary<Point, int>) from entry in junctions orderby entry.Value ascending select entry;
    }

    public static List<Point> GetNeighbours(int x, int y)
    {
        List<Point> result = new List<Point>();

        if (x == 0)
        {
            //Should be always true per prerequisites
            if (map[y, map.GetLength(1) - 1] > 0)
            {
                result.Add(new Point(map.GetLength(1) - 1, y));
            }
        }
        else
        {
            if (CheckNeighbour(x - 1, y))
            {
                result.Add(new Point(x - 1, y));
            }
        }

        if (x == map.GetLength(1) - 1)
        {
            //Should be always true per prerequisites
            if (map[y, 0] > 0)
            {
                result.Add(new Point(0, y));
            }
        }
        else
        {
            if (CheckNeighbour(x + 1, y))
            {
                result.Add(new Point(x + 1, y));
            }
        }

        if (CheckNeighbour(x, y - 1))
        {
            result.Add(new Point(x, y - 1));
        }

        if (CheckNeighbour(x, y + 1))
        {
            result.Add(new Point(x, y + 1));
        }

        return result;
    }

    public static bool CheckNeighbour(int x, int y)
    {
        //Common.WriteLine("Checking neighbour: x:" + x.ToString() + " y:" + y.ToString() + " map[y, x]:" + map[y, x].ToString());
        bool result = false;
        //Should never happen per prerequisites
        if (y < 0 || y > map.GetLength(0) - 1)
        {
            return false;
        }

        //x, y are switched in a matrix
        if (map[y, x] > 0)

        {
            result = true;
        }
        return result;
    }

    public static void CalculateVisibleTilesForWholeMap()
    {
        for (int i = 0; i < map.GetLength(0); i++)
        {
            for (int j = 0; j < map.GetLength(1); j++)
            {
                if (map[i, j] > 0)
                {
                    Point tile = new Point(j, i);
                    visibleTiles.Add(tile, GetTilesVisibleFrom(tile));
                }
            }
        }
    }

    public static List<Point> GetTilesVisibleFrom(Point origin)
    {
        List<Point> result = new List<Point>();

        int j = origin.x;
        int i = origin.y;
        bool skip = false;

        //looking left
        while (map[i, j] > 0)
        {
            //it's on the edge 
            if (j == 0)
            {
                j = map.GetLength(1) - 1;
            }
            else
            {
                j--;
            }

            //Handle a rare case when we have a map with straight line with passages on sides - indicated by reachgin a starting point
            if (j == origin.x && i == origin.y)
            {
                skip = true;
                break;
            }

            if (map[i, j] > 0)
            {
                result.Add(new Point(j, i));
            }
        }

        j = origin.x;
        i = origin.y;
        //looking left (skipping in case of a straight vertical passage)
        if (!skip)
        {
            while (map[i, j] > 0)
            {
                //it's on the edge 
                if (j == map.GetLength(1) - 1)
                {
                    j = 0;
                }
                else
                {
                    j++;
                }

                if (map[i, j] > 0)
                {
                    result.Add(new Point(j, i));
                }
            }
        }

        j = origin.x;
        i = origin.y;
        //looking up
        //No passages on vertical axis, so 0 and GetLength(0) - 1 cannot have walkable tiles
        while (i > 0)
        {
            i--;
            if (map[i, j] > 0)
            {
                result.Add(new Point(j, i));
            }
            else
            {
                break;
            }
        }

        j = origin.x;
        i = origin.y;
        //looking up
        //No passages on vertical axis, so 0 and GetLength(0) - 1 cannot have walkable tiles
        while (i < map.GetLength(0) - 1)
        {
            i++;
            if (map[i, j] > 0)
            {
                result.Add(new Point(j, i));
            }
            else
            {
                break;
            }
        }

        return result;
    }

    public static void PrintMatrix(string[] levelRows)
    {
        for (int i = 0; i < levelRows.Length; i++)
        {
            for (int j = 0; j < levelRows[i].Length; j++)
            {
                Common.WriteLine(1, levelRows[i][j].ToString());
            }
        }
    }

    public static void PrintJunctions()
    {
        foreach (KeyValuePair<Point, int> junction in junctions)
        {
            Common.WriteLine(1, junction.Key.ToString() + " #" + junction.Value.ToString());
        }
    }
}

public enum PacType
{
	ROCK,
	PAPER,
	SCISSORS
}

public class Pac
{
	public int id;
	public Point origin;
	public Point previousOrigin;
	public PacType pacType;
	public int speedTurnsLeft;
	public int cooldown = 0;
	public bool shouldActivateSpeed;
	public bool shouldActivateSwitch;
	public PacType switchTo;
	public int speedActivatedTurn = -Common.SpeedCooldownDuration;
	public int switchActivatedTurn = -Common.SwitchCooldownDuration;
	public Point currentTarget;
	public Point previousTarget;
	public bool isOnHold;
	public bool isInCollision;
	public bool isAlive = true;
	public bool hasFixedTarget;
	public string latestStrategy;
	public bool inPursuit;

	public bool isOnPath;
	public List<Point> path;
	public int indexOnPath;
	public int distanceToTarget;

	public Pac(int id, Point origin, string pacType)
	{
		this.id = id;
		this.origin = origin;
		this.pacType = MapType(pacType);
	}

	public bool CanActivateAbility()
	{
		return cooldown == 0;
	}

	public PacType MapType(string typeString)
	{
		PacType result;
		if (!Enum.TryParse<PacType>(typeString, out result))
		{
			Common.WriteLine(8, "Cannot map Pac type: " + typeString);
		}
		return result;
	}

	public void CheckFixedTarget()
	{
		if (hasFixedTarget && origin.Equals(currentTarget))
		{
			hasFixedTarget = false;
		}
	}

	public Point ResetTargetIfStationary(Point target)
	{
		Point result = null;
		if (target != origin)
		{
			result = target;
		}
		return result;
	}
}

public static class PacController
{
	public static List<Pac> myPacs = new List<Pac>();
	public static List<Pac> enemyPacs = new List<Pac>();
	public static List<Pac> myCurrentPacs = new List<Pac>();
	public static List<Pac> enemyCurrentPacs = new List<Pac>();

	public static void AddPac(Pac pac)
	{
		myPacs.Add(pac);
	}

	public static void AddPac(int id, int x, int y, string pacType, bool mine, int speedTurnsLeft, int abilityCooldown)
	{
		Point origin = new Point(x, y);
		Pac pac = new Pac(id, origin, pacType);
		pac.speedTurnsLeft = speedTurnsLeft;
		pac.cooldown = abilityCooldown;
		
		pac.cooldown = abilityCooldown;
		if (mine)
		{
			myCurrentPacs.Add(pac);
			//myPacs.Add(pac);
		}
		else
		{
			enemyCurrentPacs.Add(pac);
			//enemyPacs.Add(pac);
		}
		Common.WriteLine(4, "Added Pac! Id: " + pac.id.ToString() + " Pac origin: " + pac.origin.ToString());
	}

	public static Pac GetMyPac(int id)
	{
		return myPacs.Where(x => x.id == id).FirstOrDefault();
	}

	public static Pac GetEnemyPac(int id)
	{
		return enemyPacs.Where(x => x.id == id).FirstOrDefault();
	}

	public static void ClearPacs()
	{
		if (enemyCurrentPacs != null && enemyCurrentPacs.Count > 0)
		{
			enemyCurrentPacs.Clear();
		}

		if (myCurrentPacs != null && myCurrentPacs.Count > 0)
		{
			myCurrentPacs.Clear();
		}
	}

	public static void SyncPacs()
	{
		if (myPacs.Count() != 0)
		{
			foreach (Pac pac in myPacs)
			{
				Pac currentPac = myCurrentPacs.Where(x => x.id == pac.id).FirstOrDefault();
				if (currentPac == null)
				{
					pac.isAlive = false;
					Common.WriteLine(4, "Synced dead Pac! Id: " + pac.id.ToString() + " Pac origin: " + pac.origin.ToString());
				}
				else
				{
					pac.speedTurnsLeft = currentPac.speedTurnsLeft;
					pac.pacType = currentPac.pacType;				  
					pac.cooldown = currentPac.cooldown;
					pac.previousOrigin = pac.origin;
					pac.origin = currentPac.origin;
					pac.CheckFixedTarget();
					pac.isInCollision = false;
					pac.shouldActivateSpeed = false;
					pac.shouldActivateSwitch = false;
                    pac.isOnHold = false;
					Common.WriteLine(4, "Synced Pac! Id: " + pac.id.ToString() + " Pac origin: " + pac.origin.ToString());
				}
			}
		}
		//First turn only - adding pacs
		else
		{
			foreach (Pac pac in myCurrentPacs)
			{
				myPacs.Add(pac);
				Common.WriteLine(4, "Synced added Pac! Id: " + pac.id.ToString() + " Pac origin: " + pac.origin.ToString());
			}
		}

		Common.WriteLine(5, "Enemy pacs count:" + enemyPacs.Count().ToString());
		if (enemyPacs.Count() != 0)
		{
			List<Pac> enemyPacsToRemove = new List<Pac>();
			foreach (Pac pac in enemyPacs)
			{
				Pac currentPac = enemyCurrentPacs.Where(x => x.id == pac.id).FirstOrDefault();
				if (currentPac == null)
				{
					enemyPacsToRemove.Add(pac);
					Common.WriteLine(5, "Enemy pac to remove! Id: " + pac.id.ToString());
				}
				else
				{
					pac.speedTurnsLeft = currentPac.speedTurnsLeft;
					pac.cooldown = currentPac.cooldown;
					pac.previousOrigin = pac.origin;
					pac.origin = currentPac.origin;
					Common.WriteLine(5, "Synced enemy pac! Id: " + pac.id.ToString() + " Pac origin: " + pac.origin.ToString());
				}
			}

			foreach (Pac enemyPac in enemyPacsToRemove)
			{
				Pac currentPac = GetEnemyPac(enemyPac.id);
				if (currentPac != null)
				{
					enemyPacs.Remove(currentPac);
				}
			}

			foreach (Pac enemyPac in enemyCurrentPacs)
			{
				Pac currentPac = GetEnemyPac(enemyPac.id);
				if (currentPac == null)
				{
					enemyPacs.Add(enemyPac);
				}
				Common.WriteLine(5, "Synced added enemy pac! Id: " + enemyPac.id.ToString() + " Pac origin: " + enemyPac.origin.ToString());
			}
		}
		//First turn only - adding pacs
		else
		{
			foreach (Pac pac in enemyCurrentPacs)
			{
				enemyPacs.Add(pac);
				Common.WriteLine(5, "Synced added enemy pac! Id: " + pac.id.ToString() + " Pac origin: " + pac.origin.ToString());
			}
		}
	}

	public static void ClearCurrentTargets()
	{
		foreach (Pac pac in myPacs)
		{
			pac.previousTarget = pac.currentTarget;
			pac.currentTarget = null;
		}
	}

	public static Dictionary<int, Point> GetCurrentTargets()
	{
		Dictionary<int, Point> result = new Dictionary<int, Point>();
		foreach (Pac pac in myPacs)
		{
			if ((pac.isAlive) && (pac.currentTarget != null))
			{
				result.Add(pac.id, pac.currentTarget);
			}
		}

		return result;
	}

	public static Pac GetPacWithCurrentTarget(Point target)
	{
		Pac pac = null;

		if (GetCurrentTargets().Values.Contains(target))
		{
			foreach (KeyValuePair<int, Point> keyValuePair in GetCurrentTargets())
			{
				if (keyValuePair.Value.Equals(target))
				{
					Common.WriteLine(7, "Found pac for target: " + target.ToString() + " id: " + keyValuePair.Key.ToString());
					pac = GetMyPac(keyValuePair.Key);
				}
			}

			//pac = GetPac(GetCurrentTargets().Where(x => x.Value.Equals(target)).FirstOrDefault().Key);
		}
		return pac;
	}

	public static void DetectCollisions()
	{
		foreach (Pac pac in myPacs)
		{
			//First turn special case
			if (pac.previousOrigin == null)
			{
				continue;
			}
				
			if (pac.origin == pac.previousOrigin && pac.cooldown >= 9)
			//if ((pac.previousTarget == pac.currentTarget) && (!pac.isOnHold))
			{
				Common.WriteLine(8, "Collision! Pac id:" + pac.id.ToString());
				pac.isInCollision = true;
			}
		}
		Common.WriteLine(2, "Finished detecting collisions");
	}

	public static Pac GetClosestEnemy(Pac pac)
	{
		Pac result = null;
		int minDistance = int.MaxValue;
		int distance;
		for (int i = 0; i < enemyPacs.Count(); i++)
		{
			distance = pac.origin.GetDistanceTo(enemyPacs[i].origin);
			if (distance < minDistance)
			{
				result = enemyPacs[i];
				minDistance = distance;
			}
		}

		if (result != null)
		{
			Common.WriteLine(7, "Found closest enemy!. myPac:" + pac.id.ToString() + " enemy: " + result.id.ToString());
		}
		return result;
	}

	public static Pac GetClosestVisibleEnemy(Pac myPac, bool withOppositeDirection = false)
	{
		Common.WriteLine(4, "GetClosestVisibleEnemy start");
		Pac result = null;
		int minDistance = int.MaxValue;
		int distanceToEnemy = int.MaxValue;

		if (enemyPacs.Count == 0)
		{
			Common.WriteLine(4, "No enemies!");
			return null;
		}

		List<Pac> enemies = new List<Pac>();

		foreach (Pac enemy in enemyPacs)
		{
			Common.WriteLine(4, "GetClosestVisibleEnemy enemy: " + enemy.origin.ToString());
			// foreach (Point tile in Level.GetTilesVisibleFrom(myPac.origin))
			// {
			// 	Common.WriteLine("GetClosestVisibleEnemy visible: " + tile.ToString());
			// }

			if (Level.GetTilesVisibleFrom(myPac.origin).Contains(enemy.origin))
			{
				Common.WriteLine(8, "GetClosestVisibleEnemy match found! opositeDirection:" + withOppositeDirection.ToString());
				distanceToEnemy = myPac.origin.GetDistanceTo(enemy.origin);
				if (withOppositeDirection)
				{
					if (PacController.HaveOppositeDirection(myPac, enemy, myPac.currentTarget))
					{
						if (distanceToEnemy < minDistance)
						{
							result = enemy;
							minDistance = distanceToEnemy;
						}
					}

				}
				else
				{
					if (distanceToEnemy < minDistance)
					{
						result = enemy;
						minDistance = distanceToEnemy;
					}
				}

			}
		}

		if (result != null)
		{
			Common.WriteLine(8, "Found closest visible enemy!. myPac:" + myPac.id.ToString() + " enemy: " + result.id.ToString());
		}
		return result;
	}

	public static Point GetDirectionSigns(Point start, Point end)
	{
		int xSign = Math.Sign(end.x - start.x);
		int ySign = Math.Sign(end.y - start.y);
		return new Point(xSign, ySign);
	}

	public static bool HaveOppositeDirection(Pac myPac, Pac otherPac, Point myPacTarget = null)
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

		//Special case for first turn or the first time we see enemy pack
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
		bool result = (otherPacDirection.x == 0 && otherPacDirection.y == 0) ||
						((myPacDirection.x == -otherPacDirection.x) && Math.Abs(myPac.origin.x + myPacDirection.x - otherPac.origin.x - otherPacDirection.x) < Math.Abs(myPac.origin.x - otherPac.origin.x)) ||
						((myPacDirection.y == -otherPacDirection.y) && Math.Abs(myPac.origin.y + myPacDirection.y - otherPac.origin.y - otherPacDirection.y) < Math.Abs(myPac.origin.y - otherPac.origin.y));
		if (result)
		{
			Common.WriteLine(8, "Pac directions opposite. myPac:" + myPac.id.ToString() + " direction:" + myPacDirection.ToString() + " other pac:" + otherPac.id.ToString() + " direction:" + otherPacDirection.ToString());
		}
		return result;
	}
}

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
        return BigPellets.Contains(position) || Pellets.Contains(position);
	}

	//For later use - calculation of quadrant densities relative to origin 
	public static float[] GetDensityScores(Point origin)
	{
		float[] densities = new float[4];



		return densities;
	}
}

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

	public static bool operator ==(Point a, Point b)
	{
		if (object.ReferenceEquals(a, null))
		{
			return object.ReferenceEquals(b, null);
		}
        else
        {
            if (object.ReferenceEquals(b, null))
            {
                return false;
            }
        }

		return (a.x == b.x) && (a.y == b.y);
	}

	public static bool operator !=(Point a, Point b)
	{
		return !(a == b);
	}

	public int GetDistanceTo(Point target)
	{
		int a = target.x - x;
		int b = target.y - y;
		//Euclidian
		//return Math.Sqrt(a*a + b*b);

		////Manhattan
		return Math.Abs(a) + Math.Abs(b);
	}

	public override string ToString()
	{
		return x.ToString() + " " + y.ToString();
	}
}

public class Node : Point
{
	public int gCost;
	public int hCost;

	public Node parent;

	public Node(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public Node(Point point)
	{
		x = point.x;
		y = point.y;
	}

	public Node(Point point, int gCost, int hCost)
		:this(point)
	{
		this.gCost = gCost;
		this.hCost = hCost;
	}

	public int fCost
	{
		get
		{
			return gCost + hCost;
		}
	}



}

public static class Common
{
	public static readonly int debugPriority = 5;
	//when set to true, debug will print only messages of priority set in debugPriority, else it will print everything with equal or higher priority
	public static bool showOnlyDebugPriority = false;
	public static readonly int SpeedCooldownDuration = 10;
	public static readonly int SwitchCooldownDuration = 10;
    public static readonly int minDistanceForSwitch = 3;

	public static int CurrentTurn = 0;
	public static List<int> remainingPacs;

	//Used when we realize that there is a pac closer to already selected target that the pac it was selected for, and we want to switch these pacs - assing this already selected target to current/closer pac and keep searching for farther pac
	public static void SwapPacTargets(ref Pac previousOwner, ref Pac newOwner, Point target)
	{
		if (previousOwner.hasFixedTarget || previousOwner.inPursuit)
		{
			Common.WriteLine(8, "Cannot swap: Pac: " + previousOwner.id.ToString() + " has fixed target: " + previousOwner.currentTarget.ToString());
			return;
		}
		Common.WriteLine(5, "Swapping targets - Previous: " + previousOwner.id.ToString() + " New" + newOwner.id.ToString() + " Pellet:" + target.ToString());

		newOwner.currentTarget = target;
		previousOwner.currentTarget = null;
		//previousOwner.isOnPath = false;
		newOwner = previousOwner;
		return;
	}

	public static PacType GetStrongerPacType(PacType otherPacType)
	{
		PacType result = PacType.PAPER;
		switch (otherPacType)
		{
			case PacType.PAPER:
				result = PacType.SCISSORS;
				break;
			case PacType.ROCK:
				result = PacType.PAPER;
				break;
			case PacType.SCISSORS:
				result = PacType.ROCK;
				break;
		}
		return result;
	}

	public static void WriteLine(int priority, string message)
	{
		if (priority >= Common.debugPriority)
		{
			if (Common.showOnlyDebugPriority && priority != Common.debugPriority)
			{
				return;
			}

			Console.Error.WriteLine(message);
		}
	} 
}