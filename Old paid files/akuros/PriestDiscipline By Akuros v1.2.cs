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
using Timer = robotManager.Helpful.Timer;
using robotManager.Products;

#if VISUAL_STUDIO
namespace PriestDiscipline
{
#endif

	public class Main : ICustomClass
	{
		#region PARAMS
		public const string AUTHOR = "Akuros";
		public const string VER = "v.1.2";
		public const string NAME = "PriestDiscipline";
		public static float MaxRange = 38;
		public float Range { get { return MaxRange; } }
		bool _isLaunched = false;
		#endregion PARAMS

		#region SPELLS
		public class Spells
		{
			//healing
				public static Spell Plea = new Spell("Plea");
				public static Spell PowerWordRadiance = new Spell("Power Word: Radiance");
				public static Spell PowerWordShield = new Spell("Power Word: Shield");
				public static Spell ShadowMend = new Spell("Shadow Mend");
				public static Spell Rapture = new Spell("Rapture");
				public static Spell FlashHeal = new Spell("Flash Heal");
			
			//damage
				public static Spell Penance = new Spell("Penance");
				public static Spell ShadowWordPain = new Spell("Shadow Word: Pain");
				public static Spell Smite = new Spell("Smite");
				public static Spell Shadowfiend = new Spell("Shadowfiend");

			//utility
				public static Spell Levitate = new Spell("Levitate");
				public static Spell Fade = new Spell("Fade");
				public static Spell LeapofFaith = new Spell("Leap of Faith");
			public static Spell MindControl = new Spell("Mind Control");
			public static Spell MassDispel = new Spell("Mass Dispel");
			public static Spell DispelMagic = new Spell("Dispel Magic");
				public static Spell PainSuppression = new Spell("Pain Suppression");
				public static Spell PowerWordBarrier = new Spell("Power Word: Barrier");
				public static Spell Purify = new Spell("Purify");
			public static Spell ShackleUndead = new Spell("Shackle Undead");
				public static Spell PsychicScream = new Spell("Psychic Scream");
			//talents
				public static Spell Schism = new Spell("Schism");
			public static Spell AngelicFeather = new Spell("Angelic Feather");
				public static Spell ShiningForce = new Spell("Shining Force");
				public static Spell PowerWordSolace = new Spell("Power Word: Solace");
				public static Spell Mindbender = new Spell("Mindbender");
				public static Spell PowerInfusion = new Spell("Power Infusion", true);
				public static Spell ClarityofWill = new Spell("Clarity of Will");
				public static Spell DivineStar = new Spell("Divine Star");
				public static Spell Halo = new Spell("Halo");
				public static Spell PurgetheWicked = new Spell("Purge the Wicked");
			public static uint PurgetheWickedDebuffId = 204213;
				public static Spell ShadowCovenant = new Spell("Shadow Covenant");
			//honor talents
				public static Spell GladiatorsMedallion = new Spell("Gladiator's Medallion");
				public static Spell PowerWordFortitude = new Spell("Power Word: Fortitude");
				public static Spell Premonition = new Spell("Premonition");
				public static Spell Archangel = new Spell("Archangel");
				public static Spell DarkArchangel = new Spell("Dark Archangel");
			//artifact
				public static Spell LightsWrath = new Spell("Light's Wrath");
		}

		#endregion

