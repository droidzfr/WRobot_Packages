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
/// <summary>
/// Required Questing.cs
/// </summary>
public class LegionQuests : QuestClass
{
	#region ZONES
	public const int ZONE_AZSUNA = 1015;
	public const int ZONE_VALSHARAH = 1018;
	public const int ZONE_HIGHTMOUNTAIN = 1024;
	public const int ZONE_STORMHEIM = 1017;
	public const int ZONE_SURAMAR = 1033;
	public const int ZONE_DALARAN = 1014;
	public const int ZONE_BROKEN_SHORE = 1021;
	public const int ZONE_EYE_OF_AZSHARA = 1096;
	#endregion ZONES

	#region AREAS
	public const int AREA_DALARAN = 7502;
	const int AREA_DALARAN2 = 8392;
	#endregion AREAS

	#region QUEST_IDS
	public const int KAYN = 40374;
	public const int ALTRIUS = 40375;
	//
	public const int INTRO = 44663;
	public const int INTRO2 = 44184;
	public const int DEMONHUNTER_INTRO_ALLIANCE = 44473;
	public const int DEMONHUNTER_INTRO_HORDE = 41002;
	public const int AZSUNA_START = 39718;
	public const int AZSUNA_FINAL_ALLIANCE = 40794;
	public const int AZSUNA_FINAL_HORDE = 42244;
	public const int VALSHARAH_START = 39731;
	public const int VALSHARAH_FINAL = 40890;
	public const int HIGHMOUNTAIN_START = 39733;
	public const int HIGHMOUNTAIN_FINAL = 39656;
	public const int STORMHEIM_START_ALLIANCE = 39735;
	public const int STORMHEIM_START_HORDE = 39864;
	public const int STORMHEIM_FINAL_ALLIANCE = 39122; //unfinished
	public const int STORMHEIM_FINAL_HORDE = 38882; //unfinished
	public const int SURAMAR_START1 = 39985;
	public const int SURAMAR_START2 = 44555;
	public const int SURAMAR_RAID = 43362;
	public const int SURAMAR_FINAL = 43568;
	public const int BROKEN_SHORE_START = 46730;
	public const int BROKEN_SHORE_OPEN = 46734; //assault on broken shore
	public const int BROKEN_SHORE_FINAL = BROKEN_SHORE_OPEN;
	public const int ARGUS_START1 = 47835;
	public const int ARGUS_START2 = 48507;
	public const int ARGUS_START3 = 47221;
	public const int ARGUS_START4 = 48506;
	public const int ARGUS_DUNGEON = 47654;
	public const int ARGUS_FINAL = 48272; //not final
	#endregion QUEST_IDS

	public LegionQuests()
	{
		Log("started");
		FixLegionBlacklist();
	}
	/// <summary>
	/// LegionQuests copy
	/// </summary>
	void FixLegionBlacklist()
	{
		wManager.wManagerSetting.AddBlackListZone(new Vector3(-844.5972, 4467.76, 736.0415), 7, false); //dalaran center, teleportation room teleporter
		wManager.wManagerSetting.AddBlackListNpcEntry(93940, false); // Bradensbrook in Val'sharah
		wManager.wManagerSetting.AddBlackListNpcEntry(96565, false); // Obsidian Overlook in Highmountain
		var badNodesPositions = new List<Vector3>()
		{
			new Vector3(2816.719, 7278.946, 21.87156, "None"), // http://www.wowhead.com/npc=93940/douglas-carrington // Bradensbrook in Val'sharah
			new Vector3(2974.094, 4347.88, 652.882, "None"), // http://www.wowhead.com/npc=96565/chofa-nighthoof // Obsidian Overlook in Highmountain
		};
		foreach (var badNodeP in badNodesPositions)
		{
			wManager.wManagerSetting.AddBlackListZone(badNodeP, 15, false);
		}
		var taxiNodesCount = 0;
		lock (wManager.Wow.Helpers.Taxi.TaxiList.Locker)
		{
			//var taxiNodes = Taxi.GetTaxiNodes();
			var taxiNodes = Taxi.TaxiList.GetTaxiNodesOfCurrentPlayer(false);
			taxiNodesCount = taxiNodes.Count;
			foreach (var taxi in taxiNodes)
			{
				foreach (var badP in badNodesPositions)
				{
					if (badP.DistanceTo2D(taxi.Position) < 10)
					{
						taxi.Active = false;
						Taxi.TaxiList.Disable(taxi);
						Logging.WriteDebug("[Legion Blacklist] taxi deactivated=" + taxi.Name);
						break;
					}
					else
					{
						//taxi.Active = true;
						//Taxi.TaxiList.Active(taxi);
					}
				}
			}
			Taxi.TaxiList.Save();
		}
		Logging.WriteDebug("[Legion Blacklist] completed total taxi nodes=" + taxiNodesCount);
	}
	#region POSITIONS
	public static class Positions
	{
		public static Vector3 TELEPORT_TO_PORTRAIT_ROOM = new Vector3(-844.5972, 4467.76, 736.0415);
		public static Vector3 TELEPORT_FROM_PORTRAIT_ROOM = new Vector3(-779.9896, 4415.237, 602.6288);
		public static Vector3 PORTRAIT_ROOM_PORTALS = new Vector3(-882.6309, 4498.373, 580.3107, "None");
		public static Vector3 PORTRAIT_ROOM = new Vector3(-843.7814, 4467.366, 588.849, "None");
	}
	#endregion POSITIONS

