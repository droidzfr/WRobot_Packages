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

public class SuramarQuests
{
	#region CONSTS
	public const int SURAMAR_AREA = 7637;
	public const uint MASQUERADE_SPELL = 202477; // http://www.wowhead.com/spell=202477/masquerade
	public const uint ENCHANTED_PARTY_MASK_SPELL = 208824; // http://www.wowhead.com/spell=208824/enchanted-party-mask
	public const int THE_NIGHTFALLEN_REPUTATION = 1859; // http://www.wowhead.com/faction=1859/the-nightfallen
	public const int ANCIENT_MANA_CURRENCY = 1155; // http://www.wowhead.com/currency=1155/ancient-mana
	public const string VAR_ANCIENT_MANA = "Cameleto10AncientMana";
	public const float SURAMAR_CITY_RADIUS = 985f;

	public static int THE_ARCWAY_OPENING_THE_ARCWAY = 42490;
	public static int THE_ARCWAY_LONG_BURIED_KNOWLEDGE = 42491;
	public static int COURT_OF_STARS_BEWARE_THE_FURY_OF_A_PATIENT_ELF = 43314;
	public static int THE_EMERALD_NIGHTMARE_THE_STUFF_OF_DREAMS = 43362;

	public static bool NeedRaid
	{
		get
		{
			if (Questing.Complete(THE_EMERALD_NIGHTMARE_THE_STUFF_OF_DREAMS))
				return false;
			if (Questing.Has(THE_EMERALD_NIGHTMARE_THE_STUFF_OF_DREAMS) && !Quest.IsObjectiveComplete(2, THE_EMERALD_NIGHTMARE_THE_STUFF_OF_DREAMS))
				return true;
			return false;
		}
	}

	public static bool NeedDungeons
	{
		get
		{
			if (Quest.HasQuest(THE_ARCWAY_OPENING_THE_ARCWAY) && !Quest.IsObjectiveComplete(1, THE_ARCWAY_OPENING_THE_ARCWAY))
				return true;

			if (Quest.HasQuest(THE_ARCWAY_LONG_BURIED_KNOWLEDGE) && !Quest.IsObjectiveComplete(1, THE_ARCWAY_LONG_BURIED_KNOWLEDGE))
				return true;

			if (Quest.HasQuest(COURT_OF_STARS_BEWARE_THE_FURY_OF_A_PATIENT_ELF) && !Quest.IsObjectiveComplete(2, COURT_OF_STARS_BEWARE_THE_FURY_OF_A_PATIENT_ELF))
				return true;

			return false;
		}
	}

	#endregion CONSTS
	static SuramarQuests()
	{
	}
	public static void Log(string text)
	{
		Logging.WriteDebug("[Suramar] " + text);
	}
	public static bool InSuramar
	{
		get
		{
			return Usefuls.AreaId == SURAMAR_AREA;
		}
	}
	public static bool IsHonored
	{
		get
		{
			return TheNightfallenReputationLevel >= 6;
		}
	}
	public static bool IsRevered
	{
		get
		{
			return TheNightfallenReputationLevel >= 7;
		}
	}
	/// <summary>
	/// 1 - Hated, 2 - Hostile, 3 - Unfriendly, 4 - Neutral, 5 - Friendly, 6 - Honored, 7 - Revered, 8 - Exalted
	/// </summary>
	public static int TheNightfallenReputationLevel
	{
		get
		{
			List<string> values = Questing.GetReputation(THE_NIGHTFALLEN_REPUTATION);
			if (values == null || values.Count < 3)
				return 0;

			int level = 0;
			int.TryParse(values[2], out level);
			return level;
		}
	}
	public static int TheNightfallenReputation
	{
		get
		{
			List<string> values = Questing.GetReputation(THE_NIGHTFALLEN_REPUTATION);
			if (values == null || values.Count < 6)
				return 0;

			int amount = 0;
			int.TryParse(values[5], out amount);
			return amount;
		}
	}
	public static bool NeedMask
	{
		get
		{
			var me = ObjectManager.Me;
			if (me.IsCast || me.InCombat || me.PlayerUsingVehicle || me.IsOnTaxi)
				return false;

			if (!InCity)
				return false;

			if (me.HaveBuff(MASQUERADE_SPELL) || me.HaveBuff(ENCHANTED_PARTY_MASK_SPELL))
				return false;

			if (Quest.GetQuestCompleted(42079))
				return Questing.ExtraButton();

			return true;
		}
	}
	public static void CastMask()
	{
		MovementManager.StopMoveTo(true, 1000);
		if (ItemsManager.HasItemById(Items.MASK_ITEMID))
		{
			Questing.Use((int)Items.MASK_ITEMID);
		}
		else
		{
			Questing.UseExtraButton(1);
		}
		Questing.WaitIsCasting(ObjectManager.Me);
	}
	public static int AncientMana
	{
		get
		{
			return Questing.Currency(ANCIENT_MANA_CURRENCY);
		}
	}
	public static int AncientManaMax
	{
		get
		{
			return Questing.CurrencyMax(ANCIENT_MANA_CURRENCY);
		}
	}
	public static int AncientManaNeed
	{
		get
		{
			if (Var.Exist(VAR_ANCIENT_MANA) && Var.IsType(VAR_ANCIENT_MANA, typeof(int)))
			{
				return Var.GetVar<int>(VAR_ANCIENT_MANA);
			}
			return 0;
		}
		set
		{
			Var.SetVar(VAR_ANCIENT_MANA, value);
		}
	}
	public static bool InShalaran
	{
		get
		{
			return ObjectManager.Me.IsIndoors && ObjectManager.Me.Position.DistanceTo2D(Positions.SHALARAN_CENTER) < 150;
		}
	}
	public static bool InSuramarCity
	{
		get
		{
			return InCity;
		}
	}
	public static bool InCity
	{
		get
		{
			return ObjectManager.Me.Position.DistanceTo2D(Positions.SURAMAR_CITY_CENTER) < SURAMAR_CITY_RADIUS;
		}
	}
	public static List<int> CitySubzone = new List<int>()
	{
		Subzone.THE_GRAND_PROMENADE,
		Subzone.LUNASTRE_ESTATE,
		Subzone.STAR_CALLER_RETREAT,
		Subzone.THE_WANING_CRESCENT,
		Subzone.ARTISANS_GALLERY,
		Subzone.ASTRAVAR_HARBOR,
		Subzone.EVERMOON_BAZAAR,
		Subzone.EVERMOON_COMMONS,
		Subzone.SHIMMERSHADE_GARDEN,
		Subzone.TERRACE_OF_ORDER,
		Subzone.ESTATE_OF_THE_FIRST_ARCANIST,
		Subzone.TWILIGHT_VINEYARDS,
		Subzone.CONCOURSE_OF_DESTINY,
		Subzone.TERRACE_OF_ENLIGHTENMENT,
		Subzone.MOONBEAM_CAUSEWAY,
		Subzone.SUNSET_PARK,
		Subzone.THE_MENAGERIE,
		Subzone.THE_GILDED_MARKET,
		Subzone.MOONLIT_LANDING,
		Subzone.SANCTUM_OF_ORDER,
		Subzone.EVERMOON_TERRACE,
		Subzone.SURAMAR_CITY,
	};

