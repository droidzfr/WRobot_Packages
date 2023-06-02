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

public class DemonHunterClassHall : QuestClass
{
	#region QUEST ID
	public const int QUEST_START_ZONE_FINAL_HAVOC_KAYN = 39688;
	public const int QUEST_START_ZONE_FINAL_VENGEANCE_KAYN = 40255;
	public const int QUEST_START_ZONE_FINAL_HAVOC_ALTRIUS = 39694;
	public const int QUEST_START_ZONE_FINAL_VENGEANCE_ALTRIUS = 40256;
	public const int QUEST_ARTIFACT_HAVOC_KAYN = 39247;
	public const int QUEST_ARTIFACT_HAVOC_ALTRIUS = 41119;
	public const int QUEST_ARTIFACT_VENGEANCE_KAYN = 40249;
	public const int QUEST_ARTIFACT_VENGEANCE_ALTRIUS = 41863;
	public const int QUEST_FIRST_ARTIFACT = 42869;

	public const int QUEST_CHOOSE_SECOND_ARTIFACT_KAYN = 44383;
	public const int QUEST_CHOOSE_SECOND_ARTIFACT_ALTRIUS = 44379;

	#endregion QUEST ID

	public DemonHunterClassHall()
	{
		Name = "Demon Hunter Class Hall";
		QuestId.Add(0);
		Step.AddRange(new[] { 0, 0, 0, 0, 0 });
	}
	protected static void Log(string text)
	{
		Logging.WriteDebug("[Demon Hunter Class Hall] " + text);
	}

	#region CONDITIONS
	public static bool Can
	{
		get
		{
			return WoWClass.DemonHunter == ObjectManager.Me.WowClass;
		}
	}

	public static bool NeedHavocFirst
	{
		get
		{
			return WowSpecializations.DemonHunter_Havoc == ObjectManager.Me.GetSpecialization && !NeedStart && NeedArtifactHavoc && NeedFirstArtifact;
			//return WowSpecializations.DemonHunter_Havoc == ObjectManager.Me.GetSpecialization && ((Quest.GetQuestCompleted(39688) && !Quest.GetQuestCompleted(39247)) || (Quest.GetQuestCompleted(40255) && !Quest.GetQuestCompleted(41119))) && Questing.Need(42869);
		}
	}

	public static bool NeedVengeanceFirst
	{
		get
		{
			return WowSpecializations.DemonHunter_Vengeance == ObjectManager.Me.GetSpecialization && !NeedStart && NeedArtifactVengeance && NeedFirstArtifact;
			//return WowSpecializations.DemonHunter_Vengeance == ObjectManager.Me.GetSpecialization && ((Quest.GetQuestCompleted(39688) && !Quest.GetQuestCompleted(40249)) || (Quest.GetQuestCompleted(40255) && !Quest.GetQuestCompleted(41863))) && Questing.Need(42869);
		}
	}

	public static bool NeedStart
	{
		get
		{
			return Questing.NotComplete(
				QUEST_START_ZONE_FINAL_HAVOC_KAYN,
				QUEST_START_ZONE_FINAL_VENGEANCE_KAYN,
				QUEST_START_ZONE_FINAL_HAVOC_ALTRIUS,
				QUEST_START_ZONE_FINAL_VENGEANCE_ALTRIUS
			);
		}
	}

	public static bool NeedArtifactHavoc
	{
		get
		{
			return Questing.NotComplete(QUEST_ARTIFACT_HAVOC_ALTRIUS, QUEST_ARTIFACT_HAVOC_KAYN);
		}
	}

	public static bool NeedArtifactVengeance
	{
		get
		{
			return Questing.NotComplete(QUEST_ARTIFACT_VENGEANCE_KAYN, QUEST_ARTIFACT_VENGEANCE_ALTRIUS);
		}
	}

	public static bool NeedFirstArtifact
	{
		get
		{
			return Questing.NotComplete(QUEST_FIRST_ARTIFACT);
		}
	}

	public static bool NeedSecondArtifact
	{
		get
		{
			return Questing.NotComplete(QUEST_CHOOSE_SECOND_ARTIFACT_KAYN, QUEST_CHOOSE_SECOND_ARTIFACT_ALTRIUS);
		}
	}

	public static bool InClassHall
	{
		get
		{
			return LegionQuests.InClassHall;
		}
	}
	#endregion CONDITIONS


