using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

namespace Lionrise
{
    public sealed class LionriseUI : MonoBehaviour
    {
        private static readonly Color Mint = new Color32(90, 214, 190, 255);
        private static readonly Color Ice = new Color32(249, 246, 191, 255);
        private static readonly Color Muted = new Color32(183, 176, 155, 255);
        private static readonly Color Ember = new Color32(239, 102, 50, 255);
        private static readonly Color Alert = new Color32(228, 61, 68, 255);
        private static readonly Color Panel = new Color32(20, 12, 15, 246);
        private static Font sharedFont;

        private GameStateManager manager;
        private Canvas canvas;
        private RectTransform safeRoot;
        private GameObject loadingScreen;
        private GameObject titleScreen;
        private GameObject gameScreen;
        private GameObject endingScreen;
        private GameObject settingsScreen;
        private GameObject journeyMenu;
        private RectTransform journeyPanel;
        private Text journeyTitle;
        private Text journeyBody;
        private Text loadingText;
        private Text titleRecord;
        private Button resumeButton;
        private Text yearText;
        private Text progressText;
        private Text eraText;
        private Text speakerText;
        private Text roleText;
        private Text promptText;
        private Text historyText;
        private Text leftChoiceText;
        private Text rightChoiceText;
        private Text instructionText;
        private Text miniGameHint;
        private Text policyText;
        private Text endingEyebrow;
        private Text endingTitle;
        private Text endingSummary;
        private Text endingScore;
        private Text endingMeters;
        private RectTransform cardBounds;
        private RectTransform card;
        private CanvasGroup cardCanvasGroup;
        private HoloCardGraphic cardGraphic;
        private CardDragController dragController;
        private GameObject holdControls;
        private ReignsBackdropController backdropController;
        private Image promptPanel;
        private Image speakerPanel;
        private Image cardBackPanel;
        private Image portraitArt;
        private Image titlePortrait;
        private Sprite[] advisorSprites;
        private Sprite[] advisorBlinkSprites;
        private Sprite[] citySprites;
        private PortraitBlinkController portraitMotion;
        private ReignsSpecialCardGraphic specialCardGraphic;
        private GameObject chapterOverlay;
        private Text chapterEyebrow;
        private Text chapterTitle;
        private Text chapterBody;
        private Text chapterButtonText;
        private GameObject crisisHud;
        private Text crisisStepText;
        private Text crisisTimerText;
        private MeterWidget cohesion;
        private MeterWidget growth;
        private MeterWidget security;
        private MeterWidget autonomy;
        private Action resumeAction;
        private int basePromptSize = 19;
        private int lastChapterShown = -1;
        private bool crisisMode;
        private int crisisStep;
        private int crisisLeft;
        private int crisisRight;
        private float crisisSeconds;
        private Coroutine crisisClock;
        private int entryDirection = -1;
        private readonly List<Action> settingsRefreshers = new List<Action>();

        private static readonly string[] CrisisPrompts =
        {
            "A debris swarm cuts toward the habitat ring.",
            "A hostile probe spoofs the dock authority signal.",
            "The water vault loses pressure across three decks."
        };

        private static readonly string[] CrisisLeft = { "JAM THE FIELD", "CUT THE GRID", "SEAL THE DECKS" };
        private static readonly string[] CrisisRight = { "INTERCEPT", "TRACE IT", "VENT & REPAIR" };

        public static LionriseUI Create(Transform parent, GameStateManager manager)
        {
            var root = new GameObject("Runtime UI");
            root.transform.SetParent(parent, false);
            var ui = root.AddComponent<LionriseUI>();
            ui.manager = manager;
            ui.Build();
            return ui;
        }

        private void Build()
        {
            EnsureEventSystem();
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            // Reigns Beyond uses a 600x600 height-matched virtual canvas. At 1024x768
            // this produces an 800x600 working area and a centered 280px card.
            scaler.referenceResolution = new Vector2(600, 600);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;
            gameObject.AddComponent<GraphicRaycaster>();

            LoadArt();
            var backdrop = CreateRect("Generated Reigns Background", transform, Vector2.zero, Vector2.one);
            backdropController = backdrop.gameObject.AddComponent<ReignsBackdropController>();
            backdropController.Build();

            safeRoot = CreateRect("Safe Area", transform, Vector2.zero, Vector2.one);
            safeRoot.gameObject.AddComponent<SafeAreaFitter>();

            BuildLoading();
            BuildTitle();
            BuildGame();
            BuildEnding();
            BuildSettings();
        }

        private void BuildLoading()
        {
            loadingScreen = CreateRect("Loading", safeRoot, Vector2.zero, Vector2.one).gameObject;
            var mark = CreateRect("Signal", loadingScreen.transform, new Vector2(.43f, .54f), new Vector2(.57f, .68f));
            mark.gameObject.AddComponent<LionMarkGraphic>().raycastTarget = false;
            loadingText = CreateText(loadingScreen.transform, "CALIBRATING CIVIC SIGNAL…", 14, TextAnchor.UpperCenter, Muted, FontStyle.Bold);
            SetRect(loadingText.rectTransform, new Vector2(.08f, .43f), new Vector2(.92f, .52f));
        }

        private void BuildTitle()
        {
            titleScreen = CreateRect("Title", safeRoot, Vector2.zero, Vector2.one).gameObject;

            var spine = CreateRect("Archive Spine", titleScreen.transform, new Vector2(.30f, 0f), new Vector2(.70f, 1f));
            var spineImage = spine.gameObject.AddComponent<Image>();
            spineImage.color = new Color32(14, 12, 18, 242);
            spineImage.raycastTarget = false;

            var protocol = CreateText(titleScreen.transform, "ARCHIVE SIMULATION // SINGAPORE 1965 → ASTER LION 2165", 11, TextAnchor.MiddleCenter, Muted, FontStyle.Bold);
            SetRect(protocol.rectTransform, new Vector2(.08f, .93f), new Vector2(.92f, .98f));

            var title = CreateText(titleScreen.transform, "LIONRISE", 48, TextAnchor.MiddleCenter, Ice, FontStyle.Bold);
            SetRect(title.rectTransform, new Vector2(.08f, .79f), new Vector2(.92f, .93f));
            var subtitle = CreateText(titleScreen.transform, "FROM THIRD WORLD TO FIRST — IN SPACE", 15, TextAnchor.MiddleCenter, Ember, FontStyle.Bold);
            SetRect(subtitle.rectTransform, new Vector2(.08f, .75f), new Vector2(.92f, .80f));

            var portrait = CreateRect("Founder Card", titleScreen.transform, new Vector2(.325f, .31f), new Vector2(.675f, .74f));
            titlePortrait = portrait.gameObject.AddComponent<Image>();
            titlePortrait.raycastTarget = false;
            titlePortrait.preserveAspect = true;
            titlePortrait.sprite = advisorSprites != null && advisorSprites.Length > 0 ? advisorSprites[0] : null;

            var pitch = CreateText(titleScreen.transform,
                "A FUTURIST RETELLING OF LEE KUAN YEW'S 1965–2015 TRANSFORMATION.",
                14, TextAnchor.MiddleCenter, Ice);
            SetRect(pitch.rectTransform, new Vector2(.20f, .25f), new Vector2(.80f, .31f));

            CreateButton(titleScreen.transform, "BEGIN THE TIMELINE", new Vector2(.30f, .15f), new Vector2(.70f, .23f), StartClicked, true, 14);
            resumeButton = CreateButton(titleScreen.transform, "RESUME", new Vector2(.30f, .075f), new Vector2(.70f, .14f), () => resumeAction?.Invoke(), false, 13);
            titleRecord = CreateText(titleScreen.transform, string.Empty, 10, TextAnchor.MiddleCenter, Muted);
            SetRect(titleRecord.rectTransform, new Vector2(.15f, .02f), new Vector2(.85f, .07f));
        }

