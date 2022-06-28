using System;
using System.Text;
using System.Threading.Tasks;

public class Tools
{
	static Tools()
	{
		
	}
	
	public static bool DisableHardestQuests = true;
	
	public static Dictionary<int, Vector3> InnkeeperPosById = new Dictionary<int, Vector3>()
	{
		{1464, new Vector3(-3827.93, -831.901, 10.0906)},{295, new Vector3(-9462.66, 16.1915, 56.96339)},
		{6737, new Vector3(6406.51, 515.367, 8.642637)},{6736,new Vector3(9802.21, 982.608, 1313.897)}
	};
	
	public static Dictionary<string, int> FlyMasterKalimdorByTaxiButton = new Dictionary<string, int>()
	{
		{ "Auberdine", 3841},{ "Rut'theran Village", 3838},{ "Astranaar", 4267},{ "Thalanaar", 4319},{ "Theramore", 4321},{ "Stonetalon Peak", 4407},{ "Nijel's Point", 6706},{ "Gadgetzan", 7823},{ "Feathermoon", 8019},
		{ "Nighthaven", 10897},{ "Everlook", 11138},{ "Talrendis Point", 12577},{ "Talonbranch Glade", 12578},{ "Cenarion Hold", 15177},{ "Marshal's Refuge", 10583},{ "Ratchet", 16227}
    };
	
	public static Dictionary<string, int> FlyMasterAzerothByTaxiButton = new Dictionary<string, int>()
	{
		{ "Stormwind", 352},{ "Sentinel Hill", 523},{ "Ironforge", 1573},{ "Menethil Harbor", 1571},{ "Thelsamar", 1572},{ "Lakeshire", 931},{ "Darkshire", 2409},{ "Southshore", 2432},{ "Refuge Pointe", 2835},
		{ "Booty Bay", 2859},{ "Aerie Peak", 8018},{ "Nethergarde Keep", 8609},{ "Chillwind Camp", 12596},{ "Light's Hope Chapel", 12617},{ "Morgan's Vigil", 2299},{ "Thorium Point", 2941}
    };
	
	public static Dictionary<int, Vector3> FlyMasterVectorById = new Dictionary<int, Vector3>()
	{
		{ 1571, new Vector3(-3793.2, -782.052, 9.014864)},{ 2835, new Vector3(-1240.03, -2513.96, 21.92968)},{ 2432, new Vector3(-715.146, -512.134, 26.54459)},
		{ 12596, new Vector3(928.273, -1429.08, 64.75089)},{ 12617, new Vector3(2269.85, -5345.39, 86.94098)},{ 8018, new Vector3(282.096, -2001.28, 194.127)},
		{ 1573, new Vector3(-4821.13, -1152.4, 502.2116)},{ 1572, new Vector3(-5424.85, -2929.87, 347.5623)},{ 2941,  new Vector3(-6559.06, -1169.38, 309.7965)},
		{ 2299, new Vector3(-8365.08, -2736.93, 185.6077)},{ 931, new Vector3(-9435.21, -2234.88, 69.10919)},{ 352, new Vector3(-8835.76, 490.084, 109.6157)},
		{ 2409, new Vector3(-10513.8, -1258.79, 41.43174)},{ 523, new Vector3(-10628.3, 1037.27, 34.10983)},{ 8609, new Vector3(-11110.2, -3437.1, 79.19871)},
		{ 2859, new Vector3(-14477.9, 464.101, 36.38121)},{ 16227, new Vector3(-898.246, -3769.65, 11.71021)},{ 4321, new Vector3(-3828.88, -4517.51, 10.66054)},
		{ 7823, new Vector3(-7224.87, -3738.21, 8.401266)},{ 10583, new Vector3(-6110.54, -1140.35, -186.9486)},{ 15177, new Vector3(-6758.55, 775.594, 89.02187)},
		{ 8019, new Vector3(-4370.54, 3340.11, 12.26888)},{ 4319, new Vector3(-4491.12, -778.347, -40.20255)},{ 6706, new Vector3(133.8058, 1326.801, 193.4992)},
		{ 4407, new Vector3(2683.58, 1461.902, 233.3266)},{ 4267, new Vector3(2827.088, -288.6166, 107.1412)},{ 11138, new Vector3(6800.54, -4742.35, 701.4984)},
		{ 12577, new Vector3(2721.612, -3880.077, 100.9026)},{ 12578, new Vector3(6206.385, -1948.432, 571.3064)},{ 10897, new Vector3(7454.85, -2491.61, 462.6164)},
		{ 3838, new Vector3(8640.58,841.118,23.26397)},{ 3841, new Vector3(6343.2f,561.651f,15.79909f)}
    };
	
