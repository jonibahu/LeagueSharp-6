using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

namespace LeBlanc.Helper
{
    internal static class Spells
    {
        public static readonly Dictionary<SpellSlot, Spell> Spell = new Dictionary<SpellSlot, Spell>
        {
            { SpellSlot.Q, new Spell(SpellSlot.Q, 700f) },
            { SpellSlot.W, new Spell(SpellSlot.W, 600f) },
            { SpellSlot.E, new Spell(SpellSlot.E, 950f) },
            { SpellSlot.R, new Spell(SpellSlot.R) }
        };
        public static void Init()
        {
            Spell[SpellSlot.Q].SetTargetted(0.25f, 2000);
            Spell[SpellSlot.W].SetSkillshot(0.25f, 70, 1500, false, SkillshotType.SkillshotCircle);
            Spell[SpellSlot.E].SetSkillshot(0.25f, 70, 1600, true, SkillshotType.SkillshotLine);

            if (Spell[SpellSlot.R].HasStatus(SpellSlot.Q))
            {
                Spell[SpellSlot.R] = new Spell(SpellSlot.R, Spell[SpellSlot.Q].Range);
                Spell[SpellSlot.R].SetTargetted(Spell[SpellSlot.Q].Delay, Spell[SpellSlot.Q].Speed);
            }
            if (Spell[SpellSlot.R].HasStatus(SpellSlot.W))
            {
                Spell[SpellSlot.R] = new Spell(SpellSlot.R, Spell[SpellSlot.W].Range);
                Spell[SpellSlot.R].SetSkillshot(Spell[SpellSlot.W].Delay, Spell[SpellSlot.W].Width, Spell[SpellSlot.W].Speed, false, SkillshotType.SkillshotCircle);
            }
            if (Spell[SpellSlot.R].HasStatus(SpellSlot.E))
            {
                Spell[SpellSlot.R] = new Spell(SpellSlot.R, Spell[SpellSlot.E].Range);
                Spell[SpellSlot.R].SetSkillshot(Spell[SpellSlot.E].Delay, Spell[SpellSlot.E].Width, Spell[SpellSlot.E].Speed, true, SkillshotType.SkillshotLine);
            }
        }

        public static bool IsSecond(this Spell spell)
        {
            if (spell.Slot == SpellSlot.W)
                return ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 2;
            if (spell.Slot == SpellSlot.R)
                return ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).ToggleState == 2;

            return false;
        }

        public static bool HasStatus(this Spell spell, SpellSlot spellSlot)
        {
            if (spell.Slot == SpellSlot.R)
            {
                switch (spellSlot)
                {
                    case SpellSlot.Q:
                        return spell.Instance.Name == "LeblancChaosOrbM";
                    case SpellSlot.W:
                        return spell.Instance.Name == "LeblancSlideM";
                    case SpellSlot.E:
                        return spell.Instance.Name == "LeblancSoulShackleM";
                }
            }

            return false;
        }
    }
}
