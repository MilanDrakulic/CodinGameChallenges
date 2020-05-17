using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Pacman
{
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
            PrintMatrix(levelRows);
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

        public static List<Node> GetNeighbourNodes(int x, int y)
        {
            List<Node> result = new List<Node>();

            if (x == 0)
            {
                //Should be always true per prerequisites
                if (map[y, map.GetLength(1) - 1] > 0)
                {
                    result.Add(new Node(map.GetLength(1) - 1, y));
                }
            }
            else
            {
                if (CheckNeighbour(x - 1, y))
                {
                    result.Add(new Node(x - 1, y));
                }
            }

            if (x == map.GetLength(1) - 1)
            {
                //Should be always true per prerequisites
                if (map[y, 0] > 0)
                {
                    result.Add(new Node(0, y));
                }
            }
            else
            {
                if (CheckNeighbour(x + 1, y))
                {
                    result.Add(new Node(x + 1, y));
                }
            }

            if (CheckNeighbour(x, y - 1))
            {
                result.Add(new Node(x, y - 1));
            }

            if (CheckNeighbour(x, y + 1))
            {
                result.Add(new Node(x, y + 1));
            }

            return result;
        }

        public static bool CheckNeighbour(int x, int y)
        {
            //Console.Error.WriteLine("Checking neighbour: x:" + x.ToString() + " y:" + y.ToString() + " map[y, x]:" + map[y, x].ToString());
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
                    Console.Error.WriteLine(levelRows[i][j]);
                }
            }
        }

        public static void PrintJunctions()
        {
            foreach (KeyValuePair<Point, int> junction in junctions)
            {
                Console.Error.WriteLine(junction.Key.ToString() + " #" + junction.Value.ToString());
            }
        }
    }
}
