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
    public float Range { get { return 25.0f; } }
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
        Logging.Write("Priest FC Is initialized.");
        Rotation();
    }
 
    public void Dispose() // When product stopped
    {
        _isLaunched = false;
        Logging.Write("Priest FC Stop in progress.");
    }
     
    public void ShowConfiguration() // When use click on Fight class settings
    {
    }
 
 
    // SPELLS:
    //
 
    // Buff:
    public Spell PowerWordFortitude = new Spell("Power Word: Fortitude");
    public Spell InnerFire = new Spell("Inner Fire");
    public Spell ShadowProtection = new Spell("Shadow Protection");
    public Spell Shadowform = new Spell("Shadowform");
    public Spell Renew = new Spell("Renew");
    public Spell Shield = new Spell("Power Word: Shield");
 
    // Range Combat:
    public Spell Smite = new Spell("Smite");
    public Spell ShadowWordPain = new Spell("Shadow word: Pain");
    public WoWSpell MindBlast = new WoWSpell("Mind Blast", 10000);
    public Spell DevouringPlague = new Spell("Devouring Plague");
    public Spell VampiricEmbrace = new Spell("Vampiric Embrace");
    public Spell LesserHeal = new Spell("Lesser Heal");
    public Spell FlashHeal = new Spell("Flash Heal");
    public Spell Shoot = new Spell("Shoot");
    public Spell Mindflay = new Spell("Mind Flay");
    public WoWSpell HolyFire = new WoWSpell("Holy Fire", 6000);



     

    public bool WandActive()
    {
        return Memory.WowMemory.Memory.ReadInt32(_wowBase + 0x7E0BA0) > 0;
    }
 
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
                Logging.WriteError("Priest FC  ERROR: " + e);
            }
 
            Thread.Sleep(100); // Pause 10 ms to reduce the CPU usage.
        }
        Logging.Write("Priest FC  Is now stopped.");
    }


    public void Buff()
    {
        if (PowerWordFortitude.KnownSpell && !ObjectManager.Me.HaveBuff("Power Word: Fortitude"))
        {
			Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
            PowerWordFortitude.Launch();
        }
        if (InnerFire.KnownSpell && !ObjectManager.Me.HaveBuff("Inner Fire"))
        {
            InnerFire.Launch();
        }
        if (ShadowProtection.KnownSpell && !ObjectManager.Me.HaveBuff("Shadow Protection"))
        {
			Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
            ShadowProtection.Launch();
        }
        if (Shadowform.KnownSpell && !ObjectManager.Me.HaveBuff("Shadowform"))
        {
            Shadowform.Launch();
        }
        if (Renew.KnownSpell && !ObjectManager.Me.HaveBuff("Renew") && !ObjectManager.Me.HaveBuff("Shadowform") && ObjectManager.Me.HealthPercent <= 70  && ObjectManager.Me.ManaPercentage > 10)
        {
			Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
            Renew.Launch();
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
        if (ObjectManager.Me.HealthPercent <= 40 && !FlashHeal.KnownSpell)
        {
            LesserHeal.Launch();
        } 
         
        if (ObjectManager.Me.HealthPercent <= 40 && FlashHeal.KnownSpell && !ObjectManager.Me.HaveBuff("Shadowform"))
        {
            FlashHeal.Launch();
        } 
		
        if (ObjectManager.Me.HealthPercent <= 70 && Shield.KnownSpell && !ObjectManager.Me.HaveBuff("Power Word: Shield") && !ObjectManager.Me.HaveBuff("Weakened Soul"))
        {
            Shield.Launch();
        } 
		
        if (VampiricEmbrace.KnownSpell && !ObjectManager.Target.HaveBuff("Vampiric Embrace") && ObjectManager.Target.GetDistance < 28)
        {
            VampiricEmbrace.Launch();
        }
         
        if (MindBlast.KnownSpell && ObjectManager.Me.ManaPercentage > 60 && ObjectManager.Target.GetDistance < 28)
        {
            this.MindBlast.Launch();
        }
		
        if (HolyFire.KnownSpell && !Shadowform.KnownSpell && ObjectManager.Me.ManaPercentage > 60 && ObjectManager.Target.GetDistance < 25 && ObjectManager.Target.GetDistance > 8 && ObjectManager.Target.HealthPercent >= 85)
        {
            this.HolyFire.Launch();
        }
 
        if (ShadowWordPain.KnownSpell && !ObjectManager.Target.HaveBuff("Shadow Word: Pain") && ObjectManager.Target.GetDistance < 28)
        {
            ShadowWordPain.Launch();
        }

        if (Mindflay.KnownSpell && ObjectManager.Me.ManaPercentage > 40 && ObjectManager.Target.GetDistance < 20 && ObjectManager.Me.HaveBuff("Power Word: Shield"))
        {
            Mindflay.Launch();
            Thread.Sleep(Usefuls.Latency);
            Usefuls.WaitIsCasting();
        }
		
         
        if (Smite.KnownSpell && ObjectManager.Me.ManaPercentage > 20 && ObjectManager.Me.HealthPercent >= 40 && ObjectManager.Me.Level < 5 && ObjectManager.Target.GetDistance < 25)
        {
            Smite.Launch();
        }
         
        if (DevouringPlague.KnownSpell && ObjectManager.Target.GetDistance < 25)
        {
            DevouringPlague.Launch();
        }
        
		if (!Lua.LuaDoString<bool>("return IsAutoRepeatAction(" + (SpellManager.GetSpellSlotId(SpellListManager.SpellIdByName("Shoot")) + 1) + ")") && ObjectManager.Me.HealthPercent >= 40)
				{
					if (Shoot.KnownSpell)
						SpellManager.CastSpellByNameLUA("Shoot");
					return;
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