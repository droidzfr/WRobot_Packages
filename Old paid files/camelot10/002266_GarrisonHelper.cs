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

public class GarrisonHelper
{
	public GarrisonHelper()
	{
		ResetSettings();
	}
	public class ItemID
	{
		public static uint DeadlyIronTrap = 115010;
		public static uint Hearthstone = 110560;
		public static uint PrimalSpirit = 120945;
		public static uint DraenicStone = 115508;
		public static uint DraenicSeeds = 116053;
		public static uint Frostweed = 109124;
		public static uint SumptuousFur = 111557;
		public static uint CagedMightyWolf = 119815;
		public static uint CagedMightyClefthoof = 119819;
		public static uint CagedMightyRiverbeast = 119817;
		public static uint FurryCagedBeast = 119813;
		public static uint LeatheryCagedBeast = 119814;
		public static uint MeatyCagedBeast = 119810;
		public static uint PreservedMiningPick = 118903;
		public static uint MinersCoffee = 118897;
		public static List<uint> CagesForBarn = new List<uint>() { CagedMightyWolf, CagedMightyClefthoof, CagedMightyRiverbeast, FurryCagedBeast, LeatheryCagedBeast, MeatyCagedBeast };
	}
	public class ObjectID
	{
		public static List<int> ResourcesCache = new List<int>() { /* Garrison Cache */ 236916, 237191, /* Hefty Garrison Cache */ 237723, 237720, /* Full Garrison Cache */ 237724, 237722, };
		public static List<int> WorkOrdersGarden = new List<int>() { 239238, };
		public static List<int> WorkOrdersMine = new List<int>() { 239237, };
		public static List<int> WorkOrdersAlchemy = new List<int>() { 235892, 237120, };
		public static List<int> WorkOrdersBarn = new List<int>() { 238761, };
		public static List<int> WorkOrdersTailoring = new List<int>() { 237665, };
		public static List<int> WorkOrdersTradingPost = new List<int>() { 237027, };
		public static List<int> OreDeposits = new List<int>()
		{
			228493, 237358, 243314, 232544, 230428, 228510, 237357, 243315, 232545, 228564, 237360, 243312, 232543, 228563, 237359, 243313, 232542, 232541 /*Mine Cart*/
		};
		public static List<int> HerbNodes = new List<int>()
		{
			228572, 237396, 235387, 233117, 228571, 237398, 235376, 228573, 237402, 235388, 228575, 237406, 235390, 228574, 237404, 235389, 243334, 228576, 237400, 235391, 
		};
		public static List<int> EmptyDisplayID = new List<int>() { 20508, 13845, };
	}
	public class Positions
	{
		//horde
		public static Vector3 CenterHorde1 = CenterHorde2;
		public static Vector3 CenterHorde2 = CenterHorde3;
		public static Vector3 CenterHorde3 = new Vector3(5595.898, 4527.203, 125.9193, "None");
		public static Vector3 GardenHorde2 = GardenHorde3;
		public static Vector3 GardenHorde3 = new Vector3(5415.472, 4550.596, 139.1243, "None");
		public static Vector3 MineHorde2 = MineHorde3;
		public static Vector3 MineHorde3 = new Vector3(5472.465, 4443.883, 144.7026, "None");
		//alliance
		public static Vector3 CenterAlliance1 = new Vector3(1860.227, 229.286, 76.55647, "None");
		public static Vector3 CenterAlliance2 = new Vector3(1885.368, 267.5734, 76.64108, "None");
		public static Vector3 CenterAlliance3 = CenterAlliance2;
		public static Vector3 GardenAlliance2 = new Vector3(1843.559, 151.6545, 77.9702, "None");
		public static Vector3 GardenAlliance3 = GardenAlliance2;
		public static Vector3 MineAlliance2 = new Vector3(1899.422, 90.16446, 83.52786, "None");
		public static Vector3 MineAlliance3 = MineAlliance2;

