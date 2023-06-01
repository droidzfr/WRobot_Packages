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

public class WitheredArmy : QuestClass
{
	static uint _buffScared = 218958;
	const string STEP_VAR_NAME = "Camelot10WitheredArmy";
	static float _collectRange = 25f;
	static int _buddyFaction = 1665;
	List<Vector3Bool> _path = new List<Vector3Bool>()
	{
		new Vector3Bool(2131.001, 5386.454, 50.38994),
		new Vector3Bool(2199.796, 5400.14, 36.8759),
		new Vector3Bool(2206, 5398.055, 36.87733),
		new Vector3Bool(2221.545, 5377.31, 36.87601),
		new Vector3Bool(2219.827, 5360.882, 36.87654),
		new Vector3Bool(2190.537, 5338.958, 36.87654), //5
		new Vector3Bool(2184.51, 5336.269, 36.8958), //double
		new Vector3Bool(2200.619, 5339.696, 36.87654),
		new Vector3Bool(2250.459, 5338.11, 37.01077, ()=> Quest.GetQuestCompleted(43145) && Quest.GetQuestCompleted(43071) ), //has 2 berserkers
		new Vector3Bool(2265.25, 5270.331, 14.51278, ()=> Quest.GetQuestCompleted(43145) && Quest.GetQuestCompleted(43071) ), //has 2 berserkers
		new Vector3Bool(2282.758, 5244.566, 13.9558, ()=> Quest.GetQuestCompleted(43145) && Quest.GetQuestCompleted(43071) ), //has 2 berserkers
		new Vector3Bool(2313.98, 5245.655, 14.53681, ()=> Quest.GetQuestCompleted(43145) && Quest.GetQuestCompleted(43071) ), //has 2 berserkers
		new Vector3Bool(2271.748, 5215.983, 14.53682, ()=> Quest.GetQuestCompleted(43145) && Quest.GetQuestCompleted(43071) ), //has 2 berserkers
		new Vector3Bool(2302.832, 5215.287, 15.94579, ()=> Quest.GetQuestCompleted(43145) && Quest.GetQuestCompleted(43071) ), //has 2 berserkers
		new Vector3Bool(2305.283, 5247.619, 14.84673, ()=> Quest.GetQuestCompleted(43145) && Quest.GetQuestCompleted(43071) ), //has 2 berserkers //double
		new Vector3Bool(2281.294, 5226.252, 14.66984, ()=> Quest.GetQuestCompleted(43145) && Quest.GetQuestCompleted(43071) ), //has 2 berserkers //double
		new Vector3Bool(2302.832, 5215.287, 15.94579, ()=> Quest.GetQuestCompleted(43145) && Quest.GetQuestCompleted(43071) ), //has 2 berserkers //double
		new Vector3Bool(2295.613, 5409.638, 16.5676, ()=> Quest.GetQuestCompleted(43145)), //has berserker
		new Vector3Bool(2352.687, 5401.046, 14.53832, ()=> Quest.GetQuestCompleted(43145)), //has berserker
		new Vector3Bool(2337.256, 5425.15, 14.00465, ()=> Quest.GetQuestCompleted(43145)), //has berserker
		new Vector3Bool(2323.875, 5454.477, 14.17906, ()=> Quest.GetQuestCompleted(43145)), //has berserker
		new Vector3Bool(2364.284, 5439.866, 15.93828, ()=> Quest.GetQuestCompleted(43145)), //has berserker
		new Vector3Bool(2326.985, 5457.619, 14.04999, ()=> Quest.GetQuestCompleted(43145)), //has berserker //double
		new Vector3Bool(2359.201, 5412.001, 14.53088, ()=> Quest.GetQuestCompleted(43145)), //has berserker //double
		new Vector3Bool(2366.038, 5433.521, 15.84403, ()=> Quest.GetQuestCompleted(43145)), //has berserker //double
		new Vector3Bool(2364.199, 5406.167, 14.86854, ()=> Quest.GetQuestCompleted(43145)), //has berserker //double
		new Vector3Bool(2312.945, 5412.736, 14.83897, ()=> Quest.GetQuestCompleted(43145)), //has berserker //double
		new Vector3Bool(2367.137, 5441.843, 15.94879, ()=> Quest.GetQuestCompleted(43145)), //has berserker //double
		new Vector3Bool(2295.613, 5409.638, 16.5676, ()=> Quest.GetQuestCompleted(43145)), //has berserker //double
		new Vector3Bool(2253.811, 5351.977, 37.02298), //
		new Vector3Bool(2292.775, 5403.95, 18.32154), //19 before wall crack
		new Vector3Bool(2286.895, 5429.94, 15.12867), //after wall crack
		new Vector3Bool(2272.417, 5457.187, 9.422404),
		new Vector3Bool(2247.089, 5519.042, 1.641239, ()=> !Quest.GetQuestCompleted(43149) ), //get 25% health treasure
		new Vector3Bool(2239.739, 5521.931, 1.641034),
		new Vector3Bool(2195.144, 5528.069, 1.641239),
		new Vector3Bool(2184.927, 5539.891, 1.640897), //chest //double
		new Vector3Bool(2182.205, 5483.355, 1.592948),
		new Vector3Bool(2157.548, 5433.917, 1.641177),
		new Vector3Bool(2201.579, 5422.103, 1.640879),
		new Vector3Bool(2204.557, 5427.115, 1.65529), //chest //double
		new Vector3Bool(2157.553, 5426.494, 1.640805), //double
		new Vector3Bool(2206.876, 5474.39, 1.593738), //chest //double
		new Vector3Bool(2179.963, 5467.543, -24.81754), //downstairs
		new Vector3Bool(2195.835, 5460.954, -24.81545), //big chest
		new Vector3Bool(2187.822, 5477.376, -25.40438), //30
		new Vector3Bool(2161.923, 5477.746, -25.09356),
		new Vector3Bool(2189.668, 5517.879, -25.09356),
		new Vector3Bool(2237.01, 5482.355, -25.09342),
		new Vector3Bool(2201.452, 5441.978, -25.09342),
		new Vector3Bool(2179.963, 5467.543, -24.81754),
		new Vector3Bool(2224.338, 5467.907, 1.651839),  //double
		new Vector3Bool(2111.187, 5489.16, 3.405121), //36 upstairs
		new Vector3Bool(2115.111, 5492.513, 2.107438), //chest
		new Vector3Bool(2109.359, 5493.528, 2.526567, ()=> !Quest.GetQuestCompleted(43146) ), //mana rager
		new Vector3Bool(2109.6, 5501.621, -12.25291), //go down
		new Vector3Bool(2098.635, 5423.809, -34.46243), //40
		new Vector3Bool(2104.841, 5423.174, -34.32187), //double
		new Vector3Bool(2097.004, 5443.904, -34.06064),//chest //double
		new Vector3Bool(2102.527, 5390.494, -36.91036),
		new Vector3Bool(2066.211, 5391.58, -36.4929), //chest <<tested
		new Vector3Bool(2105.874, 5376.956, -36.91038),  //double
		new Vector3Bool(2107.246, 5378.495, -36.90988), //double
		new Vector3Bool(2077.865, 5391.806, -37.24174), //double
		new Vector3Bool(2101.689, 5383.314, -36.90977), //double
		new Vector3Bool(2065.853, 5401.769, -34.70255), //double
		new Vector3Bool(2098.635, 5423.809, -34.46243), //double
		new Vector3Bool(2000.063, 5444.759, -37.19492), //43 << tested
		new Vector3Bool(1956.164, 5396.741, -37.15388), //44 << tested
		new Vector3Bool(1935.047, 5359.019, -36.22533), //45 << tested
		new Vector3Bool(1899.489, 5367.176, -37.19387),
		new Vector3Bool(1986.89, 5443.457, -36.58498, ()=> !Quest.GetQuestCompleted(43111) ), //reduced runaway << tested
		new Vector3Bool(1972.338, 5417, -36.58504), //double
		new Vector3Bool(1962.555, 5411.252, -36.09483), //double
		new Vector3Bool(1935.463, 5358.436, -36.09454), //double
		new Vector3Bool(1900.282, 5353.309, -36.09468), //double
		new Vector3Bool(1900.918, 5389.098, -36.09454),
		new Vector3Bool(1834.456, 5396.291, -37.19594), //seer pickup
		new Vector3Bool(1856.885, 5385.996, -53.78625), //50 << tested
		new Vector3Bool(1826.736, 5392.343, -37.19562),
		new Vector3Bool(1812.133, 5392.964, -36.09425, ()=> Quest.GetQuestCompleted(43111) ), //room 3
		new Vector3Bool(1789.443, 5321.594, -38.00394, ()=> Quest.GetQuestCompleted(43111) ), //room 3
		new Vector3Bool(1731.39, 5317.35, -38.00394, ()=> Quest.GetQuestCompleted(43111) ), //room 3
		new Vector3Bool(1744.605, 5375.354, -38.53253, ()=> Quest.GetQuestCompleted(43111) ), //room 3
		new Vector3Bool(1803.98, 5333.153, -36.64657, ()=> Quest.GetQuestCompleted(43111) ), //room 3
		new Vector3Bool(1785.183, 5321.631, -38.53296, ()=> Quest.GetQuestCompleted(43111) ), //room 3
		new Vector3Bool(1773.17, 5307.513, -38.00289, ()=> Quest.GetQuestCompleted(43111) ), //room 3
		new Vector3Bool(1761.726, 5306.701, -38.53195, ()=> Quest.GetQuestCompleted(43111) ), //room 3
		new Vector3Bool(1722.285, 5350.117, -38.00419, ()=> Quest.GetQuestCompleted(43111) ), //room 3
		new Vector3Bool(1736.728, 5365.94, -38.52919, ()=> Quest.GetQuestCompleted(43111) ), //room 3
		new Vector3Bool(1722.773, 5348.743, -38.00377, ()=> Quest.GetQuestCompleted(43111) ), //room 3
		new Vector3Bool(1803.069, 5332.116, -36.64614, ()=> Quest.GetQuestCompleted(43111) ), //room 3
		new Vector3Bool(1731.382, 5317.235, -38.00378, ()=> Quest.GetQuestCompleted(43111) ), //room 3
		/*
		new Vector3Bool(, ()=> Quest.GetQuestCompleted(43111) ), //room 3
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		new Vector3Bool(),
		//*/
	};
	/*
Var.SetVar("Camelot10WitheredArmy", 43);
//*/
	List<int> _searchIDs = new List<int>()
	{
		//npc
		110141,
		//objects
		251954,
		251777,
		251727,
		251778,
		251739,
		251728,
		251665,
		251742,
		251730,
		251781,
		251741,
		251743,
		251783,
		251744,
		251725,
		251779,
		251726,
		259239,
		251740,
		251732,
		251724,
		251666,
		110041,
	};
	public WitheredArmy()
	{
		Name = "Withered Army Training";
		QuestId.Add(43943);
		Step.AddRange(new[] { 0 });
		/*
		Var.SetVar("Camelot10WitheredArmy", 18);
		//*/
	}
	static void Log(string text)
	{
		Logging.Write("[Withered Army] " + text);
	}
	public override bool Pulse()
	{
		var stepNumber = StepNumber;
		if (stepNumber < 0 || stepNumber >= _path.Count)
		{
			Log("path complete");
			Thread.Sleep(10 * 1000);
			return true;
		}
		var p = _path[stepNumber];
		Log("to point #" + stepNumber);
		if (CheckPoint(p))
		{
			Log("point complete #" + stepNumber);
			StepNumber += 1;
		}
		return true;
	}
	bool CheckPoint(Vector3Bool p)
	{
		var mob = Mob();
		if (mob != null)
		{
			Fight.StartFight(mob.Guid);
			return false;
		}
		if (p.Condition != null && !p.Condition())
			return true;

		if (GoToTask.ToPosition(p))
		{
			var unit = UnitAtPoint(p);
			if (unit != null)
			{
				Log("interact unit: " + unit.Name);
				GoToTask.ToPositionAndIntecractWithNpc(unit.Position, unit.Entry, 1);
				return false;
			}
			var obj = ObjectAtPoint(p);
			if (obj != null)
			{
				if (GoToTask.ToPositionAndIntecractWithGameObject(obj.Position, obj.Entry, 1))
				{
				}
				obj = ObjectAtPoint(p);
				if (obj != null && obj.IsValid && obj.CanOpen)
				{
					Log("interact2 obj: " + obj.Name + " ");
					Questing.Gather(obj.Position, obj.Entry, 10);
					Thread.Sleep(Usefuls.Latency * 2);
					Quest.SelectGossipOption(1);
					Usefuls.WaitIsCasting();
				}
				return true;
			}
			return true;
		}
		return false;
	}
	WoWUnit Mob()
	{
		return ObjectManager.GetWoWUnitHostile()
			.OrderBy(u => u.GetDistance)
			.FirstOrDefault(target => target.IsValid
				&& target.IsAlive
				&& target.GetDistance <= 20
			);
	}
	WoWGameObject ObjectAtPoint(Vector3 p)
	{
		return ObjectManager.GetWoWGameObjectByEntry(_searchIDs)
			.OrderBy(o => o.Position.DistanceTo(p))
			.FirstOrDefault(obj => obj.IsValid
				&& obj.Position.DistanceTo(p) <= _collectRange
		);
	}
	WoWUnit UnitAtPoint(Vector3 p)
	{
		return ObjectManager.GetWoWUnitByEntry(_searchIDs)
			.OrderBy(u => u.Position.DistanceTo(p))
			.FirstOrDefault(target => target.IsValid
				&& target.IsAlive
				&& target.Position.DistanceTo(p) <= _collectRange
				&& target.Faction == _buddyFaction
		);
	}
	int StepNumber
	{
		get
		{
			return Var.GetVar<int>(STEP_VAR_NAME);
		}
		set
		{
			Var.SetVar(STEP_VAR_NAME, value);
		}
	}
	public override bool IsComplete()
	{
		return Questing.Scenario.Stage != 2;
	}
	public override bool HasQuest()
	{
		return true;
	}
	public override bool IsCompleted()
	{
		return false;
	}
	public delegate bool BoolDelegate();

