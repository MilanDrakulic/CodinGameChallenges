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
        public static Dictionary<Point, int> junctions;

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
        }

        public static void CalculateJunctions()
        {
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (map[i, j] > 0)
                    {
                        List<Point> neighbours = GetNeighbours(i, j);
                        if (neighbours.Count >= 2)
                        {
                            if (neighbours.Count == 2)
                            {
                                //besides true crossroads, we're also adding elbows
                                if ((neighbours[0].x != neighbours[1].x) && (neighbours[0].y != neighbours[1].y))
                                {
                                    junctions.Add(new Point(i, j), neighbours.Count);
                                }
                            }
                            else
                            {
                                junctions.Add(new Point(i, j), neighbours.Count);
                            }
                        }
                    }
                }
            }
            junctions = (Dictionary<Point, int>) from entry in junctions orderby entry.Value ascending select entry;
        }

        public static List<Point> GetNeighbours(int x, int y)
        {
            List<Point> result = new List<Point>();

            if (x == 0)
            {
                //Should be always true per prerequisites
                if (map[map.GetLength(1) - 1, y] > 0)
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
                if (map[0, y] > 0)
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
            bool result = false;
            //Should never happen per prerequisites
            if (y < 0 || y > map.GetLength(0) - 1)
            {
                return false;
            }

            if (map[x, y] > 0)
            {
                result = true;
            }
            return result;
        }

        public static Point GetNearestJunction(Point origin)
        {
            Point result = null;
            double minDistance = Double.MaxValue;
            double distance;

            foreach (KeyValuePair<Point, int> junction in junctions)
            {
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
}
