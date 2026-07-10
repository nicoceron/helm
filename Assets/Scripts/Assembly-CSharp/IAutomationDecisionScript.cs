using System.Collections.Generic;

public interface IAutomationDecisionScript
{
	AutomationController.CardSlideDirection SelectSlideDirection(Card card);

	CharacterCard SelectCard(List<CharacterCard> selection);

	AutomationController.PlanetAction SelectPlanetAction(List<AutomationController.PlanetAction> availableActions);
}
