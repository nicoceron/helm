using System;

namespace Lionrise
{
    public static class EffectResolver
    {
        public static void Apply(RunState state, EffectDef effect)
        {
            state.meters.cohesion = Clamp(state.meters.cohesion + effect.meters.cohesion);
            state.meters.growth = Clamp(state.meters.growth + effect.meters.growth);
            state.meters.security = Clamp(state.meters.security + effect.meters.security);
            state.meters.autonomy = Clamp(state.meters.autonomy + effect.meters.autonomy);

            state.hidden.housingStock = Clamp(state.hidden.housingStock + effect.hidden.housingStock);
            state.hidden.waterResilience = Clamp(state.hidden.waterResilience + effect.hidden.waterResilience);
            state.hidden.corruption = Clamp(state.hidden.corruption + effect.hidden.corruption);
            state.hidden.skillBase = Clamp(state.hidden.skillBase + effect.hidden.skillBase);
            state.hidden.institutionDepth = Clamp(state.hidden.institutionDepth + effect.hidden.institutionDepth);
            state.hidden.civilLiberties = Clamp(state.hidden.civilLiberties + effect.hidden.civilLiberties);
            state.hidden.founderDependence = Clamp(state.hidden.founderDependence + effect.hidden.founderDependence);
            state.hidden.foreignConfidence = Clamp(state.hidden.foreignConfidence + effect.hidden.foreignConfidence);

            foreach (var flag in effect.flagsOff ?? Array.Empty<string>()) state.flags.Remove(flag);
            foreach (var flag in effect.flagsOn ?? Array.Empty<string>())
                if (!state.flags.Contains(flag)) state.flags.Add(flag);
            foreach (var cardId in effect.unlockCards ?? Array.Empty<string>())
                if (!state.followUpQueue.Contains(cardId)) state.followUpQueue.Add(cardId);
        }

        private static int Clamp(int value) => Math.Max(0, Math.Min(100, value));
    }
}

