using System;

namespace Lionrise
{
    public static class ConditionEvaluator
    {
        public static bool Passes(CardDef card, RunState state)
        {
            var conditions = card.conditions;
            if (conditions == null) return true;
            if (state.slotIndex < conditions.minSlotIndex || state.slotIndex > conditions.maxSlotIndex) return false;

            foreach (var flag in conditions.requiredFlags ?? Array.Empty<string>())
                if (!state.flags.Contains(flag)) return false;
            foreach (var flag in conditions.blockedFlags ?? Array.Empty<string>())
                if (state.flags.Contains(flag)) return false;
            foreach (var range in conditions.meterRanges ?? Array.Empty<NamedRange>())
                if (!range.Contains(state.meters.Get(range.name))) return false;
            foreach (var range in conditions.hiddenRanges ?? Array.Empty<NamedRange>())
                if (!range.Contains(state.hidden.Get(range.name))) return false;

            return true;
        }
    }
}