	public static class Subzone
	{
		public const int SHAL_ARAN = 7928;
		public const int STAR_CALLER_RETREAT = 8434;
		public const int THE_WANING_CRESCENT = 8382;
		public const int EVERMOON_TERRACE = 8487;
		public const int EVERMOON_COMMONS = 7970;
		public const int MOONLIT_LANDING = 8378;
		public const int MIDNIGHT_COURT = 8380;
		//
		public const int THE_GRAND_PROMENADE = 7963;
		public const int LUNASTRE_ESTATE = 8021;
		public const int ARTISANS_GALLERY = 8383;
		public const int ASTRAVAR_HARBOR = 8395;
		public const int EVERMOON_BAZAAR = 8385;
		public const int SHIMMERSHADE_GARDEN = 8386;
		public const int TERRACE_OF_ORDER = 8398;
		public const int TWILIGHT_VINEYARDS = 8149;
		public const int CONCOURSE_OF_DESTINY = 8353;
		public const int TERRACE_OF_ENLIGHTENMENT = 8397;
		public const int MOONBEAM_CAUSEWAY = 8441;
		public const int SUNSET_PARK = 8436;
		public const int THE_MENAGERIE = 8351;
		public const int THE_GILDED_MARKET = 8379;
		public const int SANCTUM_OF_ORDER = 8431;
		public const int SURAMAR_CITY = 8148;
		public const int ESTATE_OF_THE_FIRST_ARCANIST = -1; //TODO
		public static class ThalyssrasEstate
		{
			public static string Name = "Thalyssra's estate";
			public static float radius = 13f;
			public static Vector3 Center = new Vector3(1118.026, 3006.433, 6.851443, "None");
			public static Vector3 Inside = new Vector3(1111.617, 3006.174, 6.450747, "None");
			public static Vector3 Outside = new Vector3(1099.062, 3004.242, 4.433238, "None");
			public static Vector3 ShipCenter = new Vector3(1046.199, 2998.477, 2.987295, "None");
			public static bool IsInside(Vector3 p)
			{
				return p.DistanceTo2D(Center) < radius;
			}
			public static void StartMoveFix()
			{
				StopMoveFix();
				robotManager.Events.ProductEvents.OnProductStopping += OnProductStop;
				wManager.Events.MovementEvents.OnMovementPulse += OnMovementPulse;
				Log(Name + " move fix start");
				ThalyssrasEstateShip.StartMoveFix();
			}
			static void OnMovementPulse(List<Vector3> points, System.ComponentModel.CancelEventArgs cancelable)
			{
				if (points.Count < 1)
					return;

				var myPos = ObjectManager.Me.Position;
				var end = points[points.Count - 1];
				if (IsInside(end))
				{
					if (IsInside(myPos))
					{
						points.Clear();
						points.Add(end);
						Log(Name + " path fixed (inside)");
					}
					else
					{
						var path = PathFinder.FindPath(Inside);
						points.Clear();
						points.AddRange(path);
						Log(Name + " path fixed from outside");
					}
				}
				else if (IsInside(myPos))
				{
					//var path = PathFinder.FindPath(Outside);
					points.Clear();
					points.Add(Center);
					points.Add(Outside);
					//points.AddRange(path);
					Log(Name + " path fixed to outside");
				}
			}
			static void OnProductStop(string productName)
			{
				StopMoveFix();
			}
			public static void StopMoveFix()
			{
				robotManager.Events.ProductEvents.OnProductStopping -= OnProductStop;
				wManager.Events.MovementEvents.OnMovementPulse -= OnMovementPulse;
				Log(Name + " move fix stop");
				ThalyssrasEstateShip.StopMoveFix();
			}
		}
		public static class ThalyssrasEstateShip
		{
			public static string Name = "Thalyssra's estate ship";
			public static float radius = 18f;
			public static Vector3 Center = new Vector3(1046.199, 2998.477, 2.987295, "None");
			public static Vector3 Inside = new Vector3(1049.621, 2997.59, 2.957039, "None");
			public static Vector3 Outside = new Vector3(1065.754, 3000.889, 2.720932, "None");
			public static List<Vector3> FromWater = new List<Vector3>()
			{
				new Vector3(1064.401, 2955.27, -1.679922, "Swimming"),
				new Vector3(1090.089, 2940.284, 2.946177, "None"),
			};
			public static bool IsInside(Vector3 p)
			{
				return p.DistanceTo2D(Center) < radius && p.DistanceZ(Center) < 1;
			}
			public static void StartMoveFix()
			{
				StopMoveFix();
				robotManager.Events.ProductEvents.OnProductStopping += OnProductStop;
				wManager.Events.MovementEvents.OnMovementPulse += OnMovementPulse;
				Log(Name + " move fix start");
			}
			static void OnMovementPulse(List<Vector3> points, System.ComponentModel.CancelEventArgs cancelable)
			{
				if (points.Count < 1)
					return;

				var myPos = ObjectManager.Me.Position;
				var end = points[points.Count - 1];
				if (myPos.IsSwimming())
				{
					var path = PathFinder.FindPath(FromWater[0]);
					points.Clear();
					points.AddRange(path);
					points.AddRange(FromWater);
					Log(Name + " path fixed from water");
					return;
				}
				if (IsInside(end))
				{
					if (IsInside(myPos))
					{
						points.Clear();
						points.Add(end);
						Log(Name + " path fixed (inside)");
					}
					else
					{
						var path = PathFinder.FindPath(Inside);
						points.Clear();
						points.AddRange(path);
						points.Add(Center);
						Log(Name + " path fixed from outside");
					}
				}
				else if (IsInside(myPos))
				{
					//var path = PathFinder.FindPath(Outside);
					points.Clear();
					points.Add(Center);
					points.Add(Outside);
					//points.AddRange(path);
					Log(Name + " path fixed to outside");
				}
			}
			static void OnProductStop(string productName)
			{
				StopMoveFix();
			}
			public static void StopMoveFix()
			{
				robotManager.Events.ProductEvents.OnProductStopping -= OnProductStop;
				wManager.Events.MovementEvents.OnMovementPulse -= OnMovementPulse;
				Log(Name + " move fix stop");
			}
		}
		public static class MidnightCourtHouse2
		{
			public static string Name = "Midnight Court House 2";
			public static float radius = 13f;
			public static Vector3 Center = new Vector3(1017.538, 3397.111, 21.40625, "None");
			public static Vector3 Inside = new Vector3(1018.028, 3391.854, 21.25576, "None");
			public static Vector3 Outside = new Vector3(1009.103, 3379.06, 19.83846, "None");
			public static bool IsInside(Vector3 p)
			{
				return p.DistanceTo2D(Center) < radius;
			}
			public static void StartMoveFix()
			{
				StopMoveFix();
				robotManager.Events.ProductEvents.OnProductStopping += OnProductStop;
				wManager.Events.MovementEvents.OnMovementPulse += OnMovementPulse;
				Log(Name + " move fix start");
			}
			static void OnMovementPulse(List<Vector3> points, System.ComponentModel.CancelEventArgs cancelable)
			{
				if (points.Count < 1)
					return;

				var myPos = ObjectManager.Me.Position;
				var end = points[points.Count - 1];
				if (IsInside(end))
				{
					if (IsInside(myPos))
					{
						points.Clear();
						points.Add(end);
						Log(Name + " path fixed (inside)");
					}
					else
					{
						var path = PathFinder.FindPath(Inside);
						points.Clear();
						points.AddRange(path);
						Log(Name + " path fixed from outside");
					}
				}
				else if (IsInside(myPos))
				{
					//var path = PathFinder.FindPath(Outside);
					points.Clear();
					points.Add(Center);
					points.Add(Outside);
					//points.AddRange(path);
					Log(Name + " path fixed to outside");
				}
			}
			static void OnProductStop(string productName)
			{
				StopMoveFix();
			}
			public static void StopMoveFix()
			{
				robotManager.Events.ProductEvents.OnProductStopping -= OnProductStop;
				wManager.Events.MovementEvents.OnMovementPulse -= OnMovementPulse;
				Log(Name + " move fix stop");
			}
		}
		public static class ShimmershadeGardenHouse2
		{
			public static string Name = "Shimmershade Garden House 2";
			public static float radius = 13f;
			public static Vector3 Center = new Vector3(783.5569, 4111.117, 2.992139, "None");
			public static Vector3 Outside = new Vector3(807.2054, 4109.418, 1.49332, "None");
			public static Vector3 Inside = new Vector3(791.0376, 4109.486, 2.992139, "None");
			public static bool IsInside(Vector3 p)
			{
				return p.DistanceTo2D(Center) < radius;
			}
			public static void StartMoveFix()
			{
				StopMoveFix();
				robotManager.Events.ProductEvents.OnProductStopping += OnProductStop;
				wManager.Events.MovementEvents.OnMovementPulse += OnMovementPulse;
				Log(Name + " move fix start");
			}
			static void OnMovementPulse(List<Vector3> points, System.ComponentModel.CancelEventArgs cancelable)
			{
				if (points.Count < 1)
					return;

				var myPos = ObjectManager.Me.Position;
				var end = points[points.Count - 1];
				if (IsInside(end))
				{
					if (IsInside(myPos))
					{
						points.Clear();
						points.Add(end);
						Log(Name + " path fixed (inside)");
					}
					else
					{
						var path = PathFinder.FindPath(Inside);
						points.Clear();
						points.AddRange(path);
						Log(Name + " path fixed from outside");
					}
				}
				else if (IsInside(myPos))
				{
					//var path = PathFinder.FindPath(Outside);
					points.Clear();
					points.Add(Center);
					points.Add(Outside);
					//points.AddRange(path);
					Log(Name + " path fixed to outside");
				}
			}
			static void OnProductStop(string productName)
			{
				StopMoveFix();
			}
			public static void StopMoveFix()
			{
				robotManager.Events.ProductEvents.OnProductStopping -= OnProductStop;
				wManager.Events.MovementEvents.OnMovementPulse -= OnMovementPulse;
				Log(Name + " move fix stop");
			}
		}
		public static class ShimmershadeGardenHouse1
		{
			public static string Name = "Shimmershade Garden House 1";
			public static float radius = 13f;
			public static Vector3 Center = new Vector3(743.7032, 4200.479, 3.578341, "None");
			public static Vector3 Outside = new Vector3(749.5173, 4181.011, 1.493545, "None");
			public static Vector3 Inside = new Vector3(748.0145, 4195.821, 3.25049, "None");
			public static bool IsInside(Vector3 p)
			{
				return p.DistanceTo2D(Center) < radius;
			}
			public static void StartMoveFix()
			{
				StopMoveFix();
				robotManager.Events.ProductEvents.OnProductStopping += OnProductStop;
				wManager.Events.MovementEvents.OnMovementPulse += OnMovementPulse;
				Log(Name + " move fix start");
			}
			static void OnMovementPulse(List<Vector3> points, System.ComponentModel.CancelEventArgs cancelable)
			{
				if (points.Count < 1)
					return;

				var myPos = ObjectManager.Me.Position;
				var end = points[points.Count - 1];
				if (IsInside(end))
				{
					if (IsInside(myPos))
					{
						points.Clear();
						points.Add(end);
						Log(Name + " path fixed (inside)");
					}
					else
					{
						var path = PathFinder.FindPath(Inside);
						points.Clear();
						points.AddRange(path);
						Log(Name + " path fixed from outside");
					}
				}
				else if (IsInside(myPos))
				{
					var path = PathFinder.FindPath(Outside);
					points.Clear();
					points.AddRange(path);
					Log(Name + " path fixed to outside");
				}
			}
			static void OnProductStop(string productName)
			{
				StopMoveFix();
			}
			public static void StopMoveFix()
			{
				robotManager.Events.ProductEvents.OnProductStopping -= OnProductStop;
				wManager.Events.MovementEvents.OnMovementPulse -= OnMovementPulse;
				Log(Name + " move fix stop");
			}
		}
		public static class VineyardHouse1
		{
			public static Vector3 Center = new Vector3(1400.309, 3254.411, 134.7887, "None");
			public static bool IsInside(Vector3 p)
			{
				return p.DistanceTo2D(Center) < 12f;
			}
			public static void StartMoveFix()
			{
				StopMoveFix();
				robotManager.Events.ProductEvents.OnProductStopping += OnProductStop;
				wManager.Events.MovementEvents.OnMovementPulse += OnMovementPulse;
				Log("Vineyard House1 move fix start");
			}
			static void OnMovementPulse(List<Vector3> points, System.ComponentModel.CancelEventArgs cancelable)
			{
				if (points.Count < 1)
					return;

				var myPos = ObjectManager.Me.Position;
				var end = points[points.Count - 1];
				if (IsInside(end))
				{
					if (IsInside(myPos))
					{
						points.Clear();
						points.Add(end);
						Log("Vineyard House1 path fixed (inside)");
					}
					else
					{
						var path = PathFinder.FindPath(Center);
						points.Clear();
						points.AddRange(path);
						Log("Vineyard House1 path fixed from outside");
					}
				}
			}
			static void OnProductStop(string productName)
			{
				StopMoveFix();
			}
			public static void StopMoveFix()
			{
				robotManager.Events.ProductEvents.OnProductStopping -= OnProductStop;
				wManager.Events.MovementEvents.OnMovementPulse -= OnMovementPulse;
				Log("Vineyard House1 move fix stop");
			}
		}
		public static class FjolrikShip
		{
			public static string Name = "Fjolrik Ship";
			public static Vector3 Center = new Vector3(1232.948, 2352.672, -117.2658, "Swimming");
			public static Vector3 Entrance = new Vector3(1212.986, 2366.362, -113.8657, "Swimming");
			public static bool IsInside(Vector3 p)
			{
				return p.DistanceTo(Center) < 10;
			}
			public static void StartMoveFix()
			{
				StopMoveFix();
				robotManager.Events.ProductEvents.OnProductStopping += OnProductStop;
				wManager.Events.MovementEvents.OnMovementPulse += OnMovementPulse;
				Log(Name + " move fix start");
			}
			static void OnMovementPulse(List<Vector3> points, System.ComponentModel.CancelEventArgs cancelable)
			{
				if (points.Count < 1)
					return;

				var myPos = ObjectManager.Me.Position;
				foreach (var p in points)
				{
					if (IsInside(p) && !IsInside(myPos) && myPos.DistanceTo(Entrance) > 10)
					{
						points.Clear();
						var path = PathFinder.FindPath(Entrance);
						Log(Name + " path fixed to entrance");
						points.AddRange(path);
						return;
					}
					if (IsInside(myPos) && !IsInside(p))
					{
						Log(Name + " ship path fixed to outside");
						points.Clear();
						points.Add(Entrance);
						return;
					}
				}
			}
			static void OnProductStop(string productName)
			{
				StopMoveFix();
			}
			public static void StopMoveFix()
			{
				robotManager.Events.ProductEvents.OnProductStopping -= OnProductStop;
				wManager.Events.MovementEvents.OnMovementPulse -= OnMovementPulse;
				Log("Fjolrik ship move fix stop");
			}
		}
		public static class SashjtarGrotto
		{
			public static Vector3 Center = new Vector3(1485.502, 2441.157, -67.66195);
			public static List<Vector3> PathInside =new List<Vector3>()
			{
				new Vector3(1344.802, 2383.029, -76.0234, "Swimming"),
				new Vector3(1359.302f, 2384.938f, -75.35843f, "Swimming"),
				new Vector3(1370.229f, 2391.599f, -80.23371f, "Swimming"),
				new Vector3(1379.156f, 2396.66f, -83.04394f, "Swimming"),
				new Vector3(1388.436f, 2401.655f, -83.83667f, "Swimming"),
				new Vector3(1397.922f, 2406.203f, -82.22986f, "Swimming"),
				new Vector3(1407.341f, 2410.517f, -80.12356f, "Swimming"),
				new Vector3(1416.914f, 2414.832f, -78.12785f, "Swimming"),
				new Vector3(1426.629f, 2418.948f, -76.49015f, "Swimming"),
				new Vector3(1436.278f, 2423.05f, -75.30875f, "Swimming"),
				new Vector3(1446.05f, 2427.123f, -74.40881f, "Swimming"),
				new Vector3(1456.044f, 2431.267f, -72.97443f, "None"),
				new Vector3(1467.919f, 2436.15f, -69.9212f, "None"),
			};
			public static bool Inside
			{
				get
				{
					if (Usefuls.ContinentId != (int)ContinentId.Troll_Raid)
						return false;

					return ObjectManager.Me.IsIndoors && ObjectManager.Me.Position.DistanceTo2D(Center) < 100;
				}
			}
			public static bool ToInside()
			{
				if (Inside)
				{
					Log("in Sashj'tar Grotto");
					return true;
				}
				if (ObjectManager.Me.Position.DistanceTo(PathInside[0]) < 25)
				{
					Log("move inside Sashj'tar Grotto");
					Questing.PathFromNear(PathInside, true);
					return false;
				}
				Log("move near Sashj'tar Grotto");
				GoToTask.ToPosition(PathInside[0]);
				return false;
			}
			public static bool ToOutside()
			{
				if (!Inside)
				{
					Log("in Sashj'tar Grotto");
					return true;
				}
				var path = new List<Vector3>(PathInside);
				path.Reverse();
				Log("move outside Sashj'tar Grotto");
				Questing.PathFromNear(path, true);
				return false;
			}
		}
	}
	public static class Positions
	{
		public static Vector3 SHALARAN_CENTER = new Vector3(1751.828, 4563.563, 124.0406, "None");
		public static Vector3 SHALARAN_TELEPORTERS = new Vector3(1769.944, 4636.429, 123.9918, "None");
		public static Vector3 SURAMAR_CITY_CENTER = new Vector3(792.6295, 3645.381, 0.6573398, "None");
		public static Vector3 SURAMAR_CITY_GATE = new Vector3(1489.618, 4341.261, 113.0719, "None");
		public static Vector3 WARP_WORKSHOP = new Vector3(1709.767, 3915.335, 167.9709); // new Vector3(1709.996f, 3914.844f, 0f);
		public static Vector3 WARP_GARDEN = new Vector3(1976.355, 3560.497, 363.0712);// new Vector3(1975.996f, 3560.247f, 0f);
		public static Vector3 WARP_LAB = new Vector3(1776.206, 3763.747, 268.8854, "None");// new Vector3(1776.192f, 3763.838f, 0f);
		public static Vector3 WARP_FOUNTAIN = new Vector3(1800.145, 3647.24, 304.2678, "None"); // new Vector3(1800.066f, 3646.988f, 0f);
		public static Vector3 WARP_TELEMLAB = new Vector3(2082.709, 3711.614, 454.3546, "None");// new Vector3(2083.047f, 3711.535f, 0f);
		public static Vector3 WARP_TESTCHAMBER = new Vector3(2088.436, 3341.729, 279.9612, "None");
	}
	public static class Portals
	{
		public class Portal
		{
			public Vector3 Position;
			public int Entry;
			public bool Use() { return GoToTask.ToPositionAndIntecractWithGameObject(Position, Entry); }
		}
		//from shalaran
		public static Portal ShalanarToWaningCrescent = new Portal()
		{
			Entry = 254137,
			Position = Positions.SHALARAN_TELEPORTERS,
		};
		public static Portal ShalanarToRuinsofEluneeth = new Portal()
		{
			Entry = 254129,
			Position = Positions.SHALARAN_TELEPORTERS,
		};
		public static Portal ShalanarToFelsoul = new Portal()
		{
			Entry = 254131,
			Position = Positions.SHALARAN_TELEPORTERS,
		};
		public static Portal ShalanarToFalanaar = new Portal()
		{
			Entry = 254133,
			Position = Positions.SHALARAN_TELEPORTERS,
		};
		public static Portal ShalanarToLunastreEstate = new Portal()
		{
			Entry = 254134,
			Position = Positions.SHALARAN_TELEPORTERS,
		};
		public static Portal ShalanarToMoonGuardStronghold = new Portal()
		{
			Entry = 254135,
			Position = Positions.SHALARAN_TELEPORTERS,
		};
		public static Portal ShalanarToTelanor = new Portal()
		{
			Entry = 254136,
			Position = Positions.SHALARAN_TELEPORTERS,
		};
		public static Portal ShalanarToTwilightVineyards = new Portal()
		{
			Entry = 254139,
			Position = Positions.SHALARAN_TELEPORTERS,
		};
		public static Portal ShalanarToSanctumOfOrder = new Portal()
		{
			Entry = 254143,
			Position = Positions.SHALARAN_TELEPORTERS,
		};
		//to shalaran
		public static Portal OutsideToShalaran = new Portal()
		{
			Entry = 260260,
			Position = new Vector3(1703.401, 4651.938, 170.0116),
		};
		public static Portal WaningCrescentToShalanar = new Portal()
		{
			Entry = 260269,
			Position = new Vector3(433.5191, 4007.51, 2.859004),
		};
		public static Portal FalanaarToShalanar = new Portal()
		{
			Entry = 260251,
			Position = new Vector3(2369.785, 5442.302, 16.19019, "None"),
		};
	}
	public static class NPC
	{
		public static Npc FIRST_ARCANIST_THALYSSRA = new Npc() {
			Entry = 97140,
			Position = new Vector3(1724.658, 4614.287, 123.7945),
			Type = Npc.NpcType.None,
		};
		public static Npc ARCANIST_VALTROIS = new Npc()
		{
			Entry = 103155,
			Position = new Vector3(1737.737, 4603.282, 96.28349),
			Type = Npc.NpcType.None,
		};
		public static Npc CHIEF_TELEMANCER_OCULETH = new Npc()
		{
			Entry = 98548,
			Position = new Vector3(1737.695, 4603.249, 96.28294),
			Type = Npc.NpcType.None,
		};
	}
	public static class Items
	{
		public const uint MASK_ITEMID = 136600; // http://www.wowhead.com/item=136600/enchanted-party-mask
		public const uint MANA_TINGED_PACK = 140226; // from shoulder enchant

