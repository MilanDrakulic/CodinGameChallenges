using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman
{
	public enum PacType
	{
		ROCK,
		PAPER,
		SCISSORS
	}

	public class Pac
	{
		public int Id;
		public Point Origin;
		public PacType PacType;
		public int Cooldown = 0;
		public int SpeedActivatedTurn = -Common.SpeedCooldownDuration;
		public int SwitchActivatedTurn = -Common.SwitchCooldownDuration;
		public Point currentTarget;
		public Point previousTarget;

		public Pac(int id, Point origin, string pacType)
		{
			Id = id;
			Origin = origin;
			PacType = MapType(pacType);
		}

		public bool CanActivateAbility()
		{
			return Cooldown == 0;
		}

		public void ActivateSpeed()
		{
			Console.WriteLine("SPEED " + this.Id.ToString());

			Console.Error.WriteLine("Pac: " + this.Id.ToString() + " activated SPEED");
			SpeedActivatedTurn = Common.CurrentTurn;
		}

		public void ActivateSwitch(string switchTo)
		{
			this.PacType = MapType(switchTo);
			Console.WriteLine("SWITCH " + this.Id.ToString() + " " + this.PacType.ToString());

			Console.Error.WriteLine("Pac: " + this.Id.ToString() + " switched to " + switchTo);
			SwitchActivatedTurn = Common.CurrentTurn;
		}

		public PacType MapType(string typeString)
		{
			PacType result;
			if (!Enum.TryParse<PacType>(typeString, out result))
			{
				Console.Error.WriteLine("Cannot map Pac type: " + typeString);
			}
			return result;
		}

		public Point GetCurrentTarget()
		{
			return Common.currentTargets[this.Id];
		}
	}

	public static class PacController
	{
		public static List<Pac> myPacs = new List<Pac>();
		public static List<Pac> enemyPacs = new List<Pac>();

		public static void AddPac(Pac pac)
		{
			myPacs.Add(pac);
		}

		public static void AddPac(int id, int x, int y, string pacType, bool mine, int abilityCooldown)
		{
			Point origin = new Point(x, y);
			Pac pac = new Pac(id, origin, pacType);
			pac.Cooldown = abilityCooldown;
			if (mine)
			{
				myPacs.Add(pac);
			}
			else
			{
				enemyPacs.Add(pac);
			}
			Console.Error.WriteLine("Added Pac! Id: " + pac.Id.ToString());
		}


		public static Pac GetPac(int id)
		{
			return myPacs.Where(x => x.Id == id).FirstOrDefault();
		}

		public static void ClearPacs()
		{
			//TODO - switch to this
			//Common.remainingPacs.Clear();

			if (myPacs != null && myPacs.Count > 0)
			{
				myPacs.Clear();
			}
		}

		public static void ClearCurrentTargets()
		{
			foreach (Pac pac in myPacs)
			{
				pac.currentTarget = null;
			}
			Common.currentTargets.Clear();
		}

		public static Dictionary<int, Point> GetCurrentTargets()
		{
			Dictionary<int, Point> result = null;
			foreach (Pac pac in myPacs)
			{
				if (pac.currentTarget != null)
				{
					result.Add(pac.Id, pac.currentTarget);
				}
			}

			return result;
		}
	}
}
