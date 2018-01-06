// Credit: Eeny

using System;
using System.Threading;
using robotManager.Helpful;
using robotManager.Products;
using wManager.Wow.Class;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using Timer = robotManager.Helpful.Timer;
using System.Collections.Generic;
using System.Configuration;
using System.ComponentModel;
using robotManager;
using System.IO;

public class Main : ICustomClass
{
    public float Range { get { return 5.0f; } }

    private bool _isLaunched;
    private ulong _lastTarget;
    private ulong _currentTarget;
    private uint _target;
    uint oldTarget;

    public void Initialize() // When product started, initialize and launch Fightclass
    {
        _isLaunched = true;
        Logging.Write("Pally FC Is initialized.");
        Rotation();
    }

    public void Dispose() // When product stopped
    {
        _isLaunched = false;
        Logging.Write("Pally FC Stop in progress.");
    }
	
    public void ShowConfiguration() // When use click on Fight class settings
    {

    }
	
	 private bool CanExcorise(WoWUnit unit)
    {
        return unit.CreatureTypeTarget != "Demon" && unit.CreatureTypeTarget != "Undead";
    }


    // SPELLS:
    //

    // Buff:
    public Spell BlessingOfWisdom = new Spell("Blessing of Wisdom");
    public Spell BlessingOfMight = new Spell("Blessing of Might");
    public Spell DevAura = new Spell("Devotion Aura");
    public Spell RetAura = new Spell("Retribution Aura");
	public Spell FlashOfLight = new Spell("Flash of Light");
	public Spell Cleanse = new Spell("Purify");


    // Close Combat:
    public Spell Crusader = new Spell("Seal of the Crusader");
	public Spell HolyLight = new Spell("Holy Light");
	public Spell Righteousness = new Spell("Seal of Righteousness");
    public Spell Judgement = new Spell("Judgement");
	public Spell Command = new Spell("Seal of Command");
	public Spell Shock = new Spell("Holy Shock");
	public Spell Consecration = new Spell("Consecration");
	public Spell HoW = new Spell("Hammer of Wrath");
	public Spell Exorcism = new Spell("Exorcism");
	public Spell DivineProtection = new Spell("Divine Protection");
	public Spell DivineShield = new Spell("Divine Shield");
	public Spell LoH = new Spell("Lay on Hands");