		//ancient mana items
		public const uint HALF_FULL_BOTTLE_OF_ARCWINE = 137010;
		public const uint SMALL_JAR_OF_ARCWINE = 140235;
		public const uint THE_TIDEMISTRESS_ENCHANTED_PEARL = 140245;
		public const uint ASTROMANCERS_COMPASS = 140242;
		public const uint PRIMED_ARCANE_CHARGE = 140406;
		public const uint ENCHANTED_MOONWELL_WATERS = 140240;
		public const uint YELLOW_ORLIGAI_EGG = 140399;
		public const uint ILLUSION_MATRIX_CRYSTAL = 140405;
		public const uint ARC_OF_SNOW = 140246;
		public const uint ANCIENT_MANA_CRYSTAL = 139786; //not bop
		public const uint ANCIENT_MANA_GEM = 139890; //not bop
		public const uint ANCIENT_MANA_CRYSTAL_CLUSTER = 143734;
		public const uint ANCIENT_MANA_SHARDS = 143733;
		public const uint A_MRGLRMRL_MLRGLR = 140236;
		public const uint AZUREFALL_ESSENCE = 140243;
		public const uint EXCAVATED_HIGHBORNE_ARTIFACT = 140239;
		public const uint LEYSCALE_KOI = 143748;
		public const uint MASTER_JEWELERS_GEM = 140248;
		public const uint ONYX_ORLIGAI_EGG = 140949;
		public static bool UseAncientManaItem()
		{
			if (AncientMana >= AncientManaMax)
				return false;

			return Questing.SafeUseItem(AncientManaItems);
		}
		public static uint[] AncientManaItems = new uint[]
		{
			HALF_FULL_BOTTLE_OF_ARCWINE,
			SMALL_JAR_OF_ARCWINE,
			THE_TIDEMISTRESS_ENCHANTED_PEARL,
			ASTROMANCERS_COMPASS,
			PRIMED_ARCANE_CHARGE,
			ENCHANTED_MOONWELL_WATERS,
			YELLOW_ORLIGAI_EGG,
			ILLUSION_MATRIX_CRYSTAL,
			ARC_OF_SNOW,
			ANCIENT_MANA_CRYSTAL_CLUSTER,
			ANCIENT_MANA_SHARDS,
			A_MRGLRMRL_MLRGLR,
			ANCIENT_MANA_CRYSTAL, //not bop
			ANCIENT_MANA_GEM, //not bop
			AZUREFALL_ESSENCE,
			EXCAVATED_HIGHBORNE_ARTIFACT,
			LEYSCALE_KOI,
			MASTER_JEWELERS_GEM,
			ONYX_ORLIGAI_EGG,
		};

