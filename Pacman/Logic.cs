using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman
{

	public static class Logic
	{
		public static Dictionary<int, Point> CurrentTargets = new Dictionary<int, Point>();

		public static void AddCurrentTargetIfNeeded(Pac pac)
		{
			if (!CurrentTargets.ContainsKey(pac.Id))
			{
				CurrentTargets.Add(pac.Id, new Point());
				Console.Error.WriteLine("Added Target! Id: " + pac.Id.ToString());
			}
		}

		public static void SetTarget(Pac pac)
		{
			AddCurrentTargetIfNeeded(pac);
			if (CurrentTargets[pac.Id].IsValid() && PelletController.ExistsAtPosition(CurrentTargets[pac.Id]))
			{
				return;
			}

			Console.Error.WriteLine("Searching for new target!");
			Point target = new Point();
			double minDistance = Double.MaxValue;
			double distance;

			if (PelletController.BigPellets.Count > 0)
			{

				foreach (Point bigPellet in PelletController.BigPellets)
				{
					distance = bigPellet.GetDistanceTo(pac.Origin);
					if (distance < minDistance)
					{
						minDistance = distance;
						target = bigPellet;
					}
				}
			}
			else
			{
				//temp logic, to be switched to density
				if (PelletController.Pellets.Count > 0)
				{
					foreach (Point pellet in PelletController.Pellets)
					{
						distance = pellet.GetDistanceTo(pac.Origin);
						if (distance < minDistance)
						{
							minDistance = distance;
							target = pellet;
						}
					}
				}
			}

			if (target.IsValid())
			{
				CurrentTargets[pac.Id] = target;
				Console.Error.WriteLine("New target: " + CurrentTargets[pac.Id].ToString());
			}

			return;
			//return target.IsValid()? target: origin;
		}
	}
}
