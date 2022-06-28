using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using robotManager.Helpful;
using robotManager.Products;
using wManager.Wow.Class;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using System.Configuration;
using System.ComponentModel;
using System.IO;
using robotManager;
using System.Collections.Generic;
using Timer = robotManager.Helpful.Timer;

public class Main : ICustomClass
{
    public float Range { get { return 29f; } }
    private bool FishTacoWarlockLaunched;

    /*-----------------------------------------------------------------------Start of Spells list------------------------------------------------------------------------------------------------*/
    public Spell DemonSkin = new Spell("Demon Skin");
	public int[] DemonSkinM = new int[] {0, 20, 48, 0, 0, 0, 0, 0, 0, 0, 0};
	
    public Spell DemonArmor = new Spell("Demon Armor");
	public int[] DemonArmorM = new int[] {0, 110, 208, 320, 460, 632, 820};
	
    public Spell FelArmor = new Spell("Fel Armor");
	public int[] FelArmorM = new int[] {0, 637, 725, 0, 0, 0, 0, 0, 0, 0, 0};
	
    public Spell Corruption = new Spell("Corruption");
	public int[] CorruptionM = new int[] {0, 35, 55, 100, 160, 225, 290, 340, 370};
	
    public Spell Immolate = new Spell("Immolate");
	public int[] ImmolateM = new int[] {0, 25, 45, 90, 150, 220, 295, 370, 380, 720, 1000};
	
    public Spell CurseOfAgony = new Spell("Curse Of Agony");
	public int[] CurseOfAgonyM = new int[] {0, 25, 50, 90, 130, 170, 215, 265};
	
    public Spell DrainSoul = new Spell("Drain Soul");
	public int[] DrainSoulM = new int[] {0, 55, 125, 210, 290, 360, 0, 0, 0, 0};
	
    public Spell UnstableAffliction = new Spell("Unstable Affliction");
	public int[] UnstableAfflictionM = new int[] {0, 270, 330, 400, 0, 0, 0, 0, 0};
	
    public Spell SiphonLife = new Spell("Siphon Life");
	public int[] SiphonLifeM = new int[] {0, 140, 190, 250, 310, 350, 410};
	
    public Spell LifeTap = new Spell("Life Tap");
	public int[] LifeTapM = new int[] {0, 0, 0, 0, 0, 0, 0, 0};
	
    public Spell Fear = new Spell("Fear");
	public int[] FearM = new int[] {0, 313, 313, 313, 0, 0, 0, 0, 0, 0, 0, 0};
	
    public Spell Shadowfiend = new Spell("Shadow fiend");
	public int[] ShadowfiendM = new int[] {0, 0, 0, 0, 0, 0, 0, 0, 0};
	
    public Spell ShadowBolt = new Spell("Shadow Bolt");
	public int[] ShadowBoltM = new int[] {0, 25, 40, 70, 110, 160, 210, 265, 315, 370, 380, 420};
	
    public Spell Shoot = new Spell("Shoot");
	
	public Spell ConsumeShadows = new Spell("Consume Shadows");
	public int[] ConsumeShadowsM = new int[] {0, 85, 150, 215, 285, 380, 480, 595};
	
	public Spell BloodPact = new Spell("Blood Pact");
	
	public Spell Suffering = new Spell("Suffering");
	public int[] SufferingM = new int[] {0, 0, 0, 0, 0, 0, 0, 0, 0}; // to do
	
	public Spell Sacrifice = new Spell("Sacrifice");
	public int[] SacrificeM = new int[] {0, 0, 0, 0, 0, 0, 0, 0, 0}; // to do
	
	public Spell DrainMana = new Spell("Drain Mana");
	public int[] DrainManaM = new int[] {0, 95, 155, 225, 310, 385, 455};
	
	public Spell DrainLife = new Spell("Drain Life");
	public int[] DrainLifeM = new int[] {0, 55, 85, 135, 185, 240, 300, 355, 425};
	
	public Spell SoulFire = new Spell("Soul Fire");
	public int[] SoulFireM = new int[] {0, 170, 185, 215, 250, 250, 250, 250, 250, 250, 250, 0, 0};
	
