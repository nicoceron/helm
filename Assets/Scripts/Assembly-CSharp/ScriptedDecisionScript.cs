using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Garry/AutomationScript")]
public class ScriptedDecisionScript : ScriptableObject, IAutomationDecisionScript
{
	[Serializable]
	public class SlideScriptEntry
	{
		public string CardName;

		public int CardId;

		public AutomationController.CardSlideDirection Direction;
	}

	[Serializable]
	public class CardEntry
	{
		public string CardName;

		public int CardId;
	}

	public List<SlideScriptEntry> SlideDirections;

	public List<CardEntry> AlwaysBuy;

	public AutomationController.CardSlideDirection SelectSlideDirection(Card card)
	{
		foreach (SlideScriptEntry slideDirection in SlideDirections)
		{
			if (slideDirection.CardName != null && slideDirection.CardName != "" && slideDirection.CardName == card.name)
			{
				return slideDirection.Direction;
			}
			if (slideDirection.CardId != 0 && slideDirection.CardId == card.id)
			{
				return slideDirection.Direction;
			}
		}
		foreach (CardEntry item in AlwaysBuy)
		{
			if (item.CardName != null && item.CardName != "" && item.CardName == card.name)
			{
				return AutomationController.CardSlideDirection.Right;
			}
			if (item.CardId != 0 && item.CardId == card.id)
			{
				return AutomationController.CardSlideDirection.Right;
			}
		}
		return AutomationController.CardSlideDirection.None;
	}

	public CharacterCard SelectCard(List<CharacterCard> selection)
	{
		foreach (CardEntry e in AlwaysBuy)
		{
			if (e.CardName != null && e.CardName != "")
			{
				CharacterCard characterCard = selection.FirstOrDefault((CharacterCard s) => e.CardName == s.CurrentCard.name);
				if (characterCard != null)
				{
					return characterCard;
				}
			}
			else if (e.CardId > 0)
			{
				CharacterCard characterCard2 = selection.FirstOrDefault((CharacterCard s) => e.CardId == s.CurrentCard.id);
				if (characterCard2 != null)
				{
					return characterCard2;
				}
			}
		}
		int index = UnityEngine.Random.Range(0, selection.Count);
		return selection[index];
	}

	public AutomationController.PlanetAction SelectPlanetAction(List<AutomationController.PlanetAction> availableActions)
	{
		int index = UnityEngine.Random.Range(0, availableActions.Count);
		return availableActions[index];
	}
}