		public static Vector3 Center
		{
			get
			{
				var level = Level;
				if (ObjectManager.Me.IsHorde)
				{
					if (level >= 3)
						return CenterHorde3;
					else if (level == 2)
						return CenterHorde2;
					return CenterHorde1;
				}
				else
				{
					if (level >= 3)
						return CenterAlliance3;
					else if (level == 2)
						return CenterAlliance2;
					return CenterAlliance1;
				}
			}
		}
		public static Vector3 Garden
		{
			get
			{
				if (ObjectManager.Me.IsHorde)
					return Level >= 3 ? GardenHorde3 : GardenHorde2;
				else
					return Level >= 3 ? GardenAlliance3 : GardenAlliance2;
			}
		}
		public static Vector3 Mine
		{
			get
			{
				if (ObjectManager.Me.IsHorde)
					return Level >= 3 ? MineHorde3 : MineHorde2;
				else
					return Level >= 3 ? MineAlliance3 : MineAlliance2;
			}
		}
	}
	public static bool HaveHearthStone
	{
		get
		{
			return ItemsManager.HasItemById(ItemID.Hearthstone);
		}
	}
	public static bool ToGarrison()
	{
		if (InGarrison)
		{
			Log("Im in Garrison");
			return true;
		}
		var hearthstoneCD = Lua.LuaDoString<int>("local startTime, duration, enable = GetItemCooldown(" + ItemID.Hearthstone + "); return startTime + duration - GetTime();");
		if (hearthstoneCD > 0)
			return false;

		if (ObjectManager.Me.IsMounted)
			MountTask.DismountMount();

		Log("To Garrison, use hearthstone");
		ItemsManager.UseItem(ItemID.Hearthstone);
		Usefuls.WaitIsCasting();
		return false;
	}
	public static bool InDraenor
	{
		get
		{
			if (InGarrison)
				return true;
			return Usefuls.ContinentId == (int) ContinentId.Draenor;
		}
	}
	public static bool InGarrisonMapID
	{
		get
		{
			var currentMapID = Lua.LuaDoString<int>("local mapID, isContinent = GetCurrentMapAreaID(); return mapID;");
			return MapID.GarrisonMaps.Contains(currentMapID);
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
	public static int Level { get { return Lua.LuaDoString<int>("local garrisonLevel, mapTexture, townHallX, townHallY = C_Garrison.GetGarrisonInfo(LE_GARRISON_TYPE_6_0); return garrisonLevel;"); } }
	public static int Resources { get { return Lua.LuaDoString<int>("local name, amount, texturePath, earnedThisWeek, weeklyMax, totalMax, isDiscovered, quality = GetCurrencyInfo('currency: 824'); return amount;"); } }
	public static bool InMine { get { return NearMine(ObjectManager.Me.Position); } }
	public static bool InGarden { get { return NearGarden(ObjectManager.Me.Position); } }
	public static bool InCenter{ get { return NearCenter(ObjectManager.Me.Position); } }
	public static bool NearCenter(Vector3 p)
	{
		return p.DistanceTo2D(Positions.Center) < 150;
	}
	public static bool NearMine(Vector3 p)
	{
		return p.DistanceTo2D(Positions.Mine) < 150;
	}
	public static bool NearGarden(Vector3 p)
	{
		return p.DistanceTo2D(Positions.Garden) < 50;
	}
	public class MapID
	{
		public static int Horde1 = 1152;
		public static int Horde2 = 1330;
		public static int Horde3 = 1153;
		public static int Horde4 = 1154;
		public static int Alliance1 = 1158;
		public static int Alliance2 = 1331;
		public static int Alliance3 = 1159;
		public static int Alliance4 = 1160;
		public static List<int> GarrisonMaps = new List<int>() { Horde1, Horde2, Horde3, Horde4, Alliance1, Alliance2, Alliance3, Alliance4, };
	}
	static void ResetSettings()
	{
		wManager.wManagerSetting.CurrentSetting.CloseIfPlayerTeleported = false;
		wManager.wManagerSetting.CurrentSetting.SearchRadius = 200;
		wManager.wManagerSetting.CurrentSetting.HarvestHerbs = false;
		wManager.wManagerSetting.CurrentSetting.HarvestMinerals = false;
		wManager.wManagerSetting.CurrentSetting.HarvestTimber = false;
		Log("reset settings");
	}
	static void Log(string text)
	{
		Logging.WriteDebug("[Garrison Helper] " + text);
	}

	public class GarrisonQuest : QuestGathererClass
	{
		protected bool _complete = false;
		protected List<int> _workers = new List<int>();
		public GarrisonQuest()
		{
			Name = "Garrison Quest";
			QuestId.Add(0);
			Step.Add(0);
			_complete = false;
		}
		protected void Log(string text)
		{
			Logging.WriteDebug("[" + Name + "] " + text);

		}
		public virtual bool InArea()
		{
			if (HotSpots.Count < 1)
				return true;
			return ObjectManager.Me.Position.DistanceTo(HotSpots[0]) < 100;
		}
		public virtual void ToArea()
		{
			if (HotSpots.Count < 1)
				return;
			GoToTask.ToPosition(HotSpots[0]);
		}
		public virtual bool CanWorkOrder()
		{
			return false;
			//return ItemsManager.GetItemCountByIdLUA(12345) > 5;
		}
		public override bool Pulse()
		{
			//goto
			if (!InArea())
			{
				Log("move to area");
				ToArea();
				return true;
			}
			//gather
			var node = ObjectManager.GetNearestWoWGameObject(ObjectManager.GetWoWGameObjectByEntry(EntryIdObjects), false, false);
			if (node != null && node.IsValid && !ObjectID.EmptyDisplayID.Contains(node.DisplayId))
			{
				//return base.Pulse();
				Log("need to gather " + node.Name);
				if (GoToTask.ToPosition(node.Position))
				{
					MountTask.DismountMount();
					GoToTask.ToPositionAndIntecractWithGameObject(node.Position, node.Entry);
				}
				return true;
			}

			//work orders
			if (CanWorkOrder())
			{
				Log("need to start work order");
				CreateWorkOrders();
				return true;
			}
			Log("complete");
			_complete = true;
			return true;
		}
		public virtual bool CreateWorkOrders()
		{
			var npc = ObjectManager.GetNearestWoWUnit(ObjectManager.GetWoWUnitByEntry(_workers));
			if (npc != null && npc.IsValid)
			{
				GoToTask.ToPositionAndIntecractWithNpc(npc.Position, npc.Entry);
				var gui = "GarrisonCapacitiveDisplayFrame.CreateAllWorkOrdersButton";
				if (Lua.LuaDoString<bool>("return " + gui + " and " + gui + ":IsVisible()"))
				{
					Thread.Sleep(Usefuls.Latency * 2);
					Lua.LuaDoString(gui + ":Click();");
					Thread.Sleep(Usefuls.Latency * 2);
					_complete = true;
				}
				return true;
			}
			return false;
		}
		public override bool IsComplete()
		{
			return _complete;
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

	/*
	== draenor
MapZoneName = Долина Призрачной Луны
SubMapZoneName = Долина Лунных Цветов
ContinentNameMpq = Draenor
ContinentId = 1116
AreaId = 6719

	==Alliance garrison1
	ContinentId = 1158
	AreaId = 7078

	==alliance garrison2
	ContinentNameMpq = SMVAllianceGarrisonLevel2new
	ContinentId = 1331
	AreaId = 7078

	//*/
}