        private void BuildGame()
        {
            gameScreen = CreateRect("Game", safeRoot, Vector2.zero, Vector2.one).gameObject;

            var gameSpine = CreateRect("Game Spine", gameScreen.transform, new Vector2(.30f, 0f), new Vector2(.70f, 1f));
            var gameSpineImage = gameSpine.gameObject.AddComponent<Image>();
            gameSpineImage.color = new Color32(12, 11, 17, 244);
            gameSpineImage.raycastTarget = false;

            yearText = CreateText(gameScreen.transform, "2165 · ASTER LION", 8, TextAnchor.MiddleLeft, Ice, FontStyle.Bold);
            SetRect(yearText.rectTransform, new Vector2(.305f, .96f), new Vector2(.46f, .995f));
            progressText = CreateText(gameScreen.transform, "01 / 14", 8, TextAnchor.MiddleRight, Ice, FontStyle.Bold);
            SetRect(progressText.rectTransform, new Vector2(.56f, .96f), new Vector2(.645f, .995f));
            CreateButton(gameScreen.transform, "≡", new Vector2(.655f, .957f), new Vector2(.695f, .995f), ToggleJourneyMenu, false, 11);

            var meterArea = CreateFixedRect("Civic Meters", gameScreen.transform, new Vector2(.5f, 1f), new Vector2(320f, 68f), new Vector2(0f, -48f));
            cohesion = CreateMeterColumn(meterArea, 0, "Cohesion", "●", new Color32(255, 189, 108, 255));
            growth = CreateMeterColumn(meterArea, 1, "Growth", "▦", new Color32(86, 221, 255, 255));
            security = CreateMeterColumn(meterArea, 2, "Security", "▲", new Color32(255, 111, 125, 255));
            autonomy = CreateMeterColumn(meterArea, 3, "Autonomy", "◉", new Color32(128, 237, 176, 255));

            var promptBack = CreateFixedRect("Question Panel", gameScreen.transform, new Vector2(.5f, 1f), new Vector2(320f, 105f), new Vector2(0f, -118f));
            promptPanel = promptBack.gameObject.AddComponent<Image>();
            promptPanel.color = new Color32(236, 207, 137, 248);
            promptPanel.raycastTarget = false;
            promptText = CreateText(promptBack, "A decision arrives.", 19, TextAnchor.MiddleCenter, new Color32(26, 20, 22, 255), FontStyle.Bold);
            promptText.resizeTextForBestFit = true;
            promptText.resizeTextMinSize = 13;
            promptText.resizeTextMaxSize = 19;
            SetRect(promptText.rectTransform, new Vector2(.06f, .08f), new Vector2(.94f, .92f));

            // Extracted reference: 280x280 card, centralPos=(0,100), with a
            // second dark card behind it that becomes visible while swiping.
            cardBounds = CreateFixedRect("Card Bounds", gameScreen.transform, new Vector2(.5f, .5f), new Vector2(280f, 280f), new Vector2(0f, 42f));
            var cardBack = CreateRect("Card Back", cardBounds, Vector2.zero, Vector2.one);
            cardBack.anchoredPosition = new Vector2(11f, -8f);
            cardBackPanel = cardBack.gameObject.AddComponent<Image>();
            cardBackPanel.color = new Color32(27, 23, 32, 255);
            cardBackPanel.raycastTarget = false;
            for (var i = 0; i < 4; i++)
            {
                var star = CreateText(cardBack, "◆", 11, TextAnchor.MiddleCenter, new Color32(236, 203, 115, 210), FontStyle.Bold);
                SetRect(star.rectTransform, new Vector2(.90f, .13f + i * .22f), new Vector2(.99f, .23f + i * .22f));
            }

            card = CreateRect("Character Card", cardBounds, Vector2.zero, Vector2.one);
            cardGraphic = card.gameObject.AddComponent<HoloCardGraphic>();
            cardCanvasGroup = card.gameObject.AddComponent<CanvasGroup>();

            var portraitFrame = CreateRect("Advisor Illustration", card, new Vector2(.01f, .01f), new Vector2(.99f, .99f));
            portraitArt = portraitFrame.gameObject.AddComponent<Image>();
            portraitArt.raycastTarget = false;
            portraitArt.preserveAspect = true;
            portraitMotion = portraitFrame.gameObject.AddComponent<PortraitBlinkController>();
            portraitMotion.Configure(portraitArt);

            var special = CreateRect("Special Card Animation", card, new Vector2(.01f, .01f), new Vector2(.99f, .99f));
            specialCardGraphic = special.gameObject.AddComponent<ReignsSpecialCardGraphic>();
            specialCardGraphic.raycastTarget = false;
            specialCardGraphic.SetMode(SpecialCardMode.None);

            eraText = CreateText(card, "SURVIVAL", 8, TextAnchor.MiddleRight, Ice, FontStyle.Bold);
            SetRect(eraText.rectTransform, new Vector2(.49f, .91f), new Vector2(.94f, .985f));

            leftChoiceText = CreateText(card, "LEFT", 15, TextAnchor.MiddleLeft, Ice, FontStyle.Bold);
            SetRect(leftChoiceText.rectTransform, new Vector2(.05f, .04f), new Vector2(.49f, .25f));
            var leftChoiceOutline = leftChoiceText.gameObject.AddComponent<Outline>();
            leftChoiceOutline.effectColor = new Color32(10, 9, 13, 235);
            leftChoiceOutline.effectDistance = new Vector2(2f, -2f);
            rightChoiceText = CreateText(card, "RIGHT", 15, TextAnchor.MiddleRight, Ice, FontStyle.Bold);
            SetRect(rightChoiceText.rectTransform, new Vector2(.51f, .04f), new Vector2(.95f, .25f));
            var rightChoiceOutline = rightChoiceText.gameObject.AddComponent<Outline>();
            rightChoiceOutline.effectColor = new Color32(10, 9, 13, 235);
            rightChoiceOutline.effectDistance = new Vector2(2f, -2f);

            dragController = card.gameObject.AddComponent<CardDragController>();
            dragController.Configure(cardBounds, canvas, leftChoiceText, rightChoiceText, cardGraphic);
            dragController.previewChanged = PreviewChoice;
            dragController.previewCleared = ClearPreview;
            dragController.committed = CommitSide;

            var speakerBack = CreateFixedRect("Speaker Panel", gameScreen.transform, new Vector2(.5f, 1f), new Vector2(320f, 70f), new Vector2(0f, -442f));
            speakerPanel = speakerBack.gameObject.AddComponent<Image>();
            speakerPanel.color = new Color32(236, 207, 137, 248);
            speakerPanel.raycastTarget = false;
            speakerText = CreateText(speakerBack, "LEE ARCHIVE", 14, TextAnchor.LowerCenter, new Color32(26, 20, 22, 255), FontStyle.Bold);
            SetRect(speakerText.rectTransform, new Vector2(.04f, .40f), new Vector2(.96f, .94f));
            roleText = CreateText(speakerBack, "FOUNDING PREMIER SIMULATION", 9, TextAnchor.UpperCenter, new Color32(52, 43, 39, 235), FontStyle.Bold);
            SetRect(roleText.rectTransform, new Vector2(.04f, .08f), new Vector2(.96f, .43f));

            historyText = CreateText(gameScreen.transform, "ARCHIVE ECHO // SINGAPORE, 1965", 8, TextAnchor.MiddleCenter, Ember, FontStyle.Bold);
            historyText.resizeTextForBestFit = true;
            historyText.resizeTextMinSize = 7;
            historyText.resizeTextMaxSize = 8;
            SetRect(historyText.rectTransform, new Vector2(.31f, .17f), new Vector2(.69f, .21f));

            instructionText = CreateText(gameScreen.transform, "DRAG THE CARD  ←   →", 9, TextAnchor.MiddleCenter, Muted, FontStyle.Bold);
            SetRect(instructionText.rectTransform, new Vector2(.35f, .10f), new Vector2(.65f, .15f));
            miniGameHint = CreateText(gameScreen.transform, string.Empty, 8, TextAnchor.MiddleCenter, Ember, FontStyle.Bold);
            SetRect(miniGameHint.rectTransform, new Vector2(.31f, .065f), new Vector2(.69f, .105f));
            policyText = CreateText(gameScreen.transform, "POLICY MEMORY // EMPTY", 7, TextAnchor.MiddleCenter, new Color32(170, 160, 139, 220), FontStyle.Bold);
            SetRect(policyText.rectTransform, new Vector2(.31f, .025f), new Vector2(.69f, .065f));

            // The reference question strip overlaps the card stack but remains
            // above it, so even two-line questions stay fully readable.
            promptBack.SetAsLastSibling();

            BuildHoldControls();
            BuildCrisisHud();
            BuildChapterOverlay();
            BuildJourneyMenu();
        }

