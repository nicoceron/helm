using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SVGImporter;
using UnityEngine;
using UnityEngine.UI;

public class PlanetButton : MonoBehaviour
{
	public enum states
	{
		opened = 0,
		closed = 1,
		activated = 2,
		transition = 3,
		hidden = 4,
		none = 5
	}

	private RectTransform mytrans;

	public GameObject arrow;

	public Card available;

	public Color iconActive;

	public Color iconDeactive;

	public Color fondActive;

	public Color fondDeactive;

	private new string name;

	public SVGImage fond;

	public RectTransform bottom;

	public RectTransform top;

	public SVGImage icon;

	private Selectable selectable;

	public List<Card> mycards = new List<Card>();

	private List<string> othernames;

	private states _state = states.none;

	protected states _oldstate;

	public bool alreadyseen;

	private AutoSelectMe scAuto;

	private bool hasArrow;

	private bool isSelecting;

	public states state
	{
		get
		{
			return _state;
		}
		set
		{
			ConfigureState(value);
		}
	}

	private void ConfigureState(states newstate)
	{
		_oldstate = _state;
		_state = newstate;
		if (state == states.closed || state == states.hidden)
		{
			arrow.SetActive(value: false);
		}
		else if (hasArrow)
		{
			ActivateButton(first: true);
		}
		if (_oldstate == state)
		{
			return;
		}
		if (_oldstate == states.activated && (name == "concert" || name == "bar"))
		{
			if (available != null)
			{
				if (available.weight < 10000 && available.weight > -1)
				{
					alreadyseen = true;
				}
			}
			else
			{
				alreadyseen = true;
			}
		}
		if (alreadyseen)
		{
			state = states.closed;
		}
		if (state != states.opened)
		{
			DeactivateButton();
		}
		else
		{
			selectable.enabled = true;
		}
		if (state != states.transition && state != states.none)
		{
			if (state == states.closed || state == states.hidden)
			{
				top.DOAnchorPosY(16f, 0.4f).SetEase(Ease.OutSine);
				bottom.DOAnchorPosY(-3f, 0.4f).SetEase(Ease.OutSine);
			}
			else
			{
				top.DOAnchorPosY(40f, 0.4f).SetEase(Ease.OutSine);
				bottom.DOAnchorPosY(-30f, 0.4f).SetEase(Ease.OutSine);
			}
			if (state == states.activated)
			{
				icon.DOColor(iconActive, 0.4f);
				fond.DOColor(fondActive, 0.4f);
				MoneyUI.diff.SetDefaultPosition(name);
			}
			else
			{
				icon.DOColor(iconDeactive, 0.4f);
				fond.DOColor(fondDeactive, 0.4f);
			}
		}
	}

	public void OnDisable()
	{
		DeactivateButton();
		state = states.none;
		_oldstate = states.none;
		GameAct diff = GameAct.diff;
		diff.OnNewCard = (Action<Card>)Delegate.Remove(diff.OnNewCard, new Action<Card>(CheckAvailability));
		GameAct diff2 = GameAct.diff;
		diff2.OnDataChange = (Action<Variables, int>)Delegate.Remove(diff2.OnDataChange, new Action<Variables, int>(CheckHidden));
	}

	public void OnEnable()
	{
		mytrans = GetComponent<RectTransform>();
		scAuto = GetComponent<AutoSelectMe>();
		available = null;
		name = base.transform.name;
		mycards = GameAct.diff.GetHiddenCards("_" + name);
		mycards.RemoveAll((Card it) => it.place != BackgroundAct.diff.curBack.type);
		string pname = BackgroundAct.diff.nameBack;
		List<Card> list = mycards.FindAll((Card it) => it.place_name == pname);
		if (list.Count > 0)
		{
			mycards = list;
		}
		selectable = GetComponent<Selectable>();
		othernames = new List<string>();
		foreach (Transform item in base.transform.parent)
		{
			if (item.name != name)
			{
				othernames.Add(item.name);
			}
		}
		CheckHidden(Variables.hide, GameAct.diff.GetInt(Variables.hide));
		GameAct diff = GameAct.diff;
		diff.OnNewCard = (Action<Card>)Delegate.Combine(diff.OnNewCard, new Action<Card>(CheckAvailability));
		GameAct diff2 = GameAct.diff;
		diff2.OnDataChange = (Action<Variables, int>)Delegate.Combine(diff2.OnDataChange, new Action<Variables, int>(CheckHidden));
	}