		#region ROTATION
		void Rotation()
		{
			while (_isLaunched)
			{
				try
				{
					if (Me.IsCast || Me.IsMounted || Me.IsDead || !Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause)
					{
						continue;
					}

					//check target
					var targetAttackable = false;
					if (HaveTarget && MyTarget.IsAttackable && MyTarget.IsAlive)
					{
						targetAttackable = true;
						//no los
						if (MyTarget.GetDistance > 5 && TraceLine.TraceLineGo(MyTarget.Position))
						{
							targetAttackable = false;
						}
					}

					if (!Me.InCombat)
					{
						//cast something out of combat?

						//pull
						if (targetAttackable)
						{
							if (Cast(Spells.ShadowWordPain, true)) continue;
							if (Cast(Spells.PurgetheWicked, true)) continue;
							if (Cast(Spells.Schism, true)) continue;
							if (Cast(Spells.Penance, true)) continue;
							if (Cast(Spells.Smite, true)) continue;
						}
						//Log("not in combat");
						continue;
					}

					//find near players with hp% < 90
					var position = Me.Position;
					//var parties = Party.GetPartyHomeAndInstance()
					var party = MyParty;
					var friendly = ObjectManager.GetObjectWoWPlayer().Where(p => p != null && p.IsValid && p.Reaction >= wManager.Wow.Enums.Reaction.Friendly).ToList();
					/*
					Log("PARTY=" + party.Count
						+ " HOME=" + Party.GetPartyGUID(wManager.Wow.Enums.PartyCategoryType.LE_PARTY_CATEGORY_HOME).Count
						+ " INST=" + Party.GetParty(wManager.Wow.Enums.PartyCategoryType.LE_PARTY_CATEGORY_INSTANCE).Count
						+ " HOME&INTS=" + Party.GetPartyGUIDHomeAndInstance().Count
						+ " NUMBER=" + Party.GetPartyNumberPlayers()
						+ " UNITS=" + Party.GetPartyHomeAndInstance().Count
						+ " GROUP=" + Party.IsInGroup()
						+ " FRIENDLY=" + friendly.Count
						);
					//*/
					var partyNear = party.Where(m => m.IsAlive && m.Position.DistanceTo(position) < MaxRange && m.HealthPercent <= 90 && !TraceLine.TraceLineGo(m.Position))
						.OrderBy(m => m.HealthPercent).ToList();

					//enemies

					var enemies = ObjectManager.GetWoWUnitHostile()
						.Where(u => u != null && u.IsValid && u.IsAlive && u.IsAttackable && u.Position.DistanceTo(position) <= MaxRange).ToList();
					//&& u.InCombat && u.IsTargetingMeOrMyPetOrPartyMember 

					var closeEnemies = enemies.Where(e => e.GetDistance <= 8).OrderBy(e => e.GetDistance).ToList();

					//self
					if (Cast(Spells.PsychicScream, closeEnemies.Count > 0)) continue;
					if (CastOnSelf(Spells.PowerWordShield, !Me.HaveBuff(Spells.PowerWordShield.Id))) continue;
					if (CastOnSelf(Spells.Purify, CanPurify)) continue;
					if (Cast(Spells.GladiatorsMedallion, Me.IsStunned || Me.Confused || Me.Possessed || Me.Rooted || Me.Silenced || Me.Fleeing)) continue;
					if (CastOnSelf(Spells.ShadowMend, Me.HealthPercent < 50 && !Me.HaveBuff(Spells.ShadowMend.Id))) continue;
					if (CastOnSelf(Spells.PainSuppression, Me.HealthPercent < 55)) continue;
					if (CastOnSelf(Spells.Fade, Me.HealthPercent < 55)) continue;
					if (Cast(Spells.Rapture, Me.HealthPercent < 55)) continue;
					if (CastOnSelf(Spells.FlashHeal, Me.HealthPercent < 25)) continue;
					if (CastOnSelf(Spells.PowerWordFortitude, !Me.HaveBuff(Spells.PowerWordFortitude.Id))) continue;
					if (CastOnSelf(Spells.ClarityofWill, Me.HealthPercent > 90 && Me.HealthPercent < 100)) continue;
					if (CastOnSelf(Spells.Plea, Me.HealthPercent < 88)) continue;
					if (CastOnGround(Spells.PowerWordBarrier, Me.Position, Me.HealthPercent < 50)) continue;
					if (Cast(Spells.ShiningForce, Me.HealthPercent < 50 && enemies.Count > 1)) continue;

					//shield everyone if no shield
					var notShieldedMemeber = partyNear.Where(m => !m.HaveBuff(Spells.PowerWordShield.Id)).FirstOrDefault();
					if (notShieldedMemeber != null)
						if (CastOn(Spells.PowerWordShield, notShieldedMemeber.Name, true)) continue;

					//heal
					var woundedMember = partyNear.FirstOrDefault();
					if (woundedMember != null)
					{
						var partyNearWounded = party.Where(m => m.Position.DistanceTo(woundedMember.Position) < 8).ToList();

						if (CastOn(Spells.Premonition, woundedMember.Name, woundedMember.HealthPercent < 65 && woundedMember.GetDistance < 20)) continue;
						if (CastOn(Spells.PowerWordRadiance, woundedMember.Name, woundedMember.HealthPercent < 65 && partyNearWounded.Count > 2)) continue;
						if (CastOn(Spells.ClarityofWill, woundedMember.Name, woundedMember.HealthPercent > 90 && woundedMember.HealthPercent < 100)) continue;
						if (CastOn(Spells.ShadowMend, woundedMember.Name, woundedMember.HealthPercent < 65 && !woundedMember.HaveBuff(Spells.ShadowMend.Id))) continue;
						if (CastOn(Spells.Plea, woundedMember.Name, woundedMember.HealthPercent < 88)) continue;
						if (CastOn(Spells.ShiningForce, woundedMember.Name, woundedMember.HealthPercent < 35 && enemies.Count > 1)) continue;
						if (CastOn(Spells.LeapofFaith, woundedMember.Name, woundedMember.HealthPercent < 35 && woundedMember.GetDistance > 20)) continue;
						if (CastOn(Spells.Archangel, woundedMember.Name, woundedMember.HealthPercent < 60)) continue;
					}

					//aoe
					if (partyNear.Count > 2 && enemies.Count > 2)
					{
						if (Cast(Spells.Halo, true)) continue;
						if (Cast(Spells.DivineStar, true)) continue;
						if (CastOnGround(Spells.PowerWordBarrier, partyNear[0].Position, true)) continue;
						if (Cast(Spells.ShadowCovenant, true)) continue;
					}

					//debuffs
					/*
					var enemyNoPurgetheWicked = enemies.Where(e => !e.HaveBuff(Spells.PurgetheWickedDebuffId)).FirstOrDefault();
					if (enemyNoPurgetheWicked != null)
						if (CastOn(Spells.PurgetheWicked, enemyNoPurgetheWicked.Name, true)) continue;

					var enemyNoShadowWordPain = enemies.Where(e => !e.HaveBuff(Spells.ShadowWordPain.Id)).FirstOrDefault();
					if (enemyNoShadowWordPain != null)
						if (CastOn(Spells.ShadowWordPain, enemyNoShadowWordPain.Name, true)) continue;

					//*/

					//dps and boosts
					if (Cast(Spells.PowerInfusion, targetAttackable)) continue;
					if (Cast(Spells.DarkArchangel, targetAttackable)) continue;
					if (Cast(Spells.PurgetheWicked, targetAttackable && !MyTarget.HaveBuff(Spells.PurgetheWickedDebuffId))) continue;
					if (Cast(Spells.ShadowWordPain, targetAttackable && !MyTarget.HaveBuff(Spells.ShadowWordPain.Id))) continue;
					if (Cast(Spells.Penance, targetAttackable)) continue;
					if (Cast(Spells.Schism, targetAttackable)) continue;
					if (Cast(Spells.LightsWrath, targetAttackable)) continue;
					if (Cast(Spells.Mindbender, targetAttackable)) continue;
					if (Cast(Spells.Shadowfiend, targetAttackable)) continue;
					if (Cast(Spells.PowerWordSolace, targetAttackable)) continue;
					if (Cast(Spells.Smite, targetAttackable)) continue;

					//do not fall
					//if (CastOnSelf(Spells.Levitate, Me.IsFallingFar)) continue;
				}
				catch (Exception e)
				{
					//Error(e.Message);
				}
				Thread.Sleep(10);
			}
		}
		#endregion ROTATION


