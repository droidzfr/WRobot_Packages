using robotManager.Helpful;
using System.Threading;
using robotManager.Products;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Configuration;
using System.ComponentModel;
using wManager;

public class Main : wManager.Plugin.IPlugin
{

    // see if nodes near me: https://wrobot.eu/forums/topic/5173-node-search-radius/?tab=comments#comment-23877
    private bool _isLaunched;
    private Random r;
    private int stayInPartyTime;
    private DateTime leavePartyTime;
    private List<string> myNodeList;
    private List<string> myEnemyList;
    private const int _LOOTINGENEMYDISTANCE = 15;

    public void Initialize()
    {
        Logging.Write("[Server Hopper] Started.");
        _isLaunched = true;
        r = new Random();


        resetLeavePartyWaitTime();
        initializeNodes();
        initializeEnemies();

        //if (!checkBetaExpired())
       // {
            doStuffLoop();
       // }

    }

    private void initializeNodes()
    {
        if (_settings._checkForSpecificNode)
        {
            string[] nodeSplit = _settings._checkForSpecificNodeName.Split(',');
            myNodeList = new List<string>(nodeSplit);
        }
    }
    private void initializeEnemies()
    {
        if (_settings._checkForSpecificEnemy)
        {
            string[] enemySplit = _settings._checkForSpecificEnemyName.Split(',');
            myEnemyList = new List<string>(enemySplit);
        }
    }

    public void Dispose()
    {
        _isLaunched = false;
        resetLeavePartyWaitTime();
        myNodeList = null;
        myEnemyList = null;
        Logging.Write("[Server Hopper] Disposed.");
    }
    public void Settings()
    {
        _settings.ToForm();
        _settings.Save();
    }
    private void testing()
    {


        Logging.Write("[Server Hopper] Starting Testing Loop.");
        while (Products.IsStarted && _isLaunched)
        {

            if (!Products.InPause)
            {
                Logging.Write("Force Skip In Out Doors: " + wManager.wManagerSetting.CurrentSetting.SkipInOutDoors);
            }
            Thread.Sleep(1000);
        }
    }

