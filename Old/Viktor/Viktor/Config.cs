using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace Viktor
{
    internal class Config
    {
        public static Menu ViktorConfig;
        public static Menu TargetSelectorMenu;
        public static Orbwalking.Orbwalker Orbwalker;

        public static readonly Color NotificationColor = Color.FromArgb(136, 207, 240);

        public static void Init()
        {
            ViktorConfig = new Menu("Apollo's " + ObjectManager.Player.ChampionName, "apollo.viktor", true);

            //Orbwalker
            ViktorConfig.AddSubMenu(new Menu("Orbwalker", "apollo.viktor.orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(ViktorConfig.SubMenu("apollo.viktor.orbwalker"));

            //Targetselector
            TargetSelectorMenu = new Menu("Target Selector", "apollo.viktor.targettelector");
            TargetSelector.AddToMenu(TargetSelectorMenu);
            ViktorConfig.AddSubMenu(TargetSelectorMenu);

            //Combo
            var combo = new Menu("Combo", "apollo.viktor.combo");
            {
                var q = new Menu("Q", "apollo.viktor.combo.q");
                q.AddItem(new MenuItem("apollo.viktor.combo.q.bool", "Use in Combo").SetValue(true));
                q.AddItem(new MenuItem("apollo.viktor.combo.q.aa", "Use only in AA range").SetValue(false));
                combo.AddSubMenu(q);

                var w = new Menu("W", "apollo.viktor.combo.w");
                w.AddItem(new MenuItem("apollo.viktor.combo.w.bool", "Use in Combo").SetValue(true));
                w.AddItem(new MenuItem("apollo.viktor.combo.w.stunned", "On stunned enemy").SetValue(true));
                w.AddItem(new MenuItem("apollo.viktor.combo.w.slow", "On slow enemy").SetValue(true));
                w.AddItem(new MenuItem("apollo.viktor.combo.w.hit", "Use if min hit").SetValue(new Slider(3, 1, 5)));
                combo.AddSubMenu(w);

                var e = new Menu("E", "apollo.viktor.combo.e");
                e.AddItem(new MenuItem("apollo.viktor.combo.e.bool", "Use in Combo").SetValue(true));
                e.AddItem(
                    new MenuItem("apollo.viktor.combo.e.pre", "E HitChance").SetValue(
                        new StringList((new[] { "Low", "Medium", "High", "Very High" }), 2)));
                combo.AddSubMenu(e);

                var r = new Menu("R", "apollo.viktor.combo.r");
                r.AddItem(new MenuItem("apollo.viktor.combo.r.bool", "Use in Combo").SetValue(true));
                r.AddItem(new MenuItem("apollo.viktor.combo.r.kill", "Use if enemy is killable").SetValue(true));
                r.AddItem(
                    new MenuItem("apollo.viktor.combo.r.minhp", "Dont ult if target has hp%").SetValue(new Slider(5)));
                r.AddItem(new MenuItem("apollo.viktor.combo.r.hit", "Use if min hit").SetValue(new Slider(3, 1, 5)));
                r.AddItem(new MenuItem("apollo.viktor.combo.r.autofollow", "Auto Follow").SetValue(true));
                r.AddItem(
                    new MenuItem("apollo.viktor.combo.r.delay", "Auto Follow click delay").SetValue(new Slider(200, 1,
                        1000)));
                combo.AddSubMenu(r);

                combo.AddItem(new MenuItem("apollo.viktor.combo.ignite.bool", "Use Ignite").SetValue(true));

                ViktorConfig.AddSubMenu(combo);
            }

            //Harass
            var harass = new Menu("Harass", "apollo.viktor.harass");
            {
                var q = new Menu("Q", "apollo.viktor.harass.q");
                q.AddItem(new MenuItem("apollo.viktor.harass.q.bool", "Use in Harass").SetValue(true));
                harass.AddSubMenu(q);

                var e = new Menu("E", "apollo.viktor.harass.e");
                e.AddItem(new MenuItem("apollo.viktor.harass.e.bool", "Use in Harass").SetValue(true));
                e.AddItem(
                    new MenuItem("apollo.viktor.harass.e.pre", "E HitChance").SetValue(
                        new StringList((new[] { "Low", "Medium", "High", "Very High" }), 3)));
                harass.AddSubMenu(e);

                harass.AddItem(
                    new MenuItem("apollo.viktor.harass.key", "Harass Toggle").SetValue(
                        new KeyBind('T', KeyBindType.Toggle)));
                harass.AddItem(new MenuItem("apollo.viktor.harass.mana", "Min Mana%").SetValue(new Slider(30)));

                ViktorConfig.AddSubMenu(harass);
            }

            //Laneclear
            var laneclear = new Menu("Laneclear", "apollo.viktor.laneclear");
            {
                var q = new Menu("Q", "apollo.viktor.laneclear.q");
                q.AddItem(new MenuItem("apollo.viktor.laneclear.q.bool", "Use in Laneclear").SetValue(false));
                q.AddItem(new MenuItem("apollo.viktor.laneclear.q.lasthit", "Use to Lasthit").SetValue(false));
                q.AddItem(new MenuItem("apollo.viktor.laneclear.q.canon", "Use to kill Canonminions").SetValue(true));
                laneclear.AddSubMenu(q);

                var e = new Menu("E", "apollo.viktor.laneclear.e");
                e.AddItem(new MenuItem("apollo.viktor.laneclear.e.bool", "Use in Laneclear").SetValue(true));
                e.AddItem(
                    new MenuItem("apollo.viktor.laneclear.e.hit", "Use if min hit").SetValue(new Slider(3, 1, 10)));
                laneclear.AddSubMenu(e);

                laneclear.AddItem(new MenuItem("apollo.viktor.laneclear.mana", "Min Mana%").SetValue(new Slider(30)));

                ViktorConfig.AddSubMenu(laneclear);
            }

            //Killsteal
            var killsteal = new Menu("Killsteal", "apollo.viktor.ks");
            {
                killsteal.AddItem(new MenuItem("apollo.viktor.ks.e.bool", "Use E").SetValue(true));

                ViktorConfig.AddSubMenu(killsteal);
            }

            //AntiGapcloser
            var gapcloser = new Menu("AntiGapcloser", "apollo.viktor.gapcloser");
            {
                gapcloser.AddItem(new MenuItem("apollo.viktor.gapcloser.w.bool", "Use W").SetValue(true));
                ViktorConfig.AddSubMenu(gapcloser);
            }

            //Interrupter
            var interrupter = new Menu("Interrupter", "apollo.viktor.interrupt");
            {
                interrupter.AddItem(new MenuItem("apollo.viktor.interrupt.w.bool", "Use W").SetValue(true));
                interrupter.AddItem(new MenuItem("apollo.viktor.interrupt.r.bool", "Use R").SetValue(false));

                ViktorConfig.AddSubMenu(interrupter);
            }

            //Drawings
            var draw = new Menu("Drawings", "apollo.viktor.draw");
            {
                draw.AddItem(
                    new MenuItem("apollo.viktor.draw.q", "Draw Q Range").SetValue(new Circle(true, Color.AntiqueWhite)));
                draw.AddItem(
                    new MenuItem("apollo.viktor.draw.w", "Draw W Range").SetValue(new Circle(true, Color.AntiqueWhite)));
                draw.AddItem(
                    new MenuItem("apollo.viktor.draw.e", "Draw E Range").SetValue(new Circle(true, Color.AntiqueWhite)));
                draw.AddItem(
                    new MenuItem("apollo.viktor.draw.r", "Draw R Range").SetValue(new Circle(true, Color.AntiqueWhite)));
                draw.AddItem(
                    new MenuItem("apollo.viktor.draw.cd", "Draw on CD").SetValue(new Circle(false, Color.DarkRed)));
                MenuItem drawComboDamageMenu = new MenuItem("apollo.viktor.draw.ind.bool", "Draw Combo Damage", true).SetValue(true);
                MenuItem drawFill = new MenuItem("apollo.viktor.draw.ind.fill", "Draw Combo Damage Fill", true).SetValue(new Circle(true, Color.FromArgb(90, 255, 169, 4)));
                draw.AddItem(drawComboDamageMenu);
                draw.AddItem(drawFill);
                DamageIndicator.DamageToUnit = Damages.ComboDmg;
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

                ViktorConfig.AddSubMenu(draw);
            }

            //PacketCast
            {
                ViktorConfig.AddItem(new MenuItem("apollo.viktor.packetcast", "PacketCast").SetValue(false));
            }

            ViktorConfig.AddToMainMenu();
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