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
#endif

public class Traveler : QuestClass
{
	public static uint GarrisonHearthstone = 110560;

	static Traveler()
	{
		ResetSettings();
		Var.SetVar("Cameleto10Traveler", true);
	}
	static void Log(string message)
	{
		Logging.Write("[Traveler] " + message);
	}
	public delegate bool BooleanDelegate();
	public static void ResetSettings()
	{
		wManager.wManagerSetting.AddBlackListZone(new Vector3(-844.5972, 4467.76, 736.0415), 7, false); //dalaran center, teleportation room teleporter
		var config = wManager.wManagerSetting.CurrentSetting;
		config.CloseIfPlayerTeleported = false;
		config.UseFlyingMount = true;
		config.UseGroundMount = true;
		config.UseMount = true;
		config.IgnoreFightGoundMount = true;
		CVar.SetCVar("autoDismount", "1");
		CVar.SetCVar("autoDismountFlying", "1");
		Conditions.ForceIgnoreIsAttacked = false;
		Log("reset settings");
	}

	public static int MapID
	{
		get
		{
			return Lua.LuaDoString<int>("local mapID, isContinent = GetCurrentMapAreaID(); return mapID;");
		}
	}
	public static int ZoneId { get { return MapID; } }

	/// <summary>
	/// try to take taxi to destination, true if reach destination, false if cannot
	/// </summary>
	/// <param name="continentID"></param>
	/// <param name="mapID"></param>
	/// <param name="nearestPosition"></param>
	/// <returns></returns>
	public static bool TryTaxi(int continentID, int mapID, Vector3 nearestPosition, bool onlyIfCanMakePathFromTaxiToPos = true)
	{
		WaitTaxi();

		if (!wManager.wManagerSetting.CurrentSetting.FlightMasterTaxiUse)
			return false;

		if (Usefuls.ContinentId != continentID)
			return false;

		if (mapID == MapID)
			return true;

		Log("try to take taxi to ingame map=" + mapID + " (current=" + MapID + ")");
		var nodes = Taxi.TaxiList.Nodes.Where(n => n.ContinentId == Usefuls.ContinentId).OrderBy(n => n.Position.DistanceTo(ObjectManager.Me.Position)).ToList();
		var checkNodesCount = 3;
		var count = nodes.Count >= checkNodesCount ? checkNodesCount : nodes.Count;
		if (count < 1)
		{
			Log("no taxi nodes");
			return false;
		}
		TaxiNode neareastNode = null;
		var neareastNodePathDistance = float.MaxValue;
		for (int i = 0; i < count; i++)
		{
			var node = nodes[i];
			var result = false;
			var path = PathFinder.FindPath(node.Position, out result);
			if (!result)
			{
				Log("skip node #" + i + ". no path to node: " + node.Name);
				continue;
			}
			var pathDist = 0f;
			for (int j = 1; j < path.Count; j++)
			{
				pathDist += path[j].DistanceTo(path[j - 1]);
			}
			if (pathDist > 0f && pathDist < neareastNodePathDistance)
			{
				neareastNode = node;
				neareastNodePathDistance = pathDist;
				Log("choosed node #" + i + " node: " + node.Name + " path dist=" + neareastNodePathDistance);
			}
			else
			{
				Log("skip node #" + i + " node: " + node.Name + " path dist=" + pathDist);
			}
		}
		if (neareastNode == null || !neareastNode.IsValid())
		{
			Log("cannot find nearest node");
			return false;
		}
		var p = neareastNode.Position;
		var skip = wManager.wManagerSetting.CurrentSetting.BlackListIfNotCompletePath;
		if (!GoToTask.ToPosition(p, 15f, skip, null))
		{
			Log("cannot reach node position " + p + " ");
			return false;
		}
		var flightmaster = wManager.Wow.Bot.States.FlightMasterTakeTaxiState.GetNearestFlightMaster();
		if (!flightmaster.IsValid)
		{
			Log("invalid flighmaster");
			return false;
		}
		if (!GoToTask.ToPositionAndIntecractWith(flightmaster, -1, skip, null, false))
		{
			Log("cannot interact flighmaster");
			return false;
		}
		Thread.Sleep(Usefuls.Latency + 800);
		if (!Taxi.TakeNearestNode(nearestPosition, onlyIfCanMakePathFromTaxiToPos))
		{
			Log("cannot fly to destination");
			return false;
		}
		Thread.Sleep(Usefuls.Latency + 800);
		WaitTaxi();
		return mapID == MapID;
	}
	public static void WaitTaxi()
	{
		while (robotManager.Products.Products.IsStarted)
		{
			if (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause)
			{
				if (!ObjectManager.Me.IsOnTaxi)
					break;
			}
			Thread.Sleep(1000);
		}
	}
	public static void UseHeathstone()
	{
		var name = SpellListManager.SpellNameInGameById(94719);
		Lua.RunMacroText("/cast " + name);
		Thread.Sleep(Usefuls.Latency * 2);
		if (ObjectManager.Me.IsCast)
		{
			Log("Heathstoning(" + name + ")");
			Thread.Sleep(Others.Random(5000, 7000));
			Usefuls.WaitIsCasting();
		}
		else if (ItemsManager.HasItemById(6948))
		{
			Log("Heathstoning");
			ItemsManager.UseItem(6948);
			Thread.Sleep(Others.Random(5000, 7000));
			Usefuls.WaitIsCasting();
		}
		else
		{
			Log("WARNING! Dont have Heathstone or The Innkeeper's Daughter");
			Thread.Sleep(5000);
		}
	}
	public static void UseHeathstoneDalaran()
	{
		if (ItemsManager.HasItemById(140192))
		{
			MountTask.DismountMount();
			ItemsManager.UseItem(140192);
			Thread.Sleep(Others.Random(5000, 7000));
			Usefuls.WaitIsCasting();
		}
		else
		{
			Log("WARNING! Dont have " + ItemsManager.GetNameById(140192));
			Thread.Sleep(Others.Random(5000, 7000));
		}
	}
	/// <summary>
	/// Copy from Questing
	/// </summary>
	public static void Goto(Vector3 position, float minDist = 10, float minDistFly = 100, int maxFlyTimeMins = 30)
	{
		var dist = ObjectManager.Me.Position.DistanceTo(position);
		if (dist > minDistFly && Lua.LuaDoString<bool>("return IsFlyableArea()"))
		{
			Log("goto (long move): " + position);
			MovementManager.StopMove();
			LongMove.StopLongMove();
			MountTask.Mount(true, MountTask.MountCapacity.Fly);
			Thread.Sleep(1 * 1000);
			LongMove.LongMoveGo(position, maxFlyTimeMins * 60 * 1000, minDistFly);
			Thread.Sleep(1 * 1000);
		}
		else if (dist > minDist)
		{
			Log("goto: " + position);
			MovementManager.StopMove();
			LongMove.StopLongMove();
			GoToTask.ToPosition(position);
		}
	}
	#region VANILLA
	public static bool InCapital
	{
		get
		{
			if (ObjectManager.Me.IsHorde)
			{
				if (!InKalimdor)
					return false;

				if (Usefuls.AreaId != (int)AreaId.Orgrimmar)
					return false;

				return true;
			}
			else if (ObjectManager.Me.IsAlliance)
			{
				if (!InEasternKingdoms)
					return false;

				if (Usefuls.AreaId != (int)AreaId.Stormwind)
					return false;

				return true;
			}
			return false;
		}
	}
	public static bool InCapitalOpen
	{
		get
		{
			if (!InCapital)
				return false;

			if (InOrgrimmarCleftOfShadow)
				return false;

			if (InStormwindWizardsSanctum)
				return false;

			return true;
		}
	}
	public static bool InOrgrimmarCleftOfShadow
	{
		get
		{
			if (!InKalimdor)
				return false;

			if (Usefuls.AreaId != (int)AreaId.Orgrimmar)
				return false;

			if (ObjectManager.Me.Position.DistanceTo2D(Coords.ORGRIMMAR_CLEFT_OF_SHADOW) > 100)
				return false;

			return ObjectManager.Me.Position.Z < Coords.ORGRIMMAR_CLEFT_OF_SHADOW.Z;
		}
	}
	public static void FromOrgrimmarCleftOfShadow()
	{
		if (InOrgrimmarCleftOfShadow)
		{
			Log("im in Orgrimmar, Cleft of Shadow. Move out");
			Move(Paths.FROM_ORGRIMMAR_CLEFT_OF_SHADOW, true);
		}
		else
		{
			Log("im not in Orgrimmar, Cleft of Shadow");
		}
	}
	public static bool InStormwindWizardsSanctum
	{
		get
		{
			if (!InEasternKingdoms)
				return false;

			if (Usefuls.AreaId != (int)AreaId.Stormwind)
				return false;

			if (ObjectManager.Me.Position.DistanceTo2D(Coords.STORMWIND_WIZARDS_SANCTUM) > 50)
				return false;

			return ObjectManager.Me.Position.DistanceZ(Coords.STORMWIND_WIZARDS_SANCTUM) < 10;
		}
	}
	public static void FromStormwindWizardsSanctum()
	{
		Log("in Stormwind Wizard's Sanctum, port out");
		MoveClick(Coords.STORMWIND_WIZARDS_SANCTUM_MOVEOUT, () => InStormwindWizardsSanctum);
		Log("in Stormwind Wizard's Sanctum, path out");
		Move(Paths.FROM_STORMWIND_WIZARDS_SANCTUM, true);
		Log("in Stormwind Wizard's Sanctum, move out");
	}
	public static void ToCapital()
	{
		if (InCapital)
		{
			ToCapitalOpen();
		}
		else if (InPandariaShrine)
		{
			Log("im in pandaria. going to capital portal");
			if (ObjectManager.Me.IsHorde)
				Portals.PANDARIA_ORGRIMMAR.Use();
			else
				Portals.PANDARIA_STORMWIND.Use();
		}
		else if (InBrokenIslesDalaran)
		{
			Log("im in dalaran. going to capital portal");
			if (ObjectManager.Me.IsHorde)
				Portals.BROKENISLES_ORGRIMMAR.Use();
			else
				Portals.BROKENISLES_STORMWIND.Use();
		}
		else
		{
			Log("im somewhere else. hearth to dalaran");
			ToBrokenIslesDalaran();
		}
	}
	public static void ToCapitalOpen()
	{
		if (InCapitalOpen)
		{
			Log("in faction capital open for flying");
			return;
		}
		else if (InStormwindWizardsSanctum)
		{
			Thread.Sleep(100);
			Log("in Stormwind Wizard's Sanctum, go out");
			FromStormwindWizardsSanctum();
		}
		else if (InOrgrimmarCleftOfShadow)
		{
			Thread.Sleep(100);
			Log("in Orgrimmar Cleft of Shadows, go out");
			FromOrgrimmarCleftOfShadow();
		}
		else
		{
			ToCapital();
		}
	}
	public static bool InKalimdor
	{
		get
		{
			return Usefuls.ContinentId == (int)ContinentId.Kalimdor;
		}
	}
	public static void ToKalimdor()
	{
		if (ObjectManager.Me.IsHorde)
			ToCapital();
		else if (ObjectManager.Me.IsAlliance)
			Instances.ToFirelandsNear();
	}
	public static bool InEasternKingdoms
	{
		get
		{
			return Usefuls.ContinentId == (int)ContinentId.Azeroth;
		}
	}
	public static void ToEasternKingdoms()
	{
		if (InEasternKingdoms)
		{
			Log("In Eastern Kingdoms");
		}
		else if (InCapital)
		{
			Log("In faction capital. use port to Blasted Lands");
			Portals.ORGRIMMAR_BLASTEDLANDS.Use();
		}
		else
		{
			Log("Not in Eastern Kingdoms. Goto Capital");
			ToCapital();
		}
	}
	public static bool InIronforge
	{
		get
		{
			if (!InEasternKingdoms)
				return false;

			return Usefuls.AreaId == (int) AreaId.Ironforge;
		}
	}
	public static bool ToIronforge()
	{
		if (InIronforge)
		{
			Log("in Ironforge. done");
			return true;
		}
		if (InPandariaShrine)
		{
			Log("To Ironforge. im in pandaria shrine. use portal to Ironforge");
			Portals.PANDARIA_IRONFORGE.Use();
		}
		else if (InBrokenIslesDalaran)
		{
			Log("To Ironforge. im in dalaran. use portal to Ironforge");
			Portals.BROKENISLES_IRONFORGE.Use();
		}
		else
		{
			Log("To Ironforge. im somewhere else. need to get in dalaran");
			ToBrokenIslesDalaran();
		}
		return false;
	}
	public static void ToSilvermoon()
	{
		if (ObjectManager.Me.IsAlliance)
		{
			Log("unable to goto silvermoon");
			return;
		}
		if (InPandariaShrine)
		{
			Log("im in pandaria shrine");
		}
		else if (InBrokenIslesDalaran)
		{
			Log("im in dalaran. going to pandaria portal");
			Portals.BROKENISLES_SILVERMOON.Use();
		}
		else
		{
			Log("im somewhere else. need to get in dalaran");
			ToBrokenIslesDalaran();
		}
	}
	public static bool InBloodElfStartZones
	{
		get
		{
			if (!InOutlands)
				return false;

			return (Usefuls.AreaId == (int)AreaId.Silvermoon || Usefuls.AreaId == (int)AreaId.EversongWoods || Usefuls.AreaId == (int)AreaId.Ghostlands);
		}
	}
	public static bool ToBloodElfStartZones()
	{
		if (ObjectManager.Me.IsAlliance)
		{
			Log("alliance cannot got to blood elf start zones.");
			return true;
		}
		if (InBloodElfStartZones)
		{
			Log("im in pandaria shrine");
			return true;
		}
		else if (InBrokenIslesDalaran)
		{
			Log("im in dalaran. going to silvermoon portal");
			Portals.BROKENISLES_SILVERMOON.Use();
		}
		else
		{
			Log("im somewhere else. need to get in dalaran");
			ToBrokenIslesDalaran();
		}
		return false;
	}
	#endregion VANILLA