        private void BuildJourneyMenu()
        {
            journeyMenu = CreateRect("Journey Menu", gameScreen.transform, Vector2.zero, Vector2.one).gameObject;
            var shade = journeyMenu.AddComponent<Image>();
            shade.color = new Color32(7, 6, 10, 205);
            journeyPanel = CreateRect("Journey Panel", journeyMenu.transform, new Vector2(.30f, .07f), new Vector2(.70f, .93f));
            var panelImage = journeyPanel.gameObject.AddComponent<Image>();
            panelImage.color = new Color32(18, 16, 23, 252);
            var outline = journeyPanel.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color32(224, 194, 130, 190);
            outline.effectDistance = new Vector2(1f, -1f);

            journeyTitle = CreateText(journeyPanel, "JOURNEY STATUS", 22, TextAnchor.MiddleCenter, Ice, FontStyle.Bold);
            SetRect(journeyTitle.rectTransform, new Vector2(.07f, .82f), new Vector2(.93f, .95f));
            journeyBody = CreateText(journeyPanel, string.Empty, 13, TextAnchor.UpperCenter, Muted);
            SetRect(journeyBody.rectTransform, new Vector2(.08f, .30f), new Vector2(.92f, .80f));

            CreateButton(journeyPanel, "STATUS", new Vector2(.05f, .20f), new Vector2(.275f, .28f), () => ShowJourneyTab(0), false, 9);
            CreateButton(journeyPanel, "OBJECTIVES", new Vector2(.285f, .20f), new Vector2(.515f, .28f), () => ShowJourneyTab(1), false, 9);
            CreateButton(journeyPanel, "EFFECTS", new Vector2(.525f, .20f), new Vector2(.75f, .28f), () => ShowJourneyTab(2), false, 9);
            CreateButton(journeyPanel, "OPTIONS", new Vector2(.76f, .20f), new Vector2(.95f, .28f), OpenSettingsFromJourney, false, 9);
            CreateButton(journeyPanel, "RETURN TO THE CARD", new Vector2(.15f, .07f), new Vector2(.85f, .16f), ToggleJourneyMenu, true, 11);
            journeyMenu.SetActive(false);
        }

        private void ToggleJourneyMenu()
        {
            if (journeyMenu == null || manager.Profile == null || !gameScreen.activeSelf) return;
            var opening = !journeyMenu.activeSelf;
            journeyMenu.SetActive(opening);
            if (opening)
            {
                dragController.SetEnabled(false);
                ShowJourneyTab(0);
                StopCoroutine(nameof(AnimateJourneyPanel));
                StartCoroutine(nameof(AnimateJourneyPanel));
            }
            else if (!manager.IsResolving && !chapterOverlay.activeSelf)
            {
                dragController.SetEnabled(true);
            }
        }

        private IEnumerator AnimateJourneyPanel()
        {
            var target = Vector2.zero;
            journeyPanel.anchoredPosition = new Vector2(0f, 100f);
            for (var elapsed = 0f; elapsed < 1f; elapsed += Time.unscaledDeltaTime)
            {
                journeyPanel.anchoredPosition = Vector2.Lerp(journeyPanel.anchoredPosition, target, Time.unscaledDeltaTime * 8f);
                yield return null;
            }
            journeyPanel.anchoredPosition = target;
        }

        private void ShowJourneyTab(int tab)
        {
            if (manager.Run == null)
            {
                journeyTitle.text = "ARCHIVE";
                journeyBody.text = "NO ACTIVE TIMELINE";
                return;
            }

            var run = manager.Run;
            switch (tab)
            {
                case 0:
                    journeyTitle.text = "JOURNEY STATUS";
                    journeyBody.text = $"YEAR {run.year:0000}   ·   DECISION {run.slotIndex + 1:00} / {run.runPlan.Length:00}\n\n" +
                        $"COHESION      {run.meters.cohesion:00}\nGROWTH          {run.meters.growth:00}\n" +
                        $"SECURITY       {run.meters.security:00}\nAUTONOMY      {run.meters.autonomy:00}";
                    break;
                case 1:
                    journeyTitle.text = "OBJECTIVES";
                    journeyBody.text = "SURVIVE FORCED INDEPENDENCE\n\nBUILD HOMES, JOBS, WATER AND DEFENCE\n\n" +
                        "CLIMB FROM WORKSHOP TO WORLD HUB\n\nMAKE INSTITUTIONS OUTLIVE THE FOUNDER";
                    break;
                default:
                    journeyTitle.text = "ACTIVE EFFECTS";
                    journeyBody.text = PolicyMemory(run.flags).Replace("POLICY MEMORY // ", string.Empty).Replace("  ·  ", "\n\n");
                    break;
            }
        }

        private void OpenSettingsFromJourney()
        {
            journeyMenu.SetActive(false);
            settingsScreen.SetActive(true);
            foreach (var refresh in settingsRefreshers) refresh();
        }

