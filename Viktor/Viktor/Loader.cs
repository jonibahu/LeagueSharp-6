using LeagueSharp;
using LeagueSharp.Common;

namespace Viktor
{
    /// <Credits:>
    /// Hellsing:
    /// Update Checker
    /// Healthpred 
    /// 
    /// blm95:
    /// DmgInd
    /// </Credits:>
    internal class Loader
    {
        private static void Main(string[] args)
        {
            if (args != null)
            {
                Utils.ClearConsole();

                CustomEvents.Game.OnGameLoad += eventArgs =>
                {
                    if (ObjectManager.Player.ChampionName != "Viktor")
                    {
                        return;
                    }
                    Utils.ClearConsole();
                    Updater.Init("Apollo16", "Viktor");
                    Spells.Init();
                    Config.Init();
                    Drawing.Init();
                    Mechanics.Init();
                    Config.ShowNotification(
                        "Apollo's " + ObjectManager.Player.ChampionName + " Loaded", Config.NotificationColor, 7000);
                };
            }
        }
    }
}