using System;
using Color = System.Drawing.Color;
using LeagueSharp;
using LeagueSharp.Common;
using LeBlanc.Helper;

namespace LeBlanc
{
    internal class Drawings
    {
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;

        public static void Init()
        {
            Drawing.OnDraw += OnDraw;
        }

        private static void OnDraw(EventArgs args)
        {
            if (args == null || Player.IsDead)
                return;

            if (Objects.SecondW.Object != null)
            {
                var width = Config.LeBlanc.Item("apollo.leblanc.misc.2w.mouseover.width").GetValue<Slider>().Value;
                var wts = Drawing.WorldToScreen(Player.Position);
                var timer = (Objects.SecondW.ExpireTime - Game.Time > 0) ? (Objects.SecondW.ExpireTime - Game.Time) : 0;

                Drawing.DrawText(wts.X - 35, wts.Y + 10, Color.White, "Second W: " + timer.ToString("0.0"));
                Render.Circle.DrawCircle(Objects.SecondW.Object.Position, 100, Color.Red, width);
            }
            if (Objects.SecondR.Object != null)
            {
                var width = Config.LeBlanc.Item("apollo.leblanc.misc.2w.mouseover.width").GetValue<Slider>().Value;
                var wts = Drawing.WorldToScreen(Player.Position);
                var timer = (Objects.SecondR.ExpireTime - Game.Time > 0) ? (Objects.SecondR.ExpireTime - Game.Time) : 0;

                Drawing.DrawText(wts.X - 35, wts.Y + 10, Color.White, "Second R: " + timer.ToString("0.0"));
                Render.Circle.DrawCircle(Objects.SecondR.Object.Position, 100, Color.Purple, width);
            }
            if (Objects.Clone.Pet != null)
            {
                var wts = Drawing.WorldToScreen(Objects.Clone.Pet.ServerPosition);
                var timer = (Objects.Clone.ExpireTime - Game.Time > 0) ? (Objects.Clone.ExpireTime - Game.Time) : 0;

                Drawing.DrawText(wts.X - 35, wts.Y + 10, Color.White, "Clone: " + timer.ToString("0.0"));
            }
            if (Config.LeBlanc.GetKeyBind("harass.key").Active)
            {
                var wts = Drawing.WorldToScreen(Player.ServerPosition);
                Drawing.DrawText(wts.X - 35, wts.Y + 10, Color.Red, "Auto Harass: Active");
            }
        }
    }
}