	public static void Log(string text)
	{
		Logging.Write("[Legion Quests] " + text);
	}
	#region COPY FROM TRAVELER
	public static bool InBrokenIsles
	{
		get
		{
			return Usefuls.ContinentId == (int)ContinentId.Troll_Raid;
		}
	}
	public static void ToBrokenIsles()
	{
		if (!InBrokenIsles)
			UseDalaranHeathstone();
	}
	public static bool InDalaran
	{
		get
		{
			return Usefuls.AreaId == AREA_DALARAN || Usefuls.AreaId == AREA_DALARAN2;
		}
	}
	public static bool InDalaraPortraitRoom
	{
		get
		{
			if (!InDalaran)
				return false;

			if (ObjectManager.Me.Position.DistanceTo2D(Positions.PORTRAIT_ROOM) > 100)
				return false;

			return ObjectManager.Me.Position.DistanceZ(Positions.PORTRAIT_ROOM) < 20;
		}
	}
	public static void ToDalaranPortraitRoom()
	{
		if (InDalaraPortraitRoom)
			return;

		if (!InDalaran)
		{
			UseDalaranHeathstone();
			return;
		}
		if (GoToTask.ToPosition(Positions.TELEPORT_TO_PORTRAIT_ROOM))
		{
			Questing.ClickMove(Positions.TELEPORT_TO_PORTRAIT_ROOM, 1);
			Thread.Sleep(Others.Random(2000, 3000));
		}
	}
	public static void FromDalaranPortraitRoom()
	{
		if (!InDalaraPortraitRoom)
			return;

		if (GoToTask.ToPosition(Positions.TELEPORT_FROM_PORTRAIT_ROOM))
		{
			Questing.ClickMove(Positions.TELEPORT_FROM_PORTRAIT_ROOM, 1);
			Thread.Sleep(Others.Random(2000, 3000));
		}
	}
	#endregion COPY FROM TRAVELER
	public static bool CanFly
	{
		get
		{
			return ObjectManager.Me.HaveBuff(233368); // http://www.wowhead.com/spell=233368/broken-isles-pathfinder
		}
	}
	public static int OrderHallResources
	{
		get
		{
			return Questing.Currency(1220);
		}
	}
	public static int ArtifactKnowledge
	{
		get
		{
			return Lua.LuaDoString<int>("local _, level = GetCurrencyInfo('currency: 1171'); return level;");
		}
	}
	public static int ArtifactLevel
	{
		get
		{
			var lua = @"
local itemID, altItemID, name, icon, xp, pointsSpent, quality, artifactAppearanceID, appearanceModID, itemAppearanceID, altItemAppearanceID, altOnTop, artifactTier = C_ArtifactUI.GetEquippedArtifactInfo();
return pointsSpent;
";
			var pointsSpent = Lua.LuaDoString<int>(lua);
			return pointsSpent;
		}
	}
	public static bool UseDalaranHeathstone()
	{
		if (!ItemsManager.HasItemById(140192))
		{
			Logging.Write("WARNING! You need 'Dalaran heathstone'");
			Thread.Sleep(5000);
			return false;
		}
		if (Questing.ItemOnCooldown(140192))
		{
			//Logging.Write("ATTENTION! 'Dalaran heathstone' on cooldown. Need to wait");
			Thread.Sleep(5000);
			return false;
		}
		ItemsManager.UseItem(140192);
		Usefuls.WaitIsCasting();
		return true;
	}
	public static bool StartTableQuests(params int[] questIds)
	{
		if (Mission.IsVisible)
		{
			Thread.Sleep(Others.Random(1000, 2000));
			Mission.Map();
			foreach (var questId in questIds)
			{
				Lua.LuaDoString("C_AdventureMap.StartQuest(" + questId + ");");
				Thread.Sleep(Others.Random(1000, 2000));
			}
			return true;
		}
		return false;
	}
	public static bool StartWorkOrder()
	{
		if (Questing.IsVisible("GarrisonCapacitiveDisplayFrame"))
		{
			Lua.LuaDoString("GarrisonCapacitiveDisplayFrame.StartWorkOrderButton:Click();");
			Thread.Sleep(Others.Random(2000, 3000));
			Log("start work order");
			return true;
		}
		return false;
	}
	public static bool StartAllWorkOrder()
	{
		if (Questing.IsVisible("GarrisonCapacitiveDisplayFrame"))
		{
			Lua.LuaDoString("GarrisonCapacitiveDisplayFrame.CreateAllWorkOrdersButton:Click();");
			Thread.Sleep(Others.Random(2000, 3000));
			Log("start all work order");
			return true;
		}
		return false;
	}

