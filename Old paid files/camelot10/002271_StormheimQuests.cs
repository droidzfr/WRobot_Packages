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

public class StormheimQuests
{
	public StormheimQuests()
	{
		ChangeSettings();
		//StartRadar();
	}
	static void StartRadar()
	{
		StopRadar();
		//Radar3D.OnDrawEvent += OnDrawRadar;
		//robotManager.Events.ProductEvents.OnProductStopping += StopRadar;
	}
	static void StopRadar(string arg = "")
	{
		Radar3D.OnDrawEvent -= OnDrawRadar;
		robotManager.Events.ProductEvents.OnProductStopping -= StopRadar;
	}
	static void OnDrawRadar()
	{
		var u = Subzone.ThorimsPeak.BottomCorner;
		var v = Subzone.ThorimsPeak.UpperCorner;
		List<float> listX = new List<float>() { u.X, v.X };
		List<float> listY = new List<float>() { u.Y, v.Y };
		List<float> listZ = new List<float>() { u.Z, v.Z };
		foreach (var x1 in listX)
		{
			foreach (var y1 in listY)
			{
				foreach (var z1 in listZ)
				{
					foreach (var x2 in listX)
					{
						foreach (var y2 in listY)
						{
							foreach (var z2 in listZ)
							{
								var a = new Vector3(x1, y1, z1);
								var b = new Vector3(x2, y2, z2);
								Radar3D.DrawLine(a, b, System.Drawing.Color.LightYellow);
							}
						}
					}
				}
			}
		}
	}
	public static bool NeedHellheim
	{
		get
		{
			return Questing.NotCompleteAll(39855, 39853);
		}
	}
	public static void StartHellheimHelper()
	{
		/* Bolstered Spirits */
		Thread t = new Thread(() =>
		{
			while (robotManager.Products.Products.IsStarted)
			{
				if (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause)
				{
					if (!Subzone.Hellheim.Inside)
					{
						Log("stop hellheim helper");
						break;
					}
					var mob = Questing.FindUnit(93130);
					if (mob != null && mob.IsValid && mob.GetDistance <= 10)
					{
						Log("grab buff. interact " + mob.Name);
						GoToTask.ToPositionAndIntecractWithNpc(mob.Position, mob.Entry);
					}
				}
				Thread.Sleep(1000);
			}
			Log("ended hellheim helper");
		});
		t.Start();
		Log("start hellheim helper");
	}

	#region HARPOON
	public class Harpoon
	{
		const uint harpoonId = 138111;
		const int harpoonQuestId = 39775;
		public const float HookMaxDist = 55;//40;
		const float hookMinDist = 5;
		static readonly List<int> hooksPointsId = new List<int>() { 91975, 91983, 92072, 92017, };
		public static bool Can { get { return Quest.GetQuestCompleted(harpoonQuestId) || ItemsManager.HasItemById(harpoonId); } }
		public static bool UseNear(float radius = HookMaxDist)
		{
			if (!Can)
				return false;

			return UseNear(ObjectManager.Me.Position, radius);
		}
		public static bool UseNear(Vector3 position, float radius = hookMinDist)
		{
			if (!Can)
				return false;

			var hookPoint = GetNear(position, radius);
			if (hookPoint != null && hookPoint.IsValid)
			{
				var hookDist = ObjectManager.Me.Position.DistanceTo(hookPoint.Position);
				Log("harpoon point=" + hookPoint.Name + " can=" + (hookDist < HookMaxDist) + " dist=" + hookDist);
				if (hookDist < HookMaxDist)
				{
					//dismount needed
					MountTask.DismountMount();
					Interact.InteractGameObject(hookPoint.GetBaseAddress);
					Thread.Sleep(10 * 1000);
					return true;
				}
			}
			return false;
		}
		public static WoWUnit GetNear(float radius = HookMaxDist)
		{
			return GetNear(ObjectManager.Me.Position, radius);
		}
		public static WoWUnit GetNear(Vector3 position, float radius = hookMinDist)
		{
			var hookPoints = ObjectManager.GetWoWUnitByEntry(hooksPointsId);
			hookPoints.Sort((a, b) =>
			{
				if (a.Position.DistanceTo(position) > b.Position.DistanceTo(position))
					return 1;
				else
					return -1;
			});
			foreach (var hookPoint in hookPoints)
			{
				if (hookPoint != null && hookPoint.IsValid && position.DistanceTo(hookPoint.Position) < radius)
				{
					return hookPoint;
				}
			}
			return null;
		}
	}
	#endregion HARPOON
	public static class Subzone
	{
		public static class SkyfireStormwind
		{
			static string Name = "Skyfire (Stormwind)";

