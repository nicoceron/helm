using UnityEngine;

[CreateAssetMenu(menuName = "Automation/Parameters")]
public class AutomationRuntimeParameters : ScriptableObject
{
	public bool Active;

	public bool AutoSlide;

	public bool AutoActions;

	public bool FastForward;

	public ScriptedDecisionScript DecisionScript;
}
