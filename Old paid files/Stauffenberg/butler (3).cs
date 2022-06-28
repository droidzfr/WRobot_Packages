//	--------------------------------------------------------------------------------------------------
//		Butler - equip better items, food and drink
//	--------------------------------------------------------------------------------------------------
//
//	If you level a new character there are many problems - bots are not changing their
//	equipment and sometimes they run out of manadrinks or food.
//	
//	Our upcoming heroes are in need of a butler to assist!
//	
//
//	- Butler takes care if there is better equipment in inventory than worn and equips the better one
//	- Also (if activated) butler switches to the food and drink which is carried in inventory most
//	- Last but not least butler is able to destroy uncommon (gray) items if you set him to do so
//	
//	For the different classes you may tweak the multipliers for item scoring within the settings,
//	so Butler is able to decide which gear is best for your juniors
//	
//	Maybe with Butler i have created a tool helping your second growth to survive leveling
//	
//	
//	(c) 2017 by Stauffenberg
//	
//	--------------------------------------------------------------------------------------------------
//	stauffenberg.eu@gmail.com
//	--------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading;
using System.ComponentModel;
using System.Configuration;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using wManager;
using wManager.Plugin;
using wManager.Wow.Class;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using robotManager.Helpful;
using robotManager.Products;


public class Main : IPlugin {
	private string butlerVersion="1.3";
	private string ButlerPrefix="[Butler] ";
	private bool ButlerLaunched=false;
	private int shortDelay=100;
	private int longDelay=500;
	private bool foundUnknownItems=false;
	private string[] itemStatConstants=null;
	private string DrinkAndFood=null;
	private WoWItem EquippedItem1=null;
	private WoWItem EquippedItem2=null;
	private List<WoWItem> EquippedItems=null;
	private Dictionary<string, float> itemValues=new Dictionary<string, float>();
	private	List<WoWItem> bagItems;
	private List<uint> knownFoodArray=new List<uint>();
	private List<uint> knownDrinkArray=new List<uint>();
	private List<string> blackFoodDrink=new List<string>();
	private Dictionary<int, int> itemValueOverrides=new Dictionary<int, int>();
	

	public void Initialize() {
		ButlerLaunched = true;
        ButlerSettings.Load();
        ButlerGlobalSettings.Load();
        ButlerBlackListSettings.Load();
		Logging.Write(ButlerPrefix+"Butler version "+butlerVersion+" is loaded and ready");
		itemStatConstants=getItemStatConstants();
		DrinkAndFood=getItemSubtypeDrinkAndFood();
		getFoodAndDrinkArray();
		getitemValueOverrides();
		if (ButlerGlobalSettings.CurrentSetting.pulseDelay<1000 || ButlerGlobalSettings.CurrentSetting.pulseDelay>60000) {ButlerGlobalSettings.CurrentSetting.pulseDelay=3000;}
		while (ButlerLaunched && Products.IsStarted) {
			try {
				if (wManager.Wow.Helpers.Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause) {
					if (!ObjectManager.Me.InCombat && !ObjectManager.Me.IsDead) {
						PulseLevelUp();
						getBagItems();
						PulseResting();
						getEquippedItems();
						PulseEquip();
						getBagItems();
						PulseDestroy();
					}
				}
			}
			catch (Exception e) {Logging.WriteError(ButlerPrefix+"there was an error: " + e);}
			Thread.Sleep(ButlerGlobalSettings.CurrentSetting.pulseDelay);
		}
	}

	private void PulseLevelUp() {
		if (ButlerBlackListSettings.CurrentSetting.playerLevel!=wManager.Wow.ObjectManager.ObjectManager.Me.Level) {
			ButlerBlackListSettings.CurrentSetting.playerLevel=wManager.Wow.ObjectManager.ObjectManager.Me.Level;
			ButlerBlackListSettings.CurrentSetting.blackItems=new List<string>();
			Logging.Write(ButlerPrefix+"blacklist cleared");
			ButlerBlackListSettings.CurrentSetting.Save();
		}
	}
	
	private void PulseResting() {
		int newitemCount=0;
		uint newItemID=0;
		string newDrinkName="";
		string newFoodName="";
		int newDrinkMax=0;
		int newFoodMax=0;
		foreach (WoWItem item in bagItems) {
			if (item.GetItemInfo.ItemSubType==DrinkAndFood) {
				if (item.GetItemInfo.ItemMinLevel<=wManager.Wow.ObjectManager.ObjectManager.Me.Level) {
					newitemCount=wManager.Wow.Helpers.ItemsManager.GetItemCountByNameLUA(item.GetItemInfo.ItemName);
					newItemID=wManager.Wow.Helpers.ItemsManager.GetIdByName(item.GetItemInfo.ItemName);
					if (knownDrinkArray.Contains(newItemID)) {
						if (newitemCount>newDrinkMax) {
							newDrinkName=item.GetItemInfo.ItemName;
							newDrinkMax=newitemCount;
						}
					}
					else
					if (knownFoodArray.Contains(newItemID)) {
						if (newitemCount>newFoodMax) {
							newFoodName=item.GetItemInfo.ItemName;
							newFoodMax=newitemCount;
						}
					}
					else {
						if (!blackFoodDrink.Contains(item.GetItemInfo.ItemLink)) {
							if (foundUnknownItems==false) {
								Logging.Write(ButlerPrefix+"not all items in inventory are known to be drink or food");
								foundUnknownItems=true;
							}
							Logging.WriteDebug(ButlerPrefix+"\""+item.GetItemInfo.ItemName+"\" ID "+newItemID+" is unknown, you may add this ID to knownAsDrink or knownAsFood");
							blackFoodDrink.Add(item.GetItemInfo.ItemLink);
						}
					}
				}
				Thread.Sleep(shortDelay);
			}
		}
		if (ButlerSettings.CurrentSetting.replaceDrink && newDrinkMax>0) {
			if (wManager.wManagerSetting.CurrentSetting.DrinkName != newDrinkName) {
				Logging.Write(ButlerPrefix+"changing Manadrink from \""+wManager.wManagerSetting.CurrentSetting.DrinkName+"\" to \""+newDrinkName+"\"");
				wManager.wManagerSetting.CurrentSetting.DrinkName = newDrinkName;
			}
		}
		if (ButlerSettings.CurrentSetting.replaceFood && newFoodMax>0) {
			if (wManager.wManagerSetting.CurrentSetting.FoodName != newFoodName) {
				Logging.Write(ButlerPrefix+"changing Food from \""+wManager.wManagerSetting.CurrentSetting.FoodName+"\" to \""+newFoodName+"\"");
				wManager.wManagerSetting.CurrentSetting.FoodName = newFoodName;
			}
		}
	}

