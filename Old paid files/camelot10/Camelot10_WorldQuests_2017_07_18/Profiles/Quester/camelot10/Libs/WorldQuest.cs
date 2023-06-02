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
using System.ComponentModel;
using System.IO;

public class WorldQuest : QuestClass
{
#region CORE
	static List<int> _cache = new List<int>();
	static internal robotManager.Helpful.Timer _timer = new robotManager.Helpful.Timer();
	static int _timerDefault = 1;
	public delegate bool BoolDelegate();

	public WorldQuest()
	{
		Name = "_WorldQuest";
		SetDefaultSettings();
	}

	static WorldQuest()
	{
	}

	public static bool Can(int questId)
	{
		return Has(questId);
	}

	public static bool Can(QuestClass quest)
	{
		return Has(quest);
	}

	public static bool Complete(int questId)
	{
		return !Has(questId);
	}

	public static int Progress(int questId, int obj)
	{
		var toRun = string.Format(@"local text, type, unknown, progress, progressTotal = GetQuestObjectiveInfo({0}, {1}, false) if progressTotal > 0 then return progress end return -1", questId, obj);
		var result = Lua.LuaDoString<int>(toRun);
		return result;
	}

	public static bool Complete(QuestClass quest)
	{
		return !Has(quest);
	}

	public static bool Complete(QuestClass quest, int obj)
	{
		if (Complete(quest))
			return true;

		foreach (var questId in quest.QuestId)
		{
			if (Complete(questId, obj))
				return true;
		}

		return false;
	}

	public static bool Complete(int questId, int obj, int minProgress)
	{
		if (!Has(questId))
			return false;

		var progress = Progress(questId, obj);
		if (progress == -1)
			return false;

		if (progress >= minProgress)
			return true;

		return false;
	}

	public static bool Complete(int questId, int obj)
	{
		return ObjectiveComplete(obj, questId);
	}

	public static bool ObjectiveComplete(int obj, int questId)
	{
		//return (Has(questId) && Quest.IsObjectiveComplete(obj, questId)) || Complete(questId);
		return (Has(questId) && IsFinished(questId, obj)) || Complete(questId);
	}

	public static bool IsFinished(int questId, int obj)
	{
		string luaCode = @"local text, objectiveType, finished = GetQuestObjectiveInfo({0}, {1}, false); return finished;";
		var toRun = string.Format(luaCode, questId, obj);
		var result = Lua.LuaDoString<bool>(toRun);
		return result;
	}

	public static bool HaveQuick(int questId)
	{
		string luaCode = @"
SetMapByID({1});
local taskInfo = C_TaskQuest.GetQuestsForPlayerByMapID({1})
if (taskInfo) then
	for i, info in ipairs(taskInfo) do
		if info.questId == {0} then
			return true;
		end
	end
end
return false;
";
		var toRun = string.Format(luaCode, questId, MapId.Current);
		var result = Lua.LuaDoString<bool>(toRun);
		return result;
	}

	public static bool Have(int questId)
	{
		UpdateWorldQuests();
		return _cache.Contains(questId);
	}

	public static void ResetUpdateTimer(int minutes)
	{
		_timer.Reset(minutes * 60 * 1000);
	}

	static void UpdateWorldQuests()
	{
		if (!_timer.IsReady)
			return;

		ResetUpdateTimer(_timerDefault);
		string luaCode = @"
local questIds = ''
local taskInfo = C_TaskQuest.GetQuestsForPlayerByMapID({0}, 1007)
if taskInfo then
	for i, info in ipairs(taskInfo) do
		--if HaveQuestData(info.questId) then
			if QuestUtils_IsQuestWorldQuest(info.questId) then
				questIds = questIds .. info.questId .. '#LUASEPARATOR#'
			end
		--end
	end
end
return questIds
";
		luaCode = luaCode.Replace("#LUASEPARATOR#", Lua.ListSeparator);
		var toRun = string.Format(luaCode, MapId.Current);
		_cache = Lua.LuaDoString<List<int>>(toRun);
		_cache.RemoveAll(i => i == 0);
		Logging.Write("[World Quests] " + string.Format(" quests={0} for zone={1}", _cache.Count, MapId.Current) + " " + string.Join(",", _cache));
	}

	public static bool Has(QuestClass quest)
	{
		foreach (var questId in quest.QuestId)
		{
			if (Has(questId))
				return true;
		}
		return false;
	}

	public static bool Has(int questId)
	{
		return Have(questId);
	}

#endregion CORE

#region MAP ID

	public class MapId
	{
		public const int Azsuna = 1015;
		public const int Valsharah = 1018;
		public const int Highmountain = 1024;
		public const int Stormheim = 1017;
		public const int Suramar = 1033;
		public const int Dalaran = 1014;
		public const int BrokenShore = 1021;
		public const int EyeOfAzshara = 1096;
		const string varName = "WorldQuestCurrentMapId";

		public static Vector3 AzsunaCenter = new Vector3(-113.8004, 6892.644, 16.99081, "None");
		public static Vector3 ValsharahCenter = new Vector3(2299.88, 6577.66, 141.787, "None");
		public static Vector3 HighmountainCenter = new Vector3(4158.713, 4383.217, 768.2132, "None");
		public static Vector3 StormheimCenter = new Vector3(3205.59, 1515.58, 180.7688, "None");
		public static Vector3 SuramarCenter = new Vector3(1615.52, 4757.86, 138.3141, "None");
		public static Vector3 BrokenShoreCenter = new Vector3(-1647.68, 3178.64, 135.931, "None");
		public static Vector3 DalaranCenter = AzsunaCenter;

		//robotManager.Helpful.Var.SetVar("WorldQuestCurrentMapId", 1018);
		public static int Current
		{
			get
			{
				var currentVar = Suramar;
				if (Var.Exist(varName))
				{
					currentVar = Var.GetVar<int>(varName);
				}
				return currentVar;
			}
			set
			{
				Var.SetVar(varName, value);
			}
		}
		public static int CurrentIngame
		{
			get
			{
				var mapID = Lua.LuaDoString<int>("local mapID, isContinent = GetCurrentMapAreaID(); return mapID;");
				//thundertotem
				if (mapID == 1080)
					mapID = Highmountain;

				//azsuna invasion scenario
				if (Usefuls.ContinentId == (int)ContinentId.LegionShipHorizontalAzsuna)
					mapID = Azsuna;

				return mapID;
			}
		}
		public static Vector3 GetCenter(int mapID)
		{
			switch (mapID)
			{
				case BrokenShore:
					return BrokenShoreCenter;
				case Suramar:
					return SuramarCenter;
				case Stormheim:
					return StormheimCenter;
				case Highmountain:
					return HighmountainCenter;
				case Valsharah:
					return ValsharahCenter;
				case Azsuna:
					return AzsunaCenter;
				case Dalaran:
				default:
					return DalaranCenter;
			}
		}
	}