	#region QUEST CLASSES
	public class StartZoneQuesting : QuestClass
	{
		public StartZoneQuesting()
		{
			Name = "Start Zone Questing";
			QuestId.Add(AZSUNA_START);
			QuestId.Add(VALSHARAH_START);
			QuestId.Add(HIGHMOUNTAIN_START);
			QuestId.Add(STORMHEIM_START_ALLIANCE);
			QuestId.Add(STORMHEIM_START_HORDE);
			Step.AddRange(new[] { 0, 0, 0, 0, 0 });
		}
		public override bool HasQuest()
		{
			return true;
		}
		//keep sealed
		public sealed override bool IsComplete()
		{
			return InDalaran;
		}
		public sealed override bool Pulse()
		{
			//already on quests
			if (DoAzsuna || DoValsharah || DoHighmountain || DoStormheim)
			{
				ToDalaran();
				return true;
			}
			UseTable();
			if (NeedAzsuna)
			{
				StartTableQuests(AZSUNA_START);
			}
			else if (NeedValsharah)
			{
				StartTableQuests(VALSHARAH_START);
			}
			else if (NeedHighmountain)
			{
				StartTableQuests(HIGHMOUNTAIN_START);
			}
			else if (NeedStormheim)
			{
				StartTableQuests(STORMHEIM_START_ALLIANCE, STORMHEIM_START_HORDE);
			}
			return true;
		}

		public virtual bool ToDalaran()
		{
			Log("WARNING! Inherited from LegionQuests.StartZoneQuesting, but not implemented ToDalaran()");
			return false;
		}

		public virtual bool UseTable()
		{
			Log("WARNING! Inherited from LegionQuests.StartZoneQuesting, but not implemented UseTable()");
			return false;
		}
	}
	public static bool ClearKaynIfAltriusChoosed()
	{
		if (ChoosedKayn)
		{

			return false;
		}
		var kayn = new List<int>()
		{
			//DH class hall
			89362,
			99247,
			95240,
			108572,
			94902,
			//DH vengeance artifact

			//DH havoc artifact

			//DH azsuna
		};
		foreach (var npc in Quest.QuesterCurrentContext.NPCList)
		{
			if (kayn.Contains(npc.Id))
			{
				Log(" (Hook) Clear NPC='" + npc.Name + "' PickUp and TurnIn quests, because player choosed Altrius.");
				npc.PickUpQuests.Clear();
				npc.TurnInQuests.Clear();
			}
		}
		return true;
	}
	#endregion QUEST CLASSES