    public void doStuffLoop()
    {

        Logging.Write("[Server Hopper] Starting Server Hopper Loop.");
        while (Products.IsStarted && _isLaunched)
        {

            if (!Products.InPause)
            {
                //lootableEnemiesWithinRangeLogging();

                if (Party.IsInGroup() == false)
                {
                    Logging.Write("[Server Hopper] Executing 'Not In Group' steps.");
                    partySteps();
                }
                else if (Party.IsInGroup() && !Party.CurrentPlayerIsLeader())
                {
                    if (stayInPartyTime == 0)
                    {
                        Logging.Write("[Server Hopper] Executing 'In Group Not Leader' steps.");
                        setupPartyWaitTime();
                    }
                    else if (partyWaitTimeExpired() && canLeaveParty(true))
                    {
                            leaveParty();
                            partySteps();
                    }
                }
                else if (Party.IsInGroup() && Party.CurrentPlayerIsLeader())
                {
                    Logging.Write("[Server Hopper] Executing 'Group Leader' steps.");
                    if (canLeaveParty(true))
                    {
                        leaveParty();
                        partySteps();
                    }
                }

                Thread.Sleep(1000);
            }
        }

    }
    private bool checkBetaExpired()
    {
        DateTime betaExpiredTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Convert.ToDouble(1504051200));
        Logging.Write("Beta Expired time: " + betaExpiredTime);
        return DateTime.UtcNow > betaExpiredTime;

    }

    public void partySteps()
    {
        resetLeavePartyWaitTime();
        applyToGroup();
        joinLFGPartyInvite();

    }
    private void leaveParty()
    {

        var tmp = calculateRandomByRange(1000, 3000);
        Logging.Write("[Server Hopper] Attempting to leave Party in " + tmp + "ms.");
        Thread.Sleep(tmp);

        Logging.Write("[Server Hopper] Checking one more time to see if it is safe to leave party.");
        if (canLeaveParty(_settings._loggingEnabled))
        {
            resetLeavePartyWaitTime();
            Logging.Write("[Server Hopper] Leaving Party.");
            /* for 1.12 testing */
            wManager.Wow.Helpers.Lua.RunMacroText("/run LeaveParty()");
        }
        else
        {
            Logging.Write("[Server Hopper] Cannot Leave Party.");
        }

    }
    private void joinLFGPartyInvite()
    {

        int joinWaitTime = calculateRandomByRange(500, 1000);
        Logging.Write("[Server Hopper] waiting " + joinWaitTime + "ms before joining group.");
        Thread.Sleep(joinWaitTime);

        Logging.Write("[Server Hopper] Joining LFG Party.");

        /* for 1.12 testing */
        wManager.Wow.Helpers.Lua.RunMacroText("/script LFGListInviteDialog.AcceptButton:Click()");

    }
    private void applyToGroup()
    {
        Logging.Write("[Server Hopper] Applying to group.");
        // instead of wManager.Wow.Helpers.Lua.RunMacroText("/run ServerHop_HopForward()");


        /* for 1.12 testing */
        var initiatePluginHopScript = "/run ServerHop_HopForward()";
        var randomMacroName = Others.GetRandomString(7);
        string _keyBindingMacro = "J";

        var macroId = Lua.LuaDoString<int>("return CreateMacro('" + randomMacroName + "', 'INV_Misc_QuestionMark', '" + initiatePluginHopScript + "', 1, nil);");
        if (macroId >= 1)
        {
            if (Lua.LuaDoString<bool>("return SetBindingMacro('" + _keyBindingMacro + "', " + macroId + ");"))
            {
                Keyboard.PressKey(wManager.Wow.Memory.WowMemory.Memory.WindowHandle, _keyBindingMacro);
            }
            Lua.LuaDoString<int>("return DeleteMacro(" + macroId + ");");
        }


    }
    private void finishLooting()
    {
        if (deadLootableEnemiesWithinRange())
        {
            /**
            int sleepTime = GetLootableMobsNearMe(_LOOTINGENEMYDISTANCE).Count * _settings._lootingWaitTimePerEnemy;

            if (sleepTime > _settings._maxLootingWaitTime)
            {
                sleepTime = _settings._maxLootingWaitTime;
            }
            int waitTime = GetLootableMobsNearMe(_LOOTINGENEMYDISTANCE).Count * _settings._lootingWaitTimePerEnemy * 1000;

            if (_settings._loggingEnabled)
            {
                lootableEnemiesWithinRangeLogging();
            }
            Thread.Sleep(waitTime);
            **/
        }
    }

    private void setupPartyWaitTime()
    {
        int minRange = _settings._waitTimeBeforeLeavingParty * 1000;
        int upperRange = minRange + (int)(minRange * .2);
        stayInPartyTime = calculateRandomByRange(minRange, upperRange);
        Logging.Write("[Server Hopper] Waiting " + stayInPartyTime + "ms before leaving group (unless we are removed from group or become leader).");
        leavePartyTime = DateTime.Now.AddMilliseconds(stayInPartyTime);
    }

    private void resetLeavePartyWaitTime()
    {
        stayInPartyTime = 0;
        leavePartyTime = default(DateTime);
    }
    // returns true if the time to wait before leaving the party has expired.
    private bool partyWaitTimeExpired()
    {
        return (DateTime.Now > leavePartyTime && (leavePartyTime != default(DateTime)));
    }

    private void watchForInvite()
    {

        EventsLuaWithArgs.OnEventsLuaWithArgs += (LuaEventsId id, List<string> args) =>
        {
            if (id == wManager.Wow.Enums.LuaEventsId.PARTY_INVITE_REQUEST)
            {
                for (int i = 0; i < args.Count; i++)
                {
                    Logging.Write("Arg" + (i + 1) + " contains: " + args[i]);
                }

            }
        };

    }

    private bool canLeaveParty(bool includeLogging)
    {
        if (includeLogging && _settings._loggingEnabled)
        {
            inCombatLogging();
            nodesWithinRangeLogging();
            enemiesWithinRangeLogging();
            lootableEnemiesWithinRangeLogging();
        }

        return (nodesWithinRange() == false && enemiesWithinRange() == false && deadLootableEnemiesWithinRange() == false && notInCombat());
    }

    private bool notInCombat()
    {
        return (!ObjectManager.Me.InCombat && !ObjectManager.Me.IsCast && !ObjectManager.Me.IsLooting());
    }
    private bool nodesWithinRange()
    {

        if (_settings._checkForNodesBeforeLeaving == false)
        {
            return false;
        }
        else
        {
            if (_settings._checkForSpecificNode)
            {

                return GetNodesNearMe(_settings._checkForNodeRangeRadius, myNodeList).Count > 0;
            }
            else
            {
                return GetNodesNearMe(_settings._checkForNodeRangeRadius).Count > 0;
            }
        }

    }
    private bool enemiesWithinRange()
    {
        if (_settings._checkForEnemiesBeforeLeaving == false)
        {
            return false;
        }
        else
        {
            if (_settings._checkForSpecificEnemy)
            {
                return GetEnemiesNearMe(_settings._checkForEnemyRangeRadius, myEnemyList).Count > 0;
            }
            else
            {
                return GetEnemiesNearMe(_settings._checkForEnemyRangeRadius).Count > 0;
            }
        }
    }
    private bool deadLootableEnemiesWithinRange()
    {

        if (_settings._checkForLootables == false)
        {
            return false;
        }
        else
        {
            if (GetLootableMobsNearMe(_settings._checkForLootablesRangeRadius).Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }

    private void inCombatLogging()
    {

        if (notInCombat() == false)
        {
            if (ObjectManager.Me.InCombat)
            {
                Logging.Write("[Server Hopper] Waiting to leave combat before running next step.");
            }
            else if (ObjectManager.Me.IsCast)
            {
                Logging.Write("[Server Hopper] Waiting to stop casting before running next step.");
            }
            else if (ObjectManager.Me.IsLooting())
            {
                Logging.Write("[Server Hopper] Waiting to finish looting before running next step.");
            }
        }

    }
    private void nodesWithinRangeLogging()
    {

        if (nodesWithinRange())
        {
            Logging.Write("[Server Hopper] Nodes are within range. Waiting before running next step.");
        }
        else
        {
            if (_settings._checkForNodesBeforeLeaving == true)
                Logging.Write("[Server Hopper] No Nodes are within range.");
            else
                Logging.Write("[Server Hopper] Checking for Nodes is disabled.");
        }
    }
    private void enemiesWithinRangeLogging()
    {

        if (enemiesWithinRange())
        {
            Logging.Write("[Server Hopper] Enemies are within range. Waiting before running next step.");
        }
        else
        {
            if (_settings._checkForEnemiesBeforeLeaving == true)
                Logging.Write("[Server Hopper] No Enemies are within range.");
            else
                Logging.Write("[Server Hopper] Checking for Enemies is disabled.");
        }
    }
    private void lootableEnemiesWithinRangeLogging() {
        if (deadLootableEnemiesWithinRange())
        {
            Logging.Write("[Server Hopper] Lootable Enemies are within range. Finishing Looting before running next step.");
        }
        else
        {
            if (_settings._checkForLootables == true)
                Logging.Write("[Server Hopper] No Lootable Enemies are within range.");
            else
                Logging.Write("[Server Hopper] Checking for Lootable Enemies is disabled.");
        }

    }

    private int calculateRandomByRange(int lower, int upper)
    {
        return (r.Next(lower, upper));
    }
    private List<WoWGameObject> GetNodesNearMe(int range)
    {
        List<WoWGameObject> nodesNearMe;

        if (wManager.wManagerSetting.CurrentSetting.SkipInOutDoors)
        {
            nodesNearMe = ObjectManager.GetObjectWoWGameObject().FindAll(p => p.GetDistance <= range && p.CanOpen && p.IsOutdoors);
        }
        else
        {
            nodesNearMe = ObjectManager.GetObjectWoWGameObject().FindAll(p => p.GetDistance <= range && p.CanOpen);
        }

        return nodesNearMe;
    }
    private List<WoWGameObject> GetNodesNearMe(int range, List<string> nodeName)
    {
        List<WoWGameObject> nodesNearMe;
        if (wManager.wManagerSetting.CurrentSetting.SkipInOutDoors)
        {
            nodesNearMe = ObjectManager.GetObjectWoWGameObject().FindAll(p => p.GetDistance <= range && p.CanOpen && nodeName.Contains(p.Name) && p.IsOutdoors);
        }
        else
        {
            nodesNearMe = ObjectManager.GetObjectWoWGameObject().FindAll(p => p.GetDistance <= range && p.CanOpen && nodeName.Contains(p.Name));
        }
        return nodesNearMe;
        //return ObjectManager.GetObjectWoWGameObject().FindAll(p => p.GetDistance <= 100f && p.CanOpen && p.Name == nodeName);
    }
    private List<WoWUnit> GetEnemiesNearMe(int range)
    {
        List<WoWUnit> enemiesNearMe = ObjectManager.GetWoWUnitHostile().FindAll(p => p.GetDistance <= range);
        return enemiesNearMe;
    }
    private List<WoWUnit> GetEnemiesNearMe(int range, List<string> enemyName)
    {
        List<WoWUnit> enemiesNearMe = ObjectManager.GetWoWUnitHostile().FindAll(p => p.GetDistance <= range && enemyName.Contains(p.Name));
        return enemiesNearMe;
        //return ObjectManager.GetWoWUnitHostile().FindAll(p => p.GetDistance <= 30 && p.Name == "");
    }
    private List<WoWUnit> GetLootableMobsNearMe(int range)
    {
        //https://wrobot.eu/forums/topic/5754-manual-looting-of-bodies/?tab=comments#comment-26198
        bool skinMobs =  wManager.wManagerSetting.CurrentSetting.SkinMobs;
        
        var lootablesNearMe = wManager.Wow.ObjectManager.ObjectManager.GetObjectWoWUnit().FindAll(p => p != null && p.IsValid && p.GetDistance <= range && (p.IsLootable || (p.Skinnable && skinMobs )));
        return lootablesNearMe;
    }

    private List<WoWUnit> GetElitesNearMe(int range)
    {

        return ObjectManager.GetWoWUnitHostile().FindAll(p => p.GetDistance <= range && p.IsElite);
    }
    private List<WoWUnit> GetElitesNearMe(int range, List<string> eliteName)
    {

        return ObjectManager.GetWoWUnitHostile().FindAll(p => p.GetDistance <= range && p.IsElite && eliteName.Contains(p.Name));
        //return ObjectManager.GetWoWUnitHostile().FindAll(p => p.GetDistance <= range && p.IsElite && p.Name == "");
    }


    private pluginSettings _settings
    {
        get
        {
            try
            {
                if (pluginSettings.CurrentSetting == null)
                    pluginSettings.Load();
                return pluginSettings.CurrentSetting;
            }
            catch
            {
            }
            return new pluginSettings();
        }
        set
        {
            try
            {
                pluginSettings.CurrentSetting = value;
            }
            catch
            {
            }
        }
    }
    [Serializable]
    public class pluginSettings : Settings
    {
        public pluginSettings()
        {
            _loggingEnabled = true;
            _waitTimeBeforeLeavingParty = 15;

            _checkForNodesBeforeLeaving = false;
            _checkForNodeRangeRadius = 300;
            _checkForSpecificNode = false;
            _checkForSpecificNodeName = "";

            _checkForEnemiesBeforeLeaving = false;
            _checkForEnemyRangeRadius = 300;
            _checkForSpecificEnemy = false;
            _checkForSpecificEnemyName = "";


            _checkForLootables = true;
            _checkForLootablesRangeRadius = 20;
            //_lootingWaitTimePerEnemy = 3;
            //_maxLootingWaitTime = 30;

        }

        [Setting]
        [Category("General")]
        [DisplayName("Time before leaving party")]
        [Description("The time the plugin will wait before leaving the party after joining (in seconds). A range of 20% will be added for a range. For example, if 10 seconds is set, then the bot will pick a random number between 10 and 12 seconds.")]
        public int _waitTimeBeforeLeavingParty { get; set; }

        [Setting]
        [Category("General")]
        [DisplayName("Enable Logging")]
        [Description("Enabling this will force [Server Hopper] to have more verbose log. This is useful when troubleshooting, or you just like to see what it's doing :)")]
        public bool _loggingEnabled { get; set; }

        [Setting]
        [Category("Node Party Settings")]
        [DisplayName("Check for Nodes before leaving party.")]
        [Description("Check for gathering nodes (Herbs, Ores). The node must be within player's skill level.")]
        public bool _checkForNodesBeforeLeaving { get; set; }

        [Setting]
        [Category("Node Party Settings")]
        [DisplayName("Node Range Scan")]
        [Description("Check for gathering nodes within this range. The node must be within player's skill level. Default range is 100")]
        public int _checkForNodeRangeRadius { get; set; }

        [Setting]
        [Category("Node Party Settings")]
        [DisplayName("Check for specific Node")]
        [Description("Check for a specific node by name.  Set to True if wish to only collect certain Nodes. Check for Nodes before leaving party must also be enabled.")]
        public bool _checkForSpecificNode { get; set; }

        [Setting]
        [Category("Node Party Settings")]
        [DisplayName("Check specific Node Name")]
        [Description("Check for a specific node by name. For multiple nodes, use a comma seperated list (Do not include spaces). Check for specific Node setting must be enabled. ")]
        public string _checkForSpecificNodeName { get; set; }

        [Setting]
        [Category("PvE Enemy Party Settings")]
        [DisplayName("Check for Enemies before leaving party.")]
        [Description("Check for PvE enemies before leaving party. Will not leave party if an enemy is near.")]
        public bool _checkForEnemiesBeforeLeaving { get; set; }

        [Setting]
        [Category("PvE Enemy Party Settings")]
        [DisplayName("Enemy Range Scan")]
        [Description("Check for Enemies within this range. Default range is 100")]
        public int _checkForEnemyRangeRadius { get; set; }

        [Setting]
        [Category("PvE Enemy Party Settings")]
        [DisplayName("Check for specific Enemy")]
        [Description("Check for a specific Enemy. Set to True if wish to only kill certain Enemy. Check for Enemies before leaving party must also be enabled.")]
        public bool _checkForSpecificEnemy { get; set; }

        [Setting]
        [Category("PvE Enemy Party Settings")]
        [DisplayName("Check for specific Enemy Name")]
        [Description("Check for specific Enemy by name. For multiple enemies, use a comma seperated list (Do not include spaces). This can be used in either the Elite Name or Normal Enemy Name. Check for specific Enemy must be Enabled.")]
        public string _checkForSpecificEnemyName { get; set; }

        [Category("PvE Enemy Loot Settings")]
        [DisplayName("Check for Loot")]
        [Description("This will allow the plugin to wait after killing mobs before leaving group so that the player can skin/loot. ")]
        public bool _checkForLootables { get; set; }

        [Category("PvE Enemy Loot Settings")]
        [DisplayName("Loot Range Scan")]
        [Description("Check for lootable/skinnable Enemies within this range. Default range is 20")]
        public int _checkForLootablesRangeRadius { get; set; }

        //[Category("PvE Enemy Loot Settings")]
        //[DisplayName("Looting Wait Per Enemy")]
        //[Description("For each enemy killed to be skinned/looted this much time before leaving party (in addition to normal wait time). ")]
        //public int _lootingWaitTimePerEnemy { get; set; }

        //[Category("PvE Enemy Loot Settings")]
        //[DisplayName("Max Looting Wait Time")]
        //[Description("This is to prevent the bot from waiting too long to finish looting/skinning (in case something goes wrong).")]
        //public int _maxLootingWaitTime { get; set; }


        public static pluginSettings CurrentSetting { get; set; }

        public bool Save()
        {
            try
            {
                return Save(AdviserFilePathAndName("ServerHopper", ObjectManager.Me.Name + "." + Usefuls.RealmName));
            }
            catch (Exception e)
            {
                Logging.WriteError("ServerHopper > Save(): " + e);
                return false;
            }
        }

        public static bool Load()
        {
            try
            {
                if (File.Exists(AdviserFilePathAndName("ServerHopper", ObjectManager.Me.Name + "." + Usefuls.RealmName)))
                {
                    CurrentSetting =
                        Load<pluginSettings>(AdviserFilePathAndName("ServerHopper",
                                                                      ObjectManager.Me.Name + "." + Usefuls.RealmName));
                    return true;
                }
                CurrentSetting = new pluginSettings();
            }
            catch (Exception e)
            {
                Logging.WriteError("ServerHopper > Load(): " + e);
            }
            return false;
        }
    }


}