	#endregion MAP ID

	public static void MovePathFromNear(List<Vector3> path, bool ignoreFight = true, BoolDelegate condition = null)
	{
		PathFromNear(path, ignoreFight, condition);
	}
	/// <summary>
	/// copy from Questing.cs > PathFromNear
	/// </summary>
	/// <param name="path"></param>
	/// <param name="ignoreFight"></param>
	/// <param name="condition"></param>
	public static void PathFromNear(List<Vector3> path, bool ignoreFight = false, BoolDelegate condition = null)
	{
		if (condition == null)
			condition = () => true;

		var closest = path.OrderBy(v => ObjectManager.Me.Position.DistanceTo(v)).FirstOrDefault();
		var clampedPath = new List<Vector3>();
		bool include = false;
		for (int i = 0; i < path.Count; i++)
		{
			if (!include)
			{
				include = path[i].DistanceTo(closest) < 3.5f;
			}
			else
			{
				clampedPath.Add(path[i]);
			}
		}

		Path(clampedPath, ignoreFight, condition);
	}
	public static void MovePath(List<Vector3> path, bool ignoreFight = true, BoolDelegate condition = null)
	{
		Path(path, ignoreFight, condition);
	}
	/// <summary>
	/// copy from Questing.cs
	/// </summary>
	/// <param name="path"></param>
	/// <param name="ignoreFight"></param>
	/// <param name="condition"></param>
	public static void Path(List<Vector3> path, bool ignoreFight = false, BoolDelegate condition = null)
	{
		if (condition == null)
			condition = () => true;

		var oldIgnore = Conditions.ForceIgnoreIsAttacked;
		if (ignoreFight)
			Conditions.ForceIgnoreIsAttacked = true;

		MovementManager.Go(path);
		//while (MovementManager.InMovement && Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause && !Conditions.IsAttackedAndCannotIgnore && condition())
		do
		{
			Thread.Sleep(1000);
		}
		while (MovementManager.InMovement && Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause && !Conditions.IsAttackedAndCannotIgnore && condition());
		MovementManager.StopMove();
		if (ignoreFight)
			Conditions.ForceIgnoreIsAttacked = oldIgnore;
	}
	/// <summary>
	/// copy from Questing.cs
	/// </summary>
	/// <param name="timeout"></param>
	public static void CancelCutscene(int timeout = 500)
	{
		Thread.Sleep(1500 + timeout);
		var lua = @"
if MovieFrame:IsVisible() then
	MovieFrame:StopMovie();
else
	CinematicFrame_CancelCinematic();
end
GameMovieFinished();
StopCinematic();
";
		Lua.LuaDoString(lua);
		Thread.Sleep(timeout);
	}

	#region KILL
	public static void KillAll(Vector3 campPosition, float radius)
	{
		var enemyEntry = ObjectManager.GetWoWUnitHostile()
			.Where(u => u != null && u.IsValid && u.IsAttackable && u.Position.DistanceTo(campPosition) < radius)
			.OrderBy(u => u.Position.DistanceTo(ObjectManager.Me.Position))
			.FirstOrDefault();
		if (enemyEntry != null)
			Fight.StartFight(enemyEntry.Guid);

		//var playerEntry = ObjectManager.GetObjectWoWPlayer().Where(u => u != null && u.IsValid && u.IsAttackable && u.Position.DistanceTo(campPosition) < radius)
		var playerEntry = ObjectManager.GetObjectWoWPlayer().Where(u => u != null && u.IsValid && u.Position.DistanceTo(campPosition) < radius && UnitCanAttack.CanAttack(u.GetBaseAddress, ObjectManager.Me.GetBaseAddress))
			.OrderBy(u => u.Position.DistanceTo(ObjectManager.Me.Position))
			.FirstOrDefault();
		if (playerEntry != null)
			Fight.StartFight(playerEntry.Guid);
	}


	public static void TryKill(Vector3 campPosition, int mobId, double healthPercent = 20, float radius = 10)
	{
		TryKill(campPosition, new List<int>() { mobId }, healthPercent, radius);
	}

	public static void TryKill(Vector3 campPosition, List<int> mobsId, double healthPercent = 20, float radius = 10)
	{
		var mobEntry = ObjectManager.GetNearestWoWUnit(ObjectManager.GetWoWUnitByEntry(mobsId));
		if (mobEntry != null && mobEntry.IsAlive && mobEntry.IsAttackable && mobEntry.IsValid)
		{
			if (mobEntry.Health <= SoloHPAmount || mobEntry.HealthPercent <= healthPercent)
			{
				Interact.InteractGameObject(mobEntry.GetBaseAddress);
				Fight.StartFight(mobEntry.Guid);
				//Logging.Write("attack mob");
				return;
			}
		}

		else if (ObjectManager.Me.Position.DistanceTo(campPosition) > radius)
		{
			//Logging.Write("going to camp position");
			campPosition = robotManager.Helpful.Math.GetRandomPointInCircle(campPosition, radius - 0.1f);
			GoToTask.ToPosition(campPosition);
			MountTask.DismountMount();
		}
		wManager.Wow.Bot.States.AntiAfk.Pulse();
	}

	public static void TryKillElite(Vector3 campPosition, List<int> mobsId, float radius = 10)
	{
		TryKill(campPosition, mobsId, WorldQuestSettings.CurrentSetting.PercentHPElite, radius);
	}

	public static void TryKillRaid(Vector3 campPosition, List<int> mobsId, float radius = 10)
	{
		TryKill(campPosition, mobsId, WorldQuestSettings.CurrentSetting.PercentHPRaid, radius);
	}

	public static void TryKillAnyElite(Vector3 campPosition, List<int> mobsId, float radius)
	{
		TryKillAny(campPosition, mobsId, WorldQuestSettings.CurrentSetting.PercentHPElite, radius);
	}