	#region OUTLANDS
	public static bool InOutlands
	{
		get
		{
			return Usefuls.ContinentId == (int)ContinentId.Expansion01;
		}
	}

	public static bool InShattrath
	{
		get
		{
			if (!InOutlands)
				return false;
			return Usefuls.AreaId == (int)AreaId.Shattrath;
		}
	}

	public static void ToShattrath()
	{
		if (InShattrath)
		{
			Log("im in shattrath");
		}
		else if (InPandariaShrine)
		{
			Log("im in pandaria. going to shattrath portal");
			if (ObjectManager.Me.IsHorde)
				Portals.PANDARIA_SHATTRATH_HORDE.Use();
			else
				Portals.PANDARIA_SHATTRATH_ALLIANCE.Use();
		}
		else
		{
			Log("im somewhere else. need to get in pandaria shrine");
			ToPandariaShrine();
		}
	}

	public static bool InQuelDanas
	{
		get
		{
			if (!InOutlands)
				return false;

			return Usefuls.AreaId == (int)AreaId.QuelDanas;
		}
	}

	public static void ToQuelDanas()
	{
		if (InQuelDanas)
		{
			Log("im in Quel Danas");
		}
		else if (InShattrath)
		{
			Log("im in Shattrath. going to Quel Danas portal");
			Portals.SHATTRATH_QUELDANAS.Use();
		}
		else
		{
			Log("im somewhere else. need to get in pandaria shrine");
			ToShattrath();
		}
	}
	#endregion OUTLANDS

