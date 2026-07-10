using System;
using System.Collections.Generic;
using UnityEngine;

public class MetersAct : MonoBehaviour
{
	public static MetersAct diff;

	public List<DataAnim> meters;

	private Dictionary<Variables, DataAnim> variable2meters = new Dictionary<Variables, DataAnim>();

	public Func<Outcome, int> OnShowOutcome;

	private void Start()
	{
		GameAct gameAct = GameAct.diff;
		gameAct.OnRefresh = (Action<Card>)Delegate.Remove(gameAct.OnRefresh, new Action<Card>(UpdateMeters));
		GameAct gameAct2 = GameAct.diff;
		gameAct2.OnRefresh = (Action<Card>)Delegate.Combine(gameAct2.OnRefresh, new Action<Card>(UpdateMeters));
	}

	private void Awake()
	{
		diff = this;
		foreach (DataAnim meter in meters)
		{
			variable2meters.Add(meter.variable, meter);
		}
	}

	public void Deactivate()
	{
		base.gameObject.SetActive(value: false);
	}

	public void Activate()
	{
		base.gameObject.SetActive(value: true);
	}

	private void UpdateMeters(Card card)
	{
		foreach (DataAnim meter in meters)
		{
			if (!meter.isShown && card.name != null && (card.name.Contains("show_" + meter.variable) || card.name.Contains("show_all")))
			{
				meter.ShowDataCol(yes: true);
			}
		}
	}

	public void CheckDanger()
	{
		foreach (DataAnim meter in meters)
		{
			meter.UpdateDanger();
		}
	}

	public void ShowAllData(bool yes)
	{
		foreach (DataAnim meter in meters)
		{
			meter.ShowDataCol(yes);
		}
	}

	public void SetDefault()
	{
	}

	public void ShowOutcome(List<Outcome> outcome, Bearer be, bool andresolve = false)
	{
		List<Outcome> list = new List<Outcome>(outcome);
		if (be != null)
		{
			_ = be.vote;
			_ = 0f;
		}
		int num = 10;
		foreach (DataAnim me in meters)
		{
			Outcome outcome2 = list.Find((Outcome it) => it.variable == me.variable);
			if (outcome2 != null)
			{
				int value = ((OnShowOutcome != null) ? OnShowOutcome(outcome2) : outcome2.value);
				me.SetAdd(value, outcome2.display, num);
			}
			else
			{
				me.SetAdd(0);
			}
			if (andresolve)
			{
				GameAct.diff.SetInt(me.variable, me.ResolveAddition());
			}
		}
	}

	public void ResolveAddition()
	{
		foreach (DataAnim meter in meters)
		{
			GameAct.diff.SetInt(meter.variable, meter.ResolveAddition());
		}
	}

	public void SendEffect(Variables va, Effect effect)
	{
		variable2meters[va].OpenEffect(effect);
	}
}