	public static void TryKillAny(Vector3 campPosition, int mobId, double healthPercent = 20, float radius = 10)
	{
		TryKillAny(campPosition, new List<int>() { mobId }, healthPercent, radius);
	}

	public static void TryKillAny(Vector3 campPosition, List<int> mobsId, double healthPercent = 20, float radius = 10)
	{
		var mobs = ObjectManager.GetWoWUnitByEntry(mobsId);
		foreach (var mobEntry in mobs)
		{
			if (mobEntry != null && mobEntry.IsAlive && mobEntry.IsAttackable && mobEntry.IsValid)
			{
				if (mobEntry.Health <= SoloHPAmount || mobEntry.HealthPercent <= healthPercent)
				{
					Interact.InteractGameObject(mobEntry.GetBaseAddress);
					Fight.StartFight(mobEntry.Guid);
					return;
				}
			}
		}
		if (ObjectManager.Me.Position.DistanceTo(campPosition) > radius)
		{
			campPosition = robotManager.Helpful.Math.GetRandomPointInCircle(campPosition, radius - 0.1f);
			wManager.Wow.Bot.Tasks.GoToTask.ToPosition(campPosition);
			MountTask.DismountMount();
		}
	}

	public static long SoloHPAmount
	{
		get
		{
			return WorldQuestSettings.CurrentSetting.CanSoloHP * 1000000;
		}
	}

#endregion KILL

#region CAN/COMPLETE

#region INVASION
	public static bool CompleteInvasion(int questID)
	{
		if (!WorldQuestSettings.CurrentSetting.DoInvasion)
			return true;
		return Complete(questID);
	}
	public static bool CompleteInvasion(int questID, int obj)
	{
		if (!WorldQuestSettings.CurrentSetting.DoInvasion)
			return true;

		return Complete(questID, obj);
	}
	public static bool CompleteInvasion(QuestClass quest)
	{
		if (!WorldQuestSettings.CurrentSetting.DoInvasion)
			return true;
		return Complete(quest);
	}
	public static bool CompleteInvasion(QuestClass quest, int obj)
	{
		if (!WorldQuestSettings.CurrentSetting.DoInvasion)
			return true;

		if (Complete(quest))
			return true;

		foreach (var questId in quest.QuestId)
		{
			if (Complete(questId, obj))
				return true;
		}
		return false;
	}
	public static bool CompleteInvasionGroup(QuestClass quest, int obj)
	{
		if (!WorldQuestSettings.CurrentSetting.DoGroup)
			return true;
		return CompleteInvasion(quest, obj);
	}
	public static bool CompleteInvasionGroup(QuestClass quest)
	{
		if (!WorldQuestSettings.CurrentSetting.DoGroup)
			return true;
		return CompleteInvasion(quest);
	}
	public static bool CompleteInvasionRaid(QuestClass quest)
	{
		if (!WorldQuestSettings.CurrentSetting.DoRaid)
			return true;
		return CompleteInvasion(quest);
	}
#endregion INVASION

	public static bool CanGroup(int questId)
	{
		return !CompleteGroup(questId);
	}

	public static bool CanGroup(QuestClass quest)
	{
		return !CompleteGroup(quest);
	}
	public static bool CompleteGroup(QuestClass quest)
	{
		return !WorldQuestSettings.CurrentSetting.DoGroup || Complete(quest);
	}

	public static bool CompleteGroup(int questId)
	{
		return !WorldQuestSettings.CurrentSetting.DoGroup || Complete(questId);
	}

	public static bool CompleteGroup(int questId, int obj)
	{
		return !WorldQuestSettings.CurrentSetting.DoGroup || Complete(questId, obj);
	}

	public static bool CanRaid(int questId)
	{
		return !CompleteRaid(questId);
	}

	public static bool CompleteRaid(int questId)
	{
		return !WorldQuestSettings.CurrentSetting.DoRaid || Complete(questId);
	}

	public static bool CompleteRaid(int questId, int obj)
	{
		return !WorldQuestSettings.CurrentSetting.DoRaid || Complete(questId, obj);
	}

	public static bool CanPvP(int questId)
	{
		return !CompletePvP(questId);
	}

	public static bool CompletePvP(int questId)
	{
		return !WorldQuestSettings.CurrentSetting.DoPvP || Complete(questId);
	}

	public static bool CompletePvP(int questId, int obj)
	{
		return !WorldQuestSettings.CurrentSetting.DoPvP || Complete(questId, obj);
	}

	public static bool CanCooking(int questId)
	{
		return !CompleteCooking(questId);
	}

	public static bool CompleteCooking(int questId)
	{
		int currentValue = wManager.Wow.Helpers.Skill.GetValue(wManager.Wow.Enums.SkillLine.Cooking);
		return !WorldQuestSettings.CurrentSetting.DoCooking || currentValue < 100 || Complete(questId);
	}

	public static bool CompleteCooking(int questId, int obj)
	{
		int currentValue = wManager.Wow.Helpers.Skill.GetValue(wManager.Wow.Enums.SkillLine.Cooking);
		return !WorldQuestSettings.CurrentSetting.DoCooking || currentValue < 100 || Complete(questId, obj);
	}

	public static bool CanFishing(int questId)
	{
		return !CompleteFishing(questId);
	}

	public static bool CompleteFishing(int questId)
	{
		int currentValue = wManager.Wow.Helpers.Skill.GetValue(wManager.Wow.Enums.SkillLine.Fishing);
		return !WorldQuestSettings.CurrentSetting.DoFishing || currentValue < 100 || Complete(questId);
	}

	public static bool CompleteFishing(int questId, int obj)
	{
		int currentValue = wManager.Wow.Helpers.Skill.GetValue(wManager.Wow.Enums.SkillLine.Fishing);
		return !WorldQuestSettings.CurrentSetting.DoFishing || currentValue < 100 || Complete(questId, obj);
	}

	public static bool CanSkinning(int questId)
	{
		return !CompleteSkinning(questId);
	}

	public static bool CompleteSkinning(int questId)
	{
		int currentValue = wManager.Wow.Helpers.Skill.GetValue(wManager.Wow.Enums.SkillLine.Skinning);
		return !WorldQuestSettings.CurrentSetting.DoSkinning || currentValue < 100 || Complete(questId);
	}

