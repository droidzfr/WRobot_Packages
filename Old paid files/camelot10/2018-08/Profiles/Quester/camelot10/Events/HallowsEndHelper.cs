#if VISUAL_STUDIO
using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using wManager.Wow.Bot.Tasks;
using wManager.Wow.Class;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using wManager.Wow.Enums;
#endif

public class HallowsEndHelper : QuestClass
{
	static Vector3 lastMoveFrom = Vector3.Zero;
	static Vector3 lastMoveTo = Vector3.Zero;
	static Thread thread = null;
	public static float StopFlyDistance = 125f;
	public static float StopFlyDistanceZ = 9999f;
	static bool blockMovement = false;
	public HallowsEndHelper()
	{
		Name = "Hallow's End Helper";
		QuestId.Add(0);
		Step.AddRange(new[] { 0 });
		System.Threading.Tasks.Task.Delay(1000).ContinueWith(t => Start());
	}
	// Continent (item1), DefaultPosition (item2), DefaultPositionSearchRange (item3), NewPosition (item4)
	static List<System.Tuple<ContinentId, Vector3, float, Vector3, BooleanDelegate>> positionChange = new List<System.Tuple<ContinentId, Vector3, float, Vector3, BooleanDelegate>>
	{
		new System.Tuple<ContinentId, Vector3, float, Vector3, BooleanDelegate>(
			ContinentId.Kalimdor,
			new Vector3(-4626.4, -3172.9, 41.3),
			3.4f,
			new Vector3(-4626.683, -3176.371, 40.7935),
			(c)=> true
		),
		new System.Tuple<ContinentId, Vector3, float, Vector3, BooleanDelegate>(
			ContinentId.Northrend,
			new Vector3(8430.461, -352.6517, 962.9901),
			25f,
			new Vector3(8425.673, -292.8407, 903.3326, "None"),
			(c) => !Quest.GetQuestCompleted(13462)
		),
		new System.Tuple<ContinentId, Vector3, float, Vector3, BooleanDelegate>(
			ContinentId.Expansion01,
			new Vector3(-4081.246, 2187.736, 107.5033, "None"),
			3f,
			new Vector3(-4082.813, 2184.295, 107.5031, "None"),
			(c) => true
		),
	};
	static void OnMovementPulse(List<Vector3> points, System.ComponentModel.CancelEventArgs cancelable)
	{
		cancelable.Cancel = blockMovement;
		var continent = (ContinentId)Usefuls.ContinentId;
		foreach (var p in points)
		{
			foreach (var pchange in positionChange)
			{
				if (p != null && pchange.Item1 == continent && pchange.Item5(null) && p.DistanceTo(pchange.Item2) <= pchange.Item3)
				{
					Log("change path position of " + p.ToStringNewVector() + " to " + pchange.Item4.ToStringNewVector() +"");
					p.X = pchange.Item4.X;
					p.Y = pchange.Item4.Y;
					p.Z = pchange.Item4.Z;
					p.Type = pchange.Item4.Type;
					p.Action = pchange.Item4.Action;
				}

			}
		}
	}
	static void OnMoveToPulse(Vector3 point, System.ComponentModel.CancelEventArgs cancelable)
	{
		cancelable.Cancel = blockMovement;
	}
	static void Routine()
	{
		while (robotManager.Products.Products.IsStarted)
		{
			if (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause)
			{
				var me = ObjectManager.Me;
				if (me.IsValid && me.IsAlive && !me.IsOnTaxi)
				{
					if (Bag.GetContainerNumFreeSlots > 3 && ItemsManager.HasItemById(37586))
					{
						Log("open Handful of Treats and update quests");
						ItemsManager.UseItem(37586);
						Thread.Sleep(Others.Random(1000, 2000));
						Quest.ConsumeQuestsCompletedRequest();
					}
					var nearTurnInNPC = Quest.QuesterCurrentContext.NPCList.OrderBy(npc => npc.Position.DistanceTo2D(ObjectManager.Me.Position)).FirstOrDefault();
					if (nearTurnInNPC != null)
					{
						var dist = nearTurnInNPC.Position.DistanceTo2D(ObjectManager.Me.Position);
						var distZ = nearTurnInNPC.Position.DistanceZ(ObjectManager.Me.Position);
						if (Var.GetVar<bool>("HallowEndDebug"))
							Log("near npc=" + nearTurnInNPC.Name + "("+ nearTurnInNPC.Id + ") dist=" + dist + " distZ=" + distZ + " needLand="+ (dist < StopFlyDistance - 1) +" stopFly=" + StopFlyDistance );

						if (dist < StopFlyDistance - 1)
						{
							if (nearTurnInNPC.CanFlyTo)
								SetAllNpcCanFly(false);

							if (distZ < StopFlyDistanceZ && ObjectManager.Me.IsMounted)
							{
								Log("stop mount");
								SetMount(false);
							}
						}
						else if (dist > StopFlyDistance + 1)
						{
							if (!nearTurnInNPC.CanFlyTo)
								SetAllNpcCanFly(true);

							if (!ObjectManager.Me.IsMounted)
							{
								Log("start mount");
								SetMount(true);
								Thread.Sleep(Others.Random(5000, 7000));
							}
						}
					}
					//Log("TEST longmove=" + LongMove.IsLongMove + " moveTo="+MovementManager.InMoveTo + " inMove=" + MovementManager.InMovement + " inMoveLoop="+ MovementManager.InMovementLoop + " inMoveNoLoop=" + MovementManager.InMovementNoLoop );
					/*
					if (Lua.LuaDoString<bool>("return IsFlyableArea()"))
					{
						var from = me.Position;
						var to = from;
						to.Z += 100f;
						if (!TraceLine.TraceLineGo(from, to, CGWorldFrameHitFlags.HitTestAll))
						{
						}
					}
					//*/
				}
			}
			Thread.Sleep(1000);
		}
	}
	static void SetMount(bool val)
	{
		blockMovement = true;
		LongMove.StopLongMove();
		MovementManager.StopMoveTo(true, 3000);
		MovementManager.StopMoveTo();
		MovementManager.StopMove();
		MovementManager.StopMoveNewThread();
		MovementManager.StopMoveToNewThread();
		//MountTask.Land();
		Log("Change mount=" + val+" mounted=" + ObjectManager.Me.IsMounted + " isFlying=" + ObjectManager.Me.IsFlying + " longmove=" + LongMove.IsLongMove);
		MountTask.DismountMount(true, true);

		wManager.wManagerSetting.CurrentSetting.UseFlyingMount = val;
		wManager.wManagerSetting.CurrentSetting.UseGroundMount = val;
		wManager.wManagerSetting.CurrentSetting.UseMount = val;
		blockMovement = false;
	}
	static void OnProductStop(string productName)
	{
		Stop();
	}
	static void SetAllNpcCanFly(bool val)
	{
		var npcList = Quest.QuesterCurrentContext.NPCList;
		foreach (var npc in npcList)
		{
			npc.CanFlyTo = val;
		}
		Log("all quest npc ("+ npcList.Count+") can fly =" + val);
	}
	public static void Start()
	{
		Stop();
		ResetSettings();
		wManager.Events.MovementEvents.OnMovementPulse += OnMovementPulse;
		wManager.Events.MovementEvents.OnMoveToPulse += OnMoveToPulse;
		robotManager.Events.ProductEvents.OnProductStopped += OnProductStop;
		thread = new Thread(Routine);
		thread.Start();
		SetAllNpcCanFly(false);
		Log("started");
	}
	public static void Stop()
	{
		wManager.Events.MovementEvents.OnMovementPulse -= OnMovementPulse;
		wManager.Events.MovementEvents.OnMoveToPulse -= OnMoveToPulse;
		robotManager.Events.ProductEvents.OnProductStopped -= OnProductStop;
		if (thread != null)
		{
			thread.Abort();
			thread = null;
		}
		Log("stopped");
	}
	public static void FixWestfallFork()
	{
		if (Quest.GetQuestCompleted(26322))
		{
			var npc = Quest.QuesterCurrentContext.NPCList.FirstOrDefault(n => n.TurnInQuests.Contains(12340));
			var old = npc.Position;
			npc.Position = new Vector3(-10503.49, 1030.581, 60.52073, "None");
			Log("Westfall backet position changed from " + old.ToStringNewVector() + " to " + npc.Position.ToStringNewVector());
		}
	}
	public static void FixReputationForks()
	{
		var scryers = new List<int>()
		{
			190111,
			190116,
		};
		var aldor = new List<int>()
		{
			190110,
			190115,
		};
		var canAlodr = CanAldor;
		var canScryers = CanScryers;
		foreach (var npc in Quest.QuesterCurrentContext.NPCList)
		{
			if (canScryers)
			{
				if (aldor.Contains(npc.Id))
				{
					Log(" (Hook) Clear NPC='" + npc.Name + "' PickUp and TurnIn quests, because player choosed Scryers.");
					npc.PickUpQuests.Clear();
					npc.TurnInQuests.Clear();
				}
			}
			else if (canAlodr)
			{
				if (scryers.Contains(npc.Id))
				{
					Log(" (Hook) Clear NPC='" + npc.Name + "' PickUp and TurnIn quests, because player choosed Aldor.");
					npc.PickUpQuests.Clear();
					npc.TurnInQuests.Clear();
				}
			}
		}
	}
	public static bool CanAldor
	{
		get
		{
			List<string> values = GetReputation(932);
			if (values.Count == 0)
				return false;

			int level = int.Parse(values[2]);
			return level > 3;
		}
	}
	public static bool CanScryers
	{
		get
		{
			List<string> values = GetReputation(934);
			if (values.Count == 0)
				return false;

			int level = int.Parse(values[2]);
			return level > 3;
		}
	}
	/// <summary>
	/// COPY FROM QUESTING.CS
	/// 0 = name, 1 = description
	/// 2 = standingID (1 - Hated, 2 - Hostile, 3 - Unfriendly, 4 - Neutral, 5 - Friendly, 6 - Honored, 7 - Revered, 8 - Exalted),
	/// 3 = barMin, 4 = barMax, 5 = barValue
	/// </summary>
	/// <param name="factionID"></param>
	/// <returns></returns>
	public static List<string> GetReputation(int factionID)
	{
		var lua = @"
local name, description, standingID, barMin, barMax, barValue, atWarWith, canToggleAtWar, isHeader, isCollapsed, hasRep, isWatched, isChild, factionID, hasBonusRepGain, canBeLFGBonus = GetFactionInfoByID({0});
return name .. '{1}' .. description .. '{1}' .. standingID .. '{1}' .. barMin .. '{1}' .. barMax .. '{1}' .. barValue;
";
		var runCode = string.Format(lua, factionID, Lua.ListSeparator);
		var result = Lua.LuaDoString<List<string>>(runCode);
		return result;
	}
	public static void ResetSettings()
	{
		StopFlyDistance = 125f;
		StopFlyDistanceZ = 9999f;
		wManager.wManagerSetting.CurrentSetting.SkinNinja = false;
		wManager.wManagerSetting.CurrentSetting.SkinMobs = false;
		wManager.wManagerSetting.CurrentSetting.HarvestHerbs = false;
		wManager.wManagerSetting.CurrentSetting.HarvestMinerals = false;
		wManager.wManagerSetting.CurrentSetting.HarvestTimber = false;
		CVar.SetCVar("autoDismount", "1");
		CVar.SetCVar("autoDismountFlying", "1");
		CVar.SetCVar("autoLootDefault", "1");
		CVar.SetCVar("autounshift", "1");
		Lua.LuaDoString("SetAutoDeclineGuildInvites(true)");
		wManager.wManagerSetting.CurrentSetting.FlightMasterTaxiUseOnlyIfNear = true;
		wManager.wManagerSetting.CurrentSetting.AquaticMountName = ""; //aquatic mount cause problems with moving in waters
		Log("settings changed");
	}
	static void Log(string text)
	{
		Logging.WriteDebug("[Hallow's End Helper] " + text);
	}
}