			public static void StartMoveFix()
			{
				StopMoveFix();
				robotManager.Events.ProductEvents.OnProductStopping += OnProductStopping;
				wManager.Events.MovementEvents.OnMovementPulse += OnMovementPulse;
				Log(Name + " move fix start");
			}
			static void OnProductStopping(string productName)
			{
				StopMoveFix();
			}
			public static void StopMoveFix()
			{
				robotManager.Events.ProductEvents.OnProductStopping -= OnProductStopping;
				wManager.Events.MovementEvents.OnMovementPulse -= OnMovementPulse;
				Log(Name + " move fix stop");
			}
			static void OnMovementPulse(List<Vector3> points, System.ComponentModel.CancelEventArgs cancelable)
			{
				var myPos = ObjectManager.Me.Position;

				if (!IsInside(myPos))
					return;

				if (points == null)
					return;

				var pointsCount = points.Count;
				if (pointsCount < 1)
					return;


				MountTask.DismountMount();
				var start = points[0];
				var end = points[points.Count - 1];
				if (myPos.DistanceTo(end) < 15 && !TraceLine.TraceLineGo(myPos, end))
				{
					Log(Name + " move direct to " + end.ToStringNewVector() + " from=" + myPos.ToStringNewVector());
					points.Clear();
					points.Add(myPos);
					points.Add(end);
					return;
				}
				var prev = points.Count;
				var path = Questing.PathClampDirected(_path, myPos, end, 5);
				if (path.Count < 2)
					return;

				points.Clear();
				points.AddRange(path);
				Log(Name + " path fix from " + prev + " to " + points.Count + ". from=" + myPos.ToStringNewVector() + " to=" + end.ToStringNewVector());
			}
			public static bool IsInside(Vector3 p)
			{
				return p.DistanceTo(Center) < 50;
			}
			public static Vector3 Center = new Vector3(-8538.036, 1400.744, 193.8927, "None");
			public static List<Vector3> _path = new List<Vector3>()
			{
				new Vector3(-8477.758f, 1401.811f, 198.232f, "None"),
				new Vector3(-8481.928f, 1401.898f, 198.2616f, "None"),
				new Vector3(-8485.744f, 1401.219f, 196.8424f, "None"),
				new Vector3(-8488.326f, 1395.802f, 196.3732f, "None"),
				new Vector3(-8490.149f, 1401.307f, 196.5009f, "None"),
				new Vector3(-8493.93f, 1402.094f, 195.2051f, "None"),
				new Vector3(-8497.928f, 1401.671f, 193.5677f, "None"),
				new Vector3(-8501.981f, 1401.617f, 193.5157f, "None"),
				new Vector3(-8506.028f, 1401.545f, 193.4984f, "None"),
				new Vector3(-8509.638f, 1400.912f, 193.6247f, "None"),
				new Vector3(-8515.559f, 1399.593f, 193.8222f, "None"),
				new Vector3(-8519.557f, 1398.901f, 194.3534f, "None"),
				new Vector3(-8523.57f, 1398.488f, 194.5863f, "None"),
				new Vector3(-8527.584f, 1398.286f, 195.7841f, "None"),
				new Vector3(-8531.572f, 1398.481f, 198.238f, "None"),
				new Vector3(-8534.021f, 1401.735f, 198.5264f, "None"),
				new Vector3(-8529.001f, 1402.722f, 196.5658f, "None"),
				new Vector3(-8524.927f, 1402.979f, 194.6469f, "None"),
				new Vector3(-8520.955f, 1403.295f, 194.5267f, "None"),
				new Vector3(-8517.377f, 1405.179f, 193.8996f, "None"),
				new Vector3(-8516.104f, 1408.802f, 193.8853f, "None"),
				new Vector3(-8518.239f, 1412.052f, 193.7525f, "None"),
				new Vector3(-8522.108f, 1413.049f, 193.5462f, "None"),
				new Vector3(-8526.075f, 1412.911f, 193.5272f, "None"),
				new Vector3(-8530.146f, 1412.707f, 193.5064f, "None"),
				new Vector3(-8534.095f, 1411.968f, 193.4999f, "None"),
				new Vector3(-8538.052f, 1411.098f, 193.8807f, "None"),
				new Vector3(-8541.832f, 1409.867f, 193.5501f, "None"),
				new Vector3(-8545.41f, 1407.997f, 193.5888f, "None"),
				new Vector3(-8547.841f, 1404.809f, 193.6006f, "None"),
				new Vector3(-8549.599f, 1401.205f, 193.6141f, "None"),
				new Vector3(-8550.829f, 1397.362f, 193.6102f, "None"),
				new Vector3(-8552.477f, 1393.676f, 193.5896f, "None"),
				new Vector3(-8555.035f, 1390.571f, 193.7456f, "None"),
				new Vector3(-8558.444f, 1388.431f, 193.5298f, "None"),
				new Vector3(-8562.361f, 1387.585f, 193.5247f, "None"),
				new Vector3(-8566.344f, 1388.302f, 194.8084f, "None"),
				new Vector3(-8570.155f, 1389.615f, 196.3465f, "None"),
				new Vector3(-8573.807f, 1391.237f, 196.483f, "None"),
				new Vector3(-8577.408f, 1393.028f, 196.5557f, "None"),
				new Vector3(-8580.92f, 1395.094f, 196.5366f, "None"),
				new Vector3(-8584.508f, 1396.96f, 196.4415f, "None"),
			};
		}
		public static class Dreygrot
		{
			static string Name = "Dreygrot Ships";
			public static List<Vector3> ShipCenters = new List<Vector3>()
			{
				//all Z = 3, ocean.Z = 0
				new Vector3(3162.091, 661.1346, 3, "None"),
				new Vector3(3232.259, 614.5892, 3, "None"),
				new Vector3(3259.116, 680.9519, 3, "None"),
			};
			public static Vector3 Center = new Vector3(3211.139, 651.6472, -0, "None");
			public static Vector3 Enter = new Vector3(3176.727, 747.4948, -1.639886, "Swimming");
			public static Vector3 GetNearShip()
			{
				return GetNearShip(ObjectManager.Me.Position);
			}
			public static Vector3 GetNearShip(Vector3 p)
			{
				return ShipCenters.OrderBy(v => v.DistanceTo2D(p)).FirstOrDefault();
			}
			public static bool IsInside(Vector3 p)
			{
				return p.DistanceTo2D(Center) < 100;
			}
			public static bool InShip(Vector3 p)
			{
				var nearShip = GetNearShip(p);
				return InShip(p, nearShip);
			}
			public static bool InShip(Vector3 p, Vector3 nearShip)
			{
				return nearShip.DistanceTo2D(p) < 40 && p.Z > nearShip.Z;
			}
			public static void StartMoveFix()
			{
				StopMoveFix();
				//wManager.wManagerSetting.CurrentSetting.UsePathsFinder = false;
				robotManager.Events.ProductEvents.OnProductStopping += OnProductStopping;
				wManager.Events.MovementEvents.OnMovementPulse += OnMovementPulse;
				Log(Name + " move fix start");
			}
			static void OnProductStopping(string productName)
			{
				StopMoveFix();
			}
			public static void StopMoveFix()
			{
				//wManager.wManagerSetting.CurrentSetting.UsePathsFinder = true;
				robotManager.Events.ProductEvents.OnProductStopping -= OnProductStopping;
				wManager.Events.MovementEvents.OnMovementPulse -= OnMovementPulse;
				Log(Name + " move fix stop");
			}
			static void OnMovementPulse(List<Vector3> points, System.ComponentModel.CancelEventArgs cancelable)
			{
				if (points == null || points.Count < 1)
					return;

				var myPos = ObjectManager.Me.Position;
				var endPos = points[points.Count - 1];
				var nearShip = GetNearShip(myPos);
				var endShip = GetNearShip(endPos);
				if (!IsInside(myPos) && IsInside(endPos))
				{
					Log(Name + " move inside grot");
					points.Clear();
					var path = PathFinder.FindPath(Enter);
					points.AddRange(path);
					points.Add(Center);
					return;
				}

				if (InShip(myPos, nearShip))
				{
					foreach (var p in points)
					{
						if (!InShip(p, nearShip))
						{
							Log(Name + " move out ship " + nearShip.ToStringNewVector());
							points.Clear();
							points.Add(Center);
							return;
						}
					}
					//inside ship movement. do nothing
					return;
				}
				foreach (var p in points)
				{
					if (InShip(p, endShip))
					{
						if (myPos.DistanceTo(endShip) < Harpoon.HookMaxDist)
						{
							Log(Name + " harpoon into ship: " + endShip.ToStringNewVector());
							cancelable.Cancel = true;
							points.Clear();
							Harpoon.UseNear(endShip, 40);
						}
						else
						{
							Log(Name + " move close to ship " + endShip.ToStringNewVector());
							endShip.Z = 0;
							points.Clear();
							//var path = PathFinder.FindPath(endShip);
							//points.AddRange(path);
							points.Add(endShip);
						}
						return;
					}
				}
			}
		}
		public static class GatesOfValorPeak
		{
			public static Vector3 Center = new Vector3(2466.033, 923.6632, 596.1412, "None");
			public static Npc VethirOut = new Npc()
			{
				Entry = 98190,
				Position = new Vector3(2488.314, 945.1337, 596.1417, "None"),
				Name = "Vethir",
			};
			public static Npc VethirIn = new Npc()
			{
				Entry = 97986,
				Position = new Vector3(2488.99, 959.9774, 241.4458, "None"),
				Name = "Vethir",
			};
			public static bool Inside
			{
				get
				{
					var myPos = ObjectManager.Me.Position;
					return myPos.DistanceTo2D(Center) < 100 && myPos.DistanceZ(Center) < 50;
				}
			}
		}
		public static class Hellheim
		{
			public static bool Inside { get { return Usefuls.ContinentId == (int)ContinentId.HelhiemExteriorArea; } }