	public static bool CompleteSkinning(int questId, int obj)
	{
		int currentValue = wManager.Wow.Helpers.Skill.GetValue(wManager.Wow.Enums.SkillLine.Skinning);
		return !WorldQuestSettings.CurrentSetting.DoSkinning || currentValue < 100 || Complete(questId, obj);
	}

	public static bool CanMining(int questId)
	{
		return !CompleteMining(questId);
	}

	public static bool CompleteMining(int questId)
	{
		int currentValue = wManager.Wow.Helpers.Skill.GetValue(wManager.Wow.Enums.SkillLine.Mining);
		return !WorldQuestSettings.CurrentSetting.DoMining || currentValue < 100 || Complete(questId);
	}

	public static bool CompleteMining(int questId, int obj)
	{
		int currentValue = wManager.Wow.Helpers.Skill.GetValue(wManager.Wow.Enums.SkillLine.Mining);
		return !WorldQuestSettings.CurrentSetting.DoMining || currentValue < 100 || Complete(questId, obj);
	}

	public static bool CanHerbalism(int questId)
	{
		return !CompleteHerbalism(questId);
	}

	public static bool CompleteHerbalism(int questId)
	{
		int currentValue = wManager.Wow.Helpers.Skill.GetValue(wManager.Wow.Enums.SkillLine.Herbalism);
		return !WorldQuestSettings.CurrentSetting.DoHerbalism || currentValue < 100 || Complete(questId);
	}

	public static bool CompleteHerbalism(int questId, int obj)
	{
		int currentValue = wManager.Wow.Helpers.Skill.GetValue(wManager.Wow.Enums.SkillLine.Herbalism);
		return !WorldQuestSettings.CurrentSetting.DoHerbalism || currentValue < 100 || Complete(questId, obj);
	}
	public static bool CanFly
	{
		get
		{
			return ObjectManager.Me.HaveBuff(233368); // http://www.wowhead.com/spell=233368/broken-isles-pathfinder
		}
	}
	public static bool CanFlyLegion
	{
		get
		{
			return CanFly;
		}
	}

#endregion CAN/COMPLETE

#region WHISTLE
	public class Whistle
	{
		public const uint whistleId = 141605;

		public static bool Can(bool isForced = false)
		{
			return !Complete(isForced);
		}

		public static bool Complete(bool isForced = false)
		{
			if (!isForced)
			{
				if (!WorldQuestSettings.CurrentSetting.UseWhistle)
					return true;

				var flightMaster = ObjectManager.GetNearestWoWUnit(ObjectManager.GetWoWUnitFlightMaster());
				if (flightMaster != null && flightMaster.IsValid && flightMaster.Position.DistanceTo(ObjectManager.Me.Position) < wManager.wManagerSetting.CurrentSetting.FlightMasterDiscoverRange)
					return true;

				var timeLeft = Lua.LuaDoString<double>("local startTime, duration, enable = GetItemCooldown(" + whistleId + "); return startTime + duration - GetTime();");
				if (timeLeft > 0)
					return true;
			}
			return ObjectManager.Me.IsIndoors;
		}

		public static bool Use()
		{
			if (!ItemsManager.HasItemById(whistleId))
			{
				Logging.WriteError("Don't have [Flight Master's Whistle].");
				return true;
			}
			if (ObjectManager.Me.InCombat)
				return true;

			ItemsManager.UseItem(whistleId);
			Usefuls.WaitIsCasting();
			Thread.Sleep(10 * 1000);
			return true;
		}
	}
#endregion WHISTLE

#region MURKY
	public class Murky : QuestClass
	{
		public Murky()
		{
			Name = "Murky";
			Step.AddRange(new[] { 0, 0, 0, 0, 0 });
		}
		protected void Log(string text)
		{
			//Logging.Write("[Murky] " + text);
		}
		public static bool TryFind(List<int> mobs, List<Vector3> path)
		{
			var mob = ObjectManager.GetNearestWoWUnit(ObjectManager.GetWoWUnitByEntry(mobs));
			if (mob != null && mob.IsAlive && mob.IsAttackable && mob.IsValid)
			{
				Interact.InteractGameObject(mob.GetBaseAddress);
				Thread.Sleep(Usefuls.Latency + 150);
				return true;
			}
			if (!MovementManager.InMovementLoop)
			{
				var closest = path.OrderBy(v => ObjectManager.Me.Position.DistanceTo(v)).FirstOrDefault();
				GoToTask.ToPosition(closest);
				MovementManager.GoLoop(path);
				return true;
			}
			return true;
		}
		public static bool TryCombat()
		{
			var target = ObjectManager.GetUnitAttackPlayer().FirstOrDefault();
			if (target == null)
				target = ObjectManager.Me.TargetObject;

			if (target == null || !target.IsAlive || !target.IsAttackable || !target.IsValid)
				return false;

			Interact.InteractGameObject(target.GetBaseAddress);
			var dist = ObjectManager.Me.Position.DistanceTo(target.Position);
			if (dist > 5)
			{
				var result = false;
				var path = PathFinder.FindPath(target.Position, out result);
				if (result)
					MovementManager.Go(path);

				Thread.Sleep(1 * 1000);
				return true;
			}

			var lua = @"start, duration, enabled = GetSpellCooldown('{0}'); return start;";

			uint spellBubble = 179038;
			if (ObjectManager.Pet.HealthPercent < 50 && Lua.LuaDoString<int>(string.Format(lua, spellBubble)) == 0)
			{
				MovementManager.Face(ObjectManager.Target);
				Lua.RunMacroText("/click [overridebar][vehicleui][possessbar,@vehicle,exists]OverrideActionBarButton3");
				Thread.Sleep(Usefuls.Latency * 2);
				return true;
			}

			uint spellMurlocks = 179062;
			if (Lua.LuaDoString<int>(string.Format(lua, spellMurlocks)) == 0)
			{
				MovementManager.Face(ObjectManager.Target);
				Lua.RunMacroText("/click [overridebar][vehicleui][possessbar,@vehicle,exists]OverrideActionBarButton4");
				Thread.Sleep(Usefuls.Latency * 2);
				return true;
			}

			uint spellSlime = 179021;
			if (Lua.LuaDoString<int>(string.Format(lua, spellSlime)) == 0)
			{
				MovementManager.Face(ObjectManager.Target);
				Lua.RunMacroText("/click [overridebar][vehicleui][possessbar,@vehicle,exists]OverrideActionBarButton1");
				Thread.Sleep(Usefuls.Latency * 2);
				return true;
			}
			return false;
		}
		public override bool IsComplete()
		{
			return WorldQuest.Complete(this);
		}
		public override bool CanConditions()
		{
			return WorldQuest.Can(this);
		}
		public override bool HasQuest()
		{
			return true;
		}
		public override bool IsCompleted()
		{
			return false;
		}
	}
#endregion MURKY

#region INVASION
	public static class Invasion
	{
		public static bool Active
		{
			get
			{
				return IsActive();
			}
		}
		public static bool IsActive()
		{
			return IsActive(MapId.Current);
		}
		public static bool IsActive(int mapID)
		{
			return TimeLeft(mapID) > 0;
		}
		public static int Time
		{
			get
			{
				return TimeLeft();
			}
		}
		public static int TimeLeft()
		{
			return TimeLeft(MapId.Current);
		}
		public static int TimeLeft(int mapID)
		{
			//var lua = @"local name, timeLeftMinutes, rewardQuestID = GetInvasionInfoByMapAreaID({0}) return timeLeftMinutes";
			var lua = @"
SetMapByID({0});
local numPOIs = GetNumMapLandmarks();
for i=1, numPOIs do
	local landmarkType, name, description, textureIndex, x, y, mapLinkID, inBattleMap, graveyardID, areaID, poiID, isObjectIcon, atlasIcon, displayAsBanner = C_WorldMap.GetMapLandmarkInfo(i);
	if (poiID and poiID > 0) then
		local timeLeftMinutes = C_WorldMap.GetAreaPOITimeLeft(poiID);
		--local name, timeLeftMinutes, rewardQuestID = GetInvasionInfo(poiID);
		if (timeLeftMinutes and timeLeftMinutes > 0) then
			return timeLeftMinutes
		end
	end
end
return 0
";
			var runCode = string.Format(lua, mapID);
			var result = Lua.LuaDoString<int>(runCode);
			return result;
		}

	}

#endregion INVASION

#region TRAVEL
	public static class Travel
	{
		static Travel()
		{
			
		}
		static void Log(string text)
		{
			Logging.WriteDebug("[World Quest Travel] " + text);
		}
		public static void ActivateAllTaxiNodes()
		{
			lock (Taxi.TaxiList.Locker)
			{
				foreach (var node in Taxi.TaxiList.Nodes)
				{
					Taxi.TaxiList.Active(node);
				}
			}
		}

