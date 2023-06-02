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

public sealed class Avoiding : QuestClass
{
	public static float Range = 50;
	public delegate bool BoolDelegate();
	static Thread _avoidThread;
	static List<int> _avoidMobs;
	static BoolDelegate _avoidCondition;
	static int _avoidDelay = 200;
	static int _avoidResumeMoveDelay = 1000;
	static void Log(string text)
	{
		Logging.WriteDebug("[Avoiding] " + text);
	}
	void Snippets()
	{
		Avoiding.StartAvoid(new List<int>() { 1234, 1234 }, ()=> true);
		Avoiding.StartAvoid(1234, 1234);
		Avoiding.StopAvoid();
	}

	public static void StartAvoid(params int[] mobs)
	{
		StartAvoid(new List<int>(mobs), () => true);
	}

	public static void StartAvoid(List<int> mobs, BoolDelegate condition = null, float range = 50f)
	{
		StopAvoid();
		if (condition == null)
			condition = () => true;

		Range = range;
		_avoidMobs = mobs;
		_avoidCondition = condition;
		_avoidThread = new Thread(AvoidThreadLoop);
		_avoidThread.Start();
		//wManager.Events.FightEvents.OnFightLoop += AvoidFightLoop;
	}

	public static void StopAvoid()
	{
		if (_avoidThread != null)
			_avoidThread.Abort();

		wManager.Events.FightEvents.OnFightLoop -= AvoidFightLoop;
		_avoidCondition = () => true;
		_avoidMobs = new List<int>();
	}

	static bool AvoidLogic()
	{
		if (ObjectManager.Me.InCombat)
			return false;

		if (!_avoidCondition())
		{
			StopAvoid();
			return false;
		}
		var mobs = ObjectManager.GetObjectWoWUnit().Where(u => u != null && u.IsValid && u.IsAlive && u.IsAttackable && _avoidMobs.Contains(u.Entry) && u.GetDistance < Range).ToList();
		if (mobs.Count < 0)
			return false;

		var deltaSummary = new Vector3();
		foreach (var mob in mobs)
		{
			var delta = mob.Position - ObjectManager.Me.Position;
			var deltaMag = delta.Magnitude();
			var deltaMove = delta / deltaMag * (deltaMag - Range);
			deltaSummary += deltaMove;
		}
		if (deltaSummary.MagnitudeSqr() < 5f * 5f)
			return false;

		MovementManager.StopMove();
		var result = false;
		var p = ObjectManager.Me.Position + deltaSummary;
		var path = PathFinder.FindPath(p, out result);
		if (!result)
			return false;

		Log("avoid mobs " + string.Join(",", mobs.Select(m => m.Name).ToArray()));
		var oldFood = wManager.wManagerSetting.CurrentSetting.FoodPercent;
		var oldDrink = wManager.wManagerSetting.CurrentSetting.DrinkPercent;
		wManager.wManagerSetting.CurrentSetting.FoodPercent = 1;
		wManager.wManagerSetting.CurrentSetting.DrinkPercent = 1;
		Conditions.ForceIgnoreIsAttacked = true;
		MovementManager.StopMoveTo(true, _avoidResumeMoveDelay);
		MovementManager.Go(path);
		while (MovementManager.InMovement && Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause)
		{
			Thread.Sleep(_avoidDelay);
		}
		wManager.wManagerSetting.CurrentSetting.FoodPercent = oldFood;
		wManager.wManagerSetting.CurrentSetting.DrinkPercent = oldDrink; 
		MovementManager.StopMove();
		Conditions.ForceIgnoreIsAttacked = false;
		return true;
	}

	static void AvoidThreadLoop()
	{
		while (robotManager.Products.Products.IsStarted)
		{
			if (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause)
			{
				AvoidLogic();
			}
			Thread.Sleep(_avoidDelay);
		}
	}

	static void AvoidFightLoop(WoWUnit unit, System.ComponentModel.CancelEventArgs cancelable)
	{
		//cancelable.Cancel = true;
		if (AvoidLogic())
		{
			//Logging.Write("figth avoid");
		}
	}



}

