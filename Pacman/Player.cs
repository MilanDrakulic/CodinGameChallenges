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
                        Console.Error.WriteLine("Pac on path: id: " + pac.id.ToString() + " index:" + pac.indexOnPath + " target:" + pac.currentTarget.ToString() + " distance:" + pac.distanceToTarget.ToString());
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
}