        private MeterWidget CreateMeterColumn(RectTransform parent, int index, string name, string shape, Color color)
        {
            var min = new Vector2(index * .25f + .01f, 0);
            var max = new Vector2((index + 1) * .25f - .01f, 1);
            var column = CreateRect(name, parent, min, max);
            return MeterWidget.Create(column, name, shape, color);
        }

        private void BuildHoldControls()
        {
            holdControls = CreateRect("Hold Controls", gameScreen.transform, new Vector2(.31f, .085f), new Vector2(.69f, .15f)).gameObject;
            CreateHoldButton(holdControls.transform, "HOLD LEFT", new Vector2(0, .05f), new Vector2(.48f, .95f), () => dragController.CommitFromKeyboard(ChoiceSide.Left));
            CreateHoldButton(holdControls.transform, "HOLD RIGHT", new Vector2(.52f, .05f), new Vector2(1, .95f), () => dragController.CommitFromKeyboard(ChoiceSide.Right));
            holdControls.SetActive(false);
        }

        private void BuildCrisisHud()
        {
            crisisHud = CreateRect("Rapid Swipe Crisis", gameScreen.transform, new Vector2(.30f, .77f), new Vector2(.70f, .82f)).gameObject;
            var panelImage = crisisHud.AddComponent<Image>();
            panelImage.color = new Color32(91, 17, 24, 246);
            panelImage.raycastTarget = false;
            crisisStepText = CreateText(crisisHud.transform, "EMERGENCY 01 / 03", 11, TextAnchor.UpperLeft, Ice, FontStyle.Bold);
            SetRect(crisisStepText.rectTransform, new Vector2(.04f, .08f), new Vector2(.72f, .92f));
            crisisTimerText = CreateText(crisisHud.transform, "7.5", 14, TextAnchor.MiddleRight, new Color32(255, 197, 108, 255), FontStyle.Bold);
            SetRect(crisisTimerText.rectTransform, new Vector2(.72f, .08f), new Vector2(.95f, .92f));
            crisisHud.SetActive(false);
        }

        private void BuildChapterOverlay()
        {
            chapterOverlay = CreateRect("Story Chapter", gameScreen.transform, Vector2.zero, Vector2.one).gameObject;
            var shade = chapterOverlay.AddComponent<Image>();
            shade.color = new Color32(5, 5, 8, 188);

            var spine = CreateRect("Intercale Spine", chapterOverlay.transform, new Vector2(.30f, 0f), new Vector2(.70f, 1f));
            var spineImage = spine.gameObject.AddComponent<Image>();
            spineImage.color = new Color32(12, 11, 17, 250);
            spineImage.raycastTarget = false;

            var rear = CreateFixedRect("Intercale Card Back", chapterOverlay.transform, new Vector2(.5f, .5f), new Vector2(280f, 280f), new Vector2(10f, 23f));
            var rearGraphic = rear.gameObject.AddComponent<HoloCardGraphic>();
            rearGraphic.borderColor = new Color32(111, 76, 94, 190);
            rearGraphic.raycastTarget = false;
            var chapterCard = CreateFixedRect("Intercale Card", chapterOverlay.transform, new Vector2(.5f, .5f), new Vector2(280f, 280f), new Vector2(0f, 34f));
            var chapterGraphic = chapterCard.gameObject.AddComponent<HoloCardGraphic>();
            chapterGraphic.borderColor = new Color32(224, 194, 130, 235);
            chapterGraphic.raycastTarget = false;

            chapterEyebrow = CreateText(chapterCard, "CHAPTER I // CAST ADRIFT", 10, TextAnchor.MiddleCenter, Ember, FontStyle.Bold);
            SetRect(chapterEyebrow.rectTransform, new Vector2(.07f, .84f), new Vector2(.93f, .94f));
            chapterTitle = CreateText(chapterCard, "A PORT WITHOUT A COUNTRY", 25, TextAnchor.MiddleCenter, Ice, FontStyle.Bold);
            chapterTitle.resizeTextForBestFit = true;
            chapterTitle.resizeTextMinSize = 18;
            chapterTitle.resizeTextMaxSize = 25;
            SetRect(chapterTitle.rectTransform, new Vector2(.07f, .60f), new Vector2(.93f, .84f));
            chapterBody = CreateText(chapterCard, "The timeline begins.", 14, TextAnchor.MiddleCenter, Muted);
            chapterBody.resizeTextForBestFit = true;
            chapterBody.resizeTextMinSize = 11;
            chapterBody.resizeTextMaxSize = 14;
            SetRect(chapterBody.rectTransform, new Vector2(.08f, .08f), new Vector2(.92f, .60f));
            var button = CreateButton(chapterOverlay.transform, "BEGIN INDEPENDENCE", new Vector2(.33f, .12f), new Vector2(.67f, .20f), CompleteChapter, true, 11);
            chapterButtonText = button.GetComponentInChildren<Text>();
            chapterOverlay.SetActive(false);
        }

        private void BuildEnding()
        {
            endingScreen = CreateRect("Ending", safeRoot, Vector2.zero, Vector2.one).gameObject;
            var spine = CreateRect("Audit Spine", endingScreen.transform, new Vector2(.30f, 0f), new Vector2(.70f, 1f));
            var spineImage = spine.gameObject.AddComponent<Image>();
            spineImage.color = new Color32(12, 11, 17, 244);
            spineImage.raycastTarget = false;
            endingEyebrow = CreateText(endingScreen.transform, "GALACTIC TIER AUDIT // COMPLETE", 13, TextAnchor.MiddleCenter, Mint, FontStyle.Bold);
            SetRect(endingEyebrow.rectTransform, new Vector2(.31f, .83f), new Vector2(.69f, .9f));
            endingTitle = CreateText(endingScreen.transform, "TIER-ONE WORLD-CITY", 34, TextAnchor.MiddleCenter, Ice, FontStyle.Bold);
            endingTitle.resizeTextForBestFit = true;
            endingTitle.resizeTextMinSize = 20;
            SetRect(endingTitle.rectTransform, new Vector2(.31f, .64f), new Vector2(.69f, .82f));
            endingSummary = CreateText(endingScreen.transform, "The Audit Choir certifies a durable galactic hub.", 15, TextAnchor.MiddleCenter, Muted);
            endingSummary.resizeTextForBestFit = true;
            endingSummary.resizeTextMinSize = 11;
            endingSummary.resizeTextMaxSize = 15;
            SetRect(endingSummary.rectTransform, new Vector2(.32f, .49f), new Vector2(.68f, .64f));
            endingScore = CreateText(endingScreen.transform, "TIER INDEX 72", 17, TextAnchor.MiddleCenter, Mint, FontStyle.Bold);
            SetRect(endingScore.rectTransform, new Vector2(.31f, .41f), new Vector2(.69f, .49f));
            endingMeters = CreateText(endingScreen.transform, string.Empty, 14, TextAnchor.MiddleCenter, Ice);
            SetRect(endingMeters.rectTransform, new Vector2(.31f, .28f), new Vector2(.69f, .41f));
        }