		#region USEFUL
		public static List<WoWUnit> MyParty
		{
			get
			{
				var party = new List<WoWUnit>();
				var guids = Party.GetPartyGUIDHomeAndInstance();
				foreach (MemoryRobot.Int128 guid in guids)
				{
					if (guid.IsNotZero())
					{
						if (guid != Me.Guid)
						{
							try
							{
								WoWObject objectByGuid = ObjectManager.GetObjectByGuid(guid);
								if (objectByGuid.IsValid)
								{
									if (objectByGuid.Type == wManager.Wow.Enums.WoWObjectType.Unit)
									{
										WoWUnit partyMember = new WoWUnit(objectByGuid.GetBaseAddress);
										party.Add(partyMember);
									}
									else if (objectByGuid.Type == wManager.Wow.Enums.WoWObjectType.Player)
									{
										WoWPlayer partyMember = new WoWPlayer(objectByGuid.GetBaseAddress);
										party.Add(partyMember);
									}
								}
							}
							catch (Exception ex)
							{
								Log("error: " + ex.Message);
							}
						}
					}
				}


				return party;
			}
			
		}

		public static WoWPlayer Me { get { return ObjectManager.Me; } }
		public static WoWUnit MyTarget { get { return ObjectManager.Target; } }
		public static uint MyMana { get { return Me.Mana; } }
		public static uint MyManaPercent { get { return Me.ManaPercentage; } }
		public static bool HaveTarget { get { return (ObjectManager.Target != null && ObjectManager.Target.IsValid) || ObjectManager.Target.IsTargetingMeOrMyPetOrPartyMember; } }
		public static DisciplinePriestSettings MyConfig { get { return DisciplinePriestSettings.CurrentSetting; } }
		public static bool CantCast(Spell spell, bool condition)
		{
			if (!condition)
				return true;
			if (!spell.KnownSpell)
				return true;
			if (!spell.IsSpellUsable)
				return true;
			if (SpellManager.GetSpellCooldownTimeLeft(spell.Name) > 0)
				return true;

			return false;
		}
		public static bool CanPurify
		{
			get
			{
				var lua = @"
for i=1,40 do
	local name, rank, icon, count, debuffAuraType = UnitAura('player', i, 'CANCELABLE|HARMFUL');
	if (name and debuffAuraType) and (debuffAuraType == 'Magic' or debuffAuraType == 'Disease') then
		return true
	end
end
return false
";
				var result = Lua.LuaDoString<bool>(lua);
				return result;
			}
		}
		public static bool CanCast(Spell spell, bool condition)
		{
			return !CantCast(spell, condition);
		}
		public static bool CastForced(Spell spell, bool condition)
		{
			if (!condition)
				return false;
			//if (!spell.KnownSpell)
			//	return false;
			if (!spell.IsSpellUsable)
				return false;
			if (SpellManager.GetSpellCooldownTimeLeft(spell.Name) > 0)
				return false;

			return true;
		}
		public static bool Cast(Spell spell, bool condition)
		{
			if (CantCast(spell, condition))
				return false;

			SpellManager.CastSpellByNameLUA(spell.NameInGame);
			Log(spell);
			return true;
		}
		public static bool CastOn(Spell spell, string unitName, bool condition)
		{
			if (CantCast(spell, condition))
				return false;

			SpellManager.CastSpellByNameOn(spell.NameInGame, unitName);
			Log(spell, "on '" + unitName+"'");
			return true;
		}
		public static bool CastOnSelf(Spell spell, bool condition)
		{
			return CastOn(spell, Me.Name, condition);
		}
		public static bool CastOnGround(Spell spell, Vector3 position, bool condition)
		{
			if (CantCast(spell, condition))
				return false;

			SpellManager.CastSpellByIDAndPosition(spell.Id, position);
			Log(spell);
			return true;
		}
		public static bool HaveBuff(string buff)
		{
			return ObjectManager.Me.HaveBuff(buff);
		}
		public static bool HaveDebuff(string debuff)
		{
			return ObjectManager.Me.Guid == ObjectManager.Target.BuffCastedBy(debuff);
		}
		#endregion USEFUL