	public static void GoToBottomTeldrassil()
	{
		if(ObjectManager.Me.Position.Z > 300)
			Tools.MovetoPositionAndForward(new Vector3(9945.692, 2614.073, 1316.31),1.502247f,3000);
	}
	
	public static void GoToTopTeldrassil()
	{
		if(ObjectManager.Me.Position.Z < 300)
			Tools.MovetoPositionAndForward(new Vector3(8785.482, 966.6973, 30.20336), 0.2326342f,3000);
	}
	
	public static void MovetoPositionAndForward(Vector3 position,float rotation,int moveForwardValue)
	{
		GoToTask.ToPosition(position);
		Thread.Sleep(1000);
		ObjectManager.Me.Rotation = rotation;
		Thread.Sleep(1000);
		wManager.Wow.Helpers.Move.Forward(Move.MoveAction.PressKey, moveForwardValue);
	}
	
	public static void TakeShipMenethilToAuberdine()
	{
		Tools.TakeShip(176310,(int)ContinentId.Azeroth,"Wetlands",(int)ContinentId.Kalimdor,"Darkshore",
			new Vector3(-3709.475, -575.0988, 0), new Vector3(-3723.09, -582.3294, 6.241098),
			new Vector3(6406.216, 823.0809, 0),
			0.8793408f,3000,1.109283f,3000);
	}
	
	public static void TakeShipAuberdineToMenethil()
	{
		Tools.TakeShip(176310,(int)ContinentId.Kalimdor,"Darkshore",(int)ContinentId.Azeroth,"Wetlands",
			new Vector3(6406.216, 823.0809, 0),new Vector3(6420.464, 817.8544, 5.731891),
			new Vector3(-3709.475, -575.0988, 0),
			 2.371758f,3000,5.253525f,3000);
	}
	
	public static void TakeShipAuberdineToDarnassus()
	{
		Tools.TakeShip(176244,(int)ContinentId.Kalimdor,"Darkshore",(int)ContinentId.Kalimdor,"Teldrassil",
			new Vector3(6594.373, 759.8262, 0),new Vector3(6578.085, 767.1378, 5.715079),
			new Vector3(8533.65, 1025.055, 0),
			6.023891f,1500,4.648809f,1500);
	}
	
	public static void TakeShipDarnassusToAuberdine()
	{
		Tools.TakeShip(176244,(int)ContinentId.Kalimdor,"Teldrassil",(int)ContinentId.Kalimdor,"Darkshore",
			new Vector3(8533.65, 1025.055, 0),new Vector3(8548.565, 1023.551, 5.811839),
			new Vector3(6594.373, 759.8262, 0),
			5.959039f,1500,4.64649f,1500);
	}
	
	public static void TakeShipMenethilToTheramore()
	{
		Tools.TakeShip(176231,(int)ContinentId.Azeroth,"Wetlands",(int)ContinentId.Kalimdor,"Dustwallow Marsh",
			new Vector3(-3905.225, -585.8085, 0), new Vector3(-3898.902, -601.1245, 5.367361),
			new Vector3(-4016.39, -4740.588, 0),
			1.527071f,3000,5.176325f,3000);
	}
	
	public static void TakeShipTheramoreToMenethil()
	{
		Tools.TakeShip(176231,(int)ContinentId.Kalimdor,"Dustwallow Marsh",(int)ContinentId.Azeroth,"Wetlands",
			new Vector3(-4016.39, -4740.588, 0), new Vector3(-4007.245, -4728.329, 5.248651),
			new Vector3(-3905.225, -585.8085, 0),
			4.551151f,3000,1.108747f,3000);
	}
	
