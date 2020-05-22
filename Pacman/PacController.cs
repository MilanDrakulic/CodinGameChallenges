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
		public Point previousOrigin;
		public PacType pacType;
		public int speedTurnsLeft;
		public int cooldown = 0;
		public bool shouldActivateSpeed;
		public bool shouldActivateSwitch;
		public PacType switchTo;
		public int speedActivatedTurn = -Common.SpeedCooldownDuration;
		public int switchActivatedTurn = -Common.SwitchCooldownDuration;
		public Point currentTarget;
		public Point previousTarget;
		public bool isOnHold;
		public bool isInCollision;
		public bool isAlive = true;
		public bool hasFixedTarget;
		public string latestStrategy;
		public bool inPursuit;

		public bool isOnPath;
		public List<Point> path;
		public int indexOnPath;
		public int distanceToTarget;

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

		public PacType MapType(string typeString)
		{
			PacType result;
			if (!Enum.TryParse<PacType>(typeString, out result))
			{
				Console.Error.WriteLine("Cannot map Pac type: " + typeString);
			}
			return result;
		}

		public void CheckFixedTarget()
		{
			if (hasFixedTarget && origin.Equals(currentTarget))
			{
				hasFixedTarget = false;
			}
		}

		public Point ResetTargetIfStationary(Point target)
		{
			Point result = null;
			if (target != origin)
			{
				result = target;
			}
			return result;
		}
	}

	public static class PacController
	{
		public static List<Pac> myPacs = new List<Pac>();
		public static List<Pac> enemyPacs = new List<Pac>();
		public static List<Pac> myCurrentPacs = new List<Pac>();
		public static List<Pac> enemyCurrentPacs = new List<Pac>();

		public static void AddPac(int id, int x, int y, string pacType, bool mine, int speedTurnsLeft, int abilityCooldown)
		{
			Point origin = new Point(x, y);
			Pac pac = new Pac(id, origin, pacType);
			pac.speedTurnsLeft = speedTurnsLeft;
			pac.cooldown = abilityCooldown;
		
			pac.cooldown = abilityCooldown;
			if (mine)
			{
				myCurrentPacs.Add(pac);
				//myPacs.Add(pac);
			}
			else
			{
				enemyCurrentPacs.Add(pac);
				//enemyPacs.Add(pac);
			}
			Console.Error.WriteLine("Added Pac! Id: " + pac.id.ToString() + " Pac origin: " + pac.origin.ToString());
		}


		public static Pac GetMyPac(int id)
		{
			return myPacs.Where(x => x.id == id).FirstOrDefault();
		}

		public static Pac GetEnemyPac(int id)
		{
			return enemyPacs.Where(x => x.id == id).FirstOrDefault();
		}

		public static void ClearPacs()
		{
			if (enemyCurrentPacs != null && enemyCurrentPacs.Count > 0)
			{
				enemyCurrentPacs.Clear();
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
						Console.Error.WriteLine("Synced dead Pac! Id: " + pac.id.ToString() + " Pac origin: " + pac.origin.ToString());
					}
					else
					{
						pac.speedTurnsLeft = currentPac.speedTurnsLeft;
						pac.pacType = currentPac.pacType;
						pac.cooldown = currentPac.cooldown;
						pac.previousOrigin = pac.origin;
						pac.origin = currentPac.origin;
						pac.CheckFixedTarget();
						pac.isInCollision = false;
						pac.shouldActivateSpeed = false;
						pac.shouldActivateSwitch = false;
						pac.isOnHold = false;
						Console.Error.WriteLine("Synced Pac! Id: " + pac.id.ToString() + " Pac origin: " + pac.origin.ToString());
					}
				}
			}
			//First turn only - adding pacs
			else
			{
				foreach (Pac pac in myCurrentPacs)
				{
					myPacs.Add(pac);
					Console.Error.WriteLine("Synced added Pac! Id: " + pac.id.ToString() + " Pac origin: " + pac.origin.ToString());
				}
			}

			Console.Error.WriteLine("Enemy pacs count:" + enemyPacs.Count().ToString());
			if (enemyPacs.Count() != 0)
			{
				List<Pac> enemyPacsToRemove = new List<Pac>();
				foreach (Pac pac in enemyPacs)
				{
					Pac currentPac = enemyCurrentPacs.Where(x => x.id == pac.id).FirstOrDefault();
					if (currentPac == null)
					{
						enemyPacsToRemove.Add(pac);
						Console.Error.WriteLine("Enemy pac to remove! Id: " + pac.id.ToString());
					}
					else
					{
						pac.speedTurnsLeft = currentPac.speedTurnsLeft;
						pac.cooldown = currentPac.cooldown;
						pac.previousOrigin = pac.origin;
						pac.origin = currentPac.origin;
						Console.Error.WriteLine("Synced enemy pac! Id: " + pac.id.ToString() + " Pac origin: " + pac.origin.ToString());
					}
				}

				foreach (Pac enemyPac in enemyPacsToRemove)
				{
					Pac currentPac = GetEnemyPac(enemyPac.id);
					if (currentPac != null)
					{
						enemyPacs.Remove(currentPac);
					}
				}

				foreach (Pac enemyPac in enemyCurrentPacs)
				{
					Pac currentPac = GetEnemyPac(enemyPac.id);
					if (currentPac == null)
					{
						enemyPacs.Add(enemyPac);
					}
					Console.Error.WriteLine("Synced added enemy pac! Id: " + enemyPac.id.ToString() + " Pac origin: " + enemyPac.origin.ToString());
				}
			}
			//First turn only - adding pacs
			else
			{
				foreach (Pac pac in enemyCurrentPacs)
				{
					enemyPacs.Add(pac);
					Console.Error.WriteLine("Synced added enemy pac! Id: " + pac.id.ToString() + " Pac origin: " + pac.origin.ToString());
				}
			}
		}

		//public static void ClearCurrentTargets()
		//{
		//	foreach (Pac pac in myPacs)
		//	{
		//		pac.previousTarget = pac.currentTarget;
		//		pac.currentTarget = null;
		//	}
		//}

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
						pac = GetMyPac(keyValuePair.Key);
					}
				}

				//pac = GetPac(GetCurrentTargets().Where(x => x.Value.Equals(target)).FirstOrDefault().Key);
			}
			return pac;
		}

		public static void DetectCollisions()
		{
			foreach (Pac pac in myPacs)
			{
				//First turn special case
				if (pac.previousOrigin == null)
				{
					continue;
				}
				
				if (pac.origin == pac.previousOrigin && pac.cooldown >= 9)
				//if ((pac.previousTarget == pac.currentTarget) && (!pac.isOnHold))
				{
					Console.Error.WriteLine("Collision! Pac id:" + pac.id.ToString());
					pac.isInCollision = true;
				}
			}
			Console.Error.WriteLine("Finished detecting collisions");
		}

		public static Pac GetClosestEnemy(Pac pac)
		{
			Pac result = null;
			int minDistance = int.MaxValue;
			int distance;
			for (int i = 0; i < enemyPacs.Count(); i++)
			{
				distance = pac.origin.GetDistanceTo(enemyPacs[i].origin);
				if (distance < minDistance)
				{
					result = enemyPacs[i];
					minDistance = distance;
				}
			}

			if (result != null)
			{
				Console.Error.WriteLine("Found closest enemy!. myPac:" + pac.id.ToString() + " enemy: " + result.id.ToString());
			}
			return result;
		}

		public static Pac GetClosestVisibleEnemy(Pac myPac, bool withOppositeDirection = false)
		{
			Console.Error.WriteLine("GetClosestVisibleEnemy start");
			Pac result = null;
			int minDistance = int.MaxValue;
			int distanceToEnemy = int.MaxValue;

			if (enemyPacs.Count == 0)
			{
				Console.Error.WriteLine("No enemies!");
				return null;
			}

			List<Pac> enemies = new List<Pac>();

			foreach (Pac enemy in enemyPacs)
			{
				Console.Error.WriteLine("GetClosestVisibleEnemy enemy: " + enemy.origin.ToString());
				//foreach (Point tile in Level.GetTilesVisibleFrom(myPac.origin))
				//{
				//	Console.Error.WriteLine("GetClosestVisibleEnemy visible: " + tile.ToString());
				//}

				if (Level.GetTilesVisibleFrom(myPac.origin).Contains(enemy.origin))
				{
					Console.Error.WriteLine("GetClosestVisibleEnemy match found! opositeDirection:" + withOppositeDirection.ToString());
					distanceToEnemy = myPac.origin.GetDistanceTo(enemy.origin);
					if (withOppositeDirection)
					{
						if (PacController.HaveOppositeDirection(myPac, enemy, myPac.currentTarget))
						{
							if (distanceToEnemy < minDistance)
							{
								result = enemy;
								minDistance = distanceToEnemy;
							}
						}

					}
					else
					{
						if (distanceToEnemy < minDistance)
						{
							result = enemy;
							minDistance = distanceToEnemy;
						}
					}

				}
			}

			if (result != null)
			{
				Console.Error.WriteLine("Found closest enemy!. myPac:" + myPac.id.ToString() + " enemy: " + result.id.ToString());
			}
			return result;
		}

		public static Point GetDirectionSigns(Point start, Point end)
		{
			int xSign = Math.Sign(end.x - start.x);
			int ySign = Math.Sign(end.y - start.y);
			return new Point(xSign, ySign);
		}

		public static bool HaveOppositeDirection(Pac myPac, Pac otherPac, Point myPacTarget = null)
		{
			Point myPacDirection;
			Point otherPacDirection;
			if (myPacTarget != null)
			{
				myPacDirection = GetDirectionSigns(myPac.origin, myPacTarget);
			}
			else
			{
				//Special case for first turn
				if (myPac.previousOrigin == null)
				{
					return false;
					//myPacDirection = new Point(0, 0);
				}
				else
				{
					myPacDirection = GetDirectionSigns(myPac.previousOrigin, myPac.origin);
				}
			}

			//Special case for first turn
			if (otherPac.previousOrigin == null)
			{
				return false;
				//otherPacDirection = new Point(0, 0);
			}
			else
			{
				otherPacDirection = GetDirectionSigns(otherPac.previousOrigin, otherPac.origin);
			}

			//either other pac is stationary, or distance in x or y between two pacs is getting smaller
			bool result = (otherPacDirection.x == 0 && otherPacDirection.y == 0) ||
							((myPacDirection.x == -otherPacDirection.x) && Math.Abs(myPac.origin.x + myPacDirection.x - otherPac.origin.x - otherPacDirection.x) < Math.Abs(myPac.origin.x - otherPac.origin.x)) ||
							((myPacDirection.y == -otherPacDirection.y) && Math.Abs(myPac.origin.y + myPacDirection.y - otherPac.origin.y - otherPacDirection.y) < Math.Abs(myPac.origin.y - otherPac.origin.y));
			Console.Error.WriteLine("Pac directions. myPac:" + myPac.id.ToString() + " direction:" + myPacDirection.ToString() + " other pac:" + otherPac.id.ToString() + " direction:" + otherPacDirection.ToString());
			return result;
		}
	}
}