	public Spell CreateHealthStone = new Spell("Create HealthStone");
	public int[] CreateHealthStoneM = new int[] {0, 95, 240, 475, 750, 1120, 1390, 0, 0, 0, 0};
	
	public Spell HowlOfTerror = new Spell("Howl Of Terror");
	public int[] HowlOfTerrorM = new int[] {0, 150, 200, 200, 200, 200, 200, 200, 200, 0, 0};
	
    // Pets list start
    public Spell SummonImp = new Spell("Summon Imp");
	public int[] SummonImpM = new int[] {0, 1673, 0, 0, 0, 0, 0, 0, 0, 0};
	
    public Spell SummonVoidwalker = new Spell("Summon Voidwalker");
	public int[] SummonVoidwalkerM = new int[] {0, 2092, 0, 0, 0, 0, 0, 0, 0, 0};
	
	public Spell Attack = new Spell("Attack");
    // Pets list end

    // fur the lulz start
    public bool canWand = false;
    public bool WandLock = false;
	public bool NewPet = true;
	public bool BattleON = false;
	public int ActionsTook = 0;
	public string LastAction = "";
    public WoWUnit Target;
    public WoWUnit Me;
    public WoWUnit Pet;
	public DateTime HowlTimer;
    // fur the lulz end
    /*-----------------------------------------------------------------------End of Spells list------------------------------------------------------------------------------------------------*/

    public void Initialize() // Start
    {
        FishTacoWarlockLaunched = true;
        FishTacoWarlockSettings.Load();
        Start(); // Init finished
    }

	public void Start()
	{
		Logging.Write("FishTacoWarlock Started");
		while (FishTacoWarlockLaunched) // Turned on
		{
			Target = ObjectManager.Target;
			Me = ObjectManager.Me;
			Pet = ObjectManager.Pet;
			if (!Products.InPause && !ObjectManager.Me.IsDeadMe)
			{
				if (Me.CastingTimeLeft >= 1500 && (LastAction.Contains("Imp") && Pet.Name == "Imp") || (LastAction.Contains("Woidwalker") && Pet.Name == "Woidwalker")) // cancel multicast pets
				{
					Lua.RunMacroText("/stopcasting");
					LastAction = "";
				}
				if (Fight.InFight && (LastAction == "Summon Woidwalker" || LastAction == "Summon Imp")) // cancel pet call during combat
				{
					Lua.RunMacroText("/stopcasting");
					LastAction = "";
				}
				if (!Fight.InFight)
				{
					LastAction = "";
					BuffShield();
					PetManagement();
					HealthStoneCreation();
					if (ActionsTook > 0 && BattleON)
					{
						Logging.Write("Actions took in battle: " + ActionsTook.ToString());
						ActionsTook = 0;
						BattleON = false;
					}
				}
				else if (Fight.InFight && Target.HealthPercent > 0)
				{
					BattleON = true;
					Rotation();
				}
			}
			Thread.Sleep(500); // Step timer
		}
	}

	public void Dispose() // Turn off
	{
		FishTacoWarlockLaunched = false;
		Logging.Write("FishTacoWarlock stopped.");
	}

