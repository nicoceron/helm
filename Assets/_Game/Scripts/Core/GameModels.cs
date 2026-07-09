using System;
using System.Collections.Generic;

namespace Lionrise
{
    [Serializable]
    public sealed class CardCollection
    {
        public CardDef[] cards = Array.Empty<CardDef>();
    }

    [Serializable]
    public sealed class CardDef
    {
        public string id;
        public string arcSlot;
        public string era;
        public string speakerId;
        public string speakerName;
        public string speakerRole;
        public string prompt;
        public ChoiceDef left;
        public ChoiceDef right;
        public ConditionDef conditions;
        public int weight = 10;
        public int cooldownRuns = 1;
        public string[] tags = Array.Empty<string>();
        public string historicalInspiration;
        public string sensitivity = "low";
        public bool crisis;
    }

    [Serializable]
    public sealed class ChoiceDef
    {
        public string label;
        public EffectDef effects = new EffectDef();
    }

    [Serializable]
    public sealed class EffectDef
    {
        public MeterDelta meters = new MeterDelta();
        public HiddenDelta hidden = new HiddenDelta();
        public string[] flagsOn = Array.Empty<string>();
        public string[] flagsOff = Array.Empty<string>();
        public string[] unlockCards = Array.Empty<string>();
        public string immediateEndingId;
    }

    [Serializable]
    public sealed class ConditionDef
    {
        public string[] requiredFlags = Array.Empty<string>();
        public string[] blockedFlags = Array.Empty<string>();
        public NamedRange[] meterRanges = Array.Empty<NamedRange>();
        public NamedRange[] hiddenRanges = Array.Empty<NamedRange>();
        public int minSlotIndex;
        public int maxSlotIndex = 99;
    }

    [Serializable]
    public struct NamedRange
    {
        public string name;
        public int min;
        public int max;

        public bool Contains(int value) => value >= min && value <= max;
    }

    [Serializable]
    public sealed class MeterDelta
    {
        public int cohesion;
        public int growth;
        public int security;
        public int autonomy;

        public int NonZeroCount => (cohesion != 0 ? 1 : 0) + (growth != 0 ? 1 : 0) +
                                   (security != 0 ? 1 : 0) + (autonomy != 0 ? 1 : 0);
    }

    [Serializable]
    public sealed class HiddenDelta
    {
        public int housingStock;
        public int waterResilience;
        public int corruption;
        public int skillBase;
        public int institutionDepth;
        public int civilLiberties;
        public int founderDependence;
        public int foreignConfidence;

        public int NonZeroCount =>
            (housingStock != 0 ? 1 : 0) + (waterResilience != 0 ? 1 : 0) +
            (corruption != 0 ? 1 : 0) + (skillBase != 0 ? 1 : 0) +
            (institutionDepth != 0 ? 1 : 0) + (civilLiberties != 0 ? 1 : 0) +
            (founderDependence != 0 ? 1 : 0) + (foreignConfidence != 0 ? 1 : 0);
    }

    [Serializable]
    public sealed class MeterState
    {
        public int cohesion = 45;
        public int growth = 35;
        public int security = 25;
        public int autonomy = 25;

        public int Get(string name)
        {
            switch (name)
            {
                case "cohesion": return cohesion;
                case "growth": return growth;
                case "security": return security;
                case "autonomy": return autonomy;
                default: throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown meter");
            }
        }
    }

    [Serializable]
    public sealed class HiddenState
    {
        public int housingStock = 20;
        public int waterResilience = 10;
        public int corruption = 55;
        public int skillBase = 25;
        public int institutionDepth = 20;
        public int civilLiberties = 55;
        public int founderDependence = 70;
        public int foreignConfidence = 25;

        public int Get(string name)
        {
            switch (name)
            {
                case "housingStock": return housingStock;
                case "waterResilience": return waterResilience;
                case "corruption": return corruption;
                case "skillBase": return skillBase;
                case "institutionDepth": return institutionDepth;
                case "civilLiberties": return civilLiberties;
                case "founderDependence": return founderDependence;
                case "foreignConfidence": return foreignConfidence;
                default: throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown hidden variable");
            }
        }
    }

    [Serializable]
    public sealed class RunState
    {
        public int schemaVersion = 1;
        public string runId;
        public int seed;
        public int slotIndex;
        public int year = 2165;
        public string[] runPlan = Array.Empty<string>();
        public string currentCardId;
        public MeterState meters = new MeterState();
        public HiddenState hidden = new HiddenState();
        public List<string> flags = new List<string>();
        public List<string> seenCardIdsThisRun = new List<string>();
        public List<string> followUpQueue = new List<string>();
        public bool crisisBurstUsed;
        public long startedUtcTicks;
    }

    [Serializable]
    public sealed class ProfileState
    {
        public int schemaVersion = 1;
        public string playerId;
        public List<string> seenCardIds = new List<string>();
        public List<string> unlockedCardIds = new List<string>();
        public List<string> seenEndingIds = new List<string>();
        public List<CardHistory> cardHistory = new List<CardHistory>();
        public float bestTierScore;
        public int totalRuns;
        public bool trueEndingUnlocked;
        public List<string> codexUnlocked = new List<string>();
        public AccessibilitySettings settings = new AccessibilitySettings();
    }

    [Serializable]
    public sealed class CardHistory
    {
        public string cardId;
        public int lastSeenRun;
    }

    [Serializable]
    public sealed class AccessibilitySettings
    {
        public bool reduceMotion;
        public bool highContrast;
        public bool largeText;
        public bool holdToChoose;
        public bool haptics = true;
    }

    public sealed class EndingResult
    {
        public string id;
        public string title;
        public string summary;
        public float tierScore;
        public bool victory;
    }
}
