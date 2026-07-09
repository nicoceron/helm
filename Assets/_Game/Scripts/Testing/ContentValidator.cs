using System;
using System.Collections.Generic;
using System.Linq;

namespace Lionrise
{
    public sealed class ValidationReport
    {
        public readonly List<string> errors = new List<string>();
        public readonly List<string> warnings = new List<string>();
        public bool IsValid => errors.Count == 0;
    }

    public static class ContentValidator
    {
        private static readonly HashSet<string> Slots = new HashSet<string>(RunPlanGenerator.ArcSlots)
        {
            "crisis"
        };

        public static ValidationReport Validate(CardCollection collection)
        {
            var report = new ValidationReport();
            var cards = collection?.cards ?? Array.Empty<CardDef>();
            if (cards.Length == 0)
            {
                report.errors.Add("Card collection is empty.");
                return report;
            }

            var ids = new HashSet<string>();
            var allIds = new HashSet<string>(cards.Where(card => card != null).Select(card => card.id));
            var definedFlags = new HashSet<string>(cards.Where(card => card != null)
                .SelectMany(card => new[] { card.left, card.right })
                .Where(choice => choice?.effects != null)
                .SelectMany(choice => choice.effects.flagsOn ?? Array.Empty<string>()));
            definedFlags.Add("forced_independence");

            foreach (var card in cards)
            {
                if (card == null) { report.errors.Add("Null card entry."); continue; }
                var label = string.IsNullOrWhiteSpace(card.id) ? "<missing id>" : card.id;
                if (!ids.Add(card.id)) report.errors.Add($"{label}: duplicate id.");
                if (!Slots.Contains(card.arcSlot)) report.errors.Add($"{label}: invalid arc slot '{card.arcSlot}'.");
                if (string.IsNullOrWhiteSpace(card.speakerId) || string.IsNullOrWhiteSpace(card.speakerName))
                    report.errors.Add($"{label}: speaker is undefined.");
                if (string.IsNullOrWhiteSpace(card.prompt)) report.errors.Add($"{label}: prompt is missing.");
                else if (card.prompt.Length > 120) report.errors.Add($"{label}: prompt exceeds 120 characters ({card.prompt.Length}).");
                if (card.left == null || card.right == null) report.errors.Add($"{label}: must have exactly two choices.");
                else
                {
                    ValidateChoice(card, card.left, "left", report, allIds);
                    ValidateChoice(card, card.right, "right", report, allIds);
                }

                ValidateConditions(card, report, definedFlags);
                if ((card.prompt ?? string.Empty).IndexOf("reigns", StringComparison.OrdinalIgnoreCase) >= 0)
                    report.errors.Add($"{label}: contains a protected reference in player-facing copy.");
            }

            foreach (var slot in RunPlanGenerator.ArcSlots)
            {
                var count = cards.Count(card => card != null && card.arcSlot == slot && !card.crisis);
                if (count < 5) report.errors.Add($"Arc slot '{slot}' has {count} cards; requires at least 5.");
            }

            return report;
        }

        private static void ValidateChoice(CardDef card, ChoiceDef choice, string side, ValidationReport report, HashSet<string> allIds)
        {
            var prefix = $"{card.id}/{side}";
            if (string.IsNullOrWhiteSpace(choice.label)) report.errors.Add($"{prefix}: label is missing.");
            else if (choice.label.Length > 20) report.errors.Add($"{prefix}: label exceeds 20 characters.");
            if (choice.effects == null) { report.errors.Add($"{prefix}: effects are missing."); return; }

            var meter = choice.effects.meters ?? new MeterDelta();
            var hidden = choice.effects.hidden ?? new HiddenDelta();
            if (meter.NonZeroCount + hidden.NonZeroCount < 2)
                report.errors.Add($"{prefix}: must change at least two systems.");

            foreach (var value in MeterValues(meter))
                if (Math.Abs(value) > 15) report.errors.Add($"{prefix}: meter delta {value} exceeds ±15.");
            foreach (var value in HiddenValues(hidden))
                if (Math.Abs(value) > 20) report.errors.Add($"{prefix}: hidden delta {value} exceeds ±20.");

            var values = MeterValues(meter).Concat(HiddenValues(hidden)).Where(value => value != 0).ToArray();
            if (!card.crisis && values.Length > 0 && values.All(value => value > 0))
                report.errors.Add($"{prefix}: all-positive effect has no trade-off.");
            if (!card.crisis && values.Length > 0 && values.All(value => value < 0))
                report.errors.Add($"{prefix}: all-negative effect is reserved for crises.");

            foreach (var unlock in choice.effects.unlockCards ?? Array.Empty<string>())
                if (!allIds.Contains(unlock)) report.errors.Add($"{prefix}: unlock card '{unlock}' does not exist.");
        }

        private static void ValidateConditions(CardDef card, ValidationReport report, HashSet<string> definedFlags)
        {
            if (card.conditions == null) return;
            var required = new HashSet<string>(card.conditions.requiredFlags ?? Array.Empty<string>());
            var blocked = new HashSet<string>(card.conditions.blockedFlags ?? Array.Empty<string>());
            foreach (var flag in required)
            {
                if (blocked.Contains(flag)) report.errors.Add($"{card.id}: flag '{flag}' is both required and blocked.");
                if (!definedFlags.Contains(flag)) report.errors.Add($"{card.id}: required flag '{flag}' is never defined.");
            }
            foreach (var flag in blocked)
                if (!definedFlags.Contains(flag)) report.warnings.Add($"{card.id}: blocked flag '{flag}' is never defined.");
        }

        private static IEnumerable<int> MeterValues(MeterDelta value)
        {
            yield return value.cohesion; yield return value.growth; yield return value.security; yield return value.autonomy;
        }

        private static IEnumerable<int> HiddenValues(HiddenDelta value)
        {
            yield return value.housingStock; yield return value.waterResilience; yield return value.corruption; yield return value.skillBase;
            yield return value.institutionDepth; yield return value.civilLiberties; yield return value.founderDependence; yield return value.foreignConfidence;
        }
    }
}

