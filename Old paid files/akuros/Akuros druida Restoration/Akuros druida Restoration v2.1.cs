    using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using robotManager;
using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using wManager.Wow.Class;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using wManager.Wow.Bot.States;
using Timer = robotManager.Helpful.Timer;
using wManager.Wow.Enums;

public class Main : ICustomClass
{
    public float Range { get { return 40; } }

    private bool _usePet = false;
    private Engine _engine;
    public void Initialize()
    {
        AkurosDruidRestorationSettings.Load();
        _engine = new Engine(false)
        {
            States = new List<State>
                        {
                             new SpellState("Mass Entanglement", 25, context => ObjectManager.Me.HealthPercent < 77, false, true, false, false, true, true, true, true, 0, false, false, false, false, false, false, wManager.Wow.Helpers.FightClassCreator.YesNoAuto.Auto, "", "none", true, true, false),
                             new SpellState("Tranquility", 24, context => (wManager.Wow.Helpers.Party.GetPartyHomeAndInstance().OrderBy(p => p.HealthPercent).FirstOrDefault(p => p != null && p.IsValid && !p.IsDead && (!false || Fight.InFight) && (!true || !TraceLine.TraceLineGo(p.Position)) && p.HealthPercent <= 25 && p.GetDistance < 40 && !(ObjectManager.Me.CooldownEnabled("")) && Interact.InteractGameObject(p.GetBaseAddress, !ObjectManager.Me.GetMove))) != null, false, false, false, false, true, true, true, true, 0, false, false, true, false, false, false, wManager.Wow.Helpers.FightClassCreator.YesNoAuto.Yes, "", "none", true, true, false),
                             new SpellState("RunMacroText(\"/Use 13\")", 23, context => (wManager.Wow.Helpers.Party.GetPartyHomeAndInstance().OrderBy(p => p.HealthPercent).FirstOrDefault(p => p != null && p.IsValid && !p.IsDead && (!false || Fight.InFight) && (!true || !TraceLine.TraceLineGo(p.Position)) && !(p.IsBoss) && p.HealthPercent < 75 && Interact.InteractGameObject(p.GetBaseAddress, !ObjectManager.Me.GetMove))) != null, false, false, false, false, true, true, true, true, 120000, true, false, true, false, false, false, wManager.Wow.Helpers.FightClassCreator.YesNoAuto.Auto, "", "player", true, true, false),
                             new SpellState("RunMacroText(\"/cast Nachwachsen\")", 22, context => ObjectManager.Me.HealthPercent < 80, false, true, false, false, true, true, true, true, 0, false, false, false, false, false, false, wManager.Wow.Helpers.FightClassCreator.YesNoAuto.Auto, "", "none", true, true, false),
                             new SpellState("Efflorescence", 21, context => (wManager.Wow.Helpers.Party.GetPartyHomeAndInstance().OrderBy(p => p.HealthPercent).FirstOrDefault(p => p != null && p.IsValid && !p.IsDead && (!false || Fight.InFight) && (!true || !TraceLine.TraceLineGo(p.Position)) && p.GetDistance < 30 && ObjectManager.Me.HealthPercent <= 95 && Interact.InteractGameObject(p.GetBaseAddress, !ObjectManager.Me.GetMove))) != null, false, false, false, false, true, true, true, true, 25000, false, false, true, false, false, false, wManager.Wow.Helpers.FightClassCreator.YesNoAuto.Auto, "", "player", true, true, false),
                             new SpellState("Essence of G'Hanir", 20, context => !(ObjectManager.Me.CooldownEnabled("")) && !(ObjectManager.Target.IsBoss), false, false, false, false, true, true, true, true, 0, false, false, false, false, false, false, wManager.Wow.Helpers.FightClassCreator.YesNoAuto.Auto, "", "none", true, true, false),
                             new SpellState("Lifebloom", 19, context => (wManager.Wow.Helpers.Party.GetPartyHomeAndInstance().OrderBy(p => p.HealthPercent).FirstOrDefault(p => p != null && p.IsValid && !p.IsDead && (!false || Fight.InFight) && (!true || !TraceLine.TraceLineGo(p.Position)) && p.HealthPercent <= 95 && p.GetDistance < 30 && Interact.InteractGameObject(p.GetBaseAddress, !ObjectManager.Me.GetMove))) != null, false, false, false, false, true, true, true, true, 15000, false, false, true, false, false, false, wManager.Wow.Helpers.FightClassCreator.YesNoAuto.Auto, "", "none", true, true, false),
                             new SpellState("Renewal", 18, context => (wManager.Wow.Helpers.Party.GetPartyHomeAndInstance().OrderBy(p => p.HealthPercent).FirstOrDefault(p => p != null && p.IsValid && !p.IsDead && (!false || Fight.InFight) && (!true || !TraceLine.TraceLineGo(p.Position)) && p.HealthPercent < 70 && Interact.InteractGameObject(p.GetBaseAddress, !ObjectManager.Me.GetMove))) != null, false, false, false, false, true, true, true, true, 150000, false, false, true, false, false, false, wManager.Wow.Helpers.FightClassCreator.YesNoAuto.Auto, "", "player", true, true, false),
                             new SpellState("Cenarion Ward", 17, context => (wManager.Wow.Helpers.Party.GetPartyHomeAndInstance().OrderBy(p => p.HealthPercent).FirstOrDefault(p => p != null && p.IsValid && !p.IsDead && (!false || Fight.InFight) && (!true || !TraceLine.TraceLineGo(p.Position)) && p.HealthPercent <= 99 && p.GetDistance < 30 && Interact.InteractGameObject(p.GetBaseAddress, !ObjectManager.Me.GetMove))) != null, false, false, false, false, true, true, true, true, 0, false, false, true, false, false, false, wManager.Wow.Helpers.FightClassCreator.YesNoAuto.Auto, "", "focus", true, true, false),
                             new SpellState("Barkskin", 16, context => !Fight.InFight && (ObjectManager.Me.HealthPercent < 50), true, false, false, false, true, true, true, true, 0, false, false, false, false, false, false, wManager.Wow.Helpers.FightClassCreator.YesNoAuto.Auto, "", "player", true, true, false),
                             new SpellState("Flourish", 15, context => (wManager.Wow.Helpers.Party.GetPartyHomeAndInstance().OrderBy(p => p.HealthPercent).FirstOrDefault(p => p != null && p.IsValid && !p.IsDead && (!false || Fight.InFight) && (!true || !TraceLine.TraceLineGo(p.Position)) && p.HealthPercent < 99 && p.GetDistance < 30 && Interact.InteractGameObject(p.GetBaseAddress, !ObjectManager.Me.GetMove))) != null, false, false, false, false, true, true, true, true, 0, false, false, true, false, false, false, wManager.Wow.Helpers.FightClassCreator.YesNoAuto.Auto, "", "none", true, true, false),
                             new SpellState("Ironbark", 14, context => (wManager.Wow.Helpers.Party.GetPartyHomeAndInstance().OrderBy(p => p.HealthPercent).FirstOrDefault(p => p != null && p.IsValid && !p.IsDead && (!false || Fight.InFight) && (!true || !TraceLine.TraceLineGo(p.Position)) && !(ObjectManager.Me.CooldownEnabled("")) && p.GetDistance < 20 && Interact.InteractGameObject(p.GetBaseAddress, !ObjectManager.Me.GetMove))) != null, false, false, false, false, true, true, true, true, 0, false, false, true, false, false, false, wManager.Wow.Helpers.FightClassCreator.YesNoAuto.Auto, "", "focus", true, true, false),
                             new SpellState("Flourish", 13, context => (wManager.Wow.Helpers.Party.GetPartyHomeAndInstance().OrderBy(p => p.HealthPercent).FirstOrDefault(p => p != null && p.IsValid && !p.IsDead && (!false || Fight.InFight) && (!true || !TraceLine.TraceLineGo(p.Position)) && (ObjectManager.Me.CooldownEnabled("")) && Interact.InteractGameObject(p.GetBaseAddress, !ObjectManager.Me.GetMove))) != null, false, false, false, false, true, true, true, true, 0, false, false, true, false, false, false, wManager.Wow.Helpers.FightClassCreator.YesNoAuto.Auto, "", "none", true, true, false),
                             new SpellState("Regrowth", 12, context => (wManager.Wow.Helpers.Party.GetPartyHomeAndInstance().OrderBy(p => p.HealthPercent).FirstOrDefault(p => p != null && p.IsValid && !p.IsDead && (!false || Fight.InFight) && (!true || !TraceLine.TraceLineGo(p.Position)) && p.GetDistance < 30 && p.HealthPercent < 92 && Interact.InteractGameObject(p.GetBaseAddress, !ObjectManager.Me.GetMove))) != null, false, false, false, false, true, true, true, true, 0, false, false, true, false, false, false, wManager.Wow.Helpers.FightClassCreator.YesNoAuto.Auto, "", "none", true, true, false),
                             new SpellState("Regrowth", 11, context => (wManager.Wow.Helpers.Party.GetPartyHomeAndInstance().OrderBy(p => p.HealthPercent).FirstOrDefault(p => p != null && p.IsValid && !p.IsDead && (!false || Fight.InFight) && (!true || !TraceLine.TraceLineGo(p.Position)) && ObjectManager.Me.HealthPercent < 80 && Interact.InteractGameObject(p.GetBaseAddress, !ObjectManager.Me.GetMove))) != null, false, false, false, false, true, true, true, true, 0, false, false, true, false, false, false, wManager.Wow.Helpers.FightClassCreator.YesNoAuto.Auto, "", "player", true, true, false),
                             new SpellState("Rejuvenation", 10, context => (wManager.Wow.Helpers.Party.GetPartyHomeAndInstance().OrderBy(p => p.HealthPercent).FirstOrDefault(p => p != null && p.IsValid && !p.IsDead && (!false || Fight.InFight) && (!true || !TraceLine.TraceLineGo(p.Position)) && ObjectManager.Me.HealthPercent < 95 && Interact.InteractGameObject(p.GetBaseAddress, !ObjectManager.Me.GetMove))) != null, false, false, false, false, true, true, true, true, 16000, false, false, true, false, false, false, wManager.Wow.Helpers.FightClassCreator.YesNoAuto.Auto, "", "player", true, true, false),
                             new SpellState("Rejuvenation", 9, context => (wManager.Wow.Helpers.Party.GetPartyHomeAndInstance().OrderBy(p => p.HealthPercent).FirstOrDefault(p => p != null && p.IsValid && !p.IsDead && (!false || Fight.InFight) && (!true || !TraceLine.TraceLineGo(p.Position)) && p.GetDistance < 30 && p.HealthPercent < 95 && Interact.InteractGameObject(p.GetBaseAddress, !ObjectManager.Me.GetMove))) != null, false, false, false, false, true, true, true, true, 16000, false, false, true, false, false, true, wManager.Wow.Helpers.FightClassCreator.YesNoAuto.Auto, "", "party1", true, true, false),
                             new SpellState("Rejuvenation", 8, context => (wManager.Wow.Helpers.Party.GetPartyHomeAndInstance().OrderBy(p => p.HealthPercent).FirstOrDefault(p => p != null && p.IsValid && !p.IsDead && (!false || Fight.InFight) && (!true || !TraceLine.TraceLineGo(p.Position)) && p.GetDistance < 30 && p.HealthPercent <= 95 && Interact.InteractGameObject(p.GetBaseAddress, !ObjectManager.Me.GetMove))) != null, false, false, false, false, true, true, true, true, 16000, false, false, true, false, false, false, wManager.Wow.Helpers.FightClassCreator.YesNoAuto.Auto, "", "party2", true, true, false),
                             new SpellState("Rejuvenation", 7, context => (wManager.Wow.Helpers.Party.GetPartyHomeAndInstance().OrderBy(p => p.HealthPercent).FirstOrDefault(p => p != null && p.IsValid && !p.IsDead && (!false || Fight.InFight) && (!true || !TraceLine.TraceLineGo(p.Position)) && p.HealthPercent <= 95 && p.GetDistance < 30 && Interact.InteractGameObject(p.GetBaseAddress, !ObjectManager.Me.GetMove))) != null, false, false, false, false, true, true, true, true, 16000, false, false, true, false, false, false, wManager.Wow.Helpers.FightClassCreator.YesNoAuto.Auto, "", "party3", true, true, false),
                             new SpellState("Rejuvenation", 6, context => (wManager.Wow.Helpers.Party.GetPartyHomeAndInstance().OrderBy(p => p.HealthPercent).FirstOrDefault(p => p != null && p.IsValid && !p.IsDead && (!false || Fight.InFight) && (!true || !TraceLine.TraceLineGo(p.Position)) && p.HealthPercent <= 95 && p.GetDistance < 30 && Interact.InteractGameObject(p.GetBaseAddress, !ObjectManager.Me.GetMove))) != null, false, false, false, false, true, true, true, true, 16000, false, false, true, false, false, false, wManager.Wow.Helpers.FightClassCreator.YesNoAuto.Auto, "", "party4", true, true, false),
                             new SpellState("Wild Growth", 5, context => (wManager.Wow.Helpers.Party.GetPartyHomeAndInstance().OrderBy(p => p.HealthPercent).FirstOrDefault(p => p != null && p.IsValid && !p.IsDead && (!false || Fight.InFight) && (!true || !TraceLine.TraceLineGo(p.Position)) && p.GetDistance < 30 && p.HealthPercent < 75 && Interact.InteractGameObject(p.GetBaseAddress, !ObjectManager.Me.GetMove))) != null, false, false, false, false, true, true, true, true, 0, false, false, true, false, false, false, wManager.Wow.Helpers.FightClassCreator.YesNoAuto.Auto, "", "none", true, true, false),
                             new SpellState("Swiftmend", 4, context => (wManager.Wow.Helpers.Party.GetPartyHomeAndInstance().OrderBy(p => p.HealthPercent).FirstOrDefault(p => p != null && p.IsValid && !p.IsDead && (!false || Fight.InFight) && (!true || !TraceLine.TraceLineGo(p.Position)) && p.GetDistance < 30 && p.HealthPercent < 75 && Interact.InteractGameObject(p.GetBaseAddress, !ObjectManager.Me.GetMove))) != null, false, false, false, false, true, true, true, true, 0, false, false, true, false, false, false, wManager.Wow.Helpers.FightClassCreator.YesNoAuto.Auto, "", "none", true, true, false),
                             new SpellState("Innervate", 3, context => ObjectManager.Me.ManaPercentage < 70, true, false, false, false, true, true, true, true, 0, false, false, false, false, false, false, wManager.Wow.Helpers.FightClassCreator.YesNoAuto.Auto, "", "player", true, true, false),
                             new SpellState("Rebirth", 2, context => (wManager.Wow.Helpers.Party.GetPartyHomeAndInstance().OrderBy(p => p.HealthPercent).FirstOrDefault(p => p != null && p.IsValid && !p.IsDead && (!false || Fight.InFight) && (!true || !TraceLine.TraceLineGo(p.Position)) && p.HealthPercent <= 0 && !(p.IsBoss) && Interact.InteractGameObject(p.GetBaseAddress, !ObjectManager.Me.GetMove))) != null, false, false, false, false, true, true, true, true, 600000, false, false, true, false, false, false, wManager.Wow.Helpers.FightClassCreator.YesNoAuto.Auto, "", "none", true, true, false),
                             new SpellState("Revive", 1, context => (wManager.Wow.Helpers.Party.GetPartyHomeAndInstance().OrderBy(p => p.HealthPercent).FirstOrDefault(p => p != null && p.IsValid && !p.IsDead && (!false || Fight.InFight) && (!true || !TraceLine.TraceLineGo(p.Position)) && p.HealthPercent <= 0 && Interact.InteractGameObject(p.GetBaseAddress, !ObjectManager.Me.GetMove))) != null, false, false, false, false, true, true, true, true, 0, false, false, true, false, false, false, wManager.Wow.Helpers.FightClassCreator.YesNoAuto.Auto, "", "none", true, true, false),

                        }
        };

        if (_usePet)
            _engine.States.Add(new PetManager { Priority = int.MaxValue });

        _engine.States.Sort();
        _engine.StartEngine(25, "_FightClass", true);
    }