	public static void TakeShip(int transportEntryId,int continentFrom,string zoneFrom,int continentTo,string zoneTo,Vector3 fromTransportWaitPosition,Vector3 fromPlayerWaitPosition,Vector3 toTransportWaitPosition,float rotationFrom,int moveValueFrom,float rotationTo,int moveValueTo)
	{
		// Change WRobot settings:
		wManager.wManagerSetting.CurrentSetting.CloseIfPlayerTeleported = false;
		wManager.Plugin.PluginsManager.DisposeAllPlugins();﻿
		//wManager.Wow.Helpers.Conditions.ForceIgnoreIsAttacked = true;

		// Code:
		//if (!Conditions.InGameAndConnectedAndProductStartedNotInPause)
			//return true;
		
		if (ObjectManager.Me.IsMounted)
			MountTask.DismountMount()﻿;
		
		Logging.Write("[Micam's Tools] Ship from " + zoneFrom + " to " + zoneTo + " : Started");
		while(Usefuls.ContinentId != continentTo || Usefuls.MapZoneName != zoneTo || ObjectManager.Me.InTransport ||
		ObjectManager.GetWoWGameObjectByEntry(transportEntryId).OrderBy(o => o.GetDistance).FirstOrDefault() == null ||
		ObjectManager.GetWoWGameObjectByEntry(transportEntryId).OrderBy(o => o.GetDistance).First().Position.DistanceTo(fromTransportWaitPosition) >= 10)
		{
			if (Usefuls.ContinentId == continentFrom && Usefuls.MapZoneName != zoneTo) // && Usefuls.MapZoneName == zoneFrom
			{
				if (!ObjectManager.Me.InTransport)
				{
					if (GoToTask.ToPosition(fromPlayerWaitPosition))
					{
						var transport = ObjectManager.GetWoWGameObjectByEntry(transportEntryId).OrderBy(o => o.GetDistance).FirstOrDefault();
						if (transport != null && transport.Position.DistanceTo(fromTransportWaitPosition) < 10)
						{
							Logging.Write("[Micam's Tools] Ship from " + zoneFrom + " to " + zoneTo + " : Get On Ship");
							Thread.Sleep(3000);
							ObjectManager.Me.Rotation = rotationFrom;
							Thread.Sleep(500);
							wManager.Wow.Helpers.Move.Forward(Move.MoveAction.PressKey, moveValueFrom);
						}
					}
				}
			}
			else if ((Usefuls.ContinentId == continentTo && Usefuls.MapZoneName == zoneTo) || (continentFrom != continentTo && Usefuls.ContinentId == continentTo))
			{
				if (ObjectManager.Me.InTransport)
				{
					var transport = ObjectManager.GetWoWGameObjectByEntry(transportEntryId).OrderBy(o => o.GetDistance).FirstOrDefault();
					if (transport != null && transport.Position.DistanceTo(toTransportWaitPosition) < 10)
					{
						Logging.Write("[Micam's Tools] Ship from " + zoneFrom + " to " + zoneTo + " : Get Out Ship");
						Thread.Sleep(3000);
						ObjectManager.Me.Rotation = rotationTo;
						Thread.Sleep(500);
						wManager.Wow.Helpers.Move.Forward(Move.MoveAction.PressKey, moveValueTo);
						Logging.Write("[Micam's Tools] Ship from " + zoneFrom + " to " + zoneTo + " : Finished");
						wManager.wManagerSetting.CurrentSetting.CloseIfPlayerTeleported = true;
						wManager.Plugin.PluginsManager.LoadAllPlugins();
						return;
					}
				}
			}
			Thread.Sleep(50);
		}
		
		//wManager.Wow.Helpers.Conditions.ForceIgnoreIsAttacked = false;
	}
	