		public static bool ToZone(int zoneID)
		{
			//wait taxi
			if (ObjectManager.Me.IsOnTaxi)
			{
				Thread.Sleep(1000);
				return false;
			}
			if (!WorldQuestSettings.CurrentSetting.UseTravel)
			{
				return true;
			}
			if (MapId.CurrentIngame != zoneID)
			{
				if (Usefuls.ContinentId != (int)ContinentId.Troll_Raid)
				{
					UseDalaranHeathstone();
					return false;
				}
				Log("try to take taxi to ingame zone=" + zoneID +" (current="+ MapId.CurrentIngame+")");
				//ActivateAllTaxiNodes();
				var nodes = Taxi.TaxiList.Nodes.Where(n => n.ContinentId == Usefuls.ContinentId).OrderBy(n => n.Position.DistanceTo(ObjectManager.Me.Position)).ToList();
				var checkNodesCount = 3;
				var count = nodes.Count >= checkNodesCount ? checkNodesCount : nodes.Count;
				if (count < 1)
				{
					Log("no taxi nodes");
					return true;
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
						Log("skip node #" + i +". no path to node: " + node.Name);
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
					return true;
				}
				var p = neareastNode.Position;
				var skip = wManager.wManagerSetting.CurrentSetting.BlackListIfNotCompletePath;
				BooleanDelegate condition = null;
				if (!GoToTask.ToPosition(p, 15f, skip, condition))
				{
					Log("cannot reach node position " + p + " ");
					return true;
				}
				var flightmaster = wManager.Wow.Bot.States.FlightMasterTakeTaxiState.GetNearestFlightMaster();
				if (!flightmaster.IsValid)
				{
					Log("invalid flighmaster");
					return true;
				}
				if (!GoToTask.ToPositionAndIntecractWith(flightmaster, -1, skip, condition, false))
				{
					Log("cannot interact flighmaster");
					return true;
				}
				Thread.Sleep(Usefuls.Latency + 800);
				if (!Taxi.TakeNearestNode(MapId.GetCenter(zoneID), true))
				{
					Log("cannot fly to destination");
					return true;
				}
				return false;
			}
			Log("in destination zone");
			return true;
		}

		public static bool UseDalaranHeathstone()
		{
			uint dalaranHeathstoneID = 140192;
			if (!ItemsManager.HasItemById(dalaranHeathstoneID))
			{
				Logging.Write("WARNING! You need 'Dalaran heathstone'");
				Thread.Sleep(5000);
				return false;
			}
			var cd = Lua.LuaDoString<int>("local startTime, duration, enable = GetItemCooldown(" + dalaranHeathstoneID + "); return startTime + duration - GetTime();");
			if (cd > 0)
			{
				Thread.Sleep(1000);
				return true;
			}
			ItemsManager.UseItem(140192);
			Usefuls.WaitIsCasting();
			return true;
		}

	}
#endregion TRAVEL

#region QUEST BASE
	public class WorldQuestFollowPathClass : QuestFollowPathClass
	{
		public float Radius = 0;
		public WorldQuestFollowPathClass() : base()
		{
			Name = "World quest follow path";
			QuestId.Add(0);
			Step = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
			StepAutoDetect = new[] { false, false, false, false, false, false, false, false, false, false };
			PickUpQuestOnItem = false;
			PickUpQuestOnItemID = 0;
			GossipOptionItem = 1;
			WoWClass = wManager.Wow.Enums.WoWClass.None;
			MinLevel = 0;
			MaxLevel = 999;
			RequiredQuest = 0;
		}
		public virtual bool IsOnQuest()
		{
			foreach (var questId in QuestId)
			{
				if (questId > 0 && WorldQuest.Can(questId))
				{
					return true;
				}
			}
			return false;
		}
		public override bool IsComplete()
		{
			var result = base.IsComplete();
			if (result)
			{
				return result;
			}

			if (!IsOnQuest())
			{
				return true;
			}

			if (Radius > 0 && ObjectManager.Me.Position.DistanceTo(Path[Path.Count - 1]) < Radius)
				return true;

			return false;
		}
		public override bool CanConditions()
		{
			return !IsComplete();
		}
		public override bool IsCompleted()
		{
			return false;
		}
		public override bool HasQuest()
		{
			return true;
		}
	}

