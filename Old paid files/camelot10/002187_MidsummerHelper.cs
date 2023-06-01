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

public class MidsummerHelper : QuestClass
{
	bool _isComplete = false;

	public MidsummerHelper()
	{
		Name = "Midsummer Helper";
		QuestId.Add(0);
		Step.AddRange(new[] { 0, 0, 0, 0, 0 });
		ChangeSetteings();
		SubscribeEvents();
	}
	static void Log(string text)
	{
		Logging.Write("[Midsummer Helper] " + text, Logging.LogType.Debug, System.Drawing.Color.DarkOrange);
	}
	static void SubscribeEvents()
	{
		UnsubscribeEvents();
		robotManager.Events.ProductEvents.OnProductStopping += OnProductStopping;
		wManager.Events.InteractEvents.OnInteractPulse += OnInteractPulse;
		Log("events subscribed");
	}
	static void UnsubscribeEvents()
	{
		robotManager.Events.ProductEvents.OnProductStopping -= OnProductStopping;
		wManager.Events.InteractEvents.OnInteractPulse -= OnInteractPulse;
		Log("events unsubscribed");
	}
	static void OnProductStopping(string productName)
	{
		UnsubscribeEvents();
		Log("product stopping");
	}
	static void ChangeSetteings()
	{
		wManager.wManagerSetting.CurrentSetting.AquaticMountName = ""; //aquatic mount cause problems with moving in waters
		wManager.wManagerSetting.CurrentSetting.HarvestAvoidPlayersRadius = 0; //ignore players near quest items
		wManager.wManagerSetting.CurrentSetting.SecurityPauseBotIfNerbyPlayer = false;
		wManager.wManagerSetting.CurrentSetting.SecurityPauseBotIfNerbyPlayerRadius = 0;
		wManager.wManagerSetting.CurrentSetting.AvoidWallWithRays = true;
		wManager.wManagerSetting.CurrentSetting.AvoidBlacklistedZonesPathFinder = true;
		wManager.wManagerSetting.CurrentSetting.IgnoreCombatWithPet = true;
		wManager.wManagerSetting.CurrentSetting.DetectEvadingMob = true;
		wManager.wManagerSetting.CurrentSetting.HarvestHerbs = false;
		wManager.wManagerSetting.CurrentSetting.HarvestMinerals = false;
		wManager.wManagerSetting.CurrentSetting.HarvestTimber = false;
		wManager.wManagerSetting.CurrentSetting.SkinMobs = false;
		wManager.wManagerSetting.CurrentSetting.SkinNinja = false;
		wManager.Wow.Helpers.CVar.SetCVar("autoDismount", "1");
		wManager.Wow.Helpers.CVar.SetCVar("autoDismountFlying", "1");
		wManager.Wow.Helpers.CVar.SetCVar("autoLootDefault", "1");
		wManager.Wow.Helpers.CVar.SetCVar("autounshift", "1");
		Log("settings changed");
	}
	static void OnInteractPulse(MemoryRobot.Int128 target, System.ComponentModel.CancelEventArgs cancelable)
	{
		var step = Quest.QuesterCurrentContext.CurrentStep;
		var p = Quest.QuesterCurrentContext.Profile as Quester.Profile.QuesterProfile;
		var sorted = p.QuestsSorted.ElementAt(step);
		if (sorted.Action != wManager.Wow.Class.QuestAction.TurnIn && sorted.Action != wManager.Wow.Class.QuestAction.PulseAllInOne)
			return;

		var interactTarget = ObjectManager.GetObjectByGuid(target);
		if (interactTarget == null || !interactTarget.IsValid)
			return;

		var turnInNpc = sorted.QuestClassInstance.NpcTurnIn;
		if (turnInNpc != null && interactTarget.Entry != turnInNpc.Id)
			return;

		var questID = sorted.QuestClassInstance.QuestId[0];
		if (!Lua.LuaDoString<bool>("return IsQuestFlaggedCompleted(" + questID + ")"))
			return;

		cancelable.Cancel = true;
		Log("restart quester. quest completed=" + questID + " >> " + Quest.GetQuestCompleted(questID));
		System.Threading.Tasks.Task.Delay(10).ContinueWith(t => robotManager.Products.Products.ProductRestart());
	}
}