    public void Dispose()
    {
        if (_engine != null)
        {
            _engine.StopEngine();
            _engine.States.Clear();
        }
    }

    public void ShowConfiguration()
    {
        AkurosDruidRestorationSettings.Load();
        AkurosDruidRestorationSettings.CurrentSetting.ToForm();
        AkurosDruidRestorationSettings.CurrentSetting.Save();
    }

    class PetManager : State
    {
        public override string DisplayName
        {
            get { return "Pet Manager"; }
        }
        Timer _petTimer = new Timer(-1);
        bool _petFistTime = true;
        public override bool NeedToRun
        {
            get
            {
                if (!_petTimer.IsReady)
                    return false;

                if (ObjectManager.Me.IsDeadMe || ObjectManager.Me.IsMounted || !Conditions.InGameAndConnected)
                {
                    _petFistTime = false;
                    _petTimer = new Timer(1000 * 3);
                    return false;
                }
                if (!ObjectManager.Pet.IsValid || ObjectManager.Pet.IsDead) {
                    if (_petFistTime) { return true; }
                    else { _petFistTime = true; }
                }
                return false;
            }
        }

        private readonly Spell _revivePet = new Spell("Revive Pet");
        private readonly Spell _callPet = new Spell("Call Pet 1");

        public override void Run()
        {
            if (!ObjectManager.Pet.IsValid)
            {
                _callPet.Launch(true);
                Thread.Sleep(Usefuls.Latency + 1000);
            }
            if (!ObjectManager.Pet.IsValid || ObjectManager.Pet.IsDead)
                _revivePet.Launch(true);

            _petTimer = new Timer(1000 * 2);
        }
    }



}