	public class QuestGoFromClass : WorldQuestFollowPathClass
	{
		public bool IsInDoors = true;

		public QuestGoFromClass() : base()
		{
			Name = "Quest Go from";
		}

		public override bool IsComplete()
		{
			var pathStart = Path[0];
			var pathEnd = Path[Path.Count - 2];
			var maxDist = pathStart.DistanceTo(pathEnd);
			var result = false;
			if (IsInDoors)
			{
				result = result || !ObjectManager.Me.IsIndoors;
			}
			result = result || ObjectManager.Me.Position.DistanceTo(pathStart) > maxDist;
			return result;
		}
	}

#endregion QUEST BASE

#region SETTINGS
	public class WorldQuestSettings : robotManager.Helpful.Settings
	{
		public static WorldQuestSettings CurrentSetting { get; set; }

		public WorldQuestSettings()
		{
			UseWhistle = true;
			DoGroup = true;
			DoRaid = true;
			DoPvP = true;
			DoInvasion = true;
			DoCooking = true;
			DoFishing = true;
			DoSkinning = true;
			DoMining = true;
			DoHerbalism = true;
			UseFlying = true;
			UseTravel = true;
			CanSoloHP = 1;
			PercentHPElite = 55;
			PercentHPRaid = 15;
			DoInvasionScenario = false;
			robotManager.Events.ProductEvents.OnProductPauseStarted -= OnProductPauseStarted;
			robotManager.Events.ProductEvents.OnProductPauseStarted += OnProductPauseStarted;
		}

		void OnProductPauseStarted(string productName)
		{
			ShowCurrent();
		}

		static string GetWindowName()
		{
			return "WorldQuestSettingsForm1";
		}

		static System.Text.StringBuilder sBuilder;
		static string GetFileName()
		{
			string identifer = ObjectManager.Me.Name + "." + Usefuls.RealmName;
			if (sBuilder == null)
			{
				System.Security.Cryptography.MD5 md5Hasher = System.Security.Cryptography.MD5.Create();
				byte[] data = md5Hasher.ComputeHash(System.Text.Encoding.Default.GetBytes(identifer));
				sBuilder = new System.Text.StringBuilder();
				for (int i = 0; i < data.Length; i++)
				{
					sBuilder.Append(data[i].ToString("x2"));
				}
			}
			return AdviserFilePathAndName("WorldQuestsSettings", sBuilder.ToString());
		}

		public bool Save()
		{
			try
			{
				var result = Save(GetFileName());
				CanShowMenu = false;
				return result;
			}
			catch (Exception e)
			{
				Logging.WriteDebug("WorldQuestSettings => Save(): " + e);
				return false;
			}
		}
		public static bool Load()
		{
			try
			{
				if (File.Exists(GetFileName()))
				{
					CurrentSetting = Load<WorldQuestSettings>(GetFileName());
					return true;
				}
				CurrentSetting = new WorldQuestSettings();
			}
			catch (Exception e)
			{
				Logging.WriteDebug("WorldQuestSettings => Load(): " + e);
			}
			return false;
		}

		public static void ShowCurrent()
		{
			if (CanShowMenu)
				CurrentSetting.ShowForm();
		}

		static robotManager.Helpful.Timer _timerMenu = new robotManager.Helpful.Timer();
		static string _timerVarName = "WorldQuestSettingMenuTimer";
		public static bool CanShowMenu
		{
			get
			{
				if (!Var.Exist(_timerVarName) || !Var.IsType(_timerVarName, typeof(long)))
				{
					return true;
				}

				_timerMenu.Reset();
				var currentTimer = _timerMenu.StartTime;
				var currentEndTimer = Var.GetVar<long>(_timerVarName);
				return currentTimer > currentEndTimer;
			}
			set
			{
				if (value == false)
				{
					_timerMenu.Reset(2 * 1000);
					var currentTimer = _timerMenu.StartTime;
					var currentEndTimer = currentTimer + (long)_timerMenu.CountDowntime;
					Var.SetVar(_timerVarName, currentEndTimer);
				}
			}
		}

		System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
		void ShowForm()
		{
			try
			{
				for (int i = 0; i < Application.OpenForms.Count; i++)
				{
					if (Application.OpenForms[i].Name.Equals(GetWindowName()))
					{
						Application.OpenForms[i].BringToFront();
						return;
					}
				}
				int padding = 5;
				int buttonContinueHeight = 50;
				int buttonResetHeight = 25;

				Form form1 = new Form();
				var size = new Point(300, 500);
				form1.Name = GetWindowName();
				form1.ClientSize = new Size(size);
				form1.Text = "World Quests Settings";
				Form form2 = form1;
				form2.FormBorderStyle = FormBorderStyle.FixedSingle;
				form2.MaximizeBox = false;
				PropertyGrid propertyGrid1 = new PropertyGrid();
				propertyGrid1.Name = "Settings Grid";
				Size gridSize = new Size(size.X - (padding * 1), size.Y - (padding * 3) - buttonContinueHeight - buttonResetHeight);
				propertyGrid1.Size = gridSize;
				int num5 = 15;
				propertyGrid1.Anchor = (AnchorStyles)num5;
				Point point = new Point(padding, (padding * 2) + buttonResetHeight);
				propertyGrid1.Location = point;
				propertyGrid1.SelectedObject = (object)this;
				PropertyGrid propertyGrid2 = propertyGrid1;
				form2.Controls.Add((Control)propertyGrid2);

				//continue
				var buttonContinue = new Button()
				{
					Location = new Point(padding, size.Y - padding - buttonContinueHeight),
					Size = new Size(size.X - (padding * 2), buttonContinueHeight),
					Text = "Save",
					UseVisualStyleBackColor = true
				};
				buttonContinue.Click += (object sender, EventArgs e) =>
				{
					form2.Hide();
					form2.Close();
					form2.Dispose();
				};
				form2.Controls.Add(buttonContinue);

				/*timer
				int seconds = AutoStartTimer;
				if (seconds < 5)
					seconds = 5;
				int timerSeconds = 1;
				timer.Interval = timerSeconds * 1000; //seconds
				timer.Tick += (object sender, EventArgs e) =>
				{
					seconds -= timerSeconds;
					buttonContinue.Text = "Continue. (Autostart in: " + seconds + " secs)";
					if (seconds <= 0)
					{
						timer.Stop();
						buttonContinue.PerformClick();
					}
				};
				timer.Start();
				//*/

				//reset
				var buttonReset = new Button()
				{
					//Location = new Point(padding, size.Y - (2 * padding) - buttonContinueHeight - buttonResetHeight),
					Location = new Point(padding, padding),
					Size = new Size(size.X - (padding * 2), buttonResetHeight),
					Text = "Reset settings",
					ForeColor = Color.Red,
					UseVisualStyleBackColor = true
				};
				buttonReset.Click += (object sender, EventArgs e) =>
				{
					buttonContinue.PerformClick();
					CurrentSetting = new WorldQuestSettings();
					CurrentSetting.ShowForm();
				};
				form2.Controls.Add(buttonReset);

				form2.FormClosed += (object sender, FormClosedEventArgs e) =>
				{
					Save();
					Logging.Write("" + CurrentSetting);
				};
				form2.ShowDialog();
			}
			catch (Exception e)
			{
				Logging.WriteDebug("WorldQuestSettings => ShowForm(): " + e);
			}
		}