		public static uint[] ArtifactPowerList = new uint[]
		{
			142054, 141667, 141668, 141933, 141947, 141934, 140255, 141950, 141936, 141937, 141671, 140241, 141943, 141946, 141674, 140244, 141675, 140254,
			140251, 141941, 141942, 141945, 141673, 140252, 141950, 141951, 141677, 141952, 141953, 141954, 140410, 141678, 141680, 141955, 140422, 140409,
			140421, 140444, 141682, 141683, 141684, 140250, 141940, 141669, 140238, 141679, 141670, 143757, 143747, 143540, 143746, 143538, 143745, 143536,
			143749, 143533, 143744, 143499, 143498, 143741, 143742, 143486, 143487, 143739, 143738, 142555, 141944, 141672, 140349, 140237, 141949, 140247,
			141948, 143743, 141935, 
		};
		public static bool TryUseArtifactPowerItems()
		{
			return Questing.SafeUseItem(ArtifactPowerList);
		}
	}
	public static class Masquerade
	{
		static Thread _thread;
		public static void Start()
		{
			if (_thread != null)
				Stop();

			_thread = new Thread(Routine);
			_thread.Start();
			Log("Masquerade start");
		}
		public static void Stop()
		{
			if (_thread == null)
				return;

			_thread.Abort();
			_thread = null;
			Log("Masquerade stop");
		}
		static void Routine()
		{
			while (robotManager.Products.Products.IsStarted)
			{
				if (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause)
				{
					if (NeedMask)
					{
						CastMask();
					}
					else
					{
						Items.TryUseArtifactPowerItems();
					}
				}
				Thread.Sleep(1000);
			}
		}
	}
	public static class GatherAncientMana
	{
		static int _nextHotspotIndex = 0;
		public static int QUESTID_VINEYARDS_UNLOCKED = 44084;
		public static bool Collected(int amount)
		{
			return !NotCollected(amount);
		}
		public static bool NotCollected(int amount)
		{
			if (Items.TryUseArtifactPowerItems())
			{
				Thread.Sleep(Others.Random(100, 200));
				return true;
			}

			if (LimitNotEnough(amount))
			{
				return true;
			}
			var need = amount - AncientMana;
			if (need > 0)
			{
				if (Items.UseAncientManaItem())
				{
					return true;
				}
				Gather(need);
				return true;
			}
			return false;
		}
		public static bool LimitNotEnough(int amount)
		{
			var need = amount - AncientManaMax;
			if (need > 0)
			{
				Log("need limit " + amount + " of ancient mana. current: " + AncientManaMax + ". raising Ancient Mana limit. Left=" + need);
				return true;
			}
			return false;
		}
		public static bool Gather(int need)
		{
			Log("need " + need + " ancient mana. gathering.");
			var hotspots = FarmSpots;
			if (Questing.Complete(QUESTID_VINEYARDS_UNLOCKED))
			{
				hotspots = FarmSpotsVineyards;
			}
			var mana = ObjectManager.GetNearestWoWGameObject(ObjectManager.GetWoWGameObjectByEntry(FarmList));
			if (mana != null && mana.IsValid)
			{
				var result = false;
				var path = PathFinder.FindPath(mana.Position, out result);
				if (!result)
				{
					Log("no path to " + mana.Name + ". blacklist");
					wManager.wManagerSetting.AddBlackList(mana.Guid, 1 * 60 * 1000);
					return true;
				}
				var pathDistance = Questing.PathDistance(path);
				if (pathDistance > wManager.wManagerSetting.CurrentSetting.SearchRadius * 2)
				{
					Log("path too long to " + mana.Name + ". blacklist");
					wManager.wManagerSetting.AddBlackList(mana.Guid, 1 * 60 * 1000);
					return true;
				}
				Log("gather ancient mana: " + mana.Name);
				if (GoToTask.ToPosition(mana.Position))
				{
					MountTask.DismountMount();
					if (GoToTask.ToPositionAndIntecractWithGameObject(mana.Position, mana.Entry))
					{
						Thread.Sleep( Others.Random(500, 1000));
						wManager.wManagerSetting.AddBlackList(mana.Guid, 1 * 60 * 1000);
						return true;
					}
				}
				return true;
			}
			_nextHotspotIndex += 1;
			if (_nextHotspotIndex >= hotspots.Count)
				_nextHotspotIndex = 0;
			else if (_nextHotspotIndex < 0)
				_nextHotspotIndex = hotspots.Count - 1;

			var p = hotspots[_nextHotspotIndex];
			Log("gather ancient mana at [" + _nextHotspotIndex +"] " + p);
			GoToTask.ToPosition(p, 10, true, (c) => {
				var m = ObjectManager.GetNearestWoWGameObject(ObjectManager.GetWoWGameObjectByEntry(FarmList));
				if (m != null && m.IsValid)
				{
					Log("found ancient mana: " + mana.Name);
					return false;
				}
				return true;
			});
			return true;
		}
		public static List<Vector3> FarmSpotsVineyards = new List<Vector3>()
		{
			new Vector3(1233.43, 3098.469, 132.8995),
			new Vector3(1315.449, 3196.157, 132.878),
			new Vector3(1408.079, 3306.502, 132.6651),
			new Vector3(1473.269, 3424.817, 132.8173),
			new Vector3(1520.407, 3551.934, 136.5533),
			new Vector3(1460.808, 3653.376, 134.0804),
			new Vector3(1403.083, 3490.309, 133.6588),
		};
		public static List<int> FarmList = new List<int>()
		{
			// default
			252408, 252772, 252774, 260248, 260249,
			// vineyards
			260495, 260498, 260494, 260492, 252408, 252772, 252774, 260248, 260249,
		};
		public static List<Vector3> FarmSpots = new List<Vector3>()
		{
			new Vector3(1611.525, 4302.211, 135.5855),
			new Vector3(1758.481, 4791.606, 147.6049),
			new Vector3(1737.297, 4714.273, 170.1424),
			new Vector3(1305.939, 4830.756, 172.1508),
			new Vector3(1386.644, 4670.978, 128.5061),
			new Vector3(1331.427, 4492.01, 122.9617),
			new Vector3(1507.027, 4382.561, 115.4927),
			new Vector3(1703.431, 4317.868, 162.0318),
			new Vector3(1633.026, 4297.357, 138.98),
			new Vector3(1724.04, 4089.502, 166.0663),
			new Vector3(1415.358, 4424.396, 121.7797),
			new Vector3(1485.619, 4954.147, 187.0151),
			new Vector3(1565.029, 4973.608, 164.4833),
			new Vector3(1549.117, 5020.103, 124.2606),
			new Vector3(1522.021, 5000.249, 120.891),
			new Vector3(1459.936, 4998.865, 120.4602),
			new Vector3(1451.035, 5047.813, 174.2053),
			new Vector3(974.6285, 5288.748, 106.7754),
			new Vector3(953.0189, 5328.632, 90.89679),
			new Vector3(913.7977, 5294.123, 94.69218),
			new Vector3(1145.838, 5571.61, 1.450079),
			new Vector3(1180.608, 5628.356, 6.87799),
			new Vector3(1312.984, 5664.373, 2.771824),
			new Vector3(1571.934, 5553.792, 13.56783),
			new Vector3(1558.282, 5467.915, -7.36157),
			new Vector3(1543.184, 5478.198, -7.76466),
			new Vector3(1590.861, 5487.653, -7.360172),
			new Vector3(1588.765, 5611.566, 11.91419),
			new Vector3(1490.427, 5668.693, 16.13418),
			new Vector3(1811.907, 5701.867, 40.31356),
			new Vector3(1924.345, 5565.773, 60.70303),
			new Vector3(1619.317, 4887.14, 162.7641),
			new Vector3(1726.316, 4596.616, 166.6429),
			new Vector3(1386.644, 4670.978, 128.5061),
			new Vector3(1331.427, 4492.01, 122.9617),
			new Vector3(1507.027, 4382.561, 115.4927),
			new Vector3(1703.431, 4317.868, 162.0318),
			new Vector3(1633.026, 4297.357, 138.98),
			new Vector3(1724.04, 4089.502, 166.0663),
			new Vector3(1415.358, 4424.396, 121.7797),
			new Vector3(1485.619, 4954.147, 187.0151),
			new Vector3(1565.029, 4973.608, 164.4833),
			new Vector3(1549.117, 5020.103, 124.2606),
			new Vector3(1522.021, 5000.249, 120.891),
			new Vector3(1459.936, 4998.865, 120.4602),
			new Vector3(1451.035, 5047.813, 174.2053),
			new Vector3(974.6285, 5288.748, 106.7754),
			new Vector3(953.0189, 5328.632, 90.89679),
			new Vector3(1451.373, 4894.687, 185.5585),
			new Vector3(913.7977, 5294.123, 94.69218),
			new Vector3(1145.838, 5571.61, 1.450079),
			new Vector3(1180.608, 5628.356, 6.87799),
			new Vector3(1312.984, 5664.373, 2.771824),
			new Vector3(1571.934, 5553.792, 13.56783),
			new Vector3(1558.282, 5467.915, -7.36157),
			new Vector3(1543.184, 5478.198, -7.76466),
			new Vector3(1590.861, 5487.653, -7.360172),
			new Vector3(1588.765, 5611.566, 11.91419),
			new Vector3(1490.427, 5668.693, 16.13418),
			new Vector3(1811.907, 5701.867, 40.31356),
			new Vector3(1924.345, 5565.773, 60.70303),
			new Vector3(1619.317, 4887.14, 162.7641),
			new Vector3(1726.316, 4596.616, 166.6429),
		};
	}

