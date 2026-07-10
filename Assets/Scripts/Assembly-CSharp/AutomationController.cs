using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AutomationController
{
	public enum CardSlideDirection
	{
		None = 0,
		Left = 1,
		Right = 2
	}

	public enum PlanetAction
	{
		None = 0,
		Concert = 1,
		Bar = 2,
		Shipyard = 3,
		Shop = 4
	}

	public class CardInteractionState
	{
		public float InteractionStartUnscaledTime;

		public bool HasTakenAction;

		public bool CanValidateMap;

		public bool CanValidateAction;

		public CardSlideDirection SlideDirection;
	}

	public const float InteractionResetDelay = 8f;

	public const float ActionDelay = 0.5f;

	public const float SelectionDelay = 0.25f;

	public const float SelectSlideDirectionDelay = 0.1f;

	public static AutomationController Instance;

	public CardInteractionState InteractionState;

	public AutomationLogger Logger;

	public int DebugFramecount;

	private bool fastForwardActive;

	private Dictionary<PlanetAction, PlanetButton> planetButtons;

	private IAutomationDecisionScript decisionScript;

	public AutomationRuntimeParameters Parameters => GameAct.diff.AutomationRuntimeParameters;

	public bool SlideLeft => InteractionState.SlideDirection == CardSlideDirection.Left;

	public bool SlideRight => InteractionState.SlideDirection == CardSlideDirection.Right;

	public bool Active => Parameters.Active;

	public bool AutoSlide => Parameters.AutoSlide;

	public bool AutoAction => Parameters.AutoActions;

	public AutomationController()
	{
		InteractionState = new CardInteractionState();
		Logger = new AutomationLogger();
		ResetInteractionState();
		planetButtons = new Dictionary<PlanetAction, PlanetButton>();
		PlanetButton[] source = Object.FindObjectsOfType<PlanetButton>(includeInactive: true);
		planetButtons[PlanetAction.Concert] = source.FirstOrDefault((PlanetButton b) => b.gameObject.name == "concert");
		planetButtons[PlanetAction.Bar] = source.FirstOrDefault((PlanetButton b) => b.gameObject.name == "bar");
		planetButtons[PlanetAction.Shipyard] = source.FirstOrDefault((PlanetButton b) => b.gameObject.name == "shipyard");
		planetButtons[PlanetAction.Shop] = source.FirstOrDefault((PlanetButton b) => b.gameObject.name == "shop");
		decisionScript = Parameters.DecisionScript;
	}

	public void OnValidateDecision(int decision)
	{
		Util.Write("OnValidateDecision");
		Logger.RecordDecision(GameAct.diff.card, (decision == -1) ? CardSlideDirection.Left : CardSlideDirection.Right);
		ResetInteractionState();
	}

	public void OnValidateSelection(Card card)
	{
		Util.Write("OnValidateSelection");
		Logger.RecordSelection(card);
		ResetInteractionState();
	}

	public void ResetInteractionState()
	{
		InteractionState.HasTakenAction = false;
		InteractionState.CanValidateMap = false;
		InteractionState.CanValidateAction = false;
		InteractionState.SlideDirection = CardSlideDirection.None;
		InteractionState.InteractionStartUnscaledTime = Time.unscaledTime;
	}

	public IEnumerator UpdateCoroutine()
	{
		while (true)
		{
			UpdateTimeScale();
			if (Time.unscaledTime > InteractionState.InteractionStartUnscaledTime + 0.1f && InteractionState.SlideDirection == CardSlideDirection.None)
			{
				if (GameAct.diff.card != null)
				{
					InteractionState.SlideDirection = decisionScript.SelectSlideDirection(GameAct.diff.card);
				}
				if (InteractionState.SlideDirection == CardSlideDirection.None)
				{
					InteractionState.SlideDirection = ((Random.value > 0.5f) ? CardSlideDirection.Left : CardSlideDirection.Right);
				}
			}
			if (Time.unscaledTime > InteractionState.InteractionStartUnscaledTime + 0.25f && GameAct.diff.selection.Count > 0)
			{
				CharacterCard characterCard = decisionScript.SelectCard(GameAct.diff.selection);
				if (characterCard != null && characterCard.choiceBut.activeInHierarchy)
				{
					Logger.RecordUIAction("Selection Action");
					InteractionState.HasTakenAction = true;
					characterCard.SelectChoice();
					characterCard.ValidChoice();
					characterCard.UnSelectChoice();
					yield return null;
					continue;
				}
			}
			if (Time.unscaledTime > InteractionState.InteractionStartUnscaledTime + 0.5f && AutoAction && !InteractionState.HasTakenAction)
			{
				MapCard mapCard = GameObject.Find("map")?.GetComponent<MapCard>();
				if (mapCard != null && mapCard.routeshown)
				{
					Logger.RecordUIAction("Map Action");
					InteractionState.HasTakenAction = true;
					InteractionState.CanValidateMap = true;
					yield return null;
					continue;
				}
				List<PlanetAction> list = GetAvailablePlanetActions().ToList();
				if (list.Count > 0)
				{
					bool num = Logger.CardsSwiped.Count > 0 && Logger.CardsSwiped.Last().id == 52;
					PlanetAction key = decisionScript.SelectPlanetAction(list);
					PlanetButton planetButton = planetButtons[key];
					if (!num && planetButton != null && planetButton.available != null && planetButton.state == PlanetButton.states.opened)
					{
						Logger.RecordUIAction("Planet action: " + planetButton.gameObject.name);
						InteractionState.HasTakenAction = true;
						planetButton.SelectButton();
						planetButton.ValidChoice();
						planetButton.UnSelectButton();
						yield return null;
						ResetInteractionState();
						continue;
					}
				}
				ConcertCard concertCard = Object.FindObjectOfType<ConcertCard>();
				GameObject gameObject = DeadCloneAct.diff.gameObject;
				if (concertCard != null || gameObject.gameObject.activeInHierarchy)
				{
					InteractionState.HasTakenAction = true;
					InteractionState.CanValidateAction = true;
					if (concertCard == null)
					{
						InputAct.diff.TapAction();
					}
					continue;
				}
			}
			if (Time.unscaledTime > InteractionState.InteractionStartUnscaledTime + 8f)
			{
				Util.Write("Reset Automation Interaction State");
				ResetInteractionState();
			}
			yield return null;
		}
	}

	private void UpdateTimeScale()
	{
		if (GameAct.diff.AutomationRuntimeParameters.FastForward && !fastForwardActive)
		{
			fastForwardActive = true;
			Time.timeScale = 10f;
		}
		if (!GameAct.diff.AutomationRuntimeParameters.FastForward && fastForwardActive)
		{
			fastForwardActive = false;
			Time.timeScale = 1f;
		}
	}

	public IEnumerable<PlanetAction> GetAvailablePlanetActions()
	{
		foreach (PlanetAction key in planetButtons.Keys)
		{
			if (planetButtons[key] != null && planetButtons[key].gameObject.activeInHierarchy && planetButtons[key].state == PlanetButton.states.opened)
			{
				yield return key;
			}
		}
	}
}