	public class Vector3Bool : Vector3
	{
		public BoolDelegate Condition = () => true;
		public Vector3Bool()
		{
		}
		public Vector3Bool(double x, double y, double z, BoolDelegate condition = null)
		{
			X = (float)x;
			Y = (float)y;
			Z = (float)z;
			//Type = type;
			if (condition != null)
				Condition = condition;
		}
	}

	#region HELPER

	static Thread _thread = null;
	static int _threadDelay = 500;

	public static void Start()
	{
		Stop();
		_thread = new Thread(() =>
		{
			while (robotManager.Products.Products.IsStarted)
			{
				if (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause)
				{
					if (HasMayhem && !IsToughAttackingMe)
						ToggleMayhem();
					else if (!HasMayhem && IsToughAttackingMe)
						ToggleMayhem();

					if (NearestBuddy != null)
						FetchBuddy();
				}
				Thread.Sleep(_threadDelay);
			}
		});
		_thread.Start();
		wManager.wManagerSetting.ClearBlacklistOfCurrentProductSession();
		wManager.Events.FightEvents.OnFightLoop += OnFightLoop;
		robotManager.Events.ProductEvents.OnProductStopping += OnProductStop;
		wManager.Events.MovementEvents.OnMovementPulse += OnMovementPulse;
	}
	static void OnMovementPulse(List<Vector3> points, System.ComponentModel.CancelEventArgs cancelable)
	{
		var continent = (ContinentId)Usefuls.ContinentId;
		for (int i = 0; i < points.Count; i++)
		{
			var p = points[i];
			if (i < points.Count - 1)
			{
				var p2 = points[i + 1];
				foreach (var pinsert in pathInsert)
				{
					if (p != null && pinsert.Item1 == continent && p.DistanceTo(pinsert.Item2) <= pinsert.Item3 && p2.DistanceTo(pinsert.Item4) <= pinsert.Item5)
					{
						Log("path insert of " + p + " to " + p2);
						points.RemoveRange(i + 1, points.Count - i - 1);
						points.AddRange(pinsert.Item6);
						return;
					}
				}
			}
			foreach (var pchange in positionChange)
			{
				if (p != null && pchange.Item1 == continent && p.DistanceTo(pchange.Item2) <= pchange.Item3)
				{
					Log("change path position of " + p + " to " + pchange.Item4);
					p.X = pchange.Item4.X;
					p.Y = pchange.Item4.Y;
					p.Z = pchange.Item4.Z;
					p.Type = pchange.Item4.Type;
					p.Action = pchange.Item4.Action;
				}

			}
		}
	}
	// Continent (item1), DefaultPosition (item2), DefaultPositionSearchRange (item3), NewPosition (item4)
	static List<Tuple<ContinentId, Vector3, float, Vector3, float, List<Vector3>>> pathInsert = new List<Tuple<ContinentId, Vector3, float, Vector3, float, List<Vector3>>>
	{
		//pass in cracked wall
		new Tuple<ContinentId, Vector3, float, Vector3, float, List<Vector3>>(
			ContinentId.TheCollapseSuramarScenario,
			//near point #1
			new Vector3(2300.772, 5428.239, 15.84146, "None"), 6,
			//next near point #2
			new Vector3(2292.563, 5428.934, 16.25397, "None"), 6,
			//insert this path
			new List<Vector3>()
			{
				new Vector3(2301.848f, 5427.985f, 15.84509f, "None"),
				new Vector3(2302.938f, 5421.325f, 14.89918f, "None"),
				new Vector3(2303.502f, 5417.873f, 14.52616f, "None"),
				new Vector3(2303.559f, 5414.371f, 14.51589f, "None"),
				new Vector3(2299.622f, 5409.054f, 15.23258f, "None"),
				new Vector3(2296.333f, 5407.828f, 16.62882f, "None"),
				new Vector3(2292.96f, 5408.423f, 17.49141f, "None"),
				new Vector3(2289.522f, 5414.3f, 18.92938f, "None"),
				new Vector3(2288.443f, 5417.518f, 20.24163f, "None"),
				new Vector3(2287.423f, 5420.814f, 18.51188f, "None"),
				new Vector3(2286.646f, 5424.28f, 16.22258f, "None"),
				new Vector3(2287.192f, 5427.724f, 15.56582f, "None"),
			}
		),
		//pass in cracked wall (reverse)
		new Tuple<ContinentId, Vector3, float, Vector3, float, List<Vector3>>(
			ContinentId.TheCollapseSuramarScenario,
			//near point #1
			new Vector3(2292.563, 5428.934, 16.25397, "None"), 7,
			//next near point #2
			new Vector3(2300.772, 5428.239, 15.84146, "None"), 7,
			//insert this path
			new List<Vector3>()
			{
				new Vector3(2287.192f, 5427.724f, 15.56582f, "None"),
				new Vector3(2286.646f, 5424.28f, 16.22258f, "None"),
				new Vector3(2287.423f, 5420.814f, 18.51188f, "None"),
				new Vector3(2288.443f, 5417.518f, 20.24163f, "None"),
				new Vector3(2289.522f, 5414.3f, 18.92938f, "None"),
				new Vector3(2292.96f, 5408.423f, 17.49141f, "None"),
				new Vector3(2296.333f, 5407.828f, 16.62882f, "None"),
				new Vector3(2299.622f, 5409.054f, 15.23258f, "None"),
				new Vector3(2303.559f, 5414.371f, 14.51589f, "None"),
				new Vector3(2303.502f, 5417.873f, 14.52616f, "None"),
				new Vector3(2302.938f, 5421.325f, 14.89918f, "None"),
				new Vector3(2301.848f, 5427.985f, 15.84509f, "None"),
			}
		),
	};
	public static void Stop()
	{
		if (_thread != null)
		{
			_thread.Abort();
			_thread = null;
			Log("combat stop");
		}
		wManager.Events.FightEvents.OnFightLoop -= OnFightLoop;
		robotManager.Events.ProductEvents.OnProductStopping -= OnProductStop;
		wManager.Events.MovementEvents.OnMovementPulse -= OnMovementPulse;
	}
	static void OnFightLoop(WoWUnit unit, System.ComponentModel.CancelEventArgs cancelable)
	{
		if (NearestBuddy != null)
			cancelable.Cancel = true;
	}
	public static bool IsToughAttackingMe
	{
		get
		{
			var me = ObjectManager.Me;
			var target = ObjectManager.Target;
			if (me.InCombat && target.IsValid && target.Reaction <= Reaction.Hostile && (target.IsElite || target.Entry == 110771))
			{
				return true;
			}
			int count = 0;
			var mobs = ObjectManager.GetWoWUnitHostile().Where(u => u.IsTargetingMe && (u.IsElite || u.Entry == 110771));
			try
			{
				count = Enumerable.Count(mobs);
			}
			catch (Exception e) { }
			return count > 0;
		}
	}
	public static void ToggleMayhem()
	{
		Lua.LuaDoString("ExtraActionButton1:Click();");
	}
	public static bool HasMayhem
	{
		get
		{
			return ObjectManager.Me.HaveBuff(216937);
		}
	}
	public static bool FetchBuddy()
	{
		WoWUnit unit = NearestBuddy;
		if (unit != null)
		{
			Log("found buddy " + unit.Name);
			Conditions.ForceIgnoreIsAttacked = true;
			MovementManager.StopMove();
			Fight.StopFight();
			var timer = new robotManager.Helpful.Timer(15 * 1000);
			while (unit != null && unit.IsValid && !timer.IsReady)
			{
				if (unit.GetDistance > 4)
				{
					Log("fetch buddy " + unit.Name);
					var path = PathFinder.FindPath(unit.Position);
					MovementManager.Go(path);
					Thread.Sleep(200);
					continue;
				}
				Log("calm buddy " + unit.Name);
				Interact.InteractGameObject(unit.GetBaseAddress, true);
				Usefuls.WaitIsCasting();
				break;
			}
		}
		Conditions.ForceIgnoreIsAttacked = false;
		return false;
	}
	public static WoWUnit NearestBuddy
	{
		get
		{
			return ObjectManager.GetObjectWoWUnit()
				.OrderBy(u => u.GetDistance)
				.FirstOrDefault(target => target.IsValid
					&& target.IsAlive
					//&& target.GetDistance <= 30
					//&& target.Faction == 1665
					&& target.HaveBuff(_buffScared)
				);
		}
	}

