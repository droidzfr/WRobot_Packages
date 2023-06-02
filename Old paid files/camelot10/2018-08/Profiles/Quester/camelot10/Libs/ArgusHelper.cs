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

public class ArgusHelper
{
	//map id: macaree=1170, antoranwastes=1171, krokuun=1184
	//map id vendicar: krokuun 1135, antoran wastes 1171,
	//area id. krokuun 8574, antoranwastes 8899, 
	public static int AREA_KROKUUN = 8574;
	public static int AREA_ANTORANWASTES = 8899;
	public static int AREA_MACAREE = 8701;
	public static int MAP_KROKUUN = 1184;
	public static int MAP_ANTORAN_WASTES = 1171;
	public static int MAP_MACAREE = 1170;
	//
	static int DalaranPortalID = 273784; //273784 // 273784
										 //
	static int AntoranWastesVeiledDen = 48202; // http://www.wowhead.com/quest=48202/reinforce-the-veiled-den or http://www.wowhead.com/quest=48929/sizing-up-the-opposition
	static int AntoranWastesLightsPurchase = 48201; // http://www.wowhead.com/quest=48201/reinforce-lights-purchase or http://www.wowhead.com/quest=47473/sizing-up-the-opposition

	static ArgusHelper()
	{
		ResetSettings();
		System.Threading.Tasks.Task.Delay(50).ContinueWith(t => {
			FixNavigation();
		});
	}
	public static bool InArgus { get { return Usefuls.ContinentId == (int)ContinentId.Argus_1; } }
	public static bool ToArgus()
	{
		if (InArgus)
		{
			Log("im in Argus (to Argus)");
			return true;
		}
		if (LegionQuests.InDalaran)
		{
			Log("im in Dalaran, use teleporter to Argus (to Argus)");
			Teleporter.DalaranToArgus.Use();
			return false;
		}
		Log("need to get into Dalaran (to Argus)");
		LegionQuests.UseDalaranHeathstone();
		return false;
	}
	public static bool FromArgus()
	{
		if (!InArgus)
		{
			Log("im not in Argus (from Argus)");
			return true;
		}
		if (InVendicarAntoranWastes)
		{
			Log("im in Vendicar (Anrotan Wastes), use portal to Dalaran (from Argus)");
			GoToTask.ToPositionAndIntecractWithGameObject(Positions.DalaranPortalAntoranWastes, DalaranPortalID);
			return false;
		}
		if (InVendicarKrokuun)
		{
			Log("im in Vendicar (Krokuun), use portal to Dalaran (from Argus)");
			GoToTask.ToPositionAndIntecractWithGameObject(Positions.DalaranPortalKrokuun, DalaranPortalID);
			return false;
		}
		if (InVendicarMacAree)
		{
			Log("im in Vendicar (Mac'Aree), use portal to Dalaran (from Argus)");
			GoToTask.ToPositionAndIntecractWithGameObject(Positions.DalaranPortalMacAree, DalaranPortalID);
			return false;
		}
		Log("im not in Vendicar, move there (from Argus)");
		ToVendicar();
		return false;
	}
	public static bool InVendicarKrokuun
	{
		get
		{
			return InKrokuun && ObjectManager.Me.Position.DistanceTo(Positions.KrokuunVendicar) < 100;
		}
	}
	public static bool ToVendicarKrokuun()
	{
		if (InVendicarKrokuun)
		{
			Log("im in Vendicar in Krokuun (To Vendicar Krokuun)");
			return true;
		}
		if (InKrokuunOpen)
		{
			Log("im in Krokuun open world. Teleport to Vendicar (To Vendicar Krokuun)");
			Teleporter.VendicarKrokuun.To();
			return false;
		}
		Log("im not in Krokuun. Travel there (To Vendicar Krokuun)");
		ToKrokuun();
		return false;
	}
	public static bool InVendicarAntoranWastes
	{
		get
		{
			return InAntoranWastes && ObjectManager.Me.Position.DistanceTo(Positions.AntoranWastesVendicar) < 100;
		}
	}
	public static bool ToVendicarAntoranWastes()
	{
		if (InVendicarAntoranWastes)
		{
			Log("im in Vendicar in Antoran Wastes (To Vendicar Antoran Wastes)");
			return true;
		}
		if (InAntoranWastesOpen)
		{
			Log("im in Antoran Wastes open world. Teleport to Vendicar (To Vendicar Antoran Wastes)");
			Teleporter.VendicarAntoranWastes.To();
			return false;
		}
		Log("im not in Antoran Wastes. Travel there (To Vendicar Antoran Wastes)");
		ToAntoranWastes();
		return false;
	}
	public static bool InVendicarMacAree
	{
		get
		{
			return InMacAree && ObjectManager.Me.Position.DistanceTo(Positions.MacAreeVendicar) < 100;
		}
	}
	public static bool ToVendicarMacAree()
	{
		if (InVendicarMacAree)
		{
			Log("im in Vendicar in Mac'Aree (To Vendicar Mac'Aree)");
			return true;
		}
		if (InMacAreeOpen)
		{
			Log("im in Mac'Aree open world. Teleport to Vendicar (To Vendicar Mac'Aree)");
			Teleporter.VendicarMacAree.To();
			return false;
		}
		Log("im not in Mac'Aree. Travel there (To Vendicar Mac'Aree)");
		ToMacAree();
		return false;
	}
	public static bool InVendicar
	{
		get
		{
			return InVendicarKrokuun || InVendicarAntoranWastes || InVendicarMacAree;
		}
	}
	public static bool ToVendicar()
	{
		if (InVendicar)
		{
			Log("im on Vendicar (To Vendicar)");
			return true;
		}
		if (InKrokuunOpen)
		{
			Log("im in Krokuun open world, teleport to Vendicar (To Vendicar)");
			Teleporter.VendicarKrokuun.To();
			return false;
		}
		if (InAntoranWastesOpen)
		{
			Log("im in Antoran Wastes open world, teleport to Vendicar (To Vendicar)");
			Teleporter.VendicarAntoranWastes.To();
			return false;
		}
		if (InMacAreeOpen)
		{
			Log("im in Mac'Aree open world, teleport to Vendicar (To Vendicar)");
			Teleporter.VendicarMacAree.To();
			return false;
		}
		Log("im not in Argus, travel there (To Vendicar)");
		ToArgus();
		return false;
	}
	public static bool InKrokuun
	{
		get
		{
			return InArgus && Usefuls.AreaId == AREA_KROKUUN;
		}
	}
	public static bool ToKrokuun()
	{
		if (InKrokuun)
		{
			Log("im in Krokuun (To Krokuun)");
			return true;
		}
		if (InVendicar)
		{
			Log("im in Vendicar, teleport to Krokuun Vendicar (To Krokuun)");
			Teleporter.VendicarKrokuun.To();
			return false;
		}
		Log("im not in Vendicar, teleport Vendicar (To Krokuun)");
		ToVendicar();
		return false;
	}
	public static bool InKrokuunOpen
	{
		get
		{
			return InKrokuun && !InVendicarKrokuun;
		}
	}
	public static bool ToKrokuunOpen()
	{
		if (InKrokuunOpen)
		{
			Log("im in krokuun open (To Krokuun Open)");
			return true;
		}
		if (InVendicar)
		{
			Log("im in Vendicar, teleport to Krokul Hovel (To Krokuun Open)");
			Teleporter.KrokulHovel.To();
			return false;
		}
		Log("not in Vendicar. travel there (To Krokuun Open)");
		ToVendicar();
		return false;
	}
	public static bool InAntoranWastes
	{
		get
		{
			return InArgus && Usefuls.AreaId == AREA_ANTORANWASTES;
		}
	}
	public static bool ToAntoranWastes()
	{
		if (InAntoranWastes)
		{
			Log("im in Antoran Wastes (To Antoran Wastes)");
			return true;
		}
		if (InVendicar)
		{
			Log("im in Vendicar, teleport to Antoran Wastes Vendicar (To Antoran Wastes)");
			Teleporter.VendicarAntoranWastes.To();
			return false;
		}
		Log("im not in Vendicar, teleport Vendicar (To Antoran Wastes)");
		ToVendicar();
		return false;
	}
	public static bool InAntoranWastesOpen
	{
		get
		{
			return InAntoranWastes && !InVendicarAntoranWastes;
		}
	}
	public static bool ToAntoranWastesOpen()
	{
		if (InAntoranWastesOpen)
		{
			Log("im in Antoran Wastes (To Antoran Wastes Open)");
			return true;
		}
		if (InVendicar)
		{
			Log("im in Vendicar, teleport to Hope's Landing (To Antoran Wastes Open)");
			Teleporter.HopesLanding.To();
			return false;
		}
		Log("not in Vendicar. travel there (To Antoran Wastes Open)");
		ToVendicar();
		return false;
	}
	public static bool OpenedVeiledDen
	{
		get
		{
			return Quest.GetQuestCompleted(AntoranWastesVeiledDen);
		}
	}
	public static bool OpenedLightsPurchase
	{
		get
		{
			return Quest.GetQuestCompleted(AntoranWastesLightsPurchase);
		}
	}
	public static bool InMacAree
	{
		get
		{
			return InArgus && Usefuls.AreaId == AREA_MACAREE;
		}
	}
	public static bool ToMacAree()
	{
		if (InMacAree)
		{
			Log("im in Mac'Aree (To Mac'Aree)");
			return true;
		}
		if (InVendicar)
		{
			Log("im in Vendicar, teleport to Mac'Aree Vendicar (To Mac'Aree)");
			Teleporter.VendicarMacAree.To();
			return false;
		}
		Log("im not in Vendicar, teleport Vendicar (To Mac'Aree)");
		ToVendicar();
		return false;
	}
	public static bool InMacAreeOpen
	{
		get
		{
			return InMacAree && !InVendicarMacAree;
		}
	}
	public static bool ToMacAreeOpen()
	{
		if (InMacAreeOpen)
		{
			Log("im in Mac'Aree (To Mac'Aree Open)");
			return true;
		}
		if (InVendicar)
		{
			Log("im in Vendicar, teleport to Triumvirate's End (To Mac'Aree Open)");
			Teleporter.TriumviratesEnd.To();
			return false;
		}
		Log("not in Vendicar. travel there (To Mac'Aree Open)");
		ToVendicar();
		return false;
	}
	//S from Krokul Hovel
	public static bool InKrokuunDarkfallRidge
	{
		get
		{
			var myPos = ObjectManager.Me.Position;
			return InKrokuunOpen && myPos.DistanceTo2D(Positions.DarkfallRidgeCenter) < 220 && myPos.DistanceZ(Positions.DarkfallRidgeCenter) < 50;
		}
	}
	public static bool InKrokuunNathraxasHold
	{
		get
		{
			var myPos = ObjectManager.Me.Position;
			return InKrokuunOpen && myPos.X > Positions.NathraxasHoldInside.X;
		}
	}
	public static bool InKrokuunShatteredFieldsCliff
	{
		get
		{
			var myPos = ObjectManager.Me.Position;
			return InKrokuunOpen && myPos.DistanceTo2D(Positions.ShatteredFieldsCliffInside) < 70;
		}
	}

	#region POSITIONS
	public static class Positions
	{
		public static Vector3 KrokuunVendicar = new Vector3(458.5647, 1449.929, 757.573, "None");
		public static Vector3 AntoranWastesVendicar = new Vector3(-2623.374, 8653.992, -79.04244, "None");
		public static Vector3 MacAreeVendicar = new Vector3(4682.881, 9851.88, 56.06652, "None");
		public static Vector3 DalaranPortalKrokuun = new Vector3(498.9296, 1467.982, 742.4532, "None");
		public static Vector3 DalaranPortalAntoranWastes = new Vector3(-2634.677, 8697.06, -94.16268, "None");
		public static Vector3 DalaranPortalMacAree = new Vector3(4725.081, 9861.455, 40.94718, "None");
		//krokuun
		public static Vector3 DarkfallRidgeCenter = new Vector3(734.3407, 1630.395, 588.4875, "None");
		public static Vector3 DarkfallRidgeOutSide = new Vector3(970.46, 1623.67, 533.5179, "None");
		public static Vector3 NathraxasHoldInside = new Vector3(1535.4, 1491.447, 474.5933, "None");
		public static Vector3 NathraxasHoldOutside = new Vector3(1511.262, 1484.037, 483.1274, "None");
		public static Vector3 ShatteredFieldsCliffInside = new Vector3(770.8271, 2533.532, 369.8428, "None");
		public static Vector3 ShatteredFieldsCliffOutside = new Vector3(850.5693, 2518.001, 366.3449, "None");
		//antoran wastes
		public static Vector3 HopesLandingCrossroad = new Vector3(-2858.894, 8886.03, -209.7137, "None");
		public static bool InHopesLandingCrossroad(Vector3 p)
		{
			return p.DistanceTo(HopesLandingCrossroad) < 55f;
		}
	}
	#endregion

	#region TELEPORTERS
	public class Teleporter
	{
		public Vector3 position;
		public int entry;
		public ContinentId continent;
		public int taxiNode = -1;
		public string x;
		public string y;
		public int questID = -1;
		static string luaFindNode = @"
local needX = {0}
local needY = {1}
local needT = 'REACHABLE'
for i = 1,NumTaxiNodes() do
	local n = TaxiNodeName(i)
	local x, y = TaxiNodePosition(i)
	local t = TaxiNodeGetType(i)
	if (x and y and needX and needY and t) then
		if (abs(x-needX) < 0.001 and abs(y-needY) < 0.001 and t == needT) then
			return i;
		end
	end
end
return -1;
";