	private void CheckHidden(Variables var, int value)
	{
		if (var != Variables.hide)
		{
			return;
		}
		if (value == 1)
		{
			if (state != states.hidden && state != states.activated)
			{
				state = states.hidden;
			}
		}
		else
		{
			state = _oldstate;
		}
		CheckAvailability(GameAct.diff.card);
	}

	private void CheckAvailability(Card card)
	{
		hasArrow = card.name.EndsWith("default") && card.bearer == Bearers.intercale;
		if (!hasArrow)
		{
			arrow.SetActive(value: false);
		}
		if ((!name.Equals("shipyard") || !hasArrow) && state == states.hidden)
		{
			return;
		}
		if (state == states.activated && !hasArrow)
		{
			bool flag = true;
			foreach (string othername in othernames)
			{
				if (card.name.Contains(othername))
				{
					flag = false;
				}
			}
			if (flag)
			{
				return;
			}
		}
		if (card.name.Contains(name))
		{
			state = states.activated;
			return;
		}
		if (mycards.Count == 0)
		{
			available = null;
			state = states.closed;
			return;
		}
		available = GameAct.diff.ProcessCards(mycards, smallbatch: true, failsafe: false);
		if (available != null)
		{
			bool flag2 = NavigationAct.diff.HasFacility(BackgroundAct.diff.GetNextName(), name);
			if (available.weight > 0 && available.weight < 10000 && !flag2)
			{
				state = states.closed;
			}
			else
			{
				state = states.opened;
			}
		}
		else
		{
			state = states.closed;
		}
	}

	public void ActivateButton(bool first)
	{
		StopCoroutine("DoActivateButton");
		StartCoroutine("DoActivateButton", first);
	}

	private IEnumerator DoActivateButton(bool first)
	{
		while (GameAct.diff.state != GameStates.interaction)
		{
			yield return 0;
		}
		arrow.SetActive(value: true);
		selectable.enabled = true;
		bool flag = InputAct.diff.NavigationMode();
		if (name.Equals("shipyard") && flag)
		{
			InputAct.diff.OpenInventory();
		}
		if (first && (!InputAct.diff || flag))
		{
			if (scAuto.enabled)
			{
				scAuto.Activate();
			}
			else
			{
				scAuto.enabled = true;
			}
		}
	}

	private void DeactivateButton()
	{
		StopCoroutine("DoActivateButton");
		selectable.enabled = false;
		scAuto.enabled = false;
		mytrans.DOKill();
		mytrans.DOScale(new Vector3(1f, 1f, 1f), 0.2f);
	}

	public void SelectButton()
	{
		if (state == states.opened)
		{
			isSelecting = true;
			mytrans.DOKill();
			mytrans.DOScale(new Vector3(1.1f, 1.1f, 1f), 0.3f).OnComplete(delegate
			{
				isSelecting = false;
			});
		}
	}

	public void UnSelectButton()
	{
		isSelecting = false;
		mytrans.DOKill();
		mytrans.DOScale(new Vector3(1f, 1f, 1f), 0.2f);
	}

	public void ValidChoice()
	{
		if (state == states.opened && !InputAct.diff.isInMenu)
		{
			if (isSelecting)
			{
				StopCoroutine("YieldValid");
				StartCoroutine("YieldValid");
			}
			else
			{
				DoValid();
			}
		}
	}

	private IEnumerator YieldValid()
	{
		while (isSelecting)
		{
			yield return 0;
		}
		while (GameAct.diff.state != GameStates.interaction)
		{
			yield return 0;
		}
		DoValid();
	}

	private void DoValid()
	{
		if (available == null)
		{
			state = states.closed;
			return;
		}
		switch (name)
		{
		case "shipyard":
			JukeBox.diff.PlaySound(SFXTypes.ui_button_location_ship);
			break;
		case "shop":
			JukeBox.diff.PlaySound(SFXTypes.ui_button_location_shop);
			break;
		case "bar":
			JukeBox.diff.PlaySound(SFXTypes.ui_button_location_bar);
			break;
		case "concert":
			JukeBox.diff.PlaySound(SFXTypes.ui_button_location_concert);
			break;
		}
		if (GameAct.diff.cardType != CardTypes.end)
		{
			GameAct.diff.OpenCard(available);
			state = states.transition;
			InputAct.diff.DisableMenuNav(closewindows: false, ignoreanimstate: true);
			InputAct.diff.RestoreSlideFocus();
		}
	}

	public void DisableChoice(bool andremove = true)
	{
		DeactivateButton();
	}
}