			static List<Vector3> PathOutside = new List<Vector3>()
			{
				new Vector3(343.6187f, 356.7687f, 30.29915f, "None"),
				new Vector3(338.3649f, 347.7526f, 30.69351f, "None"),
				new Vector3(334.2404f, 341.7401f, 30.63486f, "None"),
				new Vector3(321.8669, 323.7027, 30.45891, "None"),
			};
			static List<Vector3> PathInside = new List<Vector3>()
			{
				new Vector3(3647.192f, 767.9564f, -6.038997f, "None"),
				new Vector3(3649.899f, 765.1718f, -7.362006f, "None"),
				new Vector3(3653.25f, 761.8541f, -8.967072f, "None"),
				new Vector3(3656.685f, 758.6512f, -10.56431f, "None"),
				new Vector3(3659.344f, 756.1819f, -11.79814f, "None"),
				new Vector3(3667.321, 748.774, -15.49963, "None"),
			};
			public static bool ToInside()
			{
				if (Inside)
				{
					Log("in Hellheim (to Hellheim)");
					return true;
				}
				Questing.GotoPathFromNear(PathInside);
				return false;
			}
			public static bool ToOutside()
			{
				if (!Inside)
				{
					Log("not in Hellheim (from Hellheim)");
					return true;
				}
				Questing.GotoPathFromNear(PathOutside);
				return false;
			}
		}
		public static class ThorimsPeak
		{
			public static Vector3 BottomCorner = new Vector3(2145.000, 2100.000, 450.00);
			public static Vector3 UpperCorner = new Vector3(1700.000, 2900.000, 850);
			public static Vector3 StartBridge = new Vector3(2160.634, 2428.002, 481.8286, "None");
			public static Vector3 EndBridge = new Vector3(2126.614, 2410.428, 479.1763, "None");
			public static bool IsOnPeak(Vector3 p)
			{
				return p.X > UpperCorner.X && p.X < BottomCorner.X && p.Y > BottomCorner.Y && p.Y < UpperCorner.Y && p.Z > BottomCorner.Z && p.Z < UpperCorner.Z;
			}
			public static bool InPeak { get { return IsOnPeak(ObjectManager.Me.Position); } }
			public static bool ToPeak()
			{
				if (InPeak)
				{
					Log("im in Thorim's Peak (To Thorim's Peak)");
					return true;
				}
				if (GalebrokenPath.InPath)
				{
					if (ObjectManager.Me.Position.DistanceTo(StartBridge) > 5)
					{
						Log("in Galebroken Path, goto bridge start (To Thorim's Peak)");
						GoToTask.ToPosition(StartBridge);
						return true;
					}
					Log("hook to bridge end (To Thorim's Peak)");
					Harpoon.UseNear(EndBridge, 5);
					return true;
				}
				Log("not in Galebroken Path, go there (To Thorim's Peak)");
				GalebrokenPath.ToPath();
				return false;
			}
		}
		public static class GalebrokenPath
		{
			public static Vector3 BottomCorner = new Vector3(2300.000, 2300.000, 295.000);
			public static Vector3 UpperCorner = new Vector3(2150.000, 2600.000, 495.000);
			public static Vector3 StartTower = new Vector3(2342.365, 2367.857, 316.5357, "None");
			public static Vector3 HookTower = new Vector3(2297.899, 2342.895, 312.7951, "None");
			public static bool IsOnPath(Vector3 p)
			{
				return p.X > UpperCorner.X && p.X < BottomCorner.X && p.Y > BottomCorner.Y && p.Y < UpperCorner.Y && p.Z > BottomCorner.Z && p.Z < UpperCorner.Z;
			}
			public static bool InPath { get { return IsOnPath(ObjectManager.Me.Position); } }
			public static bool ToPath()
			{
				if (InPath)
				{
					Log("im in Galebroken path (To Galebroken Path)");
					return true;
				}
				if (Hrydshal.Inside)
				{
					if (ObjectManager.Me.Position.DistanceTo(StartTower) > 5)
					{
						Log("in Hrydshal, goto tower (To Galebroken Path)");
						GoToTask.ToPosition(StartTower);
						return true;
					}
					Log("hook to path start (To Galebroken Path)");
					Harpoon.UseNear(HookTower, 5);
					return true;
				}
				Log("not in Hrydshal, go there (To Galebroken Path)");
				Hrydshal.ToInside();
				return false;
			}
		}
		public static class Hrydshal
		{
			public static Vector3 Center = new Vector3(2540.157, 2513.805, 246.5028, "None");
			public static Vector3 TowerCenter = new Vector3(2377.939, 2348.658, 267.759, "None");
			public static Vector3 Start = new Vector3(2673.813, 2408.071, 222.0235, "None");
			public static float InnerRadius = 170f;
			public static float OuterRaidus = 300f;
			public static bool Inside
			{
				get
				{
					var myPos = ObjectManager.Me.Position;
					if (myPos.DistanceTo(TowerCenter) < 85)
						return true;

					return myPos.DistanceTo(Center) < InnerRadius;
				}
			}
			public static bool Near { get { return (ObjectManager.Me.Position.DistanceTo(Center) < OuterRaidus); } }
			public static bool ToInside()
			{
				if (Inside)
				{
					Log("im inside Hrydshal");
					return true;
				}
				if (Near && Harpoon.GetNear(5) != null)
				{
					Log("im on wall, goto center (Hrydshal)");
					GoToTask.ToPosition(Center);
					return false;
				}
				if (ObjectManager.Me.Position.DistanceTo(Start) > Harpoon.HookMaxDist)
				{
					Log("to start position (Hrydshal)");
					GoToTask.ToPosition(Start);
					return false;
				}
				if (Near && Harpoon.GetNear() != null)
				{
					Log("near. harpoon on wall (Hrydshal)");
					Harpoon.UseNear();
				}
				return false;
			}
		}
		public static class TheMawofNashal
		{
			public static bool Inside
			{
				get
				{
					return Usefuls.ContinentId == (int)ContinentId.TheMawofNashal;
				}
			}
		}
		public static class Skyfire
		{
			static string Name = "Skyfire";
			public static WoWGameObject Ship
			{
				get
				{
					var skyfire = Questing.FindObject(241630);
					if (skyfire != null && skyfire.IsValid)
						return skyfire;

					return null;
				}
			}
			public static bool IsInside(Vector3 p)
			{
				var skyfire = Ship;
				return skyfire != null && skyfire.IsValid && p.DistanceTo2D(skyfire.Position) < 100;
			}
			public static void StartMoveFix()
			{
				StopMoveFix();
				wManager.wManagerSetting.CurrentSetting.UsePathsFinder = false;
				robotManager.Events.ProductEvents.OnProductStopping += OnProductStopping;
				wManager.Events.MovementEvents.OnMovementPulse += OnMovementPulse;
				Log(Name + " move fix start");
			}
			static void OnProductStopping(string productName)
			{
				StopMoveFix();
			}
			public static void StopMoveFix()
			{
				wManager.wManagerSetting.CurrentSetting.UsePathsFinder = true;
				robotManager.Events.ProductEvents.OnProductStopping -= OnProductStopping;
				wManager.Events.MovementEvents.OnMovementPulse -= OnMovementPulse;
				Log(Name + " move fix stop");
			}
			static void OnMovementPulse(List<Vector3> points, System.ComponentModel.CancelEventArgs cancelable)
			{
				var myPos = ObjectManager.Me.Position;
				if (IsInside(myPos) && PathFixed())
				{
					var prev = points.Count;
					var end = points[points.Count - 1];
					var path = Questing.PathClampDirected(_pathFixed, myPos, end);
					points.Clear();
					points.AddRange(path);
					Log(Name + " path fix from " + prev + " to " + points.Count);
				}
			}
			static bool PathFixed()
			{
				if (_pathFixed.Count > 0)
					return true;

				var skyfire = Ship;
				if (skyfire == null || !skyfire.IsValid)
					return false;

				_pathFixed = new List<Vector3>(_pathDelta);
				foreach (var p in _pathFixed)
				{
					p.X += skyfire.Position.X;
					p.Y += skyfire.Position.Y;
					p.Z += skyfire.Position.Z;
				}
				return true;
			}
			static List<Vector3> _pathFixed = new List<Vector3>();
			static List<Vector3> _pathDelta = new List<Vector3>()
			{
				new Vector3(63.19531, -8.420898, -2.022842, "None"),
				new Vector3(58.90234, -7.889893, -2.040741, "None"),
				new Vector3(55.43018, -7.392822, -2.057037, "None"),
				new Vector3(51.94824, -7.010986, -1.77504, "None"),
				new Vector3(46.2583, -9.675781, -2.074539, "None"),
				new Vector3(42.88525, -15.38599, -2.080338, "None"),
				new Vector3(39.28711, -15.43384, -3.34584, "None"),
				new Vector3(35.83008, -15.06885, -4.933945, "None"),
				new Vector3(32.66895, -13.54492, -5.276947, "None"),
				new Vector3(32.05518, -6.954834, -5.317047, "None"),
				new Vector3(31.24219, -3.501953, -5.319946, "None"),
				new Vector3(25.43018, -3.043945, -5.024139, "None"),
				new Vector3(22.37402, -9.286865, -5.200836, "None"),
				new Vector3(19.73438, -15.78979, -5.208038, "None"),
				new Vector3(15.78906, -21.323, -5.155838, "None"),
				new Vector3(10.77441, -19.2998, -5.14534, "None"),
				new Vector3(11.18408, -15.77588, -5.217834, "None"),
				new Vector3(12.9873, -9.064941, -5.236145, "None"),
				new Vector3(14.08594, -2.133789, -5.234344, "None"),
				new Vector3(16.72998, 4.270996, -5.229645, "None"),
				new Vector3(18.25195, 7.461182, -5.228348, "None"),
				new Vector3(19.76318, 10.62622, -5.226547, "None"),
				new Vector3(21.28223, 13.81812, -4.954941, "None"),
				new Vector3(21.47314, 19.96509, -5.139145, "None"),
				new Vector3(15.77637, 20.9231, -5.138138, "None"),
				new Vector3(9.728027, 24.39111, -4.975342, "None"),
				new Vector3(4.106934, 28.3291, -5.255539, "None"),
				new Vector3(0.7861328, 29.52612, -5.261642, "None"),
				new Vector3(-4.665039, 27.71216, -5.251434, "None"),
				new Vector3(-10.7168, 25.54004, -5.236847, "None"),
				new Vector3(-17.75195, 26.31201, -5.282135, "None"),
				new Vector3(-24.71289, 27.16211, -5.221848, "None"),
				new Vector3(-31.40869, 29.01416, -5.268646, "None"),
				new Vector3(-37.72803, 27.23706, -5.196548, "None"),
				new Vector3(-40.54102, 24.96021, -5.196548, "None"),
				new Vector3(-43.31299, 22.71021, -5.196548, "None"),
				new Vector3(-47.32568, 17.22607, -5.199738, "None"),
				new Vector3(-48.77686, 10.47314, -5.311447, "None"),
				new Vector3(-49.39502, 6.996094, -5.200546, "None"),
				new Vector3(-48.31787, 0.1621094, -5.198135, "None"),
				new Vector3(-43.08789, -4.37085, -5.198135, "None"),
				new Vector3(-36.89697, -7.577881, -5.262741, "None"),
				new Vector3(-33.61865, -8.990967, -4.957245, "None"),
				new Vector3(-30.5459, -14.90283, -5.227844, "None"),
				new Vector3(-26.07764, -19.26782, -5.267838, "None"),
				new Vector3(-23.68604, -13.9668, -5.241745, "None"),
				new Vector3(-25.85059, -11.14087, -5.264648, "None"),
				new Vector3(-24.26904, -5.368896, -5.09874, "None"),
				new Vector3(-20.80078, -5.645996, -4.319046, "None"),
				new Vector3(-17.33594, -6.305908, -1.696045, "None"),
				new Vector3(-13.92188, -6.97583, 1.012665, "None"),
				new Vector3(-10.55762, -7.645996, 3.731064, "None"),
				new Vector3(-7.091797, -8.409912, 7.187256, "None"),
				new Vector3(-3.661621, -9.315918, 9.818359, "None"),
				new Vector3(0.4072266, -14.30884, 9.632858, "None"),
				new Vector3(0.5742188, -17.8689, 9.641266, "None"),
				new Vector3(6.566406, -18.00098, 9.964462, "None"),
				new Vector3(9.968262, -17.05396, 9.561356, "None"),
				new Vector3(16.8291, -16.4668, 9.588058, "None"),
				new Vector3(20.28125, -15.70483, 9.598557, "None"),
				new Vector3(23.6333, -14.698, 9.606857, "None"),
				new Vector3(30.38721, -14.94482, 9.601852, "None"),
				new Vector3(34.76221, -15.36987, 9.599854, "None"),
				new Vector3(38.24902, -14.54297, 9.594666, "None"),
				new Vector3(44.45508, -15.90283, 9.280258, "None"),
				new Vector3(47.58936, -17.56592, 9.232758, "None"),
				new Vector3(49.90234, -14.47095, 9.213959, "None"),
				new Vector3(48.2002, -8.277832, 9.258255, "None"),
				new Vector3(46.60303, -5.068848, 9.571152, "None"),
				new Vector3(46.62793, 1.401123, 10.05865, "None"),
				new Vector3(48.8584, 4.150146, 9.271652, "None"),
				new Vector3(42.86523, 4.947021, 9.590454, "None"),
				new Vector3(39.20508, 6.512207, 9.602859, "None"),
				new Vector3(36.26416, 9.924072, 9.598358, "None"),
				new Vector3(32.8291, 11.09521, 9.974457, "None"),
				new Vector3(27.30811, 15.12305, 9.617355, "None"),
				new Vector3(24.24805, 16.87622, 9.609955, "None"),
				new Vector3(17.54297, 18.83008, 9.569763, "None"),
				new Vector3(14.09619, 19.80811, 9.961166, "None"),
				new Vector3(10.28906, 23.01709, 9.660263, "None"),
				new Vector3(7.333008, 17.50806, 9.641663, "None"),
				new Vector3(12.15137, 12.47607, 9.863754, "None"),
				new Vector3(16.45508, 7.184082, 9.526962, "None"),
				new Vector3(15.15039, 0.9111328, 9.510361, "None"),
				new Vector3(8.163086, 1.561035, 10.31766, "None"),
				new Vector3(4.788086, 2.210205, 12.07166, "None"),
				new Vector3(1.42041, 2.858154, 13.91995, "None"),
				new Vector3(-2.050781, 3.525146, 15.95866, "None"),
				new Vector3(-5.44873, 4.168213, 17.88306, "None"),
				new Vector3(-8.930664, 4.818115, 19.98056, "None"),
				new Vector3(-12.41406, 5.53418, 20.81276, "None"),
				new Vector3(-19.29102, 6.76416, 20.64955, "None"),
				new Vector3(-26.22168, 7.928223, 20.59436, "None"),
			};
		}
	}
	static void Log(string text)
	{
		Logging.WriteDebug("[Stormheim] " + text);
	}
	public static void ChangeSettings()
	{
		Log("change settings");
	}
}