		public static Teleporter Vendicar
		{
			get
			{
				if (InVendicarKrokuun)
					return VendicarKrokuun;

				if (InVendicarAntoranWastes)
					return VendicarAntoranWastes;

				if (InVendicarMacAree)
					return VendicarMacAree;

				return VendicarKrokuun;
			}
		}
		public static Teleporter DalaranToArgus = new Teleporter()
		{
			entry = 121014,
			position = new Vector3(-850.5267, 4265.826, 746.2749, "None"),
			continent = ContinentId.Troll_Raid,
		};
		public static Teleporter VendicarKrokuun = new Teleporter()
		{
			entry = 123139,
			position = new Vector3(505.1614, 1471.857, 765.7972, "None"),
			continent = ContinentId.Argus_1,
			x = "0.44006",
			y = "0.52043",
		};
		public static Teleporter VendicarAntoranWastes = new Teleporter()
		{
			entry = 125514,
			position = new Vector3(-2636.905, 8703.858, -70.82, "None"),
			continent = ContinentId.Argus_1,
			x = "0.14538",
			y = "0.39255",
			questID = 48559, //http://www.wowhead.com/quest=48199/the-burning-heart
		};
		public static Teleporter VendicarMacAree = new Teleporter()
		{
			entry = 125461,
			position = new Vector3(4732.196, 9863.496, 64.28996, "None"),
			continent = ContinentId.Argus_1,
			x = "0.09799",
			y = "0.69274",
			questID = 47994, //http://www.wowhead.com/quest=47994/forming-a-bond
		};
		//krokuun
		public static Teleporter KrokulHovel = new Teleporter()
		{
			entry = 118830,
			position = new Vector3(986.6412, 1709.364, 516.9945, "None"),
			continent = ContinentId.Argus_1,
			x = "0.43022",
			y = "0.54018",
			questID = 46816, //http://www.wowhead.com/quest=46816/rendezvous
		};
		public static Teleporter ShatteredFields = new Teleporter()
		{
			entry = 123260,
			position = new Vector3(1082.926, 2270.06, 408.4375, "None"),
			continent = ContinentId.Argus_1,
			x = "0.40723",
			y = "0.54406",
		};
		public static Teleporter DestinyPoint = new Teleporter()
		{
			entry = 124569,
			position = new Vector3(1437.48, 1444.27, 491.3067, "None"),
			continent = ContinentId.Argus_1,
			x = "0.44113",
			y = "0.55859",
		};
		//antoran wastes
		public static Teleporter HopesLanding = new Teleporter()
		{
			entry = 125407,
			position = new Vector3(-2931.165, 8798.77, -231.9501, "None"),
			continent = ContinentId.Argus_1,
			x = "0.14136",
			y = "0.38037",
		};
		public static Teleporter TheVeiledDen = new Teleporter()
		{
			entry = 125409,
			position = new Vector3(-2365.713, 8878.157, -140.2207, "None"),
			continent = ContinentId.Argus_1,
			x = "0.13808",
			y = "0.40351",
		};
		//macaree
		public static Teleporter ShadowguardIncursion = new Teleporter()
		{
			entry = 123258,
			position = new Vector3(5546.06, 10563, 7.527513, "None"),
			continent = ContinentId.Argus_1,
			x = "0.06944",
			y = "0.72606",
			questID = 47220,//a beacon in the dark
		};
		public static Teleporter CityCenter = new Teleporter()
		{
			entry = 126951,
			position = new Vector3(5417.01, 10013.3, -81.4, "None"),
			continent = ContinentId.Argus_1,
			x = "0.09185",
			y = "0.72080",
			questID = 48668, //Lightforged Beacon: City Center
		};
		public static Teleporter ProphetsReflection = new Teleporter()
		{
			entry = 125350,
			position = new Vector3(6307.33, 10116.5, -16.06369, "None"),
			continent = ContinentId.Argus_1,
			x = "0.08764",
			y = "0.75709",
			questID = 47856, // http://www.wowhead.com/quest=47856/across-the-universe
		};
		public static Teleporter ConservatoryOfTheArcane = new Teleporter()
		{
			entry = 124313,
			position = new Vector3(5764.89, 9493.78, -66.74951, "None"),
			continent = ContinentId.Argus_1,
			x = "0.11302",
			y = "0.73498",
			questID = 47690, //http://www.wowhead.com/quest=47690/the-defilers-legacy
		};
		public static Teleporter TriumviratesEnd = new Teleporter()
		{
			entry = 122509,
			position = new Vector3(4983.3, 9823.47, -78.77355, "None"),
			continent = ContinentId.Argus_1,
			x = "0.09958",
			y = "0.70312",
			questID = 46941, //http://www.wowhead.com/quest=46941/the-path-forward
		};
		static List<Teleporter> Krokuun = new List<Teleporter>()
		{
			KrokulHovel,
			/*
			ShatteredFields,
			DestinyPoint,
			//*/
		};

		static List<Teleporter> AntoranWastes = new List<Teleporter>()
		{
			HopesLanding,
			/*
			TheVeiledDen,
			//*/
		};

		static List<Teleporter> MacAree = new List<Teleporter>()
		{
			TriumviratesEnd,
			ConservatoryOfTheArcane,
			ProphetsReflection,
			ShadowguardIncursion,
			CityCenter,
		};
		public static Teleporter GetNear()
		{
			if (InVendicar)
				return Vendicar;

			if (InKrokuun)
				return SearchNear(Krokuun);// Krokuun.OrderBy(t => ObjectManager.Me.Position.DistanceTo(t.position)).FirstOrDefault();

			if (InAntoranWastes)
				return SearchNear(AntoranWastes);// return AntoranWastes.OrderBy(t => ObjectManager.Me.Position.DistanceTo(t.position)).FirstOrDefault();

			if (InMacAree)
				return SearchNear(MacAree);// return MacAree.OrderBy(t => ObjectManager.Me.Position.DistanceTo(t.position)).FirstOrDefault();

			return null;
		}
		static Teleporter SearchNear(List<Teleporter> teleporters)
		{
			return teleporters.Where(t => t.questID < 0 || Quest.GetQuestCompleted(t.questID)).OrderBy(t => ObjectManager.Me.Position.DistanceTo(t.position)).FirstOrDefault();
		}
		public bool To()
		{
			if (!InArgus)
			{
				ToArgus();
				return false;
			}
			if (Usefuls.ContinentId != (int)continent)
				return false;

			var near = GetNear();
			if (near != null)
			{
				Log("found near node=" + near);
				near.Use();
				Take();
				return true;
			}
			Log("cannot find near nodes to reach node=" + this);
			return false;
		}
		public bool Take()
		{
			Thread.Sleep(Usefuls.Latency * 2);
			if (Questing.IsVisible("FlightMapFrame"))
			{
				if (taxiNode < 1 || true) //always detect nodes, coz changed after every quest
				{
					var runCode = string.Format(luaFindNode, x, y);
					taxiNode = Lua.LuaDoString<int>(runCode);
					Thread.Sleep(Usefuls.Latency * 2);
					Log("calculated taxi node=" + this);
					if (taxiNode < 1)
						return false;
				}
				Log("take taxi node=" + this);
				Lua.LuaDoString("TakeTaxiNode(" + taxiNode + ")");
				//Thread.Sleep(Usefuls.Latency * 2);
				Questing.WaitCurrentAreaIDChange();
				return true;
			}
			return false;
		}
		public bool Use()
		{
			if (Usefuls.ContinentId != (int)continent)
				return false;

			GoToTask.ToPositionAndIntecractWithNpc(position, entry);
			return true;
		}
		public override string ToString()
		{
			return "[Argus Teleporter id=" + entry + " node=" + taxiNode + " xy=" + x + "," + y + " quest=" + questID + /* " pos=" + position.ToStringNewVector() + */"]";
		}
	}
	#endregion TELEPORTERS

	#region SUBZONE
	public static void StartMoveFix()
	{
		robotManager.Events.ProductEvents.OnProductStopping += OnProductStop;
		wManager.Events.MovementEvents.OnMovementPulse += Subzone.Vendicar.OnMovementPulse;
		//krokuun
		wManager.Events.MovementEvents.OnMovementPulse += Subzone.KrokulHovel.OnMovementPulse;
		wManager.Events.MovementEvents.OnMovementPulse += Subzone.NathraxasHold.OnMovementPulse;
		wManager.Events.MovementEvents.OnMovementPulse += Subzone.DarkfallRidge.OnMovementPulse;
		wManager.Events.MovementEvents.OnMovementPulse += Subzone.ShatteredFieldsCliff.OnMovementPulse;
		//antoran wastes
		//mac'aree
		wManager.Events.MovementEvents.OnMovementPulse += Subzone.TriumviratesEnd.OnMovementPulse;

		Log("Argus move fix started");
	}
	public static void StopMoveFix()
	{
		robotManager.Events.ProductEvents.OnProductStopping -= OnProductStop;
		wManager.Events.MovementEvents.OnMovementPulse -= Subzone.Vendicar.OnMovementPulse;
		//krokuun
		wManager.Events.MovementEvents.OnMovementPulse -= Subzone.KrokulHovel.OnMovementPulse;
		wManager.Events.MovementEvents.OnMovementPulse -= Subzone.NathraxasHold.OnMovementPulse;
		wManager.Events.MovementEvents.OnMovementPulse -= Subzone.DarkfallRidge.OnMovementPulse;
		wManager.Events.MovementEvents.OnMovementPulse -= Subzone.ShatteredFieldsCliff.OnMovementPulse;
		//antoran wastes
		//mac'aree
		wManager.Events.MovementEvents.OnMovementPulse -= Subzone.TriumviratesEnd.OnMovementPulse;

		Log("Argus move fix stoped");
	}
	static void OnProductStop(string productName)
	{
		StopMoveFix();
	}
	public class Subzone
	{
		//all vendicars
		internal class Vendicar
		{
			const int TOTAL_TRIES = 3;
			static int currentTries = TOTAL_TRIES;
			static Vector3 lastTryPosition = Vector3.Zero;
			static Vector3 badPoint = new Vector3(4736.046, 9858.889, 64.29174);