	private void getFoodAndDrinkArray() {
		uint itemID=0;
		string[] FoodArray=ButlerGlobalSettings.CurrentSetting.knownAsFood.Split(',');
		foreach (string Food in FoodArray) {
			if (uint.TryParse(Food, out itemID)) { knownFoodArray.Add(itemID); }
		}
		string[] DrinkArray=ButlerGlobalSettings.CurrentSetting.knownAsDrink.Split(',');
		foreach (string Drink in DrinkArray) {
			if (uint.TryParse(Drink, out itemID)) { knownDrinkArray.Add(itemID); }
		}
	}

	private void PulseDestroy() {
		if (ButlerSettings.CurrentSetting.DestroyGray) {
			foreach (WoWItem item in bagItems) {
				if (item.GetItemInfo.ItemRarity==0) {
					while (ObjectManager.Me.InCombat || ObjectManager.Me.IsDead) {Thread.Sleep(shortDelay);}
					List<int> BagAndSlot=Bag.GetItemContainerBagIdAndSlot(item.Entry);
					Logging.Write(ButlerPrefix+"destroying \""+item.GetItemInfo.ItemName+"\"");
					Lua.LuaDoString(string.Format("PickupContainerItem({0}, {1}); DeleteCursorItem()",(object) BagAndSlot[0],(object) BagAndSlot[1]),false);
					Thread.Sleep(shortDelay);
				}
			}
		}
	}
	
	private void PulseEquip() {
		foreach (WoWItem item in bagItems) {
			if (item.IsEquippableItem) {
				if ((item.GetItemInfo.ItemRarity==0 && ButlerSettings.CurrentSetting.EquipGray)
					|| (item.GetItemInfo.ItemRarity==1 && ButlerSettings.CurrentSetting.EquipWhite)
					|| (item.GetItemInfo.ItemRarity==2 && ButlerSettings.CurrentSetting.EquipGreen)
					|| (item.GetItemInfo.ItemRarity==3 && ButlerSettings.CurrentSetting.EquipBlue)
					|| (item.GetItemInfo.ItemRarity==4 && ButlerSettings.CurrentSetting.EquipEpic)
					|| (item.GetItemInfo.ItemRarity==5 && ButlerSettings.CurrentSetting.EquipLegendary)) {
					checkThisItem(item);
					Thread.Sleep(shortDelay);
				}
			}
		}
	}
	
	private void checkThisItem(WoWItem item) {
		if  (item.GetItemInfo.ItemMinLevel<=wManager.Wow.ObjectManager.ObjectManager.Me.Level) {
			float itemValue=getItemValue(item);
			if (itemValue!=0) {
				if (!ButlerBlackListSettings.CurrentSetting.blackItems.Contains(item.GetItemInfo.ItemLink)) {
					string equipLoc=translateLoc(item.GetItemInfo.ItemEquipLoc);
					loadWeareable(equipLoc);
					bool equipThisItem=false;
					float wert1=0; float wert2=0;
					if (EquippedItem1!=null) {wert1=getItemValue(EquippedItem1);} else {wert1=-1;}
					if (EquippedItem2!=null) {wert2=getItemValue(EquippedItem2);} else {wert2=-1;}
					if (wert2>wert1) {
						wert1=wert2;
						EquippedItem1=EquippedItem2;
					}
					if (itemValue > wert1) {equipThisItem=true;}
					if (equipThisItem) {
						string debugMessage="equipping \""+item.GetItemInfo.ItemName+"\" id "+item.Entry+" value "+itemValue;
						if (EquippedItem1!=null) {
							debugMessage=debugMessage+" replacing \""+EquippedItem1.GetItemInfo.ItemName+"\" id "+EquippedItem1.Entry+" value "+getItemValue(EquippedItem1);
						}
						while (ObjectManager.Me.InCombat || ObjectManager.Me.IsDead) {Thread.Sleep(shortDelay);}
						wManager.Wow.Helpers.ItemsManager.EquipItemByName(item.GetItemInfo.ItemName);
						Thread.Sleep(shortDelay);
						Lua.LuaDoString<string>("EquipPendingItem(0);");
						Thread.Sleep(longDelay);
						getEquippedItems();
						if (!itemIsEquipped(item)) {
							ButlerBlackListSettings.CurrentSetting.blackItems.Add(item.GetItemInfo.ItemLink);
							ButlerBlackListSettings.CurrentSetting.Save();
							Logging.Write(ButlerPrefix+"item \""+item.GetItemInfo.ItemName+"\" id "+item.Entry+" blacklisted");
						}
						else Logging.Write(ButlerPrefix+debugMessage);
					}
				}
			}
			else {
				if (!ButlerBlackListSettings.CurrentSetting.blackItems.Contains(item.GetItemInfo.ItemLink)) {
					ButlerBlackListSettings.CurrentSetting.blackItems.Add(item.GetItemInfo.ItemLink);
					ButlerBlackListSettings.CurrentSetting.Save();
					Logging.WriteDebug(ButlerPrefix+"item \""+item.GetItemInfo.ItemName+"\" id "+item.Entry+" reports no stats - blacklisted");
				}
			}
		}
	}

