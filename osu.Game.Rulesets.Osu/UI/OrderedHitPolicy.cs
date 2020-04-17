// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu.UI
{
    /// <summary>
    /// Ensures that <see cref="HitObject"/>s are hit in-order.
    /// If a <see cref="HitObject"/> is hit out of order:
    /// <list type="number">
    /// <item><description>The hit is blocked if it occurred earlier than the previous <see cref="HitObject"/>'s start time.</description></item>
    /// <item><description>The hit causes all previous <see cref="HitObject"/>s to missed otherwise.</description></item>
    /// </list>
    /// </summary>
    public class OrderedHitPolicy
    {
        private readonly HitObjectContainer hitObjectContainer;

        public OrderedHitPolicy(HitObjectContainer hitObjectContainer)
        {
            this.hitObjectContainer = hitObjectContainer;
        }

        /// <summary>
        /// Determines whether a <see cref="DrawableHitObject"/> can be hit at a point in time.
        /// </summary>
        /// <param name="hitObject">The <see cref="DrawableHitObject"/> to check.</param>
        /// <param name="time">The time to check.</param>
        /// <returns>Whether <paramref name="hitObject"/> can be hit at the given <paramref name="time"/>.</returns>
        public bool IsHittable(DrawableHitObject hitObject, double time)
        {
            DrawableHitObject blockingObject = null;

            // Find the last hitobject which blocks future hits.
            foreach (var obj in enumerateHitObjectsUpTo(hitObject))
            {
                if (hitObjectCanBlockFutureHits(obj))
                    blockingObject = obj;
            }

            // If there is no previous hitobject, allow the hit.
            if (blockingObject == null)
                return true;

            // A hit is allowed if:
            // 1. The last blocking hitobject has been judged.
            // 2. The current time is after the last hitobject's start time.
            // Hits at exactly the same time as the blocking hitobject are allowed for maps that contain simultaneous hitobjects (e.g. /b/372245).
            if (blockingObject.Judged || time >= blockingObject.HitObject.StartTime)
                return true;

            return false;
        }

        /// <summary>
        /// Handles a <see cref="HitObject"/> being hit to potentially miss all earlier <see cref="HitObject"/>s.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> that was hit.</param>
        public void HandleHit(DrawableHitObject hitObject)
        {
            // Hitobjects which themselves don't block future hitobjects don't cause misses (e.g. slider ticks, spinners).
            if (!hitObjectCanBlockFutureHits(hitObject))
                return;

            foreach (var obj in enumerateHitObjectsUpTo(hitObject))
            {
                if (obj.Judged)
                    continue;

                if (hitObjectCanBlockFutureHits(obj))
                    ((DrawableOsuHitObject)obj).MissForcefully();
            }
        }

        /// <summary>
        /// Whether a <see cref="HitObject"/> blocks hits on future <see cref="HitObject"/>s until its start time is reached.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> to test.</param>
        private static bool hitObjectCanBlockFutureHits(DrawableHitObject hitObject)
            => hitObject is DrawableHitCircle;

        // Todo: Inefficient
        private IEnumerable<DrawableHitObject> enumerateHitObjectsUpTo(DrawableHitObject hitObject)
        {
            return enumerate(hitObjectContainer.AliveObjects);

            IEnumerable<DrawableHitObject> enumerate(IEnumerable<DrawableHitObject> list)
            {
                foreach (var obj in list)
                {
                    if (obj.HitObject.StartTime >= hitObject.HitObject.StartTime)
                        yield break;

                    yield return obj;

                    foreach (var nested in enumerate(obj.NestedHitObjects))
                        yield return nested;
                }
            }
        }
    }
}
