using System.Collections.Generic;
using UnityEngine;

public class RandomDecisionScript : IAutomationDecisionScript
{
	public AutomationController.CardSlideDirection SelectSlideDirection(Card card)
	{
		if (!(Random.value > 0.5f))
		{
			return AutomationController.CardSlideDirection.Right;
		}
		return AutomationController.CardSlideDirection.Left;
	}

	public CharacterCard SelectCard(List<CharacterCard> selection)
	{
		int index = Random.Range(0, selection.Count);
		return selection[index];
	}

	public AutomationController.PlanetAction SelectPlanetAction(List<AutomationController.PlanetAction> availableActions)
	{
		int index = Random.Range(0, availableActions.Count);
		return availableActions[index];
	}
}