        private void BuildSettings()
        {
            settingsScreen = CreateRect("Accessibility Settings", transform, Vector2.zero, Vector2.one).gameObject;
            var shade = settingsScreen.AddComponent<Image>();
            shade.color = new Color(0, 0, 0, .76f);
            var panel = CreateRect("Settings Panel", settingsScreen.transform, new Vector2(.30f, .12f), new Vector2(.70f, .88f));
            var panelImage = panel.gameObject.AddComponent<Image>();
            panelImage.color = Panel;
            var heading = CreateText(panel, "ACCESSIBILITY", 24, TextAnchor.MiddleLeft, Ice, FontStyle.Bold);
            SetRect(heading.rectTransform, new Vector2(.07f, .85f), new Vector2(.93f, .96f));
            var note = CreateText(panel, "Changes save locally and apply immediately.", 12, TextAnchor.MiddleLeft, Muted);
            SetRect(note.rectTransform, new Vector2(.07f, .78f), new Vector2(.93f, .86f));

            CreateSettingRow(panel, .66f, "REDUCE MOTION", () => manager.Profile.settings.reduceMotion, value => manager.Profile.settings.reduceMotion = value);
            CreateSettingRow(panel, .54f, "HIGH CONTRAST", () => manager.Profile.settings.highContrast, value => manager.Profile.settings.highContrast = value);
            CreateSettingRow(panel, .42f, "LARGE TEXT", () => manager.Profile.settings.largeText, value => manager.Profile.settings.largeText = value);
            CreateSettingRow(panel, .30f, "HOLD TO CHOOSE", () => manager.Profile.settings.holdToChoose, value => manager.Profile.settings.holdToChoose = value);
            CreateSettingRow(panel, .18f, "HAPTICS", () => manager.Profile.settings.haptics, value => manager.Profile.settings.haptics = value);
            CreateButton(panel, "RETURN TO TIMELINE", new Vector2(.07f, .045f), new Vector2(.93f, .14f), ToggleSettings, true, 14);
            settingsScreen.SetActive(false);
        }

        private void CreateSettingRow(RectTransform parent, float y, string label, Func<bool> getter, Action<bool> setter)
        {
            var row = CreateRect(label, parent, new Vector2(.07f, y), new Vector2(.93f, y + .09f));
            var labelText = CreateText(row, label, 14, TextAnchor.MiddleLeft, Ice, FontStyle.Bold);
            SetRect(labelText.rectTransform, Vector2.zero, new Vector2(.72f, 1));
            Text stateText = null;
            var button = CreateButton(row, "", new Vector2(.74f, .12f), new Vector2(1, .88f), () =>
            {
                setter(!getter());
                stateText.text = getter() ? "ON" : "OFF";
                manager.SaveSettings();
            }, false, 12);
            stateText = button.GetComponentInChildren<Text>();
            stateText.text = "—";
            settingsRefreshers.Add(() => stateText.text = manager.Profile != null && getter() ? "ON" : "OFF");
        }

        public void ShowLoading(string message)
        {
            SetScreen(loadingScreen);
            loadingText.text = message;
        }

        public void ShowFatal(string message)
        {
            SetScreen(loadingScreen);
            loadingText.text = "PROTOCOL ERROR\n\n" + message + "\n\nCheck the Console for details.";
            loadingText.fontSize = 24;
        }

        public void ShowTitle(bool canResume, Action start, Action resume)
        {
            SetScreen(titleScreen);
            SetCityStage(0);
            resumeAction = resume;
            resumeButton.gameObject.SetActive(canResume);
            if (manager.Profile != null)
            {
                titleRecord.text = manager.Profile.totalRuns == 0
                    ? "NO TIMELINES ARCHIVED"
                    : $"{manager.Profile.totalRuns:00} TIMELINES  ·  BEST INDEX {manager.Profile.bestTierScore:0}";
                ApplyAccessibility(manager.Profile.settings);
            }
        }

        public void ShowGame()
        {
            SetScreen(gameScreen);
            lastChapterShown = -1;
            ApplyAccessibility(manager.Profile.settings);
        }

        public void Present(CardDef definition, RunState state)
        {
            if (crisisClock != null)
            {
                StopCoroutine(crisisClock);
                crisisClock = null;
            }

            var cityStage = CityStageForSlot(state.slotIndex);
            backdropController.SetScene(cityStage, state.seed + state.slotIndex * 997, false);
            yearText.text = $"{state.year:0000} · ASTER LION";
            progressText.text = $"{state.slotIndex + 1:00} / {state.runPlan.Length:00}";
            var returningConsequence = definition.conditions?.requiredFlags != null && definition.conditions.requiredFlags.Length > 0;
            eraText.text = (definition.crisis ? "CRISIS INTERRUPT" : returningConsequence ? "PRIOR DECISION RETURNING" : definition.era + " ERA").ToUpperInvariant();
            eraText.color = definition.crisis ? Alert : Ice;
            speakerText.text = definition.speakerId == "arden" ? "LEE ARCHIVE" : definition.speakerName.ToUpperInvariant();
            roleText.text = definition.speakerId == "arden" ? "FOUNDING PREMIER SIMULATION" : definition.speakerRole.ToUpperInvariant();
            promptText.text = definition.prompt;
            historyText.text = "ARCHIVE ECHO // " + (string.IsNullOrWhiteSpace(definition.historicalInspiration)
                ? "SINGAPORE 1965–2015"
                : definition.historicalInspiration.ToUpperInvariant());
            leftChoiceText.text = "← " + definition.left.label.ToUpperInvariant();
            rightChoiceText.text = definition.right.label.ToUpperInvariant() + " →";
            var advisorIndex = AdvisorIndex(definition.speakerId);
            var specialMode = definition.crisis ? SpecialCardMode.Fight :
                state.slotIndex == 4 ? SpecialCardMode.Route :
                state.slotIndex == 10 ? SpecialCardMode.Concert : SpecialCardMode.None;
            specialCardGraphic.SetMode(specialMode);
            miniGameHint.text = specialMode == SpecialCardMode.Fight ? "INTERCEPT MODE // SWIPE THREE THREATS" :
                specialMode == SpecialCardMode.Route ? "ROUTE MODE // TURN THE ORBIT · RELEASE TO LOCK" :
                specialMode == SpecialCardMode.Concert ? "SIGNAL MODE // BEND THE WAVE · CHOOSE THE BEAT" : string.Empty;
            portraitArt.enabled = specialMode == SpecialCardMode.None;
            portraitMotion.enabled = specialMode == SpecialCardMode.None;
            if (advisorSprites != null && advisorSprites.Length > advisorIndex)
            {
                var blink = advisorBlinkSprites != null && advisorBlinkSprites.Length > advisorIndex ? advisorBlinkSprites[advisorIndex] : null;
                portraitMotion.SetFrames(advisorSprites[advisorIndex], blink, definition.crisis);
            }

            var panelColor = StagePanelColor(cityStage);
            promptPanel.color = panelColor;
            speakerPanel.color = panelColor;
            cardBackPanel.color = Color.Lerp(new Color32(21, 18, 27, 255), panelColor, .12f);
            Color panelText = Luminance(panelColor) > .52f ? new Color32(25, 20, 23, 255) : Ice;
            promptText.color = panelText;
            speakerText.color = panelText;
            roleText.color = Color.Lerp(panelText, panelColor, .35f);
            policyText.text = PolicyMemory(state.flags);
            cardCanvasGroup.alpha = 1;
            cardGraphic.borderColor = manager.Profile.settings.highContrast
                ? Color.white
                : definition.crisis ? Alert : Ice;
            cardGraphic.SetVerticesDirty();
            dragController.ResetCard();
            UpdateMeters(state.meters);

            crisisMode = definition.crisis;
            crisisStep = 0;
            crisisLeft = 0;
            crisisRight = 0;
            crisisHud.SetActive(crisisMode);
            if (crisisMode)
            {
                PrepareCrisisStep();
                backdropController.FlashDanger();
            }

            var chapterVisible = ShowChapterForSlot(state.slotIndex);
            dragController.SetEnabled(!chapterVisible);
            if (crisisMode && !chapterVisible) StartCrisisClock();
            StartCoroutine(AnimateCardIn(manager.Profile.settings.reduceMotion));
        }

