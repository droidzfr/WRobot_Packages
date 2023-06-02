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

public class Instancer
{
	static Instancer()
	{
	}

	//796 - black temple
	//789 - sunwell
	static List<string> _cache = new List<string>();
	static List<MapInfo> _maps = new List<MapInfo>();
	static robotManager.Helpful.Timer _timer = new robotManager.Helpful.Timer();

	public static bool Found(int mobId, float distance = 150)
	{
		var mobEntry = ObjectManager.GetNearestWoWUnit(ObjectManager.GetWoWUnitByEntry(mobId));
		if (mobEntry != null && mobEntry.IsAlive && mobEntry.IsValid)
		{
			if (mobEntry.Position.DistanceTo2D(ObjectManager.Me.Position) < distance)
			{
				return true;
			}
		}
		return false;
	}

	public static bool Alive(int instanceMapId, int bossNum)
	{
		return !Killed(instanceMapId, bossNum);
	}

	public static bool Killed(int instanceMapId, int bossNum)
	{
		UpdateInstanceKills();
		var mapName = GetMapName(instanceMapId);
		if (string.IsNullOrEmpty(mapName))
			return false;

		var bossName = mapName + bossNum;
		if (_cache.Contains(bossName))
			return true;

		return false;
	}

	static string GetMapName(int mapId)
	{
		foreach (var map in _maps)
		{
			if (map.id == mapId)
				return map.name;
		}
		var mapName = FindMapName(mapId);
		if (string.IsNullOrEmpty(mapName))
			return "";

		_maps.Add( new MapInfo(mapId, mapName) );
		return mapName;
	}

	static string FindMapName(int mapId)
	{
		var luaCode = @"
local mapname = GetMapNameByID({0});
if (not mapname) then
	return '';
end
return mapname;
";
		string toRun = string.Format(luaCode, mapId);
		string mapName = Lua.LuaDoString<string>(toRun);
		Log("find map name for mapId=" + mapId + " result=" + mapName);
		return mapName;
	}

	static void UpdateInstanceKills()
	{
		if (!_timer.IsReady)
			return;

		_timer.Reset(30 * 1000);
		_cache = new List<string>();

		var luaCode = @"
RequestRaidInfo()
local killList = '';
local n = GetNumSavedInstances()
for k=1,n do
	local name, id, reset, difficulty, locked, extended, instanceIDMostSig, isRaid, maxPlayers, difficultyName, numEncounters, encounterProgress = GetSavedInstanceInfo(k)
    if (locked and encounterProgress > 0) then
		for i = 1, numEncounters do
			local bossName, text, isKilled = GetSavedInstanceEncounterInfo(k, i)
			if (isKilled) then
				killList = killList .. name .. i .. '#LUASEPARATOR#';
			end
		end
	end
end
return killList;
		";

		string toRun = string.Format(luaCode.Replace("#LUASEPARATOR#", Lua.ListSeparator));
		var killList = Lua.LuaDoString<List<string>>(toRun);
		_cache = killList;
		_cache.RemoveAll(str => string.IsNullOrEmpty(str));
		Log("kill list updated. count=" + _cache.Count + " " + string.Join(",", _cache));
	}

	static void Log(string message)
	{
		Logging.WriteDebug("[Instancer] " + message);
	}

	static void InstancerError(string message)
	{
		Logging.WriteError("[Instancer] " + message);
	}

	public class MapInfo
	{
		public string name;
		public int id;

		public MapInfo(int mapId, string mapName)
		{
			id = mapId;
			name = mapName;
		}

	}

}