    internal void Rotation()
    {
        Logging.Write("pally FC started.");
        while (_isLaunched)
        {
            try
            {
                if (!Products.InPause)
                {
                    if (!ObjectManager.Me.IsDeadMe)
                    {
						Buff();
						Cleansing();
						UseScroll();
                        if (Fight.InFight && ObjectManager.Me.Target > 0)
                        {
                                CombatRotation();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logging.WriteError("pally FC  ERROR: " + e);
            }

            Thread.Sleep(100); // Pause 10 ms to reduce the CPU usage.
        }
        Logging.Write("pally FC  Is now stopped.");
    }
	
	public void Buff()
    {
		if (!RetAura.KnownSpell && DevAura.KnownSpell && !ObjectManager.Me.HaveBuff("Devotion Aura"))
        {
            DevAura.Launch();
        }
		
		if (RetAura.KnownSpell && !ObjectManager.Me.HaveBuff("Retribution Aura"))
        {
            RetAura.Launch();
        }

        if (BlessingOfWisdom.KnownSpell && !ObjectManager.Me.HaveBuff("Blessing of Wisdom") && Command.KnownSpell)
        {
			Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
            BlessingOfWisdom.Launch();
        }

        if (BlessingOfMight.KnownSpell && !ObjectManager.Me.HaveBuff("Blessing of Might") && !ObjectManager.Me.HaveBuff("Blessing of Wisdom") && !Command.KnownSpell)
        {
			Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
            BlessingOfMight.Launch();
        }
		
		if (ObjectManager.Me.HealthPercent <= 70  && ObjectManager.Me.ManaPercentage > 30  && !Fight.InFight && FlashOfLight.KnownSpell)
        {
			Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
			MovementManager.StopMove();
            FlashOfLight.Launch();
			Thread.Sleep(1000);
        } 
		
		if (ObjectManager.Me.HealthPercent <= 50  && ObjectManager.Me.ManaPercentage > 40  && !Fight.InFight && !FlashOfLight.KnownSpell)
        {
			Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
			MovementManager.StopMove();
            HolyLight.Launch();
            Thread.Sleep(2500);
        } 
	}
	
	public void Cleansing()
    {
		if (Cleanse.KnownSpell && ObjectManager.Me.ManaPercentage > 10 && ObjectManager.Me.HaveBuff("Poison"))
        {
            Thread.Sleep(2500);
            Cleanse.Launch();
        }
		if (Cleanse.KnownSpell && ObjectManager.Me.ManaPercentage > 10 && ObjectManager.Me.HaveBuff("Rabies"))
        {
            Thread.Sleep(2500);
            Cleanse.Launch();
        }
		if (Cleanse.KnownSpell && ObjectManager.Me.ManaPercentage > 10 && ObjectManager.Me.HaveBuff("Infected Bite"))
        {
            Thread.Sleep(2500);
            Cleanse.Launch();
        }
		if (Cleanse.KnownSpell && ObjectManager.Me.ManaPercentage > 10 && ObjectManager.Me.HaveBuff("Dark Sludge"))
        {
            Thread.Sleep(2500);
            Cleanse.Launch();
        }
	}
	
    internal void CombatRotation()
    {
         if (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause && Fight.InFight)
        {
            if (Lua.LuaDoString<bool>(@"return (UnitIsTapped(""target"")) and (not UnitIsTappedByPlayer(""target""));"))
            {
                Fight.StopFight();
                Lua.LuaDoString("ClearTarget();");
                System.Threading.Thread.Sleep(400);
            }
		}
		
		if (DivineProtection.KnownSpell && ObjectManager.Me.HealthPercent <= 40  && ObjectManager.Me.ManaPercentage > 30 && !ObjectManager.Me.HaveBuff("Forbearance"))
        {
            DivineProtection.Launch();
            HolyLight.Launch();
        } 
		
		if (DivineShield.KnownSpell && ObjectManager.Me.HealthPercent <= 40  && ObjectManager.Me.ManaPercentage > 30 && !ObjectManager.Me.HaveBuff("Forbearance"))
        {
            DivineShield.Launch();
            HolyLight.Launch();
        } 

        if (ObjectManager.Me.HealthPercent <= 70  && ObjectManager.Me.ManaPercentage > 20 && ObjectManager.Me.HaveBuff("Divine Shield"))
        {
			Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
			MovementManager.StopMove();
            HolyLight.Launch();
        } 
		
        if (ObjectManager.Me.HealthPercent <= 70  && ObjectManager.Me.ManaPercentage > 20 && ObjectManager.Me.HaveBuff("Divine Protection"))
        {
			Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
			MovementManager.StopMove();
            HolyLight.Launch();
        }  
				
        if (ObjectManager.Me.HealthPercent <= 30  && ObjectManager.Me.ManaPercentage > 30)
        {
            HolyLight.Launch();
        } 
		
		if (ObjectManager.Me.HealthPercent <= 40  && ObjectManager.Me.ManaPercentage < 30)
        {
            LoH.Launch();
        } 
		
        if (Crusader.KnownSpell && !ObjectManager.Me.HaveBuff("Seal of the Crusader") && !ObjectManager.Target.HaveBuff("Judgement of the Crusader"))
        {
            Crusader.Launch();
        }     
		
        if (Command.KnownSpell && Judgement.KnownSpell && ObjectManager.Me.HaveBuff("Seal of the Crusader"))
        {
            Judgement.Launch();
        }  		
				
		if (Command.KnownSpell && ObjectManager.Target.HaveBuff("Judgement of the Crusader") && !ObjectManager.Me.HaveBuff("Seal of Command"))
        {
            Command.Launch();
        }
		
		if (Consecration.KnownSpell && ObjectManager.Target.HaveBuff("Judgement of the Crusader") && ObjectManager.Me.ManaPercentage > 40 && ObjectManager.Target.GetDistance <= 10)
        {
            Consecration.Launch();
        }
		
		if (Exorcism.KnownSpell && ObjectManager.Me.ManaPercentage > 60 && !CanExcorise(ObjectManager.Me.TargetObject))
        {
            Exorcism.Launch();
        }
		
		if (HoW.KnownSpell && ObjectManager.Me.ManaPercentage > 70 && ObjectManager.Target.HealthPercent <= 20)
        {
            HoW.Launch();
        }
		
		if (!Command.KnownSpell && ObjectManager.Target.HaveBuff("Judgement of the Crusader") && !ObjectManager.Me.HaveBuff("Seal of Righteousness"))
        {
            Righteousness.Launch();
        }
		// low level righteousness activation
		if (!Crusader.KnownSpell && !ObjectManager.Me.HaveBuff("Seal of Righteousness"))
        {
            Righteousness.Launch();
        }
		
    }
internal void UseScroll()
        {
		if (!Fight.InFight)
			{
				// Agi scroll- send to pet
				if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(3012) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(3012);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(1477) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(1477);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(4425) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(4425);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(10309) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(10309);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(27498) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(27498);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(33457) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(33457);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(43463) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(43463);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(43464) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(43464);
				}
				
				// int scroll- buff yo self
				
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(955) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(955);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(2290) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(2290);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(4419) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(4419);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(10308) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(10308);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(27499) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(27499);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(33458) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(33458);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(37091) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(37091);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(37092) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(37092);
				}
				
				// scroll of protection - sent to pet
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(3013) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(3013);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(1478) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(1478);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(4421) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(4421);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(10305) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(10305);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(27500) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(27500);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(33459) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(33459);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(43467) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(43467);
				}
				
				// scroll of spirit- buff yourself
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(1181) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(1181);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(1712) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(1712);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(4424) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(4424);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(10306) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(10306);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(27501) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(27501);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(33460) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(33460);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(37097) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(37097);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(37098) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(37098);
				}
				
				// stamina  buff self
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(1180) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(1180);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(1711) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(1711);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(4422) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(4422);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(10307) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(10307);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(27502) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(27502);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(33461) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(33461);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(37093) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(37093);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(37094) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(37094);
				}
				
				//strength - send to pet
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(954) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(954);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(2289) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(2289);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(4426) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(4426);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(10310) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(10310);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(27503) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(27503);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(33462) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(33462);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(43465) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(43465);
				}
			}
        }
}
