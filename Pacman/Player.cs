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
            Level.InitializeLevel(width, height);
            Level.StringsToMatrix(levelRows);
            Level.CalculateJunctions();
            Level.PrintJunctions();

            // game loop
            while (true)
            {
                Common.CurrentTurn++;
                PacController.ClearCurrentTargets();
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

                    PacController.AddPac(pacId, x, y, typeId, mine, abilityCooldown);
                }

                PelletController.ClearPellets();

                int visiblePelletCount = int.Parse(Console.ReadLine()); // all pellets in sight
                Console.Error.WriteLine("Visible pellets: " + visiblePelletCount.ToString());
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

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");

                string output = "";
                foreach (Pac pac in PacController.myPacs)
                {
                    Logic.SetTarget(pac);
                    Point target = Logic.CurrentTargets[pac.Id].point;

                    output += (output == "") ? "" : "|";
                    output += "MOVE " + pac.Id.ToString() + " " + target.ToString();

                    // if (!Logic.CurrentTargets[pac.Id].onHold && pac.Cooldown == 0)
                    // {
                    //     output += " | SPEED " + pac.Id.ToString();
                    //     //Console.WriteLine("SPEED " + pac.Id.ToString());
                    // }
                }
                Console.WriteLine(output);

            }
        }
    }
}
