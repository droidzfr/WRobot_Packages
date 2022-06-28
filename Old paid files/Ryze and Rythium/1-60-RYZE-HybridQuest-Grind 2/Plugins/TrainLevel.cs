using System;
using System.Threading;
using robotManager.Helpful;
using robotManager.Products;
using wManager.Plugin;
using wManager.Wow.Helpers;

public class Main : IPlugin
{
    private bool _isLaunched;

    public void Initialize()
    {
        var l = new System.Collections.Generic.List<uint> { 5, 10, 15 };
        _isLaunched = true;
        Logging.Write("[TrainLevel] Started.");

        while (_isLaunched && Products.IsStarted)
        {
            try
            {
                if (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause)
                {
                    wManager.wManagerSetting.CurrentSetting.TrainNewSkills = l.Contains(wManager.Wow.ObjectManager.ObjectManager.Me.Level);
                }
            }
            catch (Exception e)
            {
                Logging.WriteError("[TrainLevel]: " + e);
            }
            Thread.Sleep(550);
        }
    }

    public void Dispose()
    {
        _isLaunched = false;
        Logging.Write("[TrainLevel] Stoped.");
    }

    public void Settings()
    {
        Logging.Write("[TrainLevel] No setting.");
    }
}