/*
 * SETTINGS
*/

[Serializable]
public class AkurosDruidRestorationSettings : Settings
{



    private AkurosDruidRestorationSettings()
    {
        ConfigWinForm(new System.Drawing.Point(400, 400), "AkurosDruidRestorationSettings " + Translate.Get("Settings"));
    }

    public static AkurosDruidRestorationSettings CurrentSetting { get; set; }

    public bool Save()
    {
        try
        {
            return Save(AdviserFilePathAndName("CustomClass-AkurosDruidRestorationSettings", ObjectManager.Me.Name + "." + Usefuls.RealmName));
        }
        catch (Exception e)
        {
            Logging.WriteError("AkurosDruidRestorationSettings > Save(): " + e);
            return false;
        }
    }

    public static bool Load()
    {
        try
        {
            if (File.Exists(AdviserFilePathAndName("CustomClass-AkurosDruidRestorationSettings", ObjectManager.Me.Name + "." + Usefuls.RealmName)))
            {
                CurrentSetting =
                    Load<AkurosDruidRestorationSettings>(AdviserFilePathAndName("CustomClass-AkurosDruidRestorationSettings",
                                                                 ObjectManager.Me.Name + "." + Usefuls.RealmName));
                return true;
            }
            CurrentSetting = new AkurosDruidRestorationSettings();
        }
        catch (Exception e)
        {
            Logging.WriteError("AkurosDruidRestorationSettings > Load(): " + e);
        }
        return false;
    }
}