	private void Rotation()
	{
		if (Me.HealthPercent <= 8)
		{
			if (ItemsNameCheck("Healthstone") != "")
			{
				Lua.RunMacroText("/use " + ItemsNameCheck("Healthstone"));
			}
		}
		if (!Pet.IsDead)
		{
			if (!Pet.HasTarget || Target.HealthPercent == 100)
			{
				Lua.RunMacroText("/petattack");
			}
			if (Pet.Name == "Voidwalker")
			{
				if (Sacrifice.IsSpellUsable && Pet.HealthPercent <= 10 && Me.HealthPercent <= 10 && Target.HealthPercent >= 20)
				{
					PetCastSpell(Sacrifice);
				}
				if (Suffering.IsSpellUsable && Pet.GetDistance <= 5 && Target.GetDistance <= 5 && Target.InCombatWithMe && !Target.IsDead)
				{
					PetCastSpell(Suffering);
				}
			}
		}
		if (Me.ManaPercentage >= 15) // When mana is enough
		{
			if(AmountOfAttackers() > 1 && SpellOK(HowlOfTerror) && (((((DateTime.UtcNow) - HowlTimer).TotalSeconds) > 45) || HowlTimer == null) && FishTacoWarlockSettings.CurrentSetting.HowlOfTerror && Me.Mana >= HowlOfTerrorM[SpellRank(HowlOfTerror)])
			{
				CastSpell(HowlOfTerror);
				HowlTimer = (DateTime.UtcNow);
			}
			else if(ActionsTook == 0 && ItemCheck("Soul Shard") && SpellOK(SoulFire, true) && FishTacoWarlockSettings.CurrentSetting.SoulFire && (!SpellOK(SummonVoidwalker) || (!Pet.IsDead && Pet.Name == "Voidwalker")) && Me.Mana >= SoulFireM[SpellRank(SoulFire)])
			{
				CastSpell(SoulFire);
			}
			else if (((Pet.HealthPercent > 0 && Target.HealthPercent <= 15) || Target.HealthPercent <= 5) && !ItemCheck("Soul Shard") && SpellOK(DrainSoul, true) && FishTacoWarlockSettings.CurrentSetting.DrainSH && Bag.GetContainerNumFreeSlots > 0 && Me.Mana >= DrainSoulM[SpellRank(DrainSoul)])
			{
				CastSpell(DrainSoul);
			}
			else if (SpellOK(UnstableAffliction, true) && !BuffOK(UnstableAffliction, Target) && Me.Mana >= UnstableAfflictionM[SpellRank(UnstableAffliction)] && Target.HealthPercent >= 15)
			{
				CastSpell(UnstableAffliction);
			}
			else if (SpellOK(Immolate, true, false, true) && !BuffOK(Immolate, Target) && !BuffOK(UnstableAffliction, Target) && Me.Mana >= ImmolateM[SpellRank(Immolate)] && Target.HealthPercent >= 15)
			{
				CastSpell(Immolate);
			}
			else if (SpellOK(CurseOfAgony, true) && !BuffOK(CurseOfAgony, Target) && Me.Mana >= CurseOfAgonyM[SpellRank(CurseOfAgony)] && Target.HealthPercent >= 15)
			{
				CastSpell(CurseOfAgony);
			}
			else if (SpellOK(SiphonLife, true) && !BuffOK(SiphonLife, Target) && Me.Mana >= SiphonLifeM[SpellRank(SiphonLife)] && Target.HealthPercent >= 15)
			{
				CastSpell(SiphonLife);
			}
			else if (SpellOK(Corruption, true) && !BuffOK(Corruption, Target) && Me.Mana >= CorruptionM[SpellRank(Corruption)] && Target.HealthPercent >= 15)
			{
				CastSpell(Corruption);
			}
			else
			{
				if (SpellOK(DrainLife, true) && FishTacoWarlockSettings.CurrentSetting.DrainLife && Me.HealthPercent < 50 && Me.Mana >= DrainLifeM[SpellRank(DrainLife)])
				{
					CastSpell(DrainLife);
				}
				else if (CheckForWand() && FishTacoWarlockSettings.CurrentSetting.UseWandCondition == 2)
				{
					CastSpell(Shoot);
				}
				else if (Me.Mana >= ShadowBoltM[SpellRank(ShadowBolt)])
				{
					CastSpell(ShadowBolt);
				}
			}
		}
		else // when low on mana
		{
			if (Me.HealthPercent < 50 && Target.GetDistance <= 20 && SpellOK(Fear, true, false) && !BuffOK(Fear, Target) && Me.Mana >= FearM[SpellRank(Fear)])
			{
				CastSpell(Fear);
			}
			else if (((Pet.HealthPercent > 0 && Target.HealthPercent <= 15) || Target.HealthPercent <= 5) && !ItemCheck("Soul Shard") && SpellOK(DrainSoul, true) && FishTacoWarlockSettings.CurrentSetting.DrainSH && Bag.GetContainerNumFreeSlots > 0 && Me.Mana >= DrainSoulM[SpellRank(DrainSoul)])
			{
				CastSpell(DrainSoul);
			}
			else if (SpellOK(LifeTap) && FishTacoWarlockSettings.CurrentSetting.LifeTap && Me.ManaPercentage <= 10 && Me.HealthPercent > 70)
			{
				CastSpell(LifeTap);
			}
			if (SpellOK(DrainLife, true) && FishTacoWarlockSettings.CurrentSetting.DrainLife && Me.HealthPercent < 50 && Me.Mana >= DrainLifeM[SpellRank(DrainLife)])
			{
				CastSpell(DrainLife);
			}
			else if (SpellOK(DrainMana) && FishTacoWarlockSettings.CurrentSetting.DrainMana && Target.Mana > 100 && Me.ManaPercentage <= 15 && Me.Mana >= DrainManaM[SpellRank(DrainMana)])
			{
				CastSpell(DrainMana);
			}
			else if (CheckForWand() && FishTacoWarlockSettings.CurrentSetting.UseWandCondition > 0)
			{
				CastSpell(Shoot);
			}
			else if (Me.Mana >= ShadowBoltM[SpellRank(ShadowBolt)])
			{
				CastSpell(ShadowBolt);
			}
			else
			{
				CastSpell(Attack);
			}
			return;
		}
	}