	#region NORTHREND
	public static bool InNorthrend
	{
		get
		{
			return (Usefuls.ContinentId == (int)ContinentId.Northrend);
		}
	}

	public static bool InNorthrendDalaran
	{
		get
		{
			if (!InNorthrend)
				return false;

			return (Usefuls.AreaId == (int)AreaId.DalaranNorthrend);
		}
	}

	public static void ToNorthrendDalaran()
	{
		if (InNorthrendDalaran)
		{
			Log("im in northrend dalaran");
		}
		else if (InPandariaShrine)
		{
			Log("im in pandaria. going to northrend dalaran portal");
			if (ObjectManager.Me.IsHorde)
				Portals.PANDARIA_DALARAN_NORTHREND_HORDE.Use();
			else
				Portals.PANDARIA_DALARAN_NORTHREND_ALLIANCE.Use();
		}
		else
		{
			Log("im somewhere else. need to get in pandaria shrine");
			ToPandariaShrine();
		}
	}
	#endregion NORTHREND

	#region CATACLYSM
	public static bool InTolBarad
	{
		get
		{
			return Usefuls.ContinentId == (int) ContinentId.TolBarad;
		}
	}
	public static bool ToTolBarad()
	{
		if (InTolBarad)
			return true;

		if (InCapitalOpen)
		{
			Log("im in capital open. use portal to Tol Barad");
			if (ObjectManager.Me.IsHorde)
			{
				Portals.ORGRIMMAR_TOLBARAD.Use();
			}
			else
			{
				Portals.STORMWIND_TOLBARAD.Use();
			}
			return false;
		}
		ToCapitalOpen();
		return false;
	}

	#endregion CATACLYSM

	#region PANDARIA
	public static bool InPandaria
	{
		get
		{
			return Usefuls.ContinentId == (int)ContinentId.HawaiiMainLand;
		}
	}

	public static bool InPandariaShrine
	{
		get
		{
			if (!InPandaria)
				return false;

			if (ObjectManager.Me.IsHorde)
				return Usefuls.AreaId == (int)AreaId.PandariaShrineHorde || Usefuls.AreaId == (int)AreaId.PandariaShrineHordeNear;
			else
				return Usefuls.AreaId == (int)AreaId.PandariaShrineAlliance || Usefuls.AreaId == (int)AreaId.PandariaShrineAllianceNear;
		}
	}

	public static void ToPandariaShrine()
	{
		if (InPandariaShrine)
		{
			Log("im in pandaria shrine");
		}
		else if (InBrokenIslesDalaran)
		{
			Log("im in dalaran. going to pandaria portal");
			if (ObjectManager.Me.IsHorde)
				Portals.BROKENISLES_PANDARIA_HORDE.Use();
			else
				Portals.BROKENISLES_PANDARIA_ALLAINCE.Use();
		}
		else
		{
			Log("im somewhere else. need to get in dalaran");
			ToBrokenIslesDalaran();
		}
	}

	public static void ToPandaria()
	{
		if (!InPandaria)
			ToPandariaShrine();
	}
	#endregion PANDARIA

	#region DRAENOR
	public static bool InDraenor
	{
		get
		{
			if (InGarrison)
				return true;
			return Usefuls.ContinentId == (int)ContinentId.Draenor;
		}
	}
	public static bool InGarrison
	{
		get
		{
			//horde
			if (Usefuls.ContinentId == (int)ContinentId.FWHordeGarrisonLevel1)
				return true;
			if (Usefuls.ContinentId == (int)ContinentId.FWHordeGarrisonLevel2)
				return true;
			if (Usefuls.ContinentId == (int)ContinentId.FWHordeGarrisonLeve2new)
				return true;
			if (Usefuls.ContinentId == (int)ContinentId.FWHordeGarrisonLevel3)
				return true;
			//alliance
			if (Usefuls.ContinentId == (int)ContinentId.SMVAllianceGarrisonLevel1)
				return true;
			if (Usefuls.ContinentId == (int)ContinentId.SMVAllianceGarrisonLevel2)
				return true;
			if (Usefuls.ContinentId == (int)ContinentId.SMVAllianceGarrisonLevel2new)
				return true;
			if (Usefuls.ContinentId == (int)ContinentId.SMVAllianceGarrisonLevel3)
				return true;

			return false;
		}
	}
	public static bool ToDraenor()
	{
		if (InDraenor)
		{
			Log("im in Dreanor. done");
			return true;
		}
		if (ItemsManager.HasItemById(GarrisonHearthstone))
		{
			Log("im not in Draenor but have Garrison hearthstone. use it for travel");
			ToGarrison();
			return false;
		}
		if (InCapitalOpen)
		{
			Log("im in Capital open, use portal to Asharan (to Draenor)");
			if (ObjectManager.Me.IsHorde)
				Portals.ORGRIMMAR_ASHRAN.Use();
			else
				Portals.STORMWIND_ASHRAN.Use();

			return false;
		}
		Log("Im not in Capital (to Draenor)");
		ToCapitalOpen();
		return false;
	}
	public static bool ToGarrison()
	{
		if (InGarrison)
		{
			Log("Im in Garrison");
			return true;
		}
		if (!ItemsManager.HasItemById(GarrisonHearthstone))
		{
			if (ObjectManager.Me.Level >= 100)
			{
				Log("i dont have Garrison hearthstone, travel by foots");
				ToDraenor();
			}
			else
			{
				Log("i cannot into Garrison, level too low");
				Thread.Sleep(5 * 1000);
			}
			return false;
		}
		var hearthstoneCD = Lua.LuaDoString<int>("local startTime, duration, enable = GetItemCooldown(" + GarrisonHearthstone + "); return startTime + duration - GetTime();");
		if (hearthstoneCD > 0)
			return false;

		if (ObjectManager.Me.IsMounted)
			MountTask.DismountMount();

		Log("To Garrison, use hearthstone");
		ItemsManager.UseItem(GarrisonHearthstone);
		Usefuls.WaitIsCasting();
		return false;
	}
	#endregion DRAENOR

