#if VISUAL_STUDIO
using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using wManager.Wow.Bot.Tasks;
using wManager.Wow.Class;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using wManager.Wow.Enums;
#endif

public class ArgentHelper
{
	static ArgentHelper()
	{
		Log("started");
	}
	static void Log(string text)
	{
		Logging.Write("[Argent Helper] " + text);
	}
	

}
