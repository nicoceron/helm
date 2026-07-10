using System.Collections.Generic;
using UnityEngine;

public class TestCharact : MonoBehaviour
{
	public Bearers curBearer;

	private List<CharacterCard> cards = new List<CharacterCard>();

	private BearerGen model;

	private void Start()
	{
		foreach (Transform item in base.transform)
		{
			cards.Add(item.GetComponent<CharacterCard>());
		}
		model = CardReader.diff.bearerGenModels.Find((BearerGen it) => it.bearer == curBearer);
		UpdateCards();
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.R))
		{
			UpdateCards();
		}
	}

	private void UpdateCards()
	{
		foreach (CharacterCard card in cards)
		{
			_ = card;
		}
	}
}
