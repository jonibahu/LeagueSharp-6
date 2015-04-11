using LeagueSharp;
using LeagueSharp.Common;

namespace LeBlanc
{
    internal class Loader
    {
        static void Main(string[] args)
        {
            if (args != null)
            {
                CustomEvents.Game.OnGameLoad += eventArgs =>
                {
                    if (ObjectManager.Player.ChampionName != "Leblanc")
                        return;
                    Utils.ClearConsole();
                    //todo Longrange w
                    Helper.UpdateChecker.Init("Apollo16", "LeBlanc");
                    Config.Init();
                    Helper.Spells.Init();
                    Helper.Objects.Init();
                    Drawings.Init();
                    Mechanics.Init();
                    Config.ShowNotification(
                        "Apollo's " + ObjectManager.Player.ChampionName + " Reborn Loaded", Config.NotificationColor, 10000);
                };
            }
        }
    }
}
