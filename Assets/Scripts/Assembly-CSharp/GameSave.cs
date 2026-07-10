using System;
using System.Collections.Generic;

[Serializable]
public class GameSave
{
	public bool isressurection;

	public int currentCard;

	public int ressurectCard;

	public long time;

	public Backgrounds place;

	public string place_name;

	public string place_last;

	public List<string> place_cache;

	public CardSave[] cards;

	public List<DataVariable> datavar;

	public List<DataCustom> datacustom;

	public List<PostponeEvent> postponeEvents;

	public List<NavPoint> navigation;

	public List<NavPoint> goals;

	public List<BearerSave> bearers;

	public List<ObjectiveSave> objectives;

	public List<JourneySave> journeys;

	public List<string> stats;

	public string nickname;

	public int goaltoremove;

	public string device;

	public GameSave(GameAct scGame, bool withresurrect)
	{
		if (!withresurrect && scGame.card != null)
		{
			currentCard = scGame.card.id;
			ressurectCard = scGame.card.id;
		}
		else
		{
			isressurection = true;
			List<Card> hiddenCards = scGame.GetHiddenCards("_resurrection");
			Card card = scGame.ProcessCards(hiddenCards, smallbatch: true);
			currentCard = ((scGame.card == null) ? (-1) : scGame.card.id);
			ressurectCard = card?.id ?? 163;
		}
		device = "Mac";
		place = BackgroundAct.diff.curBack.type;
		place_name = BackgroundAct.diff.nameBack;
		place_last = BackgroundAct.diff.lastBack;
		place_cache = BackgroundAct.diff.placeCache;
		journeys = DeadCloneAct.diff.journeys;
		time = DateTime.UtcNow.Ticks;
		nickname = ObjectiveAct.diff.nickname;
		List<Card> list = new List<Card>(scGame.GetCards());
		list.AddRange(scGame.GetHiddenCards());
		int num = 0;
		foreach (Card item in list)
		{
			if (item.id > num)
			{
				num = item.id;
			}
		}
		cards = new CardSave[num + 1];
		foreach (Card item2 in list)
		{
			CardSave cardSave = new CardSave(item2.nextturn, item2.weight, item2.isLocked, item2.wasSeen, item2.weightReal);
			cards[item2.id] = cardSave;
		}
		List<Bearer> list2 = GameAct.diff.bearers;
		bearers = new List<BearerSave>();
		foreach (Bearer item3 in list2)
		{
			bearers.Add(new BearerSave(item3.bearer, item3.vote, item3.name, item3.character));
		}
		datavar = new List<DataVariable>(GameAct.diff.dataVar);
		datacustom = new List<DataCustom>(GameAct.diff.dataCustom);
		postponeEvents = new List<PostponeEvent>(GameAct.diff.postponeEvents);
		navigation = new List<NavPoint>(NavigationAct.diff.navigation);
		goals = new List<NavPoint>(NavigationAct.diff.goals);
		goaltoremove = NavigationAct.diff.goaltoremove;
		objectives = ObjectiveAct.diff.PrepareSave();
		stats = DeadCloneAct.diff.overallStats;
	}
}
