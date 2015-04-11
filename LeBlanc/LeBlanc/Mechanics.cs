using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using LeBlanc.Helper;

namespace LeBlanc
{
    internal class Mechanics
    {
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;
        private static readonly Dictionary<SpellSlot, Spell> Spell = Spells.Spell;
        private static readonly bool PacketCast = Config.LeBlanc.GetBool("packetcast");
        public static readonly SpellSlot IgniteSlot = Player.GetSpellSlot("SummonerDot");

        public static void Init()
        {
            Game.OnUpdate += OnUpdate;
            AntiGapcloser.OnEnemyGapcloser += AnitGapcloser;
            Interrupter2.OnInterruptableTarget += OnPossibleToInterrupt;
        }
        private static void OnUpdate(EventArgs args)
        {
            if (Player.IsDead || Player.IsRecalling() || args == null)
                return;

            Console.WriteLine(Objects.Clone.Pet != null);

            if (!Config.LeBlanc.GetKeyBind("flee.key").Active)
                Killsteal();

            Misc();

            if (Config.LeBlanc.GetKeyBind("harass.key").Active)
                Harass();


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
                case Orbwalking.OrbwalkingMode.None:
                {
                    if (Config.LeBlanc.GetKeyBind("flee.key").Active)
                        Flee();
                    break;
                }
            }
        }