	#region LEGION
	public static bool IsInBrokenIsles
	{
		get
		{
			return Usefuls.ContinentId == (int)ContinentId.Troll_Raid;
		}
	}
	public static bool InBrokenIslesDalaran
	{
		get
		{
			if (!IsInBrokenIsles)
				return false;
			return (Usefuls.AreaId == (int)AreaId.DalaranBrokenIsles);
		}
	}
	public static void ToBrokenIsles()
	{
		if (!IsInBrokenIsles)
			ToBrokenIslesDalaran();
	}
	public static void ToBrokenIslesDalaran()
	{
		if (!InBrokenIslesDalaran)
		{
			Log("im not in broken isles dalaran. using dalaran heathstone");
			UseHeathstoneDalaran();
		}
	}
	public static bool InDalaraPortraitRoom
	{
		get
		{
			if (!InBrokenIslesDalaran)
				return false;

			if (ObjectManager.Me.Position.DistanceTo2D(Coords.DALARAN_BROKENISLES_PORTRAIT_ROOM) > 100)
				return false;

			return ObjectManager.Me.Position.DistanceZ(Coords.DALARAN_BROKENISLES_PORTRAIT_ROOM) < 30;
		}
	}
	public static void ToDalaranPortraitRoom()
	{
		if (InDalaraPortraitRoom)
			return;

		if (!InBrokenIslesDalaran)
		{
			ToBrokenIslesDalaran();
			return;
		}
		if (GoToTask.ToPosition(Coords.DALARAN_BROKENISLES_TO_PORTRAIT_ROOM))
		{
			Questing.ClickMove(Coords.DALARAN_BROKENISLES_TO_PORTRAIT_ROOM, 1);
			Thread.Sleep(Others.Random(2000, 3000));
		}
	}
	public static void FromDalaranPortraitRoom()
	{
		if (!InDalaraPortraitRoom)
			return;

		if (GoToTask.ToPosition(Coords.DALARAN_BROKENISLES_FROM_PORTRAIT_ROOM))
		{
			Questing.ClickMove(Coords.DALARAN_BROKENISLES_FROM_PORTRAIT_ROOM, 1);
			Thread.Sleep(Others.Random(2000, 3000));
		}
	}
	#endregion LEGION

	public static class Portals
	{
		//neutral
		public static PortalInfo NORTHREND_CAVERNSOFTIME = new PortalInfo(Coords.PANDARIA_PORTALS_HORDE, 193604);
		public static PortalInfo SHATTRATH_QUELDANAS = new PortalInfo(Coords.SHATTRATH, 187056);
		public static PortalInfo BROKENISLES_CAVERNOFTIME = new PortalInfo(Coords.DALARAN_BROKENISLES_PORTRAIT_ROOM_PORTALS, 246005);
		public static PortalInfo BROKENISLES_SHATTRATH = new PortalInfo(Coords.DALARAN_BROKENISLES_PORTRAIT_ROOM_PORTALS, 246011);
		public static PortalInfo BROKENISLES_WYRMRESTTEMPLE = new PortalInfo(Coords.DALARAN_BROKENISLES_PORTRAIT_ROOM_PORTALS, 246013);
		public static PortalInfo BROKENISLES_DALARANCRATER = new PortalInfo(Coords.DALARAN_BROKENISLES_PORTRAIT_ROOM_PORTALS, 246008);
		public static PortalInfo BROKENISLES_KARAZHAN = new PortalInfo(Coords.DALARAN_BROKENISLES_PORTRAIT_ROOM_PORTALS, 246009);

		//horde
		public static PortalInfo ORGRIMMAR_OUTLANDS = new PortalInfo(new Vector3(1795.887, -4282.674, 7.395189, "None"), 195142);
		public static PortalInfo ORGRIMMAR_BLASTEDLANDS = new PortalInfo(new Vector3(1777.066, -4328.517, -7.984149, "None"), 235877);
		public static PortalInfo ORGRIMMAR_HYJAL = new PortalInfo(Coords.CATACLYSM_PORTALS_HORDE, 207688);
		public static PortalInfo ORGRIMMAR_VASHJIR = new PortalInfo(Coords.CATACLYSM_PORTALS_HORDE, 207690);
		public static PortalInfo ORGRIMMAR_DEEPHOLM = new PortalInfo(Coords.CATACLYSM_PORTALS_HORDE, 207689);
		public static PortalInfo ORGRIMMAR_ULDUM = new PortalInfo(Coords.CATACLYSM_PORTALS_HORDE, 207687);
		public static PortalInfo ORGRIMMAR_TWILIGHTHIGHLANDS = new PortalInfo(Coords.CATACLYSM_PORTALS_HORDE, 207686);
		public static PortalInfo ORGRIMMAR_TOLBARAD = new PortalInfo(Coords.CATACLYSM_PORTALS_HORDE, 206595);
		public static PortalInfo ORGRIMMAR_ASHRAN = new PortalInfo(Coords.ORGRIMMAR_ASHRAN, 237738);

		public static PortalInfo PANDARIA_ORGRIMMAR = new PortalInfo(Coords.PANDARIA_PORTALS_HORDE, 215127);
		public static PortalInfo PANDARIA_UNDERCITY = new PortalInfo(Coords.PANDARIA_PORTALS_HORDE, 215124);
		public static PortalInfo PANDARIA_THUNDERBLUFF = new PortalInfo(Coords.PANDARIA_PORTALS_HORDE, 215125);
		public static PortalInfo PANDARIA_SILVERMOON = new PortalInfo(Coords.PANDARIA_PORTALS_HORDE, 215126);
		public static PortalInfo PANDARIA_SHATTRATH_HORDE = new PortalInfo(Coords.PANDARIA_PORTALS_HORDE, 215113);
		public static PortalInfo PANDARIA_DALARAN_NORTHREND_HORDE = new PortalInfo(Coords.PANDARIA_PORTALS_HORDE, 215112);

		public static PortalInfo BROKENISLES_ORGRIMMAR = new PortalInfo(Coords.BROKENISLES_PORTALS_HORDE, 246001);
		public static PortalInfo BROKENISLES_UNDERCITY = new PortalInfo(Coords.BROKENISLES_PORTALS_HORDE, 246000);
		public static PortalInfo BROKENISLES_THUNDERBLUFF = new PortalInfo(Coords.BROKENISLES_PORTALS_HORDE, 245999);
		public static PortalInfo BROKENISLES_SILVERMOON = new PortalInfo(Coords.BROKENISLES_PORTALS_HORDE, 246004);
		public static PortalInfo BROKENISLES_PANDARIA_HORDE = new PortalInfo(Coords.BROKENISLES_PORTALS_HORDE, 246007);

		//alliance
		public static PortalInfo STORMWIND_OUTLANDS = new PortalInfo(new Vector3(-8996.93, 860.6223, 29.6206, "None"), 195141);
		public static PortalInfo STORMWIND_BLASTEDLANDS = new PortalInfo(new Vector3(-9002.964, 868.8864, 129.6928, "None"), 235882);
		public static PortalInfo STORMWIND_HYJAL = new PortalInfo(Coords.CATACLYSM_PORTALS_ALLIANCE, 207692);
		public static PortalInfo STORMWIND_VASHJIR = new PortalInfo(Coords.CATACLYSM_PORTALS_ALLIANCE, 207691);
		public static PortalInfo STORMWIND_DEEPHOLM = new PortalInfo(Coords.CATACLYSM_PORTALS_ALLIANCE, 207693);
		public static PortalInfo STORMWIND_ULDUM = new PortalInfo(Coords.CATACLYSM_PORTALS_ALLIANCE, 207695);
		public static PortalInfo STORMWIND_TWILIGHTHIGHLANDS = new PortalInfo(Coords.CATACLYSM_PORTALS_ALLIANCE, 207694);
		public static PortalInfo STORMWIND_TOLBARAD = new PortalInfo(Coords.CATACLYSM_PORTALS_ALLIANCE, 206594);
		public static PortalInfo STORMWIND_ASHRAN = new PortalInfo(Coords.STORMWIND_ASHRAN, 237733);

