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

public class AzsunaHelper
{
	public AzsunaHelper()
	{
	}
	static void Log(string text)
	{
		Logging.WriteDebug("[Azsuna Helper] " + text);
	}
	public static void StartFix()
	{
		StopFix();
		robotManager.Events.ProductEvents.OnProductStopping += OnProductStop;
		wManager.Events.MovementEvents.OnMovementPulse += Subzone.CrumbledPalace.OnMovementPulse;
		Log("movement fix started");
	}
	public static void StopFix()
	{
		robotManager.Events.ProductEvents.OnProductStopping -= OnProductStop;
		wManager.Events.MovementEvents.OnMovementPulse -= Subzone.CrumbledPalace.OnMovementPulse;
		Log("movement fix stopped");
	}
	static void OnProductStop(string productName)
	{
		StopFix();
	}
	public static class Subzone
	{
		public static Vector3 Center = new Vector3(-10.85069, 6734.116, 55.58819, "None");
		public static Vector3 Outside = new Vector3(36.98286, 6738.441, 50.54235, "None");
		public static Vector3 Inside = new Vector3(-4.604612, 6733.957, 55.58764, "None");
		public static float RadiusInner = 10f;
		public static float Radius = 40;

		//prince farondis palace
		public static class CrumbledPalace
		{
			public static bool IsInside(Vector3 p)
			{
				return Center.DistanceTo2D(p) < Radius;
			}
			internal static void OnMovementPulse(List<Vector3> points, System.ComponentModel.CancelEventArgs cancelable)
			{
				var pointsCount = points.Count;
				if (pointsCount < 1)
					return;

				var end = points[pointsCount - 1];
				var myPos = ObjectManager.Me.Position;
				if (IsInside(end) && !IsInside(myPos))
				{
					var success = false;
					var path = PathFinder.FindPath(Outside, out success);
					if (success)
					{
						points.Clear();
						points.AddRange(path);
						points.Add(Inside);
						Log("Crumbled Palace path fixed from outside");
					}
				}
				else if (!IsInside(end) && IsInside(myPos))
				{
					points.Clear();
					points.Add(Center);
					points.Add(Inside);
					points.Add(Outside);
					Log("Crumbled Palace path fixed to outside");
				}
			}
		}
	}
}