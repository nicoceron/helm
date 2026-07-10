using System;
using System.Collections;
using UnityEngine;

public class SpecialAct : MonoBehaviour
{
	private void Start()
	{
		GameAct diff = GameAct.diff;
		diff.OnValidateDecision = (Action<int>)Delegate.Combine(diff.OnValidateDecision, new Action<int>(Decision));
		GameAct diff2 = GameAct.diff;
		diff2.OnRefresh = (Action<Card>)Delegate.Combine(diff2.OnRefresh, new Action<Card>(NewCard));
	}

	private void Decision(int decision)
	{
		Card card = GameAct.diff.card;
		if (card != null)
		{
			switch (card.name)
			{
			case "enablenotification":
				_ = 1;
				break;
			case "ratereigns":
				_ = 1;
				break;
			}
		}
	}

	private void NewCard(Card card)
	{
		if (card.id == 10)
		{
			SocialAct.diff.AddAchieve("reigns.arcadeFTUX.beyond");
			return;
		}
		switch (card.bearer)
		{
		case Bearers.sandman:
			PlayStep(SFXTypes.sfx_ship_footsteps);
			return;
		case Bearers.dungeon:
			PlayStep(SFXTypes.sfx_footstep_dungeon);
			return;
		}
		if (GameAct.diff.GetInt(Variables.distance) > 2000)
		{
			SocialAct.diff.AddAchieve("journeyBeyond");
		}
		if (GameAct.diff.GetBool("galaxy"))
		{
			SocialAct.diff.AddAchieve("collectorBeyond");
		}
	}

	private void PlayStep(SFXTypes type)
	{
		JukeBox.diff.PlayAttenuatedSound(type, Util.Rand(0.5f));
	}

	private IEnumerator DoPlayStep()
	{
		yield return new WaitForSeconds(Util.Rand(0.1f, 0.25f));
		if (GameAct.diff.card == null || GameAct.diff.card.bearer == Bearers.sandman)
		{
			JukeBox.diff.PlayAttenuatedSound(SFXTypes.sfx_ship_footsteps, Util.Rand(0.5f));
			yield return new WaitForSeconds(Util.Rand(0.35f, 0.45f));
			if (GameAct.diff.card == null || GameAct.diff.card.bearer == Bearers.sandman)
			{
				JukeBox.diff.PlayAttenuatedSound(SFXTypes.sfx_ship_footsteps, Util.Rand(0.25f, 0.75f));
			}
		}
	}
}