			internal static void OnMovementPulse(List<Vector3> points, System.ComponentModel.CancelEventArgs cancelable)
			{
				if (!InVendicar)
					return;

				if (points == null)
					return;

				var myPos = ObjectManager.Me.Position;
				var pointsCount = points.Count;
				if (pointsCount < 1)
					return;

				var start = points[0];
				if (start.DistanceTo(badPoint) < 7)
				{
					Log("move bugfix from " + start.ToStringNewVector() + " to=" + myPos.ToStringNewVector());
					points[0] = myPos;
				}
				var end = points[points.Count - 1];
				//if (!TraceLine.TraceLineGo(end)) return;
				if (lastTryPosition.DistanceTo(end) > 3.5f)
				{
					currentTries = TOTAL_TRIES;
					lastTryPosition = end;
				}
				else if (currentTries-- < 1)
				{
					return;
				}
				var currentPath = VendicarKrokuun;
				var currentZone = "Krokuun Vendicar:";
				if (InVendicarMacAree)
				{
					currentPath = VendicarMacAree;
					currentZone = "Mac'Aree Vendicar:";
				}
				else if (InVendicarAntoranWastes)
				{
					currentPath = VendicarAntoranWastes;
					currentZone = "Antoran Wastes Vendicar:";
				}

				if (myPos.DistanceTo(end) < 15 && !TraceLine.TraceLineGo(myPos, end))
				{
					Log(currentZone + " move direct to " + end.ToStringNewVector() + " from=" + myPos.ToStringNewVector() );
					points.Clear();
					points.Add(myPos);
					points.Add(end);
					return;
				}
				var prev = points.Count;
				var path = Questing.PathClampDirected(currentPath, myPos, end, 5);
				if (path.Count < 2)
					return;

				points.Clear();
				points.AddRange(path);
				Log(currentZone + " path fix from " + prev + " to " + points.Count + ". from=" + myPos.ToStringNewVector() + " to=" + end.ToStringNewVector() );
			}
			public static List<Vector3> VendicarAntoranWastes = new List<Vector3>()
			{
				new Vector3(-2602.418f, 8573.826f, -67.04715f, "None"),
				new Vector3(-2603.227f, 8578.04f, -67.02875f, "None"),
				new Vector3(-2603.876f, 8581.479f, -66.79741f, "None"),
				new Vector3(-2605.261f, 8588.169f, -64.56468f, "None"),
				new Vector3(-2606.012f, 8591.567f, -63.58398f, "None"),
				new Vector3(-2606.816f, 8595.211f, -63.60017f, "None"),
				new Vector3(-2608.22f, 8601.731f, -63.72285f, "None"),
				new Vector3(-2608.568f, 8605.348f, -63.72285f, "None"),
				new Vector3(-2606.493f, 8611.905f, -63.94766f, "None"),
				new Vector3(-2602.395f, 8617.703f, -63.96376f, "None"),
				new Vector3(-2600.173f, 8620.462f, -63.90594f, "None"),
				new Vector3(-2595.753f, 8625.87f, -64.34614f, "None"),
				new Vector3(-2593.459f, 8628.597f, -65.06937f, "None"),
				new Vector3(-2591.148f, 8631.364f, -65.86238f, "None"),
				new Vector3(-2587.504f, 8637.021f, -67.37147f, "None"),
				new Vector3(-2586.241f, 8640.308f, -68.14141f, "None"),
				new Vector3(-2585.007f, 8643.576f, -68.92262f, "None"),
				new Vector3(-2584.159f, 8647.032f, -69.69924f, "None"),
				new Vector3(-2583.562f, 8653.941f, -71.24014f, "None"),
				new Vector3(-2583.819f, 8657.449f, -72.02187f, "None"),
				new Vector3(-2586.032f, 8664.067f, -73.43803f, "None"),
				new Vector3(-2587.514f, 8667.238f, -74.31525f, "None"),
				new Vector3(-2589.059f, 8670.432f, -74.87679f, "None"),
				new Vector3(-2590.781f, 8673.443f, -76.0312f, "None"),
				new Vector3(-2593.065f, 8676.186f, -76.86787f, "None"),
				new Vector3(-2597.838f, 8681.202f, -78.48216f, "None"),
				new Vector3(-2600.81f, 8683.062f, -79.1094f, "None"),
				new Vector3(-2607.509f, 8682.455f, -79.2447f, "None"),
				new Vector3(-2610.651f, 8680.805f, -79.25301f, "None"),
				new Vector3(-2617.336f, 8679.219f, -79.29864f, "None"),
				new Vector3(-2623.812f, 8681.702f, -78.95616f, "None"),
				new Vector3(-2628.398f, 8686.67f, -76.71302f, "None"),
				new Vector3(-2629.938f, 8689.862f, -74.54243f, "None"),
				new Vector3(-2631.152f, 8693.043f, -71.92523f, "None"),
				new Vector3(-2632.121f, 8696.68f, -70.62148f, "None"),
				new Vector3(-2631.821f, 8703.446f, -70.81988f, "None"),
				new Vector3(-2631.917f, 8707.013f, -70.81988f, "None"),
				new Vector3(-2633.828f, 8710.046f, -70.81988f, "None"),
				new Vector3(-2639.589f, 8713.29f, -70.81988f, "None"),
				new Vector3(-2643.804f, 8708.343f, -70.81988f, "None"),
				new Vector3(-2643.239f, 8701.373f, -70.81988f, "None"),
				new Vector3(-2641.885f, 8698.136f, -70.82912f, "None"),
				new Vector3(-2638.912f, 8691.916f, -70.91237f, "None"),
				new Vector3(-2637.339f, 8688.664f, -73.81452f, "None"),
				new Vector3(-2635.889f, 8685.52f, -76.31786f, "None"),
				new Vector3(-2634.623f, 8682.212f, -78.61898f, "None"),
				new Vector3(-2633.589f, 8678.885f, -78.95416f, "None"),
				new Vector3(-2631.782f, 8672.107f, -78.95416f, "None"),
				new Vector3(-2629.972f, 8665.316f, -79.04208f, "None"),
				new Vector3(-2628.161f, 8658.519f, -79.04208f, "None"),
				new Vector3(-2626.203f, 8651.836f, -79.04208f, "None"),
				new Vector3(-2625.123f, 8648.491f, -79.04208f, "None"),
				new Vector3(-2623.993f, 8645.149f, -79.04208f, "None"),
				new Vector3(-2622.802f, 8641.843f, -78.83015f, "None"),
				new Vector3(-2621.534f, 8638.634f, -76.98426f, "None"),
				new Vector3(-2620.063f, 8635.438f, -75.13255f, "None"),
				new Vector3(-2617.959f, 8631.756f, -75.03181f, "None"),
				new Vector3(-2616.938f, 8637.17f, -75.53026f, "None"),
				new Vector3(-2617.661f, 8640.679f, -77.46999f, "None"),
				new Vector3(-2618.193f, 8644.096f, -79.04169f, "None"),
				new Vector3(-2619.268f, 8650.998f, -79.04169f, "None"),
				new Vector3(-2619.801f, 8654.472f, -79.04169f, "None"),
				new Vector3(-2620.201f, 8658.047f, -79.04169f, "None"),
				new Vector3(-2620.619f, 8665.082f, -79.04154f, "None"),
				new Vector3(-2618.829f, 8671.583f, -79.16457f, "None"),
				new Vector3(-2612.42f, 8674.225f, -79.33349f, "None"),
				new Vector3(-2606.074f, 8671.897f, -80.24316f, "None"),
				new Vector3(-2603.377f, 8669.508f, -81.57669f, "None"),
				new Vector3(-2601.442f, 8666.644f, -82.88715f, "None"),
				new Vector3(-2599.659f, 8663.651f, -84.15858f, "None"),
				new Vector3(-2598.584f, 8660.393f, -85.40948f, "None"),
				new Vector3(-2598.327f, 8656.895f, -86.70093f, "None"),
				new Vector3(-2598.606f, 8653.331f, -88.02831f, "None"),
				new Vector3(-2599.027f, 8649.884f, -89.27768f, "None"),
				new Vector3(-2599.663f, 8646.394f, -90.52601f, "None"),
				new Vector3(-2600.445f, 8643.112f, -91.69322f, "None"),
				new Vector3(-2601.256f, 8639.714f, -92.83273f, "None"),
				new Vector3(-2602.077f, 8636.282f, -93.8013f, "None"),
				new Vector3(-2602.443f, 8632.726f, -94.63488f, "None"),
				new Vector3(-2597.226f, 8629.734f, -94.64256f, "None"),
				new Vector3(-2590.446f, 8631.386f, -94.64256f, "None"),
				new Vector3(-2583.672f, 8632.878f, -94.64256f, "None"),
				new Vector3(-2580.056f, 8632.997f, -94.64256f, "None"),
				new Vector3(-2573.687f, 8630.505f, -94.64256f, "None"),
				new Vector3(-2576.036f, 8625.627f, -94.6415f, "None"),
				new Vector3(-2579.357f, 8624.255f, -94.6415f, "None"),
				new Vector3(-2582.991f, 8623f, -94.6415f, "None"),
				new Vector3(-2586.247f, 8621.7f, -94.6415f, "None"),
				new Vector3(-2592.823f, 8619.452f, -94.60826f, "None"),
				new Vector3(-2598.105f, 8614.906f, -94.51477f, "None"),
				new Vector3(-2601.717f, 8609.057f, -94.72965f, "None"),
				new Vector3(-2603.875f, 8606.165f, -94.86311f, "None"),
				new Vector3(-2609.451f, 8602.282f, -94.86311f, "None"),
				new Vector3(-2613.063f, 8602.344f, -94.86311f, "None"),
				new Vector3(-2619.497f, 8604.534f, -94.79704f, "None"),
				new Vector3(-2622.537f, 8606.17f, -94.18277f, "None"),
				new Vector3(-2625.802f, 8607.871f, -94.49851f, "None"),
				new Vector3(-2632.507f, 8609.344f, -94.60888f, "None"),
				new Vector3(-2639.571f, 8608.546f, -94.64122f, "None"),
				new Vector3(-2646.128f, 8610.495f, -94.64122f, "None"),
				new Vector3(-2651.66f, 8614.729f, -94.64122f, "None"),
				new Vector3(-2657.141f, 8619.212f, -94.64122f, "None"),
				new Vector3(-2659.178f, 8623.031f, -94.64143f, "None"),
				new Vector3(-2652.394f, 8621.584f, -94.64143f, "None"),
				new Vector3(-2645.551f, 8620.943f, -94.64143f, "None"),
				new Vector3(-2642.024f, 8620.869f, -94.64143f, "None"),
				new Vector3(-2638.509f, 8620.672f, -94.64143f, "None"),
				new Vector3(-2631.581f, 8621.015f, -94.64481f, "None"),
				new Vector3(-2628.145f, 8621.828f, -94.64208f, "None"),
				new Vector3(-2621.408f, 8623.803f, -94.64208f, "None"),
				new Vector3(-2618.122f, 8629.073f, -94.57052f, "None"),
				new Vector3(-2619.537f, 8634.672f, -94.57052f, "None"),
				new Vector3(-2622.021f, 8640.941f, -94.57052f, "None"),
				new Vector3(-2623.985f, 8643.87f, -95.36562f, "None"),
				new Vector3(-2627.503f, 8649.755f, -95.40031f, "None"),
				new Vector3(-2628.844f, 8656.393f, -95.41499f, "None"),
				new Vector3(-2628.793f, 8659.941f, -95.6431f, "None"),
				new Vector3(-2628.73f, 8663.542f, -95.36683f, "None"),
				new Vector3(-2628.771f, 8670.436f, -94.57136f, "None"),
				new Vector3(-2629.458f, 8673.973f, -94.57136f, "None"),
				new Vector3(-2631.063f, 8680.735f, -94.57136f, "None"),
				new Vector3(-2632.653f, 8687.438f, -94.3448f, "None"),
				new Vector3(-2633.505f, 8690.977f, -94.3448f, "None"),
				new Vector3(-2635.327f, 8697.701f, -94.16415f, "None"),
			};
			public static List<Vector3> VendicarKrokuun = new List<Vector3>()
			{
				new Vector3(380.399f, 1412.464f, 769.5522f, "None"),
				new Vector3(384.3988f, 1413.957f, 769.57f, "None"),
				new Vector3(387.7771f, 1415.274f, 769.5869f, "None"),
				new Vector3(391.0891f, 1416.776f, 769.8534f, "None"),
				new Vector3(394.1989f, 1418.319f, 770.8797f, "None"),
				new Vector3(397.2129f, 1419.706f, 772.0597f, "None"),
				new Vector3(400.4609f, 1421.221f, 773.0316f, "None"),
				new Vector3(406.652f, 1424.334f, 772.8928f, "None"),
				new Vector3(413.104f, 1427.537f, 772.8928f, "None"),
				new Vector3(419.4215f, 1427.779f, 772.8297f, "None"),
				new Vector3(422.4009f, 1425.713f, 772.6535f, "None"),
				new Vector3(425.4011f, 1423.83f, 772.6445f, "None"),
				new Vector3(431.3266f, 1420.13f, 772.7097f, "None"),
				new Vector3(434.3415f, 1418.244f, 772.5262f, "None"),
				new Vector3(440.4887f, 1414.912f, 771.2499f, "None"),
				new Vector3(446.6877f, 1412.031f, 769.7056f, "None"),
				new Vector3(450.0979f, 1411.512f, 768.9233f, "None"),
				new Vector3(453.6222f, 1411.253f, 768.1264f, "None"),
				new Vector3(457.2656f, 1411.091f, 767.3028f, "None"),
				new Vector3(464.0906f, 1411.68f, 765.7385f, "None"),
				new Vector3(467.5352f, 1412.363f, 764.8927f, "None"),
				new Vector3(470.907f, 1413.325f, 764.2037f, "None"),
				new Vector3(477.1272f, 1416.4f, 762.5023f, "None"),
				new Vector3(479.9255f, 1418.502f, 761.7776f, "None"),
				new Vector3(482.6886f, 1420.616f, 760.8866f, "None"),
				new Vector3(485.4529f, 1423.016f, 759.949f, "None"),
				new Vector3(490.0953f, 1428.167f, 758.3234f, "None"),
				new Vector3(491.8034f, 1434.839f, 757.4175f, "None"),
				new Vector3(489.1914f, 1441.219f, 757.4219f, "None"),
				new Vector3(485.5005f, 1446.992f, 757.3799f, "None"),
				new Vector3(485.2675f, 1453.813f, 757.6591f, "None"),
				new Vector3(488.536f, 1459.87f, 758.9282f, "None"),
				new Vector3(491.0402f, 1462.193f, 760.8963f, "None"),
				new Vector3(493.9548f, 1464.203f, 763.501f, "None"),
				new Vector3(497.0648f, 1465.922f, 766.0929f, "None"),
				new Vector3(503.7577f, 1467.638f, 765.7963f, "None"),
				new Vector3(507.336f, 1467.442f, 765.7963f, "None"),
				new Vector3(512.7958f, 1470.995f, 765.7963f, "None"),
				new Vector3(513.0669f, 1477.19f, 765.7963f, "None"),
				new Vector3(506.4713f, 1478.806f, 765.7963f, "None"),
				new Vector3(500.6451f, 1475.628f, 765.7963f, "None"),
				new Vector3(494.9153f, 1471.658f, 766.0519f, "None"),
				new Vector3(491.8152f, 1469.764f, 763.9828f, "None"),
				new Vector3(488.9301f, 1468.001f, 761.2943f, "None"),
				new Vector3(485.8501f, 1466.128f, 759.0157f, "None"),
				new Vector3(482.7991f, 1464.461f, 757.6611f, "None"),
				new Vector3(479.6115f, 1462.965f, 757.6611f, "None"),
				new Vector3(476.4236f, 1461.47f, 757.6611f, "None"),
				new Vector3(470.0543f, 1458.483f, 757.5742f, "None"),
				new Vector3(463.7736f, 1455.538f, 757.5742f, "None"),
				new Vector3(460.547f, 1453.978f, 757.5742f, "None"),
				new Vector3(454.3766f, 1450.917f, 757.5728f, "None"),
				new Vector3(451.3666f, 1449.118f, 757.5728f, "None"),
				new Vector3(448.3628f, 1447.124f, 757.5728f, "None"),
				new Vector3(445.4443f, 1445.18f, 758.7509f, "None"),
				new Vector3(442.6094f, 1443.325f, 760.5527f, "None"),
				new Vector3(439.5855f, 1441.427f, 761.7552f, "None"),
				new Vector3(445.6398f, 1441.556f, 759.5371f, "None"),
				new Vector3(448.8283f, 1442.591f, 757.751f, "None"),
				new Vector3(452.2176f, 1443.487f, 757.574f, "None"),
				new Vector3(455.6639f, 1444.241f, 757.574f, "None"),
				new Vector3(459.1081f, 1445.327f, 757.574f, "None"),
				new Vector3(465.6517f, 1447.301f, 757.574f, "None"),
				new Vector3(469.0718f, 1448.057f, 757.5741f, "None"),
				new Vector3(472.7f, 1447.948f, 757.5722f, "None"),
				new Vector3(478.7497f, 1444.708f, 757.6104f, "None"),
				new Vector3(481.4104f, 1438.455f, 757.548f, "None"),
				new Vector3(480.8411f, 1435.053f, 756.5532f, "None"),
				new Vector3(478.9595f, 1432.163f, 755.3085f, "None"),
				new Vector3(476.224f, 1429.986f, 754.0305f, "None"),
				new Vector3(473.3052f, 1427.992f, 752.74f, "None"),
				new Vector3(470.2415f, 1426.319f, 751.4761f, "None"),
				new Vector3(467.1335f, 1424.71f, 750.2528f, "None"),
				new Vector3(463.9121f, 1423.595f, 749.1086f, "None"),
				new Vector3(460.2946f, 1423.557f, 747.8442f, "None"),
				new Vector3(456.8471f, 1423.802f, 746.6239f, "None"),
				new Vector3(453.3499f, 1424.21f, 745.3926f, "None"),
				new Vector3(449.9581f, 1424.616f, 744.2514f, "None"),
				new Vector3(446.4064f, 1425.041f, 743.1686f, "None"),
				new Vector3(442.8941f, 1425.245f, 742.2686f, "None"),
				new Vector3(438.6748f, 1421.096f, 741.9724f, "None"),
				new Vector3(439.3159f, 1417.644f, 741.9729f, "None"),
				new Vector3(440.6184f, 1414.387f, 741.9729f, "None"),
				new Vector3(443.3202f, 1407.825f, 741.9729f, "None"),
				new Vector3(445.8839f, 1401.429f, 741.9729f, "None"),
				new Vector3(443.6204f, 1395.606f, 741.9729f, "None"),
				new Vector3(438.3829f, 1399.83f, 741.9729f, "None"),
				new Vector3(433.9885f, 1405.269f, 741.9729f, "None"),
				new Vector3(432.0373f, 1408.199f, 741.9887f, "None"),
				new Vector3(428.8208f, 1414.296f, 742.0067f, "None"),
				new Vector3(422.9012f, 1417.813f, 742.4121f, "None"),
				new Vector3(416.2598f, 1419.935f, 741.7524f, "None"),
				new Vector3(411.3409f, 1424.661f, 741.7524f, "None"),
				new Vector3(410.1718f, 1427.971f, 741.7524f, "None"),
				new Vector3(410.4317f, 1434.995f, 741.7524f, "None"),
				new Vector3(411.8387f, 1441.745f, 742.4297f, "None"),
				new Vector3(412.7292f, 1448.717f, 742.0074f, "None"),
				new Vector3(411.1778f, 1455.366f, 741.9755f, "None"),
				new Vector3(410.0181f, 1458.718f, 741.9755f, "None"),
				new Vector3(411.8973f, 1465.379f, 741.9755f, "None"),
				new Vector3(413.7528f, 1468.412f, 741.9755f, "None"),
				new Vector3(417.2783f, 1474.394f, 741.9755f, "None"),
				new Vector3(419.3293f, 1477.368f, 741.9755f, "None"),
				new Vector3(421.7404f, 1474.72f, 741.9755f, "None"),
				new Vector3(421.6287f, 1471.121f, 741.9755f, "None"),
				new Vector3(421.9904f, 1464.099f, 741.9755f, "None"),
				new Vector3(422.9281f, 1457.305f, 741.9755f, "None"),
				new Vector3(423.8221f, 1453.895f, 741.9739f, "None"),
				new Vector3(426.522f, 1447.513f, 741.9734f, "None"),
				new Vector3(428.0196f, 1444.15f, 741.9734f, "None"),
				new Vector3(432.3703f, 1439.55f, 742.0453f, "None"),
				new Vector3(435.8132f, 1440.205f, 742.0453f, "None"),
				new Vector3(442.1084f, 1442.876f, 742.0453f, "None"),
				new Vector3(445.6018f, 1443.905f, 742.0453f, "None"),
				new Vector3(449.1141f, 1444.237f, 741.5696f, "None"),
				new Vector3(455.9764f, 1444.438f, 741.2257f, "None"),
				new Vector3(459.4861f, 1444.566f, 741.2031f, "None"),
				new Vector3(462.7827f, 1445.817f, 741.208f, "None"),
				new Vector3(467.4045f, 1450.89f, 740.9724f, "None"),
				new Vector3(469.5377f, 1453.682f, 741.4882f, "None"),
				new Vector3(472.1207f, 1456.19f, 742.0453f, "None"),
				new Vector3(478.2652f, 1459.378f, 742.0453f, "None"),
				new Vector3(484.5701f, 1462.304f, 742.0453f, "None"),
				new Vector3(487.7511f, 1463.781f, 742.2711f, "None"),
				new Vector3(490.9829f, 1465.281f, 742.2711f, "None"),
				new Vector3(497.3037f, 1468.213f, 742.3773f, "None"),
			};
			public static List<Vector3> VendicarMacAree = new List<Vector3>()
			{
				//new
				new Vector3(4602.465f, 9832.12f, 68.06753f, "None"),
				new Vector3(4608.335f, 9833.576f, 68.27554f, "None"),
				new Vector3(4611.695f, 9834.383f, 69.04198f, "None"),
				new Vector3(4615.302f, 9835.249f, 70.36034f, "None"),
				new Vector3(4622.091f, 9837.063f, 71.58617f, "None"),
				new Vector3(4628.893f, 9836.548f, 71.3865f, "None"),
				new Vector3(4632.383f, 9835.989f, 71.3865f, "None"),
				new Vector3(4635.816f, 9835.138f, 71.3865f, "None"),
				new Vector3(4642.01f, 9832.045f, 71.1376f, "None"),
				new Vector3(4644.916f, 9830.071f, 71.14646f, "None"),
				new Vector3(4647.629f, 9827.775f, 71.21332f, "None"),
				new Vector3(4653.029f, 9823.5f, 70.73042f, "None"),
				new Vector3(4655.778f, 9821.333f, 70.02516f, "None"),
				new Vector3(4658.682f, 9819.273f, 69.25265f, "None"),
				new Vector3(4661.649f, 9817.353f, 68.4586f, "None"),
				new Vector3(4664.629f, 9815.546f, 67.67738f, "None"),
				new Vector3(4667.936f, 9814.187f, 66.89923f, "None"),
				new Vector3(4671.196f, 9813.054f, 66.12006f, "None"),
				new Vector3(4674.695f, 9812.688f, 65.35269f, "None"),
				new Vector3(4678.363f, 9812.457f, 64.52116f, "None"),
				new Vector3(4684.992f, 9812.456f, 63.04701f, "None"),
				new Vector3(4688.644f, 9812.715f, 62.4059f, "None"),
				new Vector3(4692.051f, 9812.956f, 61.60619f, "None"),
				new Vector3(4695.484f, 9813.337f, 60.83831f, "None"),
				new Vector3(4698.894f, 9814.484f, 59.9939f, "None"),
				new Vector3(4704.59f, 9818.114f, 58.48687f, "None"),
				new Vector3(4707.569f, 9820.005f, 57.82948f, "None"),
				new Vector3(4710.264f, 9822.545f, 56.71615f, "None"),
				new Vector3(4712.302f, 9829.002f, 55.73577f, "None"),
				new Vector3(4709.903f, 9835.43f, 55.90792f, "None"),
				new Vector3(4707.87f, 9841.978f, 55.78262f, "None"),
				new Vector3(4709.4f, 9848.657f, 56.14175f, "None"),
				new Vector3(4712.297f, 9851.795f, 56.41763f, "None"),
				new Vector3(4715.322f, 9853.745f, 58.74453f, "None"),
				new Vector3(4718.709f, 9855.289f, 61.05059f, "None"),
				new Vector3(4724.363f, 9858.035f, 64.55209f, "None"),
				new Vector3(4727.755f, 9859.37f, 64.28638f, "None"),
				new Vector3(4734.807f, 9858.64f, 64.29001f, "None"),
				new Vector3(4740.486f, 9861.729f, 64.29001f, "None"),
				new Vector3(4740.114f, 9868.054f, 64.29001f, "None"),
				new Vector3(4733.833f, 9870.072f, 64.29001f, "None"),
				new Vector3(4728.992f, 9867.946f, 64.29001f, "None"),
				new Vector3(4725.853f, 9866.398f, 64.27461f, "None"),
				new Vector3(4721.231f, 9864.295f, 64.25249f, "None"),
				new Vector3(4717.86f, 9863.143f, 61.4068f, "None"),
				new Vector3(4714.802f, 9861.607f, 58.9892f, "None"),
				new Vector3(4709.892f, 9860.069f, 56.15377f, "None"),
				new Vector3(4702.835f, 9858.629f, 56.15377f, "None"),
				new Vector3(4696.111f, 9857.646f, 56.05012f, "None"),
				new Vector3(4689.434f, 9856.136f, 56.06784f, "None"),
				new Vector3(4685.946f, 9855.257f, 56.06784f, "None"),
				new Vector3(4679.194f, 9853.41f, 56.06661f, "None"),
				new Vector3(4675.72f, 9852.447f, 56.06661f, "None"),
				new Vector3(4669.071f, 9850.516f, 56.85941f, "None"),
				new Vector3(4665.71f, 9849.538f, 58.75952f, "None"),
				new Vector3(4662.413f, 9848.343f, 60.20908f, "None"),
				new Vector3(4663.192f, 9844.604f, 60.23375f, "None"),
				new Vector3(4666.804f, 9844.809f, 58.84787f, "None"),
				new Vector3(4670.167f, 9845.22f, 57.04519f, "None"),
				new Vector3(4673.737f, 9845.534f, 56.06892f, "None"),
				new Vector3(4680.606f, 9845.649f, 56.06894f, "None"),
				new Vector3(4687.585f, 9845.594f, 56.06763f, "None"),
				new Vector3(4691.203f, 9845.294f, 56.06862f, "None"),
				new Vector3(4694.641f, 9844.555f, 56.06945f, "None"),
				new Vector3(4700.771f, 9841.507f, 56.11136f, "None"),
				new Vector3(4702.298f, 9835.088f, 56.05117f, "None"),
				new Vector3(4700.607f, 9831.911f, 54.80709f, "None"),
				new Vector3(4698.024f, 9829.715f, 53.61113f, "None"),
				new Vector3(4695.016f, 9827.653f, 52.3084f, "None"),
				new Vector3(4692.131f, 9825.916f, 51.15144f, "None"),
				new Vector3(4688.692f, 9825.167f, 49.91544f, "None"),
				new Vector3(4685.214f, 9824.767f, 48.68251f, "None"),
				new Vector3(4681.667f, 9825.195f, 47.42084f, "None"),
				new Vector3(4678.291f, 9825.84f, 46.23153f, "None"),
				new Vector3(4674.881f, 9826.733f, 44.97346f, "None"),
				new Vector3(4671.571f, 9827.851f, 43.74157f, "None"),
				new Vector3(4668.329f, 9828.947f, 42.59179f, "None"),
				new Vector3(4664.911f, 9829.914f, 41.55905f, "None"),
				new Vector3(4661.414f, 9830.13f, 40.78912f, "None"),
				new Vector3(4658.494f, 9824.644f, 40.466f, "None"),
				new Vector3(4660.495f, 9818.032f, 40.466f, "None"),
				new Vector3(4661.57f, 9814.605f, 40.466f, "None"),
				new Vector3(4661.939f, 9807.695f, 40.466f, "None"),
				new Vector3(4658.542f, 9801.907f, 40.466f, "None"),
				new Vector3(4652.898f, 9804.682f, 40.466f, "None"),
				new Vector3(4650.656f, 9811.333f, 40.466f, "None"),
				new Vector3(4648.588f, 9818.033f, 40.47237f, "None"),
				new Vector3(4647.365f, 9821.344f, 40.50039f, "None"),
				new Vector3(4643.271f, 9826.953f, 40.50039f, "None"),
				new Vector3(4638.155f, 9831.782f, 40.55819f, "None"),
				new Vector3(4633.569f, 9836.898f, 40.24713f, "None"),
				new Vector3(4632.573f, 9840.301f, 40.24713f, "None"),
				new Vector3(4633.827f, 9847.105f, 40.27007f, "None"),
				new Vector3(4635.829f, 9853.856f, 40.74781f, "None"),
				new Vector3(4637.553f, 9860.388f, 40.5004f, "None"),
				new Vector3(4637.754f, 9864.072f, 40.5004f, "None"),
				new Vector3(4637.099f, 9871.043f, 40.46756f, "None"),
				new Vector3(4640.112f, 9877.354f, 40.46756f, "None"),
				new Vector3(4644.449f, 9882.729f, 41.32889f, "None"),
				new Vector3(4646.995f, 9885.174f, 40.46862f, "None"),
				new Vector3(4651.443f, 9883.444f, 40.46862f, "None"),
				new Vector3(4649.494f, 9876.714f, 40.46862f, "None"),
				new Vector3(4648.563f, 9869.834f, 40.46862f, "None"),
				new Vector3(4648.835f, 9862.924f, 40.46862f, "None"),
				new Vector3(4649.057f, 9859.392f, 40.46679f, "None"),
				new Vector3(4649.702f, 9855.901f, 40.46679f, "None"),
				new Vector3(4650.334f, 9852.444f, 40.46679f, "None"),
				new Vector3(4651.524f, 9849.15f, 40.46679f, "None"),
				new Vector3(4657.387f, 9846.107f, 40.5395f, "None"),
				new Vector3(4664.112f, 9847.759f, 40.5395f, "None"),
				new Vector3(4667.553f, 9848.603f, 40.53796f, "None"),
				new Vector3(4671.002f, 9849.569f, 40.32545f, "None"),
				new Vector3(4676.719f, 9853.123f, 39.71619f, "None"),
				new Vector3(4679.221f, 9855.647f, 39.69412f, "None"),
				new Vector3(4685.239f, 9857.242f, 39.70473f, "None"),
				new Vector3(4689.374f, 9856.641f, 39.46569f, "None"),
				new Vector3(4692.874f, 9856.094f, 39.93138f, "None"),
				new Vector3(4699.76f, 9856.179f, 40.53901f, "None"),
				new Vector3(4703.244f, 9856.89f, 40.53901f, "None"),
				new Vector3(4706.657f, 9857.696f, 40.53901f, "None"),
				new Vector3(4713.404f, 9859.241f, 40.57635f, "None"),
				new Vector3(4716.852f, 9859.919f, 40.76488f, "None"),
				new Vector3(4720.3f, 9860.633f, 40.76488f, "None"),
				new Vector3(4723.739f, 9861.354f, 40.83934f, "None"),
			};
		}
		//krokuun middle base
		internal class KrokulHovel
		{
			internal static void OnMovementPulse(List<Vector3> points, System.ComponentModel.CancelEventArgs cancelable)
			{
				if (!InKrokuunOpen)
					return;

				if (points == null)
					return;
				var myPos = ObjectManager.Me.Position;
				var pointsCount = points.Count;
				if (pointsCount < 1)
					return;

				var end = points[points.Count - 1];
			}
		}
		//S from krorul hovel
		internal class DarkfallRidge
		{
			internal static void OnMovementPulse(List<Vector3> points, System.ComponentModel.CancelEventArgs cancelable)
			{
				if (!InKrokuunOpen)
					return;

				if (points == null)
					return;
				var myPos = ObjectManager.Me.Position;
				var pointsCount = points.Count;
				if (pointsCount < 1)
					return;

				var end = points[points.Count - 1];
			}
		}
		//N of Krokuun
		internal class NathraxasHold
		{
			internal static void OnMovementPulse(List<Vector3> points, System.ComponentModel.CancelEventArgs cancelable)
			{
				if (!InKrokuunOpen)
					return;

				if (points == null)
					return;
				var myPos = ObjectManager.Me.Position;
				var pointsCount = points.Count;
				if (pointsCount < 1)
					return;

				var end = points[points.Count - 1];
			}
		}
		//S of Krokuun
		internal class ShatteredFieldsCliff
		{
			internal static bool IsInside(Vector3 p)
			{
				return !InKrokuunShatteredFieldsCliff;
			}
			internal static void OnMovementPulse(List<Vector3> points, System.ComponentModel.CancelEventArgs cancelable)
			{
				if (!InKrokuunOpen)
					return;

				if (points == null)
					return;
				var myPos = ObjectManager.Me.Position;
				var pointsCount = points.Count;
				if (pointsCount < 1)
					return;

				var end = points[points.Count - 1];
			}
		}
		//S mac'aree 
		internal class TriumviratesEnd
		{
			internal static Vector3 Pylon3 = new Vector3(5027.635, 9870.633, -76.49744);
			static List<Vector3> PathToPylon3 = new List<Vector3>()
			{
				new Vector3(4989.852f, 9834.66f, -79.22293f, "None"),
				new Vector3(4995.672f, 9839.013f, -78.81702f, "None"),
				new Vector3(5000.911f, 9843.524f, -78.50297f, "None"),
				new Vector3(5005.154f, 9849.115f, -78.09761f, "None"),
				new Vector3(5009.672f, 9854.396f, -78.09761f, "None"),
				new Vector3(5014.718f, 9859.407f, -78.29232f, "None"),
				new Vector3(5019.76f, 9864.109f, -77.85727f, "None"),
				new Vector3(5024.31f, 9868.215f, -76.76617f, "None"),
			};
			internal static bool IsNearPylon3(Vector3 p)
			{
				return Pylon3.DistanceTo2D(p) < 30;
			}
			internal static void OnMovementPulse(List<Vector3> points, System.ComponentModel.CancelEventArgs cancelable)
			{
				if (!InMacAreeOpen)
					return;
				var myPos = ObjectManager.Me.Position;
				var isNear = IsNearPylon3(myPos);
				if (isNear || points.Count < 1)
					return;

				var end = points[points.Count - 1];
				foreach (var p in points)
				{
					if (IsNearPylon3(p))
					{
						var success = false;
						var path = PathFinder.FindPath(PathToPylon3[0], out success);
						if (success)
						{
							var old = points.Count;
							points.Clear();
							points.AddRange(path);
							var path2 = Questing.PathClamp(PathToPylon3, PathToPylon3[0], end);
							points.AddRange(path2);
							Log("Mac'Aree: Triumvirate's End fix path to pylon3 from outside path=" + old + " to=" + points.Count);
							return;
						}
					}
				}
			}
		}
	}
	#endregion

