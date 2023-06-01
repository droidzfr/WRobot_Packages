//FISHER START
#if VISUAL_STUDIO
using robotManager.Helpful;
using System;
using System.Linq;
using System.Collections.Generic;
using wManager.Wow.Class;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using System.Threading;
using wManager.Wow.Bot.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Configuration;
using System.IO;
#endif

public class Fisher
{
	static int BlacklistMinutes = 3;
	static int HotspotIndex = 0;
	static robotManager.Helpful.Timer _timer = new robotManager.Helpful.Timer(4 * 60 * 1000);
	static MemoryRobot.Int128 _lastGuid = MemoryRobot.Int128.Zero();
	public static bool UsePole = true;
	public static bool UseSkillUpFishes = true;
	public static bool UsePowerUpFishes = true;
	public static bool CanWalkOnWater = false;
	public Fisher()
	{
		ResetSettings();
		SaveWeaponName();
		SubscribeFishingEvents();
		Var.SetVar("Cameleto10Fisher", true);
	}
	public static void Log(string text)
	{
		Logging.WriteDebug("[Fisher] " + text);
	}
	public static void ResetSettings()
	{
		Log("reset settings");
	}
	//farm poolId on path
	public static void Loop(List<Vector3> path, int poolId)
	{
		while (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause && !Conditions.IsAttackedAndCannotIgnore)
		{
			MovementManager.GoLoop(path);
			Thread.Sleep(1000);
			TryFishNode(poolId);
		}
	}
	public static void Hotspots(List<Vector3> hotspots, int poolId, bool isRandom = false)
	{
		Hotspots(hotspots, new List<int>() { poolId }, isRandom);
	}
	public static void Hotspots(List<Vector3> hotspots, List<int> poolIDs, bool isRandom = false)
	{
		if (TryFishNode(poolIDs))
			return;

		Vector3 p = new Vector3();
		if (isRandom)
		{
			p = hotspots[Others.Random(0, hotspots.Count - 1)];
		}
		else
		{
			HotspotIndex += 1;
			if (HotspotIndex >= hotspots.Count)
				HotspotIndex = 0;
			if (HotspotIndex < 0)
				HotspotIndex = hotspots.Count - 1;

			p = hotspots[HotspotIndex];
		}
		GoToTask.ToPosition(p, 3.5f, false, (c) =>
		{
			if (Conditions.IsAttackedAndCannotIgnore)
				return false;

			var pool = ObjectManager.GetNearestWoWGameObject(ObjectManager.GetWoWGameObjectByEntry(poolIDs));
			if (pool != null && pool.IsValid)
				return false;

			return true;
		});
	}
	public static bool TryFishNode(int poolId)
	{
		return TryFishNode(new List<int>() { poolId });
	}
	public static bool TryFishNode(List<int> poolIDs)
	{
		//var pool = ObjectManager.GetObjectWoWGameObject().Where(o => o.IsValid && o.Entry == poolId).OrderBy(o => o.GetDistance2D).FirstOrDefault();
		var pool = ObjectManager.GetNearestWoWGameObject(ObjectManager.GetWoWGameObjectByEntry(poolIDs));
		if (pool == null)
		{
			//Log("no pool");
			return false;
		}

		var fishPosition = Fishing.FindTheUltimatePoint(pool.Position, CanWalkOnWater);
		var zero = new Vector3();
		if (zero == fishPosition)
		{
			Log("no pool point. blacklist");
			wManager.wManagerSetting.AddBlackList(pool.Guid, BlacklistMinutes * 60 * 1000);
			return false;
		}

		var poolGUID = pool.Guid;
		var poolPosition = pool.Position;
		while (pool != null && pool.IsValid && Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause && !Conditions.IsAttackedAndCannotIgnore)
		{
			if (_lastGuid != poolGUID)
			{
				_lastGuid = poolGUID;
				_timer.Reset();
			}
			else if (_timer.IsReady)
			{
				Log("timer run out. blacklist");
				wManager.wManagerSetting.AddBlackList(poolGUID, BlacklistMinutes * 60 * 1000);
				_lastGuid = MemoryRobot.Int128.Zero();
				pool = null;
				return false;
			}

			if (!FishingTask.IsLaunched)
			{
				/*
				var result = false;
				var path = PathFinder.FindPath(fishPosition, out result);
				if (!result)
				{
					Log("no path to pool point. blacklist");
					wManager.wManagerSetting.AddBlackList(pool.Guid, BlacklistMinutes * 60 * 1000);
					return false;
				}
				var pathDist = 0f;
				for (int j = 1; j < path.Count; j++)
				{
					pathDist += path[j].DistanceTo(path[j - 1]);
				}
				if (pathDist > wManager.wManagerSetting.CurrentSetting.SearchRadius)
				{
					Log("too far pool point. blacklist");
					wManager.wManagerSetting.AddBlackList(pool.Guid, BlacklistMinutes * 60 * 1000);
					return false;
				}
				//*/
				if (GoToTask.ToPosition(fishPosition, 1.5f, false, (c) =>
				{
					if (Conditions.IsAttackedAndCannotIgnore)
						return false;

					var otherPool = ObjectManager.GetNearestWoWGameObject(ObjectManager.GetWoWGameObjectByEntry(poolIDs));
					if (otherPool != null && otherPool.IsValid && otherPool.Guid != poolGUID && Fishing.FindTheUltimatePoint(otherPool.Position, CanWalkOnWater) != Vector3.Zero)
					{
						pool = null;
						Log("found other pool " + otherPool.Name);
						return false;
					}
					/*
					if (pool == null || !pool.IsValid)
						return false;
					//*/

					return true;
				}))
				{
					if (pool == null || !pool.IsValid)
						return false;

					MountTask.DismountMount();
					ClickToMove.CGPlayer_C__ClickToMove(fishPosition.X, fishPosition.Y, fishPosition.Z, MemoryRobot.Int128.Zero(), (int)ClickToMoveType.Move, 0.5f);
					Thread.Sleep(Others.Random(500, 1000));
					MovementManager.Face(pool);
					Thread.Sleep(Usefuls.Latency * 2);
					Move.Forward(Move.MoveAction.PressKey, Others.Random(50, 100));
					if (MovementManager.IsFacing(ObjectManager.Me.Position, ObjectManager.Me.Rotation, poolPosition))
					{
						Log("start on " + pool.Name);
						FishingTask.LoopFish(poolGUID, false, "", "", 600000, "", 3600000, 0, 0, 0, true);
					}
					else
					{
						//Log("pool vanish");
						pool = null;
					}
				}
			}
			Thread.Sleep(500);
			if (TraceLine.TraceLineGo(poolPosition) || ObjectManager.Me.IsSwimming)
			{
				Log("no line of sight to pool or swimming. blacklist");
				wManager.wManagerSetting.AddBlackList(poolGUID, BlacklistMinutes * 60 * 1000);
				pool = null;
				break;
			}
		}

		//Log("stop fishing node");
		Stop();
		return true;
	}
	public static bool TryFish(Vector3 position, float rotation)
	{
		if (FishingTask.IsLaunched)
			return true;

		if (GoToTask.ToPosition(position, 1.5f, false, (c) =>
		{
			if (Conditions.IsAttackedAndCannotIgnore)
				return false;

			return true;
		}))
		{
			MountTask.DismountMount();
			ObjectManager.Me.Rotation = rotation;
			Move.Forward(Move.MoveAction.PressKey, 50);
			Thread.Sleep(Others.Random(50, 100));
			if (ObjectManager.Me.IsSwimming)
			{
				Move.Backward(Move.MoveAction.PressKey, 50);
				Thread.Sleep(Others.Random(50, 100));
			}
			FishingTask.LoopFish();
		}
		return true;
	}
	public static void Stop()
	{
		if (FishingTask.IsLaunched)
		{
			Log("stop");
			FishingTask.StopLoopFish();
		}

		if (ObjectManager.Me.PlayerUsingVehicle)
			Usefuls.EjectVehicle();

	}
	public static void SubscribeFishingEvents()
	{
		UnsubscribeFishingEvents();
		Log("events subscribed");
		robotManager.Events.LoggingEvents.OnAddLog += OnAddLog;
		robotManager.Events.ProductEvents.OnProductStopping += OnProductStopping;
		wManager.Events.FishingEvents.OnFishingPulse += OnFishingStart; // start fishing
		wManager.Events.FishingEvents.OnFishEnd += OnFishingEnd; // end of fishing
		//wManager.Events.FishingEvents.OnFishLoop += OnFishLoop; // pulse every loop
		wManager.Events.FishingEvents.OnFishSuccessful += OnFishingSuccessful; // catched something
		wManager.Events.FightEvents.OnFightStart += OnFightStart;
	}
	public static void UnsubscribeFishingEvents()
	{
		Log("events unsubscribed");
		robotManager.Events.LoggingEvents.OnAddLog -= OnAddLog;
		robotManager.Events.ProductEvents.OnProductStopping -= OnProductStopping;
		wManager.Events.FishingEvents.OnFishingPulse -= OnFishingStart; // start fishing
		wManager.Events.FishingEvents.OnFishEnd -= OnFishingEnd; // end of fishing
																 //wManager.Events.FishingEvents.OnFishLoop += OnFishLoop; // pulse every loop
		wManager.Events.FishingEvents.OnFishSuccessful -= OnFishingSuccessful; // catched something
		wManager.Events.FightEvents.OnFightStart -= OnFightStart;
	}
	static void OnAddLog(Logging.Log log)
	{
		// Logging.WriteDebug("[Quester] LoadProfile: " + Bot.Profile.QuestsSorted[i].NameClass);
		var logStartBy = "[Quester] LoadProfile: ";
		if (log != null && log.LogType == robotManager.Helpful.Logging.LogType.Debug && log.Text.StartsWith(logStartBy))
		{
			var profile = log.Text.Remove(0, logStartBy.Length).Trim();
			Log("Profile changed: " + profile);
			TryEquipWeapon();
			UnsubscribeFishingEvents();
		}
	}
	static void OnProductStopping(string productName)
	{
		TryEquipWeapon();
		UnsubscribeFishingEvents();
	}
	static void OnFightStart(WoWUnit unit, System.ComponentModel.CancelEventArgs cancelable)
	{
		//Log("# FIGHT START");
		TryEquipWeapon();
	}
	static void OnFishingStart(MemoryRobot.Int128 node, System.ComponentModel.CancelEventArgs cancelable)
	{
		//Log("@ START FISHING");
		TryEquipFishingPole();
	}
	static void OnFishingEnd()
	{
		//Log("@ END FISHING");
		//TryEquipWeapon();
		TryGatherBarrels();
	}
	static void OnFishingSuccessful(MemoryRobot.Int128 node)
	{
		//catched something
		TrySkillUpFishes();
		TryPowerUpFishes();
		//Log("@ OnFishSuccessful");
	}
	static void OnFishLoop(MemoryRobot.Int128 node)
	{ 
	}
	public static void TryEquipFishingPole()
	{
		if (!UsePole)
			return;

		if (!Fishing.IsEquipedFishingPoles())
		{
			foreach (var poleID in Items.Poles)
			{
				var poleName = ItemsManager.GetNameById(poleID);
				if (ItemsManager.GetItemCountByNameLUA(poleName) > 0)
				{
					Fishing.EquipFishingPoles(poleName);
					Thread.Sleep(Usefuls.Latency * 2);
					Log("equiped fishing pole: " + Fishing.FishingPolesName() +" must=" + poleName);
					return;
				}
				else
				{
				}
			}
		}
	}
	const string WEAPON_VAR = "Camelot10_Fisher_SavedWeapon";
	static void SaveWeaponName()
	{
		var weaponID = ObjectManager.Me.GetEquipedItemBySlot(InventorySlot.INVSLOT_MAINHAND);
		if (weaponID == 0)
		{
			Log("WARNING! No equipped weapon, its saved for later use when attacked while fishing");
			return;
		}
		else if (Var.Exist(WEAPON_VAR) && Var.IsType(WEAPON_VAR, typeof(uint)))
		{
			var weaponName = ItemsManager.GetNameById(weaponID);
			Log("Weapon already saved to=" + weaponName);
		}
		else
		{
			Var.SetVar(WEAPON_VAR, weaponID);
			var weaponName = ItemsManager.GetNameById(weaponID);
			Log("Weapon saved to=" + weaponName);
		}
	}
	static void TryEquipWeapon()
	{
		if (!Var.Exist(WEAPON_VAR))
		{
			//Log(">>>TRY EQUIP WEAPON BUT THERE IS NO VAR");
			return;
		}

		if (!Var.IsType(WEAPON_VAR, typeof(uint)))
		{
			//Log(">>>TRY EQUIP WEAPON BUT ITS NOT UINT " + Var.GetVar<string>(WEAPON_VAR));
			return;
		}
		if (FishingTask.IsLaunched)
		{
			Log("abort fishing");
			FishingTask.StopLoopFish();
			Thread.Sleep(Usefuls.Latency * 2);
		}
		var weaponID = Var.GetVar<uint>(WEAPON_VAR);
		var weaponName = ItemsManager.GetNameById(weaponID);
		if (ObjectManager.Me.GetEquipedItemBySlot(InventorySlot.INVSLOT_MAINHAND) != weaponID)
		{
			Log("equip weapon=" + weaponName);
			ItemsManager.EquipItemByName(weaponName);
		}
	}
	public static void TrySkillUpFishes()
	{
		if (!UseSkillUpFishes)
			return;

		var fishing = Skill.GetValue(SkillLine.Fishing);
		if (fishing > 800)
			return;

		var fishingMax = Skill.GetMaxValue(SkillLine.Fishing);
		if (fishing >= fishingMax)
			return;

		foreach (var fishID in Items.SkillUpFishes)
		{
			if (UseItem(fishID))
				Thread.Sleep(Usefuls.Latency * 2);
		}
	}
	public static void TryPowerUpFishes()
	{
		if (!UsePowerUpFishes)
			return;

		foreach (var fishID in Items.ArtifactPowerFishes)
		{
			if (UseItem(fishID))
				Thread.Sleep(Usefuls.Latency * 2);
		}
	}
	static bool UseItem(uint itemID)
	{
		if (!ItemsManager.HasItemById(itemID))
			return false;

		MountTask.DismountMount();
		ItemsManager.UseItem(itemID);
		Usefuls.WaitIsCasting();
		return true;
	}
	public static class Items
	{
		//poles (order by +fishing)
		public static uint UnderlightAngler = 133755; //+60
		public static uint EphemeralFishingPole = 118381; //+100
		public static uint ArcaniteFishingPole = 19970; //+40
		public static uint MastercraftKaluakFishingPole = 44050; //+30
		public static uint DraenicFishingPole = 116826; //+30
		public static uint SavageFishingPole = 116825; //+30
		public static uint DragonFishingPole = 84661; //+30
		public static uint BoneFishingPole = 45991; //+30
		public static uint JeweledFishingPole = 45992; //+30
		public static uint NatsLuckyFishingPole = 45858; //+25
		public static uint SethsGraphiteFishingPole = 25978; //+20
		public static uint NatPaglesExtremeAnglerFC5000 = 19022; //+20
		public static uint BigIronFishingPole = 6367; //+20
		public static List<uint> Poles = new List<uint>()
		{
			UnderlightAngler,
			EphemeralFishingPole,
			ArcaniteFishingPole,
			MastercraftKaluakFishingPole,
			DraenicFishingPole,
			SavageFishingPole,
			DragonFishingPole,
			BoneFishingPole,
			JeweledFishingPole,
			NatsLuckyFishingPole,
			SethsGraphiteFishingPole,
			NatPaglesExtremeAnglerFC5000,
			BigIronFishingPole,
		};
		//fishes artifact power
		public static uint ThunderingStormrayAP = 139663;
		public static uint ThornedFlounderAP = 139656;
		public static uint TerrorfinAP = 139655;
		public static uint TaintedRunescaleKoiAP = 139666;
		public static uint SeerspinePufferAP = 139665;
		public static uint SeabottomSquidAP = 139668;
		public static uint OodelfjiskAP = 139661;
		public static uint NarthalasHermitAP = 139653;
		public static uint AncientHighmountainSalmonAP = 139660;
		public static uint MagicEaterFrogAP = 139664;
		public static uint LeyshimmerBlennyAP = 139652;
		public static uint GraybellyLobsterAP = 139662;
		public static uint GhostlyQueenfishAP = 139654;
		public static uint ColdriverCarpAP = 139659;
		public static uint AxefishAP = 139667;
		public static uint AncientMossgillAP = 139657;
		public static uint AncientBlackBarracudaAP = 139669;
		public static uint MountainPufferAP = 139658;
		public static List<uint> ArtifactPowerFishes = new List<uint>()
		{
			ThunderingStormrayAP,
			ThornedFlounderAP,
			TerrorfinAP,
			TaintedRunescaleKoiAP,
			SeerspinePufferAP,
			SeabottomSquidAP,
			OodelfjiskAP,
			NarthalasHermitAP,
			AncientHighmountainSalmonAP,
			MagicEaterFrogAP,
			LeyshimmerBlennyAP,
			GraybellyLobsterAP,
			GhostlyQueenfishAP,
			ColdriverCarpAP,
			AxefishAP,
			AncientMossgillAP,
			AncientBlackBarracudaAP,
			MountainPufferAP,
		};
		// fishes skill
		public static uint LeyshimmerBlenny = 133725;
		public static uint NarthalasHermit = 133726;
		public static uint GhostlyQueenfish = 133727;
		public static uint ThornedFlounder = 133729;
		public static uint AncientMossgill = 133730;
		public static uint Terrorfin = 133728;
		public static uint MountainPuffer = 133731;
		public static uint AncientHighmountainSalmon = 133733;
		public static uint ColdriverCarp = 133732;
		public static uint Oodelfjisk = 133734;
		public static uint ThunderingStormray = 133736;
		public static uint GraybellyLobster = 133735;
		public static uint SeerspinePuffer = 133738;
		public static uint TaintedRunescaleKoi = 133739;
		public static uint MagicEaterFrog = 133737;
		public static uint AncientBlackBarracuda = 133742;
		public static uint Axefish = 133740;
		public static uint SeabottomSquid = 133741;
		public static List<uint> SkillUpFishes = new List<uint>()
		{
			//azsuna
			LeyshimmerBlenny,
			NarthalasHermit,
			GhostlyQueenfish,
			//valsharah
			AncientMossgill,
			ThornedFlounder,
			Terrorfin,
			//highmountain
			MountainPuffer,
			AncientHighmountainSalmon,
			ColdriverCarp,
			//stormheim
			Oodelfjisk,
			ThunderingStormray,
			GraybellyLobster,
			//suramar
			SeerspinePuffer,
			TaintedRunescaleKoi,
			MagicEaterFrog,
			//ocean
			AncientBlackBarracuda,
			Axefish,
			SeabottomSquid,
		};
	}
	static void TryGatherBarrels()
	{
		var barrel = ObjectManager.GetNearestWoWGameObject(ObjectManager.GetWoWGameObjectByEntry(capturedFishes));
		if (barrel != null && barrel.IsValid && barrel.CreatedBy == ObjectManager.Me.Guid)
		{
			GoToTask.ToPositionAndIntecractWithGameObject(barrel.Position, barrel.Entry);
		}
	}
	static List<int> capturedFishes = new List<int>()
	{
		//barakuda http://www.wowhead.com/object=251390/captured-fish
		//cursed queenfish http://www.wowhead.com/object=251356/captured-fish
		251390,
		243563,
		251356,
		251386,
		251387,
		251388,
		251389,
		247851,
	};

	public static class Fight
	{
		public static void StartFix()
		{
			wManager.Events.FightEvents.OnFightLoop += OnFightLoop;
			Log("fight fix enabled");
		}
		public static void StopFix()
		{
			wManager.Events.FightEvents.OnFightLoop -= OnFightLoop;
			Log("fight fix disabled");
		}
		static void OnFightLoop(WoWUnit unit, System.ComponentModel.CancelEventArgs cancelable)
		{
			if (unit != null && unit.IsValid && unit.IsAlive && unit.IsSwimming && !ObjectManager.Me.IsSwimming)
			{
				Log("fight bug. mob is swimming but im not. move to mob");
				//cancelable.Cancel = true;
				GoToTask.ToPosition(unit.Position);
			}
		}
	}



}