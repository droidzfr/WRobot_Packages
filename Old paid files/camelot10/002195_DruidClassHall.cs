/// required QUESTING, LEGION QUESTS
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

public class DruidClassHall : QuestClass
{
	#region QUEST ID
	public const int QUEST_FIRST_ARTIFACT = 41255; // http://www.wowhead.com/quest=41255/sowing-the-seed;
	public const int QUEST_SECOND_ARTIFACT = 43980; // http://www.wowhead.com/quest=43980/another-weapon-of-old
	public const int QUEST_THIRD_ARTIFACT = 44431; // http://www.wowhead.com/quest=44431/more-weapons-of-old
	public const int QUEST_FOURTH_ARTIFACT = 44443; // http://www.wowhead.com/quest=44443/weapons-of-the-ancients
	public const int QUEST_ARTIFACT_BALANCE = 40900;
	public const int QUEST_ARTIFACT_FERAL = 42430;
	public const int QUEST_ARTIFACT_GUARDIAN = 41918;
	public const int QUEST_ARTIFACT_RESTORATION = 41689;
	#endregion QUEST ID

	public DruidClassHall()
	{
		Name = "[Druid Class Hall]";
		QuestId.Add(0);
		Step.Add(0);
	}

	public static bool NeedFirstArtifact { get { return Questing.NotComplete(QUEST_FIRST_ARTIFACT); } }
	public static bool NeedArtifactBalance { get { return Questing.NotComplete(QUEST_ARTIFACT_BALANCE); } }
	public static bool NeedArtifactFeral { get { return Questing.NotComplete(QUEST_ARTIFACT_FERAL); } }
	public static bool NeedArtifactGuardian { get { return Questing.NotComplete(QUEST_ARTIFACT_GUARDIAN); } }
	public static bool NeedArtifactRestoration { get { return Questing.NotComplete(QUEST_ARTIFACT_RESTORATION); } }

	public static bool NeedBalanceFirst { get { return WowSpecializations.Druid_Balance == ObjectManager.Me.GetSpecialization && NeedArtifactBalance && NeedFirstArtifact; } }
	public static bool NeedFeralFirst { get { return WowSpecializations.Druid_Feral == ObjectManager.Me.GetSpecialization && NeedArtifactFeral && NeedFirstArtifact; } }
	public static bool NeedGuardianFirst { get { return WowSpecializations.Druid_Guardian == ObjectManager.Me.GetSpecialization && NeedArtifactGuardian && NeedFirstArtifact; } }
	public static bool NeedRestorationFirst { get { return WowSpecializations.Druid_Restoration == ObjectManager.Me.GetSpecialization && NeedArtifactRestoration && NeedFirstArtifact; } }

	public static bool ToDalaran()
	{
		if (InClassHall)
		{
			var pos = new Vector3();
			GoToTask.ToPositionAndIntecractWithGameObject(pos, 0);
			return true;
		}
		LegionQuests.UseDalaranHeathstone();
		return true;
	}
	public static bool InMoonglade { get { return Usefuls.ContinentId == (int) ContinentId.Kalimdor && Usefuls.AreaId == Areas.Moonglade; } }
	public static bool InDreamgrove { get { return Usefuls.AreaId == Areas.Dreamgrove; } }
	public static bool InEmeraldDreamway { get { return Usefuls.AreaId == Areas.EmeraldDreamway; } }
	public static bool InGrizzlyHills { get { return Usefuls.AreaId == Areas.GrizzlyHills; } }

	public static bool ToClassHall()
	{
		return ToDreamgrove();
	}
	public static bool ToDreamgrove()
	{
		if (InDreamgrove)
		{
			Log("in Dreamgrove");
			return true;
		}
		if (InEmeraldDreamway)
		{
			Log("in Emerald Dreamway. Port to Dreamgrove");
			if (GoToTask.ToPosition(Positions.DreamgrovePortNear))
			{
				Questing.ClickMove(Positions.DreamgrovePortIn);
				Questing.Wait(10);
			}
			return true;
		}
		Log("not in Emerald Dreamway. need move in Dreamgrove");
		ToEmeraldDreamway();
		return false;
	}
	public static bool ToEmeraldDreamway()
	{
		if (InEmeraldDreamway)
		{
			Log("in Emerald Dreamway. done");
			return true;
		}
		Log("need move in Emerald Dreamway. use Dreamwalk");
		SpellManager.CastSpellByIdLUA(204874);
		Usefuls.WaitIsCasting();
		Questing.Wait(2);
		return false;
	}
	public static bool ToGrizzlyHills()
	{
		if (InGrizzlyHills)
		{
			Log("in Grizzly Hills. done");
			return true;
		}
		if (InEmeraldDreamway)
		{
			Log("in Emerald Dreamway. Port to Grizzly Hills");
			if (GoToTask.ToPosition(Positions.GrizzlyHillsPortNear))
			{
				Questing.ClickMove(Positions.GrizzlyHillsPortIn);
				Questing.Wait(10);
			}
			return true;
		}
		Log("not in Emerald Dreamway. need move in Grizzly Hills");
		ToEmeraldDreamway();
		return false;
	}
	public static class Areas
	{
		public static int Moonglade = 493;
		public static int EmeraldDreamway = 7979;
		public static int Dreamgrove = 7846;
		public static int GrizzlyHills = 394; //ContinentNameMpq = Northrend // ContinentId = 571 // AreaId = 394

	}
	public static class Positions
	{
		public static Vector3 DreamgrovePortNear = new Vector3(1774.086, 1507.432, 8.155212, "None");
		public static Vector3 DreamgrovePortIn = new Vector3(1784.506, 1504.145, 9.844843);
		public static Vector3 EmeraldDreamPortNear = new Vector3(4141.608, 7302.58, 22.75611);
		public static Vector3 EmeraldDreamPortIn = new Vector3(4150.013, 7293.335, 22.77204);
		public static Vector3 GrizzlyHillsPortNear = new Vector3(1760.629, 1606.302, 8.390942);
		public static Vector3 GrizzlyHillsPortIn = new Vector3(1771.433, 1609.672, 8.922731);
	}

	#region CONDITIONS
	public static bool Can { get { return WoWClass.Druid == ObjectManager.Me.WowClass; } }
	public static bool InClassHall { get { return LegionQuests.InClassHall; } }
	#endregion CONDITIONS

	#region UTILS
	static void Log(string text)
	{
		Logging.WriteDebug("[Druid Class Hall] " + text);
	}
	#endregion UTILS


}