		#region INTERFACE
		public void Initialize()
		{
			Log("STARTED");
			DisciplinePriestSettings.Load();
			_isLaunched = true;
			Rotation();
		}

		public void Dispose()
		{
			_isLaunched = false;
			Log("STOPED");
		}

		public void ShowConfiguration()
		{
			DisciplinePriestSettings.Load();
			DisciplinePriestSettings.CurrentSetting.ToForm();
			DisciplinePriestSettings.CurrentSetting.Save();
		}
		#endregion INTERFACE


		#region HELPERS
		static string _lastLog;
		public static void Log(string text)
		{
			if (_lastLog == text) return;

			_lastLog = text;
			Logging.WriteFight("[" + NAME + "] " + text);
		}
		public static void Log(Spell spell, string extra="")
		{
			Log(spell.Name + " [" + spell.NameInGame + "] " + extra);
		}
		public static void Error(string text)
		{
			Logging.WriteError("[" + NAME + "] " + text);
		}
		#endregion
	}

	#region SETTINGS
	[Serializable]
	public class DisciplinePriestSettings : Settings
	{
		public const string PREFIX = "PriestDiscipline";

		DisciplinePriestSettings()
		{
			ConfigWinForm(new System.Drawing.Point(400, 400), PREFIX + Translate.Get("Settings"));
		}

		public static DisciplinePriestSettings CurrentSetting { get; set; }

		public bool Save()
		{
			try
			{
				return Save(AdviserFilePathAndName("CustomClass-" + PREFIX, ObjectManager.Me.Name + "." + Usefuls.RealmName));
			}
			catch (Exception e)
			{
				Logging.WriteError(PREFIX + "Settings > Save(): " + e);
				return false;
			}
		}

		public static bool Load()
		{
			try
			{
				if (File.Exists(AdviserFilePathAndName("CustomClass-" + PREFIX, ObjectManager.Me.Name + "." + Usefuls.RealmName)))
				{
					CurrentSetting = Load<DisciplinePriestSettings>(AdviserFilePathAndName("CustomClass-" + PREFIX, ObjectManager.Me.Name + "." + Usefuls.RealmName));
					return true;
				}
				CurrentSetting = new DisciplinePriestSettings();
			}
			catch (Exception e)
			{
				Logging.WriteError(PREFIX + "Settings > Load(): " + e);
			}
			return false;
		}
	}
	#endregion SETTINGS

#if VISUAL_STUDIO
}
#endif