	private float getItemValue(WoWItem item) {
		float itemValue=0;
		if (item!=null) {
			itemValue=getItemOverride(item);
			if (itemValue==-1701) {
				itemValue=0;
				string itemLink = item.GetItemInfo.ItemLink;
				if (!itemValues.TryGetValue(itemLink, out itemValue)) {
					string itemStats=getItemStats(item);
					itemValue=getItemStatValue(itemStats);
					itemValues.Add(itemLink,itemValue);
					Logging.WriteDebug(ButlerPrefix+"acknowledging item \""+item.GetItemInfo.ItemName+"\" id "+item.Entry+" with a value of "+itemValue);
				}
			}
		}
		return itemValue;
	}

	private int getItemOverride(WoWItem item) {
	int itemValue=-1701;
		if (item!=null) {
			int itemEntry = item.Entry;
			if (!itemValueOverrides.TryGetValue(itemEntry, out itemValue)) {
				itemValue=-1701;
			} else {
//				Logging.WriteDebug(ButlerPrefix+"overrides item \""+item.GetItemInfo.ItemName+"\" id "+item.Entry+" with a value of "+itemValue);
			}
		}
		return itemValue;
	}

	private void getitemValueOverrides() {
		int itemEntry=0; int itemValue=0; int converted=0;
		string[] KeyPair=ButlerGlobalSettings.CurrentSetting.ItemValueOverrides.Split(',');
		foreach (string Key in KeyPair) {
			itemEntry=0; itemValue=0; converted=0;
			string[] KeyValue=Key.Split(':');
			if (int.TryParse(KeyValue[0], out itemEntry)) {converted=converted+1;};
			if (int.TryParse(KeyValue[1], out itemValue)) {converted=converted+1;};
			if (converted==2) { itemValueOverrides.Add(itemEntry,itemValue); }
		}
	}

	private void getBagItems() {
		bagItems = wManager.Wow.Helpers.Bag.GetBagItem();
	}

	private void getEquippedItems() {
		EquippedItems=wManager.Wow.Helpers.EquippedItems.GetEquippedItems();
	}
	
	private string translateLoc(string equipLoc) {
		string adjustedLoc="";
		if (equipLoc=="INVTYPE_AMMO")			{adjustedLoc="0";}
		if (equipLoc=="INVTYPE_HEAD")			{adjustedLoc="1";}
		if (equipLoc=="INVTYPE_NECK")			{adjustedLoc="2";}
		if (equipLoc=="INVTYPE_SHOULDER")		{adjustedLoc="3";}
		if (equipLoc=="INVTYPE_BODY")			{adjustedLoc="4";}
		if (equipLoc=="INVTYPE_CHEST")			{adjustedLoc="5";}
		if (equipLoc=="INVTYPE_ROBE")			{adjustedLoc="5";}
		if (equipLoc=="INVTYPE_WAIST")			{adjustedLoc="6";}
		if (equipLoc=="INVTYPE_LEGS")			{adjustedLoc="7";}
		if (equipLoc=="INVTYPE_FEET")			{adjustedLoc="8";}
		if (equipLoc=="INVTYPE_WRIST")			{adjustedLoc="9";}
		if (equipLoc=="INVTYPE_HAND")			{adjustedLoc="10";}
		if (equipLoc=="INVTYPE_FINGER")			{adjustedLoc="11";}
		if (equipLoc=="INVTYPE_TRINKET")		{adjustedLoc="13";}
		if (equipLoc=="INVTYPE_CLOAK")			{adjustedLoc="15";}
		if (equipLoc=="INVTYPE_WEAPON")			{adjustedLoc="16";}
		if (equipLoc=="INVTYPE_SHIELD")			{adjustedLoc="17";}
		if (equipLoc=="INVTYPE_2HWEAPON")		{adjustedLoc="16";}
		if (equipLoc=="INVTYPE_WEAPONMAINHAND")	{adjustedLoc="16";}
		if (equipLoc=="INVTYPE_WEAPONOFFHAND")	{adjustedLoc="17";}
		if (equipLoc=="INVTYPE_HOLDABLE")		{adjustedLoc="17";}
		if (equipLoc=="INVTYPE_RANGED")			{adjustedLoc="18";}
		if (equipLoc=="INVTYPE_THROWN")			{adjustedLoc="18";}
		if (equipLoc=="INVTYPE_RANGEDRIGHT")	{adjustedLoc="18";}
		if (equipLoc=="INVTYPE_RELIC")			{adjustedLoc="18";}
		if (equipLoc=="INVTYPE_TABARD")			{adjustedLoc="19";}
		if (equipLoc=="INVTYPE_BAG")			{adjustedLoc="20";}
		if (equipLoc=="INVTYPE_QUIVER")			{adjustedLoc="20";}
		return adjustedLoc;
	}
	
	private void loadWeareable(string equipLoc) {
		bool loadFirst=true;
		EquippedItem1=null;
		EquippedItem2=null;
		foreach (WoWItem item in EquippedItems) {
			if (equipLoc==translateLoc(item.GetItemInfo.ItemEquipLoc)) {
				if (loadFirst) {
					EquippedItem1=item;
					loadFirst=false;
				} else {EquippedItem2=item;}
			}
		}
	}
	
	private bool itemIsEquipped(WoWItem newItem) {
		bool isEquipped=false;
		foreach (WoWItem item in EquippedItems) {if (newItem.GetItemInfo.ItemLink==item.GetItemInfo.ItemLink) {isEquipped=true;}}
		return isEquipped;
	}

