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
using System.Diagnostics;
using robotManager;
using System.IO;
using wManager.Wow;

public class Main : ICustomClass
{
    public float Range 
	{ 
		get 
		{
			if(RangeCheck == true)
			{
			  return 30f;
			}
			return 5f;
		} 
	}
    private Random _r = new Random();
    private readonly uint _wowBase = (uint)Memory.WowMemory.Memory.GetProcess().MainModule.BaseAddress;
    private bool _isLaunched;
    private ulong _lastTarget;
    private ulong _currentTarget;
    private uint _target;
    uint oldTarget;
	public bool _attackRepeating =false;
    public bool _autoshotRepeating;
	public bool RangeCheck;
	
	
	
    private bool Canpoison(WoWUnit unit)
    {
        return unit.CreatureTypeTarget != "Elemental" && unit.CreatureTypeTarget != "Mechanical";
    }
 
    public void Initialize() // When product started, initialize and launch Fightclass
    {
        _isLaunched = true;
        Logging.Write("Lock FC Is initialized.");
        Rotation();
    }
 
    public void Dispose() // When product stopped
    {
        _isLaunched = false;
        Logging.Write("Lock FC Stop in progress.");
    }
     
    public void ShowConfiguration() // When use click on Fight class settings
    {
    }
 
 
    // SPELLS:
    // Healthstone:
    private Spell RevivePet = new Spell("Revive Pet");
    private Spell CallPet = new Spell("Call Pet");
    public  Spell MendPet = new Spell("Mend Pet");
    public  Spell FeedPet = new Spell("Feed Pet");

    // Buff:
    public Spell AspectHawk = new Spell("Aspect of the Hawk");
    public Spell AspectMonkey = new Spell("Aspect of the Monkey");
    public Spell HuntersMark = new Spell("Hunter's Mark");

    //Crowd Control
    public Spell ConcussiveShot = new Spell("Concussive Shot");

    // Close Combat:
    public Spell RaptorStrike = new Spell("Raptor Strike");
    public Spell WingClip = new Spell("Wing Clip");