    /*-----------------------------------------------------------------------Start of action rules------------------------------------------------------------------------------------------------*/
	public void BuffShield()
	{
		if (ObjectManager.Me.ManaPercentage > 70)
		{
			if (BuffOK(FelArmor, Me) || BuffOK(DemonArmor, Me) || BuffOK(DemonSkin, Me))
			{
				return;
			}
			else if (SpellOK(FelArmor) && !BuffOK(FelArmor, Me) && Me.Mana >= FelArmorM[SpellRank(FelArmor)])
			{
				CastSpell(FelArmor);
			}
			else if (SpellOK(DemonArmor) && !BuffOK(DemonArmor, Me) && Me.Mana >= DemonArmorM[SpellRank(DemonArmor)])
			{
				CastSpell(DemonArmor);
			}
			else if (SpellOK(DemonSkin) && !BuffOK(DemonSkin, Me) && Me.Mana >= DemonSkinM[SpellRank(DemonSkin)])
			{
				CastSpell(DemonSkin);
			}
		}
	}
	
	public int SpellRank(Spell ThisSpell)
	{
		int Rank = 0;
		foreach (uint current in SpellListManager.SpellIdByName(ThisSpell.Name))
		{
			if (SpellManager.SpellBookID().Contains((current)))
			{
				Rank++;
			}
		}
		return Rank;
	}
	
	public bool BuffOK(Spell Buff, WoWUnit UnitTarget)
	{
		return UnitTarget.HaveBuff(Buff.Name);
	}

	public bool SpellOK(Spell CurrentSpell, bool DG = false, bool SU = false, bool Repeat = false)
	{
		//Logging.Write("Spell check: " + CurrentSpell.Name.ToString() + " Distance: " + DG.ToString() + " Usable: " + SU.ToString());
		if (LastAction == CurrentSpell.Name && Repeat == true)
		{
			return false;
		}
		if(!CurrentSpell.KnownSpell)
		{
			return false;
		}
		if(!CurrentSpell.IsDistanceGood && DG == true)
		{
			return false;
		}
		if(!Shadowfiend.IsSpellUsable && SU == true)
		{
			return false;
		}
		return true;
	}
	
	private void PetCastSpell(Spell Current)
	{
		SpellManager.CastSpellByNameLUA(Current.Name);
		ActionsTook++;
		return;
	}

	private void CastSpell(Spell Current)
	{
		if (ObjectManager.Me.CastingSpellId == 0)
		{
			Logging.Write("Spell casted: " + Current.Name.ToString());
			LastAction = Current.Name;
			if (Current.Name == "Shoot")
			{
				Lua.RunMacroText("/cast !Shoot");
			}
			else
			{
				SpellManager.CastSpellByNameLUA(Current.Name);
			}
			ActionsTook++;
		}
		return;
	}

	private bool CheckForWand()
	{
		foreach (WoWItem current in wManager.Wow.Helpers.EquippedItems.GetEquippedItems())
		{
			if (current.GetItemInfo.ItemEquipLoc == "INVTYPE_RANGEDRIGHT")
			{
				return true;
			}
		}
		return false;
	}