	static void Log(string text)
	{
		Logging.WriteDebug("[Argus Helper] " + text);
	}

	public static class Warframe
	{
		static Vector3 _destination = Vector3.Zero;
		//1 248292/judgment-blast dist=60 cd=1.5
		//2 251509/purifying-flame dist=14 cd=6
		//3 251485/crusader-leap dist=10-60 cd=10
		//5 251569/vindication cast=6s cd=15 nomove
		static void Trace(string text)
		{
			Log("warframe: " + text);
		}
		public static bool Can
		{
			get
			{
				return ObjectManager.Me.PlayerUsingVehicle && ObjectManager.Me.HaveBuff(250924); //  Fel Sludge Immunity: ID=250924
			}
		}
		public static bool Pulse(params int[] mobsID)
		{
			if (!Can)
				return false;

			var mob = Questing.FindUnit(mobsID);
			if (mob != null && mob.IsValid && mob.IsAlive && mob.IsAttackable)
			{
				Trace("target=" + mob.Name);
				Interact.InteractGameObject(mob.GetBaseAddress, false, false, true);
				if (!MovementManager.IsFacing(ObjectManager.Me.Position, ObjectManager.Me.Rotation, mob.Position))
				{
					MovementManager.Face(mob);
					Thread.Sleep(Usefuls.Latency * 2);
				}
				return Pulse(mob.Position);
			}
			return Pulse(Vector3.Zero);
		}
		public static bool Pulse(Vector3 dest, float dist = 5f)
		{
			if (!Can)
				return false;

			_destination = dest;
			if (_destination != null && _destination != Vector3.Zero && ObjectManager.Me.Position.DistanceTo(dest) > dist)
			{
				Trace("goto " + _destination.ToStringNewVector());
				GoToTask.ToPosition(_destination, 9, false, (c) => { return Pulse(); });
			}
			return Pulse();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <returns>continue other actions or not</returns>
		public static bool Pulse()
		{
			if (!Can)
				return false;

			if (TryVindication())
				return false;

			if (TryCrusaderLeap())
				return false;

			TryPurifyingFlame();
			TryJudgmentBlast();
			return true;
		}
		public static bool TryCrusaderLeap()
		{
			if (_destination == null || _destination == Vector3.Zero)
				return false;

			var dist = ObjectManager.Me.Position.DistanceTo(_destination);
			if (dist > 10 && dist < 60 && Questing.NoCooldownSpell(251485))
			{
				Trace("#3 jump at targ=" + _destination.ToStringNewVector());
				Questing.Vehicle.UseButton(3);
				Thread.Sleep(Usefuls.Latency * 2);
				ClickOnTerrain.Pulse(_destination);
				Questing.Wait(2);
				return true;
			}
			return false;
		}
		public static bool TryVindication()
		{
			if (Questing.NoCooldownSpell(251569) && ObjectManager.Pet.HealthPercent < 50)
			{
				Trace("#5 heal");
				Questing.Vehicle.UseButton(5);
				Thread.Sleep(Usefuls.Latency * 2);
				Usefuls.WaitIsCasting();
				return true;
			}
			return false;
		}
		public static bool TryPurifyingFlame()
		{
			if (Questing.NoCooldownSpell(251509))
			{
				Trace("#2 attack");
				Questing.Vehicle.UseButton(2);
				return true;
			}
			return false;
		}
		public static bool TryJudgmentBlast()
		{
			if (Questing.NoCooldownSpell(248292))
			{
				//Trace("#1 attack");
				Questing.Vehicle.UseButton(1);
				return true;
			}
			return false;
		}
	}