		public static PortalInfo PANDARIA_STORMWIND = new PortalInfo(Coords.PANDARIA_PORTALS_ALLIANCE, 215119);
		public static PortalInfo PANDARIA_IRONFORGE = new PortalInfo(Coords.PANDARIA_PORTALS_ALLIANCE, 215118);
		public static PortalInfo PANDARIA_DARNASSUS = new PortalInfo(Coords.PANDARIA_PORTALS_ALLIANCE, 215116);
		public static PortalInfo PANDARIA_EXODAR = new PortalInfo(Coords.PANDARIA_PORTALS_ALLIANCE, 215117);
		public static PortalInfo PANDARIA_SHATTRATH_ALLIANCE = new PortalInfo(Coords.PANDARIA_PORTALS_ALLIANCE, 215120);
		public static PortalInfo PANDARIA_DALARAN_NORTHREND_ALLIANCE = new PortalInfo(Coords.PANDARIA_PORTALS_ALLIANCE, 215121);

		public static PortalInfo BROKENISLES_STORMWIND = new PortalInfo(Coords.BROKENISLES_PORTALS_ALLIANCE, 246002);
		public static PortalInfo BROKENISLES_IRONFORGE = new PortalInfo(Coords.BROKENISLES_PORTALS_ALLIANCE, 245998);
		public static PortalInfo BROKENISLES_DARNASSUS = new PortalInfo(Coords.BROKENISLES_PORTALS_ALLIANCE, 245997);
		public static PortalInfo BROKENISLES_EXODAR = new PortalInfo(Coords.BROKENISLES_PORTALS_ALLIANCE, 246003);
		public static PortalInfo BROKENISLES_PANDARIA_ALLAINCE = new PortalInfo(Coords.BROKENISLES_PORTALS_ALLIANCE, 246006);
	}

	public static class Coords
	{
		public static Vector3 SHATTRATH = new Vector3(-1863.39, 5430.373, -5.049042, "Flying");

		public static Vector3 CATACLYSM_PORTALS_HORDE = new Vector3(2048.193, -4377.466, 102.8623, "Flying");
		public static Vector3 CATACLYSM_PORTALS_ALLIANCE = new Vector3(-8206.709, 427.2209, 125.7481, "Flying");

		public static Vector3 ORGRIMMAR_ASHRAN = new Vector3(1649.625, -4336.812, 27.2183, "None");
		public static Vector3 STORMWIND_ASHRAN = new Vector3(-8404.327, 202.7488, 155.3472, "None");

		public static Vector3 PANDARIA_PORTALS_HORDE = new Vector3(1729.32, 888.9265, 487.119, "None");
		public static Vector3 PANDARIA_PORTALS_ALLIANCE = new Vector3(815.691, 182.3423, 519.6915, "None");

		public static Vector3 BROKENISLES_PORTALS_HORDE = new Vector3(-714.7886, 4406.515, 727.0811, "None");
		public static Vector3 BROKENISLES_PORTALS_ALLIANCE = new Vector3(-927.9012, 4565.281, 729.2716, "None");

		public static Vector3 STORMWIND_WIZARDS_SANCTUM = new Vector3(-9002.968, 869.1647, 29.6207, "None");
		public static Vector3 STORMWIND_WIZARDS_SANCTUM_MOVEOUT = new Vector3(-9026.567, 897.5941, 29.62126, "None");

		public static Vector3 ORGRIMMAR_CLEFT_OF_SHADOW = new Vector3(1803.223, -4338.04, 60, "Flying");

		public static Vector3 DALARAN_BROKENISLES_TO_PORTRAIT_ROOM = new Vector3(-844.5972, 4467.76, 736.0415);
		public static Vector3 DALARAN_BROKENISLES_FROM_PORTRAIT_ROOM = new Vector3(-779.9896, 4415.237, 602.6288);
		public static Vector3 DALARAN_BROKENISLES_PORTRAIT_ROOM_PORTALS = new Vector3(-882.6309, 4498.373, 580.3107, "None");
		public static Vector3 DALARAN_BROKENISLES_PORTRAIT_ROOM = new Vector3(-843.7814, 4467.366, 588.849, "None");

		// instances
		public static Vector3 BLACKTEMPLE_NEAR = new Vector3(-3632.671, 317.9774, 45.54625, "Flying");
		public static Vector3 BLACKTEMPLE_MOVEIN = new Vector3(-3665.674, 319.7783, 34.85582, "None");

		public static Vector3 FIRELANDS_NEAR = new Vector3(4009.115, -2989.421, 1054.373, "Flying");
		public static Vector3 FIRELANDS_MOVEIN = new Vector3(3977.269, -2917.31, 1002.547, "None");
		public static Vector3 FIRELANDS_MOVEOUT = new Vector3(-574.0074, 333.0068, 115.484, "None");
		public static Vector3 FIRELANDS_NEAR_EXIT = new Vector3(-551.6964, 321.0503, 115.4774, "None");

		public static Vector3 SUNWELLPLATEAU_NEAR = new Vector3(12561.39, -6774.708, 15.09085, "None");
		public static Vector3 SUNWELLPLATEAU_MOVEIN = new Vector3(12542.34, -6777.375, 14.99986, "None");

		public static Vector3 MOLTENCORE_NEAR = new Vector3(-7508.632, -1039.495, 180.9118, "None");
		public static Vector3 MOLTENCORE_MOVEOUT = new Vector3(1106.135, -464.0515, -105.0753, "None");

		public static Vector3 BLACKROCKDEPTHS_MOVEIN = new Vector3(-7178.013, -931.2283, 166.1644, "None");
		public static Vector3 BLACKROCKDEPTHS_MOVEOUT = new Vector3(456.1039, 44.58232, -70.41068, "None");

		public static Vector3 HEARTHOFFEAR_NEAR = new Vector3(171.5684, 4033.487, 252.5316, "None");
		public static Vector3 HEARTHOFFEAR_MOVEIN = new Vector3(165.117, 4069.244, 255.8762, "None");
		public static Vector3 HEARTHOFFEAR_EXIT = new Vector3(-2382.683, 459.3292, 422.341, "None");
		public static Vector3 HEARTHOFFEAR_MOVEOUT = new Vector3(-2395.222, 459.3198, 422.3409, "None");

		public static Vector3 MOGUSHANVAULTS_EXIT = new Vector3(3858.586, 1045.029, 490.0744, "None");
		public static Vector3 MOGUSHANVAULTS_MOVEOUT = new Vector3(3849.095, 1044.883, 490.0574, "None");
		public static Vector3 MOGUSHANVAULTS_NEAR = new Vector3(4001.232, 1089.177, 497.1545, "None");
		public static Vector3 MOGUSHANVAULTS_MOVEIN = new Vector3(3973.886, 1121.351, 493.2914, "None");
	}

