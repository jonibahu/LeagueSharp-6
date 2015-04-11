using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

namespace Viktor
{
    internal class Spells
    {
        public static readonly int ECastRange = 550;

        public static readonly Dictionary<SpellSlot, Spell> Spell = new Dictionary<SpellSlot, Spell>
        {
            { SpellSlot.Q, new Spell(SpellSlot.Q, 700f) },
            { SpellSlot.W, new Spell(SpellSlot.W, 700f) },
            { SpellSlot.E, new Spell(SpellSlot.E, 700f) },
            { SpellSlot.R, new Spell(SpellSlot.R, 700f) }
        };

        public static void Init()
        {
            Spell[SpellSlot.Q].SetTargetted(0.25f, 2000);
            Spell[SpellSlot.W].SetSkillshot(0.25f, 300, float.MaxValue, false, SkillshotType.SkillshotCircle);
            Spell[SpellSlot.E].SetSkillshot(0.0f, 90, 1200, false, SkillshotType.SkillshotLine);
            Spell[SpellSlot.R].SetSkillshot(0.25f, 250, float.MaxValue, false, SkillshotType.SkillshotCircle);
        }
    }
}