	public class Invasion
	{
		public Invasion()
		{
		}
		internal static void Log(string text)
		{
			Logging.WriteDebug("[Argus Invasion] " + text);
		}
		public static class MobID
		{
			//greater boss
			public static int MatronFolnuna = 124514;
			public static int PitLordVilemus = 124719;
			public static int InquisitorMeto = 124592;
			public static int Occularus = 124492;
			public static int Sotanathor = 124555;
			public static int MistressAlluradel = 124625;
			public static List<int> BossesGreater = new List<int>() { MatronFolnuna, PitLordVilemus, InquisitorMeto, Occularus, Sotanathor, MistressAlluradel };
			//normal boss
			public static int Mazgoroth = 125137;
			public static int Gorgoloth = 125148;
			public static int DreadKnightZakgal = 125252;
			public static int FelLordKazral = 125272;
			public static int FlamecallerVezrah = 125280;
			public static int FlameweaverVerathix = 125314;
			public static int HarbingerDrelnathar = 125483;
			public static int DreadbringerValus = 125527;
			public static int Malphazel = 125578;
			public static int VogretharTheDefiled = 125587;
			public static int VelthrakThePunisher = 125634;
			public static int FlamebringerAzrothel = 125655;
			public static int Baldrazar = 125666;
			public static List<int> Bosses = new List<int>() { Mazgoroth, Gorgoloth, DreadKnightZakgal, FelLordKazral, FlamecallerVezrah, FlameweaverVerathix, HarbingerDrelnathar, DreadbringerValus, Malphazel, VogretharTheDefiled, VelthrakThePunisher, FlamebringerAzrothel, Baldrazar, };
			//mobs / common
			public static int BlacksoulInquisitor = 125779;
			public static int BladeswornDisciple = 125788;
			public static int ConqueringRiftstalker = 125199;
			public static int FelflameSubjugator = 125197;
			public static int FelrageMarauder = 125781;
			public static int WickedCorruptor = 125777;
			public static int WickedCorruptor2 = 125776;
			public static int BurningHound = 125936;
			public static int ChitteringFiend = 125921;
			public static int FelflameInvader = 125757;
			public static int FelflameInvader2 = 125755;
			public static int FelfrenzyBerserker = 125785;
			public static int FrothingRazorwing = 125933;
			public static int ShadowbladeAcolyte = 125790;
			public static int ShadowswornBetrayer = 125758;
			public static int ShadowswornBetrayer2 = 125759;
			public static int HatefulScamp = 125931;
			public static int DeepTerror = 126275;
			public static int WyrmtongueBloodhauler = 125963;
			public static int RedwoodGuardian = 127569;
			public static int RedwoodGuardian2 = 127611;
			public static int VengefulConqueror = 125745;
			public static int CrazedCorruptor = 126231;
			//mobs / specific
			public static int FragmentOfArgus = 125254;
			public static int ExplosiveOrb = 125656;
			public static int GrippingShadows = 125667;
			public static int ShadowyIllusion = 125579;
			public static List<int> Regular = new List<int>()
			{
				BlacksoulInquisitor,
				BladeswornDisciple,
				ConqueringRiftstalker,
				FelflameSubjugator,
				FelrageMarauder,
				WickedCorruptor,
				WickedCorruptor2,
				BurningHound,
				FelflameInvader,
				FelflameInvader2,
				FelfrenzyBerserker,
				FrothingRazorwing,
				ShadowbladeAcolyte,
				ShadowswornBetrayer,
				ShadowswornBetrayer2,
				HatefulScamp,
				DeepTerror,
				WyrmtongueBloodhauler,
				RedwoodGuardian,
				RedwoodGuardian2,
				VengefulConqueror,
				//
				FragmentOfArgus,
				ExplosiveOrb,
				GrippingShadows,
				ShadowyIllusion,
			};
		}
		public static bool InRift { get { return Usefuls.ContinentId == (int)ContinentId.Argus_Rifts; } }

		public static int MinBossHitpointsPercent = 95;
		public static int MinBossHitpoints = 2 * 1000 * 1000;

		#region QUESTS
		public class RiftBase
		{
			internal int _area = -1;
			internal int _portal = -1;
			internal List<int> _mobs = new List<int>();
			internal List<Vector3> _hotpots = new List<Vector3>();
			internal List<Vector3> _exit = new List<Vector3>();
			public RiftBase()
			{
				Questing.Group.Start(()=>InRift);
			}
			public virtual bool Pulse()
			{
				return true;
			}
			public virtual bool Exit()
			{
				var port = Questing.FindUnit(Portals.Out);
				if (port != null && port.IsValid)
				{
					Log("use portal exit");
					GoToTask.ToPositionAndIntecractWithNpc(port.Position, port.Entry);
				}
				else if (_exit.Count > 0)
				{
					var p = _exit[0];
					Log("to portal exit");
					GoToTask.ToPosition(p);
				}
				return true;
			}
		}
		public class RiftNormal : RiftBase
		{
			public override bool Pulse()
			{
				var stage = Questing.Scenario.Stage;
				if (stage == 3)
				{
					var boss = Questing.FindUnit(MobID.Bosses);
					if (boss != null && boss.IsValid && boss.IsAlive && boss.IsAttackable && (boss.HealthPercent < MinBossHitpointsPercent || boss.Health < MinBossHitpoints))
					{
						Log("attack boss " + boss.Name);
						Questing.Attack(boss);
					}
					else if (_hotpots.Count > 0)
					{
						var p = _hotpots[0];
						if (ObjectManager.Me.Position.DistanceTo2D(p) > 15)
						{
							Log("to boss camp at " + p.ToStringNewVector() );
							GoToTask.ToPosition(p, 10, false, (c)=>
							{
								if (ObjectManager.GetUnitAttackPlayer().Count > 0)
								{
									MountTask.DismountMount();
									return false;
								}
								return true;
							});
						}
					}
					return true;
				}
				else if (stage == 0)
				{
					Log("out");
					Exit();
				}
				else
				{
					Questing.Grind(_mobs, _hotpots);
				}
				return true;
			}
		}
		public class RiftGreater : RiftBase
		{
			internal float _range = 50;
			internal int _questID = -1;
			public override bool Pulse()
			{
				if (_questID < 0)
					Log("Greater Rift quest for area = " + _area + " doesnt have questID=" + _questID);

				if (_questID < 0 || Quest.GetQuestCompleted(_questID))
				{
					Exit();
					return true;
				}
				var boss = Questing.FindUnit(_mobs);
				if (boss != null && boss.IsValid && boss.IsAlive && boss.IsAttackable && (boss.HealthPercent < MinBossHitpointsPercent || boss.Health < MinBossHitpoints))
				{
					Log("attack greater boss " + boss.Name);
					Questing.Attack(boss);
				}
				else if (_hotpots.Count > 0)
				{
					var p = _hotpots[0];
					if (ObjectManager.Me.Position.DistanceTo2D(p) > _range)
					{
						Log("to greater boss camp");
						if (GoToTask.ToPosition(p))
						{
							MountTask.DismountMount();
						}
						return true;
					}
				}
				else
				{
					Log("Greater Rift quest for area = " + _area + " doesnt have hotspots=" + _hotpots.Count);
				}
				return true;
			}
		}
		#endregion QUESTS

