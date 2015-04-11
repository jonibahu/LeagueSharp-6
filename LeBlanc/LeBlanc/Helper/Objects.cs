using System;
using LeagueSharp;

namespace LeBlanc.Helper
{
    internal class Objects
    {
        public static class SecondW
        {
            public static GameObject Object { get; set; }
            public static double ExpireTime { get; set; }
        }
        public static class SecondR
        {
            public static GameObject Object { get; set; }
            public static double ExpireTime { get; set; }
        }
        public static class Clone
        {
            public static Obj_AI_Base Pet { get; set; }
            public static double ExpireTime { get; set; }
        }

        public static void Init()
        {
            GameObject.OnCreate += OnCreateObject;
            GameObject.OnDelete += OnDeleteObject;
        }

        private static void OnCreateObject(GameObject sender, EventArgs args)
        {
            if (sender == null || !sender.IsValid)
                return;

            if (sender.Name.Contains("LeBlanc_Base_W_return_indicator.troy"))
            {
                SecondW.Object = sender;
                SecondW.ExpireTime = Game.Time + 4;
            }
            if (sender.Name.Contains("LeBlanc_Base_RW_return_indicator.troy"))
            {
                SecondR.Object = sender;
                SecondR.ExpireTime = Game.Time + 4;
            }
            if (sender.Name.Equals(ObjectManager.Player.Name)) //sender.Name.Contains("LeBlanc_Base_P_poof.troy")
            {
                Clone.Pet = sender as Obj_AI_Base;
                Clone.ExpireTime = Game.Time + 8;
            }
        }
        private static void OnDeleteObject(GameObject sender, EventArgs args)
        {
            if (sender == null || !sender.IsValid)
                return;

            if (sender.Name.Contains("LeBlanc_Base_W_return_indicator.troy"))
            {
                SecondW.Object = null;
            }
            if (sender.Name.Contains("LeBlanc_Base_RW_return_indicator.troy"))
            {
                SecondW.Object = null;
            }
            if (sender.Name.Equals(ObjectManager.Player.Name))
            {
                Clone.Pet = null;
            }

        }
    }
}