	#region THIRST
	public static class Thirst
	{
		public static int NeedAncientMana = 50;
		public static bool Feed(Npc npc)
		{
			if (GatherAncientMana.NotCollected(NeedAncientMana))
				return true;

			Log("Goto and complete feed " + npc.Entry + " at " + npc.Position);
			if (GoToTask.ToPositionAndIntecractWithNpc(npc.Position, npc.Entry))
			{
				Thread.Sleep(Usefuls.Latency + 200);
				Questing.Gossip(1);
				Thread.Sleep(Usefuls.Latency + 200);
				Quest.CompleteQuest();
				Thread.Sleep(Usefuls.Latency + 200);
				Quest.CloseQuestWindow();
				Thread.Sleep(Usefuls.Latency + 200);
			}
			return true;
		}
		public static bool Thalyssra(QuestClass quest = null)
		{
			if (quest != null && Questing.Complete(quest))
			{
				return false;
			}
			if (IsThalyssraThirsty)
			{
				Log("Thalyssra is Thirsty. Go and feed");
				Feed(NPC.FIRST_ARCANIST_THALYSSRA);
				return true;
			}
			return false;
		}
		public static bool Valtrois(QuestClass quest = null)
		{
			if (quest != null && Questing.Complete(quest))
			{
				return false;
			}
			if (IsValtroisThirsty)
			{
				Log("Valtrois is Thirsty. Go and feed");
				Feed(NPC.ARCANIST_VALTROIS);
				return true;
			}
			return false;
		}
		public static bool Oculeth(QuestClass quest = null)
		{
			if (quest != null && Questing.Complete(quest))
			{
				return false;
			}
			if (IsOculethThirsty)
			{
				Log("Oculeth is Thirsty. Go and feed");
				Feed(NPC.CHIEF_TELEMANCER_OCULETH);
				return true;
			}
			return false;
		}
		public static bool IsThirsty(int friendId)
		{
			if (Items.TryUseArtifactPowerItems())
			{
				Thread.Sleep(Others.Random(100, 200));
			}

			List<string> values = Questing.GetFriendReputation(friendId);
			int count = values.Count;
			//Logging.Write("@@-=------>>> " +  + " <<< #" + values.Count + " ||| " + string.Join(",", values) );
			//if (values.Count == 0 || values.Count < 6) return false;

			int level = 0;
			if (count > 0)
				 int.TryParse(values[0], out level);

			int threshold = 0;
			if (count > 4)
				int.TryParse(values[4], out threshold);

			int newlevel = level - threshold;
			bool result = newlevel <= 1;
			Log("Thirst check. friend=" + friendId + " thirsty=" + result + " level=" + level + " threshold=" + threshold + " values=" + count);
			return result;
		}
		public static bool IsThalyssraThirsty
		{
			get
			{
				return IsThirsty(1860);
			}
		}
		public static bool IsOculethThirsty
		{
			get
			{
				return IsThirsty(1862);
			}
		}
		public static bool IsValtroisThirsty
		{
			get
			{
				return IsThirsty(1919);
			}
		}
	}
	#endregion THIRST