		#region RIFTS NORMAL
		//tested 1
		public class Val : RiftNormal
		{
			public const int Area = 9127;
			public const int Portal = 126499;
			public static List<Vector3> Exits = new List<Vector3>() { new Vector3(-4170.895, 663.2453, 20.1905, "None"), };
			public static List<Vector3> Hotspots = new List<Vector3>()
			{
				new Vector3(-4102.611, 602.3239, 9.893845, "None"),
				new Vector3(-4004.833, 584.2082, 0.05630473, "None"),
				new Vector3(-4066.593, 605.5196, 4.601417, "None"),
				new Vector3(-4015.308, 600.5054, 2.307814, "None"),
			};
			public static List<int> Mobs = MobID.Regular;
			public static bool IsInside { get { return InRift && Usefuls.AreaId == Area; } }
			public Val() { _area = Area; _hotpots = Hotspots; _exit = Exits; _mobs = Mobs; _portal = Portal; }
			public override bool Pulse()
			{
				var stage = Questing.Scenario.Stage;
				if (stage == 2)
				{
					var demonhunter = Questing.FindUnit(126446);
					if (demonhunter != null && demonhunter.IsValid && demonhunter.IsAlive)
					{
						Log("interact=" + demonhunter.Name);
						GoToTask.ToPositionAndIntecractWithNpc(demonhunter.Position, demonhunter.Entry);
					}
					else
					{
						var i = Others.Random(0, Hotspots.Count - 1);
						var p = Hotspots[i];
						Log("to hotspot #" + i + " at=" + p.ToStringNewVector());
						GoToTask.ToPosition(p);
					}
					return true;
				}
				return base.Pulse();
			}
		}
		//tested: 2
		public class Cengar : RiftNormal
		{
			public const int Area = 9126;
			public const int Portal = 126120;
			public static List<Vector3> Exits = new List<Vector3>() { new Vector3(654.0816, 596.4531, 40.225), };
			public static List<Vector3> Hotspots = new List<Vector3>() { new Vector3(664.9532, 643.9272, 40.22417, "None"), };
			public static List<int> Mobs = MobID.Regular;
			public static bool IsInside { get { return InRift && Usefuls.AreaId == Area; } }
			public static float MinZ = 39.25407f;//bellow this Z -> in lava, force run to near hotspot
			public Cengar()
			{
				_area = Area;
				_hotpots = Hotspots;
				_exit = Exits;
				_mobs = Mobs;
				_portal = Portal;
				StartCengarFix();
			}
			void StartCengarFix()
			{
				StopCengarFix();
				wManager.Events.FightEvents.OnFightLoop += OnFightLoop;
				Log("start cengar fight fix");
			}
			void StopCengarFix()
			{
				wManager.Events.FightEvents.OnFightLoop -= OnFightLoop;
				Log("stop cengar fight fix");
			}
			void OnFightLoop(WoWUnit unit, System.ComponentModel.CancelEventArgs cancelable)
			{
				if (!IsInside)
				{
					StopCengarFix();
					return;
				}
				if (ObjectManager.Me.Position.Z < MinZ)
				{
					cancelable.Cancel = true;
					var p = _hotpots.OrderBy(v => ObjectManager.Me.Position.DistanceTo2D(v)).FirstOrDefault();
					if (p != null && p != Vector3.Zero)
					{
						//GoToTask.ToPosition(p);
						Log("get out from lava");
						Questing.ClickMove(p);
						Questing.Wait(5);
					}
				}
			}
			public override bool Pulse()
			{
				var stage = Questing.Scenario.Stage;
				if (stage == 1)
				{
					Questing.Grind(new List<int>() { 126230 }, _hotpots);
					return true;
				}
				else if (stage == 2)
				{
					//kill fire giants without buffs
					//var mob = ObjectManager.GetObjectWoWUnit().Where(u => u.IsValid && u.Entry == 124835 && u.IsAlive && u.IsAttackable && !u.HaveBuff(251888)).OrderBy(o => o.GetDistance).FirstOrDefault();
					//Questing.Attack(mob);
					Questing.Grind(new List<int>() { 124835 }, _hotpots);
					return true;
				}
				return base.Pulse();
			}
		}
		//tested 2
		public class Aurinor : RiftNormal
		{
			public const int Area = 9100;
			public const int Portal = 125849;
			public static List<Vector3> Exits = new List<Vector3>() { new Vector3(-4066.524, -4659.221, 80.46143), };
			public static List<Vector3> Hotspots = new List<Vector3>() { new Vector3(-4033.093, -4945.625, 121.7319, "None"), new Vector3(-4076.119, -4868.174, 112.4614, "None"), new Vector3(-4073.653, -4759.229, 85.88012, "None"), };
			public static List<int> Mobs = MobID.Regular;
			public static bool IsInside { get { return InRift && Usefuls.AreaId == Area; } }
			public Aurinor()
			{
				_area = Area;
				_hotpots = Hotspots;
				_exit = Exits;
				_mobs = Mobs;
				_portal = Portal;
				StartPathfindingFix();
			}
			public override bool Pulse()
			{
				var stage = Questing.Scenario.Stage;
				if (stage == 2)
				{
					Questing.GatherInteractKill(_hotpots, 125856);
					//Questing.Grind(new List<int>() { 125856 }, _hotpots);
					return true;
				}
				else if (stage == 3)
				{
					_hotpots = new List<Vector3>() { new Vector3(-4061.876, -4688.732, 80.46176, "None"), };
				}
				return base.Pulse();
			}
			static void StartPathfindingFix()
			{
				StopPathfindingFix();
				wManager.Events.MovementEvents.OnMovementPulse += OnMovementPulse;
				Log("Aurinor path fix start");
			}
			static void StopPathfindingFix()
			{
				wManager.Events.MovementEvents.OnMovementPulse -= OnMovementPulse;
				Log("Aurinor path fix stop");
			}
			static void OnMovementPulse(List<Vector3> points, System.ComponentModel.CancelEventArgs cancelable)
			{
				if (!IsInside)
				{
					StopPathfindingFix();
					return;
				}
				if (points == null)
					return;
				var count = points.Count;
				if (count < 2)
					return;

				var start = points[0];
				var end = points[count - 1];
				if (!TraceLine.TraceLineGo(end))
					return;

				points.Clear();
				var pathMain = Questing.PathClampDirected(Path, start, end);
				if (pathMain.Count < 1)
					return;

				var startMain = pathMain[0];
				var countStart = 0;
				if (TraceLine.TraceLineGo(startMain))
				{
					var pathStart = PathFinder.FindPath(start, startMain);
					countStart = pathStart.Count;
					points.AddRange(pathStart);
				}
				else
				{
					points.Add(start);
				}
				points.AddRange(pathMain);
				var mainEnd = pathMain[pathMain.Count - 1];
				var countEnd = 0;
				if (TraceLine.TraceLineGo(mainEnd))
				{
					var pathEnd = PathFinder.FindPath(mainEnd, end);
					countEnd = pathEnd.Count;
					points.AddRange(pathEnd);
				}
				Log("Aurinor path fixed. from=" + count + " to=" + points.Count + " start=" + countStart + " main=" + pathMain.Count + " end=" + countEnd);
			}
			static List<Vector3> Path = new List<Vector3>()
			{
				new Vector3(-4025.564f, -4992.333f, 128.8848f, "None"),
				new Vector3(-4016.318f, -4983.517f, 128.8458f, "None"),
				new Vector3(-4011.164f, -4978.282f, 127.8125f, "None"),
				new Vector3(-4007.015f, -4972.218f, 125.9949f, "None"),
				new Vector3(-4004.714f, -4965.212f, 124.1661f, "None"),
				new Vector3(-4004.234f, -4957.897f, 122.6991f, "None"),
				new Vector3(-4006.113f, -4950.851f, 121.2496f, "None"),
				new Vector3(-4009.024f, -4944.099f, 119.8396f, "None"),
				new Vector3(-4013.073f, -4938.024f, 118.5348f, "None"),
				new Vector3(-4017.793f, -4932.315f, 117.0585f, "None"),
				new Vector3(-4022.745f, -4926.983f, 115.9411f, "None"),
				new Vector3(-4027.783f, -4921.612f, 115.2574f, "None"),
				new Vector3(-4032.689f, -4916.067f, 114.6451f, "None"),
				new Vector3(-4036.646f, -4909.876f, 113.8957f, "None"),
				new Vector3(-4040.125f, -4903.438f, 113.1196f, "None"),
				new Vector3(-4043.048f, -4896.679f, 112.6042f, "None"),
				new Vector3(-4045.94f, -4889.906f, 112.1279f, "None"),
				new Vector3(-4049.167f, -4883.274f, 111.582f, "None"),
				new Vector3(-4053.404f, -4877.33f, 111.3725f, "None"),
				new Vector3(-4057.894f, -4871.585f, 111.2205f, "None"),
				new Vector3(-4062.85f, -4866.124f, 110.8011f, "None"),
				new Vector3(-4074.533f, -4857.14f, 110.0281f, "None"),
				new Vector3(-4081.032f, -4853.886f, 109.3773f, "None"),
				new Vector3(-4087.624f, -4850.769f, 108.5416f, "None"),
				new Vector3(-4094.077f, -4847.196f, 107.9549f, "None"),
				new Vector3(-4099.733f, -4842.314f, 107.8856f, "None"),
				new Vector3(-4103.973f, -4836.387f, 107.882f, "None"),
				new Vector3(-4106.978f, -4829.721f, 107.8183f, "None"),
				new Vector3(-4109.365f, -4822.818f, 107.0545f, "None"),
				new Vector3(-4112.172f, -4815.994f, 105.9499f, "None"),
				new Vector3(-4111.906f, -4808.731f, 104.8956f, "None"),
				new Vector3(-4117.56f, -4799.142f, 104.3143f, "None"),
				new Vector3(-4122.608f, -4793.928f, 103.5937f, "None"),
				new Vector3(-4126.129f, -4787.473f, 102.1221f, "None"),
				new Vector3(-4127.136f, -4780.277f, 100.7896f, "None"),
				new Vector3(-4124.586f, -4773.502f, 99.35542f, "None"),
				new Vector3(-4118.481f, -4769.553f, 97.54041f, "None"),
				new Vector3(-4111.322f, -4768.311f, 95.11562f, "None"),
				new Vector3(-4103.973f, -4767.881f, 92.26426f, "None"),
				new Vector3(-4089.338f, -4768.954f, 88.09329f, "None"),
				new Vector3(-4082.054f, -4769.608f, 87.20068f, "None"),
				new Vector3(-4075.191f, -4767.11f, 86.60323f, "None"),
				new Vector3(-4071.413f, -4760.931f, 85.86458f, "None"),
				new Vector3(-4069.637f, -4753.786f, 85.11414f, "None"),
				new Vector3(-4069.093f, -4746.534f, 84.6414f, "None"),
				new Vector3(-4068.798f, -4739.189f, 83.94742f, "None"),
				new Vector3(-4068.51f, -4732.036f, 82.89617f, "None"),
				new Vector3(-4068.209f, -4724.575f, 81.82955f, "None"),
				new Vector3(-4067.519f, -4717.11f, 80.70802f, "None"),
				new Vector3(-4067.115f, -4709.935f, 80.4714f, "None"),
				new Vector3(-4066.892f, -4702.559f, 80.46216f, "None"),
				new Vector3(-4066.982f, -4695.209f, 80.46216f, "None"),
				new Vector3(-4067.169f, -4687.832f, 80.46216f, "None"),
			};
		}
		//tested 2
		public class Naigtal : RiftNormal
		{
			public const int Area = 9102;
			public const int Portal = 126593;
			public static List<Vector3> Exits = new List<Vector3>() { new Vector3(-1753.264, -1410.38, 27.798), };
			public static List<Vector3> Hotspots = new List<Vector3>()
			{
				new Vector3(-1781.135, -1449.18, 16.94704, "None"),
				new Vector3(-1787.167, -1532.51, 4.557395, "None"),
				new Vector3(-1898.394, -1515.631, 7.051389, "None"),
			};
			public static List<int> Mobs = MobID.Regular;
			public static bool IsInside { get { return InRift && Usefuls.AreaId == Area; } }
			public Naigtal() { _area = Area; _hotpots = Hotspots; _exit = Exits; _mobs = Mobs; _portal = Portal; }
			public override bool Pulse()
			{
				var stage = Questing.Scenario.Stage;
				if (stage == 1)
				{
					Questing.Grind(new List<int>() { 125958 }, _hotpots);
					return true;
				}
				else if (stage == 3)
				{
					_hotpots = new List<Vector3>() { new Vector3(-1858.271, -1565.391, -0.2610433, "None"), };
				}
				return base.Pulse();
			}
		}
		//tested 2
		public class Sangua : RiftNormal
		{
			public const int Area = 9128;
			public const int Portal = 125863;
			public static List<Vector3> Exits = new List<Vector3>() { new Vector3(-1390.74, 761.0278, 62.40871), };
			public static List<Vector3> Hotspots = new List<Vector3>()
			{
				new Vector3(-1481.606, 759.5313, 61.09289, "None"),
				new Vector3(-1413.726, 775.7604, 61.09098, "None"),
				new Vector3(-1390.267, 734.3663, 61.08427, "None"),
				new Vector3(-1340.552, 815.0903, 61.09292, "None"),
				new Vector3(-1481.606, 759.5313, 61.09289, "None"),
				new Vector3(-1413.726, 775.7604, 61.09098, "None"),
				new Vector3(-1390.267, 734.3663, 61.08427, "None"),
				new Vector3(-1340.552, 815.0903, 61.09292, "None"),
			};
			public static List<int> Mobs = MobID.Regular;
			public static bool IsInside { get { return InRift && Usefuls.AreaId == Area; } }
			public Sangua() { _area = Area; _hotpots = Hotspots; _exit = Exits; _mobs = Mobs; _portal = Portal; }
			public override bool Pulse()
			{
				var stage = Questing.Scenario.Stage;
				if (stage == 1)
				{
					Questing.Grind(new List<int>() { 125874 }, _hotpots);
					return true;
				}
				else if (stage == 3)
				{
					_hotpots = new List<Vector3>() { new Vector3(-1440.662, 793.8834, 64.87183, "None"), }; // old { new Vector3(-3750.61, -8077.96, 1.104051, "None"), };
				}
				return base.Pulse();
			}
		}
		//tested 2
		public class Bonich : RiftNormal
		{
			public const int Area = 9180;
			public const int Portal = 126547;
			public static List<Vector3> Exits = new List<Vector3>() { };
			public static List<Vector3> Hotspots = new List<Vector3>()
			{
				new Vector3(-3795.264, -8127.464, 7.570378, "None"),
				new Vector3(-3714.877, -8076.997, 0.8833292, "None"),
				new Vector3(-3813.733, -8045.342, 0.9020075, "None"),
				new Vector3(-3686.788, -8045.567, 3.918347, "None"),
				new Vector3(-3742.601, -7942.385, 1.102061, "None"),
				new Vector3(-3747.285, -7953.367, 0.9858366, "None"),
			};
			public static List<int> Mobs = MobID.Regular;
			public static bool IsInside { get { return InRift && Usefuls.AreaId == Area; } }
			public Bonich() { _area = Area; _hotpots = Hotspots; _exit = Exits; _mobs = Mobs; _portal = Portal; }
			public override bool Pulse()
			{
				var stage = Questing.Scenario.Stage;
				if (stage == 2)
				{
					Questing.Grind(new List<int>() { 127982 }, _hotpots);
					return true;
				}
				else if (stage == 3)
				{
					_hotpots = new List<Vector3>() { new Vector3(-3743.218, -8061.853, 0.889137, "None"), };
				}
				return base.Pulse();
			}
		}
		#endregion RIFTS NORMAL