        private static Color StagePanelColor(int stage)
        {
            switch (stage)
            {
                case 0: return new Color32(226, 174, 103, 250);
                case 1: return new Color32(222, 185, 132, 250);
                case 2: return new Color32(47, 105, 122, 250);
                default: return new Color32(57, 111, 92, 250);
            }
        }

        private static float Luminance(Color color) => color.r * .2126f + color.g * .7152f + color.b * .0722f;

        private void LoadArt()
        {
            var advisors = Resources.Load<Texture2D>("Art/advisor_blink_atlas_v3");
            if (advisors != null)
            {
                advisorSprites = new Sprite[8];
                advisorBlinkSprites = new Sprite[8];
                for (var i = 0; i < advisorSprites.Length; i++)
                {
                    var column = i % 4;
                    var openRow = i < 4 ? 3 : 1;
                    var blinkRow = i < 4 ? 2 : 0;
                    advisorSprites[i] = CreateCellSprite(advisors, column, openRow, 4, 4, 1.5f, "Advisor Open " + i);
                    advisorBlinkSprites[i] = CreateCellSprite(advisors, column, blinkRow, 4, 4, 1.5f, "Advisor Blink " + i);
                }
            }

            var cities = Resources.Load<Texture2D>("Art/city_atlas");
            if (cities != null)
            {
                citySprites = new[]
                {
                    CreateCellSprite(cities, 0, 1, 2, 2, 4f, "Survival Port"),
                    CreateCellSprite(cities, 1, 1, 2, 2, 4f, "Building City"),
                    CreateCellSprite(cities, 0, 0, 2, 2, 4f, "Global Hub"),
                    CreateCellSprite(cities, 1, 0, 2, 2, 4f, "Green World City")
                };
            }
        }

        private static Sprite CreateCellSprite(Texture2D texture, int column, int row, int columns, int rows, float gutter, string name)
        {
            var width = texture.width / (float)columns;
            var height = texture.height / (float)rows;
            var rect = new Rect(column * width + gutter, row * height + gutter, width - gutter * 2f, height - gutter * 2f);
            var sprite = Sprite.Create(texture, rect, new Vector2(.5f, .5f), 100f, 0, SpriteMeshType.FullRect);
            sprite.name = name;
            return sprite;
        }

        private int AdvisorIndex(string speakerId)
        {
            switch (speakerId)
            {
                case "arden":
                case "cabinet":
                case "choir": return 0;
                case "lim":
                case "koh": return 1;
                case "calyx":
                case "nox": return 2;
                case "saan":
                case "helix": return 3;
                case "jana":
                case "rusk": return 4;
                case "amina":
                case "mina":
                case "venn": return 5;
                case "rao":
                case "thorn": return 6;
                default: return 7;
            }
        }

        private void SetCityStage(int stage)
        {
            backdropController?.SetScene(stage, 2165 + stage * 97, true);
        }

        private static int CityStageForSlot(int slot)
        {
            if (slot >= 13) return 3;
            if (slot >= 9) return 2;
            if (slot >= 4) return 1;
            return 0;
        }

        private bool ShowChapterForSlot(int slot)
        {
            var chapter = CityStageForSlot(slot);
            if (chapter == lastChapterShown) return false;
            lastChapterShown = chapter;

            switch (chapter)
            {
                case 0:
                    chapterEyebrow.text = "CHAPTER I // CAST ADRIFT";
                    chapterTitle.text = "A PORT WITHOUT A COUNTRY";
                    chapterBody.text = "ARCHIVE BASIS: SINGAPORE, 1965.\n\nIn 2165, Aster Lion is expelled from its Federation. You inherit crowded docks, imported water, no army, and eleven weeks of oxygen credit. Fifty compressed years begin now.";
                    chapterButtonText.text = "BEGIN INDEPENDENCE";
                    break;
                case 1:
                    chapterEyebrow.text = "CHAPTER II // BUILD THE BASICS";
                    chapterTitle.text = "HOMES, JOBS, WATER";
                    chapterBody.text = "Public housing rises where leaking decks once stood. Foreign factories bring wages and dependency. Every reservoir, wage pact, and school language rewrites who belongs to the city.";
                    chapterButtonText.text = "ENTER THE BUILDING YEARS";
                    break;
                case 2:
                    chapterEyebrow.text = "CHAPTER III // CLIMB THE LADDER";
                    chapterTitle.text = "FROM WORKSHOP TO WORLD HUB";
                    chapterBody.text = "Cheap labour cannot carry a nation forever. Upgrade industry, educate a multilingual workforce, defend the port, and decide how much political control the transformation will cost.";
                    chapterButtonText.text = "ENTER THE GLOBAL ERA";
                    break;
                default:
                    chapterEyebrow.text = "CHAPTER IV // OUTLIVE THE FOUNDER";
                    chapterTitle.text = "INSTITUTIONS OR ONE MAN";
                    chapterBody.text = "The skyline can survive vacuum. The final question is whether the state can survive its founder. Hand power to institutions, or preserve the command system that built everything.";
                    chapterButtonText.text = "FACE THE FINAL AUDIT";
                    break;
            }

            chapterOverlay.SetActive(true);
            return true;
        }

        private void CompleteChapter()
        {
            chapterOverlay.SetActive(false);
            dragController.SetEnabled(true);
            if (crisisMode) StartCrisisClock();
        }

        private void CommitSide(ChoiceSide side)
        {
            entryDirection = side == ChoiceSide.Left ? 1 : -1;
            if (!crisisMode)
            {
                manager.Choose(side);
                return;
            }
            StartCoroutine(ResolveCrisisStep(side));
        }

        private void StartCrisisClock()
        {
            if (!crisisMode || crisisClock != null) return;
            crisisSeconds = 7.5f;
            crisisClock = StartCoroutine(CrisisCountdown());
        }

        private IEnumerator CrisisCountdown()
        {
            while (crisisMode && crisisSeconds > 0f)
            {
                crisisSeconds -= Time.unscaledDeltaTime;
                crisisTimerText.text = Mathf.Max(0f, crisisSeconds).ToString("0.0");
                yield return null;
            }
            crisisClock = null;
            if (crisisMode) FinishCrisis();
        }

