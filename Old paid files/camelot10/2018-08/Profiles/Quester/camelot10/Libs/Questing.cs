// QUESTING START
#if VISUAL_STUDIO
using robotManager.Helpful;
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
using System;
//using MemoryRobot;
#endif

public class Questing
{
	public delegate bool BoolDelegate();

	static Questing()
	{
		ResetSettings();
		Var.SetVar("Cameleto10Questing", true);
		//		var questing = robotManager.Helpful.Var.Exist("Cameleto10Questing") && robotManager.Helpful.Var.GetVar<bool>("Cameleto10Questing");
		//		var r= RequiredQuest == -1; if (r) RequiredQuest = 0; return r;
	}

	public static void Log(string text)
	{
		Logging.Write("[Questing] " + text);
	}
	public static void LogDebug(string text)
	{
		Logging.WriteDebug("[Questing] " + text);
	}

	#region QUESTS
	public static bool ObjectiveComplete(QuestClass quest, int objectiveNum = 1)
	{
		foreach (var questId in quest.QuestId)
		{
			if (Quest.IsObjectiveComplete(objectiveNum, questId))
				return true;
		}
		return false;
	}

	public static int ObjectiveProgress(QuestClass quest, int obj)
	{
		foreach (var questId in quest.QuestId)
		{
			if (!Quest.HasQuest(questId))
				continue;

			var toRun = string.Format(@"local text, type, unknown, progress, progressTotal = GetQuestObjectiveInfo({0}, {1}, false) if progressTotal > 0 then return progress end return -1", questId, obj);
			var result = Lua.LuaDoString<int>(toRun);
			if (result >= 0)
				return result;
		}
		return -1;
	}
	public static bool CompleteForced(QuestClass quest)
	{
		return CompleteForced(quest.QuestId.ToArray());
	}
	public static bool CompleteForced(params int[] questIds)
	{
		var lua = @"
local quests = {#QUESTS_ID#}
for q, i in pairs(quests) do
	if IsQuestFlaggedCompleted(i) then
		return true
	end
end
return false
";
		lua = lua.Replace("#QUESTS_ID#", string.Join(", ", questIds));
		return Lua.LuaDoString<bool>(lua);
	}
	public static bool Complete(QuestClass quest)
	{
		return Complete(quest.QuestId.ToArray());
	}
	public static bool Complete(params int[] questIds)
	{
		foreach (var questId in questIds)
		{
			//if (Quest.IsObjectiveComplete(1, questId))
			if (Quest.GetQuestCompleted(questId))
				return true;
		}
		return false;
	}
	public static bool CompleteLua(QuestClass quest)
	{
		return CompleteLua(quest.QuestId.ToArray());
	}
	public static bool CompleteLua(params int[] questIds)
	{
		var lua = @"local result = false;";
		foreach (var questId in questIds)
		{
			lua += " result = result or IsQuestFlaggedCompleted(" + questId + ");";
		}
		var r = Lua.LuaDoString<bool>(lua);
		//Logging.Write("@@>> LUA=> " + lua + " ====> " + r);
		return r;
	}
	public static bool Has(QuestClass quest)
	{
		return Has(quest.QuestId.ToArray());
	}
	public static bool Has(params int[] questIds)
	{
		foreach (var questId in questIds)
		{
			if (Quest.HasQuest(questId))
				return true;
		}
		return false;
	}
	public static bool Do(QuestClass quest)
	{
		return Do(quest.QuestId.ToArray());
	}
	public static bool Do(params int[] questIds)
	{
		foreach (var questId in questIds)
		{
			if (Has(questId) && !Complete(questId))
				return true;
		}
		return false;
	}
	public static bool Need(QuestClass quest)
	{
		return Need(quest.QuestId.ToArray());
	}
	public static bool Need(params int[] questIds)
	{
		foreach (var questId in questIds)
		{
			if (!Has(questId) && !Complete(questId))
				return true;
		}
		return false;
	}
	public static bool NotComplete(QuestClass quest) //Breadcrumb
	{
		return NotComplete(quest.QuestId.ToArray());
	}
	public static bool NotComplete(params int[] questIds) //Breadcrumb
	{
		foreach (var questId in questIds)
		{
			if (Complete(questId))
				return false;
		}
		return true;
	}
	public static bool NotCompleteAll(QuestClass quest)
	{
		return NotCompleteAll(quest.QuestId.ToArray());
	}

	public static bool NotCompleteAll(params int[] questIds)
	{
		foreach (var questId in questIds)
		{
			if (!Complete(questId))
				return true;
		}
		return false;
	}
	public static bool Done(QuestClass quest)
	{
		return Done(quest.QuestId.ToArray());
	}
	public static bool Done(params int[] questIds)
	{
		foreach (var questId in questIds)
		{
			if (Has(questId) && Complete(questId))
				return true;
		}
		return false;
	}
	public static bool Recieved(QuestClass quest)
	{
		return Recieved(quest.QuestId.ToArray());
	}
	public static bool Recieved(params int[] questIds)
	{
		foreach (var questId in questIds)
		{
			if (Has(questId) || Complete(questId))
				return true;
		}
		return false;
	}
	public static bool RecievedAll(QuestClass quest)
	{
		return RecievedAll(quest.QuestId.ToArray());
	}
	public static bool RecievedAll(params int[] questIds)
	{
		foreach (var questId in questIds)
		{
			if (!Has(questId) && !Complete(questId))
				return false;
		}
		return true;
	}
	public static bool Abandon(int questID)
	{
		var lua = @"
for questIndex = 1, GetNumQuestLogEntries() do
	local title, level, tag, suggestedGroup, isHeader, isCollapsed, isComplete, questID = GetQuestLogTitle(questIndex)
	if questID ~= 0 and questID == {0} then
		SelectQuestLogEntry(questIndex)
		SetAbandonQuest()
		AbandonQuest()
		return true
	end
end
return false
";
		var runCode = string.Format(lua, questID);
		return Lua.LuaDoString<bool>(runCode);
	}
	public static bool Abandon(QuestClass quest)
	{
		foreach (var questID in quest.QuestId)
		{
			Abandon(questID);
		}
		return true;
	}
	#endregion QUESTS


	#region HELPERS
	public static void Merge(QuestClass quest, string targetClass)
	{
		var grinder = (QuestGrinderClass)quest;
		if (grinder == null)
			return;

		var questsDic = Quest.QuesterCurrentContext.QuestsClasses;
		foreach (var qPair in questsDic)
		{
			var qName = qPair.Key;
			var qClass = (QuestGrinderClass)qPair.Value;
			if (qClass != null && string.Equals(qName, targetClass))
			{
				qClass.HotSpots.AddRange(grinder.HotSpots);
				qClass.EntryTarget.AddRange(grinder.EntryTarget);
				LogDebug("merged quest=" + quest.Name + " into=" + qClass.Name);
			}
		}
	}
	public static void ToVar(QuestClass quest, string varHotspots, string varTargets)
	{
		var grinder = (QuestGrinderClass)quest;
		if (grinder == null)
			return;

		List<Vector3> hotspots = grinder.HotSpots;
		if (Var.Exist(varHotspots) && Var.IsType(varHotspots, hotspots.GetType()))
		{
			hotspots = Var.GetVar<List<Vector3>>(varHotspots);
			hotspots.AddRange(grinder.HotSpots);
		}
		Var.SetVar(varHotspots, hotspots);

		List<int> targets = grinder.EntryTarget;
		if (Var.Exist(varTargets) && Var.IsType(varTargets, hotspots.GetType()))
		{
			targets = Var.GetVar<List<int>>(varTargets);
			targets.AddRange(grinder.EntryTarget);
		}
		Var.SetVar(varTargets, targets);
	}
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

	public static bool UseTransport(int transportID, Vector3 fromTransport, Vector3 fromWait, Vector3 fromInTransport, ContinentId fromContinent, Vector3 toTransport, Vector3 toEnd, ContinentId toContinent, float minDist = 3f)
	{
		wManager.wManagerSetting.CurrentSetting.CloseIfPlayerTeleported = false;

		if (!Conditions.InGameAndConnectedAndProductStartedNotInPause)
			return true;

		if (Usefuls.ContinentId == (int)fromContinent)
		{
			if (!ObjectManager.Me.InTransport)
			{
				if (GoToTask.ToPosition(fromWait))
				{
					Thread.Sleep(Usefuls.Latency * 3);
					var transportGO = ObjectManager.GetWoWGameObjectByEntry(transportID).OrderBy(o => o.GetDistance).FirstOrDefault();
					if (transportGO != null && transportGO.IsValid && transportGO.Position.DistanceTo(fromTransport) < minDist)
					{
						GoToTask.ToPosition(fromInTransport);
						MountTask.DismountMount();
						Questing.DisableForm();
						Thread.Sleep(Usefuls.Latency * 3);
					}
				}
			}
		}
		else if (Usefuls.ContinentId == (int)toContinent)
		{
			if (ObjectManager.Me.InTransport)
			{
				var transportGO = ObjectManager.GetWoWGameObjectByEntry(transportID).OrderBy(o => o.GetDistance).FirstOrDefault();
				if (transportGO != null && transportGO.IsValid && transportGO.Position.DistanceTo(toTransport) < minDist)
				{
					GoToTask.ToPosition(toEnd);
				}
			}
		}
		return true;
	}

	public static bool InteractNPC(Vector3 p, int npcId, float range = 4.5f, int gossip = 1)
	{
		if (GoToTask.ToPosition(p, range))
		{
			//var npc = ObjectManager.GetNearestWoWUnit(ObjectManager.GetWoWUnitByEntry(npcId));
			var npc = ObjectManager.GetObjectWoWUnit().Where(u => u != null && u.IsValid && u.Entry == npcId).FirstOrDefault();
			if (npc != null)
			{
				Interact.InteractGameObject(npc.GetBaseAddress);
				Log("interact " + npc.Name);
				Thread.Sleep(200 + Usefuls.Latency);
				Usefuls.SelectGossipOption(gossip);
				Thread.Sleep(Others.Random(500, 1000));
				return true;
			}
		}
		return false;
	}

	static int _correctNPCIndex = 0;
	public static bool CorrectNPC(NPCQuest npc, params int[] npcIDs)
	{
		var p = npc.Position;
		var npcFound = FindUnit(npc.Id);
		if (npcFound != null && npcFound.IsValid)
		{
			Log("npc with id=" + npcFound.Entry + " found");
			return true;
		}

		_correctNPCIndex += 1;
		if (_correctNPCIndex >= npcIDs.Length)
			_correctNPCIndex = 0;

		npc.Id = npcIDs[_correctNPCIndex];
		return true;
	}
	public static bool CorrectAndFindNPC(NPCQuest npc, params int[] npcIDs)
	{
		var finded = CorrectNPC(npc, npcIDs);
		if (finded)
		{
			var npcFound = FindUnit(npc.Id);
			if (npcFound != null && npcFound.IsValid)
			{
				npc.Position = npcFound.Position;
				//Log("npc with id=" + npcFound.Entry + " found at " + npc.Position.ToStringNewVector() );
				return true;
			}
		}
		return false;
	}
	public static bool FindNPC(NPCQuest npc, List<Vector3> hotspots)
	{
		var p = hotspots[Others.Random(0, hotspots.Count - 1)];
		GoToTask.ToPosition(p, 3.5f, false, (c) =>
		{
			var n = FindUnit(npc.Id);
			if (n != null && n.IsValid)
			{
				Log("located npc=" + n.Entry + " at " + n.Position);
				return false;
			}
			return true;
		});
		var npcFound = FindUnit(npc.Id);
		if (npcFound != null && npcFound.IsValid)
		{
			Log("found npc=" + npcFound.Entry + " at " + npcFound.Position);
			npc.Position = npcFound.Position;
			return true;
		}
		return false;
	}

	public static void PickUp(Vector3 p, int npcId, float range = 4.5f, int gossip = 1)
	{
		if (InteractNPC(p, npcId, range, gossip))
		{
			var gossips = Quest.GetNumGossipAvailableQuests();
			if (gossips > 0)
			{
				Quest.SelectGossipAvailableQuest(1);
				Thread.Sleep(Usefuls.Latency + 1000);
			}
			//* OLD
			Quest.AcceptQuest();
			Log("accepted quest");
			//*/
		}
	}

	public static void TurnIn(Vector3 p, int npcId, float range = 4.5f, int gossip = 1)
	{
		if (InteractNPC(p, npcId, range, gossip))
		{
			Quest.CompleteQuest(gossip);
			Log("turnin quest");
		}
	}

	public static void TurnInQuest()
	{
		Lua.LuaDoString("QuestFrameCompleteButton:Click(); QuestFrameCompleteQuestButton:Click();");
	}

	public static void CloseGossip()
	{
		Lua.LuaDoString("ClearTarget(); GossipFrameCloseButton:Click(); GossipFrameGreetingGoodbyeButton:Click();");
		Thread.Sleep(Others.Random(1000, 2000));
	}
	public static void CloseQuest()
	{
		Lua.LuaDoString("ClearTarget(); QuestFrameGoodbyeButton:Click(); QuestFrameCloseButton:Click();");
		Thread.Sleep(Others.Random(1000, 2000));
	}
	public static bool CloseNPCFrame()
	{
		Lua.LuaDoString("QuestFrameGoodbyeButton:Click(); QuestFrameCloseButton:Click(); GossipFrameCloseButton:Click(); GossipFrameGreetingGoodbyeButton:Click();");
		return true;
	}
	static robotManager.Helpful.Timer _antiAfkTimer = new robotManager.Helpful.Timer(3 * 60 * 1000);
	public static void AntiAFK()
	{
		if (!_antiAfkTimer.IsReady)
			return;

		_antiAfkTimer.Reset();
		wManager.Wow.Bot.States.AntiAfk.Pulse();
	}
	public static void Camp(Vector3 p, List<int> mobs, float healthPercent = 20, float minHealth = 1000000, float radius = 10)
	{
		var mob = FindUnit(mobs);
		if (mob != null && mob.IsAlive && mob.IsAttackable && mob.IsValid)
		{
			if (mob.Health <= minHealth || mob.HealthPercent <= healthPercent)
			{
				Attack(mob);
				return;
			}
		}
		else if (ObjectManager.Me.Position.DistanceTo(p) > radius)
		{
			p = robotManager.Helpful.Math.GetRandomPointInCircle(p, radius - 0.1f);
			GoToTask.ToPosition(p);
			MountTask.DismountMount();
		}
		wManager.Wow.Bot.States.AntiAfk.Pulse();
	}
	public delegate bool GrindDelegate(WoWUnit unit);
	public static void Grind(List<int> mobs, List<Vector3> hotspots = null, GrindDelegate condition = null)
	{
		if (hotspots == null || hotspots.Count < 1)
			hotspots = new List<Vector3>() { ObjectManager.Me.Position };

		if (condition == null)
			condition = u => { return true; };

		var mob = ObjectManager.GetNearestWoWUnit(ObjectManager.GetWoWUnitByEntry(mobs));
		if (mob != null && mob.IsAlive && mob.IsAttackable && mob.IsValid && condition(mob))
		{
			Attack(mob);
		}
		GoToTask.ToPosition(hotspots[Others.Random(0, hotspots.Count - 1)], 3.5f, false, (c) =>
		{
			var aggro = ObjectManager.GetUnitAttackPlayer().FirstOrDefault();
			if (aggro != null && aggro.IsValid && aggro.IsAlive && aggro.IsAttackable)
			{
				Attack(aggro);
				return false;
			}
			var mob2 = FindUnit(mobs);
			return !Attack(mob2);
		});
	}
	public static bool Equip(params int[] itemIds)
	{
		var result = false;
		foreach (var itemId in itemIds)
		{
			if (ItemsManager.IsEquippableItem(itemId) && ItemsManager.HasItemById((uint)itemId) && !Lua.LuaDoString<bool>("return IsEquippedItem(" + itemId + ")"))
			{
				var itemName = ItemsManager.GetNameById(itemId);
				if (!string.IsNullOrEmpty(itemName))
				{
					ItemsManager.EquipItemByName(itemName);
					Log("Equip " + itemName);
					Thread.Sleep(Others.Random(500, 1000));
					result = true;
				}
			}
		}
		return result;
	}

	public static bool Gather(int objectID, float minDist = 5f, bool ignoreBlacklist = true)
	{
		if (ignoreBlacklist)
			wManager.wManagerSetting.ClearBlacklistOfCurrentProductSession();

		var targetObject = ObjectManager.GetObjectWoWGameObject().Where(o => o.IsValid && o.Entry == objectID).OrderBy(o => o.GetDistance).FirstOrDefault();
		if (targetObject != null)
		{
			if (targetObject.GetDistance > minDist)
			{
				GoToTask.ToPosition(targetObject.Position, minDist - 0.1f);
			}

			Interact.InteractGameObject(targetObject.GetBaseAddress);
			Log("Gather " + targetObject.Name);
			Usefuls.WaitIsCasting();
			Thread.Sleep(Others.Random(500, 1000));
			return true;
		}
		return false;
	}

	public static bool Gather(Vector3 position, int objectID, float minDist = 5)
	{
		wManager.wManagerSetting.ClearBlacklistOfCurrentProductSession();
		if (ObjectManager.Me.Position.DistanceTo(position) > minDist)
			GoToTask.ToPosition(position, minDist);

		var targetObject = ObjectManager.GetObjectWoWGameObject().Where(o => o.IsValid && o.Entry == objectID).OrderBy(o => o.GetDistance).FirstOrDefault();
		if (targetObject != null)
		{
			MovementManager.StopMove();
			Thread.Sleep(Usefuls.Latency * 2);
			Interact.InteractGameObject(targetObject.GetBaseAddress);
			Log("Gather " + targetObject.Name);
			Usefuls.WaitIsCasting();
			Thread.Sleep(Others.Random(500, 1000));
			return true;
		}
		return false;
	}

	public static void Use(params int[] itemIds)
	{
		foreach (var itemId in itemIds)
		{
			var itemName = ItemsManager.GetNameById(itemId);
			if (!string.IsNullOrEmpty(itemName))
			{
				ItemsManager.UseItem(itemName);
				Log("Use " + itemName);
				Usefuls.WaitIsCasting();
				Thread.Sleep(Others.Random(500, 1000));
			}
		}
	}

	public static bool Attack(params int[] mobIDs)
	{
		var mob = ObjectManager.GetNearestWoWUnit(ObjectManager.GetWoWUnitByEntry(new List<int>(mobIDs)));
		if (mob != null && mob.IsValid && mob.IsAlive && mob.IsAttackable)
		{
			Interact.InteractGameObject(mob.GetBaseAddress);
			Fight.StartFight(mob.Guid);
			return true;
		}
		return false;
	}

	public static void Repair(Vector3 position, int npcId)
	{
		if (ObjectManager.Me.GetDurabilityPercent > 99)
			return;

		if (GoToTask.ToPositionAndIntecractWithNpc(position, npcId))
		{
			Vendor.RepairAllItems();
			Log("Repair");
		}
	}

	static robotManager.Helpful.Timer CheckGUIDTimer = new robotManager.Helpful.Timer(2 * 60 * 1000);
	static MemoryRobot.Int128 CheckGUIDLast = MemoryRobot.Int128.Zero();
	public static bool CheckGUID(MemoryRobot.Int128 guid, int maxMins = 3, int blacklistMins = 3)
	{
		if (CheckGUIDLast != guid)
		{
			CheckGUIDLast = guid;
			CheckGUIDTimer.Reset(maxMins * 60 * 1000);
		}
		else if (CheckGUIDTimer.IsReady)
		{
			Log("guid=" + guid + " check failed. blacklist for " + blacklistMins + "mins");
			wManager.wManagerSetting.AddBlackList(CheckGUIDLast, blacklistMins * 60 * 1000);
			CheckGUIDLast = MemoryRobot.Int128.Zero();
			return false;
		}
		return true;
	}
	public static void Buy(Vector3 position, int npcId, int itemId, int amount = 1)
	{
		if (GoToTask.ToPositionAndIntecractWithNpc(position, npcId))
		{
			Usefuls.SelectGossipOption(wManager.Wow.Enums.GossipOptionsType.vendor);
			Thread.Sleep(Others.Random(500, 700));
			while (amount-- > 0 && Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause)
			{
				var itemName = ItemsManager.GetNameById(itemId);
				if (!string.IsNullOrEmpty(itemName))
				{
					Vendor.BuyItem(itemName, 1);
					Log("Buy " + itemName);
					Thread.Sleep(1 * 1000);
				}
			}
		}
	}
	public static void AddHarvestBySkill(SkillLine skill, params int[] objectsIDs)
	{
		int skillValue = Skill.GetValue(skill);
		if (skillValue < 1)
			return;

		switch (skill)
		{
			case SkillLine.Herbalism:
				wManager.wManagerSetting.CurrentSetting.HarvestHerbs = true;
				break;
			case SkillLine.Mining:
				wManager.wManagerSetting.CurrentSetting.HarvestMinerals = true;
				break;
			case SkillLine.Skinning:

				break;
			default:
				break;
		}
		AddHarvest(objectsIDs);
	}
	public static void AddHarvest(params int[] objectsIDs)
	{
		wManager.wManagerSetting.CurrentSetting.ListHarvest.AddRange(objectsIDs);
	}
	public static void UpdateMounts(bool isForced = false)
	{
		var settings = wManager.wManagerSetting.CurrentSetting;

		if (isForced || string.IsNullOrEmpty(settings.GroundMountName))
			settings.GroundMountName = SpellManager.GetGroudMountName();

		if (isForced || string.IsNullOrEmpty(settings.FlyingMountName))
			settings.FlyingMountName = SpellManager.GetFlyMountName();

		if (isForced || string.IsNullOrEmpty(settings.AquaticMountName))
			settings.AquaticMountName = SpellManager.GetAquaticMountName();

		Log("Mounts updated. ground: " + settings.GroundMountName + " flying: " + settings.FlyingMountName + " aquatic: " + settings.AquaticMountName);
	}
	public static void StopCutscene(params int[] questIds)
	{
		if (Has(questIds))
			CancelCutscene();
	}
	public static void CancelCutscene(QuestClass quest, int timeout = 500)
	{
		if (quest == null)
			return;

		if (!Has(quest))
			return;

		CancelCutscene(timeout);
	}
	public static void CancelCutscene(int timeout = 500)
	{
		Thread.Sleep(1500 + timeout);
		//Lua.LuaDoString("MovieFrame:StopMovie(); CinematicFrame_CancelCinematic(); StopCinematic();");
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
		Log("Cutscene cancel");
		Thread.Sleep(timeout);
	}

	public static bool UseExtraButton(int num = 1)
	{
		if (ExtraButton())
		{
			Lua.LuaDoString("ExtraActionButton" + num + ":Click()");
			return true;
		}
		return false;
	}

	public static bool ExtraButton()
	{
		var lua = @"return HasExtraActionBar()";
		return Lua.LuaDoString<bool>(lua);
	}

	public static bool ExtraButton(uint spellID)
	{
		return ExtraButton((int)spellID);
	}

	public static bool ExtraButton(int spellId)
	{
		//local slot = extraPage*12 + 11
		var lua = @"
local extraPage = GetExtraBarIndex()
local slot = extraPage * 12 - 11
if HasExtraActionBar() then
	local action, id, subType, spellID = GetActionInfo(slot)
	if id then
		return id
	end
end
return 0";

		var extraSpellId = Lua.LuaDoString<int>(lua);
		if (extraSpellId == 0)
		{
			return false;
		}

		if (spellId != extraSpellId)
		{
			return false;
		}
		return true;
	}

	public static bool Gossip(int num = 1)
	{
		var buttonName = "GossipTitleButton" + num;
		if (IsVisible(buttonName))
		{
			Lua.LuaDoString(buttonName + ":Click()");
			Thread.Sleep(Others.Random(500, 1500));
			return true;
		}
		return false;
	}

	public static bool QuestButton(int num = 1)
	{
		var buttonName = "QuestTitleButton" + num;
		if (IsVisible(buttonName))
		{
			Lua.LuaDoString(buttonName + ":Click()");
			Thread.Sleep(Others.Random(500, 1500));
			return true;
		}
		return false;
	}
	public static bool ChooseQuest(int num = 1, int confirmStatic = 2)
	{
		if (IsVisible("QuestChoiceFrameOption" + num))
		{
			Lua.LuaDoString("QuestChoiceFrameOption" + num + ".OptionButton:Click();");
			Thread.Sleep(Others.Random(1000, 1500));
			if (confirmStatic > 0)
				ClickStaticPopup(confirmStatic);

			return true;
		}
		return false;
	}
	public static bool ClickStaticPopup(int buttonNum, int staticNum = 1)
	{
		var buttonName = "StaticPopup" + staticNum + "Button" + buttonNum;
		if (IsVisible(buttonName))
		{
			Lua.LuaDoString(buttonName + ":Click();");
			Thread.Sleep(Others.Random(1000, 1500));
			return true;
		}
		return false;
	}

	public static bool PossedButton(int num = 1)
	{
		if (Lua.LuaDoString<bool>("return PossessButton" + num + ":IsVisible()"))
		{
			Lua.LuaDoString("PossessButton" + num + ":Click()");
			return true;
		}
		return false;
	}
	public static List<Vector3> PathClampDirected(List<Vector3> path, Vector3 start, Vector3 end = null, float maxDeltaZ = 999999)
	{
		if (end == null || end == Vector3.Zero)
			end = path[path.Count - 1];

		var startDist = float.PositiveInfinity;
		var endDist = float.PositiveInfinity;
		var startP = Vector3.Zero;
		var endP = Vector3.Zero;
		var startI = 0;
		var endI = 0;
		var isReverse = false;
		for (int i = 0; i < path.Count; i++)
		{
			var p = path[i];
			var distS = p.DistanceTo(start);
			var distE = p.DistanceTo(end);
			var deltaZEnd = p.DistanceZ(end);
			if (distS < startDist)
			{
				startP = p;
				startI = i;
				startDist = distS;
			}
			//Logging.Write("@@i=" + i + " distE=" + distE + " endDist=" + endDist + " deltaZEnd=" + deltaZEnd + " maxDeltaZ=" + maxDeltaZ + " ");
			if (distE < endDist && deltaZEnd < maxDeltaZ)
			{
				endP = p;
				endI = i;
				endDist = distE;
				//Logging.Write("###> endI=" + endI + " endP=" + endP + " endDist=" + endDist);
			}
		}
		if (startI > endI)
		{
			var tmp = startI;
			startI = endI;
			endI = tmp;
			isReverse = true;
		}

		var clampedPath = new List<Vector3>();
		for (int i = 0; i < path.Count; i++)
		{
			if (i < startI || i > endI)
				continue;

			clampedPath.Add(path[i]);
		}

		if (isReverse)
			clampedPath.Reverse();

		return clampedPath;
	}
	public static List<Vector3> PathClamp(List<Vector3> path, Vector3 start, Vector3 end = null)
	{
		if (end == null || end == Vector3.Zero)
			end = path[path.Count - 1];

		var startP = path.OrderBy(v => start.DistanceTo(v)).FirstOrDefault();
		var endP = path.OrderBy(v => end.DistanceTo(v)).FirstOrDefault();
		var clampedPath = new List<Vector3>();
		bool includeStart = false;
		bool skipEnd = false;
		for (int i = 0; i < path.Count; i++)
		{
			var p = path[i];
			//skip at start
			if (!includeStart)
			{
				includeStart = p.DistanceTo(startP) < 3.5f;
			}
			else if (!skipEnd)
			{
				skipEnd = p.DistanceTo(endP) < 3.5f;
				clampedPath.Add(p);
			}
		}
		return clampedPath;
	}
	public static void GotoPathFromNear(List<Vector3> path, bool ignoreFight = false, BoolDelegate condition = null, float minDist = 5f)
	{
		var closest = path.OrderBy(v => ObjectManager.Me.Position.DistanceTo(v)).FirstOrDefault();
		if (ObjectManager.Me.Position.DistanceTo(closest) > minDist)
		{
			Logging.WriteDebug("Goto Path from near. path count=" + path.Count + ". closest=" + closest.ToStringNewVector() + " ");
			if (!GoToTask.ToPosition(closest, minDist))
			{
				Logging.WriteDebug("Goto Path from near. failed");
				return;
			}
		}
		PathFromNear(path, ignoreFight, condition, minDist);
	}

	public static void PathFromNear(List<Vector3> path, bool ignoreFight = false, BoolDelegate condition = null, float minDist = 3.5f)
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

		Path(clampedPath, ignoreFight, condition, minDist);
	}
	public static void Escort(Vector3 p, int npcID, float escortRange = 10f, float attackRange = 30, List<int> _mobs = null)
	{
		var npc = ObjectManager.GetNearestWoWUnit(ObjectManager.GetWoWUnitByEntry(npcID));
		if (npc != null && npc.IsValid)
		{
			if (npc.InCombat)
			{
				var mob = ObjectManager.GetNearestWoWUnit(ObjectManager.GetWoWUnitAttackables(attackRange));
				if (mob != null && mob.IsValid && mob.IsAlive && mob.IsAttackable)
				{
					Interact.InteractGameObject(mob.GetBaseAddress);
					Fight.StartFight(mob.Guid);
					return;
				}
			}
			if (_mobs != null && _mobs.Count > 0)
			{
				var aggro = ObjectManager.GetNearestWoWUnit(ObjectManager.GetWoWUnitByEntry(_mobs));
				if (aggro != null && aggro.IsValid && aggro.IsAlive && aggro.IsAttackable && aggro.Position.DistanceTo(npc.Position) < attackRange)
				{
					Interact.InteractGameObject(aggro.GetBaseAddress);
					Fight.StartFight(aggro.Guid);
					return;
				}
			}
			GoToTask.ToPosition(npc.Position, escortRange);
			return;
		}

		var npc0 = ObjectManager.GetNearestWoWUnit(ObjectManager.GetWoWUnitByEntry(npcID));
		if (npc0 != null && npc0.IsValid)
		{
			GoToTask.ToPositionAndIntecractWithNpc(p, 90383, 1);
			return;
		}
		GoToTask.ToPosition(p);
	}
	public static void Path(List<Vector3> path, bool ignoreFight = false, BoolDelegate condition = null, float minDist = 3.5f)
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
			Thread.Sleep(300);
		}
		while (MovementManager.InMovement && Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause && !Conditions.IsAttackedAndCannotIgnore && condition());
		MovementManager.StopMove();
		if (ignoreFight)
			Conditions.ForceIgnoreIsAttacked = oldIgnore;
	}

	public static void PathAsPet(List<Vector3> path, float precision = 3.5f, BoolDelegate condition = null)
	{
		if (condition == null)
			condition = () => true;

		var pet = ObjectManager.Pet;
		Log("move pet path=" + path.Count);
		foreach (var p in path)
		{
			while (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause && condition() && pet != null && pet.IsValid && pet.IsAlive && pet.Position.DistanceTo(p) > precision)
			{
				ClickMove(p);
				//Log(">> move to " + p.ToStringNewVector() + " 1=" + Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause + " 2=" + condition() + " 3=" + (pet != null) + " 4=" + pet.IsValid + " 5=" + pet.IsAlive + " 6=" + (pet.Position.DistanceTo(p) > precision));
				Thread.Sleep(1000);
			}
		}
		Log("move pet end " + path.Count);
	}

	public static void PathClickToMove(List<Vector3> path, float precision = 3.5f, bool ignoreFight = false, BoolDelegate runCondition = null)
	{
		Log("move click path=" + path.Count);
		var useLuaToMove = wManager.wManagerSetting.CurrentSetting.UseLuaToMove;
		wManager.wManagerSetting.CurrentSetting.UseLuaToMove = false;
		var oldIgnore = Conditions.ForceIgnoreIsAttacked;
		if (ignoreFight)
			Conditions.ForceIgnoreIsAttacked = true;

		if (runCondition == null)
			runCondition = () => true;

		foreach (var p in path)
		{
			while (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause && ObjectManager.Me.Position.DistanceTo(p) > precision && runCondition())
			{
				ClickMove(p);
				Thread.Sleep(300);
			}
		}
		if (ignoreFight)
			Conditions.ForceIgnoreIsAttacked = oldIgnore;

		wManager.wManagerSetting.CurrentSetting.UseLuaToMove = useLuaToMove;
		Log("move click end " + path.Count);
	}
	public static float PathDistance(List<Vector3> path)
	{
		if (path == null || path.Count < 2)
			return float.MaxValue;

		var pathDist = 0f;
		for (int j = 1; j < path.Count; j++)
		{
			pathDist += path[j].DistanceTo(path[j - 1]);
		}
		return pathDist;
	}
	public static float PathDistanceSqr(List<Vector3> path)
	{
		if (path == null || path.Count < 2)
			return float.MaxValue;

		var pathDistSqr = 0f;
		for (int j = 1; j < path.Count; j++)
		{
			pathDistSqr += (path[j] - path[j - 1]).MagnitudeSqr();
		}
		return pathDistSqr;
	}
	public static void ClickMove(Vector3 p, float precision = 3.5f)
	{
		ClickToMove.CGPlayer_C__ClickToMove(p.X, p.Y, p.Z, MemoryRobot.Int128.Zero(), (int)wManager.Wow.Enums.ClickToMoveType.Move, precision);
	}

	public static void ClickMove(WoWUnit unit, float precistion = 0.5f)
	{
		ClickToMove.CGPlayer_C__ClickToMove(unit.Position.X, unit.Position.Y, unit.Position.Z, unit.Guid, (int)wManager.Wow.Enums.ClickToMoveType.Move, precistion);
	}
	public static Vector3 GetForwardPosition(Vector3 from, float rotation, float distance)
	{
		if (from != null && from != Vector3.Empty)
		{
			float changedRotation = 0 - robotManager.Helpful.Math.DegreeToRadian(robotManager.Helpful.Math.RadianToDegree(rotation) - 90);
			return new Vector3((System.Math.Sin(changedRotation) * distance) + from.X, (System.Math.Cos(changedRotation) * distance) + from.Y, from.Z);
		}
		return new Vector3(0, 0, 0);
	}
	public static Vector3 GetBackPosition(Vector3 from, float rotation, float distance)
	{
		if (from != null && from != Vector3.Empty)
		{
			float changedRotation = 0 - robotManager.Helpful.Math.DegreeToRadian(robotManager.Helpful.Math.RadianToDegree(rotation) + 90);
			return new Vector3((System.Math.Sin(changedRotation) * distance) + from.X, (System.Math.Cos(changedRotation) * distance) + from.Y, from.Z);
		}
		return new Vector3(0, 0, 0);
	}
	public static void Wait(QuestClass quest, float secs, float secsMax = -1)
	{
		if (quest != null && Complete(quest))
			return;

		Wait(secs, secsMax);
	}
	public static void Wait(float secs, float secsMax = -1)
	{
		if (secsMax <= secs)
			secsMax = secs + 1;

		Thread.Sleep(Others.Random((int)secs * 1000, (int)secsMax * 1000));
	}
	public static void WaitIsCasting(WoWUnit unit)
	{
		//copy from Usefuls.WaitIsCasting
		var timer = new robotManager.Helpful.Timer((double)(Usefuls.Latency + 200));
		while (!timer.IsReady && !unit.IsCast && Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause)
			Thread.Sleep(5);
		while (unit.IsCast && Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause)
			Thread.Sleep(30);
	}

	public static bool CompleteAutoCompletableQuest(QuestClass quest)
	{
		if (quest == null)
			return false;

		foreach (var questID in quest.QuestId)
		{
			CompleteAutoCompletableQuest(questID);
		}
		return true;
	}

	public static bool CompleteAutoCompletableQuest(int questId)
	{
		if (!Quest.HasQuest(questId))
			return false;

		var lua = @"
for questIndex = 1, GetNumQuestLogEntries() do
	local title, level, tag, suggestedGroup, isHeader, isCollapsed, isComplete, questID = GetQuestLogTitle(questIndex)
	if questID ~= 0 and questID == {0} then
		ShowQuestComplete(questIndex);
		return true
	end
end
return false
";
		var runCode = string.Format(lua, questId);
		if (Lua.LuaDoString<bool>(runCode))
		{
			CompleteQuestFrame();
		}
		return true;
	}

	public static bool IsVisible(string gui)
	{
		return Lua.LuaDoString<bool>("return " + gui + " and " + gui + ":IsVisible()");
	}

	public static bool CompleteQuestFrame()
	{
		Thread.Sleep(Others.Random(500, 1000));
		Lua.LuaDoString("CompleteQuest()");
		Thread.Sleep(Others.Random(500, 1000));
		Lua.LuaDoString("GetQuestReward()");
		return true;
	}

	public static bool NoCooldownSpell(int spellId)
	{
		return NoCooldownSpell((uint)spellId);
	}

	public static bool NoCooldownSpell(uint spellId)
	{
		var lua = @"start, duration, enabled = GetSpellCooldown('{0}'); return start;";
		var runCode = string.Format(lua, spellId);
		var cd = Lua.LuaDoString<int>(runCode);
		return cd == 0;
	}

	public static bool VehicleButton(int num)
	{
		Lua.RunMacroText("/click OverrideActionBarButton" + num);
		Thread.Sleep(Usefuls.Latency * 2);
		return true;
	}

	//DONT USE THIS
	static bool _PopupTurninOrPickup()
	{
		var lua = @"
for i = 1, GetNumAutoQuestPopUps() do
	local id, type = GetAutoQuestPopUp(i)
	local title = GetQuestLogTitle(GetQuestLogIndexByID(id))
	if title and title ~= '' then
		local block = AUTO_QUEST_POPUP_TRACKER_MODULE:GetBlock(id)
		AutoQuestPopUpTracker_OnMouseDown(block)
		return true
		if type == 'COMPLETE' then
			--
		elseif type == 'OFFER' then
			--
		end
	end
end
return false
";
		int antiloop = 10;
		while (antiloop-- > 0 && Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause && Lua.LuaDoString<bool>(lua))
		{
			Lua.LuaDoString("AcceptQuest()");
			Lua.LuaDoString("CompleteQuest()");
			Log("accept/complete popup quest");
			Thread.Sleep(Others.Random(300, 500));
		}
		return true;
	}

	public static void RemoveItem(uint itemID)
	{
		var lua = @"
for i=0,4 do
	for j=1,36 do
		if GetContainerItemID(i,j) == {0} then
			PickupContainerItem(i,j)
			DeleteCursorItem();
			return
		end
	end
end
";
		var runCode = string.Format(lua, itemID);
		Lua.LuaDoString(runCode);
	}
	public static WoWUnit FindUnit(params int[] npcIDs)
	{
		return FindUnit(new List<int>(npcIDs));
	}
	public static WoWUnit FindUnit(List<int> npcIDs, bool includeDead = false)
	{
		return ObjectManager.GetNearestWoWUnit(ObjectManager.GetWoWUnitByEntry(ObjectManager.GetObjectWoWUnit(), npcIDs, includeDead));
	}
	public static WoWUnit FindUnitIncludeDead(List<int> npcIDs)
	{
		return FindUnit(npcIDs, true);
	}
	public static WoWUnit FindUnitIncludeDead(params int[] npcIDs)
	{
		return FindUnit(new List<int>(npcIDs), true);
	}
	public static WoWGameObject FindObject(params int[] objIDs)
	{
		return FindObject(new List<int>(objIDs));
	}
	public static WoWGameObject FindObject(List<int> objIDs)
	{
		return ObjectManager.GetNearestWoWGameObject(ObjectManager.GetWoWGameObjectByEntry(objIDs));
	}
	public static bool Attack(int entry)
	{
		return Attack(FindUnit(entry));
	}
	public static bool Attack(WoWUnit unit)
	{
		if (unit == null || !unit.IsValid || unit.IsDead || !unit.IsAttackable)
			return false;

		Interact.InteractGameObject(unit.GetBaseAddress);
		Fight.StartFight(unit.Guid);
		return true;
	}
	public static bool Camp(List<Vector3> hotspots, List<int> ids, bool includeDead = false)
	{
		var p = hotspots[Others.Random(0, hotspots.Count - 1)];
		GoToTask.ToPosition(p, 5, false, (c) =>
		{
			var go2 = FindObject(ids);
			if (go2 != null && go2.IsValid)
			{
				return false;
			}
			var unit2 = FindUnit(ids, includeDead);
			if (unit2 != null && unit2.IsValid)
			{
				return false;
			}
			return true;
		});
		return true;
	}
	public static bool GatherInteractKill(List<Vector3> hotspots, List<int> ids, bool includeDead = false, int gossip = 1, Reaction minReaction = Reaction.Neutral, int blacklistTime = 5)
	{
		var go = FindObject(ids);
		if (go != null && go.IsValid)
		{
			Log("Gather/Interact/Kill gather=" + go.Name);
			GoToTask.ToPositionAndIntecractWithGameObject(go.Position, go.Entry, gossip);
			return true;
		}
		var unit = FindUnit(ids, includeDead);
		if (unit != null && unit.IsValid)
		{
			if (unit.IsAlive && unit.IsAttackable && unit.Reaction <= minReaction)
			{
				Log("Gather/Interact/Kill kill=" + unit.Name);
				Attack(unit);
				return true;
			}
			Log("Gather/Interact/Kill interact=" + unit.Name);
			if (GoToTask.ToPositionAndIntecractWithNpc(unit.Position, unit.Entry, gossip, false, null, includeDead))
			{
				wManager.wManagerSetting.AddBlackList(unit.Guid, blacklistTime * 1000, true);
			}
			return true;
		}
		return Camp(hotspots, ids);
	}
	public static bool GatherInteractKill(List<Vector3> hotspots, params int[] IDs)
	{
		var listIDs = new List<int>(IDs);
		var gameObject = ObjectManager.GetNearestWoWGameObject(ObjectManager.GetWoWGameObjectByEntry(listIDs));
		if (gameObject != null && gameObject.IsValid)
		{
			Log("Gather/Interact/Kill gather=" + gameObject.Name);
			GoToTask.ToPositionAndIntecractWithGameObject(gameObject.Position, gameObject.Entry, 1);
			return true;
		}
		var unit = ObjectManager.GetNearestWoWUnit(ObjectManager.GetWoWUnitByEntry(listIDs));
		if (unit != null && unit.IsValid && unit.IsAlive)
		{
			if (unit.Reaction <= Reaction.Neutral && unit.IsAttackable)
			{
				Log("Gather/Interact/Kill kill=" + unit.Name);
				Interact.InteractGameObject(unit.GetBaseAddress, false, false, true);
				Fight.StartFight(unit.Guid);
				return true;
			}
			else
			{
				//Log("Gather/Interact/Kill interact=" + unit.Name);
				GoToTask.ToPositionAndIntecractWithNpc(unit.Position, unit.Entry, 1);
				return true;
			}
		}
		var p = hotspots[Others.Random(0, hotspots.Count - 1)];
		//Log("Gather/Interact/Kill hotspot=" + p);
		GoToTask.ToPosition(p, 5, false, (c) =>
		{
			var go = ObjectManager.GetNearestWoWGameObject(ObjectManager.GetWoWGameObjectByEntry(listIDs));
			if (go != null && go.IsValid)
			{
				Log("Gather/Interact/Kill found=" + go.Name);
				return false;
			}
			var u = ObjectManager.GetNearestWoWUnit(ObjectManager.GetWoWUnitByEntry(listIDs));
			if (u != null && unit.IsValid && unit.IsAlive)
			{
				Log("Gather/Interact/Kill founded=" + u.Name);
				return false;
			}
			return true;
		});
		return false;
	}
	public static int ZoneId
	{
		get
		{
			return Lua.LuaDoString<int>("local mapID, isContinent = GetCurrentMapAreaID(); return mapID;");
		}
	}

	/// <summary>
	/// 0 = friendRep, 1 = friendMaxRep, 2 = friendName, 3 = friendText,
	/// 4 = friendThreshold, 5 = nextFriendThreshold
	/// </summary>
	/// <param name="factionID"></param>
	/// <returns></returns>
	public static List<string> GetFriendReputation(int factionID)
	{
		var lua = @"
local friendID, friendRep, friendMaxRep, friendName, friendText, friendTexture, friendTextLevel, friendThreshold, nextFriendThreshold = GetFriendshipReputation({0});
local result = friendRep .. '{1}' .. friendMaxRep .. '{1}' .. friendName .. '{1}' .. friendText .. '{1}' .. friendThreshold;
if nextFriendThreshold then
	result =  result .. '{1}' .. nextFriendThreshold
end
return result;
";
		var runCode = string.Format(lua, factionID, Lua.ListSeparator);
		var result = Lua.LuaDoString<List<string>>(runCode);
		return result;
	}

	/// <summary>
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
	public static int Currency(int currencyID)
	{
		var lua = @"local name, amount, texturePath, earnedThisWeek, weeklyMax, totalMax, isDiscovered, quality = GetCurrencyInfo('currency: {0}'); return amount;";
		var runCode = string.Format(lua, currencyID);
		return Lua.LuaDoString<int>(runCode);
	}
	public static int CurrencyMax(int currencyID)
	{
		var lua = @"local name, amount, texturePath, earnedThisWeek, weeklyMax, totalMax, isDiscovered, quality = GetCurrencyInfo('currency: {0}'); return totalMax;";
		var runCode = string.Format(lua, currencyID);
		return Lua.LuaDoString<int>(runCode);
	}
	public static bool SafeUseItem(params uint[] itemIDs)
	{
		var me = ObjectManager.Me;
		if (me == null || !me.IsValid || me.IsDead || me.InCombat || me.GetMove || me.IsFlying || me.IsCast)
			return false;

		foreach (var itemID in itemIDs)
		{
			if (ItemsManager.HasItemById(itemID))
			{
				var itemName = ItemsManager.GetNameById(itemID);
				Log("safe use item=" + itemName);
				ItemsManager.UseItem(itemName);
				Usefuls.WaitIsCasting();
				return true;
			}
		}
		return false;
	}
	public static bool UseItem(uint itemID)
	{
		var lua = @"
for bag = 0,4 do
	for slot = 1,GetContainerNumSlots(bag) do
		local id = GetContainerItemID(bag, slot)
		if (id and id == {0}) then
			if (GetContainerItemCooldown(bag,slot)==0) then
				UseContainerItem(bag,slot)
				return true
			end
		end
	end
end
return false
";
		return Lua.LuaDoString<bool>(string.Format(lua, itemID));
	}
	public static bool UseItem(int itemID)
	{
		return UseItem((uint)itemID);
	}
	public static int ItemCooldown(int itemID)
	{
		return ItemCooldown((uint)itemID);
	}
	public static int ItemCooldown(uint itemID)
	{
		return Lua.LuaDoString<int>("local startTime, duration, enable = GetItemCooldown(" + itemID + "); return startTime + duration - GetTime();");
	}
	public static bool ItemOnCooldown(uint itemID)
	{
		return ItemCooldown(itemID) > 0;
	}
	public static bool ItemOnCooldown(int itemID)
	{
		return ItemOnCooldown((uint)itemID);
	}
	/// <summary>
	/// WARNING! PRODUCT RESTART, USE WITH CAUTION
	/// </summary>
	/// <param name="file"></param>
	public static void LoadProfile(string file)
	{
		try
		{
			Log("load profile=" + file);
			Quester.Bot.QuesterSetting.CurrentSetting.ProfileName = file;
			System.Threading.Tasks.Task.Delay(10).ContinueWith(t => robotManager.Products.Products.ProductRestart());
			//robotManager.Products.Products.ProductRestart();
		}
		catch (System.Exception e)
		{
			Log("ERROR LOADING PROFILE: " + e.Message + " >> " + e.StackTrace);
		}
	}
	public static void DisableForm()
	{
		Lua.RunMacroText("/cancelform");
	}
	public static void DiableFlying()
	{
		wManager.wManagerSetting.CurrentSetting.UseFlyingMount = false;
		MountTask.DismountMount();
	}
	public static bool Mount
	{
		get
		{
			return wManager.wManagerSetting.CurrentSetting.UseMount;
		}
		set
		{
			wManager.wManagerSetting.CurrentSetting.UseMount = value;
			if (!value)
			{
				DisableForm();
				MountTask.DismountMount();
			}
		}
	}
	public static void DisableMount()
	{
		wManager.wManagerSetting.CurrentSetting.UseMount = false;
		MountTask.DismountMount();
	}

	static bool _fightClassDisabled = false;
	public static bool FightclassDisabled
	{
		get
		{
			return _fightClassDisabled;
		}
		set
		{
			if (value == _fightClassDisabled)
				return;

			_fightClassDisabled = value;
			Conditions.ForceIgnoreIsAttacked = _fightClassDisabled;
			Log("fightclass and combat disabled=" + _fightClassDisabled + " ");
			if (_fightClassDisabled)
				wManager.Wow.Helpers.CustomClass.DisposeCustomClass();
			else
				wManager.Wow.Helpers.CustomClass.LoadCustomClass();
			/*
			wManager.Events.FightEvents.OnFightStart -= _OnFightStart;
			wManager.Events.FightEvents.OnFightLoop -= _OnFightLoop;
			if (_fightClassDisabled)
			{
				wManager.Events.FightEvents.OnFightStart += _OnFightStart;
				wManager.Events.FightEvents.OnFightLoop += _OnFightLoop;
			}
			//*/
		}
	}
	static void _OnFightLoop(WoWUnit unit, System.ComponentModel.CancelEventArgs cancelable)
	{
		do
		{
			Thread.Sleep(100);
		}
		while (_fightClassDisabled);
	}
	static void _OnFightStart(WoWUnit unit, System.ComponentModel.CancelEventArgs cancelable)
	{
		do
		{
			Thread.Sleep(100);
		}
		while (_fightClassDisabled);
	}
	static bool _movementDisabled = false;
	public static bool MovementDisabled
	{
		get
		{
			return _movementDisabled;
		}
		set
		{
			if (value == _movementDisabled)
				return;

			_movementDisabled = value;
			Log("movement disabled = " + _movementDisabled);
			if (_movementDisabled)
			{
				MovementManager.StopMove();
				MovementManager.StopMoveNewThread();
				MovementManager.StopMoveTo();
				MovementManager.StopMoveToNewThread();
				LongMove.StopLongMove();
			}
			wManager.Events.MovementEvents.OnMovementPulse -= _OnMovementPulse;
			wManager.Events.MovementEvents.OnMoveToPulse -= _OnMoveToPulse;
			if (_movementDisabled)
			{
				wManager.Events.MovementEvents.OnMovementPulse += _OnMovementPulse;
				wManager.Events.MovementEvents.OnMoveToPulse += _OnMoveToPulse;
			}
		}
	}
	static void _OnMoveToPulse(Vector3 point, System.ComponentModel.CancelEventArgs cancelable)
	{
		cancelable.Cancel = true;
	}
	static void _OnMovementPulse(List<Vector3> points, System.ComponentModel.CancelEventArgs cancelable)
	{
		cancelable.Cancel = true;
	}
	public static bool DisablePet
	{
		set
		{
			Log("disable pet=" + value);
			robotManager.Events.FiniteStateMachineEvents.OnBeforeCheckIfNeedToRunState -= _OnBeforeCheckIfNeedToRunStateDisablePet;
			if (value)
			{
				robotManager.Events.FiniteStateMachineEvents.OnBeforeCheckIfNeedToRunState += _OnBeforeCheckIfNeedToRunStateDisablePet;
			}
		}
	}
	static List<string> _summonPetNames = new List<string>()
	{
		//frost mage
		//warlock
		//hunter
		//death knight?
	};
	static void _OnBeforeCheckIfNeedToRunStateDisablePet(robotManager.FiniteStateMachine.Engine engine, robotManager.FiniteStateMachine.State state, System.ComponentModel.CancelEventArgs cancelable)
	{
		try
		{
			var s = state as wManager.Wow.Bot.States.SpellState;
			if (s != null)
			{
				if (s.Spell.Name.StartsWith("Call Pet"))
				{
					LogDebug("disable pet spell=" + s.Spell.Name);
					cancelable.Cancel = true;
				}
			}
		}
		catch { }
	}
	static bool _dismountOnInteract = false;
	public static bool DismountOnInteract
	{
		get
		{
			return _dismountOnInteract;
		}
		set
		{
			if (_dismountOnInteract == value)
				return;
			wManager.Events.InteractEvents.OnInteractPulse -= _OnInteractPulse;
			_dismountOnInteract = value;
			Log("dismount on interact = " + _dismountOnInteract);
			if (_dismountOnInteract)
				wManager.Events.InteractEvents.OnInteractPulse += _OnInteractPulse;
		}
	}
	static void _OnInteractPulse(MemoryRobot.Int128 target, System.ComponentModel.CancelEventArgs cancelable)
	{
		try
		{
			if (ObjectManager.Me.IsMounted)
			{
				MountTask.DismountMount();
				var timer = new robotManager.Helpful.Timer(5 * 1000);
				while (!timer.IsReady && Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause && ObjectManager.Me.IsFalling)
				{
					Thread.Sleep(50);
				}
				Thread.Sleep(Usefuls.Latency * 2);
			}
		}
		catch
		{
		}
	}
	static Thread _avoidSpellThread = null;
	public static void AvoidSpellAOE(uint spellID, float strafeSecs = 1f, int questID = 0, float checkSecs = 0.5f)
	{
		AvoidSpellStop();
		int strafeTimeout = (int)(strafeSecs * 1000);
		int waitTimeout = (int)(checkSecs * 1000);
		_avoidSpellThread = new Thread(() =>
		{
			while (robotManager.Products.Products.IsStarted)
			{
				Thread.Sleep(waitTimeout);
				if (!Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause)
					continue;

				if (questID > 0 && !Quest.HasQuest(questID))
				{
					Log("cancel avoid spell=" + spellID);
					break;
				}
				var key = Others.Random(0, 1) == 1 ? wManager.Wow.Enums.Keybindings.STRAFELEFT : wManager.Wow.Enums.Keybindings.STRAFERIGHT;
				if (ObjectManager.Me.HaveBuff(spellID))
				{
					wManager.Wow.Helpers.Keybindings.PressKeybindings(key, strafeTimeout);
				}
			}
		});
		_avoidSpellThread.Start();
		Log("start avoid AOE spell=" + SpellListManager.SpellNameInGameById(spellID));
	}
	public static void AvoidSpellTarget(uint spellID, float strafeSecs = 1f, int questID = 0, float checkSecs = 0.5f)
	{
		AvoidSpellStop();
		int strafeTimeout = (int)(strafeSecs * 1000);
		int waitTimeout = (int)(checkSecs * 1000);
		_avoidSpellThread = new Thread(() =>
		{
			while (robotManager.Products.Products.IsStarted)
			{
				Thread.Sleep(waitTimeout);
				if (!Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause)
					continue;

				if (questID > 0 && !Quest.HasQuest(questID))
				{
					Log("cancel avoid spell=" + spellID);
					break;
				}
				if (ObjectManager.Target == null || !ObjectManager.Target.IsValid || ObjectManager.Target.CastingSpellId != spellID)
					continue;

				var key = Others.Random(0, 1) == 1 ? wManager.Wow.Enums.Keybindings.STRAFELEFT : wManager.Wow.Enums.Keybindings.STRAFERIGHT;
				wManager.Wow.Helpers.Keybindings.PressKeybindings(key, strafeTimeout);
			}
		});
		_avoidSpellThread.Start();
		Log("start avoid cast spell=" + SpellListManager.SpellNameInGameById(spellID));
	}
	public static void AvoidSpellStop()
	{
		if (_avoidSpellThread == null)
			return;

		_avoidSpellThread.Abort();
		_avoidSpellThread = null;
		Log("stop avoid spells");
	}
	public static void WaitCurrentAreaIDChange(int mins = 1)
	{
		WaitAreaIDChange(Usefuls.AreaId, mins);
	}
	public static void WaitAreaIDChange(int prevAreaID, int minutes = 3)
	{
		var timer = new robotManager.Helpful.Timer(minutes * 60 * 1000);
		while (robotManager.Products.Products.IsStarted)
		{
			if (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause)
			{
				if (timer.IsReady)
					break;

				if (Usefuls.AreaId != prevAreaID)
				{
					Log("area id changed succsefully from " + prevAreaID + " to " + Usefuls.AreaId);
					break;
				}
			}
			Thread.Sleep(1000);
		}
	}

	public static Vector3 GetPositionFrom(Vector3 pos, Vector3 from, float range)
	{
		var delta = pos - from;
		var deltaMag = delta.Magnitude();
		var deltaNormalized = delta / deltaMag;
		var deltaNeed = deltaNormalized * range;
		return pos + deltaNeed;
	}

	public static void GatherOrInteract(List<Vector3> hotspots, List<int> IDs)
	{
		var go = ObjectManager.GetNearestWoWGameObject(ObjectManager.GetWoWGameObjectByEntry(IDs));
		if (go != null && go.IsValid)
		{
			Gather(go.Position, go.Entry);
			return;
		}
		var npc = ObjectManager.GetNearestWoWUnit(ObjectManager.GetWoWUnitByEntry(IDs));
		if (npc != null && npc.IsValid)
		{
			if (npc.IsAttackable)
			{
				Attack(npc);
			}
			else
			{
				InteractNPC(npc.Position, npc.Entry);
			}
			return;
		}
		var p = hotspots[Others.Random(0, hotspots.Count - 1)];
		GoToTask.ToPosition(p, 3.5f, false, (c) =>
		{
			var go2 = ObjectManager.GetNearestWoWGameObject(ObjectManager.GetWoWGameObjectByEntry(IDs));
			if (go2 != null && go2.IsValid)
				return false;

			var npc2 = ObjectManager.GetNearestWoWUnit(ObjectManager.GetWoWUnitByEntry(IDs));
			if (npc2 != null && npc2.IsValid)
				return false;

			return true;
		});
	}

	public static bool ChangeQuestNPCPosition(int npcID, Vector3 pos)
	{
		var npc = Quest.QuesterCurrentContext.NPCList.FirstOrDefault(n => n.Id == npcID);
		if (npc == null)
			return false;

		npc.Position = pos;
		Log("npc " + npc.Name + " changed to " + npc.Position);
		return true;
	}

	#endregion HELPERS


	#region SETTINGS
	public static void ResetSettings()
	{
		wManager.wManagerSetting.ClearBlacklistOfCurrentProductSession();
		wManager.wManagerSetting.CurrentSetting.LootMobs = true;
		wManager.wManagerSetting.CurrentSetting.LootChests = true;
		wManager.wManagerSetting.CurrentSetting.UseMail = false;
		wManager.wManagerSetting.CurrentSetting.SellGray = true;
		wManager.wManagerSetting.CurrentSetting.SellGreen = false;
		wManager.wManagerSetting.CurrentSetting.SellWhite = false;
		wManager.wManagerSetting.CurrentSetting.SellBlue = false;
		wManager.wManagerSetting.CurrentSetting.Repair = true;
		wManager.wManagerSetting.CurrentSetting.Selling = true;
		wManager.wManagerSetting.CurrentSetting.UseMammoth = true;
		wManager.wManagerSetting.CurrentSetting.AvoidBlacklistedZonesPathFinder = true;
		wManager.wManagerSetting.CurrentSetting.CloseIfPlayerTeleported = false;
		wManager.wManagerSetting.CurrentSetting.MaxUnitsNear = 99;
		wManager.wManagerSetting.CurrentSetting.CanAttackUnitsAlreadyInFight = true;
		wManager.wManagerSetting.CurrentSetting.AttackElite = true;
		wManager.wManagerSetting.CurrentSetting.AttackBeforeBeingAttacked = false;
		wManager.wManagerSetting.CurrentSetting.SearchRadius = 100;
		wManager.wManagerSetting.CurrentSetting.SkinNinja = false;
		wManager.wManagerSetting.CurrentSetting.SkinMobs = false;
		wManager.wManagerSetting.CurrentSetting.HarvestHerbs = false;
		wManager.wManagerSetting.CurrentSetting.HarvestMinerals = false;
		wManager.wManagerSetting.CurrentSetting.HarvestTimber = false;
		wManager.wManagerSetting.CurrentSetting.UseFlyingMount = true;
		wManager.wManagerSetting.CurrentSetting.UseGroundMount = true;
		wManager.wManagerSetting.CurrentSetting.UseMount = true;
		wManager.wManagerSetting.CurrentSetting.AquaticMountName = ""; //aquatic mount cause problems with moving in waters
		wManager.wManagerSetting.CurrentSetting.IgnoreFightGoundMount = false;
		wManager.wManagerSetting.CurrentSetting.SkipInOutDoors = false;
		wManager.wManagerSetting.CurrentSetting.RandomJumping = true;
		wManager.wManagerSetting.CurrentSetting.FlightMasterTaxiUse = true; //default wrobot
		wManager.wManagerSetting.CurrentSetting.FlightMasterTaxiUseOnlyIfNear = false; //default wrobot
		wManager.wManagerSetting.CurrentSetting.FlightMasterDiscoverRange = 150f; //default wrobot
		wManager.wManagerSetting.CurrentSetting.FlightMasterTaxiDistance = 1000f; //default wrobot
		wManager.wManagerSetting.CurrentSetting.HarvestAvoidPlayersRadius = 0; //ignore players near quest items
		wManager.wManagerSetting.CurrentSetting.SecurityPauseBotIfNerbyPlayer = false;
		wManager.wManagerSetting.CurrentSetting.SecurityPauseBotIfNerbyPlayerRadius = 0;
		wManager.wManagerSetting.CurrentSetting.BlackListIfNotCompletePath = true;
		wManager.wManagerSetting.CurrentSetting.AvoidWallWithRays = true;
		wManager.wManagerSetting.CurrentSetting.IgnoreCombatWithPet = true;
		wManager.wManagerSetting.CurrentSetting.AssignTalents = true;
		wManager.wManagerSetting.CurrentSetting.DetectEvadingMob = true; //new testing
		wManager.Wow.Helpers.CVar.SetCVar("autoDismount", "1");
		wManager.Wow.Helpers.CVar.SetCVar("autoDismountFlying", "1");
		wManager.Wow.Helpers.CVar.SetCVar("autoLootDefault", "1");
		wManager.Wow.Helpers.CVar.SetCVar("autounshift", "1");
		wManager.Wow.Helpers.Lua.LuaDoString("SetAutoDeclineGuildInvites(true)");
		wManager.Wow.Helpers.Conditions.ForceIgnoreIsAttacked = false;
		wManager.wManagerSetting.CurrentSetting.FoodPercent = 35;
		wManager.wManagerSetting.CurrentSetting.DrinkPercent = 35;
		wManager.wManagerSetting.CurrentSetting.NpcMailboxSearchRadius = 1000;
		//wManager.wManagerSetting.CurrentSetting.CalcuCombatRange = true;

		Quest.QuesterCurrentContext.SkipPickUpQuestAfterXSecondes = 60 * 60;
		Quester.Bot.QuesterSetting.CurrentSetting.SkipPickUpQuestAfterXSecondes = 60 * 60;

		if (wManager.Wow.Bot.Tasks.FishingTask.IsLaunched)
			wManager.Wow.Bot.Tasks.FishingTask.StopLoopFish();

		if (wManager.Wow.ObjectManager.ObjectManager.Me.PlayerUsingVehicle)
			wManager.Wow.Helpers.Usefuls.EjectVehicle();

		BlacklistBadNPC();
		Log("reset settings");
	}
	#endregion SETTINGS

	#region SCENARIO
	public static class Scenario
	{
		static robotManager.Helpful.Timer timerInfo = new robotManager.Helpful.Timer(1 * 1000);
		static Scenario()
		{
			timerInfo.ForceReady();
		}
		//local scenarioName, currentStage, numStages, flags, hasBonusStep, isBonusStepComplete, _, xp, money, scenarioType, areaName = C_Scenario.GetInfo();
		const string LUA_INFO = @"local result = ''; local scenarioInfo = {C_Scenario.GetInfo()};
if scenarioInfo and scenarioInfo[1] then
	for i, value in ipairs(scenarioInfo) do
		result = result .. tostring(value) .. '#LUASEPARATOR#'
	end
end
return result";

		static int _stage = 0;
		static int _stageMax = 0;
		static string _oldLog = "";
		static void Log(string text)
		{
			if (_oldLog == text)
				return;

			_oldLog = text;
			Logging.WriteDebug("[Questing.Scenario] " + text);
		}
		static void UpdateInfo()
		{
			if (!timerInfo.IsReady)
				return;

			timerInfo.Reset();
			//var lua = string.Format(LUA_INFO, Lua.ListSeparator);
			var lua = LUA_INFO.Replace("#LUASEPARATOR#", Lua.ListSeparator);
			var values = Lua.LuaDoString<List<string>>(lua);
			int count = values.Count;
			_stage = GetIntValue(values, 1);
			_stageMax = GetIntValue(values, 2);
			Log("info update. values=" + count + " stage=" + _stage + " stage_max=" + _stageMax);
		}
		static int GetIntValue(List<string> values, int i)
		{
			var r = 0;
			if (values.Count > i)
				int.TryParse(values[i], out r);

			return r;
		}
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
				var lua = @"local stageName, stageDescription, numCriteria, _, _, _, numSpells, spellInfo, weightedProgress = C_Scenario.GetStepInfo(); return numCriteria;";
				return Lua.LuaDoString<int>(lua);
			}
		}

		public static int Stage
		{
			get
			{
				//var lua = @"local scenarioName, currentStage, numStages, flags, _, _, _, xp, money, scenarioType = C_Scenario.GetInfo();return currentStage;";
				//return Lua.LuaDoString<int>(lua);
				UpdateInfo();
				return _stage;
			}
		}

		public static int MaxStage
		{
			get
			{
				//var lua = @"local scenarioName, currentStage, numStages, flags, _, _, _, xp, money, scenarioType = C_Scenario.GetInfo(); return numStages;";
				//return Lua.LuaDoString<int>(lua);
				UpdateInfo();
				return _stageMax;
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

	#region ACHIEVEMENT
	public static class Achievement
	{
		//11446 - Legion Flying

		public static bool Done(int achievID)
		{
			var lua = @"local id, name, points, completed, month, day, year, description, flags, icon, rewardText, isGuildAch, wasEarnedByMe, earnedBy = GetAchievementInfo({0}); return completed;";
			var runCode = string.Format(lua, achievID);
			return Lua.LuaDoString<bool>(runCode);
		}
		public static bool DoneCriteria(int achievID, int criteria)
		{
			var lua = @"local description, type, completed, quantity, requiredQuantity, characterName, flags, assetID, quantityString, criteriaID = GetAchievementCriteriaInfo({0}, {1}); return completed;";
			var runCode = string.Format(lua, achievID, criteria);
			return Lua.LuaDoString<bool>(runCode);
		}

	}


	//print("can fly in legion:" .. tostring(completed))
	#endregion ACHIEVEMENT

	#region VEHICLE
	public static class Vehicle
	{
		// /use [overridebar][vehicleui][possessbar,@vehicle,exists]14;[extrabar]13;12
		static void Log(string text)
		{
			Logging.WriteDebug("[Questing.Vehicle] " + text);
		}
		public static bool UseButton(int num = 1)
		{
			//PetActionButton1
			Lua.RunMacroText("/click [overridebar][vehicleui][possessbar,@vehicle,exists]OverrideActionBarButton" + num + ";ActionButton" + num);
			return true;
		}
		public static void Aim(WoWUnit unit, float precision = 0.5f, float angleDelta = 0f)
		{
			ClickToMove.CGPlayer_C__ClickToMove(unit.Position.X, unit.Position.Y, unit.Position.Z, unit.Guid, (int)ClickToMoveType.Move, precision);
			Thread.Sleep(Usefuls.Latency * 2);

			/*
			var dist = ObjectManager.Me.Position.DistanceTo2D(unit.Position);
			var height = unit.Position.Z - ObjectManager.Me.Position.Z;
			var angle = robotManager.Helpful.Math.GetAngle(dist, height);
			//*/
			var angle = robotManager.Helpful.Math.GetAngle(ObjectManager.Me.Position, unit.Position);
			var radians = robotManager.Helpful.Math.DegreeToRadian(angle + angleDelta);
			AimAngle(radians);
		}
		public static void Aim(Vector3 p, float precision = 0.5f, float angleDelta = 0f)
		{
			ClickToMove.CGPlayer_C__ClickToMove(p.X, p.Y, p.Z, MemoryRobot.Int128.Zero(), (int)ClickToMoveType.Move, precision);
			Thread.Sleep(Usefuls.Latency * 2);

			var angle = robotManager.Helpful.Math.GetAngle(ObjectManager.Me.Position, p);
			var radians = robotManager.Helpful.Math.DegreeToRadian(angle + angleDelta);
			AimAngle(radians);
		}
		static void AimAngle(float radians)
		{
			var minDelta = 0.035f; //~5 degree
			var delta = radians - CurrentAngle;
			int timeMs = 30;
			var timer = new robotManager.Helpful.Timer(1 * 1000);
			var delta2 = 0f;
			do
			{
				if (delta > 0)
				{
					Log("pitch up a bit delta=" + delta);
					wManager.Wow.Helpers.Keybindings.PressKeybindings(wManager.Wow.Enums.Keybindings.PITCHUP, timeMs);
				}
				else if (delta < 0)
				{
					Log("pitch down a bit delta=" + delta);
					wManager.Wow.Helpers.Keybindings.PressKeybindings(wManager.Wow.Enums.Keybindings.PITCHDOWN, timeMs);
				}

				Thread.Sleep(timeMs);
				delta2 = radians - CurrentAngle;
				if (System.Math.Abs(delta2) < minDelta)
				{
					Log("aim1. delta=" + delta + " delta2=" + delta2 + " near minDelta=" + minDelta + " time_left=" + timer.TimeLeft());
					return;
				}

				if (delta < 0 && delta2 >= 0)
				{
					Log("aim2. delta=" + delta + " delta2=" + delta2 + " greater minDelta=" + minDelta + " time_left=" + timer.TimeLeft());
					return;
				}

				if (delta > 0 && delta2 <= 0)
				{
					Log("aim3. delta=" + delta + " delta2=" + delta2 + " below minDelta=" + minDelta + " time_left=" + timer.TimeLeft());
					return;
				}
				//Thread.Sleep(timeMs);
			}
			while (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause && ObjectManager.Me.PlayerUsingVehicle && !timer.IsReady);
			Log("aim4. timer expired. delta=" + delta + " delta2=" + delta2 + " below minDelta=" + minDelta + " time_left=" + timer.TimeLeft());
		}
		public static float CurrentAngle
		{
			get
			{
				return Lua.LuaDoString<float>("return VehicleAimGetAngle()");
			}
		}
	}
	#endregion VEHICLE

	static void BlacklistBadNPC()
	{
		wManager.wManagerSetting.AddBlackListNpcEntry(93974, true); //val'sharah, repair only for tailors, wrobot trying to repair but fail http://www.wowhead.com/npc=93974/leyweaver-erenyi

		//wManager.wManagerSetting.AddBlackListNpcEntry(99905, true); //(A) highmountain, vendor in thundertotem, can't fly/elevators http://www.wowhead.com/npc=99905/shale-greyfeather
		NpcDB.ListNpc.RemoveAll(npc => npc.Entry == 99905);
	}

	#region AREA
	public static class Areas
	{
		static List<int> BoostedList = new List<int>()
		{
			(int) ContinentId.BoostExperience,
			(int) ContinentId.BoostExperience2,
			(int) ContinentId.BoostExperience2Horde,
			(int) ContinentId.TransportBoostExperienceAllianceGunship,
			(int) ContinentId.TransportBoostExperienceAllianceGunship2,
			(int) ContinentId.TransportBoostExperienceHordeGunship,
			(int) ContinentId.TransportBoostExperienceHordeGunship2,
		};
		public static bool InBoostedZone
		{
			get { return BoostedList.Contains((int)Usefuls.ContinentId); }
		}
	}
	#endregion AREA

	public class Rect
	{
		public Vector3 UpperTopLeft = Vector3.Zero;
		public Vector3 DownBottomRight = Vector3.Zero;
		public bool IsInside(Vector3 p)
		{
			return false;
		}
	}

	public static class Map
	{
		public static int Current
		{
			get
			{
				return Lua.LuaDoString<int>("SetMapToCurrentZone(); local mapID, isContinent = GetCurrentMapAreaID(); return mapID;");
			}
		}
		static string LuaSetMapByID(int mapID = -1)
		{
			if (mapID > 0)
				return "SetMapByID(" + mapID + ");";
			else
				return "SetMapToCurrentZone();";
		}

		public static int GetLandMarkTextureIndexAt(Vector3 mapCoords, int mapID = -1)
		{
			var lua = LuaSetMapByID(mapID) + @"
local needX = {0}
local needY = {1}
local numPOIs = GetNumMapLandmarks();
for i=1, numPOIs do
	local landmarkType, name, description, textureIndex, x, y, mapLinkID, inBattleMap, graveyardID, areaID, poiID, isObjectIcon, atlasIcon, displayAsBanner = C_WorldMap.GetMapLandmarkInfo(i);
	if (x and y and needX and needY) then
		if (abs(x-needX) < 0.001 and abs(y-needY) < 0.001) then
			return textureIndex
		end
	end
end
return -1
";
			var runCode = string.Format(lua, LuaHelper.ToNumber(mapCoords.X), LuaHelper.ToNumber(mapCoords.Y));
			return Lua.LuaDoString<int>(runCode);
		}

		/// <summary>
		/// ingame map coordinates, not actual positions
		/// </summary>
		/// <param name="mapID"></param>
		/// <param name="landmarks"></param>
		/// <returns></returns>
		public static List<Vector3> GetLandMarks(int mapID, params string[] landmarks)
		{
			var lua = LuaSetMapByID(mapID) + @"
local numPOIs = GetNumMapLandmarks();
local result = '';
for i=1, numPOIs do
	local landmarkType, name, description, textureIndex, x, y, mapLinkID, inBattleMap, graveyardID, areaID, poiID, isObjectIcon, atlasIcon, displayAsBanner = C_WorldMap.GetMapLandmarkInfo(i);
	if (atlasIcon and ({0})) then
		result = result .. x .. '#LUASEPARATOR2#'.. y .. '#LUASEPARATOR#'
	end
end
return result;
";
			var conditionsList = new List<string>();
			foreach (var l in landmarks)
				conditionsList.Add("atlasIcon == '" + l + "'");

			var condition = string.Join(" or ", conditionsList);
			lua = lua.Replace("#LUASEPARATOR#", Lua.ListSeparator);
			lua = lua.Replace("#LUASEPARATOR2#", LuaHelper.Separator);
			lua = string.Format(lua, condition);
			var luaResult = Lua.LuaDoString<List<string>>(lua);
			luaResult.RemoveAll(i => string.IsNullOrEmpty(i));
			var coords = new List<Vector3>();
			foreach (var line in luaResult)
			{
				var p = LuaHelper.ToVector(line);
				if (p != null && p != Vector3.Zero)
					coords.Add(p);
			}
			var result = new List<Vector3>();
			foreach (var mapCoords in coords)
			{
				result.Add(GetPosition(mapCoords, mapID));
			}
			return result;
		}
		public static Vector3 GetPosition(float x, float y, int mapID = -1, bool strict = false)
		{
			return GetPosition(new Vector3(x, y, 0), mapID, strict);
		}
		public static Vector3 GetPosition(Vector3 mapCoords, int mapID = -1, bool strict = false)
		{
			var lua = LuaSetMapByID(mapID) + " local _, worldX, worldY = GetWorldLocFromMapPos({0}, {1}); return worldX .. '#LUASEPARATOR#'.. worldY;";
			lua = lua.Replace("#LUASEPARATOR#", Lua.ListSeparator);
			lua = string.Format(lua, LuaHelper.ToNumber(mapCoords.X), LuaHelper.ToNumber(mapCoords.Y));
			var coords = Lua.LuaDoString<List<float>>(lua);
			var p = LuaHelper.ToVector(coords);
			p.Z = PathFinder.GetZPosition(p, strict);
			return p;
		}
	}

	public class OffmeshHelper
	{
		static OffmeshHelper()
		{
			ClearOffmeshes();
		}
		static void ClearOffmeshes()
		{
			//temp fix
			if (PathFinder.OffMeshConnections.MeshConnection == null)
				PathFinder.OffMeshConnections.MeshConnection = new List<PathFinder.OffMeshConnection>();
			else
				PathFinder.OffMeshConnections.MeshConnection.Clear();

			Log("clear offmeshes=" + PathFinder.OffMeshConnections.MeshConnection.Count);
		}

		static int _combineLimit = 0;
		public static int CombineLimit
		{
			get
			{
				return 0;
			}
			set
			{
				robotManager.Events.ProductEvents.OnProductStopping -= OnProductStop;
				wManager.Events.MovementEvents.OnMovementPulse -= OnMovementPulse;
				_combineLimit = value;
				if (_combineLimit > 0)
				{
					robotManager.Events.ProductEvents.OnProductStopping += OnProductStop;
					wManager.Events.MovementEvents.OnMovementPulse += OnMovementPulse;
				}
				LogOffmesh("combine limit set to " + value);
			}
		}
		static void OnMovementPulse(List<Vector3> points, System.ComponentModel.CancelEventArgs cancelable)
		{
			CombinePath(ref points, _combineLimit);
		}
		public static bool CombinePath(ref List<Vector3> points, int limit)
		{
			if (ObjectManager.Me.InCombat)
				return true;

			if (limit < 1)
				return true;

			if (points == null || points.Count < 2)
				return true;

			var count = points.Count;
			var start = points[0];
			var end = points[count - 1];
			var success = false;
			var path = PathFinder.FindPath(start, end, out success);
			if (!success)
			{
				var offmeshes = PathFinder.OffMeshConnections.MeshConnection;
				if (offmeshes == null || offmeshes.Count < 1)
					return true; //false ?

				var combinedPaths = new List<List<Vector3>>();
				foreach (var offmesh in offmeshes)
				{
					if (offmesh == null || offmesh.Path == null || offmesh.Path.Count < 2 || offmesh.ContinentId != Usefuls.ContinentId)
						continue;

					//LogOffmesh("try combine offmesh=" + offmesh.Name + " ");
					var combinedPath = new List<Vector3>();
					var successToOffmeshStart = false;
					var offmeshPath = offmesh.Path;
					var offmeshStart = offmeshPath[0];
					var pathOffmeshStart = PathFinder.FindPath(start, offmeshStart, out successToOffmeshStart);
					if (!successToOffmeshStart)
					{
						//LogOffmesh("SKIP#"+ limit+". no path to start. offmesh=[" + offmesh.Name + "]");
						continue;
					}

					var successFromOffmeshEnd = false;
					var offmeshEnd = offmeshPath[offmeshPath.Count - 1];
					var pathOffmeshEnd = PathFinder.FindPath(offmeshEnd, end, out successFromOffmeshEnd);
					if (successFromOffmeshEnd)
					{
						combinedPath.AddRange(pathOffmeshStart);
						combinedPath.AddRange(offmeshPath);
						combinedPath.AddRange(pathOffmeshEnd);
						combinedPaths.Add(combinedPath);
						LogOffmesh("FOUND#" + limit + " usefull offmesh path=[" + offmesh.Name + "]");
						continue;
					}
					if (limit <= 1)
					{
						//LogOffmesh("FAILED#" + limit + ". no path to end. offmesh=[" + offmesh.Name + "]");
						continue;
					}
					LogOffmesh("GO DEEP#" + limit + ". from=" + offmeshEnd.ToString() + ". offmesh=[" + offmesh.Name + "]");
					var pathDeep = new List<Vector3>() { offmeshEnd, end };
					CombinePath(ref pathDeep, limit - 1);
					if (pathDeep.Count > 2)
					{
						LogOffmesh("SUCCESS#" + limit + ". deep offmesh path=" + pathDeep.Count);
						combinedPath.AddRange(pathOffmeshStart);
						combinedPath.AddRange(offmeshPath);
						combinedPath.AddRange(pathDeep);
						combinedPaths.Add(combinedPath);
						continue;
					}
				}
				if (combinedPaths.Count < 1)
				{
					LogOffmesh("NOTHING#" + limit + "");
					return true;
				}

				if (combinedPaths.Count > 2)
					combinedPaths.Sort(SortPathsByDistance);

				points.Clear();
				points.AddRange(combinedPaths[0]);
				Var.SetVar("Cameleto10PathTester_Lines_Green", points);
				LogOffmesh("COMBINED#" + limit + ". old path=" + count + " to new=" + points.Count + "");
				Thread.Sleep(10 * 1000);
				return true;
			}
			points.Clear();
			points.AddRange(path);
			LogOffmesh("DIRECT#" + limit + ". old=" + count + " new=" + points.Count + "");
			return true;
		}
		static int SortPathsByDistance(List<Vector3> path1, List<Vector3> path2)
		{
			var dist1 = PathDistanceSqr(path1);
			var dist2 = PathDistanceSqr(path2);
			return dist1.CompareTo(dist2);
		}
		static void OnProductStop(string productName)
		{
			CombineLimit = 0;
		}
		static void LogOffmesh(string text)
		{
			Logging.Write("[Offmesh helper] " + text, Logging.LogType.Debug, System.Drawing.Color.Gold);
		}
		public static bool CreateChain(int continentID, string prefix, params Vector3[] connectPoints)
		{
			return CreateChain(continentID, prefix, connectPoints.ToList());
		}
		public static bool CreateChain(int continentID, string prefix, List<Vector3> connectPoints)
		{
			var result1 = ConnectPoints(continentID, prefix, connectPoints);
			if (!result1)
				return false;

			connectPoints.Reverse();
			return ConnectPoints(continentID, prefix + "(reversed)", connectPoints);
		}
		static bool ConnectPoints(int continentID, string prefix, List<Vector3> points)
		{
			var offmeshConnections = new List<PathFinder.OffMeshConnection>();
			if (points == null || points.Count < 2)
				return false;

			var count = points.Count;

			for (int startI = 0; startI < count - 1; startI++)
			{
				for (int endI = startI + 1; endI < count; endI++)
				{
					var offmesh = new PathFinder.OffMeshConnection();
					offmesh.Name = prefix + " (" + startI + "-" + endI + ")";
					offmesh.ContinentId = continentID;
					offmesh.Path = new List<Vector3>();
					//LogOffmesh("@>>> new offmesh name=" + offmesh.Name + " continent=" + continentID + " ");
					for (int pathI = startI; pathI <= endI; pathI++)
					{
						var p = points[pathI];
						offmesh.Path.Add(p);
						//LogOffmesh("#----- >>> add start=" +startI+" end=" +endI+ " current=>"+pathI + " p="+ p.ToStringNewVector()+" " );
					}
					offmeshConnections.Add(offmesh);
				}
			}
			//LogOffmesh("====== DONE CONNECT POINTS prefix="+ prefix+". points=" + connectPoints.Count+" offmes_links=" + offmeshConnections.Count);
			return Add(offmeshConnections);
		}

		public static bool Add(params PathFinder.OffMeshConnection[] offmeshConnections)
		{
			return Add(offmeshConnections.ToList());
		}
		public static bool Add(List<PathFinder.OffMeshConnection> offMeshConnections)
		{
			if (PathFinder.OffMeshConnections.MeshConnection == null || PathFinder.OffMeshConnections.MeshConnection.Count <= 0)
				PathFinder.OffMeshConnections.Load();

			PathFinder.OffMeshConnections.MeshConnection.AddRange(offMeshConnections);
			//PathFinder.OffMeshConnections.Save();
			return true;
		}
	}

	public static class LuaHelper
	{
		public static string Separator = "@";

		public static string ToNumber(float val)
		{
			return val.ToString().Replace(",", ".");
		}
		public static string[] ToStrings(string luaResult, string separator = "")
		{
			if (string.IsNullOrEmpty(separator))
				separator = Separator;

			string[] lines = luaResult.Split(new string[1] { separator }, System.StringSplitOptions.RemoveEmptyEntries);
			return lines;
		}
		public static float[] ToFloats(string luaResult)
		{
			var lines = ToStrings(luaResult);
			var result = new float[lines.Length];
			for (int i = 0; i < lines.Length; i++)
			{
				var line = lines[i];
				float val = 0;
				float.TryParse(line, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out val);
				result[i] = val;
			}
			return result;
		}
		public static Vector3 ToVector(IList<float> arr)
		{
			var p = new Vector3();
			if (arr != null && arr.Count > 1)
			{
				p.X = arr[0];
				p.Y = arr[1];
				p.Z = 0;
			}
			return p;
		}
		public static Vector3 ToVector(string luaResult)
		{
			var coords = ToFloats(luaResult);
			return ToVector(coords);
		}

	}

	public class Group
	{
		static Thread _thread = null;
		static string _logged = string.Empty;
		static BoolDelegate _condition = null;

		static void Trace(string text)
		{
			if (string.Equals(_logged, text)) return;

			_logged = text;
			Logging.WriteDebug("[Questing.Group] " + _logged);
		}
		static void Routine()
		{
			while (robotManager.Products.Products.IsStarted)
			{
				if (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause)
				{
					if (_condition != null && _condition() == false)
					{
						Stop();
						return;
					}
					Lua.LuaDoString("ConfirmReadyCheck(1); AcceptGroup();");
				}
				Thread.Sleep(3 * 1000);
			}
			Stop();
		}
		public static void Start(BoolDelegate condition = null)
		{
			if (_thread != null)
				Stop();

			_condition = condition;
			_thread = new Thread(Routine);
			_thread.Start();
			Trace("started");
		}
		public static void Stop()
		{
			if (_thread == null)
				return;

			_thread.Abort();
			_thread = null;
			Lua.LuaDoString("LeaveParty()");
			Trace("stopped");
		}
	}
}
// QUESTING END