	public static class Instances
	{
		#region VANILLA
		public static bool InBlackRockDepths
		{
			get
			{
				return Usefuls.ContinentId == (int)ContinentId.BlackrockDepths;
			}
		}
		public static bool NearMoltenCore
		{
			get
			{
				if (!InEasternKingdoms)
					return false;

				return ObjectManager.Me.Position.DistanceTo2D(Coords.MOLTENCORE_NEAR) < 50;
			}
		}
		public static bool InMoltenCore
		{
			get
			{
				return Usefuls.ContinentId == (int)ContinentId.MoltenCore;
			}
		}
		public static void ToMoltenCoreNear()
		{
			if (NearMoltenCore)
			{
				Log("Near Molten Core");
			}
			else if (InMoltenCore)
			{
				Log("In Molten Core. Go outside");
				GoToTask.ToPosition(Coords.MOLTENCORE_MOVEOUT);
			}
			else if (InEasternKingdoms)
			{
				Log("In Eastern Kingdoms, fly to Molten Core");
				Goto(Coords.MOLTENCORE_NEAR, 10, 600, 15);
			}
			else if (InBlackRockDepths)
			{
				Log("In Blackrock Depths. Go outside");
				GoToTask.ToPosition(Coords.BLACKROCKDEPTHS_MOVEOUT);
			}
			else if (ItemsManager.HasItemById(37863)) //Direbrew's Remote
			{
				Log("Use [Direbrew's Remote] to get in Blackrock Depths");
			}
			else
			{
				ToEasternKingdoms();
			}
		}
		public static void ToMoltenCore()
		{
			if (InMoltenCore)
			{
				Log("In Molten Core");
			}
			else if (NearMoltenCore)
			{
				Log("Near Molten Core. Go inside");
				GoToTask.ToPositionAndIntecractWithNpc(Coords.MOLTENCORE_NEAR, 14387, 1);
				Thread.Sleep(5 * 1000);
			}
			else
			{
				ToMoltenCoreNear();
			}
		}
		#endregion VANILLA

		#region BURNING CRUSADE
		public static bool InBlackTemple
		{
			get
			{
				return Usefuls.ContinentId == (int)ContinentId.BlackTemple;
			}
		}

		public static void ToBlackTemple()
		{
			if (InBlackTemple)
			{
				Log("Im in Black Temple");
			}
			else if (NearBlackTemple)
			{
				Log("Im near Black Temple, going inside");
				GoToTask.ToPosition(Coords.BLACKTEMPLE_MOVEIN);
			}
			else
			{
				ToBlackTempleNear();
			}
		}

		public static bool NearBlackTemple
		{
			get
			{
				if (InBlackTemple)
					return true;
				if (!InOutlands)
					return false;

				return ObjectManager.Me.Position.DistanceTo2D(Coords.BLACKTEMPLE_NEAR) < 500;
			}
		}
		const string BLACKTEMPLE_VAR = "Camelot10_Traveler_BlackTemple_Neck";
		public static void ToBlackTempleNear()
		{
			uint teleportMedalion = 32757;  //http://www.wowhead.com/item=32757/blessed-medallion-of-karabor
			if (InBlackTemple || NearBlackTemple)
			{
				if (Var.Exist(BLACKTEMPLE_VAR) && Var.IsType(BLACKTEMPLE_VAR, typeof(uint)))
				{
					var neckID = Var.GetVar<uint>(BLACKTEMPLE_VAR);
					var neckName = ItemsManager.GetNameById(neckID);
					ItemsManager.EquipItemByName(neckName);
					Var.VarDictionary.Remove(BLACKTEMPLE_VAR);
					Log("Neck equip back: " + neckName);
				}
				Log("Im in Black Temple or near");
			}
			else if (ItemsManager.HasItemById(teleportMedalion))
			{
				Log("Equipping teleport item");
				var neckID = ObjectManager.Me.GetEquipedItemBySlot(InventorySlot.INVSLOT_MAINHAND);
				if (neckID != 0)
				{
					Var.SetVar(BLACKTEMPLE_VAR, neckID);
					var neckName = ItemsManager.GetNameById(neckID);
					Log("Saved neck for re-equip: " + neckName);
				}
				ItemsManager.EquipItemByName(ItemsManager.GetNameById(teleportMedalion));
				Thread.Sleep(35 * 1000);
				Log("Teleporting to Black Temple");
				ItemsManager.UseItem(teleportMedalion);
				Usefuls.WaitIsCasting();
			}
			else if (InOutlands)
			{
				Log("Im in Outlands. Moving near Black Temple");
				GoToTask.ToPosition(Coords.BLACKTEMPLE_NEAR);
			}
			else
			{
				Log("Im in somewhere else. Go to Shattrath");
				ToShattrath();
				Thread.Sleep(10 * 1000);
				ToBlackTempleNear();
			}
		}

		// sunwell
		public static bool InSunwell
		{
			get
			{
				return Usefuls.ContinentId == (int)ContinentId.SunwellPlateau;
			}
		}

		public static void ToSunwell()
		{
			if (InSunwell)
			{
				Log("Im in Sunwell");
			}
			else if (InQuelDanas)
			{
				Log("Im in Quel Danas, goin to sunwell");
				GoToTask.ToPosition(Coords.SUNWELLPLATEAU_MOVEIN);
			}
			else
			{
				ToQuelDanas();
			}
		}
		#endregion BURNING CRUSADE

		#region CATACLYSM
		public static bool InFirelands
		{
			get
			{
				return Usefuls.ContinentId == (int)ContinentId.Firelands1;
			}
		}
		public static bool InFirelandsNear
		{
			get
			{
				if (!InKalimdor)
					return false;

				return ObjectManager.Me.Position.DistanceTo2D(Coords.FIRELANDS_NEAR) < 100;
			}
		}

		public static bool InFirelandsFlyingDistance
		{
			get
			{
				if (!InKalimdor)
					return false;

				return ObjectManager.Me.Position.DistanceTo2D(Coords.FIRELANDS_NEAR) < 1800;
			}
		}

		public static void ToFirelandsNear()
		{
			if (InFirelandsNear)
			{
				Log("im near firelands");
			}
			else if (InFirelands)
			{
				Log("im in firelands. move out");
				if (GoToTask.ToPosition(Coords.FIRELANDS_NEAR_EXIT))
				{
					MoveClick(Coords.FIRELANDS_MOVEOUT, () => InFirelands);
				}
			}
			else if (InFirelandsFlyingDistance)
			{
				Log("im in flying distance to firelands. fly");
				MountTask.Mount(true, MountTask.MountCapacity.Fly);
				Usefuls.WaitIsCasting();
				LongMove.LongMoveGo(Coords.FIRELANDS_NEAR);
			}
			else if (InStormwindWizardsSanctum)
			{
				Log("in stormwind wizards sanctum. moveout");
				FromStormwindWizardsSanctum();
			}
			else if (InOrgrimmarCleftOfShadow)
			{
				Log("in orgrimmar cleft of shadows. moveout");
				FromOrgrimmarCleftOfShadow();
			}
			else if (InCapital)
			{
				Log("im in capital. use port to hyjal");
				if (ObjectManager.Me.IsHorde)
				{
					Portals.ORGRIMMAR_HYJAL.Use();
				}
				else if (ObjectManager.Me.IsAlliance)
				{
					Portals.STORMWIND_HYJAL.Use();
				}
			}

			else
			{
				Log("im somewhere esle. travel to capital");
				ToCapital();
			}
		}
		public static void ToFirelands()
		{
			if (InFirelands)
			{
				Log("im in firelands");
			}
			else if (InFirelandsNear)
			{
				Log("in near firelands. move in");
				if (GoToTask.ToPosition(Coords.FIRELANDS_NEAR))
				{
					MoveClick(Coords.FIRELANDS_MOVEIN, () => !InFirelands);
				}
			}
			else
			{
				Log("im too far from firelands entrace. start travel");
				ToFirelandsNear();
			}
		}
		#endregion CATACLYSM