        private void PrepareCrisisStep()
        {
            var index = Mathf.Clamp(crisisStep, 0, CrisisPrompts.Length - 1);
            promptText.text = CrisisPrompts[index];
            historyText.text = "RAPID SWIPE DRILL // THREE THREATS // ONE OUTCOME";
            crisisStepText.text = $"EMERGENCY {index + 1:00} / {CrisisPrompts.Length:00}";
            leftChoiceText.text = "← " + CrisisLeft[index];
            rightChoiceText.text = CrisisRight[index] + " →";
        }

        private IEnumerator ResolveCrisisStep(ChoiceSide side)
        {
            dragController.SetEnabled(false);
            if (side == ChoiceSide.Left) crisisLeft++;
            else crisisRight++;

            var from = card.anchoredPosition;
            var target = from + new Vector2(side == ChoiceSide.Left ? -80f : 80f, 18f);
            for (var elapsed = 0f; elapsed < .12f; elapsed += Time.unscaledDeltaTime)
            {
                var t = Mathf.Clamp01(elapsed / .12f);
                card.anchoredPosition = Vector2.Lerp(from, target, t);
                cardCanvasGroup.alpha = 1f - t;
                yield return null;
            }

            crisisStep++;
            cardCanvasGroup.alpha = 1f;
            dragController.ResetCard();
            if (crisisStep >= CrisisPrompts.Length)
            {
                FinishCrisis();
                yield break;
            }

            PrepareCrisisStep();
            dragController.SetEnabled(true);
        }

        private void FinishCrisis()
        {
            if (!crisisMode) return;
            crisisMode = false;
            if (crisisClock != null)
            {
                StopCoroutine(crisisClock);
                crisisClock = null;
            }
            dragController.SetEnabled(false);
            promptText.text = "Emergency sequence locked. The city will live with your doctrine.";
            crisisStepText.text = "SEQUENCE COMPLETE";
            crisisTimerText.text = "LOCK";
            manager.Choose(crisisRight > crisisLeft ? ChoiceSide.Right : ChoiceSide.Left);
        }

        private IEnumerator AnimateCardIn(bool reduceMotion)
        {
            if (reduceMotion)
            {
                card.localRotation = Quaternion.identity;
                card.localScale = Vector3.one;
                card.anchoredPosition = Vector2.zero;
                yield break;
            }

            // Exact turnleft/turnright reference shape: 0.5 seconds, starts at
            // 180 degrees, reaches x +/-50 and scale 1.2 at the midpoint.
            card.localRotation = Quaternion.Euler(0f, entryDirection * 180f, 0f);
            card.localScale = Vector3.one * .99f;
            card.anchoredPosition = Vector2.zero;
            for (var elapsed = 0f; elapsed < .5f; elapsed += Time.unscaledDeltaTime)
            {
                var t = Mathf.Clamp01(elapsed / .5f);
                var eased = t * t * (3f - 2f * t);
                var scale = t <= .5f ? Mathf.Lerp(.99f, 1.2f, t * 2f) : Mathf.Lerp(1.2f, 1f, (t - .5f) * 2f);
                card.localRotation = Quaternion.Euler(0f, entryDirection * Mathf.Lerp(180f, 0f, eased), 0f);
                card.localScale = new Vector3(scale, scale, t <= .5f ? Mathf.Lerp(1f, 1.2f, t * 2f) : Mathf.Lerp(1.2f, 1f, (t - .5f) * 2f));
                card.anchoredPosition = new Vector2(entryDirection * Mathf.Sin(t * Mathf.PI) * 50f, 0f);
                yield return null;
            }
            card.localRotation = Quaternion.identity;
            card.localScale = Vector3.one;
            card.anchoredPosition = Vector2.zero;
        }

        public IEnumerator AnimateCommit(ChoiceSide side, MeterState before, MeterState after, bool reduceMotion)
        {
            dragController.SetEnabled(false);
            var from = card.anchoredPosition;
            var direction = side == ChoiceSide.Left ? -1f : 1f;
            var duration = reduceMotion ? .04f : .25f;
            for (var elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                var t = Mathf.Clamp01(elapsed / duration);
                if (reduceMotion) card.anchoredPosition = from + new Vector2(direction * 420f * t, 0f);
                else
                {
                    var pos = card.anchoredPosition;
                    pos.x += direction * Time.unscaledDeltaTime * 2000f * t;
                    var travelled = Mathf.Abs(pos.x - from.x);
                    pos.y -= Time.unscaledDeltaTime * travelled * travelled * .011f;
                    card.anchoredPosition = pos;
                    card.localRotation *= Quaternion.Euler(0f, 0f, -direction * Time.unscaledDeltaTime * Mathf.Lerp(35f, 165f, t));
                }
                cardCanvasGroup.alpha = 1f - Mathf.InverseLerp(.62f, 1f, t);
                if (t > .35f) UpdateMeters(after);
                yield return null;
            }
            UpdateMeters(after);
            ClearPreview();
            if (!reduceMotion) yield return new WaitForSecondsRealtime(.12f);
            cardCanvasGroup.alpha = 1;
            dragController.ResetCard();
        }

        public void ShowEnding(EndingResult result, RunState run, Action replay, Action title)
        {
            SetScreen(endingScreen);
            SetCityStage(3);
            endingEyebrow.text = result.victory ? "GALACTIC TIER AUDIT // RECOGNIZED" : "GALACTIC TIER AUDIT // TIMELINE CLOSED";
            endingTitle.text = result.title.ToUpperInvariant();
            endingSummary.text = result.summary;
            endingScore.text = result.tierScore > 0 ? $"TIER INDEX {result.tierScore:0}" : $"COLLAPSED IN YEAR {run.year:0000}";
            endingMeters.text = $"● COHESION {run.meters.cohesion:00}    ▦ GROWTH {run.meters.growth:00}\n▲ SECURITY {run.meters.security:00}    ◉ AUTONOMY {run.meters.autonomy:00}";
            RemoveNamedChild(endingScreen.transform, "Replay Button");
            RemoveNamedChild(endingScreen.transform, "Archive Button");
            var replayButton = CreateButton(endingScreen.transform, "RUN ANOTHER TIMELINE", new Vector2(.32f, .16f), new Vector2(.68f, .235f), () => replay?.Invoke(), true, 12);
            replayButton.name = "Replay Button";
            var archiveButton = CreateButton(endingScreen.transform, "RETURN TO ARCHIVE", new Vector2(.32f, .075f), new Vector2(.68f, .14f), () => title?.Invoke(), false, 12);
            archiveButton.name = "Archive Button";
        }

        public void ApplyAccessibility(AccessibilitySettings settings)
        {
            if (settings == null) return;
            promptText.fontSize = settings.largeText ? 24 : basePromptSize;
            promptText.resizeTextMaxSize = promptText.fontSize;
            instructionText.gameObject.SetActive(!settings.holdToChoose);
            holdControls.SetActive(settings.holdToChoose && gameScreen.activeSelf);
            if (manager.Run != null) UpdateMeters(manager.Run.meters);
            cardGraphic.borderColor = settings.highContrast ? Color.white : Ice;
            cardGraphic.SetVerticesDirty();
        }

