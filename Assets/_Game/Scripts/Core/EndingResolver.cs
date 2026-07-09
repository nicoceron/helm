using System;

namespace Lionrise
{
    public static class EndingResolver
    {
        public static EndingResult Immediate(RunState state)
        {
            if (state.meters.cohesion <= 0) return Ending("deck_riots", "Deck Riots", "The habitat survives. The social contract does not.");
            if (state.meters.cohesion >= 100) return Ending("subsidy_spiral", "Subsidy Spiral", "Every promise passed. The treasury did not.");
            if (state.meters.growth <= 0) return Ending("bankruptcy", "Bankruptcy", "The dock lights go dark, one unpaid ring at a time.");
            if (state.meters.growth >= 100) return Ending("corporate_protectorate", "Corporate Protectorate", "Aster Lion prospers under a logo it does not own.");
            if (state.meters.security <= 0) return Ending("sabotage_night", "Sabotage Night", "One unguarded night undoes a generation of plans.");
            if (state.meters.security >= 100) return Ending("emergency_without_end", "Emergency Without End", "The sirens stop. The emergency laws remain.");
            if (state.meters.autonomy <= 0) return Ending("ice_blackmail", "Ice Pipeline Blackmail", "The taps still run, whenever the neighbour approves.");
            if (state.meters.autonomy >= 100) return Ending("fortress_aster", "Fortress Aster", "Self-reliance hardens into a sealed airlock.");
            return null;
        }

        public static EndingResult Final(RunState state)
        {
            var meters = state.meters;
            var hidden = state.hidden;
            var imbalance = (Math.Abs(meters.cohesion - 55) + Math.Abs(meters.growth - 55) +
                             Math.Abs(meters.security - 55) + Math.Abs(meters.autonomy - 55)) / 4f;
            var balanceScore = Math.Max(0f, 100f - imbalance * 2.1f);
            var foundationScore = hidden.housingStock * .18f + hidden.waterResilience * .18f +
                                  hidden.skillBase * .16f + hidden.institutionDepth * .18f +
                                  hidden.foreignConfidence * .14f + hidden.civilLiberties * .16f;
            var penalty = hidden.corruption * .20f + hidden.founderDependence * .12f;
            var score = Math.Max(0f, Math.Min(100f, balanceScore * .45f + foundationScore * .55f - penalty));
            var durableInstitutions = hidden.institutionDepth >= 75 && hidden.corruption < 35 &&
                                      hidden.founderDependence < 40 && hidden.civilLiberties > 50;
            var auditEvidence = state.flags.Contains("audit_institutions") || state.flags.Contains("audit_water_records") ||
                                state.flags.Contains("audit_rights") || state.flags.Contains("audit_housing_queue") ||
                                state.flags.Contains("audit_public_assets") || state.flags.Contains("audit_system_test");
            if (durableInstitutions && auditEvidence) score = Math.Min(100f, score + 16f);

            EndingResult result;
            if (meters.autonomy < 22 || hidden.waterResilience < 25)
                result = Ending("protectorate", "Protectorate", "A polished port, still one valve away from permission.");
            else if (meters.security > 78 && hidden.civilLiberties < 42)
                result = Ending("garrison_state", "Garrison State", "Safe streets, quiet courts, and cameras that never blink.");
            else if (meters.growth > 78 && hidden.corruption > 55 && hidden.foreignConfidence > 55)
                result = Ending("corporate_port", "Corporate Port", "The skyline rises. Public ownership becomes a historical footnote.");
            else if (meters.growth > 62 && hidden.housingStock < 42)
                result = Ending("crowded_miracle", "Crowded Miracle", "The numbers are first-world. Deck 12 is still raining indoors.");
            else if (score >= 85 && durableInstitutions)
                result = Ending("civic_republic", "Civic Republic", "No miracle, no cult: just a city that learned to outlive its makers.", true);
            else if (score >= 72 && hidden.civilLiberties < 45)
                result = Ending("glass_city", "Glass City", "A flawless city of bright windows and carefully lowered voices.", true);
            else if (score >= 72 && hidden.founderDependence > 60)
                result = Ending("founders_shadow", "Founder’s Shadow", "Aster Lion reached Tier One. It still cannot imagine morning without Arden.", true);
            else if (score >= 72)
                result = Ending("tier_one", "Tier-One World-City", "The Audit Choir certifies a durable galactic hub.", true);
            else
                result = Ending("failed_port", "Failed Port", "Aster Lion endures, but the leap to Tier One remains unfinished.");

            result.tierScore = score;
            return result;
        }

        private static EndingResult Ending(string id, string title, string summary, bool victory = false)
        {
            return new EndingResult { id = id, title = title, summary = summary, victory = victory };
        }
    }
}
