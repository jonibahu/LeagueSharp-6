using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace Viktor
{
    internal class Damages
    {
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;
        private static readonly Dictionary<SpellSlot, Spell> Spell = Spells.Spell;

        
        public static float ComboDmg(Obj_AI_Base enemy)
        {
            var qaaDmg = new Double[] { 20, 25, 30, 35, 40, 45, 50, 55, 60, 70, 80, 90, 110, 130, 150, 170, 190, 210 };
            var damage = 0d;

            if (Spell[SpellSlot.Q].IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);
            }

            if (Player.HasBuff("viktorpowertransferreturn") || Spell[SpellSlot.Q].IsReady())
            {
                damage += Player.CalcDamage(enemy, Damage.DamageType.Magical,
                    qaaDmg[Player.Level >= 18 ? 18 - 1 : Player.Level - 1] +
                    (Player.TotalMagicalDamage*.5) + Player.TotalAttackDamage());
            }

            if (Spell[SpellSlot.E].IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);
            }

            if (
                Player.Buffs.Where(h => h.Name.Contains("E") && h.Name.Contains("Aug") && h.Name.Contains("Viktor"))
                    .Any())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q) * .4;
            }

            if (Spell[SpellSlot.R].IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.R);
                damage += 5 * Player.GetSpellDamage(enemy, SpellSlot.R, 1);
            }

            if (Mechanics.ChaosStorm != null)
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.R, 1);
            }

            if (Mechanics.IgniteSlot != SpellSlot.Unknown &&
                Player.Spellbook.CanUseSpell(Mechanics.IgniteSlot) == SpellState.Ready)
            {
                damage += ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
            }

            return (float)damage;
        }
    }
}