	#region CONDITIONS
	public static bool NeedIntro
	{
		get
		{
			return Questing.NotComplete(INTRO, INTRO2);
		}
	}
	public static bool NeedDemonHunterIntro
	{
		get
		{
			if (ObjectManager.Me.WowClass != WoWClass.DemonHunter)
				return false;

			return Questing.NotComplete(DEMONHUNTER_INTRO_ALLIANCE, DEMONHUNTER_INTRO_HORDE);
		}
	}
	//for non Demon Hunter Kayn is default NPC. Only Demon Hunter who choose Altrius see him instead Kayn
	public static bool ChoosedKayn
	{
		get
		{
			return !ChoosedAltrius;
		}
	}
	public static bool ChoosedAltrius
	{
		get
		{
			return Quest.GetQuestCompleted(ALTRIUS);
		}
	}
	public static bool InClassHall
	{
		get
		{
			return ObjectManager.Me.HaveBuff(213170);
		}
	}
	public static bool DoAzsuna
	{
		get
		{
			return NeedAzsuna && Questing.Recieved(AZSUNA_START);
		}
	}
	public static bool DoValsharah
	{
		get
		{
			return NeedValsharah && Questing.Recieved(VALSHARAH_START);
		}
	}
	public static bool DoHighmountain
	{
		get
		{
			return NeedHighmountain && Questing.Recieved(HIGHMOUNTAIN_START);
		}
	}
	public static bool DoStormheim
	{
		get
		{
			return NeedStormheim && Questing.Recieved(STORMHEIM_START_ALLIANCE, STORMHEIM_START_HORDE);
		}
	}
	public static bool DoSuramar
	{
		get
		{
			return NeedSuramar && Questing.Recieved(SURAMAR_START1, SURAMAR_START2);
		}
	}
	public static bool DoBrokenShore
	{
		get
		{
			return NeedBrokenShore && Questing.Recieved(BROKEN_SHORE_START);
		}
	}
	public static bool DoArgus
	{
		get
		{
			return NeedArgus && Questing.Recieved(ARGUS_START1, ARGUS_START2, ARGUS_START3, ARGUS_START4);
		}
	}
	public static bool DoAny
	{
		get
		{
			return DoAzsuna || DoValsharah || DoHighmountain || DoStormheim || DoSuramar || DoBrokenShore || DoArgus;
		}
	}
	public static bool NeedAny
	{
		get
		{
			return NeedAzsuna || NeedValsharah || NeedHighmountain || NeedStormheim || NeedSuramar || NeedBrokenShore || NeedArgus;
		}
	}
	public static bool NeedAzsuna
	{
		get
		{
			return Questing.NotComplete(AZSUNA_FINAL_ALLIANCE, AZSUNA_FINAL_HORDE);
		}
	}
	public static bool NeedValsharah
	{
		get
		{
			return Questing.NotComplete(VALSHARAH_FINAL);
		}
	}
	public static bool NeedHighmountain
	{
		get
		{
			return Questing.NotComplete(HIGHMOUNTAIN_FINAL);
		}
	}
	public static bool NeedStormheim
	{
		get
		{
			return Questing.NotComplete(STORMHEIM_FINAL_ALLIANCE, STORMHEIM_FINAL_HORDE);
		}
	}
	public static bool NeedSuramar
	{
		get
		{
			return ObjectManager.Me.Level >= 110 && !NeedBrokenShore && (!Questing.Has(SURAMAR_RAID) && Questing.NotComplete(SURAMAR_FINAL));
		}
	}
	public static bool NeedBrokenShore
	{
		get
		{
			return ObjectManager.Me.Level >= 110 && Questing.NotComplete(BROKEN_SHORE_FINAL);
		}
	}
	public static bool NeedArgus
	{
		get
		{
			return ObjectManager.Me.Level >= 110 && !NeedBrokenShore && (!Questing.Has(ARGUS_DUNGEON) && Questing.NotComplete(ARGUS_FINAL));
		}
	}
	#endregion CONDITIONS