        private static void Combo()
        {
            var t = GetTarget(Spell[SpellSlot.E].Range, TargetSelector.DamageType.Magical);

            if (t == null)
                return;

            if (IgniteSlot != SpellSlot.Unknown && IgniteSlot.IsReady() && Config.LeBlanc.GetBool("combo.ignite.bool") &&
                t.Health < Damages.ComboDamage(t))
            {
                Player.Spellbook.CastSpell(IgniteSlot, t);
            }

            if (Spell[SpellSlot.R].IsReadyAndActive(MenuHelper.Mode.Combo))
            {
                if (Damages.R.W(t) > 2 * Damages.R.Q(t))
                {

                    if (Spell[SpellSlot.R].HasStatus(SpellSlot.W))
                    {
                        Cast.R.W(t, Spell[SpellSlot.R].GetHitChance(MenuHelper.Mode.Combo, "W"));
                    }
                    else if ((!Spell[SpellSlot.W].IsReady() || Spell[SpellSlot.W].IsSecond()))
                    {
                        if (((t.Health - Damages.ComboDamage(t)) / t.MaxHealth) >= .4 &&
                            Spell[SpellSlot.R].HasStatus(SpellSlot.E))
                        {
                            Cast.R.E(t, Spell[SpellSlot.R].GetHitChance(MenuHelper.Mode.Combo, "E"));
                        }
                        else if (Spell[SpellSlot.R].HasStatus(SpellSlot.Q))
                        {
                            Cast.R.Q(t);
                        }
                    }
                }
                else
                {
                    if (Spell[SpellSlot.R].HasStatus(SpellSlot.Q))
                    {
                        Cast.R.Q(t);
                    }
                    else if (!Spell[SpellSlot.Q].IsReady())
                    {
                        if (((t.Health - Damages.ComboDamage(t)) / t.MaxHealth) >= .4 && Spell[SpellSlot.R].HasStatus(SpellSlot.E))
                        {
                            Cast.R.E(t, Spell[SpellSlot.R].GetHitChance(MenuHelper.Mode.Combo, "E"));
                        }
                        else if (Spell[SpellSlot.R].HasStatus(SpellSlot.W))
                        {
                            Cast.R.W(t, Spell[SpellSlot.R].GetHitChance(MenuHelper.Mode.Combo, "W"));
                        }
                    }
                }
            }

            if (Spell[SpellSlot.Q].IsReadyAndActive(MenuHelper.Mode.Combo))
            {
                Cast.Q(t);
            }
            if (Spell[SpellSlot.W].IsReadyAndActive(MenuHelper.Mode.Combo) && !Spell[SpellSlot.W].IsSecond())
            {
                Cast.W(t, Spell[SpellSlot.W].GetHitChance(MenuHelper.Mode.Combo));
            }
            if (Spell[SpellSlot.E].IsReadyAndActive(MenuHelper.Mode.Combo))
            {
                Cast.E(t, Spell[SpellSlot.E].GetHitChance(MenuHelper.Mode.Combo));
            }
            if (Spell[SpellSlot.R].IsSecond() && Config.LeBlanc.GetBool("combo.r.w.second"))
            {
                Cast.R.W2();
            }
        }
        private static void Laneclear()
        {
            var minions = MinionManager.GetMinions(Player.ServerPosition, Spell[SpellSlot.Q].Range);
            var mana = Player.ManaPercent > Config.LeBlanc.GetSlider("laneclear.mana");

            if (Config.LeBlanc.GetBool("laneclear.w2.bool") && Spell[SpellSlot.W].IsSecond())
            {
                Cast.W2();
            }

            if (minions == null || !mana)
                return;

            if (Spell[SpellSlot.Q].IsReadyAndActive(MenuHelper.Mode.Laneclear))
            {
                var minionQ =
                    minions.Where(
                        h =>
                            h.Distance(Player) > Orbwalking.GetRealAutoAttackRange(h) &&
                            h.Health < Player.GetSpellDamage(h, SpellSlot.Q)).OrderBy(h => h.Health).FirstOrDefault();

                if (minionQ != null)
                    Spell[SpellSlot.Q].CastOnUnit(minionQ, PacketCast);
            }

            if (Spell[SpellSlot.W].IsReadyAndActive(MenuHelper.Mode.Laneclear) && !Spell[SpellSlot.W].IsSecond())
            {
                var farmLoc =
                    MinionManager.GetBestCircularFarmLocation(
                        minions.Select(m => m.ServerPosition.To2D()).ToList(), 100,
                        Spell[SpellSlot.W].Range);

                if (farmLoc.MinionsHit >= Config.LeBlanc.GetSlider("laneclear.w.hit"))
                    Spell[SpellSlot.W].Cast(farmLoc.Position, PacketCast);
            }
        }
        private static void Jungleclear()
        {
            var minion = MinionManager.GetMinions(
                Player.ServerPosition, Spell[SpellSlot.W].Range, MinionTypes.All, MinionTeam.Neutral,
                MinionOrderTypes.MaxHealth).FirstOrDefault();
            var mana = Player.ManaPercent > Config.LeBlanc.GetSlider("laneclear.mana");

            if (Config.LeBlanc.GetBool("laneclear.w2.bool") && Spell[SpellSlot.W].IsSecond())
            {
                Cast.W2();
            }

            if (minion == null || !mana)
                return;

            if (Spell[SpellSlot.Q].IsReadyAndActive(MenuHelper.Mode.Laneclear))
            {
                Cast.Q(minion);
            }
            if (Spell[SpellSlot.W].IsReadyAndActive(MenuHelper.Mode.Laneclear) && !Spell[SpellSlot.Q].IsReady() &&
                !Spell[SpellSlot.W].IsSecond())
            {
                Spell[SpellSlot.W].Cast(minion, PacketCast);
            }
        }
        private static void Harass()
        {
            var t = TargetSelector.GetTarget(Player, Spell[SpellSlot.E].Range, TargetSelector.DamageType.Magical);
            var mana = Player.ManaPercent > Config.LeBlanc.GetSlider("harass.mana");

            if (t == null && !mana)
                return;

            if (Spell[SpellSlot.Q].IsReadyAndActive(MenuHelper.Mode.Harass))
            {
                Cast.Q(t);
            }
            if (!Spell[SpellSlot.Q].IsReady() && Spell[SpellSlot.W].IsReadyAndActive(MenuHelper.Mode.Harass) &&
                !Spell[SpellSlot.W].IsSecond())
            {
                Cast.W(t, Spell[SpellSlot.W].GetHitChance(MenuHelper.Mode.Harass));
            }
            if (Spell[SpellSlot.E].IsReadyAndActive(MenuHelper.Mode.Harass))
            {
                Cast.E(t, Spell[SpellSlot.E].GetHitChance(MenuHelper.Mode.Harass));
            }
            if (Config.LeBlanc.GetBool("harass.w2.bool") && Spell[SpellSlot.W].IsSecond())
            {
                Cast.W2();
            }
        }
        private static void Flee()
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            
            var t =
                HeroManager.Enemies.Where(h => h.IsValidTarget(Spell[SpellSlot.E].Range))
                    .OrderBy(h => Player.Distance(h))
                    .FirstOrDefault();
            var pos = Player.ServerPosition.Extend(Game.CursorPos, Spell[SpellSlot.W].Range);