		#region PANDARIA
		public static bool InHearthOfFear
		{
			get
			{
				return Usefuls.ContinentId == (int)ContinentId.MantidRaid;
			}
		}
		public static bool InHearthOfFearNear
		{
			get
			{
				if (!InPandaria)
					return false;

				return ObjectManager.Me.Position.DistanceTo2D(Coords.HEARTHOFFEAR_NEAR) < 100;
			}
		}
		public static void ToHearthOfFearNear()
		{
			if (InHearthOfFearNear)
			{
				Log("im near Hearth of Fear");
			}
			else if (InHearthOfFear)
			{
				Log("im in Hearth of Fear. move out");
				if (GoToTask.ToPosition(Coords.HEARTHOFFEAR_EXIT))
				{
					MoveClick(Coords.HEARTHOFFEAR_MOVEOUT, () => InHearthOfFear);
				}
			}
			else if (InPandaria)
			{
				Log("im in Pandaria. fly to Hearth of Fear");
				MountTask.Mount(true, MountTask.MountCapacity.Fly);
				Usefuls.WaitIsCasting();
				LongMove.LongMoveGo(Coords.HEARTHOFFEAR_NEAR);
			}
			else
			{
				Log("im somewhere esle. travel to Pandaria");
				ToPandaria();
			}
		}
		public static void ToHearthOfFear()
		{
			if (InHearthOfFear)
			{
				Log("im in Hearth of Fear");
			}
			else if (InHearthOfFearNear)
			{
				Log("in near Hearth of Fear. move in");
				if (GoToTask.ToPosition(Coords.HEARTHOFFEAR_NEAR))
				{
					MoveClick(Coords.HEARTHOFFEAR_MOVEIN, () => !InHearthOfFear);
				}
			}
			else
			{
				Log("im too far from Hearth of Fear entrace. start travel");
				ToHearthOfFearNear();
			}
		}
		public static bool InMogushanVaults
		{
			get
			{
				return Usefuls.ContinentId == (int)ContinentId.MogushanPalace;
			}
		}
		#endregion PANDARIA
	}

	public enum AreaId
	{
		//alliance
		Stormwind = 1519,
		Ironforge = 1537,
		//horde
		Orgrimmar = 1637,
		QuelDanas = 4080,
		Shattrath = 3703,
		Silvermoon = 3487,
		EversongWoods = 3430,
		Ghostlands = 3433,
		//
		DalaranNorthrend = 4395,
		DalaranBrokenIsles = 7502,
		PandariaShrineAlliance = 6553, //alliance PandariaShrineOfSevenStars
		PandariaShrineAllianceNear = 6142,
		PandariaShrineHorde = 6141, //horde PandariaShrineOfTwoMoons
		PandariaShrineHordeNear = 6554,
	}

	public class PortalInfo
	{
		public Vector3 position;
		public int objectId;
		public PortalInfo(Vector3 pos, int id)
		{
			position = pos;
			objectId = id;
		}

		public void Use()
		{
			GoToTask.ToPosition(position);
			MountTask.DismountMount();
			GoToTask.ToPositionAndIntecractWithGameObject(position, objectId);
			Questing.WaitCurrentAreaIDChange();
		}
	}

