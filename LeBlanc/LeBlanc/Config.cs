using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using LeBlanc.Helper;
using Color = System.Drawing.Color;

namespace LeBlanc
{
    internal class Config
    {
        public static readonly Color NotificationColor = Color.FromArgb(136, 207, 240);
        public static Menu LeBlanc;
        public static Orbwalking.Orbwalker Orbwalker;
        public class Configs
        {
            public static Menu TargetSelector;
            public static Menu Orbwalker;
            public static Menu Combo;
            public static Menu Harass;
            public static Menu LaneClear;
            public static Menu KillSteal;
            public static Menu Drawing;
            public static Menu AntiGapcloser;
            public static Menu Misc;
            public static Menu Flee;
        }
        public static void Init()
        {
            LeBlanc = new Menu("Apollo's " + ObjectManager.Player.ChampionName, "apollo.leblanc", true);

            //Orbwalker
            Configs.Orbwalker = new Menu("Orbwalker", "apollo.leblanc.orbwalker");
            LeBlanc.AddSubMenu(Configs.Orbwalker);
            Orbwalker = new Orbwalking.Orbwalker(Configs.Orbwalker);

            //Targetselector
            Configs.TargetSelector = new Menu("Target Selector", "apollo.leblanc.targettelector");
            TargetSelector.AddToMenu(Configs.TargetSelector);
            LeBlanc.AddSubMenu(Configs.TargetSelector);

            //Drawings
            Configs.Drawing = new Menu("Drawings", "apollo.leblanc.draw");

            //Combo
            Configs.Combo = new Menu("Combo", "apollo.leblanc.combo");
            {
                var q = new Menu("Q", "apollo.leblanc.combo.q");
                q.AddBool("combo.q.bool", "Use in Combo");
                Configs.Combo.AddSubMenu(q);

                var w = new Menu("W", "apollo.leblanc.combo.w");
                w.AddBool("combo.w.bool", "Use in Combo");
                w.AddHitChance("combo.w.pre");
                Configs.Combo.AddSubMenu(w);

                var e = new Menu("E", "apollo.leblanc.combo.e");
                e.AddBool("combo.e.bool", "Use in Combo");
                e.AddHitChance("combo.e.pre");
                Configs.Combo.AddSubMenu(e);

                var r = new Menu("R", "apollo.leblanc.combo.r");
                {
                    r.AddBool("combo.r.bool", "Use in Combo");

                    var rw = new Menu("W", "apollo.leblanc.combo.r.w");
                    rw.AddBool("combo.r.w.second", "Use Second R(W)");
                    rw.AddHitChance("combo.r.w.pre");
                    r.AddSubMenu(rw);

                    var re = new Menu("E", "apollo.leblanc.combo.r.e");
                    rw.AddBool("Use.E", "Use R(E)");
                    re.AddHitChance("combo.r.e.pre");
                    r.AddSubMenu(re);
                }
                Configs.Combo.AddSubMenu(r);

                Configs.Combo.AddBool("combo.ignite.bool", "Use Ignite");
            }

            //Harass
            Configs.Harass = new Menu("Harass", "apollo.leblanc.harass");
            {
                var q = new Menu("Q", "apollo.leblanc.harass.q");
                q.AddBool("harass.q.bool", "Use in Harass");
                Configs.Harass.AddSubMenu(q);

                var w = new Menu("W", "apollo.leblanc.harass.w");
                w.AddBool("harass.w.bool", "Use in Harass");
                w.AddBool("harass.w2.bool", "Use second W in Harass");
                w.AddHitChance("harass.w.pre", 3);
                Configs.Harass.AddSubMenu(w);

                var e = new Menu("E", "apollo.leblanc.harass.e");
                e.AddBool("harass.e.bool", "Use in Harass");
                e.AddHitChance("harass.e.pre", 3);
                Configs.Harass.AddSubMenu(e);

                Configs.Harass.AddSlider("harass.mana", "Minimum Mana%", 30);
                Configs.Harass.AddKeyBind("harass.key", "ToggleKey", "H", KeyBindType.Toggle);
            }

            //Laneclear
            Configs.LaneClear = new Menu("Laneclear", "apollo.leblanc.laneclear");
            {
                var q = new Menu("Q", "apollo.leblanc.laneclear.q");
                q.AddBool("laneclear.q.bool", "Use in Laneclear");
                Configs.LaneClear.AddSubMenu(q);

                var w = new Menu("W", "apollo.leblanc.laneclear.w");
                w.AddBool("laneclear.w.bool", "Use in Laneclear");
                w.AddBool("laneclear.w2.bool", "Use second W in Laneclear");
                w.AddSlider("laneclear.w.hit", "Minimum Hit", 3, 1, 10);
                Configs.LaneClear.AddSubMenu(w);

                Configs.LaneClear.AddSlider("laneclear.mana", "Minimum Mana%", 30);
            }

            //Killsteal
            Configs.KillSteal = new Menu("Killsteal", "apollo.leblanc.ks");
            {
                Configs.KillSteal.AddBool("ks.q.bool", "Use Q");
                Configs.KillSteal.AddBool("ks.w.bool", "Use W");
            }

            //AnitGapcloser
            Configs.AntiGapcloser = new Menu("AntiGapcloser", "apollo.leblanc.antigap");
            {
                Configs.AntiGapcloser.AddBool("antigapcloser.e.bool", "Use E");
                Configs.AntiGapcloser.AddBool("antigapcloser.r.bool", "Use R(E)");
            }
            //Flee
            Configs.Flee = new Menu("Flee", "apollo.leblanc.flee");
            {
                var w = new Menu("W", "apollo.leblanc.flee.w");
                w.AddBool("flee.w.bool", "Use in Flee");
                Configs.Flee.AddSubMenu(w);

                var e = new Menu("E", "apollo.leblanc.flee.e");
                e.AddBool("flee.e.bool", "Use in Flee");
                e.AddHitChance("flee.e.pre", 1);
                Configs.Flee.AddSubMenu(e);

                var r = new Menu("R", "apollo.leblanc.flee.r");
                r.AddBool("flee.r.bool", "Use in Flee R(W)");
                Configs.Flee.AddSubMenu(r);

                Configs.Flee.AddKeyBind("flee.key", "KeyBind", "A", KeyBindType.Press);
            }

            //Misc
            Configs.Misc = new Menu("Misc", "apollo.leblanc.misc");
            {
                var secondW = new Menu("Second W", "apollo.leblanc.misc.2w");
                secondW.AddBool("misc.2w.mouseover.bool", "Use if Mouse over");
                secondW.AddSlider("misc.2w.mouseover.width", "Mouse over zone width", 70, 1);
                Configs.Misc.AddSubMenu(secondW);

                var clone = new Menu("Clone", "apollo.leblanc.misc.clone");
                clone.AddBool("misc.clone.move.bool", "Use Clone");
                Configs.Misc.AddSubMenu(clone);
            }

            //PacketCast
            {
                LeBlanc.AddBool("packetcast", "PacketCast (Doesnt work)", false);
            }

            AssassinManager.Init();
            LeBlanc.AddSubMenu(Configs.Combo);
            LeBlanc.AddSubMenu(Configs.Harass);
            LeBlanc.AddSubMenu(Configs.LaneClear);
            LeBlanc.AddSubMenu(Configs.KillSteal);
            LeBlanc.AddSubMenu(Configs.AntiGapcloser);
            MenuHelper.AddInterrupter(new List<string>(new[] { "E", "R" }));
            LeBlanc.AddSubMenu(Configs.Flee);
            MenuHelper.DrawSpell(
                new List<Spell>(
                    new[] { Spells.Spell[SpellSlot.Q], Spells.Spell[SpellSlot.W], Spells.Spell[SpellSlot.E] }));
            DamageIndicator.Init(Damages.ComboDamage);
            LeBlanc.AddSubMenu(Configs.Misc);

            LeBlanc.AddToMainMenu();
        }

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