	private float getItemStatValue(string itemStats) {
		float itemWert=0;
		if (itemStats.Length>1) {
			string[] statsArray=itemStats.Split(';');
			foreach (string itemStat in statsArray) {
				string[] statArray=itemStat.Split(':');
				string statKey=statArray[0];
				string statValue=statArray[1];
				float statValueFloat=0;
				float multiplier=1;
				statValueFloat=Convert.ToSingle(statValue, CultureInfo.InvariantCulture);
				int index=-1;
				for (var i=0; i<itemStatConstants.Length; i++) {if (itemStatConstants[i]==statKey) {index=i;};}
				if (index==0)  {multiplier=ButlerSettings.CurrentSetting.multiAGILITY;};
				if (index==1)  {multiplier=ButlerSettings.CurrentSetting.multiARMOR_PENETRATION_RATING;};
				if (index==2)  {multiplier=ButlerSettings.CurrentSetting.multiATTACK_POWER;};
				if (index==3)  {multiplier=ButlerSettings.CurrentSetting.multiBLOCK_RATING;};
				if (index==4)  {multiplier=ButlerSettings.CurrentSetting.multiBLOCK_VALUE;};
				if (index==5)  {multiplier=ButlerSettings.CurrentSetting.multiCRIT_MELEE_RATING;};
				if (index==6)  {multiplier=ButlerSettings.CurrentSetting.multiCRIT_RANGED_RATING;};
				if (index==7)  {multiplier=ButlerSettings.CurrentSetting.multiCRIT_RATING;};
				if (index==8)  {multiplier=ButlerSettings.CurrentSetting.multiCRIT_SPELL_RATING;};
				if (index==9)  {multiplier=ButlerSettings.CurrentSetting.multiDAMAGE_PER_SECOND;};
				if (index==10) {multiplier=ButlerSettings.CurrentSetting.multiDEFENSE_SKILL_RATING;};
				if (index==11) {multiplier=ButlerSettings.CurrentSetting.multiDODGE_RATING;};
				if (index==12) {multiplier=ButlerSettings.CurrentSetting.multiEXPERTISE_RATING;};
				if (index==13) {multiplier=ButlerSettings.CurrentSetting.multiFERAL_ATTACK_POWER;};
				if (index==14) {multiplier=ButlerSettings.CurrentSetting.multiHASTE_MELEE_RATING;};
				if (index==15) {multiplier=ButlerSettings.CurrentSetting.multiHASTE_RANGED_RATING;};
				if (index==16) {multiplier=ButlerSettings.CurrentSetting.multiHASTE_RATING;};
				if (index==17) {multiplier=ButlerSettings.CurrentSetting.multiHASTE_SPELL_RATING;};
				if (index==18) {multiplier=ButlerSettings.CurrentSetting.multiHEALTH;};
				if (index==19) {multiplier=ButlerSettings.CurrentSetting.multiHEALTH_REGENERATION;};
				if (index==20) {multiplier=ButlerSettings.CurrentSetting.multiHIT_MELEE_RATING;};
				if (index==21) {multiplier=ButlerSettings.CurrentSetting.multiHIT_RANGED_RATING;};
				if (index==22) {multiplier=ButlerSettings.CurrentSetting.multiHIT_RATING;};
				if (index==23) {multiplier=ButlerSettings.CurrentSetting.multiHIT_SPELL_RATING;};
				if (index==24) {multiplier=ButlerSettings.CurrentSetting.multiHIT_TAKEN_RATING;};
				if (index==25) {multiplier=ButlerSettings.CurrentSetting.multiHIT_TAKEN_SPELL_RATING;};
				if (index==26) {multiplier=ButlerSettings.CurrentSetting.multiHIT_TAKEN_MELEE_RATING;};
				if (index==27) {multiplier=ButlerSettings.CurrentSetting.multiHIT_TAKEN_RANGED_RATING;};
				if (index==28) {multiplier=ButlerSettings.CurrentSetting.multiINTELLECT;};
				if (index==29) {multiplier=ButlerSettings.CurrentSetting.multiMANA;};
				if (index==30) {multiplier=ButlerSettings.CurrentSetting.multiMANA_REGENERATION;};
				if (index==31) {multiplier=ButlerSettings.CurrentSetting.multiMELEE_ATTACK_POWER;};
				if (index==32) {multiplier=ButlerSettings.CurrentSetting.multiPARRY_RATING;};
				if (index==33) {multiplier=ButlerSettings.CurrentSetting.multiRANGED_ATTACK_POWER;};
				if (index==34) {multiplier=ButlerSettings.CurrentSetting.multiRESILIENCE_RATING;};
				if (index==35) {multiplier=ButlerSettings.CurrentSetting.multiSPELL_DAMAGE_DONE;};
				if (index==36) {multiplier=ButlerSettings.CurrentSetting.multiSPELL_HEALING_DONE;};
				if (index==37) {multiplier=ButlerSettings.CurrentSetting.multiSPELL_POWER;};
				if (index==38) {multiplier=ButlerSettings.CurrentSetting.multiSPELL_PENETRATION;};
				if (index==39) {multiplier=ButlerSettings.CurrentSetting.multiSPIRIT;};
				if (index==40) {multiplier=ButlerSettings.CurrentSetting.multiSTAMINA;};
				if (index==41) {multiplier=ButlerSettings.CurrentSetting.multiSTRENGTH;};
				itemWert = itemWert + statValueFloat * multiplier;
			}
		}
		return itemWert;
	}
	
	private string getItemStats(WoWItem item) {
		string itemStats=Lua.LuaDoString("istats=GetItemStats(\""+item.GetItemInfo.ItemLink+"\") stats4butler=\"\" for stat, value in pairs(istats) do stats4butler=stats4butler ..  _G[stat] .. \":\" .. value .. \";\" end","stats4butler");
		if (itemStats.Length>1) {itemStats=itemStats.Substring(0,itemStats.Length-1);};
		return itemStats;
	}

	private string getItemSubtypeDrinkAndFood() {
		string subClasses=Lua.LuaDoString("stats4butler = GetAuctionItemSubClasses(4)","stats4butler");
		return subClasses;
	}
			