        private void PreviewChoice(ChoiceSide side, float strength)
        {
            if (manager.CurrentCard == null) return;
            portraitMotion.React(side, strength);
            specialCardGraphic.SetInput(side, strength);
            var effect = side == ChoiceSide.Left ? manager.CurrentCard.left.effects : manager.CurrentCard.right.effects;
            cohesion.SetPreview(effect.meters.cohesion);
            growth.SetPreview(effect.meters.growth);
            security.SetPreview(effect.meters.security);
            autonomy.SetPreview(effect.meters.autonomy);
        }

        private void ClearPreview()
        {
            portraitMotion.ClearReaction();
            specialCardGraphic.ClearInput();
            cohesion.ClearPreview();
            growth.ClearPreview();
            security.ClearPreview();
            autonomy.ClearPreview();
        }

        private void UpdateMeters(MeterState meters)
        {
            var highContrast = manager.Profile != null && manager.Profile.settings.highContrast;
            cohesion.SetValue(meters.cohesion, highContrast);
            growth.SetValue(meters.growth, highContrast);
            security.SetValue(meters.security, highContrast);
            autonomy.SetValue(meters.autonomy, highContrast);
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame && manager.Profile != null)
            {
                if (settingsScreen.activeSelf) ToggleSettings();
                else ToggleJourneyMenu();
            }
            if (chapterOverlay.activeSelf)
            {
                if (keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame))
                    CompleteChapter();
                return;
            }
            if (titleScreen.activeSelf && keyboard != null &&
                (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame))
            {
                manager.StartNewRun();
                return;
            }
            if (!gameScreen.activeSelf || settingsScreen.activeSelf || manager.IsResolving) return;
            if (keyboard != null && (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame)) dragController.CommitFromKeyboard(ChoiceSide.Left);
            if (keyboard != null && (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame)) dragController.CommitFromKeyboard(ChoiceSide.Right);
#else
            if (Input.GetKeyDown(KeyCode.Escape) && manager.Profile != null)
            {
                if (settingsScreen.activeSelf) ToggleSettings();
                else ToggleJourneyMenu();
            }
            if (chapterOverlay.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)) CompleteChapter();
                return;
            }
            if (titleScreen.activeSelf && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)))
            {
                manager.StartNewRun();
                return;
            }
            if (!gameScreen.activeSelf || settingsScreen.activeSelf || manager.IsResolving) return;
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) dragController.CommitFromKeyboard(ChoiceSide.Left);
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) dragController.CommitFromKeyboard(ChoiceSide.Right);
#endif
        }

        private void StartClicked() => manager.StartNewRun();

        private void ToggleSettings()
        {
            if (manager.Profile == null) return;
            settingsScreen.SetActive(!settingsScreen.activeSelf);
            if (settingsScreen.activeSelf)
                foreach (var refresh in settingsRefreshers) refresh();
            if (!settingsScreen.activeSelf)
            {
                manager.SaveSettings();
                if (gameScreen.activeSelf && !manager.IsResolving && !chapterOverlay.activeSelf)
                    dragController.SetEnabled(true);
            }
        }

        private void SetScreen(GameObject active)
        {
            loadingScreen.SetActive(active == loadingScreen);
            titleScreen.SetActive(active == titleScreen);
            gameScreen.SetActive(active == gameScreen);
            endingScreen.SetActive(active == endingScreen);
            settingsScreen.SetActive(false);
            if (journeyMenu != null) journeyMenu.SetActive(false);
        }

        private static string Initials(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "?";
            var words = value.Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries);
            return words.Length == 1 ? words[0].Substring(0, Mathf.Min(2, words[0].Length)).ToUpperInvariant() :
                (words[0][0].ToString() + words[words.Length - 1][0]).ToUpperInvariant();
        }

        private static string PolicyMemory(IReadOnlyList<string> flags)
        {
            if (flags == null || flags.Count <= 1) return "POLICY MEMORY // EMPTY";
            var start = Mathf.Max(1, flags.Count - 2);
            var memory = string.Empty;
            for (var i = start; i < flags.Count; i++)
            {
                if (memory.Length > 0) memory += "  ·  ";
                memory += flags[i].Replace('_', ' ').ToUpperInvariant();
            }
            return "POLICY MEMORY // " + memory;
        }

        private static void RemoveNamedChild(Transform parent, string childName)
        {
            var child = parent.Find(childName);
            if (child != null) Destroy(child.gameObject);
        }

        private static Button CreateButton(Transform parent, string label, Vector2 min, Vector2 max, UnityEngine.Events.UnityAction action,
            bool primary, int fontSize = 14)
        {
            var root = CreateRect(label + " Button", parent, min, max);
            var image = root.gameObject.AddComponent<Image>();
            image.color = primary ? new Color32(150, 55, 39, 245) : new Color32(54, 25, 31, 245);
            var button = root.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = primary ? new Color32(193, 76, 46, 255) : new Color32(91, 38, 43, 255);
            colors.pressedColor = new Color32(113, 42, 34, 255);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;
            button.onClick.AddListener(action);
            var text = CreateText(root, label, fontSize, TextAnchor.MiddleCenter, Ice, FontStyle.Bold);
            SetRect(text.rectTransform, new Vector2(.04f, .04f), new Vector2(.96f, .96f));
            return button;
        }

        private static void CreateHoldButton(Transform parent, string label, Vector2 min, Vector2 max, Action complete)
        {
            var root = CreateRect(label, parent, min, max);
            var image = root.gameObject.AddComponent<Image>();
            image.color = new Color32(54, 25, 31, 250);
            var fillRect = CreateRect("Hold Progress", root, Vector2.zero, Vector2.one);
            var fill = fillRect.gameObject.AddComponent<Image>();
            fill.color = new Color32(193, 76, 46, 170);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;
            fill.fillAmount = 0;
            fill.raycastTarget = false;
            var text = CreateText(root, label, 11, TextAnchor.MiddleCenter, Ice, FontStyle.Bold);
            SetRect(text.rectTransform, Vector2.zero, Vector2.one);
            var hold = root.gameObject.AddComponent<HoldChoiceButton>();
            hold.Configure(fill);
            hold.completed = complete;
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            var system = new GameObject("EventSystem");
            DontDestroyOnLoad(system);
            system.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            var inputModule = system.AddComponent<InputSystemUIInputModule>();
            inputModule.AssignDefaultActions();
#else
            system.AddComponent<StandaloneInputModule>();
#endif
        }

        public static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            SetRect(rect, anchorMin, anchorMax);
            return rect;
        }

        private static RectTransform CreateFixedRect(string name, Transform parent, Vector2 anchor, Vector2 size, Vector2 position)
        {
            var rect = CreateRect(name, parent, anchor, anchor);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            return rect;
        }

        public static Text CreateText(Transform parent, string value, int size, TextAnchor alignment, Color color, FontStyle style = FontStyle.Normal)
        {
            var rect = CreateRect("Text", parent, Vector2.zero, Vector2.one);
            var text = rect.gameObject.AddComponent<Text>();
            text.font = GetFont();
            text.text = value;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = color;
            text.fontStyle = style;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }

        public static void SetRect(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static Font GetFont()
        {
            if (sharedFont != null) return sharedFont;
            sharedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (sharedFont == null) sharedFont = Font.CreateDynamicFontFromOSFont(new[] { "Avenir Next", "Helvetica Neue", "Arial" }, 24);
            return sharedFont;
        }
    }
}