    public static void TakeTaxi(string fromTown, string toTown, int continent)
    {
		var flyMasterFrom = continent == 1 ? FlyMasterKalimdorByTaxiButton[fromTown] : FlyMasterAzerothByTaxiButton[fromTown];
		var flyMasterTo = continent == 1 ? FlyMasterKalimdorByTaxiButton[toTown] : FlyMasterAzerothByTaxiButton[toTown];		
			
		var flyMasterPosFrom = FlyMasterVectorById[flyMasterFrom];
		var flyMasterPosTo = FlyMasterVectorById[flyMasterTo];
		
		wManager.Wow.Helpers.Conditions.ForceIgnoreIsAttacked = true;
		
		Logging.Write("Taking taxi from [" + fromTown + " (npc id = " + flyMasterFrom + ")] to [" + toTown + " (npc id = " + flyMasterTo + ")]");
		
		while(ObjectManager.Me.Position.DistanceTo2D(flyMasterPosFrom) > 100 )
		{
			GoToTask.ToPosition(flyMasterPosFrom);
		}
		
		wManager.Wow.Helpers.Conditions.ForceIgnoreIsAttacked = false;
		
		while(ObjectManager.Me.Position.DistanceTo2D(flyMasterPosFrom) < 300)
		{
			if (!ObjectManager.Me.IsOnTaxi) 
			{
				if (wManager.Wow.Bot.Tasks.GoToTask.ToPositionAndIntecractWithNpc(flyMasterPosFrom, flyMasterFrom))
				{
					Usefuls.SelectGossipOption(GossipOptionsType.taxi);
					var taxiButtonId = GetTaxiIdFromTown(toTown);
					//Logging.Write("Taking button id = " + taxiButtonId);
					Lua.LuaDoString("TaxiButton"+taxiButtonId+":Click()",false);
				}
			}
		}
		
		while(ObjectManager.Me.IsOnTaxi)
		{
			Thread.Sleep(1000);
		}
	
		Logging.Write("Sleep Start");
		Thread.Sleep(5000);
		Logging.Write("Sleep Stop");

    }
	
	public static int GetTaxiIdFromTown(string town)
	{
		string id = "";
		int i = 1;
		while(id != "INVALID")
        {
			id = Lua.LuaDoString<string>("id = TaxiNodeName("+ i +"); return id; ");
			if (id.Split(',')[0] == town) return i;
			i++;
		}
		
		return 0;
	}
	
	public static void LearnTaxi(string town,int continent)
    {
        var flyMaster = continent == 1 ? FlyMasterKalimdorByTaxiButton[town] : FlyMasterAzerothByTaxiButton[town];
        var flyMasterPos = FlyMasterVectorById[flyMaster];

        bool learned = false;
        while(!learned)
        {
            if (wManager.Wow.Bot.Tasks.GoToTask.ToPositionAndIntecractWithNpc(flyMasterPos, flyMaster))
            {
                Usefuls.SelectGossipOption(GossipOptionsType.taxi);
                learned = true;
            }
        }
    }
	
	public static void SelectZoneHearthstone(int idNpc)
	{
		var npcPos = InnkeeperPosById[idNpc];
		bool selected = false;
		while(!selected)
        {
            if (wManager.Wow.Bot.Tasks.GoToTask.ToPositionAndIntecractWithNpc(npcPos, idNpc))
            {
				//Thread.Sleep(2000);
                Usefuls.SelectGossipOption(GossipOptionsType.binder);
				Thread.Sleep(2000);
				Lua.LuaDoString("StaticPopup1Button1:Click()",false);
                selected = true;
            }
        }
		
	}
	
	public static void UseHearthstone()
	{
		wManager.wManagerSetting.CurrentSetting.CloseIfPlayerTeleported = false;
		wManager.Wow.Helpers.ItemsManager.UseItem(6948); // http://www.wowhead.com/item=6948/hearthstone
		System.Threading.Thread.Sleep(1000 * 20); // 20 secondes
		wManager.wManagerSetting.CurrentSetting.CloseIfPlayerTeleported = true;
	}
	
	public static void LearnSpellsFromNpc(int idNpc,Vector3 npcPos)
	{
		Logging.Write("[LearnSpellsFromNpc] Starting.");
		bool learned = false;
		while(!learned)
        {
            if (wManager.Wow.Bot.Tasks.GoToTask.ToPositionAndIntecractWithNpc(npcPos, idNpc))
            {
                wManager.Wow.Helpers.Usefuls.SelectGossipOption(wManager.Wow.Enums.GossipOptionsType.trainer);
				Thread.Sleep(1000);
				wManager.Wow.Helpers.Trainer.TrainingSpell();
				Thread.Sleep(1000);
                learned = true;
            }
        }
		Logging.Write("[LearnSpellsFromNpc] Ending.");
	}
	