	private string[] getItemStatConstants() {
		string luacommand = "stats4butler=ITEM_MOD_AGILITY_SHORT .. \";\".. "+
								"ITEM_MOD_ARMOR_PENETRATION_RATING_SHORT .. \";\".. "+
								"ITEM_MOD_ATTACK_POWER_SHORT .. \";\".. "+
								"ITEM_MOD_BLOCK_RATING_SHORT .. \";\".. "+
								"ITEM_MOD_BLOCK_VALUE_SHORT .. \";\".. "+
								"ITEM_MOD_CRIT_MELEE_RATING_SHORT .. \";\".. "+
								"ITEM_MOD_CRIT_RANGED_RATING_SHORT .. \";\".. "+
								"ITEM_MOD_CRIT_RATING_SHORT .. \";\".. "+
								"ITEM_MOD_CRIT_SPELL_RATING_SHORT .. \";\".. "+
								"ITEM_MOD_DAMAGE_PER_SECOND_SHORT .. \";\".. "+
								"ITEM_MOD_DEFENSE_SKILL_RATING_SHORT .. \";\".. "+
								"ITEM_MOD_DODGE_RATING_SHORT .. \";\".. "+
								"ITEM_MOD_EXPERTISE_RATING_SHORT .. \";\".. "+
								"ITEM_MOD_FERAL_ATTACK_POWER_SHORT .. \";\".. "+
								"ITEM_MOD_HASTE_MELEE_RATING_SHORT .. \";\".. "+
								"ITEM_MOD_HASTE_RANGED_RATING_SHORT .. \";\".. "+
								"ITEM_MOD_HASTE_RATING_SHORT .. \";\".. "+
								"ITEM_MOD_HASTE_SPELL_RATING_SHORT .. \";\".. "+
								"ITEM_MOD_HEALTH_SHORT .. \";\".. "+
								"ITEM_MOD_HEALTH_REGENERATION_SHORT .. \";\".. "+
								"ITEM_MOD_HIT_MELEE_RATING_SHORT .. \";\".. "+
								"ITEM_MOD_HIT_RANGED_RATING_SHORT .. \";\".. "+
								"ITEM_MOD_HIT_RATING_SHORT .. \";\".. "+
								"ITEM_MOD_HIT_SPELL_RATING_SHORT .. \";\".. "+
								"ITEM_MOD_HIT_TAKEN_RATING_SHORT .. \";\".. "+
								"ITEM_MOD_HIT_TAKEN_SPELL_RATING_SHORT .. \";\".. "+
								"ITEM_MOD_HIT_TAKEN_MELEE_RATING_SHORT .. \";\".. "+
								"ITEM_MOD_HIT_TAKEN_RANGED_RATING_SHORT .. \";\".. "+
								"ITEM_MOD_INTELLECT_SHORT .. \";\".. "+
								"ITEM_MOD_MANA_SHORT .. \";\".. "+
								"ITEM_MOD_MANA_REGENERATION_SHORT .. \";\".. "+
								"ITEM_MOD_MELEE_ATTACK_POWER_SHORT .. \";\".. "+
								"ITEM_MOD_PARRY_RATING_SHORT .. \";\".. "+
								"ITEM_MOD_RANGED_ATTACK_POWER_SHORT .. \";\".. "+
								"ITEM_MOD_RESILIENCE_RATING_SHORT .. \";\".. "+
								"ITEM_MOD_SPELL_DAMAGE_DONE_SHORT .. \";\".. "+
								"ITEM_MOD_SPELL_HEALING_DONE_SHORT .. \";\".. "+
								"ITEM_MOD_SPELL_POWER_SHORT .. \";\".. "+
								"ITEM_MOD_SPELL_PENETRATION_SHORT .. \";\".. "+
								"ITEM_MOD_SPIRIT_SHORT .. \";\".. "+
								"ITEM_MOD_STAMINA_SHORT .. \";\".. "+
								"ITEM_MOD_STRENGTH_SHORT";
		string statConstants=Lua.LuaDoString(luacommand,"stats4butler");
		string[] statConstantsArray=statConstants.Split(';');
		return statConstantsArray;
	}

	public void Dispose() 
	{
		ButlerLaunched = false;
	}

	public void Settings() {
		ButlerSettings.Load();
		ButlerSettings.CurrentSetting.ToForm();
		ButlerSettings.CurrentSetting.Save();
		ButlerGlobalSettings.Load();
		ButlerGlobalSettings.CurrentSetting.ToForm();
		ButlerGlobalSettings.CurrentSetting.Save();
		ButlerBlackListSettings.Load();
		ButlerBlackListSettings.CurrentSetting.Save();
	}
}


public class ButlerBlackListSettings : Settings {
	public ButlerBlackListSettings() {
		blackItems=new List<string>();
		playerLevel=0;
	}

    public static ButlerBlackListSettings CurrentSetting { get; set; }

    public bool Save() {
		try {return Save(AdviserFilePathAndName("Butler",  ObjectManager.Me.Name + ".Blacklist."+ wManager.Wow.Helpers.Usefuls.RealmName));}
		catch (Exception e) {Logging.WriteDebug("Butler failed to save blacklist settings because of: " + e); return false;}
	}

	public static bool Load() {
		try {
			if (File.Exists(AdviserFilePathAndName("Butler", ObjectManager.Me.Name + ".Blacklist."+ wManager.Wow.Helpers.Usefuls.RealmName))) {
				CurrentSetting = Load<ButlerBlackListSettings>(AdviserFilePathAndName("Butler", ObjectManager.Me.Name + ".Blacklist."+ wManager.Wow.Helpers.Usefuls.RealmName));
				return true; 
			}
			CurrentSetting = new ButlerBlackListSettings();
		}
		catch (Exception e) {
			Logging.WriteDebug("Butler failed to load blacklist settings because of: " + e);
		}
		return false;
	}