	public static void FindEntrancePoint(Vector3 p1, Vector3 p2, float distance = 10f)
	{
		var delta = new Vector3(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
		delta *= distance;
		var p3 = p2 + delta;
		Logging.Write("ENTRANCE POINT CALCULATED: " + p3.ToStringNewVector());
	}

	public static void Move(List<Vector3> path, bool ignoreFight = false)
	{
		var oldIgnore = Conditions.ForceIgnoreIsAttacked;
		if (ignoreFight)
			Conditions.ForceIgnoreIsAttacked = true;

		MovementManager.Go(path);
		do
		{
			Thread.Sleep(1000);
		}
		while (MovementManager.InMovement && Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause && !Conditions.IsAttackedAndCannotIgnore);
		MovementManager.StopMove();
		if (ignoreFight)
			Conditions.ForceIgnoreIsAttacked = oldIgnore;
	}

	public static void MoveClick(Vector3 p, BooleanDelegate condition)
	{
		while (condition() && Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause && !Conditions.IsAttackedAndCannotIgnore)
		{
			ClickToMove.CGPlayer_C__ClickToMove(p.X, p.Y, p.Z, MemoryRobot.Int128.Zero(), (int)ClickToMoveType.Move, 0.5f);
			Thread.Sleep(Others.Random(500, 1000));
		}
	}

	public static class Paths
	{
		public static List<Vector3> FROM_STORMWIND_WIZARDS_SANCTUM = new List<Vector3>()
		{
			/*
			new Vector3(-9001.922f, 865.5825f, 29.6207f, "None"),
			new Vector3(-9005.982f, 871.1793f, 29.6207f, "None"),
			new Vector3(-9008.735f, 874.5976f, 29.6207f, "None"),
			new Vector3(-9011.387f, 877.7703f, 29.6207f, "None"),
			new Vector3(-9014.041f, 881.1155f, 29.6207f, "None"),
			new Vector3(-9016.631f, 884.5637f, 29.6207f, "None"),
			new Vector3(-9019.104f, 887.9193f, 29.6207f, "None"),
			new Vector3(-9026.567, 897.5941, 29.62126, "None"),
			//*/
			new Vector3(-9014.817f, 873.1132f, 148.616f, "None"),
			new Vector3(-9010.438f, 867.6962f, 146.6526f, "None"),
			new Vector3(-9006.62f, 866.3571f, 144.4889f, "None"),
			new Vector3(-9002.92f, 868.4304f, 142.4687f, "None"),
			new Vector3(-9001.925f, 872.3652f, 140.3814f, "None"),
			new Vector3(-9003.929f, 876.0081f, 138.1437f, "None"),
			new Vector3(-9007.757f, 877.5584f, 135.8545f, "None"),
			new Vector3(-9011.72f, 876.3581f, 133.723f, "None"),
			new Vector3(-9013.243f, 872.8591f, 131.9229f, "None"),
			new Vector3(-9011.952f, 868.4532f, 129.693f, "None"),
			new Vector3(-9008.784f, 866.3384f, 129.693f, "None"),
			new Vector3(-9004.449f, 866.1371f, 129.693f, "None"),
			new Vector3(-9000.804f, 863.9247f, 129.7348f, "None"),
			new Vector3(-8997.371f, 861.7692f, 128.6832f, "None"),
			new Vector3(-8993.456f, 859.9705f, 127.2438f, "None"),
		};

		public static List<Vector3> FROM_ORGRIMMAR_CLEFT_OF_SHADOW = new List<Vector3>() {
			new Vector3(1798.946f, -4328.533f, 6.080236f, "Flying"),
			new Vector3(1795.875f, -4326.814f, 4.42988f, "Flying"),
			new Vector3(1788.084f, -4323.274f, 4.315794f, "Flying"),
			new Vector3(1780.843f, -4319.252f, 7.231764f, "Flying"),
			new Vector3(1773.653f, -4314.912f, 10.35413f, "Flying"),
			new Vector3(1766.67f, -4310.563f, 13.17553f, "Flying"),
			new Vector3(1759.578f, -4306.144f, 15.76818f, "Flying"),
			new Vector3(1752.618f, -4301.647f, 18.10365f, "Flying"),
			new Vector3(1745.847f, -4296.484f, 20.44409f, "Flying"),
			new Vector3(1740.664f, -4290.052f, 22.76319f, "Flying"),
			new Vector3(1737.333f, -4282.413f, 25.06974f, "Flying"),
			new Vector3(1737.57f, -4273.992f, 27.55037f, "Flying"),
			new Vector3(1741.313f, -4266.48f, 30.38848f, "Flying"),
			new Vector3(1746.693f, -4260.58f, 33.45964f, "Flying"),
			new Vector3(1752.511f, -4254.991f, 36.44896f, "Flying"),
			new Vector3(1759.369f, -4249.647f, 38.80262f, "Flying"),
			new Vector3(1766.323f, -4245.21f, 41.20506f, "Flying"),
			new Vector3(1773.845f, -4241.246f, 43.55302f, "Flying"),
			new Vector3(1781.69f, -4238.302f, 45.98756f, "Flying"),
			new Vector3(1789.658f, -4236.031f, 48.74838f, "Flying"),
			new Vector3(1797.713f, -4234.384f, 51.27284f, "Flying"),
			new Vector3(1806.21f, -4234.676f, 53.8224f, "Flying"),
			new Vector3(1814.394f, -4236.033f, 57.01455f, "Flying"),
			new Vector3(1822.626f, -4237.836f, 59.20106f, "Flying"),
			new Vector3(1830.885f, -4239.79f, 61.1441f, "Flying"),
			new Vector3(1838.647f, -4242.108f, 64.10677f, "Flying"),
			new Vector3(1846.689f, -4244.944f, 66.61571f, "Flying"),
			new Vector3(1854.57f, -4247.742f, 69.00578f, "Flying"),
			new Vector3(1860.569f, -4249.872f, 74.85528f, "Flying"),
			new Vector3(1865.012f, -4251.451f, 81.6699f, "Flying"),
		};

	}

	#region TRANSPORT
	public class Transport
	{

		public int ID;
		public Vector3 fromStop;
		public Vector3 fromWait;
		public Vector3 fromIn;
		public ContinentId from;
		public Vector3 toStop;
		public Vector3 toEnd;
		public ContinentId to;
		public float minDist = 3f;

		//------------------------------------------------------------
		public static int ORGRIMMAR_TIRISFALGLADES_ID = 164871;
		public static Vector3 ORGRIMMAR_TIRISFALGLADES_STOP = new Vector3(1833.509, -4391.543, 152.7679, "None");
		public static Vector3 ORGRIMMAR_TIRISFALGLADES_WAIT = new Vector3(1843.296, -4394.557, 135.233, "None");
		public static Vector3 ORGRIMMAR_TIRISFALGLADES_IN = new Vector3(1833.743, -4385.087, 135.0355, "None");
		public static Vector3 TIRISFALGLADES_ORGRIMMAR_STOP = new Vector3(2062.376, 292.998, 114.973, "None");
		public static Vector3 TIRISFALGLADES_ORGRIMMAR_WAIT = new Vector3(2066.221, 285.8821, 97.03187, "None");
		public static Vector3 TIRISFALGLADES_ORGRIMMAR_IN = new Vector3(2068.304, 296.2917, 97.26978, "None");
		public static Transport FromTirisfalGladesToOrgrimmar = new Transport()
		{
			ID			= ORGRIMMAR_TIRISFALGLADES_ID,
			fromStop	= TIRISFALGLADES_ORGRIMMAR_STOP,
			fromWait	= TIRISFALGLADES_ORGRIMMAR_WAIT,
			fromIn		= TIRISFALGLADES_ORGRIMMAR_IN,
			from		= ContinentId.Azeroth,
			toStop		= ORGRIMMAR_TIRISFALGLADES_STOP,
			toEnd		= ORGRIMMAR_TIRISFALGLADES_WAIT,
			to			= ContinentId.Kalimdor,
		};

		public static Transport FromOrgrimmarToTirisfalGlades = new Transport()
		{
			ID			= ORGRIMMAR_TIRISFALGLADES_ID,
			fromStop	= ORGRIMMAR_TIRISFALGLADES_STOP,
			fromWait	= ORGRIMMAR_TIRISFALGLADES_WAIT,
			fromIn		= ORGRIMMAR_TIRISFALGLADES_IN,
			from		= ContinentId.Kalimdor,
			toStop		= TIRISFALGLADES_ORGRIMMAR_STOP,
			toEnd		= TIRISFALGLADES_ORGRIMMAR_WAIT,
			to			= ContinentId.Azeroth,
		};
		
		//------------------------------------------------------------
		public static Transport FromOrgrimmarToStranglethornVale = new Transport()
		{
			ID = 175080,
			fromStop = new Vector3(1880.818, -4435.269, 152.8608, "None"),
			fromWait = new Vector3(1869.101, -4418.77, 135.2329, "None"),
			fromIn = new Vector3(1874.014, -4423.041, 135.0976, "None"),
			from = ContinentId.Kalimdor,
			toStop = new Vector3(),
			toEnd = new Vector3(),
			to = ContinentId.Azeroth,
		};
		public static Transport FromOrgrimmarToBoreanTundra = new Transport()
		{
			ID = 186238,
			fromStop = new Vector3(1775.066, -4299.745, 151.0326, "None"),
			fromWait = new Vector3(1762.784, -4284.637, 133.1065, "None"),
			fromIn = new Vector3(1767.474, -4289.588, 133.1746, "None"),
			from = ContinentId.Kalimdor,
			toStop = new Vector3(),
			toEnd = new Vector3(),
			to = ContinentId.Northrend,
		};
		public static Transport FromBoreanTundraToOrgrimmar = new Transport()
		{
			ID = 186238,
			fromStop = new Vector3(2837.908, 6187.443, 140.1648, "None"),
			fromWait = new Vector3(2836.438, 6184.474, 121.9354, "None"),
			fromIn = new Vector3(2840.492, 6188.314, 122.2986, "None"),
			from = ContinentId.Northrend,
			toStop = new Vector3(),
			toEnd = new Vector3(),
			to = ContinentId.Kalimdor,
		};
		public static Transport FromOrgrimmarToThunderBluff = new Transport()
		{
			ID = 186238,
			fromStop = new Vector3(1663.308, -4392.533, 151.9516, "None"),
			fromWait = new Vector3(1734.316, -4256.077, 133.1068, "None"),
			fromIn = new Vector3(1729.55, -4251.263, 133.5126, "None"),
			from = ContinentId.Kalimdor,
			toStop = new Vector3(),
			toEnd = new Vector3(),
			to = ContinentId.Kalimdor,
		};
		public bool Pulse()
		{
			if (!Can)
				return false;

			Questing.UseTransport(ID, fromStop, fromWait, fromIn, from, toStop, toEnd, to, minDist);
			return true;
		}
		public bool Complete
		{
			get
			{
				return Usefuls.ContinentId == (int)to && !ObjectManager.Me.InTransport;
			}
		}
		public bool Can
		{
			get
			{
				return Usefuls.ContinentId == (int)from || Usefuls.ContinentId == (int) to;
			}
		}
		public override string ToString()
		{
			return "[Transport from=" + ( (ContinentId)from ) +" to=" +((ContinentId)to) +"]";
		}
	}
	#endregion TRANSPORT
}
