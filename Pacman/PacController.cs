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
		public int id;
		public Point origin;
		public PacType pacType;
		public int cooldown = 0;
		public int speedActivatedTurn = -Common.SpeedCooldownDuration;
		public int switchActivatedTurn = -Common.SwitchCooldownDuration;
		public Point currentTarget;
		public Point previousTarget;
		public bool isOnHold;
		public bool isAlive = true;
		public string latestStrategy;

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

		public void ActivateSpeed()
		{
			Console.WriteLine("SPEED " + this.id.ToString());

			Console.Error.WriteLine("Pac: " + this.id.ToString() + " activated SPEED");
			speedActivatedTurn = Common.CurrentTurn;
		}

		public void ActivateSwitch(string switchTo)
		{
			this.pacType = MapType(switchTo);
			Console.WriteLine("SWITCH " + this.id.ToString() + " " + this.pacType.ToString());

			Console.Error.WriteLine("Pac: " + this.id.ToString() + " switched to " + switchTo);
			switchActivatedTurn = Common.CurrentTurn;
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
	}

	public static class PacController
	{
		public static List<Pac> myPacs = new List<Pac>();
		public static List<Pac> enemyPacs = new List<Pac>();
		public static List<Pac> myCurrentPacs = new List<Pac>();

		public static void AddPac(Pac pac)
		{
			myPacs.Add(pac);
		}

		public static void AddPac(int id, int x, int y, string pacType, bool mine, int abilityCooldown)
		{
			Point origin = new Point(x, y);
			Pac pac = new Pac(id, origin, pacType);
			pac.cooldown = abilityCooldown;
			if (mine)
			{
				myCurrentPacs.Add(pac);
				//myPacs.Add(pac);
			}
			else
			{
				enemyPacs.Add(pac);
			}
			Console.Error.WriteLine("Added Pac! Id: " + pac.id.ToString());
		}


		public static Pac GetPac(int id)
		{
			return myPacs.Where(x => x.id == id).FirstOrDefault();
		}

		public static void ClearPacs()
		{
			if (enemyPacs != null && enemyPacs.Count > 0)
			{
				enemyPacs.Clear();
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
					}
					else
					{
						pac.cooldown = currentPac.cooldown;
					}
				}
			}
			//First turn only - adding pacs
			else
			{
				foreach (Pac pac in myCurrentPacs)
				{
					myPacs.Add(pac);
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
						Console.Error.WriteLine("Found pac for target: " + target.ToString() + " id: " + keyValuePair.Key.ToString());
						pac = GetPac(keyValuePair.Key);
					}
				}

				//pac = GetPac(GetCurrentTargets().Where(x => x.Value.Equals(target)).FirstOrDefault().Key);
			}
			return pac;
		}
	}
}