	public static bool CheckIfLearned(int Profession,string spellName)
	{
		if ((int)wManager.Wow.Enums.SkillLine.FirstAid == Profession)
		{
			return Lua.LuaDoString<bool>("local learned = false CastSpellByName(\"First Aid\") for i=1,GetNumTradeSkills()do local na,_,n=GetTradeSkillInfo(i)if na==\""+spellName+"\" then learned = true end end CastSpellByName(\"First Aid\") return learned");
		}
		
		return false;
	}
	
	public static bool Craft(int Profession,string spellName)
	{
		if ((int)wManager.Wow.Enums.SkillLine.FirstAid == Profession)
		{
			Logging.Write("[First Aid] Crafting " + spellName);
			Lua.LuaDoString("CastSpellByName(\"First Aid\") for i=1,GetNumTradeSkills()do local na,_,n=GetTradeSkillInfo(i)if na==\""+spellName+"\" then DoTradeSkill(i,n)end end CastSpellByName(\"First Aid\")",false);
			Thread.Sleep(1000);
			return false;
		}
		
		return false;
	}
	
	/* exemple on .xml file :
	<QuestsSorted Action="RunCode" NameClass="Tools.BuyObjectFromNpc(1464, new Vector3(-3827.93, -831.901, 10.0906),2,&quot;Tough Jerky&quot;)" />
	this will buy 2 Tough Jerky (1 tough jerky equals 5) from inkeeper in wetlands
	*/
	public static void BuyObjectFromNpc(int idNpc,Vector3 npcPos,int nb,string itemName)
	{
		bool selected = false;
		while(!selected)
        {
            if (wManager.Wow.Bot.Tasks.GoToTask.ToPositionAndIntecractWithNpc(npcPos, idNpc))
            {
                Usefuls.SelectGossipOption(GossipOptionsType.vendor);
				Thread.Sleep(1000);
				
				
				for(int nbi = 0; nbi < nb ; nbi++)
				{
					Lua.LuaDoString(@"local nameObject = '"+itemName+@"';
						for i = 1, GetMerchantNumItems() do
							local name, _, _, _, numAvailable = GetMerchantItemInfo(i);
							if name and (numAvailable == -1 or numAvailable > 0) and string.sub(name, 1, string.len(nameObject)) == nameObject then
								BuyMerchantItem(i);
								break;
							end
						end CloseMerchant()",false);
					
					
					Thread.Sleep(500);
				}
				
				
                selected = true;
            }
        }
		//Logging.Write("=================== OK ==============");
	}
	
	public class SimpleQuestReward
	{
		public int num;
		public string sType;
		public string sSubType;
		
		public SimpleQuestReward(int p_num, string p_sType, string p_sSubType)
		{
			this.num = p_num;
			this.sType = p_sType;
			this.sSubType = p_sSubType;
		}
	}
	
	public static bool TurnInCustom(QuestClass currentQuestClass)
	{
		Logging.Write("[TurnInCustom] Name = " + currentQuestClass.Name + " | QuestId = " + currentQuestClass.QuestId.First());
		if (!currentQuestClass.HasQuest() || !currentQuestClass.IsComplete())
			return true;
		
		//int logId = 2; //Burning Crusade TODO find equivalent
		int logId = Quest.GetLogIdByQuestId(currentQuestClass.QuestId.First()); //Vanilla
		Lua.LuaDoString("SelectQuestLogEntry(" + logId + ");");
				
		int numQuestLogChoices = Lua.LuaDoString<int>("numQuestLogChoices = GetNumQuestLogChoices(); return numQuestLogChoices; ");
		Logging.Write("[TurnInCustom] logId = " + logId + " | numQuestLogChoices = " + numQuestLogChoices);
		List<SimpleQuestReward> rewards = new List<SimpleQuestReward>();
		
		for (int i = 1; i <= numQuestLogChoices; i++)
        {
            string link = Lua.LuaDoString<string>("link = GetQuestLogItemLink('choice', "+ i + "); return link; ");
			string[] links = link.Split(':');
			string sType = Lua.LuaDoString<string>("local sName, sLink, iRarity, iLevel, sType, sSubType, iStackCount = GetItemInfo(" + links[1] + "); return sType; ");
			string sSubType = Lua.LuaDoString<string>("local sName, sLink, iRarity, iLevel, sType, sSubType, iStackCount = GetItemInfo(" + links[1] + "); return sSubType; ");		
			Logging.Write("[TurnInCustom] item " + i + " = " + link + " | sType = " + sType + " | sSubType = " + sSubType);
			rewards.Add(new SimpleQuestReward(i, sType, sSubType));
        }
		
		if (rewards.Count > 0)
		{
			currentQuestClass.GossipOptionItem = PickBestItem(rewards);			
		}
		
		Logging.Write("[TurnInCustom] item selected = " + currentQuestClass.GossipOptionItem);		
		bool returnValue = TurnInCopy(currentQuestClass);
		return returnValue;
	}
	
	public static int PickBestItem(List<SimpleQuestReward> items)
	{
		SimpleQuestReward itemSelected = null;
		List<string> weapons = new List<string>();
		List<string> armors = new List<string>();
		
		// we prepare the list of weapons and armors that we can equip
		switch (ObjectManager.Me.WowClass)
		{
		case WoWClass.Druid:
			weapons = new List<string> {"One-Handed Maces","Two-Handed Maces", "Polearms", "Staves", "Daggers", "Fist Weapons"};
			armors = new List<string> {"Cloth", "Leather"};
			break;
		case WoWClass.Hunter:
			weapons = new List<string> {"One-Handed Axes", "Two-Handed Axes", "One-Handed Swords", "Two-Handed Swords", "Polearms", "Staves", "Daggers", "Fist Weapons", "Bows", "Crossbows", "Guns"};
			armors = new List<string> {"Cloth", "Leather"};
			if (ObjectManager.Me.Level > 39) armors.Add("Mail");
			break;
		case WoWClass.Mage:
			weapons = new List<string> {"One-Handed Swords", "Staves", "Daggers"};
			armors = new List<string> {"Cloth"};
			break;
		case WoWClass.Paladin:
			weapons = new List<string> {"Two-Handed Axes", "Two-Handed Swords", "Two-Handed Maces", "One-Handed Axes", "One-Handed Swords", "One-Handed Maces", "Polearms"};
			armors = new List<string> {"Cloth", "Leather", "Mail", "Shields"};
			if (ObjectManager.Me.Level > 39) armors.Add("Plate");
			break;
		case WoWClass.Priest:
			weapons = new List<string> {"One-Handed Maces", "Staves", "Daggers", "Wands"};
			armors = new List<string> {"Cloth"};
			break;
		case WoWClass.Rogue:
			weapons = new List<string> {"One-Handed Swords", "One-Handed Maces", "Daggers", "Fist Weapons", "Bows", "Crossbows", "Guns"};
			armors = new List<string> {"Cloth", "Leather"};
			break;
		case WoWClass.Shaman:
			weapons = new List<string> {"One-Handed Axes", "Two-Handed Axes", "One-Handed Maces", "Two-Handed Maces", "Staves", "Daggers", "Fist Weapons"};
			armors = new List<string> {"Cloth", "Leather", "Shields"};
			if (ObjectManager.Me.Level > 39) armors.Add("Mail");
			break;
		case WoWClass.Warlock:
			weapons = new List<string> {"One-Handed Swords", "Staves", "Daggers"};
			armors = new List<string> {"Cloth"};
			break;
		case WoWClass.Warrior:
			weapons = new List<string> {"One-Handed Axes", "Two-Handed Axes", "One-Handed Swords", "Two-Handed Swords", "One-Handed Maces", "Two-Handed Maces", "Polearms", "Staves", "Daggers", "Fist Weapons", "Bows", "Crossbows", "Guns"};
			armors = new List<string> {"Cloth", "Leather", "Mail", "Shields"};
			if (ObjectManager.Me.Level > 39) armors.Add("Plate");
			break;
		default:
			break;
		}		
		
		foreach(var item in items)
		{
			// if it's the first, we take it
			if (itemSelected == null && armors.Contains(item.sSubType))
			{
				itemSelected = item;
			}
			else
			{
				// if it's a weapon, we check than we can use it
				if (item.sType == "Weapon" && weapons.Contains(item.sSubType))
				{
					// we take this one, we can exit.
					return item.num;
				}
				// if it's an armor, we check than we can use it
				else if (item.sType == "Armor" && armors.Contains(item.sSubType))
				{
					// we pick the best armor (if it's same sub type, we keep the selected)
					itemSelected = PickBestArmor(itemSelected, item);
				}
			}
		}
		
		return itemSelected == null ? 1 : itemSelected.num;
	}
	
	public static SimpleQuestReward PickBestArmor(SimpleQuestReward itemSelected, SimpleQuestReward item)
	{
		// if we havent an armor yet, we change for the new item if its an armor
		if (itemSelected.sType != "Armor" && item.sType == "Armor")
			return item;
		// else we take shield > Plate > Mail > Leather > Cloth
		switch (itemSelected.sSubType)
		{
		case "Shields":
			return itemSelected;
		case "Plate":
			if (item.sSubType == "Shields")
				return item;
			else
				return itemSelected;
		case "Mail":
			if (item.sSubType == "Shields" || item.sSubType == "Plate")
				return item;
			else
				return itemSelected;
		case "Leather":
			if (item.sSubType == "Shields" || item.sSubType == "Plate" || item.sSubType == "Mail")
				return item;
			else
				return itemSelected;
		case "Cloth":
			if (item.sSubType == "Shields" || item.sSubType == "Plate" || item.sSubType == "Mail" || item.sSubType == "Leather")
				return item;
			else
				return itemSelected;
		default:
			return itemSelected;
		}
	}
	
	public static bool TurnInCopy(QuestClass currentQuestClass)
    {
      if (currentQuestClass.NpcTurnIn == null || currentQuestClass.NpcTurnIn.Id <= 0)
      {
        return false;
      }
	  
      bool flag = currentQuestClass.NpcTurnIn.GameObject ? GoToTask.ToPositionAndIntecractWithGameObject(currentQuestClass.NpcTurnIn.Position, currentQuestClass.NpcTurnIn.Id, -1, false, (BooleanDelegate) null) : GoToTask.ToPositionAndIntecractWithNpc(currentQuestClass.NpcTurnIn.Position, currentQuestClass.NpcTurnIn.Id, -1, false, (BooleanDelegate) null, true);
	  if (!flag)
	  {
		return false;
	  }
	  
      Quest.CompleteQuest(currentQuestClass.GossipOptionItem);
      for (int gossipOption = Quest.GetNumGossipActiveQuests(); ((gossipOption < 1 ? 0 : (currentQuestClass.HasQuest() ? 1 : 0)) & (flag ? 1 : 0)) != 0; --gossipOption)
      {
        if (gossipOption <= 0)
          gossipOption = 1;
        if (Conditions.IsAttackedAndCannotIgnore || wManager.Wow.ObjectManager.ObjectManager.Me.IsDead)
          return false;
        flag = currentQuestClass.NpcTurnIn.GameObject ? GoToTask.ToPositionAndIntecractWithGameObject(currentQuestClass.NpcTurnIn.Position, currentQuestClass.NpcTurnIn.Id, -1, false, (BooleanDelegate) null) : GoToTask.ToPositionAndIntecractWithNpc(currentQuestClass.NpcTurnIn.Position, currentQuestClass.NpcTurnIn.Id, -1, false, (BooleanDelegate) null, true);
        Thread.Sleep(Usefuls.Latency + 1000);
        Quest.SelectGossipActiveQuest(gossipOption);
        Thread.Sleep(Usefuls.Latency + 1000);
        Quest.CompleteQuest(currentQuestClass.GossipOptionItem);
        Thread.Sleep(Usefuls.Latency + 1000);
        Quest.CloseQuestWindow();
        Thread.Sleep(Usefuls.Latency + 1000);
      }
      if (currentQuestClass.HasQuest())
      {
        for (int index = 1; ((index > 15 ? 0 : (currentQuestClass.HasQuest() ? 1 : 0)) & (flag ? 1 : 0)) != 0; ++index)
        {
			
          if (Conditions.IsAttackedAndCannotIgnore || wManager.Wow.ObjectManager.ObjectManager.Me.IsDead)
            return false;
          flag = currentQuestClass.NpcTurnIn.GameObject ? GoToTask.ToPositionAndIntecractWithGameObject(currentQuestClass.NpcTurnIn.Position, currentQuestClass.NpcTurnIn.Id, -1, false, (BooleanDelegate) null) : GoToTask.ToPositionAndIntecractWithNpc(currentQuestClass.NpcTurnIn.Position, currentQuestClass.NpcTurnIn.Id, -1, false, (BooleanDelegate) null, true);
          Thread.Sleep(Usefuls.Latency + 1000);
		  Lua.LuaDoString("QuestTitleButton" + index + ":Click();");
          Thread.Sleep(Usefuls.Latency + 1000);
          Quest.CompleteQuest(currentQuestClass.GossipOptionItem);
          Thread.Sleep(Usefuls.Latency + 1000);
          Quest.CloseQuestWindow();
          Thread.Sleep(Usefuls.Latency + 1000);
        }
      }
      return !currentQuestClass.HasQuest();
    }
	
	public static void AddNotToSell(string objectName)
	{
		if (!wManager.wManagerSetting.CurrentSetting.DoNotSellList.Contains(objectName))
			wManager.wManagerSetting.CurrentSetting.DoNotSellList.Add(objectName);		
	}
	
	public static void RemoveNotToSell(string objectName)
	{
		if (wManager.wManagerSetting.CurrentSetting.DoNotSellList.Contains(objectName))
			wManager.wManagerSetting.CurrentSetting.DoNotSellList.Remove(objectName);
	}
	
	public static void UseItemById(string name, bool lootItem = false)
	{
		if (lootItem)
		{
			Task.Run(async delegate
			{
				robotManager.Helpful.Keyboard.DownKey(wManager.Wow.Memory.WowMemory.Memory.WindowHandle,
					System.Windows.Forms.Keys.Shift);
				await Task.Delay(1000);
				robotManager.Helpful.Keyboard.UpKey(wManager.Wow.Memory.WowMemory.Memory.WindowHandle,
					System.Windows.Forms.Keys.Shift);
			});
		}
		
		wManager.Wow.Helpers.ItemsManager.UseItem(name);
	}
	
	
	public static void UseItemQuestTeldrassil() {
		Thread t = new Thread(() =>
		{
			uint itemId = 8149;
			int questId = 2561;
			while (robotManager.Products.Products.IsStarted)
			{
				if (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause)
				{
					if (!Quest.HasQuest(questId))
						break;
					if (ObjectManager.Target.IsValid && ObjectManager.Target.IsDead)
					{
						ItemsManager.UseItem(itemId);
					}
				}
				Thread.Sleep(500);
			}
		});
		t.Start();
	}
}

//


/* 
/script ChatFrame1:AddMessage("frame name: " .. GetMouseFocus():GetName())

Kalimdor fly button continent = 1

TaxiButton1 Darkshore = Auberdine
TaxiButton2 Teldrassil = Rut'theran Village
TaxiButton3 Ashenvale = Astranaar
TaxiButton4 Feralas (Thalanaar) = Thalanaar
TaxiButton5 Dustwallow Marsh = Theramore
TaxiButton6 Stonetalon mountains = 
TaxiButton7 Desolace = Nijel's Point
TaxiButton8 Tanaris = Gadgetzan
TaxiButton9 Feralas (Feathermoon) = Feathermoon
TaxiButton10 Moonglade = Nighthaven ??
TaxiButton11 Winterspring = Everlook
TaxiButton12 Azshara = Talrendis Point
TaxiButton13 Felwood = Talonbranch Glade
TaxiButton14 Silithus = Cenarion Hold
TaxiButton15 UnGoro = Marshal's Refuge
TaxiButton16 The Barrens = Ratchet

VANILLA

Azeroth fly button continent = 0

TaxiButton1 Stormwind = Stormwind
TaxiButton2 Westfall = Sentinel Hill
TaxiButton3 Ironforge = Ironforge
TaxiButton4 Wetlands = Menethil Harbor
TaxiButton5 Loch Modan = Thelsamar
TaxiButton6 Redridge = Lakeshire
TaxiButton7 Duskwood = Darkshire
TaxiButton8 Hillsbrad = Southshore
TaxiButton9 Arathi = Refuge Pointe
TaxiButton10 STV = Booty Bay
TaxiButton11 Hinterlands = Aerie Peak
TaxiButton12 Blasted Lands = Nethergarde Keep
TaxiButton13 WPL = Chillwind Camp
TaxiButton14 EPL = Light's Hope Chapel
TaxiButton15 Burning Steppes = Morgan's Vigil
TaxiButton16 Searing Gorge = Thorium Point

*/
