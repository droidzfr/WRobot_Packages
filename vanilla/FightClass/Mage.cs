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
    public float Range { get { return 30.0f; } }
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
    public Spell ConjureWater = new Spell ("Conjure Water");
    public Spell ConjureFood = new Spell ("Conjure Food");
 
    // Buff:
    public Spell AI = new Spell("Arcane Intellect");
    public Spell FrostArmor = new Spell("Frost Armor");

	

 
    // Range Combat:
    public Spell Fireball = new Spell("Fireball");
	public WoWSpell Pyroblast  = new WoWSpell("Pyroblast", 10000);
	public Spell FireBlast = new Spell("Fire Blast");


     

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
                        if (Fight.InFight && ObjectManager.Me.Target > 0 && ObjectManager.Target.IsAttackable)
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
 
            Thread.Sleep(100); // Pause 100 ms to reduce the CPU usage.
        }
        Logging.Write("Druid FC  Is now stopped.");
    }

	
	
    public void Buff()
    {
        if (AI.KnownSpell && !ObjectManager.Me.HaveBuff("Arcane Intellect") && ObjectManager.Me.ManaPercentage > 70)
        {
			Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
            AI.Launch();
			Lua.LuaDoString("ClearTarget();");
        }
		
        if (FrostArmor.KnownSpell && !ObjectManager.Me.HaveBuff("Frost Armor") && ObjectManager.Me.ManaPercentage > 70)
        {
			Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
            FrostArmor.Launch();
			Lua.LuaDoString("ClearTarget();");
        }
		
        if (ConjureWater.KnownSpell && ItemsManager.GetItemCountByIdLUA(5350) <= 4 && ItemsManager.GetItemCountByIdLUA(2288) <= 4  && ItemsManager.GetItemCountByIdLUA(2136) <= 4  && ItemsManager.GetItemCountByIdLUA(3772) <= 4  && ItemsManager.GetItemCountByIdLUA(8077) <= 4  && ItemsManager.GetItemCountByIdLUA(8078) <= 4  && ObjectManager.Me.ManaPercentage > 70 && !Fight.InFight)
        {
                SpellManager.CastSpellByNameLUA("Conjure Water");
        }
		
        if (ConjureFood.KnownSpell && ItemsManager.GetItemCountByIdLUA(5349) <= 4 && ItemsManager.GetItemCountByIdLUA(1113) <= 4  && ItemsManager.GetItemCountByIdLUA(1114) <= 4  && ItemsManager.GetItemCountByIdLUA(1487) <= 4  && ItemsManager.GetItemCountByIdLUA(8075) <= 4  && ItemsManager.GetItemCountByIdLUA(8076) <= 4  && ObjectManager.Me.ManaPercentage > 70 && !Fight.InFight)
        {
                SpellManager.CastSpellByNameLUA("Conjure Food");
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
                System.Threading.Thread.Sleep(200);
            }
			
			if (Pyroblast.KnownSpell && ObjectManager.Target.HealthPercent >= 99 && ObjectManager.Target.GetDistance < 30)
            {
				this.Pyroblast.Launch();
            } 	
			
			if (Fireball.KnownSpell && ObjectManager.Target.GetDistance < 30)
            {
				Fireball.Launch();
            } 	
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