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
	}

	public static class PacController
	{
		public static List<Pac> pacs = new List<Pac>();
		public static List<Pac> enemyPacs = new List<Pac>();

		public static void AddPac(Pac pac)
		{
			pacs.Add(pac);
		}

		public static void AddPac(int id, int x, int y, string pacType, bool mine)
		{
			Point origin = new Point(x, y);
			Pac pac = new Pac(id, origin, pacType);
			if (mine)
			{
				pacs.Add(pac);
			}
			else
			{
				enemyPacs.Add(pac);
			}
			Console.Error.WriteLine("Added Pac! Id: " + pac.Id.ToString());
		}


		public static Pac GetPac(int id)
		{
			return pacs.Where(x => x.Id == id).FirstOrDefault();
		}

		public static void ClearPacs()
		{
			if (pacs != null && pacs.Count > 0)
			{
				pacs.Clear();
			}
		}
	}
}
