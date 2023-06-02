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
using System.ComponentModel;
using System.IO;
#endif

public class Fisher
{
	static int BlacklistMinutes = 3;
	static int HotspotIndex = 0;

	static robotManager.Helpful.Timer _timer = new robotManager.Helpful.Timer(4 * 60 * 1000);
	static MemoryRobot.Int128 _lastGuid = MemoryRobot.Int128.Zero();

	public Fisher()
	{
		ResetSettings();
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

		GoToTask.ToPosition(p, 3.5f, false, (c) => {
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

		var fishPosition = Fishing.FindTheUltimatePoint(pool.Position, false);
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
				if (GoToTask.ToPosition(fishPosition, 1.5f, false, (c) => {
					if (Conditions.IsAttackedAndCannotIgnore)
						return false;

					var otherPool = ObjectManager.GetNearestWoWGameObject(ObjectManager.GetWoWGameObjectByEntry(poolIDs));
					if (otherPool != null && otherPool.IsValid && otherPool.Guid != poolGUID && Fishing.FindTheUltimatePoint(otherPool.Position, false) != Vector3.Zero)
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
						FishingTask.LoopFish(poolGUID, false, "", "", 600000, "", 3600000, 0, true);
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

		if (GoToTask.ToPosition(position, 1.5f, false, (c) => {
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


}
//FISHER END