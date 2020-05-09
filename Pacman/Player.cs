using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman
{
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
            Level level = new Level(width, height);
            level.StringsToMatrix(levelRows);

            // game loop
            while (true)
            {
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

                    PacController.AddPac(pacId, x, y, typeId, mine);
                }

                PelletController.ClearPellets();

                int visiblePelletCount = int.Parse(Console.ReadLine()); // all pellets in sight
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

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");

                string output = "";
                foreach (Pac pac in PacController.pacs)
                {
                    Logic.SetTarget(pac);
                    Point target = Logic.CurrentTargets[pac.Id];

                    output += (output == "") ? "" : "|";
                    output += "MOVE " + pac.Id.ToString() + " " + target.ToString();
                }
                Console.WriteLine(output);

                //for (int i = 0; i < PacController.pacs.Count; i++)
                //{
                //    Logic.SetTarget(PacController.pacs[i]);
                //    Console.WriteLine("MOVE " + PacController.pacs[i].Id.ToString() + " " + Logic.CurrentTargets[PacController.pacs[i].Id].ToString());
                //    //Console.WriteLine("MOVE 0 15 10"); // MOVE <pacId> <x> <y>
                //}
            }
        }
    }
}
