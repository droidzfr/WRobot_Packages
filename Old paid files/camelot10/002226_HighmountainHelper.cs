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

public class HighmountainHelper
{
	static HighmountainHelper()
	{
		ResetSettings();
	}
	public static void ResetSettings()
	{
		Log("reset settings");
	}
	public static class Subzone
	{
		static Vector3 RessurectCaveCenter = new Vector3(3987.755, 4632.434, 636.0258, "None"); //radius 25
		static Vector3 RessurectCaveInside = new Vector3(3988.254, 4656.405, 641.8357, "None");
		static Vector3 RessurectCaveOutside = new Vector3(3988.474, 4675.718, 640.6574, "None");
		static Vector3 ThundertotemCenter = new Vector3(4083.05, 4385.5, 670.6265, "None"); //radius 90
		static Vector3 ThundertotemInsdide = new Vector3(4115.426, 4460.921, 661.0004, "None");
		static Vector3 ThundertotemOutside = new Vector3(4126.301, 4479.39, 660.0369, "None");

		//continent, center, radius, height, inside, outside
		static List<System.Tuple<ContinentId, string, Vector3, float, float, Vector3, Vector3>> subzones = new List<System.Tuple<ContinentId, string, Vector3, float, float, Vector3, Vector3>>()
		{
			new System.Tuple<ContinentId, string, Vector3, float, float, Vector3, Vector3>(
				ContinentId.Troll_Raid, "Ressurect Cave", RessurectCaveCenter, 25, 25, RessurectCaveInside, RessurectCaveOutside
			),
			/*
			new System.Tuple<ContinentId, string, Vector3, float, float, Vector3, Vector3>(
				ContinentId.Troll_Raid, "Thundertotem 1st floor", ThundertotemCenter, 90, 50, ThundertotemInsdide, ThundertotemOutside
			),
			//*/
		};
		public static void StartFix()
		{
			StopFix();
			robotManager.Events.ProductEvents.OnProductStopping += OnProductStop;
			wManager.Events.MovementEvents.OnMovementPulse += OnMovementPulse;
			Log("nav fix start");
		}
		public static void StopFix()
		{
			robotManager.Events.ProductEvents.OnProductStopping -= OnProductStop;
			wManager.Events.MovementEvents.OnMovementPulse -= OnMovementPulse;
			Log("nav fix stop");
		}
		static void OnMovementPulse(List<Vector3> points, System.ComponentModel.CancelEventArgs cancelable)
		{
			if (points.Count < 1)
				return;

			var myPos = ObjectManager.Me.Position;
			var end = points[points.Count - 1];
			var continent = (ContinentId)Usefuls.ContinentId;

			foreach (var subzone in subzones)
			{
				if (subzone.Item1 != continent)
					continue;

				var name = subzone.Item2;
				var center = subzone.Item3;
				var radius = subzone.Item4;
				var height = subzone.Item5;
				var inside = subzone.Item6;
				var outside = subzone.Item7;
				var endIsInside = center.DistanceTo2D(end) < radius && center.DistanceZ(end) < height;
				var meIsInside = center.DistanceTo2D(myPos) < radius && center.DistanceZ(myPos) < height;
				if (endIsInside && !meIsInside)
				{
					var path = PathFinder.FindPath(inside);
					points.Clear();
					points.AddRange(path);
					Log(name + " path fixed from outside");
					return;
				}
				else if (meIsInside && !endIsInside)
				{
					var path = PathFinder.FindPath(outside);
					points.Clear();
					points.AddRange(path);
					Log(name + " path fixed to outside");
					return;
				}
			}
		}
		static void OnProductStop(string productName)
		{
			StopFix();
		}
	}
	static void Log(string text)
	{
		Logging.WriteDebug("[Highmountain Helper] " + text);
	}
}