	//ContinentId = 1626
	//AreaId = 7637
	//ingame mapid = 1007
	public static bool InScenario
	{
		get
		{
			return Usefuls.ContinentId == (int)ContinentId.TheCollapseSuramarScenario;
		}
	}
	#endregion HELPER

	static void OnProductStop(string productName)
	{
		Stop();
	}

	// Continent (item1), DefaultPosition (item2), DefaultPositionSearchRange (item3), NewPosition (item4)
	static List<Tuple<ContinentId, Vector3, float, Vector3>> positionChange = new List<Tuple<ContinentId, Vector3, float, Vector3>>
	{
		//pass in cracked wall
		//new Tuple<ContinentId, Vector3, float, Vector3>(ContinentId.TheCollapseSuramarScenario, new Vector3(2294.032, 5430.879, 16.96338, "None"), 4f, new Vector3(2300.819, 5410.238, 14.98664, "None")),
		//new Tuple<ContinentId, Vector3, float, Vector3>(ContinentId.TheCollapseSuramarScenario, new Vector3(2290.22, 5432.098, 15.53407, "None"), 2f, new Vector3(2286.877, 5414.85, 19.948, "None")),
	};

	#region FINISH

	static Vector3 _rewardPosition = new Vector3(2106.301, 5393.583, 48.79777, "None");
	static List<int> _rewardsID = new List<int>()
	{
		251668, 251669, 251745, 251746, 251747, 251748, 251753, 251754, 251755, 251756, 251757, 251758, 251759, 252452, 251749
	};
	static List<uint> _rewardsItemID = new List<uint>()
	{
		140448, //+25% damage / Lens of Qin'dera
		140450, //Berseker / Berserking Helm of Taenna
		139011, //Berseker / Berserking Helm of Ondry'el
		139018, //+Damage / Box of Calming Whispers
		140260, //rep items
		141870, //rep items
		147416, //+100 rep
		147418, //+24rep
	};
	public static bool Finish()
	{
		if (ObjectManager.Me.Position.DistanceTo(_rewardPosition) > 50)
		{
			Log("to reward place");
			GoToTask.ToPosition(_rewardPosition);
			return true;
		}
		var reward = ObjectManager.GetNearestWoWGameObject(ObjectManager.GetWoWGameObjectByEntry(_rewardsID));
		if (reward != null && reward.IsValid)
		{
			Log("collect reward: " + reward.Name);
			GoToTask.ToPositionAndIntecractWithGameObject(reward.Position, reward.Entry);
			return true;
		}
		if (UseAllRewardsItems())
			return true;

		Log("end scenario");
		var p = new Vector3(2098.582, 5395.352, 47.30122);
		GoToTask.ToPositionAndIntecractWithGameObject(p, 258976, 1);
		Questing.ClickStaticPopup(1);
		Thread.Sleep(Others.Random(3000, 4000));
		return false;
	}
	static bool UseAllRewardsItems()
	{
		if (UseRewardItem(139010, 43149))   //25% troop health
			return true;

		if (UseRewardItem(139017, 43111))   //reduce runaway
			return true;

		if (UseRewardItem(140451, 43146))   //mana-rager / Spellmask of Azsylla
			return true;

		if (UseRewardItem(139027, 43134))   //spellseer / Lenses of Spellseer Dellian
			return true;

		if (UseRewardItem(139019, 43128))   //mana-rager 2 / Spellmask of Alla'onus
			return true;

		if (UseRewardItem(139028, 43135))   //starcaller / Disc of the Starcaller
			return true;

		foreach (var itemID in _rewardsItemID)
		{
			if (UseRewardItem(itemID))
				return true;
		}
		return false;
	}
	static bool UseRewardItem(uint itemID, int questID = 0)
	{
		if (ItemsManager.HasItemById(itemID))
		{
			if (questID > 0 && !Quest.GetQuestCompleted(questID))
				return false;

			Log("use reward: " + ItemsManager.GetNameById(itemID));
			ItemsManager.UseItem(itemID);
			Usefuls.WaitIsCasting();
			return true;
		}

		return false;
	}
	#endregion FINISH

}
