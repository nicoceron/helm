using System;

namespace Lionrise
{
    /// <summary>
    /// Each card compresses several years of implementation. These gains represent the baseline
    /// capacity built during that arc; the selected policy's explicit effects still determine
    /// who pays, how durable the result is, and which later consequences become available.
    /// A crisis replaces the decision, not the years, so its planned arc still matures.
    /// </summary>
    public static class NationalDevelopment
    {
        public static void ApplyArcMilestone(RunState state, string arcSlot)
        {
            var hidden = state.hidden;
            switch (arcSlot)
            {
                case "cut":
                    hidden.institutionDepth = Add(hidden.institutionDepth, 7);
                    hidden.foreignConfidence = Add(hidden.foreignConfidence, 7);
                    break;
                case "legitimacy":
                    hidden.institutionDepth = Add(hidden.institutionDepth, 7);
                    hidden.civilLiberties = Add(hidden.civilLiberties, 7);
                    break;
                case "housing": hidden.housingStock = Add(hidden.housingStock, 42); break;
                case "jobs":
                    hidden.skillBase = Add(hidden.skillBase, 15);
                    hidden.foreignConfidence = Add(hidden.foreignConfidence, 13);
                    RaiseCapacityMeters(state);
                    break;
                case "water": hidden.waterResilience = Add(hidden.waterResilience, 30); break;
                case "cohesion":
                    hidden.institutionDepth = Add(hidden.institutionDepth, 9);
                    hidden.civilLiberties = Add(hidden.civilLiberties, 16);
                    break;
                case "corruption":
                    hidden.institutionDepth = Add(hidden.institutionDepth, 9);
                    hidden.corruption = Add(hidden.corruption, -14);
                    break;
                case "defence":
                    hidden.institutionDepth = Add(hidden.institutionDepth, 9);
                    hidden.foreignConfidence = Add(hidden.foreignConfidence, 7);
                    RaiseCapacityMeters(state);
                    break;
                case "withdrawal":
                    hidden.housingStock = Add(hidden.housingStock, 9);
                    hidden.skillBase = Add(hidden.skillBase, 13);
                    hidden.foreignConfidence = Add(hidden.foreignConfidence, 11);
                    RaiseCapacityMeters(state);
                    break;
                case "skills":
                    hidden.skillBase = Add(hidden.skillBase, 19);
                    hidden.civilLiberties = Add(hidden.civilLiberties, 9);
                    RaiseCapacityMeters(state);
                    break;
                case "upgrade":
                    hidden.skillBase = Add(hidden.skillBase, 15);
                    hidden.foreignConfidence = Add(hidden.foreignConfidence, 15);
                    hidden.corruption = Add(hidden.corruption, -2);
                    RaiseCapacityMeters(state);
                    break;
                case "autonomy":
                    hidden.waterResilience = Add(hidden.waterResilience, 30);
                    hidden.foreignConfidence = Add(hidden.foreignConfidence, 5);
                    RaiseCapacityMeters(state);
                    break;
                case "succession":
                    hidden.institutionDepth = Add(hidden.institutionDepth, 20);
                    hidden.founderDependence = Add(hidden.founderDependence, -21);
                    break;
            }
        }

        private static int Add(int current, int delta) => Math.Max(0, Math.Min(100, current + delta));

        private static void RaiseCapacityMeters(RunState state)
        {
            state.meters.growth = Add(state.meters.growth, 1);
            state.meters.security = Add(state.meters.security, 1);
        }
    }
}