		public override string ToString()
		{
			return "World Quest Settings: whistle=" + UseWhistle + " elite=" + DoGroup + " raid=" + DoRaid + " pvp=" + DoPvP + " soloHP=" + CanSoloHP + " eliteHP%=" + PercentHPElite + " raidHP%=" + PercentHPRaid +
				" cooking=" + DoCooking + " fishing=" + DoFishing + " skinning=" + DoSkinning + " mining=" + DoMining + " herbalism=" + DoHerbalism ;
		}

		// general
		[Setting]
		[Category("General")]
		[DisplayName("Flight Master's Whistle")]
		[Description("Use [Flight Master's Whistle]")]
		public bool UseWhistle
		{
			get
			{
				return _useWhistle;
			}
			set
			{
				_useWhistle = ItemsManager.HasItemById(141605) && value;
			}
		}
		bool _useWhistle;

		[Setting]
		[Category("General")]
		[DisplayName("Flying")]
		[Description("Use flying mount, disable flying if you stuck alot")]
		public bool UseFlying
		{
			get
			{
				return _flyingMount;
			}
			set
			{
				_flyingMount = value;
				wManager.wManagerSetting.CurrentSetting.UseFlyingMount = _flyingMount;
			}
		}
		bool _flyingMount;

		[Setting]
		[Category("General")]
		[DisplayName("Travel")]
		[Description("Use Dalaran Hearthstone + Taxi to travel to zone")]
		public bool UseTravel { get; set; }

		[Setting]
		[Category("General")]
		[DisplayName("Elite quests")]
		[Description("Do Group/Elite/WANTED quests, recommended item level > 810")]
		public bool DoGroup { get; set; }

		[Setting]
		[Category("General")]
		[DisplayName("Raid quests")]
		[Description("Do Raid/DANGER/DEADLY quests, recommended item level > 820")]
		public bool DoRaid { get; set; }

		[Setting]
		[Category("General")]
		[DisplayName("PvP quests")]
		[Description("Do PvP quests, recommended item level > 800")]
		public bool DoPvP { get; set; }

		[Setting]
		[Category("General")]
		[DisplayName("Invasion quests")]
		[Description("Do Legion Invasion quests, recommended item level > 800")]
		public bool DoInvasion { get; set; }

		[Setting]
		[Category("General")]
		[DisplayName("Invasion scenario")]
		[Description("Do Legion Invasion scenario. Warning group quest with other players. High report risk")]
		public bool DoInvasionScenario { get; set; }

		// combat
		[Setting]
		[Category("Combat")]
		[DisplayName("Can solo mHP")]
		[Description("I can kill mobs with equal or less than HP (in millions). Otherwise [Elite HP% Attack] settings")]
		public int CanSoloHP { get; set; }

		[Setting]
		[Category("Combat")]
		[DisplayName("Elite HP% Attack")]
		[Description("Camp Group/Elite/WANTED mob until it health percent reach that minimum")]
		public int PercentHPElite { get; set; }

		[Setting]
		[Category("Combat")]
		[DisplayName("Raid HP% Attack")]
		[Description("Camp Raid/DANGER/DEADLY mob until it health percent reach that minimum")]
		public int PercentHPRaid { get; set; }

		//profession
		[Setting]
		[Category("Profession")]
		[DisplayName("Cooking")]
		[Description("Do Cooking quests, require skill level >= 100")]
		public bool DoCooking { get; set; }

		[Setting]
		[Category("Profession")]
		[DisplayName("Fishing")]
		[Description("Do Fighing quests, require skill level >= 100")]
		public bool DoFishing { get; set; }

		[Setting]
		[Category("Profession")]
		[DisplayName("Skinning")]
		[Description("Do Skinning quests, require skill level >= 100")]
		public bool DoSkinning { get; set; }

		[Setting]
		[Category("Profession")]
		[DisplayName("Mining")]
		[Description("Do Mining quests, require skill level >= 100")]
		public bool DoMining { get; set; }

		[Setting]
		[Category("Profession")]
		[DisplayName("Herbalism")]
		[Description("Do Herbalism quests, require skill level >= 100")]
		public bool DoHerbalism { get; set; }
	}
#endregion SETTINGS