	[Setting]
	[Category("Blacklist Settings")]
	[DisplayName("Blacklisted Items")]
	[Description("Comma-separated ItemsIDs that missed to be equipped")]
	public List<string> blackItems { get; set; }

	[Setting]
	[Category("Blacklist Settings")]
	[DisplayName("Player level")]
	[Description("Player level")]
	public uint playerLevel { get; set; }
}


public class ButlerGlobalSettings : Settings {
	public ButlerGlobalSettings() {
		knownAsDrink = "159,1179,1205,1645,1708,3772,5350,8766,22018,27860,28399,33444,33445,34062,35954,43513";
		knownAsFood = "117,414,422,787,1707,2070,2287,3770,3771,3927,4536,4537,4538,4539,4540,4541,4542,4544,4592,4593,4594,4599,4601,4602,4656,8932,8950,8952,8953,8957,21552,27854,27855,27856,29363,29451,29450,33443,33449,33454,35948,35949,35950,35952,35953";
		ItemValueOverrides = "2586:0,11508:0,12064:0";
		pulseDelay = 3000;
	}

    public static ButlerGlobalSettings CurrentSetting { get; set; }

    public bool Save() {
		try {return Save(AdviserFilePathAndName("Butler", "Globals"));}
		catch (Exception e) {Logging.WriteDebug("Butler failed to save global settings because of: " + e); return false;}
	}

	public static bool Load() {
		try {
			if (File.Exists(AdviserFilePathAndName("Butler", "Globals"))) {
				CurrentSetting = Load<ButlerGlobalSettings>(AdviserFilePathAndName("Butler", "Globals"));
				return true; 
			}
			CurrentSetting = new ButlerGlobalSettings();
		}
		catch (Exception e) {
			Logging.WriteDebug("Butler failed to load global settings because of: " + e);
		}
		return false;
	}

	[Setting]
	[Category("Resting Settings")]
	[DisplayName("Known as Manadrink")]
	[Description("Comma-separated ItemsIDs of known Manadrinks to use")]
	public string knownAsDrink { get; set; }

	[Setting]
	[Category("Resting Settings")]
	[DisplayName("Known as Food")]
	[Description("Comma-separated ItemIDs of known food to use")]
	public string knownAsFood { get; set; }

	[Setting]
	[Category("Global Settings")]
	[DisplayName("Item value overrides")]
	[Description("Comma-separated ItemID:value pairs to fix the item value calculation - 2586:0 would never, 2586:99999999 would everytime equip my gamemaster robe")]
	public string ItemValueOverrides { get; set; }

	[Setting]
	[Category("Global Settings")]
	[DisplayName("Pulse delay")]
	[Description("Time in milliseconds between butler pulses (3000 by default)")]
	public int pulseDelay { get; set; }
}


public class ButlerSettings : Settings {
	public ButlerSettings() {
		DestroyGray = false;
		EquipGray = true;
		EquipWhite = true;
		EquipGreen = true;
		EquipBlue = true;
		EquipEpic = false;
		EquipLegendary = false;
		replaceFood = true;
		replaceDrink = true;
		multiAGILITY = 100;
		multiARMOR_PENETRATION_RATING = 100;
		multiATTACK_POWER = 100;
		multiBLOCK_RATING = 100;
		multiBLOCK_VALUE = 100;
		multiCRIT_MELEE_RATING = 100;
		multiCRIT_RANGED_RATING = 100;
		multiCRIT_RATING = 100;
		multiCRIT_SPELL_RATING = 100;
		multiDAMAGE_PER_SECOND = 5;
		multiDEFENSE_SKILL_RATING = 100;
		multiDODGE_RATING = 100;
		multiEXPERTISE_RATING = 100;
		multiFERAL_ATTACK_POWER = 100;
		multiHASTE_MELEE_RATING = 100;
		multiHASTE_RANGED_RATING = 100;
		multiHASTE_RATING = 100;
		multiHASTE_SPELL_RATING = 100;
		multiHEALTH = 100;
		multiHEALTH_REGENERATION = 100;
		multiHIT_MELEE_RATING = 100;
		multiHIT_RANGED_RATING = 100;
		multiHIT_RATING = 100;
		multiHIT_SPELL_RATING = 100;
		multiHIT_TAKEN_RATING = 100;
		multiHIT_TAKEN_SPELL_RATING = 100;
		multiHIT_TAKEN_MELEE_RATING = 100;
		multiHIT_TAKEN_RANGED_RATING = 100;
		multiINTELLECT = 100;
		multiMANA = 100;
		multiMANA_REGENERATION = 100;
		multiMELEE_ATTACK_POWER = 100;
		multiPARRY_RATING = 100;
		multiRANGED_ATTACK_POWER = 100;
		multiRESILIENCE_RATING = 100;
		multiSPELL_DAMAGE_DONE = 100;
		multiSPELL_HEALING_DONE = 100;
		multiSPELL_POWER = 100;
		multiSPELL_PENETRATION = 100;
		multiSPIRIT = 100;
		multiSTAMINA = 100;
		multiSTRENGTH = 100;
	}

    public static ButlerSettings CurrentSetting { get; set; }

    public bool Save() {
		try {return Save(AdviserFilePathAndName("Butler", ObjectManager.Me.Name + "." + wManager.Wow.Helpers.Usefuls.RealmName));}
		catch (Exception e) {Logging.WriteDebug("Butler failed to save settings because of: " + e); return false;}
	}

	public static bool Load() {
		try {
			if (File.Exists(AdviserFilePathAndName("Butler", ObjectManager.Me.Name + "." + wManager.Wow.Helpers.Usefuls.RealmName))) {
				CurrentSetting = Load<ButlerSettings>(AdviserFilePathAndName("Butler",ObjectManager.Me.Name + "." + wManager.Wow.Helpers.Usefuls.RealmName));
				return true; 
			}
			CurrentSetting = new ButlerSettings();
		}
		catch (Exception e) {
			Logging.WriteDebug("Butler failed to load settings because of: " + e);
		}
		return false;
	}