    // Ranged Combat:
    public Spell SerpentSting = new Spell("Serpent Sting");
    public Spell ArcaneShot = new Spell("Arcane Shot");
    public Spell AutoShot = new Spell("Auto Shot");
    public Spell AimedShot = new Spell("Aimed Shot");


 
    internal void Rotation()
    {
        Logging.Write("Lock FC started.");
        while (_isLaunched)
        {
            try
            {
                if (!Products.InPause)
                {
                    if (!ObjectManager.Me.IsDeadMe)
                    {
						PetManager();
                        Buff();
						Feed();	
						UseScroll();
						if (ObjectManager.Me.Target > 0  && ObjectManager.Target.IsAttackable)
                        {
                            Lua.LuaDoString("PetAttack();");
                        }
                        if (Fight.InFight && ObjectManager.Me.Target > 0 && ObjectManager.Target.IsAttackable)
                        {
							RangeManager();
                            CombatRotation();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logging.WriteError("Lock FC  ERROR: " + e);
            }
 
            Thread.Sleep(10); // Pause 10 ms to reduce the CPU usage.
        }
        Logging.Write("Lock FC  Is now stopped.");
    }

	private void RangeManager()
	{	
		wManager.Events.FightEvents.OnFightLoop += (unit, cancelable) => {
            if (ObjectManager.Target.GetDistance < 11 && ObjectManager.Target.IsTargetingMyPet && ObjectManager.Target.HealthPercent >= 50)
            {
                RangeCheck = false;
                var xvector = (ObjectManager.Me.Position.X) - (ObjectManager.Target.Position.X);
                var yvector = (ObjectManager.Me.Position.Y) - (ObjectManager.Target.Position.Y);

                Vector3 newpos = new Vector3()
                {
                    X = ObjectManager.Me.Position.X + (float)((xvector * (20 / ObjectManager.Target.GetDistance) - xvector)),
                    Y = ObjectManager.Me.Position.Y + (float)((yvector * (20 / ObjectManager.Target.GetDistance) - yvector)),
                    Z = ObjectManager.Me.Position.Z
                };
                MovementManager.Go(PathFinder.FindPath(newpos), false);
                Thread.Sleep(1500);
            }
			if (ObjectManager.Target.GetDistance < 11 && ObjectManager.Target.HealthPercent <= 50)
            {
                RangeCheck = true;
            }
			
        };    
	}
	
	internal void PetManager()
    {
        // Toon is dead or on mount => Must do nothing
        if (!ObjectManager.Me.IsDeadMe || !ObjectManager.Me.IsMounted)
        {

        // Call Pet
        if (!ObjectManager.Pet.IsValid && CallPet.KnownSpell)
        {
            CallPet.Launch();
            Thread.Sleep(Usefuls.Latency + 1000);
        }
        // Revive Pet
        if (!ObjectManager.Pet.IsValid || ObjectManager.Pet.IsDead && RevivePet.KnownSpell)
            RevivePet.Launch();

		if (ObjectManager.Pet.IsValid && ObjectManager.Me.IsAlive &&  MendPet.KnownSpell && MendPet.IsDistanceGood && ObjectManager.Pet.HealthPercent <=60)
        {
            MendPet.Launch();
        }
      
	    }
		
    }
	
    public void Feed()	
	{
		if (ObjectManager.Target.IsDead && FeedPet.KnownSpell && ObjectManager.Target.HealthPercent <= 1)
			{
						 if (ObjectManager.Pet.IsAlive && !ObjectManager.Me.IsCast && !ObjectManager.Pet.HaveBuff("Feed Pet Effect"))
       					 {
       					     	Lua.LuaDoString("if GetPetHappiness() < 3 then\r\n  CastSpellByName(\"Feed Pet\")\r\n  PickupContainerItem(0, 1)\r\nend  ");
									Thread.Sleep(500);
       					 }
						 if (ObjectManager.Pet.IsAlive && !ObjectManager.Me.IsCast && !ObjectManager.Pet.HaveBuff("Feed Pet Effect"))
       					 {
       					     	Lua.LuaDoString("if GetPetHappiness() < 3 then\r\n  CastSpellByName(\"Feed Pet\")\r\n  PickupContainerItem(0, 2)\r\nend  ");
									Thread.Sleep(500);
       					 }
						 if (ObjectManager.Pet.IsAlive && !ObjectManager.Me.IsCast && !ObjectManager.Pet.HaveBuff("Feed Pet Effect"))
       					 {
       					     	Lua.LuaDoString("if GetPetHappiness() < 3 then\r\n  CastSpellByName(\"Feed Pet\")\r\n  PickupContainerItem(0, 3)\r\nend  ");
									Thread.Sleep(500);
       					 }
						 if (ObjectManager.Pet.IsAlive && !ObjectManager.Me.IsCast && !ObjectManager.Pet.HaveBuff("Feed Pet Effect"))
       					 {
       					     	Lua.LuaDoString("if GetPetHappiness() < 3 then\r\n  CastSpellByName(\"Feed Pet\")\r\n  PickupContainerItem(0, 4)\r\nend  ");
									Thread.Sleep(500);
       					 }
			}
	}


    public void Buff()
    {
        // Aspect of the Hawk
        if (AspectHawk.KnownSpell && !ObjectManager.Me.HaveBuff("Aspect of the Hawk"))
        {
            AspectHawk.Launch();
        }

        // Aspect of the Monkey
        if (AspectMonkey.KnownSpell && !ObjectManager.Me.HaveBuff("Aspect of the Monkey") && !AspectHawk.KnownSpell)
        {
            AspectMonkey.Launch();
        }        
    }

	internal void UseScroll()
	        {
				if (!Fight.InFight)
					{
						// Agi scroll- send to pet
						if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(3012) && !ObjectManager.Me.IsDeadMe)
						{
							Interact.InteractGameObject(ObjectManager.Pet.GetBaseAddress);
							ItemsManager.UseItem(3012);
						}
						else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(1477) && !ObjectManager.Me.IsDeadMe)
						{
							Interact.InteractGameObject(ObjectManager.Pet.GetBaseAddress);
							ItemsManager.UseItem(1477);
						}
						else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(4425) && !ObjectManager.Me.IsDeadMe)
						{
							Interact.InteractGameObject(ObjectManager.Pet.GetBaseAddress);
							ItemsManager.UseItem(4425);
						}
						else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(10309) && !ObjectManager.Me.IsDeadMe)
						{
							Interact.InteractGameObject(ObjectManager.Pet.GetBaseAddress);
							ItemsManager.UseItem(10309);
						}
						else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(27498) && !ObjectManager.Me.IsDeadMe)
						{
							Interact.InteractGameObject(ObjectManager.Pet.GetBaseAddress);
							ItemsManager.UseItem(27498);
						}
						else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(33457) && !ObjectManager.Me.IsDeadMe)
						{
							Interact.InteractGameObject(ObjectManager.Pet.GetBaseAddress);
							ItemsManager.UseItem(33457);
						}
						else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(43463) && !ObjectManager.Me.IsDeadMe)
						{
							Interact.InteractGameObject(ObjectManager.Pet.GetBaseAddress);
							ItemsManager.UseItem(43463);
						}
						else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(43464) && !ObjectManager.Me.IsDeadMe)
						{
							Interact.InteractGameObject(ObjectManager.Pet.GetBaseAddress);
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
							Interact.InteractGameObject(ObjectManager.Pet.GetBaseAddress);
							ItemsManager.UseItem(3013);
						}
						else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(1478) && !ObjectManager.Me.IsDeadMe)
						{
							Interact.InteractGameObject(ObjectManager.Pet.GetBaseAddress);
							ItemsManager.UseItem(1478);
						}
						else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(4421) && !ObjectManager.Me.IsDeadMe)
						{
							Interact.InteractGameObject(ObjectManager.Pet.GetBaseAddress);
							ItemsManager.UseItem(4421);
						}
						else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(10305) && !ObjectManager.Me.IsDeadMe)
						{
							Interact.InteractGameObject(ObjectManager.Pet.GetBaseAddress);
							ItemsManager.UseItem(10305);
						}
						else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(27500) && !ObjectManager.Me.IsDeadMe)
						{
							Interact.InteractGameObject(ObjectManager.Pet.GetBaseAddress);
							ItemsManager.UseItem(27500);
						}
						else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(33459) && !ObjectManager.Me.IsDeadMe)
						{
							Interact.InteractGameObject(ObjectManager.Pet.GetBaseAddress);
							ItemsManager.UseItem(33459);
						}
						else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(43467) && !ObjectManager.Me.IsDeadMe)
						{
							Interact.InteractGameObject(ObjectManager.Pet.GetBaseAddress);
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
							Interact.InteractGameObject(ObjectManager.Pet.GetBaseAddress);
							ItemsManager.UseItem(954);
						}
						else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(2289) && !ObjectManager.Me.IsDeadMe)
						{
							Interact.InteractGameObject(ObjectManager.Pet.GetBaseAddress);
							ItemsManager.UseItem(2289);
						}
						else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(4426) && !ObjectManager.Me.IsDeadMe)
						{
							Interact.InteractGameObject(ObjectManager.Pet.GetBaseAddress);
							ItemsManager.UseItem(4426);
						}
						else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(10310) && !ObjectManager.Me.IsDeadMe)
						{
							Interact.InteractGameObject(ObjectManager.Pet.GetBaseAddress);
							ItemsManager.UseItem(10310);
						}
						else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(27503) && !ObjectManager.Me.IsDeadMe)
						{
							Interact.InteractGameObject(ObjectManager.Pet.GetBaseAddress);
							ItemsManager.UseItem(27503);
						}
						else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(33462) && !ObjectManager.Me.IsDeadMe)
						{
							Interact.InteractGameObject(ObjectManager.Pet.GetBaseAddress);
							ItemsManager.UseItem(33462);
						}
						else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(43465) && !ObjectManager.Me.IsDeadMe)
						{
							Interact.InteractGameObject(ObjectManager.Pet.GetBaseAddress);
							ItemsManager.UseItem(43465);
						}
				}
            }

    internal void CombatRotation()
    {
				// auto tag avoid 
         if (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause && Fight.InFight)
        {
            if (Lua.LuaDoString<bool>(@"return (UnitIsTapped(""target"")) and (not UnitIsTappedByPlayer(""target""));"))
            {
                Fight.StopFight();
                Lua.LuaDoString("ClearTarget();");
                System.Threading.Thread.Sleep(400);
            }
		}
		

        //Mend Pet
        if (ObjectManager.Pet.IsValid &&  MendPet.KnownSpell && ObjectManager.Pet.HealthPercent <=30)
        {
            MendPet.Launch();
            //channeling ?
            return;
        }
		
        // Hunter's Mark:
        if (HuntersMark.KnownSpell && ObjectManager.Pet.IsValid && !HuntersMark.TargetHaveBuff && ObjectManager.Target.GetDistance > 10)
        {
            HuntersMark.Launch();
        }
		
		if (SerpentSting.KnownSpell && !ObjectManager.Target.HaveBuff("Serpent Sting") && ObjectManager.Target.GetDistance < 34 && ObjectManager.Target.GetDistance > 10 && Canpoison(ObjectManager.Me.TargetObject) && ObjectManager.Target.HealthPercent >=30 && ObjectManager.Me.ManaPercentage > 15)
        {
			Lua.LuaDoString("PetAttack();");
            SerpentSting.Launch();
        }
        //Auto Shot
        if (!SpellManager.IsRepeating(AutoShot.Ids)  && ObjectManager.Target.GetDistance > 10 && ObjectManager.Target.GetDistance <= 34 )
        {
            AutoShot.Launch();
            Thread.Sleep(Usefuls.Latency + 1300);
            //AutoShot.Launch();
            return;
        }
		
		
		if (ArcaneShot.KnownSpell && ObjectManager.Target.GetDistance < 34 && ObjectManager.Target.HealthPercent >=30 && ObjectManager.Me.ManaPercentage > 75 && ObjectManager.Target.GetDistance > 10)
        {
            ArcaneShot.Launch();
        }
		
		if (RaptorStrike.KnownSpell && ObjectManager.Target.GetDistance < 8)
        {
            RaptorStrike.Launch();
        }
    }
	    public class WoWSpell : Spell
    {
        private Timer _timer;

        #region Constructor

        /// <summary>
        /// Creates a new instance of the <see cref="WoWSpell"/> class.
        /// </summary>
        /// <param name="spellNameEnglish">The spell name.</param>
        /// <param name="cooldownTimer">The cooldown time.</param>
        public WoWSpell(string spellNameEnglish, double cooldownTimer)
            : base(spellNameEnglish)
        {
            // Set timer
            this._timer = new Timer(cooldownTimer);
        }

        #endregion

        #region Public

        public bool IsReady
        {
            get
            {
                return this._timer.IsReady;
            }
        }

        /// <summary>
        /// Casts the spell if it is ready.
        /// </summary>
        public new void Launch()
        {
            // Is ready?
            if (!this.IsReady)
            {
                // Return
                return;
            }

            // Call launch
            base.Launch();

            // Reset timer
            this._timer.Reset();
        }

        #endregion
    }
}