	#region COMMANDS
	public static bool UseTable()
	{
		if (!InClassHall)
		{
			ToClassHall();
			return false;
		}
		if (LegionQuests.Mission.IsVisible)
			return true;

		var pos = new Vector3(1558.994, 1412.886, 237.1088, "None");
		GoToTask.ToPositionAndIntecractWithNpc(pos, 98613);
		Thread.Sleep(Others.Random(500, 1000));
		return true;
	}


	public static bool ToClassHall()
	{
		if (InClassHall)
			return true;

		if (!InIllidaryRedoubt)
		{
			ToIllidaryRedoubt();
			return false;
		}
		GoToTask.ToPositionAndIntecractWithGameObject(IllidaryRedoubtCenter, 251286);
		return true;
	}

	public static bool ToDalaran()
	{
		if (InClassHall)
		{
			var pos = new Vector3(1457.668, 1411.165, 244.9198);
			GoToTask.ToPositionAndIntecractWithGameObject(pos, 247007);
			return true;
		}
		LegionQuests.UseDalaranHeathstone();
		return true;
	}

	public static bool GatherAndTrainAshtongueWarriors()
	{
		if (!InClassHall)
		{
			ToClassHall();
			return true;
		}
		var p = new Vector3(1558.925, 1424.124, 237.1093, "None");
		if (GoToTask.ToPosition(p))
		{
			var flag = ObjectManager.GetNearestWoWGameObject(ObjectManager.GetWoWGameObjectByEntry(250891));
			if (flag != null && flag.IsValid)
			{
				Questing.Gather(flag.Position, flag.Entry);
			}
			if (GoToTask.ToPositionAndIntecractWithNpc(p, 103025))
			{
				Thread.Sleep(Others.Random(2000, 3000));
				LegionQuests.StartWorkOrder();
			}
		}
		return true;
	}
	#endregion COMMANDS


	#region ILLIDARY_REDOUBT
	public static float IllidaryRedoubtRadius = 70;
	public static Vector3 IllidaryRedoubtCenter = new Vector3(-960.0129, 4071.191, 648.1414, "None");
	public static int DalaranAreaId = 7502;
	public static Vector3 IllidaryRedoubtGlideStart = new Vector3(-824.3328, 4243.032, 743.4282, "None");

	public static bool InIllidaryRedoubt
	{
		get
		{
			if (Usefuls.ContinentId != (int)ContinentId.Troll_Raid)
				return false;

			if (Usefuls.AreaId != DalaranAreaId)
				return false;

			var myPos = ObjectManager.Me.Position;
			if (myPos.DistanceTo(IllidaryRedoubtCenter) > IllidaryRedoubtRadius)
				return false;

			return myPos.DistanceZ(IllidaryRedoubtCenter) < 20;
		}
	}

	public static bool ToIllidaryRedoubt()
	{
		if (ObjectManager.Me.IsFalling)
		{
			Log("glide");
			Thread.Sleep(2 * 1000);
			return false;
		}
		if (Usefuls.ContinentId != (int)ContinentId.Troll_Raid || Usefuls.AreaId != DalaranAreaId)
		{
			Log("dalaran heathstone");
			LegionQuests.UseDalaranHeathstone();
			return false;
		}
		if (!InIllidaryRedoubt)
		{
			Log("glide to illidary redoubt");
			GlideToIllidaryRedoubt();
			return false;
		}
		return true;
	}

	static bool GlideToIllidaryRedoubt()
	{
		if (ObjectManager.Me.IsFalling)
		{
			Log("glide");
			Thread.Sleep(2 * 1000);
			return true;
		}

		if (GoToTask.ToPosition(IllidaryRedoubtGlideStart))
		{
			Log("im on glide pos. start glide");
			MountTask.DismountMount();
			Thread.Sleep(Others.Random(1000, 2000));
			MovementManager.MoveTo(IllidaryRedoubtCenter);
			Thread.Sleep(300);
			Move.JumpOrAscend();
			Thread.Sleep(300);
			Move.JumpOrAscend();
			SpellManager.CastSpellByIdLUA(131347);
			Thread.Sleep(4 * 1000);
		}
		Log("im in illidary redoubt");
		return true;
	}

	#endregion ILLIDARY_REDOUBT


	#region QUEST
	public override bool Pulse()
	{
		if (!InClassHall)
			ToClassHall();

		return true;
	}

	public override bool CanConditions()
	{
		return ObjectManager.Me.WowClass == WoWClass.DemonHunter;
	}
	public override bool HasQuest()
	{
		return CanConditions();
	}
	public override bool IsCompleted()
	{
		return InClassHall;
	}
	public override bool IsComplete()
	{
		return IsCompleted();
	}
	#endregion QUEST
}
