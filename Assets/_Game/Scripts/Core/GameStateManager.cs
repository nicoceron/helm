using System;
using System.Collections;
using UnityEngine;

namespace Lionrise
{
    public enum ChoiceSide { Left, Right }

    public sealed class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        public RunState Run { get; private set; }
        public ProfileState Profile { get; private set; }
        public CardDef CurrentCard { get; private set; }
        public bool IsResolving { get; private set; }

        private CardDatabase database;
        private WeightedDeck deck;
        private LionriseUI ui;
        private CivicAudio audioSystem;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null) return;
            var root = new GameObject("Lionrise App");
            DontDestroyOnLoad(root);
            root.AddComponent<GameStateManager>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Application.targetFrameRate = 60;
            audioSystem = gameObject.AddComponent<CivicAudio>();
            ui = LionriseUI.Create(transform, this);
            StartCoroutine(Initialize());
        }

        private IEnumerator Initialize()
        {
            ui.ShowLoading("CALIBRATING CIVIC SIGNAL…");
            Profile = SaveSystem.LoadProfile();
            database = new CardDatabase();
            string error = null;
            yield return database.Load(() => { }, message => error = message);
            if (!string.IsNullOrEmpty(error))
            {
                ui.ShowFatal(error);
                yield break;
            }
            var savedRun = SaveSystem.LoadRun();
            ui.ShowTitle(savedRun != null, StartNewRun, () => ResumeRun(savedRun));
        }

        public void StartNewRun()
        {
            var seed = unchecked((int)DateTime.UtcNow.Ticks);
            Run = new RunState
            {
                runId = $"{DateTime.UtcNow:yyyyMMddTHHmmssZ}_{Math.Abs(seed):x8}",
                seed = seed,
                runPlan = RunPlanGenerator.Generate(),
                startedUtcTicks = DateTime.UtcNow.Ticks
            };
            Run.flags.Add("forced_independence");
            deck = new WeightedDeck(database, seed);
            SaveSystem.SaveRun(Run);
            ui.ShowGame();
            DrawCurrentCard();
        }

        private void ResumeRun(RunState savedRun)
        {
            Run = savedRun;
            // Migrate local prototype saves that used year 5 as a placeholder.
            if (Run.year < 2000) Run.year += 2160;
            if (Run.runPlan == null || Run.runPlan.Length != RunPlanGenerator.ArcSlots.Length)
                Run.runPlan = RunPlanGenerator.Generate();
            deck = new WeightedDeck(database, Run.seed + Run.slotIndex * 7919);
            ui.ShowGame();
            CurrentCard = database.Find(Run.currentCardId);
            if (CurrentCard == null) DrawCurrentCard();
            else ui.Present(CurrentCard, Run);
        }

        private void DrawCurrentCard()
        {
            if (Run.slotIndex >= Run.runPlan.Length)
            {
                Finish(EndingResolver.Final(Run));
                return;
            }

            CurrentCard = deck.Draw(Run, Profile, Run.runPlan[Run.slotIndex]);
            Run.currentCardId = CurrentCard.id;
            if (!Run.seenCardIdsThisRun.Contains(CurrentCard.id)) Run.seenCardIdsThisRun.Add(CurrentCard.id);
            SaveSystem.SaveRun(Run);
            ui.Present(CurrentCard, Run);
        }

        public void Choose(ChoiceSide side)
        {
            if (IsResolving || CurrentCard == null) return;
            StartCoroutine(ResolveChoice(side));
        }

        private IEnumerator ResolveChoice(ChoiceSide side)
        {
            IsResolving = true;
            audioSystem.Commit(side, Profile.settings.haptics);
            var choice = side == ChoiceSide.Left ? CurrentCard.left : CurrentCard.right;
            var before = SnapshotMeters();
            EffectResolver.Apply(Run, choice.effects);
            NationalDevelopment.ApplyArcMilestone(Run, Run.runPlan[Run.slotIndex]);
            TrackCard(CurrentCard.id);

            yield return ui.AnimateCommit(side, before, Run.meters, Profile.settings.reduceMotion);

            EndingResult ending = null;
            if (!string.IsNullOrEmpty(choice.effects.immediateEndingId))
                ending = EndingResolver.Immediate(Run) ?? new EndingResult
                {
                    id = choice.effects.immediateEndingId,
                    title = "Protocol Terminated",
                    summary = "This policy ended the timeline."
                };
            ending ??= EndingResolver.Immediate(Run);
            if (ending != null)
            {
                Finish(ending);
                yield break;
            }

            Run.slotIndex++;
            Run.year += Run.slotIndex % 2 == 0 ? 4 : 3;
            Run.currentCardId = null;
            SaveSystem.SaveRun(Run);
            IsResolving = false;
            DrawCurrentCard();
        }

        private MeterState SnapshotMeters()
        {
            return new MeterState
            {
                cohesion = Run.meters.cohesion,
                growth = Run.meters.growth,
                security = Run.meters.security,
                autonomy = Run.meters.autonomy
            };
        }

        private void TrackCard(string id)
        {
            if (!Profile.seenCardIds.Contains(id)) Profile.seenCardIds.Add(id);
            var history = Profile.cardHistory.Find(record => record.cardId == id);
            if (history == null)
            {
                history = new CardHistory { cardId = id };
                Profile.cardHistory.Add(history);
            }
            history.lastSeenRun = Profile.totalRuns;
        }

        private void Finish(EndingResult ending)
        {
            IsResolving = false;
            Profile.totalRuns++;
            Profile.bestTierScore = Math.Max(Profile.bestTierScore, ending.tierScore);
            if (!Profile.seenEndingIds.Contains(ending.id)) Profile.seenEndingIds.Add(ending.id);
            if (ending.id == "civic_republic") Profile.trueEndingUnlocked = true;
            SaveSystem.SaveProfile(Profile);
            SaveSystem.DeleteRun();
            audioSystem.Ending(ending.victory);
            ui.ShowEnding(ending, Run, StartNewRun, ReturnToTitle);
        }

        public void ReturnToTitle()
        {
            CurrentCard = null;
            Run = null;
            ui.ShowTitle(false, StartNewRun, null);
        }

        public void SaveSettings()
        {
            SaveSystem.SaveProfile(Profile);
            ui.ApplyAccessibility(Profile.settings);
        }
    }
}
