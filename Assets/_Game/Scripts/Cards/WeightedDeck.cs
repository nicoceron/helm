using System;
using System.Collections.Generic;
using System.Linq;

namespace Lionrise
{
    public sealed class WeightedDeck
    {
        private readonly CardDatabase database;
        private readonly Random random;

        public WeightedDeck(CardDatabase database, int seed)
        {
            this.database = database;
            random = new Random(seed);
        }

        public CardDef Draw(RunState state, ProfileState profile, string arcSlot)
        {
            var crisisTags = ActiveCrisisTags(state);
            var guaranteedCrisis = arcSlot == "withdrawal";
            var crisisEligible = guaranteedCrisis || crisisTags.Count > 0;
            if (!state.crisisBurstUsed && CanReplaceWithCrisis(arcSlot) && crisisEligible &&
                (guaranteedCrisis || random.NextDouble() < .42))
            {
                var crisis = Filter(state, profile, database.Cards.Where(card => card.crisis &&
                    (guaranteedCrisis || (card.tags ?? Array.Empty<string>()).Any(crisisTags.Contains))));
                if (crisis.Count > 0)
                {
                    state.crisisBurstUsed = true;
                    return Pick(crisis, state, crisisTags);
                }
            }

            var candidates = Filter(state, profile, database.Cards.Where(card => !card.crisis && card.arcSlot == arcSlot));
            if (candidates.Count == 0)
            {
                candidates = database.Cards.Where(card => !card.crisis && card.arcSlot == arcSlot &&
                    !state.seenCardIdsThisRun.Contains(card.id)).ToList();
            }
            if (candidates.Count == 0) throw new InvalidOperationException($"No valid card for arc slot '{arcSlot}'.");
            return Pick(candidates, state, crisisTags);
        }

        private static bool CanReplaceWithCrisis(string slot)
        {
            return slot != "cut" && slot != "legitimacy" && slot != "succession" && slot != "final_audit";
        }

        private static List<CardDef> Filter(RunState state, ProfileState profile, IEnumerable<CardDef> source)
        {
            return source.Where(card => ConditionEvaluator.Passes(card, state))
                .Where(card => !state.seenCardIdsThisRun.Contains(card.id))
                .Where(card => !OnCooldown(card, profile))
                .ToList();
        }

        private static bool OnCooldown(CardDef card, ProfileState profile)
        {
            if (card.cooldownRuns <= 0) return false;
            var record = profile.cardHistory.Find(item => item.cardId == card.id);
            return record != null && profile.totalRuns - record.lastSeenRun <= card.cooldownRuns;
        }

        private CardDef Pick(IReadOnlyList<CardDef> cards, RunState state, HashSet<string> crisisTags)
        {
            var total = 0;
            var weights = new int[cards.Count];
            for (var i = 0; i < cards.Count; i++)
            {
                var card = cards[i];
                var weight = Math.Max(1, card.weight);
                foreach (var tag in card.tags ?? Array.Empty<string>())
                    if (crisisTags.Contains(tag)) weight += 8;
                if (state.followUpQueue.Contains(card.id)) weight += 25;
                weights[i] = weight;
                total += weight;
            }

            var roll = random.Next(total);
            for (var i = 0; i < cards.Count; i++)
            {
                roll -= weights[i];
                if (roll < 0) return cards[i];
            }
            return cards[cards.Count - 1];
        }

        public static HashSet<string> ActiveCrisisTags(RunState state)
        {
            var tags = new HashSet<string>();
            if (state.meters.cohesion <= 12 || state.meters.cohesion >= 88) tags.Add("cohesion");
            if (state.meters.growth <= 12 || state.meters.growth >= 88) tags.Add("economy");
            if (state.meters.security <= 12 || state.meters.security >= 88) tags.Add("security");
            if (state.meters.autonomy <= 12 || state.meters.autonomy >= 88) tags.Add("sovereignty");
            if (state.hidden.corruption >= 75) tags.Add("corruption");
            if (state.hidden.waterResilience <= 20) tags.Add("water");
            return tags;
        }
    }
}
