using System;
using UnityEngine;

public class TutoAct : MonoBehaviour
{
	private const string FirstDecisionCard = "_separation";

	public GameObject scmod;

	private bool ghostly;

	private int decision;

	private float timeChoice;

	private void Start()
	{
		GameAct diff = GameAct.diff;
		diff.OnCharacter = (Action<Bearers>)Delegate.Combine(diff.OnCharacter, new Action<Bearers>(StartCheck));
		GameAct diff2 = GameAct.diff;
		diff2.OnCharacter = (Action<Bearers>)Delegate.Combine(diff2.OnCharacter, new Action<Bearers>(NewCard));
		GameAct diff3 = GameAct.diff;
		diff3.OnChoice = (Action<int>)Delegate.Combine(diff3.OnChoice, new Action<int>(UpdateTime));
	}

	private void UpdateTime(int decision)
	{
		if (decision == 1 || decision == -1)
		{
			timeChoice = Time.realtimeSinceStartup;
		}
	}

	private void NewCard(Bearers bear)
	{
		if (!ghostly && GameAct.diff.card.name == FirstDecisionCard)
		{
			ghostly = true;
			GameAct.diff.ShowDataCol(yes: true);
			timeChoice = Time.realtimeSinceStartup;
		}
		else if (ghostly)
		{
			string curCardName = GameAct.diff.GetCurCardName();
			if (curCardName == "_7")
			{
				Disable();
			}
			else if (InputAct.diff.curInput == Inputs.touch && curCardName == "_5" && Time.realtimeSinceStartup - timeChoice < 0.5f)
			{
				GameAct.diff.PlayModal(ModalTypes.custom, scmod, 10f, "", decal: false);
				Disable();
			}
			else if (curCardName != FirstDecisionCard)
			{
				Disable();
			}
		}
		else if (!ghostly && !IsBriefingCard(GameAct.diff.card.name))
		{
			Disable();
		}
	}

	private static bool IsBriefingCard(string cardName)
	{
		return cardName == "first_card" || cardName.StartsWith("_briefing_", StringComparison.Ordinal);
	}

	private void Disable()
	{
		ghostly = false;
		GameAct diff = GameAct.diff;
		diff.OnCharacter = (Action<Bearers>)Delegate.Remove(diff.OnCharacter, new Action<Bearers>(StartCheck));
		GameAct diff2 = GameAct.diff;
		diff2.OnCharacter = (Action<Bearers>)Delegate.Remove(diff2.OnCharacter, new Action<Bearers>(NewCard));
		GameAct diff3 = GameAct.diff;
		diff3.OnValidateDecision = (Action<int>)Delegate.Remove(diff3.OnValidateDecision, new Action<int>(OutBut));
		GameAct diff4 = GameAct.diff;
		diff4.OnChoice = (Action<int>)Delegate.Remove(diff4.OnChoice, new Action<int>(ControlCheck));
		GameAct diff5 = GameAct.diff;
		diff5.OnChoice = (Action<int>)Delegate.Remove(diff5.OnChoice, new Action<int>(UpdateTime));
	}

	private void ControlCheck(int dec)
	{
		if (dec == decision)
		{
			return;
		}
		decision = dec;
		if (InputAct.diff.isSimulating || !InputAct.diff.NavigationMode())
		{
			return;
		}
		if (decision == 0)
		{
			if ((bool)AnimBut.diff)
			{
				AnimBut.diff.Lock();
			}
		}
		else if ((bool)AnimBut.diff)
		{
			AnimBut.diff.UnLock(ControlModes.next, withoutbut: true);
		}
	}

	private void OutBut(int dec)
	{
		if ((bool)AnimBut.diff)
		{
			AnimBut.diff.Lock();
		}
		GameAct diff = GameAct.diff;
		diff.OnChoice = (Action<int>)Delegate.Remove(diff.OnChoice, new Action<int>(ControlCheck));
		GameAct diff2 = GameAct.diff;
		diff2.OnValidateDecision = (Action<int>)Delegate.Remove(diff2.OnValidateDecision, new Action<int>(OutBut));
		decision = 0;
	}

	private void StartCheck(Bearers bear)
	{
		if (!(GameAct.diff.card.name != FirstDecisionCard))
		{
			InputAct.diff.Simulate(-0.5f, 0.5f);
			GameAct diff = GameAct.diff;
			diff.OnValidateDecision = (Action<int>)Delegate.Combine(diff.OnValidateDecision, new Action<int>(OutBut));
			GameAct diff2 = GameAct.diff;
			diff2.OnChoice = (Action<int>)Delegate.Combine(diff2.OnChoice, new Action<int>(ControlCheck));
		}
	}
}
