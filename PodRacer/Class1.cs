using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace PodRacer
{

    /**
     * This code automatically collects game data in an infinite loop.
     * It uses the standard input to place data into the game variables such as x and y.
     * YOU DO NOT NEED TO MODIFY THE INITIALIZATION OF THE GAME VARIABLES.
     **/
    class Player
    {
        public static bool boostUsed = false;
        public static int previousX = -1;
        public static int previousY = -1;
        
        public static int directionX = 1;
        public static int directionY = 1;

        static void Main(string[] args)
        {
            while (true)
            {
                string[] inputs = Console.ReadLine().Split(' ');
                int x = int.Parse(inputs[0]); // x position of your pod
                int y = int.Parse(inputs[1]); // y position of your pod
                int nextCheckpointX = int.Parse(inputs[2]); // x position of the next check point
                int nextCheckpointY = int.Parse(inputs[3]); // y position of the next check point
                int nextCheckpointDist = int.Parse(inputs[4]);
                int nextCheckpointAngle = int.Parse(inputs[5]);

                // inputs = Console.ReadLine().Split(' ');
                // int opponentX = int.Parse(inputs[0]);
                // int opponentY = int.Parse(inputs[1]);

                int targetX = nextCheckpointX;
                int targetY = nextCheckpointY;

                int thrust = 100;
                //int offset = 1200;

                // if (previousX != -1)
                // {
                //     directionX = Math.Sign(x - previousX);
                //     //directionX = (nextCheckpointX > x? 1: -1) * Math.Sign(x - previousX);
                // }
                // if (previousY != -1)
                // {
                //     directionY = Math.Sign(y - previousY);
                //     //directionY = (nextCheckpointY > y? 1: -1) * Math.Sign(y - previousY);
                // }
                // previousX = x;
                // previousY = y;

                // double offsetAngle = 0;
                // double hipotenusa = Math.Sqrt(offset * offset + nextCheckpointDist * nextCheckpointDist);
                // offsetAngle = Math.Asin(offset / hipotenusa);

                if (Math.Abs(nextCheckpointAngle) > 90)
                {
                    thrust = 0;
                    //thrust = Math.Abs(nextCheckpointAngle) > 90? 0: 10;

                    //targetX = ClampX(targetX - (int)(directionX * offset * Math.Cos(offsetAngle)));
                    //targetY = ClampY(targetY - (int)(directionY * offset * Math.Sin(offsetAngle)));

                    //targetX = ClampX(targetX - directionX * offset);
                    //targetY = ClampY(targetY - directionY * offset);
                }
                else
                {
                    if (nextCheckpointDist < 2000)
                    {
                        //thrust = 5;
                        thrust = nextCheckpointDist < 600 ? 0 : 50 + (nextCheckpointDist / 2000) * 50;
                    }

                }

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");

                // if ((nextCheckpointX != targetX) || (nextCheckpointY != targetY))
                // {
                //     Console.Error.WriteLine("CheckX: " + nextCheckpointX.ToString() + ", TargetX: " + targetX.ToString());
                //     Console.Error.WriteLine("CheckY: " + nextCheckpointY.ToString() + ", TargetY: " + targetY.ToString());
                // }

                if ((nextCheckpointDist > 4000) && (Math.Abs(nextCheckpointAngle) < 4))
                {
                    //Console.Error.WriteLine("Angle: " + nextCheckpointAngle.ToString());
                    Console.WriteLine(targetX + " " + targetY + " BOOST");
                }
                else
                {
                    Console.WriteLine(targetX + " " + targetY + " " + thrust);
                }








            //// game loop
            //while (true)
            //{
            //    string[] inputs = Console.ReadLine().Split(' ');
            //    int x = int.Parse(inputs[0]); // x position of your pod
            //    int y = int.Parse(inputs[1]); // y position of your pod
            //    int nextCheckpointX = int.Parse(inputs[2]); // x position of the next check point
            //    int nextCheckpointY = int.Parse(inputs[3]); // y position of the next check point
            //    int nextCheckpointDist = int.Parse(inputs[4]);
            //    int nextCheckpointAngle = int.Parse(inputs[5]);

            //    inputs = Console.ReadLine().Split(' ');
            //    int opponentX = int.Parse(inputs[0]);
            //    int opponentY = int.Parse(inputs[1]);


            //    int targetX = nextCheckpointX;
            //    int targetY = nextCheckpointY;

            //    int thrust = 100;
            //    int offset = 1200;

            //    if (previousX != -1)
            //    {
            //        directionX = Math.Sign(x - previousX);
            //        //directionX = (nextCheckpointX > x? 1: -1) * Math.Sign(x - previousX);
            //    }
            //    if (previousY != -1)
            //    {
            //        directionY = Math.Sign(y - previousY);
            //        //directionY = (nextCheckpointY > y? 1: -1) * Math.Sign(y - previousY);
            //    }
            //    previousX = x;
            //    previousY = y;

            //    double offsetAngle = 0;
            //    double hipotenusa = Math.Sqrt(offset * offset + nextCheckpointDist * nextCheckpointDist);
            //    offsetAngle = Math.Asin(offset / hipotenusa);

            //    if (Math.Abs(nextCheckpointAngle) > 90)
            //    {
            //        thrust = Math.Abs(nextCheckpointAngle) > 45 ? 0 : 50;

            //        targetX = ClampX(targetX - (int)(directionX * offset * Math.Cos(offsetAngle)));
            //        targetY = ClampY(targetY - (int)(directionY * offset * Math.Sin(offsetAngle)));

            //        //targetX = ClampX(targetX - directionX * offset);
            //        //targetY = ClampY(targetY - directionY * offset);
            //    }
            //    else
            //    {
            //        if (nextCheckpointDist < 1800)
            //        {
            //            //thrust = 5;
            //            thrust = nextCheckpointDist < 600 ? 0 : 50 + (nextCheckpointDist / 1800) * 50;
            //        }

            //    }

            //    // Write an action using Console.WriteLine()
            //    // To debug: Console.Error.WriteLine("Debug messages...");

            //    if ((nextCheckpointX != targetX) || (nextCheckpointY != targetY))
            //    {
            //        Console.Error.WriteLine("CheckX: " + nextCheckpointX.ToString() + ", TargetX: " + targetX.ToString());
            //        Console.Error.WriteLine("CheckY: " + nextCheckpointY.ToString() + ", TargetY: " + targetY.ToString());
            //    }
            //    if ((nextCheckpointDist > 4000) && (nextCheckpointAngle < 4))
            //    {
            //        Console.WriteLine(targetX + " " + targetY + " BOOST");
            //    }
            //    else
            //    {
            //        Console.WriteLine(targetX + " " + targetY + " " + thrust);
            //    }

        }

        }

        public static int ClampX(int x)
        {
            if (x > 16000)
            {
                return 16000;
            }
            if (x < 0)
            {
                return 0;
            }
            return x;
        }

        public static int ClampY(int y)
        {
            if (y > 9000)
            {
                return 9000;
            }
            if (y < 0)
            {
                return 0;
            }
            return y;
        }
    }
}
