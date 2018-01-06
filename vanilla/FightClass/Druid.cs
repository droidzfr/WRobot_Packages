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
    public float Range { get { return 5.0f; } }
    private Random _r = new Random();
    private readonly uint _wowBase = (uint)Memory.WowMemory.Memory.GetProcess().MainModule.BaseAddress;
    private bool _isLaunched;
    private ulong _lastTarget;
    private ulong _currentTarget;
    private uint _target;
    uint oldTarget;
 
    public void Initialize() // When product started, initialize and launch Fightclass
    {
        _isLaunched = true;
        Logging.Write("Feral Druid FC Is initialized.");
        Rotation();
    }
 
    public void Dispose() // When product stopped
    {
        _isLaunched = false;
        Logging.Write("Feral Druid FC Stop in progress.");
    }
     
    public void ShowConfiguration() // When use click on Fight class settings
    {
    }
	
    private bool CanBleed(WoWUnit unit)
    {
        return unit.CreatureTypeTarget != "Elemental" && unit.CreatureTypeTarget != "Mechanical";
    }
 
 
    // SPELLS:
    //
 
    // Buff:
    public Spell Mark = new Spell("Mark of the Wild");
    public Spell Thorns = new Spell("Thorns");
    public Spell Innervate = new Spell("Innervate");
    public Spell Omen = new Spell("Omen of Clarity");
    public Spell decurse = new Spell("Remove Curse");
	

 
    // Range Combat:
    public Spell Moonfire = new Spell("Moonfire");
    //public Spell HealingTouch  = new Spell("Healing Touch");
    public WoWSpell HealingTouch = new WoWSpell("Healing Touch", 6000);
	public Spell Rejuvenation  = new Spell("Rejuvenation");
	public Spell Bear = new Spell("Bear Form");
	public Spell Cat  = new Spell("Cat Form");
	public Spell Maul  = new Spell("Maul");
	public WoWSpell Regrowth  = new WoWSpell("Regrowth", 5000);
	public Spell Claw  = new Spell("Claw");
	public Spell Rake  = new Spell("Rake");
	public Spell Rip  = new Spell("Rip");
	public Spell FaerieFire = new Spell("Faerie Fire (Feral)()");	
	public Spell FerociousBite = new Spell("Ferocious Bite");
	public Spell Roar = new Spell("Demoralizing Roar");	
	public Spell Bash  = new Spell("bash");
	public Spell Skinning  = new Spell("Skinning");




     

    internal void Rotation()
    {
        Logging.Write("Priest FC started.");
        while (_isLaunched)
        {
            try
            {
                if (!Products.InPause)
                {
                    if (!ObjectManager.Me.IsDeadMe)
                    {
                        Buff();
                        if (Fight.InFight && ObjectManager.Me.Target > 0)
                        {
                            CombatRotation();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logging.WriteError("Druid FC  ERROR: " + e);
            }
 
            Thread.Sleep(400); // Pause 100 ms to reduce the CPU usage.
        }
        Logging.Write("Druid FC  Is now stopped.");
    }

	
	
    public void Buff()
    {
        if (Mark.KnownSpell && !ObjectManager.Me.HaveBuff("Mark of the Wild") && !ObjectManager.Me.HaveBuff("Cat Form") && !ObjectManager.Me.HaveBuff("Bear Form") && ObjectManager.Me.ManaPercentage > 65)
        {
			Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
            Mark.Launch();
			Lua.LuaDoString("ClearTarget();");
        }
        if (Thorns.KnownSpell && !ObjectManager.Me.HaveBuff("Thorns") && !ObjectManager.Me.HaveBuff("Cat Form") && !ObjectManager.Me.HaveBuff("Bear Form") && ObjectManager.Me.ManaPercentage > 65)
        {
			Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
            Thorns.Launch();
			Lua.LuaDoString("ClearTarget();");
        }
		
        if (ObjectManager.Me.HealthPercent <= 60 && !ObjectManager.Me.HaveBuff("Cat Form") && !ObjectManager.Me.HaveBuff("Bear Form") && ObjectManager.Me.ManaPercentage > 30 && !ObjectManager.Me.HaveBuff("Rejuvenation") && !ObjectManager.Me.InCombatFlagOnly)
        {
            this.HealingTouch.Launch();
        }
		
        if (Rejuvenation.KnownSpell && !ObjectManager.Me.HaveBuff("Rejuvenation") && ObjectManager.Me.HealthPercent <= 65 && !ObjectManager.Me.HaveBuff("Cat Form") && !ObjectManager.Me.HaveBuff("Bear Form") && ObjectManager.Me.ManaPercentage > 45 && !ObjectManager.Me.InCombatFlagOnly)
        {
            Rejuvenation.Launch();
        }
		
        if (decurse.KnownSpell && ObjectManager.Me.HaveBuff("Curse of Thorns") && !ObjectManager.Me.HaveBuff("Cat Form") && !ObjectManager.Me.HaveBuff("Bear Form") && ObjectManager.Me.ManaPercentage > 45 && !ObjectManager.Me.InCombatFlagOnly)
        {
            decurse.Launch();
        }
		
        if (Innervate.KnownSpell  && ObjectManager.Me.ManaPercentage < 30 && !ObjectManager.Me.HaveBuff("Cat Form") && !ObjectManager.Me.HaveBuff("Bear Form") && Innervate.IsSpellUsable)
        {
            Innervate.Launch();
        } 
		
        if (Omen.KnownSpell && !ObjectManager.Me.HaveBuff("Omen of Clarity")  && ObjectManager.Me.ManaPercentage > 30 && !ObjectManager.Me.HaveBuff("Cat Form") && !ObjectManager.Me.HaveBuff("Bear Form") && ObjectManager.Me.ManaPercentage > 70)
        {
            Omen.Launch();
        }
		
		// break bear for the vendor man
		 if ((ObjectManager.Target.IsNpcVendor) && !(ObjectManager.Me.InCombatFlagOnly) && ObjectManager.Me.HaveBuff("Bear Form"))
		 {
			    Lua.LuaDoString("CastSpellByName(\"Bear Form\",1)");
				Thread.Sleep(400);
		 }	 
		 //break cat for the vendor man
		if ((ObjectManager.Target.IsNpcVendor) && !(ObjectManager.Me.InCombatFlagOnly) && ObjectManager.Me.HaveBuff("Cat Form"))
		{
		        Lua.LuaDoString("CastSpellByName(\"Cat Form\",1)");	
				Thread.Sleep(400);
		}
				
		// break bear for the quest man
 
		 if ((wManager.Wow.ObjectManager.ObjectManager.Target.Reaction == wManager.Wow.Enums.Reaction.Friendly) && !(ObjectManager.Me.InCombatFlagOnly) && ObjectManager.Me.HaveBuff("Bear Form"))
		 {
			    Lua.LuaDoString("CastSpellByName(\"Bear Form\",1)");
				Thread.Sleep(400);
		 }	
		 if ((wManager.Wow.ObjectManager.ObjectManager.Target.Reaction == wManager.Wow.Enums.Reaction.Honored) && !(ObjectManager.Me.InCombatFlagOnly) && ObjectManager.Me.HaveBuff("Bear Form"))
		 {
			    Lua.LuaDoString("CastSpellByName(\"Bear Form\",1)");
				Thread.Sleep(400);
		 }	
		 if ((wManager.Wow.ObjectManager.ObjectManager.Target.Reaction == wManager.Wow.Enums.Reaction.Revered) && !(ObjectManager.Me.InCombatFlagOnly) && ObjectManager.Me.HaveBuff("Bear Form"))
		 {
			    Lua.LuaDoString("CastSpellByName(\"Bear Form\",1)");
				Thread.Sleep(400);
		 }	
		 //break cat for the quest man

		if ((wManager.Wow.ObjectManager.ObjectManager.Target.Reaction == wManager.Wow.Enums.Reaction.Friendly) && !(ObjectManager.Me.InCombatFlagOnly) && ObjectManager.Me.HaveBuff("Cat Form"))
		{
		        Lua.LuaDoString("CastSpellByName(\"Cat Form\",1)");	
				Thread.Sleep(400);
		}		
		if ((wManager.Wow.ObjectManager.ObjectManager.Target.Reaction == wManager.Wow.Enums.Reaction.Honored) && !(ObjectManager.Me.InCombatFlagOnly) && ObjectManager.Me.HaveBuff("Cat Form"))
		{
		        Lua.LuaDoString("CastSpellByName(\"Cat Form\",1)");	
				Thread.Sleep(400);
		}		
		if ((wManager.Wow.ObjectManager.ObjectManager.Target.Reaction == wManager.Wow.Enums.Reaction.Revered) && !(ObjectManager.Me.InCombatFlagOnly) && ObjectManager.Me.HaveBuff("Cat Form"))
		{
		        Lua.LuaDoString("CastSpellByName(\"Cat Form\",1)");	
				Thread.Sleep(400);
		}		
		var nodesNearMe = ObjectManager.GetObjectWoWGameObject().FindAll(p => p.GetDistance <= 8 && p.CanOpen);
		
		// break bear for the nodes 
		 if (nodesNearMe.Count > 0 && !(ObjectManager.Me.InCombatFlagOnly) && ObjectManager.Me.HaveBuff("Bear Form"))
		 {
			    Lua.LuaDoString("CastSpellByName(\"Bear Form\",1)");
				Thread.Sleep(400);
		 }	 
		 //break cat for the nodes
		if (nodesNearMe.Count > 0 && !(ObjectManager.Me.InCombatFlagOnly) && ObjectManager.Me.HaveBuff("Cat Form"))
		{
		        Lua.LuaDoString("CastSpellByName(\"Cat Form\",1)");	
				Thread.Sleep(400);
				
		}
		
		// break bear for the trainer man
		 if ((ObjectManager.Target.IsNpcTrainer) && !(ObjectManager.Me.InCombatFlagOnly) && ObjectManager.Me.HaveBuff("Bear Form"))
		 {
			    Lua.LuaDoString("CastSpellByName(\"Bear Form\",1)");
				Thread.Sleep(400);
		 }	 
		 //break cat for the trainer man
		if ((ObjectManager.Target.IsNpcTrainer) && !(ObjectManager.Me.InCombatFlagOnly) && ObjectManager.Me.HaveBuff("Cat Form"))
		{
		        Lua.LuaDoString("CastSpellByName(\"Cat Form\",1)");
				Thread.Sleep(400);
		}

		// break bear for the out of combat heals
		 if (!ObjectManager.Me.HaveBuff("Rejuvenation") && !(ObjectManager.Me.InCombatFlagOnly) && ObjectManager.Me.HaveBuff("Bear Form") && ObjectManager.Me.HealthPercent <= 60 && ObjectManager.Me.ManaPercentage > 50 && ObjectManager.Me.Target < 1)
		 {
			    Lua.LuaDoString("CastSpellByName(\"Bear Form\",1)");
				Thread.Sleep(400);
		 }	 
		 //break cat for the out of combat heals
		if (!ObjectManager.Me.HaveBuff("Rejuvenation") && !ObjectManager.Me.InCombatFlagOnly && ObjectManager.Me.HaveBuff("Cat Form") && ObjectManager.Me.HealthPercent <= 40 && ObjectManager.Me.ManaPercentage > 50 && ObjectManager.Me.Target < 1)
		{
		        Lua.LuaDoString("CastSpellByName(\"Cat Form\",1)");	
				Thread.Sleep(400);
				
		}
		// break bear for the buffs
		 if (!ObjectManager.Me.HaveBuff("Thorns") && !(ObjectManager.Me.InCombatFlagOnly) && ObjectManager.Me.HaveBuff("Bear Form") && ObjectManager.Me.ManaPercentage > 90)
		 {
			    Lua.LuaDoString("CastSpellByName(\"Bear Form\",1)");
				Thread.Sleep(400);
		 }	 
		 //break cat for the buffs
		if (!ObjectManager.Me.HaveBuff("Thorns") && !(ObjectManager.Me.InCombatFlagOnly) && ObjectManager.Me.HaveBuff("Cat Form") && ObjectManager.Me.ManaPercentage > 90)
		{
		        Lua.LuaDoString("CastSpellByName(\"Cat Form\",1)");	
				Thread.Sleep(400);
		}
		// break bear for the buffs
		 if (!ObjectManager.Me.HaveBuff("Mark of the Wild") && !(ObjectManager.Me.InCombatFlagOnly) && ObjectManager.Me.HaveBuff("Bear Form") && ObjectManager.Me.ManaPercentage > 80)
		 {
			    Lua.LuaDoString("CastSpellByName(\"Bear Form\",1)");
				Thread.Sleep(400);
		 }	 
		 //break cat for the buffs
		if (!ObjectManager.Me.HaveBuff("Mark of the Wild") && !(ObjectManager.Me.InCombatFlagOnly) && ObjectManager.Me.HaveBuff("Cat Form") && ObjectManager.Me.ManaPercentage > 80)
		{
		        Lua.LuaDoString("CastSpellByName(\"Cat Form\",1)");	
				Thread.Sleep(400);
		}

		//get into cat for travel
        if (!ObjectManager.Me.HaveBuff("Cat Form") && ObjectManager.Me.HealthPercent >= 81 && Cat.KnownSpell && ObjectManager.Me.Target < 1 && ObjectManager.Me.ManaPercentage > 70 && !(ObjectManager.Me.InCombatFlagOnly) && ObjectManager.Me.HaveBuff("Thorns") && ObjectManager.Me.HaveBuff("Mark of the Wild"))
        {
             Cat.Launch();
			 Thread.Sleep(400);
        } 		
/*
		//Drop cat for the drinking.... omfg i cant believe im doing this
        if (ObjectManager.Me.HaveBuff("Cat Form") && Cat.KnownSpell && ObjectManager.Me.Target < 1 && ObjectManager.Me.ManaPercentage < 35 && !(ObjectManager.Me.InCombatFlagOnly))
        {
             Cat.Launch();
			 Thread.Sleep(400);
        } 	
		// bear too  jfc
        if (ObjectManager.Me.HaveBuff("Bear Form") && Bear.KnownSpell && ObjectManager.Me.Target < 1 && ObjectManager.Me.ManaPercentage < 35 && !(ObjectManager.Me.InCombatFlagOnly))
        {
             Bear.Launch();
			 Thread.Sleep(400);
        } 	
*/
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
		
         if (Regrowth.KnownSpell && !ObjectManager.Me.HaveBuff("Regrowth") && ObjectManager.Me.HealthPercent <= 35 && !ObjectManager.Me.HaveBuff("Cat Form") && !ObjectManager.Me.HaveBuff("Bear Form"))
        {
            this.Regrowth.Launch();
        }
		
         if (!ObjectManager.Me.HaveBuff("Rejuvenation") && ObjectManager.Me.HealthPercent <= 70 && !ObjectManager.Me.HaveBuff("Cat Form") && !ObjectManager.Me.HaveBuff("Bear Form") && ObjectManager.Me.ManaPercentage > 30)
        {
            Rejuvenation.Launch();
        }
		
        if (decurse.KnownSpell && ObjectManager.Me.HaveBuff("Curse of Thorns") && !ObjectManager.Me.HaveBuff("Cat Form") && !ObjectManager.Me.HaveBuff("Bear Form") && ObjectManager.Me.ManaPercentage > 45)
        {
            decurse.Launch();
        }

        if (ObjectManager.Me.HealthPercent <= 35 && !ObjectManager.Me.HaveBuff("Cat Form") && !ObjectManager.Me.HaveBuff("Bear Form") && !Regrowth.KnownSpell && ObjectManager.Me.ManaPercentage > 30)
        {
            this.HealingTouch.Launch();
        } 
		
        if (ObjectManager.Me.HealthPercent <= 40 && !ObjectManager.Me.HaveBuff("Cat Form") && !ObjectManager.Me.HaveBuff("Bear Form") && Regrowth.KnownSpell && ObjectManager.Me.HaveBuff("Regrowth") && ObjectManager.Me.ManaPercentage > 30)
        {
            this.HealingTouch.Launch();
        } 
 
        if (Moonfire.KnownSpell && !ObjectManager.Target.HaveBuff("Moonfire") && ObjectManager.Target.GetDistance < 20 && !Bear.KnownSpell && ObjectManager.Me.ManaPercentage > 30)
        {
            Moonfire.Launch();
        }


		
		// break cat form for the heals
        if (ObjectManager.Me.HaveBuff("Cat Form") && ObjectManager.Me.HealthPercent <= 35 && !ObjectManager.Me.HaveBuff("Rejuvenation") && ObjectManager.Me.ManaPercentage > 50)
        {
             Lua.LuaDoString("CastSpellByName(\"Cat Form\",1)");
        }
		
		// break cat form for the heals
        if (ObjectManager.Me.HaveBuff("Bear Form") && ObjectManager.Me.HealthPercent <= 35 && !ObjectManager.Me.HaveBuff("Rejuvenation") && ObjectManager.Me.ManaPercentage > 50)
        {
             Lua.LuaDoString("CastSpellByName(\"Bear Form\",1)");
        }
		
		//Get into bear (combat)
		
        if (!ObjectManager.Me.HaveBuff("Bear Form") && ObjectManager.Me.HealthPercent >= 41 && Bear.KnownSpell && !Cat.KnownSpell && ObjectManager.Target.GetDistance < 12 && ObjectManager.Me.InCombatFlagOnly)
        {
             Bear.Launch();
        }         

        if (Bash.KnownSpell && ObjectManager.Target.IsCast && Bash.IsSpellUsable && ObjectManager.Me.HaveBuff("Bear Form"))
        {
            Bash.Launch();
        }   
		
		if (Roar.KnownSpell && ObjectManager.Me.HaveBuff("Bear Form") && !ObjectManager.Target.HaveBuff("Demoralizing Roar"))
        {
            Roar.Launch();
		} 
		
        if (ObjectManager.Me.HaveBuff("Bear Form") && ObjectManager.Me.Rage >= 15)
        {
             Maul.Launch();
        } 
		
		//Get into Cat (combat)
		
        if (!ObjectManager.Me.HaveBuff("Cat Form") && ObjectManager.Me.HealthPercent >= 41 && Cat.KnownSpell && ObjectManager.Target.HealthPercent > 1)
        {
             Cat.Launch();
        }         
		
		if (ObjectManager.Me.HaveBuff("Cat Form") && !ObjectManager.Target.HaveBuff("Faerie Fire (Feral)"))
        {
            Lua.LuaDoString("CastSpellByName(\"Faerie Fire (Feral)()\")");
		} 

		if (ObjectManager.Me.HaveBuff("Cat Form") && ObjectManager.Me.ComboPoint >= 3 && ObjectManager.Target.HealthPercent >= 30 && !ObjectManager.Target.HaveBuff("Rip") && CanBleed(ObjectManager.Me.TargetObject))
        {
            Rip.Launch();
		} 		

		if (ObjectManager.Me.HaveBuff("Cat Form") && ObjectManager.Me.ComboPoint > 3 && ObjectManager.Target.HealthPercent <= 50 && FerociousBite.KnownSpell)
        {
            FerociousBite.Launch();
			
        } 
		
        if (ObjectManager.Me.HaveBuff("Cat Form") && ObjectManager.Me.ComboPoint <=4 && ObjectManager.Target.GetDistance < 6 && Rake.KnownSpell && !ObjectManager.Target.HaveBuff("Rake") && CanBleed(ObjectManager.Me.TargetObject))
        {
             Rake.Launch();
        }   
		
        if (ObjectManager.Me.HaveBuff("Cat Form") && ObjectManager.Me.ComboPoint <=4 && ObjectManager.Target.GetDistance < 6)
        {
             Claw.Launch();
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