	[Setting]
	[Category("Common Settings")]
	[DisplayName("Destroy poor")]
	[Description("Destroy poor (gray) items in inventory")]
	public bool DestroyGray { get; set; }

	[Setting]
	[Category("Common Settings")]
	[DisplayName("Equip poor")]
	[Description("Equip poor (gray) items on pickup")]
	public bool EquipGray { get; set; }

	[Setting]
	[Category("Common Settings")]
	[DisplayName("Equip common")]
	[Description("Equip common (white) items on pickup")]
	public bool EquipWhite { get; set; }

	[Setting]
	[Category("Common Settings")]
	[DisplayName("Equip uncommon")]
	[Description("Equip uncommon (green) items on pickup")]
	public bool EquipGreen { get; set; }

	[Setting]
	[Category("Common Settings")]
	[DisplayName("Equip rare")]
	[Description("Equip rare (blue) items on pickup")]
	public bool EquipBlue { get; set; }

	[Setting]
	[Category("Common Settings")]
	[DisplayName("Equip epic")]
	[Description("Equip epic (purple) items on pickup")]
	public bool EquipEpic { get; set; }

	[Setting]
	[Category("Common Settings")]
	[DisplayName("Equip legendary")]
	[Description("Equip legendary items on pickup")]
	public bool EquipLegendary { get; set; }

	[Setting]
	[Category("Item stats multiplier BASIC")]
	[DisplayName("AGILITY")]
	[Description("Multiplier for Agility")]
	public float multiAGILITY { get; set; }

	[Setting]
	[Category("Item stats multiplier SECONDARY")]
	[DisplayName("ARMOR_PENETRATION_RATING")]
	[Description("Multiplier for Armor Penetration")]
	public float multiARMOR_PENETRATION_RATING { get; set; }

	[Setting]
	[Category("Item stats multiplier SECONDARY")]
	[DisplayName("ATTACK_POWER")]
	[Description("Multiplier for Attack Power")]
	public float multiATTACK_POWER { get; set; }

	[Setting]
	[Category("Item stats multiplier SECONDARY")]
	[DisplayName("BLOCK_RATING")]
	[Description("Multiplier for Block rating")]
	public float multiBLOCK_RATING { get; set; }

	[Setting]
	[Category("Item stats multiplier SECONDARY")]
	[DisplayName("BLOCK_VALUE")]
	[Description("Multiplier for Block value")]
	public float multiBLOCK_VALUE { get; set; }

	[Setting]
	[Category("Item stats multiplier SECONDARY")]
	[DisplayName("CRIT_MELEE_RATING")]
	[Description("Multiplier for Crit (melee)")]
	public float multiCRIT_MELEE_RATING { get; set; }

	[Setting]
	[Category("Item stats multiplier SECONDARY")]
	[DisplayName("CRIT_RANGED_RATING")]
	[Description("Multiplier for Crit (ranged)")]
	public float multiCRIT_RANGED_RATING { get; set; }

	[Setting]
	[Category("Item stats multiplier SECONDARY")]
	[DisplayName("CRIT_RATING")]
	[Description("Multiplier for Crit")]
	public float multiCRIT_RATING { get; set; }

	[Setting]
	[Category("Item stats multiplier SECONDARY")]
	[DisplayName("CRIT_SPELL_RATING")]
	[Description("Multiplier for Crit (spell)")]
	public float multiCRIT_SPELL_RATING { get; set; }

	[Setting]
	[Category("Item stats multiplier BASIC")]
	[DisplayName("DAMAGE_PER_SECOND")]
	[Description("Multiplier for DPS")]
	public float multiDAMAGE_PER_SECOND { get; set; }

	[Setting]
	[Category("Item stats multiplier SECONDARY")]
	[DisplayName("DEFENSE_SKILL_RATING")]
	[Description("Multiplier for Defense")]
	public float multiDEFENSE_SKILL_RATING { get; set; }

	[Setting]
	[Category("Item stats multiplier SECONDARY")]
	[DisplayName("DODGE_RATING")]
	[Description("Multiplier for Dodge")]
	public float multiDODGE_RATING { get; set; }

	[Setting]
	[Category("Item stats multiplier SECONDARY")]
	[DisplayName("EXPERTISE_RATING")]
	[Description("Multiplier for Expertise")]
	public float multiEXPERTISE_RATING { get; set; }

	[Setting]
	[Category("Item stats multiplier SECONDARY")]
	[DisplayName("FERAL_ATTACK_POWER")]
	[Description("Multiplier for Feral Attack Power")]
	public float multiFERAL_ATTACK_POWER { get; set; }

	[Setting]
	[Category("Item stats multiplier SECONDARY")]
	[DisplayName("HASTE_MELEE_RATING")]
	[Description("Multiplier for Haste (melee)")]
	public float multiHASTE_MELEE_RATING { get; set; }

	[Setting]
	[Category("Item stats multiplier SECONDARY")]
	[DisplayName("HASTE_RANGED_RATING")]
	[Description("Multiplier for Haste (ranged)")]
	public float multiHASTE_RANGED_RATING { get; set; }

	[Setting]
	[Category("Item stats multiplier SECONDARY")]
	[DisplayName("HASTE_RATING")]
	[Description("Multiplier for Haste")]
	public float multiHASTE_RATING { get; set; }

	[Setting]
	[Category("Item stats multiplier SECONDARY")]
	[DisplayName("HASTE_SPELL_RATING")]
	[Description("Multiplier for Haste (spell)")]
	public float multiHASTE_SPELL_RATING { get; set; }