	#region LEYLINE
	public class LeylineQuest : QuestClass
	{
		public LeylineQuest()
		{
			Name = "Leyline Feed";
			QuestId.Add(0);
			Step.AddRange(new[] { 1, 0, 0, 0, 0 });
		}
		public override bool IsComplete()
		{
			return AncientMana >= 250;
		}
		public override bool PickUp()
		{
			if (GatherAncientMana.NotCollected(250))
				return true;

			return base.PickUp();
		}
		public override bool TurnIn()
		{
			if (GatherAncientMana.NotCollected(250))
				return true;

			return base.TurnIn();
		}
	}
	#endregion LEYLINE

	#region WHITERED
	public static class Whitered
	{
		public static bool Try(List<Vector3> hotspots, List<int> _mobs)
		{
			var castRange = 28f;
			var withered = ObjectManager.GetNearestWoWUnit(ObjectManager.GetWoWUnitByEntry(_mobs));
			if (withered != null && withered.IsValid && withered.IsAlive && withered.IsAttackable)
			{
				var wPos = withered.Position;
				var wBaseAddr = withered.GetBaseAddress;
				var wGUID = withered.Guid;
				var result = false;
				var path = PathFinder.FindPath(wPos, out result);
				if (!result)
				{
					Log("blaklist withered. no path");
					wManager.wManagerSetting.AddBlackList(wGUID, 3 * 60 * 1000);
					return false;
				}
				Interact.InteractGameObject(wBaseAddr, true, false, true);
				if (ObjectManager.Me.Position.DistanceTo(wPos) > castRange)
				{
					GoToTask.ToPosition(wPos, castRange);
					return true;
				}
				if (TraceLine.TraceLineGo(wPos))
				{
					Log("blaklist withered. no line of sight");
					wManager.wManagerSetting.AddBlackList(wGUID, 3 * 60 * 1000);
					return false;
				}
				Log("rescue withered");
				MountTask.DismountMount();
				MovementManager.StopMove();
				MovementManager.Face(withered);
				Questing.UseExtraButton();
				Thread.Sleep(Usefuls.Latency * 2);
				// this quest give aoe skill http://www.wowhead.com/quest=40125/branch-of-the-arcandor 
				ClickOnTerrain.Pulse(wPos);
				Thread.Sleep(Usefuls.Latency * 2);
				Usefuls.WaitIsCasting();
				return true;
			}
			var p = hotspots[Others.Random(0, hotspots.Count - 1)];
			GoToTask.ToPosition(p, 5f, false, (c) => {
				var w = ObjectManager.GetNearestWoWUnit(ObjectManager.GetWoWUnitByEntry(_mobs));
				if (w != null && w.IsValid)
				{
					Interact.InteractGameObject(w.GetBaseAddress, true, false, true);
					return false;
				}
				return true;
			});
			return true;
		}
	}
	#endregion WHITERED