		#region RIFTS GREATER
		public class MatronFolnuna : RiftGreater
		{
			public const int Area = 9295;
			public const int Portal = 127528;
			public const int QuestID = 49169;
			public const int QuestBonusLootID = 49173;
			public static List<Vector3> Exits = new List<Vector3>() { new Vector3(4498.033, 6591.74, 41.91304), };
			public static List<Vector3> Hotspots = new List<Vector3>() { new Vector3(4425.705, 6565.272, 41.96574, "None"), };
			public static List<int> Mobs = MobID.BossesGreater;
			public static bool IsInside { get { return InRift && Usefuls.AreaId == Area; } }
			public MatronFolnuna() { _area = Area; _hotpots = Hotspots; _exit = Exits; _mobs = Mobs; _portal = Portal; _questID = QuestID; }
		}
		public class PitLordVilemus : RiftGreater
		{
			public const int Area = 9296;
			public const int Portal = 127531;
			public const int QuestID = 49168;
			public const int QuestBonusLootID = 49174;
			public static List<Vector3> Exits = new List<Vector3>() { new Vector3(-9886.21, -4989.61, 128.8879, "None") };
			public static List<Vector3> Hotspots = new List<Vector3>() { new Vector3(-9935.045, -4731.848, 82.86861, "None"), };
			public static List<int> Mobs = MobID.BossesGreater;
			public static bool IsInside { get { return InRift && Usefuls.AreaId == Area; } }
			public PitLordVilemus()
			{
				_area = Area;
				_hotpots = Hotspots;
				_exit = Exits;
				_mobs = Mobs;
				_portal = Portal;
				_questID = QuestID;
				StartPathfindingFix();
			}
			static void StartPathfindingFix()
			{
				StopPathfindingFix();
				wManager.Events.MovementEvents.OnMovementPulse += OnMovementPulse;
				Log("PitLordVilemus path fix start");
			}
			static void StopPathfindingFix()
			{
				wManager.Events.MovementEvents.OnMovementPulse -= OnMovementPulse;
				Log("PitLordVilemus path fix stop");
			}
			static void OnMovementPulse(List<Vector3> points, System.ComponentModel.CancelEventArgs cancelable)
			{
				if (!IsInside)
				{
					StopPathfindingFix();
					return;
				}
				if (points == null)
					return;
				var count = points.Count;
				if (count < 2)
					return;

				var start = points[0];
				var end = points[count - 1];
				if (!TraceLine.TraceLineGo(end))
					return;

				points.Clear();
				var pathMain = Questing.PathClampDirected(Path, start, end);
				if (pathMain.Count < 1)
					return;

				var startMain = pathMain[0];
				var countStart = 0;
				if (TraceLine.TraceLineGo(startMain))
				{
					var pathStart = PathFinder.FindPath(start, startMain);
					countStart = pathStart.Count;
					points.AddRange(pathStart);
				}
				else
				{
					points.Add(start);
				}
				points.AddRange(pathMain);
				var mainEnd = pathMain[pathMain.Count - 1];
				var countEnd = 0;
				if (TraceLine.TraceLineGo(mainEnd))
				{
					var pathEnd = PathFinder.FindPath(mainEnd, end);
					countEnd = pathEnd.Count;
					points.AddRange(pathEnd);
				}
				Log("PitLordVilemus path fixed. from=" + count + " to=" + points.Count + " start=" + countStart + " main=" + pathMain.Count + " end=" + countEnd);
			}
			static List<Vector3> Path = new List<Vector3>()
			{
				new Vector3(-9895.073f, -4993.136f, 128.9525f, "None"),
				new Vector3(-9891.176f, -4991.042f, 128.8877f, "None"),
				new Vector3(-9885.823f, -4986.071f, 128.8877f, "None"),
				new Vector3(-9880.358f, -4981.074f, 128.3283f, "None"),
				new Vector3(-9876.253f, -4975.14f, 127.1056f, "None"),
				new Vector3(-9874.235f, -4968.039f, 124.8389f, "None"),
				new Vector3(-9874.273f, -4960.813f, 123.2723f, "None"),
				new Vector3(-9875.793f, -4953.637f, 121.9737f, "None"),
				new Vector3(-9877.415f, -4946.423f, 120.3732f, "None"),
				new Vector3(-9880.268f, -4939.604f, 118.8457f, "None"),
				new Vector3(-9884.543f, -4933.577f, 117.3068f, "None"),
				new Vector3(-9889.183f, -4928.01f, 116.1232f, "None"),
				new Vector3(-9893.92f, -4922.39f, 115.3389f, "None"),
				new Vector3(-9898.034f, -4916.259f, 114.7066f, "None"),
				new Vector3(-9901.428f, -4909.727f, 113.9191f, "None"),
				new Vector3(-9904.403f, -4903.087f, 113.1758f, "None"),
				new Vector3(-9907.352f, -4896.338f, 112.6616f, "None"),
				new Vector3(-9910.326f, -4889.489f, 112.1144f, "None"),
				new Vector3(-9913.648f, -4883.055f, 111.594f, "None"),
				new Vector3(-9917.648f, -4876.878f, 111.3558f, "None"),
				new Vector3(-9922.398f, -4871.315f, 111.2285f, "None"),
				new Vector3(-9927.703f, -4866.209f, 110.8939f, "None"),
				new Vector3(-9933.28f, -4861.383f, 110.3457f, "None"),
				new Vector3(-9939.375f, -4857.229f, 110.0809f, "None"),
				new Vector3(-9946.01f, -4854.209f, 109.5511f, "None"),
				new Vector3(-9952.929f, -4851.728f, 108.7257f, "None"),
				new Vector3(-9959.705f, -4848.9f, 108.0327f, "None"),
				new Vector3(-9965.942f, -4845.048f, 107.9101f, "None"),
				new Vector3(-9970.549f, -4839.472f, 107.8817f, "None"),
				new Vector3(-9974.035f, -4832.985f, 107.8817f, "None"),
				new Vector3(-9977.332f, -4826.367f, 107.47f, "None"),
				new Vector3(-9979.074f, -4819.524f, 106.5224f, "None"),
				new Vector3(-9981.977f, -4812.725f, 105.6733f, "None"),
				new Vector3(-9985.642f, -4806.546f, 105.0965f, "None"),
				new Vector3(-9989.23f, -4799.983f, 104.3912f, "None"),
				new Vector3(-9992.361f, -4793.366f, 103.4006f, "None"),
				new Vector3(-9995.295f, -4786.661f, 102.2199f, "None"),
				new Vector3(-9992.685f, -4780.304f, 100.6021f, "None"),
				new Vector3(-9986.992f, -4775.801f, 98.55713f, "None"),
				new Vector3(-9980.146f, -4773.397f, 96.17544f, "None"),
				new Vector3(-9972.811f, -4772.361f, 93.26927f, "None"),
				new Vector3(-9965.517f, -4771.746f, 90.73256f, "None"),
				new Vector3(-9958.146f, -4771.156f, 88.72659f, "None"),
				new Vector3(-9950.928f, -4770.394f, 87.4444f, "None"),
				new Vector3(-9943.833f, -4768.343f, 86.77097f, "None"),
				new Vector3(-9939.582f, -4762.599f, 86.16426f, "None"),
				new Vector3(-9937.26f, -4755.637f, 85.29163f, "None"),
				new Vector3(-9936.295f, -4748.368f, 84.7927f, "None"),
				new Vector3(-9935.658f, -4740.958f, 84.13364f, "None"),
				new Vector3(-9935.04f, -4735.932f, 83.50744f, "None"),
				new Vector3(-9934.376f, -4729.837f, 82.64214f, "None"),
				new Vector3(-9933.797f, -4726.31f, 82.09087f, "None"),
				new Vector3(-9933.335f, -4722.854f, 81.4575f, "None"),
				new Vector3(-9932.85f, -4719.311f, 80.87955f, "None"),
				new Vector3(-9932.428f, -4715.843f, 80.59319f, "None"),
				new Vector3(-9932.31f, -4708.95f, 80.46386f, "None"),
				new Vector3(-9932.301f, -4705.415f, 80.46181f, "None"),
				new Vector3(-9932.45f, -4701.828f, 80.46181f, "None"),
				new Vector3(-9932.816f, -4694.859f, 80.46181f, "None"),
				new Vector3(-9932.987f, -4687.917f, 80.46181f, "None"),
				new Vector3(-9933.096f, -4682.759f, 80.46181f, "None"),
				new Vector3(-9933.219f, -4676.936f, 80.46181f, "None"),
				new Vector3(-9933.298f, -4673.233f, 80.46181f, "None"),
			};
		}
		public class InquisitorMeto : RiftGreater
		{
			public const int Area = 9297;
			public const int Portal = 127535;
			public const int QuestID = 49166;
			public const int QuestBonusLootID = -1;
			public static List<Vector3> Exits = new List<Vector3>() { new Vector3(5710.35, -1414.885, 29.43402, "None"), };
			public static List<Vector3> Hotspots = new List<Vector3>() { new Vector3(5640.993, -1549.726, 1.816501, "None"), };
			public static List<int> Mobs = MobID.BossesGreater;
			public static bool IsInside { get { return InRift && Usefuls.AreaId == Area; } }
			public InquisitorMeto() { _area = Area; _hotpots = Hotspots; _exit = Exits; _mobs = Mobs; _portal = Portal; _questID = QuestID; }
		}
		public class Occularus : RiftGreater
		{
			public const int Area = 9298;
			public const int Portal = 127533;
			public const int QuestID = 49170;
			public const int QuestBonusLootID = -1;
			public static List<Vector3> Exits = new List<Vector3>() { new Vector3(-10032.4, 2787.61, 18.74589, "None"), };
			public static List<Vector3> Hotspots = new List<Vector3>() { new Vector3(-9966.898, 2736.701, 9.42625, "None"), };
			public static List<int> Mobs = MobID.BossesGreater;
			public static bool IsInside { get { return InRift && Usefuls.AreaId == Area; } }
			public Occularus() { _area = Area; _hotpots = Hotspots; _exit = Exits; _mobs = Mobs; _portal = Portal; _questID = QuestID; }
		}
		//tested 1
		public class Sotanathor : RiftGreater
		{
			public const int Area = 9299;
			public const int Portal = 127532;
			public const int QuestID = 49171;	//good
			public const int QuestBonusLootID = -1;
			public static List<Vector3> Exits = new List<Vector3>() { new Vector3(-1385.455, 8355.195, 94.52447, "None"), };
			public static List<Vector3> Hotspots = new List<Vector3>() { new Vector3(-1431.579, 8236.357, 62.15427, "None"), };
			public static List<int> Mobs = MobID.BossesGreater;
			public static bool IsInside { get { return InRift && Usefuls.AreaId == Area; } }
			public Sotanathor() { _area = Area; _hotpots = Hotspots; _exit = Exits; _mobs = Mobs; _portal = Portal; _questID = QuestID; }
		}
		public class MistressAlluradel : RiftGreater
		{
			public const int Area = 9300;
			public const int Portal = 127536;
			public const int QuestID = 49167;
			public const int QuestBonusLootID = -1;
			public static List<Vector3> Exits = new List<Vector3>() { new Vector3(5259.431, -9772.651, 10.80953), };
			public static List<Vector3> Hotspots = new List<Vector3>() { new Vector3(5303.167, -9685.952, 0.9761651, "None"), };
			public static List<int> Mobs = MobID.BossesGreater;
			public static bool IsInside { get { return InRift && Usefuls.AreaId == Area; } }
			public MistressAlluradel() { _area = Area; _hotpots = Hotspots; _exit = Exits; _mobs = Mobs; _portal = Portal; _questID = QuestID; }
		}
		#endregion RIFTS GREATER

		#region LOGIC
		internal static RiftBase _current;
		public static bool Work()
		{
			if (!InRift)
			{
				_current = null;
				return false;
			}

			if (_current == null)
			{
				switch (Usefuls.AreaId)
				{
					//normal
					case Cengar.Area:
						Log("started Cengar");
						_current = new Cengar();
						break;
					case Naigtal.Area:
						Log("started Naigtal");
						_current = new Naigtal();
						break;
					case Val.Area:
						Log("started Val");
						_current = new Val();
						break;
					case Sangua.Area:
						Log("started Sangua");
						_current = new Sangua();
						break;
					case Bonich.Area:
						Log("started Bonich");
						_current = new Bonich();
						break;
					case Aurinor.Area:
						Log("started Aurinor");
						_current = new Aurinor();
						break;
					//greater
					case MatronFolnuna.Area:
						Log("started MatronFolnuna");
						_current = new MatronFolnuna();
						break;
					case PitLordVilemus.Area:
						Log("started PitLordVilemus");
						_current = new PitLordVilemus();
						break;
					case InquisitorMeto.Area:
						Log("started InquisitorMeto");
						_current = new InquisitorMeto();
						break;
					case Occularus.Area:
						Log("started Occularus");
						_current = new Occularus();
						break;
					case Sotanathor.Area:
						Log("started Sotanathor");
						_current = new Sotanathor();
						break;
					case MistressAlluradel.Area:
						Log("started MistressAlluradel");
						_current = new MistressAlluradel();
						break;
					default:
						Log("unknown rift area " + Usefuls.AreaId);
						return true;
				}
			}
			_current.Pulse();
			return true;
		}
		#endregion LOGIC

		public static class Portals
		{
			public static float NearRange = 100f;
			public static string IconRift = "poi-rift1";
			public static string IconRiftGreater = "poi-rift2";

			static List<Vector3> CurrentKrokuun = new List<Vector3>();
			static List<Vector3> CurrentKrokuunGreater = new List<Vector3>();
			static List<Vector3> CurrentAntoranWastes = new List<Vector3>();
			static List<Vector3> CurrentAntoranWastesGreater = new List<Vector3>();
			static List<Vector3> CurrentMacAree = new List<Vector3>();
			static List<Vector3> CurrentMacAreeGreater = new List<Vector3>();

