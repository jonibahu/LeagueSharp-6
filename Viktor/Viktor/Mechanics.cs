using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace Viktor
{
    internal static class Mechanics
    {
        private static readonly Dictionary<SpellSlot, Spell> Spell = Spells.Spell;
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;
        private static readonly bool PacketCast = Config.ViktorConfig.Item("apollo.viktor.packetcast").GetValue<bool>();
        public static GameObject ChaosStorm;
        public static readonly SpellSlot IgniteSlot = Player.GetSpellSlot("SummonerDot");

        public static void Init()
        {
            Game.OnUpdate += OnUpdate;
            GameObject.OnCreate += OnCreate;
            GameObject.OnDelete += OnDelete;
            Interrupter2.OnInterruptableTarget += OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            Orbwalking.AfterAttack += AfterAttack;
        }

        private static void OnUpdate(EventArgs args)
        {
            if (args == null || Player.IsDead || Player.IsRecalling())
                return;

            AutoFollowR();
            KillSteal();

            if (Config.ViktorConfig.Item("apollo.viktor.harass.key").GetValue<KeyBind>().Active)
            {
                Harass();
            }

            switch (Config.Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                {
                    Combo();
                    break;
                }
                case Orbwalking.OrbwalkingMode.LaneClear:
                {
                    Laneclear();
                    Jungleclear();
                    break;
                }
                case Orbwalking.OrbwalkingMode.Mixed:
                {
                    Harass();
                    break;
                }
            }
        }

        private static void Combo()
        {
            var t = TargetSelector.GetTarget(
                Spell[SpellSlot.E].Range + Spells.ECastRange, TargetSelector.DamageType.Magical);
            var useQ = Config.ViktorConfig.Item("apollo.viktor.combo.q.bool").GetValue<bool>();
            var useW = Config.ViktorConfig.Item("apollo.viktor.combo.w.bool").GetValue<bool>();
            var useE = Config.ViktorConfig.Item("apollo.viktor.combo.e.bool").GetValue<bool>();
            var useR = Config.ViktorConfig.Item("apollo.viktor.combo.r.bool").GetValue<bool>();
            var preE =
                (HitChance)
                    (Config.ViktorConfig.Item("apollo.viktor.combo.e.pre").GetValue<StringList>().SelectedIndex + 3);

            if (t == null)
                return;

            if (IgniteSlot != SpellSlot.Unknown && IgniteSlot.IsReady() &&
                Config.ViktorConfig.Item("apollo.viktor.combo.ignite.bool").GetValue<bool>() &&
                t.Health < Damages.ComboDmg(t))
            {
                Player.Spellbook.CastSpell(IgniteSlot, t);
            }

            if (useQ)
            {
                CastQ(t);
            }
            if (useW)
            {
                CastW();
            }
            if (useE)
            {
                CastE(t, preE);
            }
            if (useR)
            {
                CastR(t);
            }
        }

        private static void Harass()
        {
            var t = TargetSelector.GetTarget(1025, TargetSelector.DamageType.Magical);
            var mana = Config.ViktorConfig.Item("apollo.viktor.harass.mana").GetValue<Slider>().Value;
            var useQ = Config.ViktorConfig.Item("apollo.viktor.harass.q.bool").GetValue<bool>();
            var useE = Config.ViktorConfig.Item("apollo.viktor.harass.e.bool").GetValue<bool>();
            var preE =
                (HitChance)
                    (Config.ViktorConfig.Item("apollo.viktor.harass.e.pre").GetValue<StringList>().SelectedIndex + 3);

            if (mana > Player.ManaPercent)
            {
                return;
            }

            if (useQ)
            {
                CastQ(t);
            }
            if (useE)
            {
                CastE(t, preE);
            }
        }

        private static void Laneclear()
        {
            var mana = Player.ManaPercent > Config.ViktorConfig.Item("apollo.viktor.laneclear.mana").GetValue<Slider>().Value;
            var useE = Config.ViktorConfig.Item("apollo.viktor.laneclear.e.bool").GetValue<bool>();
            var minhitE = Config.ViktorConfig.Item("apollo.viktor.laneclear.e.hit").GetValue<Slider>().Value;

            if (!mana)
                return;

            if (useE && Spell[SpellSlot.E].IsReady())
            {
                var dic = new Dictionary<MinionManager.FarmLocation, Obj_AI_Base>();
                var list = new List<MinionManager.FarmLocation>();
                foreach (var minion in MinionManager.GetMinions(Player.ServerPosition, Spells.ECastRange))
                {
                    var farmLocation =
                        MinionManager.GetBestLineFarmLocation(
                            MinionManager.GetMinionsPredictedPositions(
                                MinionManager.GetMinions(minion.ServerPosition, Spell[SpellSlot.E].Range),
                                Spell[SpellSlot.E].Delay, Spell[SpellSlot.E].Width, Spell[SpellSlot.E].Speed,
                                minion.ServerPosition, Spell[SpellSlot.E].Range, false, SkillshotType.SkillshotLine),
                            Spell[SpellSlot.E].Width, Spell[SpellSlot.E].Range);

                    if (farmLocation.MinionsHit >= minhitE)
                    {
                        dic.Add(farmLocation, minion);
                        list.Add(farmLocation);
                    }
                }

                if (list.Any())
                {
                    var farmloc = list.OrderBy(h => h.MinionsHit).FirstOrDefault();
                    var minion = dic[farmloc];
                    Spell[SpellSlot.E].Cast(minion.ServerPosition, farmloc.Position.To3D());
                }

                dic.Clear();
                list.Clear();
            }
        }

        private static void Jungleclear()
        {
            var minions = MinionManager.GetMinions(
                Player.ServerPosition, Spell[SpellSlot.E].Range + Spells.ECastRange, MinionTypes.All, MinionTeam.Neutral,
                MinionOrderTypes.MaxHealth);
            var mana = Player.ManaPercent > Config.ViktorConfig.Item("apollo.viktor.laneclear.mana").GetValue<Slider>().Value;

            if (minions == null || !mana)
                return;

            var useQ = Config.ViktorConfig.Item("apollo.viktor.laneclear.q.bool").GetValue<bool>();
            var useE = Config.ViktorConfig.Item("apollo.viktor.laneclear.e.bool").GetValue<bool>();
            var minionsQ = minions.Where(h => h.IsValidTarget(Spell[SpellSlot.Q].Range)).OrderBy(h => h.MaxHealth).FirstOrDefault();

            if (useQ && Spell[SpellSlot.Q].IsReady() && minionsQ != null)
            {
                Spell[SpellSlot.Q].CastOnUnit(minionsQ, PacketCast);
            }

            if (useE && Spell[SpellSlot.E].IsReady())
            {
                var minionE = minions.FirstOrDefault();
                if (minionE != null)
                {
                    if (Player.Distance(minionE) < Spells.ECastRange)
                    {
                        var sourcePosition = minionE.ServerPosition;
                        Spell[SpellSlot.E].UpdateSourcePosition(sourcePosition, sourcePosition);
                        var lineFarm =
                            MinionManager.GetBestLineFarmLocation(
                                minions.Select(m => m.ServerPosition.To2D()).ToList(), Spell[SpellSlot.E].Width,
                                Spell[SpellSlot.E].Range);
                        Spell[SpellSlot.E].Cast(sourcePosition, lineFarm.Position.To3D());
                    }
                    else if (Player.Distance(minionE) < Spells.ECastRange + Spell[SpellSlot.E].Range)
                    {
                        var sourcePosition = Player.ServerPosition.Extend(minionE.ServerPosition, Spells.ECastRange);
                        Spell[SpellSlot.E].UpdateSourcePosition(sourcePosition, sourcePosition);
                        var lineFarm =
                            MinionManager.GetBestLineFarmLocation(
                                minions.Select(m => m.ServerPosition.To2D()).ToList(), Spell[SpellSlot.E].Width,
                                Spell[SpellSlot.E].Range);
                        Spell[SpellSlot.E].Cast(sourcePosition, lineFarm.Position.To3D());
                    }
                }
            }
        }

        private static void CastQ(Obj_AI_Base t)
        {
            if (!Spell[SpellSlot.Q].IsReady() || t == null || !Spell[SpellSlot.Q].IsInRange(t))
            {
                return;
            }
            if (Player.Distance(t) < Player.AttackRange)
                Spell[SpellSlot.Q].Cast(t, PacketCast);
            else if (!Config.ViktorConfig.Item("apollo.viktor.combo.q.aa").GetValue<bool>())
                Spell[SpellSlot.Q].Cast(t, PacketCast);
        }

        private static void CastW()
        {
            if (!Spell[SpellSlot.W].IsReady())
            {
                return;
            }

            foreach (var Object in
                ObjectManager.Get<Obj_AI_Base>()
                    .Where(
                        h =>
                            h.Distance(Player.ServerPosition) < Spell[SpellSlot.W].Range && h.Team != Player.Team &&
                            (h.HasBuff("teleport_target", true) || h.HasBuff("Pantheon_GrandSkyfall_Jump", true))))
            {
                Spell[SpellSlot.W].Cast(Object.Position, true);
            }

            foreach (
                var enemy in
                    ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget(Spell[SpellSlot.W].Range)))
            {
                if (enemy.HasBuff("rocketgrab2"))
                {
                    foreach (var ally in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (ally.BaseSkinName == "Blitzcrank" &&
                            ObjectManager.Player.Distance(ally.Position) < Spell[SpellSlot.W].Range)
                            Spell[SpellSlot.W].Cast(ally, true);
                    }

                }
                else if (enemy.HasBuffOfType(BuffType.Stun) || enemy.HasBuffOfType(BuffType.Snare) ||
                         enemy.HasBuffOfType(BuffType.Charm) || enemy.HasBuffOfType(BuffType.Fear) ||
                         enemy.HasBuffOfType(BuffType.Taunt) || enemy.HasBuffOfType(BuffType.Suppression) ||
                         enemy.IsStunned || enemy.HasBuff("Recall"))
                    Spell[SpellSlot.W].Cast(enemy, true);
                else
                    Spell[SpellSlot.W].CastIfHitchanceEquals(enemy, HitChance.Immobile, true);
            }

            var t = TargetSelector.GetTarget(Spell[SpellSlot.W].Range, TargetSelector.DamageType.Magical);
            if (t != null)
            {
                if (Config.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo &&
                    t.IsValidTarget(500) && t.Path.Count() < 2)
                {
                    if (t.HasBuffOfType(BuffType.Slow))
                        Spell[SpellSlot.W].CastIfHitchanceEquals(t, HitChance.VeryHigh, true);
                    else if (t.CountEnemiesInRange(250) > 2)
                        Spell[SpellSlot.W].CastIfHitchanceEquals(t, HitChance.VeryHigh, true);
                }

                var waypoints = t.GetWaypoints();
                if (t.Path.Count() < 2 &&
                    (t.ServerPosition.Distance(waypoints.Last().To3D()) > 500 ||
                     Math.Abs(
                         (ObjectManager.Player.Distance(waypoints.Last().To3D()) -
                          ObjectManager.Player.Distance(t.Position))) > 400 || t.Path.Count() == 0))
                {
                    Spell[SpellSlot.W].CastIfHitchanceEquals(t, HitChance.VeryHigh, true);
                }
            }
        }

        private static void CastE(Obj_AI_Base t, HitChance hit)
        {
            if (t == null)
                return;
            

            if (Player.Distance(t.ServerPosition) < Spells.ECastRange && Spell[SpellSlot.E].IsReady())
            {
                var sourcePosition = t.ServerPosition;
                Spell[SpellSlot.E].UpdateSourcePosition(sourcePosition, sourcePosition);
                var preE = Spell[SpellSlot.E].GetPrediction(t, true);
                if (preE.Hitchance >= hit)
                {
                    Spell[SpellSlot.E].Cast(sourcePosition, preE.CastPosition);
                }
            }
            else if (Player.Distance(t.ServerPosition) < Spells.ECastRange + Spell[SpellSlot.E].Range &&
                     Spell[SpellSlot.E].IsReady())
            {
                var sourcePosition = Player.ServerPosition.Extend(t.ServerPosition, Spells.ECastRange);
                Spell[SpellSlot.E].UpdateSourcePosition(sourcePosition, sourcePosition);
                var preE = Spell[SpellSlot.E].GetPrediction(t, true);
                if (preE.Hitchance >= hit)
                {
                    Spell[SpellSlot.E].Cast(sourcePosition, preE.CastPosition);
                }
            }
        }

        private static void CastR(Obj_AI_Base t)
        {
            if (t == null || !Spell[SpellSlot.R].IsReady() || Spell[SpellSlot.R].Instance.Name != "ViktorChaosStorm" || !t.IsValidTarget(Spell[SpellSlot.R].Range))
            {
                return;
            }

            var preR = Spell[SpellSlot.R].GetPrediction(t, true);
            if (t.IsValidTarget(Spell[SpellSlot.R].Range) &&
                Config.ViktorConfig.Item("apollo.viktor.combo.r.kill").GetValue<bool>() &&
                Damages.ComboDmg(t) > t.Health &&
                t.HealthPercent > Config.ViktorConfig.Item("apollo.viktor.combo.r.minhp").GetValue<Slider>().Value)
            {
                Spell[SpellSlot.R].Cast(t, PacketCast, true);
            }
            else if (preR.CastPosition.CountEnemiesInRange(Spell[SpellSlot.R].Width) >=
                     Config.ViktorConfig.Item("apollo.viktor.combo.r.hit").GetValue<Slider>().Value)
            {
                Spell[SpellSlot.R].Cast(preR.CastPosition, PacketCast);
            }
        }

        private static void AutoFollowR()
        {
            if (Spell[SpellSlot.R].Instance.Name != "ViktorChaosStorm" && Config.ViktorConfig.Item("apollo.viktor.combo.r.autofollow").GetValue<bool>())
            {
                var stormT = TargetSelector.GetTarget(
                    Player, 1100, TargetSelector.DamageType.Magical);
                if (stormT != null)
                    Utility.DelayAction.Add(
                        Config.ViktorConfig.Item("apollo.viktor.combo.r.delay").GetValue<Slider>().Value,
                        () => Spell[SpellSlot.R].Cast(stormT.ServerPosition));
            }
        }

        private static void KillSteal()
        {
            var useE = Config.ViktorConfig.Item("apollo.viktor.ks.e.bool").GetValue<bool>();
            var t =
                HeroManager.Enemies.Where(
                    h =>
                        h.IsValidTarget(Spells.ECastRange + Spell[SpellSlot.E].Range) &&
                        h.Health + 15 < Player.GetSpellDamage(h, SpellSlot.E))
                    .OrderBy(h => h.Health)
                    .FirstOrDefault(h => h.Health > 15);

            if (t == null)
                return;

            if (useE && Spell[SpellSlot.E].IsReady())
            {
                if (Player.Distance(t) < Spells.ECastRange)
                {
                    var sourcePosition = t.ServerPosition;
                    Spell[SpellSlot.E].UpdateSourcePosition(sourcePosition, sourcePosition);
                    var preE = Spell[SpellSlot.E].GetPrediction(t, true);

                    Spell[SpellSlot.E].Cast(sourcePosition, preE.CastPosition);
                }
                else
                {
                    var sourcePosition = Player.ServerPosition.Extend(t.ServerPosition, Spells.ECastRange);
                    Spell[SpellSlot.E].UpdateSourcePosition(sourcePosition, sourcePosition);
                    var preE = Spell[SpellSlot.E].GetPrediction(t, true);

                    Spell[SpellSlot.E].Cast(sourcePosition, preE.CastPosition);
                }
            }
        }

        private static void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Config.ViktorConfig.Item("apollo.viktor.gapcloser.w.bool").GetValue<bool>() &&
                Player.Distance(gapcloser.Sender) < Orbwalking.GetRealAutoAttackRange(Player) &&
                Spell[SpellSlot.W].IsReady())
            {
                Spell[SpellSlot.W].Cast(gapcloser.End, PacketCast);
            }
        }

        private static void OnInterruptableTarget(Obj_AI_Hero unit, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (args.DangerLevel >= Interrupter2.DangerLevel.High)
            {
                var useW = Config.ViktorConfig.Item("apollo.viktor.interrupt.w.bool").GetValue<bool>();
                var useR = Config.ViktorConfig.Item("apollo.viktor.interrupt.r.bool").GetValue<bool>();

                if (useW && Spell[SpellSlot.W].IsReady() && unit.IsValidTarget(Spell[SpellSlot.W].Range) &&
                    (Game.Time + 1.5 + Spell[SpellSlot.W].Delay) >= args.EndTime)
                {
                    Spell[SpellSlot.W].Cast(unit.ServerPosition, PacketCast);
                }
                else if (useR && unit.IsValidTarget(Spell[SpellSlot.R].Range) && Spell[SpellSlot.R].Instance.Name == "ViktorChaosStorm")
                {
                    Spell[SpellSlot.R].Cast(unit.ServerPosition, PacketCast);
                }
            }
        }

        private static void OnCreate(GameObject sender, EventArgs args)
        {
            if (!sender.IsValid)
            {
                return;
            }

            if (sender.Name.Contains("Viktor_Base_R_Droid.troy"))
            {
                ChaosStorm = sender;
            }
        }

        private static void OnDelete(GameObject sender, EventArgs args)
        {
            if (!sender.IsValid)
            {
                return;
            }

            if (sender.Name.Contains("Viktor_Base_R_Droid.troy"))
            {
                ChaosStorm = null;
            }
        }

        private static void AfterAttack(AttackableUnit unit, AttackableUnit unit2)
        {
            var minions = MinionManager.GetMinions(Player.ServerPosition, Spell[SpellSlot.Q].Range);
            var useQ = Config.ViktorConfig.Item("apollo.viktor.laneclear.q.bool").GetValue<bool>();
            var lastHitQ = Config.ViktorConfig.Item("apollo.viktor.laneclear.q.lasthit").GetValue<bool>();
            var lastHitCanonQ = Config.ViktorConfig.Item("apollo.viktor.laneclear.q.canon").GetValue<bool>();
            var mana = Player.ManaPercent > Config.ViktorConfig.Item("apollo.viktor.laneclear.mana").GetValue<Slider>().Value;
            if (Config.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.LaneClear || !mana || minions == null || !Spell[SpellSlot.Q].IsReady() || !useQ)
                return;

            var minionLasthit =
                minions.Where(
                    h =>
                        HealthPrediction.GetHealthPrediction(
                            h, (int)(Player.Distance(h) / Spell[SpellSlot.Q].Speed),
                            (int) (Spell[SpellSlot.Q].Delay * 1000 + Game.Ping / 2f)) <
                        Player.GetSpellDamage(h, SpellSlot.Q) &&
                        HealthPrediction.GetHealthPrediction(
                            h, (int)(Player.Distance(h) / Spell[SpellSlot.Q].Speed),
                            (int) (Spell[SpellSlot.Q].Delay * 1000 + Game.Ping / 2f)) > 0)
                    .OrderBy(h => h.Health)
                    .FirstOrDefault();
            if (lastHitQ && minionLasthit != null)
            {
                Spell[SpellSlot.Q].CastOnUnit(minionLasthit, PacketCast);
            }

            var canonLasthit =
                minions.Where(
                    h =>
                        h.BaseSkinName.Contains("Siege") &&
                        HealthPrediction.GetHealthPrediction(
                            h, (int)(Player.Distance(h) / Spell[SpellSlot.Q].Speed),
                            (int) (Spell[SpellSlot.Q].Delay * 1000 + Game.Ping / 2f)) <
                        Player.GetSpellDamage(h, SpellSlot.Q) &&
                        HealthPrediction.GetHealthPrediction(
                            h, (int)(Player.Distance(h) / Spell[SpellSlot.Q].Speed),
                            (int) (Spell[SpellSlot.Q].Delay * 1000 + Game.Ping / 2f)) > 0)
                    .OrderBy(h => h.Health)
                    .FirstOrDefault();
            if (lastHitCanonQ && canonLasthit != null)
            {
                Spell[SpellSlot.Q].CastOnUnit(canonLasthit, PacketCast);
            }
        }
    }
}