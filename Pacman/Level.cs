using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman
{
    public class Level
    {
        public int[,] map;

        public Level(int width, int height)
        {
            map = new int[width, height];
        }

        public void StringsToMatrix(string[] levelRows)
        {
            for (int i = 0; i < levelRows.Length; i++)
            {
                for (int j = 0; j < levelRows[i].Length; j++)
                {
                    map[i, j] = levelRows[i][j] == ' '? 1: 0;
                }
            }
        }
    }
}
