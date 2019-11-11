// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    public class DrawableOsuEditRuleset : DrawableOsuRuleset
    {
        public DrawableOsuEditRuleset(Ruleset ruleset, IWorkingBeatmap beatmap, IReadOnlyList<Mod> mods)
            : base(ruleset, beatmap, mods)
        {
        }

        public override DrawableHitObject<OsuHitObject> CreateDrawableRepresentation(OsuHitObject h)
            => base.CreateDrawableRepresentation(h)?.With(d => d.ApplyCustomUpdateState += updateState);

        private void updateState(DrawableHitObject hitObject, ArmedState state)
        {
            switch (state)
            {
                case ArmedState.Miss:
                    // Get the existing fade out transform
                    var existing = hitObject.Transforms.LastOrDefault(t => t.TargetMember == nameof(Alpha));
                    if (existing == null)
                        return;

                    using (hitObject.BeginAbsoluteSequence(existing.StartTime))
                        hitObject.FadeOut(500).Expire();
                    break;
            }
        }

        protected override Playfield CreatePlayfield() => new OsuPlayfieldNoCursor();

        public override PlayfieldAdjustmentContainer CreatePlayfieldAdjustmentContainer() => new OsuPlayfieldAdjustmentContainer { Size = Vector2.One };

        private class OsuPlayfieldNoCursor : OsuPlayfield
        {
            protected override GameplayCursorContainer CreateCursor() => null;
        }
    }
}