	public static void SetDefaultSettings()
	{
		WorldQuestSettings.Load();

		wManager.wManagerSetting.ClearBlacklistOfCurrentProductSession();
		wManager.wManagerSetting.CurrentSetting.AvoidBlacklistedZonesPathFinder = true;
		wManager.wManagerSetting.CurrentSetting.CloseIfPlayerTeleported = false;
		wManager.wManagerSetting.CurrentSetting.MaxUnitsNear = 99;
		wManager.wManagerSetting.CurrentSetting.CanAttackUnitsAlreadyInFight = true;
		wManager.wManagerSetting.CurrentSetting.AttackElite = true;
		wManager.wManagerSetting.CurrentSetting.AttackBeforeBeingAttacked = false;
		wManager.wManagerSetting.CurrentSetting.SearchRadius = 200;
		wManager.wManagerSetting.CurrentSetting.SkinNinja = false;
		wManager.wManagerSetting.CurrentSetting.SkinMobs = false;
		wManager.wManagerSetting.CurrentSetting.HarvestHerbs = false;
		wManager.wManagerSetting.CurrentSetting.HarvestMinerals = false;
		wManager.wManagerSetting.CurrentSetting.HarvestTimber = false;
		//wManager.wManagerSetting.CurrentSetting.UseFlyingMount = true;
		wManager.wManagerSetting.CurrentSetting.UseGroundMount = true;
		wManager.wManagerSetting.CurrentSetting.UseMount = true;
		wManager.wManagerSetting.CurrentSetting.IgnoreFightGoundMount = true;
		wManager.wManagerSetting.CurrentSetting.SkipInOutDoors = false;
		wManager.wManagerSetting.CurrentSetting.RandomJumping = false;
		//wManager.wManagerSetting.CurrentSetting.FlightMasterTaxiUse = true; //default wrobot
		//wManager.wManagerSetting.CurrentSetting.FlightMasterTaxiUseOnlyIfNear = true; //default false
		wManager.wManagerSetting.CurrentSetting.FlightMasterDiscoverRange = 150f; //default wrobot
		wManager.wManagerSetting.CurrentSetting.FlightMasterTaxiDistance = 1000f; //default wrobot
		wManager.wManagerSetting.CurrentSetting.IgnoreCombatWithPet = true;
		wManager.wManagerSetting.CurrentSetting.HarvestAvoidPlayersRadius = 0; //ignore players near quest items
		wManager.Wow.Helpers.CVar.SetCVar("autoDismount", "1");
		wManager.Wow.Helpers.CVar.SetCVar("autoDismountFlying", "1");
		wManager.Wow.Helpers.CVar.SetCVar("autoLootDefault", "1");
		wManager.Wow.Helpers.CVar.SetCVar("autounshift", "1");
		wManager.Wow.Helpers.Conditions.ForceIgnoreIsAttacked = false;

		//wManager.wManagerSetting.CurrentSetting.FlyAboveGroundHeight

		if (wManager.Wow.Bot.Tasks.FishingTask.IsLaunched)
			wManager.Wow.Bot.Tasks.FishingTask.StopLoopFish();

		if (ObjectManager.Me.PlayerUsingVehicle)
			Usefuls.EjectVehicle();

		Logging.WriteDebug("World Quests: set default settings (enable: ignore mount fight, use taxi, auto dismount for ground & flying mounts, autoloot. disable: harvest, skinning, random jumping)");
		//WorldQuestSettings.ShowCurrent();

		TryBuffSelf();
	}

	public static void TryBuffSelf()
	{
		// http://www.wowhead.com/item=147707/repurposed-fel-focuser
		// http://www.wowhead.com/spell=242551/fel-focus
		if (ItemBuff(147707, 242551))
			return;

		// http://www.wowhead.com/item=118922/oralius-whispering-crystal
		// http://www.wowhead.com/spell=176151/whispers-of-insanity
		if (ItemBuff(118922, 176151))
			return;

		// http://www.wowhead.com/item=129192/inquisitors-menacing-eye
		// http://www.wowhead.com/spell=193456/gaze-of-the-legion
		if (ItemBuff(129192, 193456))
			return;

		// http://www.wowhead.com/item=86569/crystal-of-insanity
		// http://www.wowhead.com/spell=127230/visions-of-insanity
		if (ItemBuff(86569, 127230))
			return;
	}

	static bool ItemBuff(uint itemID, uint itemBuffID)
	{
		if (ItemsManager.HasItemById(itemID)) // http://www.wowhead.com/item=147707/repurposed-fel-focuser
		{
			if (!ObjectManager.Me.HaveBuff(itemBuffID)) // http://www.wowhead.com/spell=242551/fel-focus
			{
				var timeLeft = Lua.LuaDoString<int>("local startTime, duration, enable = GetItemCooldown("+ itemID+"); return startTime + duration - GetTime();");
				if (timeLeft <= 0)
				{
					ItemsManager.UseItem(itemID);
				}
			}
		}
		return false;
	}

	#region SCENARIO
	/// <summary>
	/// copy from Questing.cs
	/// </summary>
	public static class Scenario
	{
		public static bool IsButtonEnterPressed
		{
			get
			{
				var gui = "LFGDungeonReadyDialogEnterDungeonButton";
				if (Lua.LuaDoString<bool>("return " + gui + ":IsVisible()"))
				{

					Lua.LuaDoString(gui + ":Click()");
					return true;
				}
				return false;
			}
		}
		public static int Step
		{
			get
			{
				var lua = @"
local stageName, stageDescription, numCriteria, _, _, _, numSpells, spellInfo, weightedProgress = C_Scenario.GetStepInfo();
return numCriteria;
";
				return Lua.LuaDoString<int>(lua);
			}
		}

		public static int Stage
		{
			get
			{
				var lua = @"
local scenarioName, currentStage, numStages, flags, _, _, _, xp, money, scenarioType = C_Scenario.GetInfo();
return currentStage;
";
				return Lua.LuaDoString<int>(lua);
			}
		}

		public static int MaxStage
		{
			get
			{
				var lua = @"
local scenarioName, currentStage, numStages, flags, _, _, _, xp, money, scenarioType = C_Scenario.GetInfo();
return numStages;
";
				return Lua.LuaDoString<int>(lua);
			}
		}

		static string luaCriteria = @"
local criteriaString, criteriaType, completed, quantity, totalQuantity, flags, assetID, quantityString, criteriaID, duration, elapsed, _, isWeightedProgress = C_Scenario.GetCriteriaInfo({0});
";
		public static bool CriteriaComplete(int criteria)
		{
			var runCode = string.Format(luaCriteria + " return completed;", criteria);
			return Lua.LuaDoString<bool>(runCode);
		}
		public static int CriteriaQuantity(int criteria)
		{
			var runCode = string.Format(luaCriteria + " return quantity;", criteria);
			return Lua.LuaDoString<int>(runCode);
		}
		public static int CriteriaTotalQuantity(int criteria)
		{
			var runCode = string.Format(luaCriteria + " return totalQuantity;", criteria);
			return Lua.LuaDoString<int>(runCode);
		}

	}
	#endregion SCENARIO


}