	private int AmountOfAttackers()
	{
		int Qty = 0;
		List<WoWUnit> woWUnitAttackables = ObjectManager.GetWoWUnitAttackables(3.40282347E+38f);
		foreach (WoWUnit current in woWUnitAttackables)
		{
			if (current.IsTargetingMe && current.GetDistance <= 10 && !BuffOK(HowlOfTerror, Target))
			{
				Qty=Qty+1;
			}
		}
		return Qty;
	}
	
	private string ItemsNameCheck(string scanitem)
	{
		foreach (WoWItem current in Bag.GetBagItem())
		{
			if ((current.GetItemInfo.ItemName.Contains(scanitem)))
			{
				return current.GetItemInfo.ItemName;
			}
		}
		return "";
	}

	private bool ItemsCheck(string scanitem)
	{
		foreach (WoWItem current in Bag.GetBagItem())
		{
			if ((current.GetItemInfo.ItemName.Contains(scanitem)))
			{
				return true;
			}
		}
		return false;
	}
	
	private bool ItemCheck(string scanitem)
	{
		foreach (WoWItem current in Bag.GetBagItem())
		{
			if ((current.GetItemInfo.ItemName) == (scanitem))
			{
				return true;
			}
		}
		return false;
	}
	
	public void HealthStoneCreation()
	{
		if (FishTacoWarlockSettings.CurrentSetting.CreateHS && Bag.GetContainerNumFreeSlots > 0)
		{
			if (SpellOK(CreateHealthStone) && !ItemsCheck("Healthstone") && ItemCheck("Soul Shard") && Me.Mana >= CreateHealthStoneM[SpellRank(CreateHealthStone)])
			{
				if (!SpellOK(SummonVoidwalker) || (!Pet.IsDead && Pet.Name == "Voidwalker"))
				{
					CastSpell(CreateHealthStone);
				}
			}
		}
	}
	
	public void PetManagement()
	{
		if ((!ObjectManager.Pet.IsValid || ObjectManager.Pet.IsDead) && !ObjectManager.Me.IsMounted)
		{
			if (SpellOK(SummonVoidwalker) && ItemCheck("Soul Shard") && Me.ManaPercentage >= 25)
			{
				Thread.Sleep(200);
				CastSpell(SummonVoidwalker);
				NewPet = true;
			}
			else if (SpellOK(SummonImp) && Me.ManaPercentage >= 20)
			{
				Thread.Sleep(200);
				CastSpell(SummonImp);
				NewPet = true;
			}
			return;
		}
		if (Pet.Name != "" && !Pet.IsDead)
		{
			if (Pet.Name == "Imp")
			{
				if (NewPet == true)
				{
					NewPet = false;
					Lua.RunMacroText("/petautocaston Firebolt");
				}
				if (BloodPact.IsSpellUsable && !BuffOK(BloodPact, Me))
				{
					PetCastSpell(BloodPact);
				}
				if (ItemCheck("Soul Shard") && SpellOK(SummonVoidwalker))
				{
					CastSpell(SummonVoidwalker);
					return;
				}
			}
			if (Pet.Name == "Voidwalker")
			{
				if (NewPet == true)
				{
					NewPet = false;
					Lua.RunMacroText("/petautocaston Torment");
				}
				if (Pet.HealthPercent < FishTacoWarlockSettings.CurrentSetting.PetHeal && ConsumeShadows.IsSpellUsable && Pet.Mana >= ConsumeShadowsM[SpellRank(ConsumeShadows)] && !BuffOK(ConsumeShadows, Pet))
				{
					PetCastSpell(ConsumeShadows);
				}
			}
		}
	}
    /*-----------------------------------------------------------------------End of action rules------------------------------------------------------------------------------------------------*/

    /*-----------------------------------------------------------------------Start of settings------------------------------------------------------------------------------------------------*/

    public void ShowConfiguration() // When use click on Fight class settings
    {
        FishTacoWarlockSettings.Load();
        FishTacoWarlockSettings.CurrentSetting.ToForm();
        FishTacoWarlockSettings.CurrentSetting.Save();
    }
    [Serializable]
    public class FishTacoWarlockSettings : Settings
    {
		private int _UseWandCondition = 2;
        [Setting]
        [DefaultValue(2)]
        [Category("Settings")]
        [DisplayName("Use Wand")]
        [Description("0 = Never 1 = When low on mana 2 = Always")]
        public int UseWandCondition { get { return _UseWandCondition; } set { _UseWandCondition = value; } }
		
