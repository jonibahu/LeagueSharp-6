using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace Darius
{
    internal class Program
    {
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;
        private static Menu Menu;
        private static Orbwalking.Orbwalker Orbwalker; 

        private static Spell Q;
        private static Spell W;
        private static Spell E;
        private static Spell R;

        private static readonly SpellSlot IgniteSlot = Player.GetSpellSlot("SummonerDot");

        private static void Main()
        {
            CustomEvents.Game.OnGameLoad += OnLoad;
        }

        private static void OnLoad(EventArgs args)
        {
            Utils.ClearConsole();
            if (Player.ChampionName != "Darius") return;

            #region Spells
            Q = new Spell(SpellSlot.Q, 425);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 540);
            R = new Spell(SpellSlot.R, 460);

            E.SetSkillshot(0.3f, (float)(80 * Math.PI / 180), int.MaxValue, false, SkillshotType.SkillshotCone);
            #endregion

            #region Menu
            Menu = new Menu("Apollo's Darius", "Darius", true);

            TargetSelector.AddToMenu(Menu.SubMenu("Target Selector"));
            Orbwalker = new Orbwalking.Orbwalker(Menu.AddSubMenu(new Menu("Orbwalking", "Orbwalking")));

            Menu.SubMenu("Combo").AddItem(new MenuItem("UseQC", "Use Q").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseWC", "Use W").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseEC", "Use E").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseRC", "Use R").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseIgniteC", "Use Ignite").SetValue(true));

            Menu.SubMenu("Harass").AddItem(new MenuItem("UseQH", "Use Q").SetValue(true));
            Menu.SubMenu("Harass").AddItem(new MenuItem("UseWH", "Use W").SetValue(true));
            Menu.SubMenu("Harass").AddItem(new MenuItem("UseEH", "Use E").SetValue(true));
            Menu.SubMenu("Harass").AddItem(new MenuItem("ManaH", "Minimum Mana%").SetValue(new Slider(30)));

            Menu.SubMenu("LaneClear").AddItem(new MenuItem("UseQL", "Use Q").SetValue(true));
            Menu.SubMenu("LaneClear").AddItem(new MenuItem("HitQL", "Minimum Hit Q").SetValue(new Slider(3, 1, 10)));
            Menu.SubMenu("LaneClear").AddItem(new MenuItem("ManaL", "Minimum Mana%").SetValue(new Slider(30)));

            Menu.SubMenu("Misc").AddItem(new MenuItem("InterrupterE", "Use E as Interrupter").SetValue(true));

            Menu.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "Q Range").SetValue(new Circle(true, Color.AntiqueWhite)));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "E Range").SetValue(new Circle(true, Color.AntiqueWhite)));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("DrawR", "R Range").SetValue(new Circle(true, Color.AntiqueWhite)));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("CDDraw", "Draw CD").SetValue(new Circle(false, Color.DarkRed)));
            MenuItem drawComboDamageMenu = new MenuItem("DmgDraw", "Draw Combo Damage", true).SetValue(true);
            MenuItem drawFill = new MenuItem("DmgFillDraw", "Draw Combo Damage Fill", true).SetValue(new Circle(true, Color.FromArgb(90, 255, 169, 4)));
            Menu.SubMenu("Drawings").AddItem(drawComboDamageMenu);
            Menu.SubMenu("Drawings").AddItem(drawFill);
            DamageIndicator.DamageToUnit = ComboDamage;
            DamageIndicator.Enabled = drawComboDamageMenu.GetValue<bool>();
            DamageIndicator.Fill = drawFill.GetValue<Circle>().Active;
            DamageIndicator.FillColor = drawFill.GetValue<Circle>().Color;
            drawComboDamageMenu.ValueChanged +=
                delegate(object sender, OnValueChangeEventArgs eventArgs)
                {
                    DamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
                };
            drawFill.ValueChanged +=
                delegate(object sender, OnValueChangeEventArgs eventArgs)
                {
                    DamageIndicator.Fill = eventArgs.GetNewValue<Circle>().Active;
                    DamageIndicator.FillColor = eventArgs.GetNewValue<Circle>().Color;
                };

            Menu.AddToMainMenu();
            #endregion

            UpdateChecker.Init("Apollo16", "Darius");
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Interrupter2.OnInterruptableTarget += OnInterruptableTarget;
            Orbwalking.AfterAttack += AfterAttack;
            ShowNotification("Apollo's " + ObjectManager.Player.ChampionName + " Loaded", NotificationColor, 7000);
        }

        private static void OnUpdate(EventArgs args)
        {
            if (Player.IsDead || Player.IsRecalling())
                return;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (IgniteSlot != SpellSlot.Unknown && IgniteSlot.IsReady() && Menu.Item("UseIgniteC").GetValue<bool>())
                {
                    var t = TargetSelector.GetTarget(600, TargetSelector.DamageType.True);
                    if (t != null)
                        if (t.Health < ComboDamage(t))
                            Player.Spellbook.CastSpell(IgniteSlot, t);
                }

                if (E.IsReady() && Menu.Item("UseEC").GetValue<bool>())
                {
                    var t = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
                    if (t != null)
                        if (!Orbwalking.InAutoAttackRange(t))
                            E.CastIfHitchanceEquals(t, HitChance.High);
                }
                if (Q.IsReady() && Menu.Item("UseQC").GetValue<bool>())
                {
                    var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                    if (t != null)
                        Q.Cast();
                }
                if (R.IsReady() && Menu.Item("UseRC").GetValue<bool>())
                {
                    var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.True);
                    if (t != null)
                        if (t.Health < RDamage(t))
                            R.Cast(t);
                }
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                if (Player.ManaPercentage() < Menu.Item("ManaH").GetValue<Slider>().Value)
                    return;

                if (E.IsReady() && Menu.Item("UseEH").GetValue<bool>())
                {
                    var t = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
                    if (t != null)
                        if (!Orbwalking.InAutoAttackRange(t))
                            E.CastIfHitchanceEquals(t, HitChance.High);
                }
                if (Q.IsReady() && Menu.Item("UseQH").GetValue<bool>())
                {
                    var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                    if (t != null)
                        Q.Cast();
                }
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                if (Player.ManaPercentage() < Menu.Item("ManaL").GetValue<Slider>().Value)
                    return;

                if (Q.IsReady() && Menu.Item("UseQL").GetValue<bool>())
                {
                    var minionsQ = MinionManager.GetMinions(Q.Range).Count(h => h.Distance(Player) > 270) >=
                                   Menu.Item("HitQL").GetValue<Slider>().Value;
                    if (minionsQ)
                        Q.Cast();
                }
            }
        }

        private static void AfterAttack(AttackableUnit sender, AttackableUnit target)
        {
            if (!sender.IsMe || !W.IsReady() || !HeroManager.Enemies.Any(h => h.IsValidTarget(Orbwalking.GetRealAutoAttackRange(h))))
                return;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && Menu.Item("UseWC").GetValue<bool>())
                W.Cast();
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && Menu.Item("UseWH").GetValue<bool>())
                W.Cast();
        }

        private static void OnDraw(EventArgs args)
        {
            if (Player.IsDead || args == null)
                return;

            Spell[] spells = { Q, E, R };
            foreach (var spell in spells)
            {
                var menuItem = Menu.Item("Draw" + spell.Slot).GetValue<Circle>();
                var drawCd = Menu.Item("CDDraw").GetValue<Circle>();
                if (menuItem.Active && spell.Level > 0)
                {
                    Render.Circle.DrawCircle(Player.Position, spell.Range, (drawCd.Active && !spell.IsReady()) ? drawCd.Color : menuItem.Color);
                }
            }
        }

        private static void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!sender.IsValidTarget(E.Range) && args.DangerLevel < Interrupter2.DangerLevel.Medium &&
                Menu.Item("InterrupterE").GetValue<bool>())
                return;

            E.Cast(sender);
        }

        private static float RDamage(Obj_AI_Base enemy)
        {
            var baseDmg = new[] { 160, 250, 340 }[R.Level - 1];
            var additionalDmg = (new[] { 160, 250, 340 }[R.Level - 1] + (Player.TotalAttackDamage - Player.BaseAttackDamage)) * (0.2 * enemy.Buffs.Count(h => h.Name == "dariushemo"));
            var maxDmg = new[] { 320, 500, 680 }[R.Level - 1] + (Player.TotalAttackDamage - Player.BaseAttackDamage);
            return (float)Player.CalcDamage(enemy, Damage.DamageType.True, (baseDmg + additionalDmg) > maxDmg ? maxDmg : baseDmg + additionalDmg);
        }

        private static float ComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (Q.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);
            }

            if (W.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.W);
            }

            if (R.IsReady())
            {
                damage += RDamage(enemy);
            }

            if (IgniteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                damage += ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
            }

            return (float)damage;
        }

        public static readonly Color NotificationColor = Color.FromArgb(136, 207, 240);

        public static Notification ShowNotification(string message, Color color, int duration = -1, bool dispose = true)
        {
            var notif = new Notification(message).SetTextColor(color);
            Notifications.AddNotification(notif);

            if (dispose)
            {
                Utility.DelayAction.Add(duration, () => notif.Dispose());
            }

            return notif;
        }
    }
}