	#region MISSION
	public static class Mission
	{
		public static bool IsVisible
		{
			get
			{
				return Questing.IsVisible("OrderHallMissionFrame");
			}
		}
		public static bool Open()
		{
			if (!IsVisible)
				return false;

			Lua.LuaDoString("OrderHallMissionFrame.Tab1:Click();");
			Thread.Sleep(Others.Random(1500, 2000));
			return true;
		}
		public static bool Map()
		{
			if (!IsVisible)
				return false;

			Lua.LuaDoString("OrderHallMissionFrame.Tab3:Click();");
			Thread.Sleep(Others.Random(1500, 2000));
			return true;
		}
		public static bool Do(int missionID, params int[] followerList)
		{
			if (!IsVisible)
				return false;

			foreach (int follower in followerList)
			{
				string lua = @"
local missionID = {0};
local followerID = {1};
local allFollowers = C_Garrison.GetFollowers();
for i=1,#allFollowers do
	if (allFollowers[i].garrFollowerID == followerID)  then  
		C_Garrison.AddFollowerToMission(missionID, allFollowers[i].followerID);
		break;
	end
end
";
				var runCode = string.Format(lua, missionID, follower);
				Lua.LuaDoString(runCode);
				Thread.Sleep(Others.Random(750, 1500));
			}

			Thread.Sleep(Others.Random(750, 1500));
			Lua.LuaDoString(string.Format("C_Garrison.StartMission({0});", missionID));
			Thread.Sleep(Others.Random(2500, 3500));
			Close();
			return true;
		}
		public static bool Complete(int missionID)
		{
			string lua = @"
local missionNum = {0};
local allMissions = C_Garrison.GetInProgressMissions(LE_FOLLOWER_TYPE_GARRISON_7_0);
for i=1,#allMissions do
	if (allMissions[i].missionID == missionNum)  then  
		return allMissions[i].timeLeftSeconds
	end
end";
			var runCode = string.Format(lua, missionID);
			int timeLeft = Lua.LuaDoString<int>(runCode);
			if (timeLeft == 0)
				return true;
			else
				return false;
		}

		public static bool InProgress(int missionID)
		{
			string lua = @"
local missionNum = {0};
local allMissions = C_Garrison.GetInProgressMissions(LE_FOLLOWER_TYPE_GARRISON_7_0);
for i=1,#allMissions do
	if (allMissions[i].missionID == missionNum)  then  
		return allMissions[i].inProgress
	end
end
";
			var runCode = string.Format(lua, missionID);
			var r = Lua.LuaDoString<bool>(runCode);
			return r;
		}
		public static bool Done(int missionID)
		{
			return InProgress(missionID) && Complete(missionID);
		}
		public static bool NotDone(int missionID)
		{
			return InProgress(missionID) && !Complete(missionID);
		}
		public static bool CloseCompleteReport()
		{
			if (!IsVisible)
				return false;

			var lua = "OrderHallMissionFrameMissions.CompleteDialog.BorderFrame.ViewButton:Click();";
			Lua.LuaDoString(lua);
			Thread.Sleep(Others.Random(1000, 2000));
			return true;
		}
		public static bool MissionBonusRoll(int missionID)
		{
			if (!IsVisible)
				return false;

			var lua = "C_Garrison.MissionBonusRoll({0});";
			var runCode = string.Format(lua, missionID);
			Lua.LuaDoString(runCode);
			Thread.Sleep(Others.Random(1000, 2000));
			return true;
		}
		public static bool Close()
		{
			if (!IsVisible)
				return false;

			var lua = "OrderHallMissionFrame.CloseButton:Click();";
			Lua.LuaDoString(lua);
			Thread.Sleep(Others.Random(1000, 2000));
			return true;
		}
	}
	#endregion MISSION

	#region WHISTLE
	public static class Whistle
	{
		public const uint ID = 141605;
		public static bool Can(bool isForced = false)
		{
			if (!ItemsManager.HasItemById(ID))
				return false;

			if (ObjectManager.Me.IsIndoors)
				return false;

			if (isForced)
				return true;

			return !Questing.ItemOnCooldown(ID);
		}
		public static bool Complete(bool isForced = false)
		{
			return !Can(isForced);
		}
		public static void Use(bool isForced = false)
		{
			if (!ItemsManager.HasItemById(ID))
				return;

			if (ObjectManager.Me.InCombat)
				return;

			Questing.Use( (int) ID);
		}
	}
	#endregion WHISTLE
}