		private bool _DrainSH = true;
        [Setting]
        [DefaultValue(true)]
        [Category("Settings")]
        [DisplayName("Fill soul shard")]
        [Description("True = Fill soul shard\nFalse = Do not fill soul shards")]
        public bool DrainSH { get { return _DrainSH; } set { _DrainSH = value; } }
		
		private bool _DrainMana = true;
        [Setting]
        [DefaultValue(true)]
        [Category("Settings")]
        [DisplayName("Drain mana")]
        [Description("True = Will drain mana on low mana level\nFalse = Will not drain mana on low mana level")]
        public bool DrainMana { get { return _DrainMana; } set { _DrainMana = value; } }

		private bool _DrainLife = true;
        [Setting]
        [DefaultValue(true)]
        [Category("Settings")]
        [DisplayName("Drain life")]
        [Description("True = Will drain life on low life level\nFalse = Will not drain life on low life level")]
        public bool DrainLife { get { return _DrainLife; } set { _DrainLife = value; } }
		
		private bool _LifeTap = true;
        [Setting]
        [DefaultValue(true)]
        [Category("Settings")]
        [DisplayName("Life Tap")]
        [Description("True = Will use Life tap on low mana when atleast 70 % of health is remaining\nFalse = Will not use Life tap.")]
        public bool LifeTap { get { return _LifeTap; } set { _LifeTap = value; } }
		
		private int _PetHeal = 70;
		[Setting]
		[DefaultValue(70)]
		[Category("Settings")]
		[DisplayName("Heal Voidwalker at health %")]
		[Description("Select at which rate of health Voidwalker will be healed")]
		public int PetHeal { get { return _PetHeal; } set { _PetHeal = value; } }
		
		private bool _SoulFire = false;
        [Setting]
        [DefaultValue(false)]
        [Category("Settings")]
        [DisplayName("Soul Fire")]
        [Description("True = Use Soul Fire\nFalse = Do not use Soul Fire")]
        public bool SoulFire { get { return _SoulFire; } set { _SoulFire = value; } }
		
		private bool _CreateHS = true;
        [Setting]
        [DefaultValue(true)]
        [Category("Settings")]
        [DisplayName("Create HealthStone")]
        [Description("True = Create Healthstone if possible\nFalse = Do not create a HealthStone")]
        public bool CreateHS { get { return _CreateHS; } set { _CreateHS = value; } }
		
		private bool _HowlOfTerror = true;
        [Setting]
        [DefaultValue(true)]
        [Category("Settings")]
        [DisplayName("Howl Of Terror")]
        [Description("True = Use Howl Of Terror\nFalse = Do not use Howl Of Terror")]
        public bool HowlOfTerror { get { return _HowlOfTerror; } set { _HowlOfTerror = value; } }
		
        public FishTacoWarlockSettings()
        {
            ConfigWinForm(new System.Drawing.Point(400, 400), "FishTaco Warlock Settings");
        }

        public static FishTacoWarlockSettings CurrentSetting { get; set; }

        public bool Save()
        {
            try
            {
                return Save(AdviserFilePathAndName("FishTacoWarlock", ObjectManager.Me.Name + "." + Usefuls.RealmName));
            }
            catch (Exception e)
            {
                Logging.WriteError("FishTacoWarlockSettings > Save(): " + e);
                return false;
            }
        }

        public static bool Load()
        {
            try
            {
                if (File.Exists(AdviserFilePathAndName("FishTacoWarlock", ObjectManager.Me.Name + "." + Usefuls.RealmName)))
                {
                    CurrentSetting = Load<FishTacoWarlockSettings>(AdviserFilePathAndName("FishTacoWarlock",ObjectManager.Me.Name + "." + Usefuls.RealmName));
                    return true;
                }
                CurrentSetting = new FishTacoWarlockSettings();
            }
            catch (Exception e)
            {
                Logging.WriteError("FishTacoWarlockSettings > Load(): " + e);
            }
            return false;
        }
    }
    /*-----------------------------------------------------------------------End of settings------------------------------------------------------------------------------------------------*/
}