			static robotManager.Helpful.Timer TimerKrokuun = new robotManager.Helpful.Timer(30 * 1000);
			static robotManager.Helpful.Timer TimerKrokuunGreater = new robotManager.Helpful.Timer(30 * 1000);
			static robotManager.Helpful.Timer TimerAntoranWastes = new robotManager.Helpful.Timer(30 * 1000);
			static robotManager.Helpful.Timer TimerAntoranWastesGreater = new robotManager.Helpful.Timer(30 * 1000);
			static robotManager.Helpful.Timer TimerMacAree = new robotManager.Helpful.Timer(30 * 1000);
			static robotManager.Helpful.Timer TimerMacAreeGreater = new robotManager.Helpful.Timer(30 * 1000);
			static Portals()
			{
				TimerKrokuun.ForceReady();
				TimerKrokuunGreater.ForceReady();
				TimerAntoranWastes.ForceReady();
				TimerAntoranWastesGreater.ForceReady();
				TimerMacAree.ForceReady();
				TimerMacAreeGreater.ForceReady();
			}
			public static bool UseNear()
			{
				var port = FindNear();
				if (port != null && port.IsValid)
				{
					Log("found invasion portal " + port.Name + " at=" + port.Position.ToStringNewVector());
					GoToTask.ToPositionAndIntecractWithNpc(port.Position, port.Entry);
					return true;
				}
				return false;
			}
			public static WoWUnit FindNear(Vector3 p = null)
			{
				if (p == null || p == Vector3.Zero)
					p = ObjectManager.Me.Position;

				var all = new List<int>(Minor);
				all.AddRange(Greater);
				var port = Questing.FindUnit(all);
				if (IsActive(port))
					return port;

				return null;
			}
			public static bool IsActive(WoWUnit port)
			{
				if (port == null || !port.IsValid)
					return false;

				return IsActive(port.Position);
			}
			public static bool IsActive(Vector3 p)
			{
				if (p == null || p == Vector3.Zero)
					return false;

				var hotspot = GetNearActive(p);
				if (hotspot != null && hotspot != Vector3.Zero)
					return true;

				return false;
			}
			public static Vector3 GetNearActive(Vector3 p)
			{
				if (p == null || p == Vector3.Zero)
					p = ObjectManager.Me.Position;

				Update();

				Vector3 hotspot = null;

				hotspot = GetNearFromList(p, CurrentKrokuun);
				if (hotspot != null && hotspot != Vector3.Zero)
					return hotspot;

				hotspot = GetNearFromList(p, CurrentKrokuunGreater);
				if (hotspot != null && hotspot != Vector3.Zero)
					return hotspot;

				hotspot = GetNearFromList(p, CurrentAntoranWastes);
				if (hotspot != null && hotspot != Vector3.Zero)
					return hotspot;

				hotspot = GetNearFromList(p, CurrentAntoranWastesGreater);
				if (hotspot != null && hotspot != Vector3.Zero)
					return hotspot;

				hotspot = GetNearFromList(p, CurrentMacAree);
				if (hotspot != null && hotspot != Vector3.Zero)
					return hotspot;

				hotspot = GetNearFromList(p, CurrentMacAreeGreater);
				if (hotspot != null && hotspot != Vector3.Zero)
					return hotspot;

				return null;
			}
			public static Vector3 GetNearFromList(Vector3 p, List<Vector3> list)
			{
				return list.OrderBy(v => v.DistanceTo2D(p)).FirstOrDefault(v => v.DistanceTo2D(p) < NearRange);
			}
			public static Vector3 GetNearHotspot(Vector3 p)
			{
				if (p == null || p == Vector3.Zero)
					p = ObjectManager.Me.Position;

				Vector3 hotspot = null;
				
				hotspot = GetNearFromList(p, KrokuunHotspots);
				if (hotspot != null && hotspot != Vector3.Zero)
					return hotspot;

				hotspot = GetNearFromList(p, AntoranWastesHotspots);
				if (hotspot != null && hotspot != Vector3.Zero)
					return hotspot;

				hotspot = GetNearFromList(p, MacAreeHotspots);
				if (hotspot != null && hotspot != Vector3.Zero)
					return hotspot;

				return null;
			}
			public static int CountRegular
			{
				get
				{
					GetKrokuun();
					GetAntoranWastes();
					GetMacAree();
					return CurrentKrokuun.Count + CurrentAntoranWastes.Count + CurrentMacAree.Count;
				}
			}
			public static int CountGreater
			{
				get
				{
					GetKrokuunGreater();
					GetAntoranWastesGreater();
					GetMacAreeGreater();
					return CurrentKrokuunGreater.Count + CurrentAntoranWastesGreater.Count + CurrentMacAreeGreater.Count;
				}
			}
			public static int CountAll
			{
				get
				{
					return CountRegular + CountGreater;
				}
			}
			public static void UpdateKrokuun()
			{
				GetKrokuun();
				GetKrokuunGreater();
			}
			public static void UpdateAntotanWastes()
			{
				GetAntoranWastes();
				GetAntoranWastesGreater();
			}
			public static void UpdateMacAree()
			{
				GetMacAree();
				GetMacAreeGreater();
			}
			public static void UpdateRegular()
			{
				GetKrokuun();
				GetAntoranWastes();
				GetMacAree();
			}
			public static void UpdateGreater()
			{
				GetKrokuunGreater();
				GetAntoranWastesGreater();
				GetMacAreeGreater();
			}
			public static void Update()
			{
				UpdateRegular();
				UpdateGreater();
			}
			static List<Vector3> CorrectMapPositions(List<Vector3> mapPositions, List<Vector3> precisePositions)
			{
				var result = new List<Vector3>();
				foreach (var mapP in mapPositions)
				{
					var near = GetNearFromList(mapP, precisePositions);
					if (near != null && near != Vector3.Zero)
						result.Add(near);
					else
						result.Add(mapP);
				}
				return result;
			}
			public static List<Vector3> GetKrokuun()
			{
				if (TimerKrokuun.IsReady)
				{
					TimerKrokuun.Reset();
					var mapPositions = Questing.Map.GetLandMarks(MAP_KROKUUN, IconRift);
					CurrentKrokuun = CorrectMapPositions(mapPositions, KrokuunHotspots);
				}
				return CurrentKrokuun;
			}
			public static List<Vector3> GetKrokuunGreater()
			{
				if (TimerKrokuunGreater.IsReady)
				{
					TimerKrokuunGreater.Reset();
					var mapPositions = Questing.Map.GetLandMarks(MAP_KROKUUN, IconRiftGreater);
					CurrentKrokuunGreater = CorrectMapPositions(mapPositions, KrokuunHotspots);
				}
				return CurrentKrokuunGreater;
			}
			public static List<Vector3> GetAntoranWastes()
			{
				if (TimerAntoranWastes.IsReady)
				{
					TimerAntoranWastes.Reset();
					var mapPositions = Questing.Map.GetLandMarks(MAP_ANTORAN_WASTES, IconRift);
					CurrentAntoranWastes = CorrectMapPositions(mapPositions, AntoranWastesHotspots);
				}
				return CurrentAntoranWastes;
			}
			public static List<Vector3> GetAntoranWastesGreater()
			{
				if (TimerAntoranWastesGreater.IsReady)
				{
					TimerAntoranWastesGreater.Reset();
					var mapPositions = Questing.Map.GetLandMarks(MAP_ANTORAN_WASTES, IconRiftGreater);
					CurrentAntoranWastesGreater = CorrectMapPositions(mapPositions, AntoranWastesHotspots);
				}
				return CurrentAntoranWastesGreater;
			}
			public static List<Vector3> GetMacAree()
			{
				if (TimerMacAree.IsReady)
				{
					TimerMacAree.Reset();
					var mapPositions = Questing.Map.GetLandMarks(MAP_MACAREE, IconRift);
					CurrentMacAree = CorrectMapPositions(mapPositions, MacAreeHotspots);
				}
				return CurrentMacAree;
			}
			public static List<Vector3> GetMacAreeGreater()
			{
				if (TimerMacAreeGreater.IsReady)
				{
					TimerMacAreeGreater.Reset();
					var mapPositions = Questing.Map.GetLandMarks(MAP_MACAREE, IconRiftGreater);
					CurrentMacAreeGreater = CorrectMapPositions(mapPositions, MacAreeHotspots);
				}
				return CurrentMacAreeGreater;
			}
			public static List<int> Minor = new List<int>() { Cengar.Portal, Naigtal.Portal, Val.Portal, Sangua.Portal, Bonich.Portal, Aurinor.Portal, };
			public static List<int> Greater = new List<int>() { MistressAlluradel.Portal, MatronFolnuna.Portal, PitLordVilemus.Portal, InquisitorMeto.Portal, Occularus.Portal, Sotanathor.Portal };
			public static List<int> Out = new List<int>()
			{
				124884, //Cengar, Aurinor
			};
			public static List<Vector3> KrokuunHotspots = new List<Vector3>()
			{
				new Vector3(628.2518, 1222.403, 441.9168), //70,81
				new Vector3(1815.953, 1045.214, 501.4757), //75,34 // good
				new Vector3(2016.802, 1520.774, 408.6734), //62,25 // greater
				new Vector3(736.7952, 2027.215, 390.4212), //48,77
			};
			public static List<Vector3> KrokuunMapCoords = new List<Vector3>()
			{
				new Vector3(0.7001, 0.8170, 0), //70,81
				new Vector3(0.9299, 0.6068, 0), //75,34 // good
				new Vector3(0.8597, 0.5569, 0), //62,25 // greater
				new Vector3(0.7848, 0.8743, 0), //48,77
			};
			public static List<Vector3> AntoranWastesHotspots = new List<Vector3>()
			{
				new Vector3(-3357.363, 9082.359, -168.0572), //66, 70 //good
				new Vector3(-2692.599, 8985.086, -137.701), //greater
				new Vector3(-2544.95, 8999.505, -138.3018), //69, 33 //good
				new Vector3(2458.948, 9397.223, -128.9223), //57, 30 //good
				new Vector3(-2763.939, 9230.536, -169.6993), //60, 43 //good
				new Vector3(-3220.726, 9106.102, -164.4698), //66, 64 //greater //good
				new Vector3(-2226.712, 9383.924, -59.89701), //58, 20 //good
				new Vector3(-2473.896, 9172.278, -158.6594), //64, 31 //greater //good
				new Vector3(-2915.547, 9377.62, -161.2649), //58, 50 //greater //good
			};
			public static List<Vector3> AntoranWastesMapCoords = new List<Vector3>()
			{
				new Vector3(0.6608, 0.6916, 0), //66, 70 ///good
				new Vector3(0.6894, 0.3983, 0), //bad?
				new Vector3(0.6851, 0.3332, 0), //69, 33 //good
				new Vector3(0.5682, 0.2953, 0), //57, 30 //good
				new Vector3(0.6025, 0.4298, 0), //60, 43 //good
				new Vector3(0.6538, 0.6313, 0), //66, 64 //greater// good
				new Vector3(0.5721, 0.1928, 0), //58, 20 //good
				new Vector3(0.6344, 0.3019, 0), //64, 31 //greater //good
				new Vector3(0.5739, 0.4968, 0), //58, 50 //greater //good
			};
			public static List<Vector3> MacAreeHotspots = new List<Vector3>()
			{
				new Vector3(5786.783, 9248.674, -28.09992), //72,39
				new Vector3(6352.261, 10281.95, 38.57191), //40,13 // good
				new Vector3(6215.544, 9557.909, -79.70746), //63,19
				new Vector3(5519.492, 10072.35, -85.66666), //47,51 //greater //good
			};
			public static List<Vector3> MacAreeMapCoords = new List<Vector3>()
			{
				new Vector3(0.7203, 0.3848, 0), //72,39 //good
				new Vector3(0.4032, 0.1246, 0), //40,13 //good
				new Vector3(0.6252, 0.1877, 0), //63,19 //good
				new Vector3(0.4680, 0.5078, 0), //47,51 //greater //good
			};

		}
	}
	public static void ResetSettings()
	{
		Log("reset settings");
	}
	public static void FixNavigation()
	{
		//FixNavigationKrokuun();
		FixNavigationAntoranwastes();
		//FixNavigationMacAree();

		Var.SetVar("Cameleto10PathTester_Destination", Teleporter.HopesLanding.position);
		Log("PATH TRACED TO hopes landing");
	}
	public static void FixNavigationKrokuun()
	{
		var badSpots = new List<Vector3>()
		{
			//robot
			new Vector3(966.6677, 1512.044, 550.2554, "None"),
			new Vector3(995.1309, 1514.021, 544.1593, "None"),
		};
		foreach (var p in badSpots)
		{
			wManager.wManagerSetting.AddBlackListZone(p, 15, false);
		}
		var krokuunHovelCave = new List<Vector3>()
		{
			new Vector3(968.098, 1709.575, 525.0884, "None"),
			new Vector3(963.9632, 1711.419, 526.7278, "None"),
			new Vector3(953.6521, 1718.045, 529.0692, "None"),
		};
		Questing.OffmeshHelper.CreateChain((int)ContinentId.Argus_1, "Krokuun Hovel Cave", krokuunHovelCave);
		var shatteredFieldsStairs = new List<Vector3>()
		{
			new Vector3(937.5199, 1935.406, 469.9249, "None"),
			new Vector3(942.1003, 1937.891, 467.6447, "None"),
		};
		Questing.OffmeshHelper.CreateChain((int)ContinentId.Argus_1, "Shattered Fields Stairs", shatteredFieldsStairs);
		var krokuunHovelStairs = new List<Vector3>()
		{
			new Vector3(961.3267, 1576.04, 546.9158, "None"),
			new Vector3(960.496, 1573.092, 547.2367, "None"),
			new Vector3(968.1282, 1562.134, 546.7881, "None"),
			new Vector3(979.3612, 1552.716, 546.1615, "None"),
			new Vector3(986.7557, 1548.146, 545.7874, "None"),
			new Vector3(1021.544, 1551.457, 543.1418, "None"),
			new Vector3(1047.324, 1558.337, 536.4019, "None"),
		};
		Questing.OffmeshHelper.CreateChain((int)ContinentId.Argus_1, "Krokuun Hovel Stairs", krokuunHovelStairs);

		var krokuunHovelStairs2 = new List<Vector3>()
		{
			new Vector3(961.3267, 1576.04, 546.9158, "None"),
			new Vector3(944.2624, 1563.501, 550.4792, "None"),
			new Vector3(940.2314, 1558.615, 550.7825, "None"),
		};
		Questing.OffmeshHelper.CreateChain((int)ContinentId.Argus_1, "Krokuun Hovel Stairs2", krokuunHovelStairs2);

		var nathraxasHold = new List<Vector3>()
		{
			new Vector3(1802.381, 1813.673, 398.3794, "None"),
			new Vector3(1804.342, 1818.87, 398.4007, "None"),
			new Vector3(1806.283, 1838.13, 400, "None"),
			new Vector3(1809.058, 1845.248, 400.973, "None"),
		};
		Questing.OffmeshHelper.CreateChain((int)ContinentId.Argus_1, "Nath'raxas Hold", nathraxasHold);

		
		Log("navigation fixed in Krokuun");
	}
	public static void FixNavigationAntoranwastes()
	{
		var badSpots = new List<Vector3>()
		{
			//hopes landing
			new Vector3(-2823.989, 8838.809, -209.0935),
			new Vector3(-2787.728, 8844.316, -209.1537),
			new Vector3(-2765.498, 8844.922, -208.7058),
			new Vector3(-2735.734, 8779.047, -202.695),
			new Vector3(-2762.992, 8765.779, -197.8463),
			new Vector3(-2793.739, 8752.172, -198.6862),
			new Vector3(-2831.412, 8779.538, -223.36),
			new Vector3(-2841.402, 8743.868, -215.8409),
			new Vector3(-2865.348, 8722.61, -224.6263),
			new Vector3(-2876.899, 8745.164, -230.0804),
			new Vector3(-2861.619, 8778.229, -229.205),
		};
		var radius = 25f;
		foreach (var p in badSpots)
		{
			wManager.wManagerSetting.AddBlackListZone(p, radius, false);
			//PathFinder.ReportBigDangerArea(p, radius);
			PathFinder.Pather.ReportArea(p, radius, RDManaged.RD.PolyArea.POLYAREA_BIGDANGER);
		}
		//PathFinder.Pather.
		//PathFinder.Pather.ReportArea(vector3, AvoidItSettings.CurrentSetting.SpotRadius, RD.PolyArea.POLYAREA_BIGDANGER);
		var hopesLanding = new List<Vector3>()
		{
			new Vector3(-2897.719, 8799.521, -228.5435, "None"),
			new Vector3(-2895.331, 8802.357, -227.7084, "None"),
			new Vector3(-2859.957, 8835.806, -214.8958, "None"),
			new Vector3(-2853.968, 8867.577, -211.8608, "None"),
		};
		Questing.OffmeshHelper.CreateChain((int)ContinentId.Argus_1, "Hope's Landing Entrance", hopesLanding);
		Log("navigation fixed in Antoran wastes");
	}
	public static void FixNavigationMacAree()
	{
		Log("navigation fixed in MacAree");
	}
}