	#region WARP
	public static class Warps
	{
		public class WarpNode
		{
			public Vector3 Position;
			public int Entry;
			public bool Use()
			{
				if (GoToTask.ToPositionAndIntecractWithGameObject(Position, Entry))
				{
					if (GoToTask.ToPosition(Position))
					{
						Questing.ClickMove(Position, 3.5f);
						Questing.Wait(1);
						return true;
					}
				}
				return false;
			}
		}
		public static WarpNode WORKSHOP_GARDENS = new WarpNode()
		{
			Entry = 245925,
			Position = Positions.WARP_WORKSHOP,
		};
		public static WarpNode WORKSHOP_TESTCHAMBER = new WarpNode()
		{
			Entry = 246237,
			Position = Positions.WARP_WORKSHOP,
		};
		public static WarpNode GARDENS_WARPLAB = new WarpNode()
		{
			Entry = 245927,
			Position = Positions.WARP_GARDEN,
		};
		public static WarpNode WARPLAB_WORKSHOP = new WarpNode()
		{
			Entry = 245928,
			Position = Positions.WARP_LAB,
		};
		public static WarpNode GARDENS_FOUNTAIN = new WarpNode()
		{
			Entry = 245954,
			Position = Positions.WARP_GARDEN,
		};
		public static WarpNode FOUNTAIN_TELEMLAB = new WarpNode()
		{
			Entry = 245947,
			Position = Positions.WARP_FOUNTAIN,
		};
		public static WarpNode TELEMLAB_FOUNTAIN = new WarpNode()
		{
			Entry = 245950,
			Position = Positions.WARP_TELEMLAB,
		};
		public static WarpNode FOUNTAIN_GARDENS = new WarpNode()
		{
			Entry = 245942,
			Position = Positions.WARP_FOUNTAIN,
		};
		public static WarpNode GARDENS_WORKSHOP = new WarpNode()
		{
			Entry = 245926,
			Position = Positions.WARP_GARDEN,
		};
		public static WarpNode TESTCHAMBER_WORKSHOP = new WarpNode()
		{
			Entry = 245928,
			Position = Positions.WARP_TESTCHAMBER,
		};
		public static List<Vector3> Coords = new List<Vector3>()
		{
			Positions.WARP_WORKSHOP,
			Positions.WARP_GARDEN,
			Positions.WARP_LAB,
			Positions.WARP_FOUNTAIN,
			Positions.WARP_TELEMLAB,
		};
		public static Vector3 Nearest
		{
			get
			{
				return Coords.OrderBy(p => p.DistanceTo2D(ObjectManager.Me.Position)).FirstOrDefault();
			}
		}
		public static bool NearWorkshop
		{
			get
			{
				return Nearest.DistanceTo2D(Positions.WARP_WORKSHOP) < 10;
			}
		}
		public static bool NearGardens
		{
			get
			{
				return Nearest.DistanceTo2D(Positions.WARP_GARDEN) < 10;
			}
		}
		public static bool NearWarpLab
		{
			get
			{
				return Nearest.DistanceTo2D(Positions.WARP_LAB) < 10;
			}
		}
		public static bool NearFountain
		{
			get
			{
				return Nearest.DistanceTo2D(Positions.WARP_FOUNTAIN) < 10;
			}
		}
		public static bool NearTelemetryLabs
		{
			get
			{
				return Nearest.DistanceTo2D(Positions.WARP_TELEMLAB) < 10;
			}
		}
	}
	#endregion WARP


}