            if (t != null && Spell[SpellSlot.E].IsReadyAndActive(MenuHelper.Mode.Flee))
            {
                Cast.E(t, Spell[SpellSlot.E].GetHitChance(MenuHelper.Mode.Flee));
            }
            if (NavMesh.GetCollisionFlags(pos.X, pos.Y) != CollisionFlags.Wall &&
                NavMesh.GetCollisionFlags(pos.X, pos.Y) != CollisionFlags.Building)
            {
                if (Spell[SpellSlot.W].IsReadyAndActive(MenuHelper.Mode.Flee) && !Spell[SpellSlot.W].IsSecond())
                {
                    Spell[SpellSlot.W].Cast(pos, PacketCast);
                }
                if (Spell[SpellSlot.R].IsReadyAndActive(MenuHelper.Mode.Flee) && !Spell[SpellSlot.R].IsSecond() &&
                    Spell[SpellSlot.R].HasStatus(SpellSlot.W))
                {
                    Spell[SpellSlot.R].Cast(pos, PacketCast);
                }
            }
        }
        private static void Killsteal()
        {
            var t = HeroManager.Enemies.Where(h => h.IsValidTarget(Spell[SpellSlot.Q].Range)).ToList();

            if (t.Any())
            {
                if (Spell[SpellSlot.Q].IsReadyAndActive(MenuHelper.Mode.Ks) &&
                    Spell[SpellSlot.W].IsReadyAndActive(MenuHelper.Mode.Ks) && !Spell[SpellSlot.W].IsSecond())
                {
                    var tQw =
                        t.Where(
                            h =>
                                h.Health + 14 <
                                2 * Player.GetSpellDamage(h, SpellSlot.Q) + Player.GetSpellDamage(h, SpellSlot.W))
                            .OrderBy(h => TargetSelector.GetPriority(h))
                            .FirstOrDefault();
                    if (tQw != null && tQw.IsValid)
                    {
                        Cast.Q(tQw);
                        Cast.W(tQw, HitChance.Medium);
                    }
                }
                else if (Spell[SpellSlot.Q].IsReadyAndActive(MenuHelper.Mode.Ks))
                {
                    var tQ =
                        t.Where(h => h.Health + 14 < Player.GetSpellDamage(h, SpellSlot.Q))
                            .OrderBy(h => TargetSelector.GetPriority(h))
                            .FirstOrDefault();
                    if (tQ != null && tQ.IsValid)
                    {
                        Cast.Q(tQ);
                    }
                }
                else if (Spell[SpellSlot.W].IsReadyAndActive(MenuHelper.Mode.Ks) && !Spell[SpellSlot.W].IsSecond())
                {
                    var tW =
                        t.Where(h => h.Health + 14 < Player.GetSpellDamage(h, SpellSlot.W))
                            .OrderBy(h => TargetSelector.GetPriority(h))
                            .FirstOrDefault();
                    if (tW != null && tW.IsValid)
                    {
                        Cast.W(tW, HitChance.Medium);
                    }
                }
            }
        }
        private static void Misc()
        {
            if (Config.LeBlanc.GetBool("misc.2w.mouseover.bool") && Spell[SpellSlot.W].IsSecond() && Objects.SecondW.ExpireTime - Game.Time > 0.1)
            {
                var width = Config.LeBlanc.Item("apollo.leblanc.misc.2w.mouseover.width").GetValue<Slider>().Value;
                var exPos = Objects.SecondW.Object.Position.Extend(Game.CursorPos, width);
                var exPosDis = Objects.SecondW.Object.Position.Distance(exPos);
                var cursorDis = Objects.SecondW.Object.Position.Distance(Game.CursorPos);
                if (cursorDis < exPosDis)
                {
                    Cast.W2();
                }
            }

            var pet = Objects.Clone.Pet;
            if (Config.LeBlanc.GetBool("misc.clone.move.bool") && pet != null && pet.IsValid && !pet.IsDead &&
                pet.Health > 1)
            {
                var t =
                    HeroManager.Enemies.Where(h => h.IsValidTarget(1000))
                        .OrderBy(h => h.Distance(Player))
                        .FirstOrDefault();
                if (t != null && pet.CanAttack)
                    pet.IssueOrder(GameObjectOrder.AutoAttackPet, t);
            }
        }
        private static void OnPossibleToInterrupt(Obj_AI_Base unit, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (Spell[SpellSlot.E].IsInRange(unit))
            {
                if (Spell[SpellSlot.E].IsReadyAndActive(MenuHelper.Mode.Interrupter) &&
                    args.DangerLevel >= Config.LeBlanc.GetInterrupterDangerLevel(Spell[SpellSlot.E]))
                {
                    Cast.E(unit, HitChance.High);
                }
                if (Spell[SpellSlot.R].IsReadyAndActive(MenuHelper.Mode.Interrupter) &&
                    Spell[SpellSlot.R].HasStatus(SpellSlot.E) &&
                    args.DangerLevel >= Config.LeBlanc.GetInterrupterDangerLevel(Spell[SpellSlot.R]))
                {
                    Cast.R.E(unit, HitChance.High);
                }
            }
        }
        private static void AnitGapcloser(ActiveGapcloser gapcloser)
        {
            if (Spell[SpellSlot.E].IsInRange(gapcloser.Sender))
            {
                if (Spell[SpellSlot.E].IsReadyAndActive(MenuHelper.Mode.Antigapcloser))
                {
                    Cast.E(gapcloser.Sender, HitChance.High);
                }
                if (Spell[SpellSlot.R].IsReadyAndActive(MenuHelper.Mode.Antigapcloser) &&
                    Spell[SpellSlot.R].HasStatus(SpellSlot.E))
                {
                    Cast.R.E(gapcloser.Sender, HitChance.High);
                }
            }
        }
        private static Obj_AI_Hero GetTarget(float vDefaultRange = 0,
            TargetSelector.DamageType vDefaultDamageType = TargetSelector.DamageType.Physical)
        {
            if (Math.Abs(vDefaultRange) < 0.00001)
                vDefaultRange = Spell[SpellSlot.W].Range;

            if (!Config.LeBlanc.Item("AssassinActive").GetValue<bool>())
                return TargetSelector.GetTarget(vDefaultRange, vDefaultDamageType);

            var assassinRange = Config.LeBlanc.Item("AssassinSearchRange").GetValue<Slider>().Value;

            var vEnemy =
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(
                        enemy =>
                            enemy.Team != ObjectManager.Player.Team && !enemy.IsDead && enemy.IsVisible &&
                            Config.LeBlanc.Item("Assassin" + enemy.ChampionName) != null &&
                            Config.LeBlanc.Item("Assassin" + enemy.ChampionName).GetValue<bool>() &&
                            ObjectManager.Player.Distance(enemy) < assassinRange);

            if (Config.LeBlanc.Item("AssassinSelectOption").GetValue<StringList>().SelectedIndex == 1)
            {
                vEnemy = (from vEn in vEnemy select vEn).OrderByDescending(vEn => vEn.MaxHealth);
            }

            Obj_AI_Hero[] objAiHeroes = vEnemy as Obj_AI_Hero[] ?? vEnemy.ToArray();

            Obj_AI_Hero t = !objAiHeroes.Any()
                ? TargetSelector.GetTarget(vDefaultRange, vDefaultDamageType)
                : objAiHeroes[0];

            return t;
        }
    }
}