	[Setting]
	[Category("Item stats multiplier BASIC")]
	[DisplayName("HEALTH")]
	[Description("Multiplier for Health")]
	public float multiHEALTH { get; set; }

	[Setting]
	[Category("Item stats multiplier BASIC")]
	[DisplayName("HEALTH_REGENERATION")]
	[Description("Multiplier for Health Regeneration (Hp5)")]
	public float multiHEALTH_REGENERATION { get; set; }

	[Setting]
	[Category("Item stats multiplier SECONDARY")]
	[DisplayName("HIT_MELEE_RATING")]
	[Description("Multiplier for Hit (melee)")]
	public float multiHIT_MELEE_RATING { get; set; }

	[Setting]
	[Category("Item stats multiplier SECONDARY")]
	[DisplayName("HIT_RANGED_RATING")]
	[Description("Multiplier for Hit (ranged)")]
	public float multiHIT_RANGED_RATING { get; set; }

	[Setting]
	[Category("Item stats multiplier SECONDARY")]
	[DisplayName("HIT_RATING")]
	[Description("Multiplier for Hit")]
	public float multiHIT_RATING { get; set; }

	[Setting]
	[Category("Item stats multiplier SECONDARY")]
	[DisplayName("HIT_SPELL_RATING")]
	[Description("Multiplier for Hit (spell)")]
	public float multiHIT_SPELL_RATING { get; set; }

	[Setting]
	[Category("Item stats multiplier SECONDARY")]
	[DisplayName("HIT_TAKEN_RATING")]
	[Description("Multiplier for Miss")]
	public float multiHIT_TAKEN_RATING { get; set; }

	[Setting]
	[Category("Item stats multiplier SECONDARY")]
	[DisplayName("HIT_TAKEN_SPELL_RATING")]
	[Description("Multiplier for Spell miss")]
	public float multiHIT_TAKEN_SPELL_RATING { get; set; }

	[Setting]
	[Category("Item stats multiplier SECONDARY")]
	[DisplayName("HIT_TAKEN_MELEE_RATING")]
	[Description("Multiplier for Melee miss")]
	public float multiHIT_TAKEN_MELEE_RATING { get; set; }

	[Setting]
	[Category("Item stats multiplier SECONDARY")]
	[DisplayName("HIT_TAKEN_RANGED_RATING")]
	[Description("Multiplier for Ranged miss")]
	public float multiHIT_TAKEN_RANGED_RATING { get; set; }

	[Setting]
	[Category("Item stats multiplier BASIC")]
	[DisplayName("INTELLECT")]
	[Description("Multiplier for Intellect")]
	public float multiINTELLECT { get; set; }

	[Setting]
	[Category("Item stats multiplier BASIC")]
	[DisplayName("MANA")]
	[Description("Multiplier for Mana")]
	public float multiMANA { get; set; }

	[Setting]
	[Category("Item stats multiplier BASIC")]
	[DisplayName("MANA_REGENERATION")]
	[Description("Multiplier for Mana Regeneration (Mp5)")]
	public float multiMANA_REGENERATION { get; set; }

	[Setting]
	[Category("Item stats multiplier SECONDARY")]
	[DisplayName("MELEE_ATTACK_POWER")]
	[Description("Multiplier for Attack Power (melee)")]
	public float multiMELEE_ATTACK_POWER { get; set; }

	[Setting]
	[Category("Item stats multiplier SECONDARY")]
	[DisplayName("PARRY_RATING")]
	[Description("Multiplier for Parry")]
	public float multiPARRY_RATING { get; set; }

	[Setting]
	[Category("Item stats multiplier SECONDARY")]
	[DisplayName("RANGED_ATTACK_POWER")]
	[Description("Multiplier for Attack Power (ranged)")]
	public float multiRANGED_ATTACK_POWER { get; set; }

	[Setting]
	[Category("Item stats multiplier SECONDARY")]
	[DisplayName("RESILIENCE_RATING")]
	[Description("Multiplier for Resilience")]
	public float multiRESILIENCE_RATING { get; set; }

	[Setting]
	[Category("Item stats multiplier BASIC")]
	[DisplayName("SPELL_DAMAGE_DONE")]
	[Description("Multiplier for Spellpower")]
	public float multiSPELL_DAMAGE_DONE { get; set; }

	[Setting]
	[Category("Item stats multiplier BASIC")]
	[DisplayName("SPELL_HEALING_DONE")]
	[Description("Multiplier for Healing")]
	public float multiSPELL_HEALING_DONE { get; set; }

	[Setting]
	[Category("Item stats multiplier BASIC")]
	[DisplayName("SPELL_POWER")]
	[Description("Multiplier for Spellpower")]
	public float multiSPELL_POWER { get; set; }

	[Setting]
	[Category("Item stats multiplier SECONDARY")]
	[DisplayName("SPELL_PENETRATION")]
	[Description("Multiplier for Penetration")]
	public float multiSPELL_PENETRATION { get; set; }

	[Setting]
	[Category("Item stats multiplier BASIC")]
	[DisplayName("SPIRIT")]
	[Description("Multiplier for Spirit")]
	public float multiSPIRIT { get; set; }

	[Setting]
	[Category("Item stats multiplier BASIC")]
	[DisplayName("STAMINA")]
	[Description("Multiplier for Stamina")]
	public float multiSTAMINA { get; set; }

	[Setting]
	[Category("Item stats multiplier BASIC")]
	[DisplayName("STRENGTH")]
	[Description("Multiplier for Strength")]
	public float multiSTRENGTH { get; set; }

	[Setting]
	[Category("Common Settings")]
	[DisplayName("Replace Food")]
	[Description("Replace Food from General Settings with known food you have most in inventory")]
	public bool replaceFood { get; set; }

	[Setting]
	[Category("Common Settings")]
	[DisplayName("Replace Drink")]
	[Description("Replace Manadrink from General Settings with known drink you have most in inventory")]
	public bool replaceDrink { get; set; }
}
