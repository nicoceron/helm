using System;
using System.Collections;
using System.Collections.Generic;
using SVGImporter;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameAct : MonoBehaviour
{
	public AutomationRuntimeParameters AutomationRuntimeParameters;

	public DeadCloneAct deadCaptain;

	public string version = "";

	public string gamename = "reigns_got";

	public CardTypes cardType = CardTypes.character;

	public SpeechAct speaker;

	public CardReader reader;

	public MetersAct scMeters;

	public GameObject charaPrefab;

	public GameObject modalPrefab;

	public GameObject gameUI;

	public Transform modalUI;

	public GameObject interUI;

	public Transform characterRepo;

	public Text question;

	public string intercale;

	public SVGImage reignSign;

	public WhoAct scCh;

	private string title;

	public DialogAct scDia;

	public DialogAct scDiaXl;

	private List<Card> cards = new List<Card>();

	private List<Card> hiddenCards = new List<Card>();

	public List<PostponeEvent> postponeEvents = new List<PostponeEvent>();

	public Card card;

	private List<int> ignoredId = new List<int>();

	private Card forceCard;

	public CardAct cardSc;

	private Dictionary<Bearers, CardAct> specialCards;

	private Card lastCard;

	private int lastcardId = -1;

	private List<Bearers> bearersEnum = new List<Bearers>();

	public List<Bearer> bearers = new List<Bearer>();

	public static GameAct diff;

	private List<string> incrCustom = new List<string>();

	public List<DataCustom> dataCustom = new List<DataCustom>();

	public List<DataVariable> dataVar = new List<DataVariable>();

	public List<string> endCards = new List<string>();

	public List<int> seenEndCards = new List<int>();

	public List<int> seenBearers = new List<int>();

	public List<int> seenCards = new List<int>();

	public float timespent;

	public int decision = -10;

	private Bearer curBearer;

	public Bearers lastBearer = Bearers.none;

	private Card cardtoberemoved;

	private Bearers bearertoremove = Bearers.none;

	private Vector2 centralPos = new Vector2(0f, 100f);

	public int lastlength;

	private int maxage;

	public Action<Variables, int> OnDataChange;

	public Action<Card> OnShortcut;

	public Func<CardTypes, bool> OnCardHiding;

	public Action<GameSave> OnGameInit;

	public Action<GameStates> OnStart;

	public Action<Card> OnNewCard;

	public Func<CardTypes, bool> OnNewCardSuspend;

	public Action OnJourneyEnd;

	public Action OnLanding;

	public Action<ModalAct> OnNewModal;

	public Action<Card> OnUpdate;

	public Action<Card> OnRefresh;

	public Action<int> OnChoice;

	public Action<Bearers> OnCharacter;

	public Action OnUpdateCards;

	public Func<string, string> OnQuestion;

	public Func<bool, bool> OnSuspendStart;

	public Action<int> OnValidateDecision;

	public Action<Card> OnValidSelection;

	public Action<List<Card>> OnInitSelection;

	public GameObject spinner;

	private Dictionary<Bearers, Bearer> roles = new Dictionary<Bearers, Bearer>();

	private string randoSuffix = "";

	private GameStates _state = GameStates.none;

	protected GameStates _oldstate;

	public string nextCard = "";

	private int nextCardId = -1;

	private int slot;

	public EffectAct scEf;

	public bool onlyYes;

	public bool onlyNo;

	public JourneyAct scKi;

	private float questionY;

	public JukeBox scJu;

	public List<CharacterCard> selection;

	private bool isOver;

	public bool isafterdeath;

	public string storeSave;

	private GameSave resurrectSave;

	private int resurrectId;

	private GameSave _cloudgame;

	private GameSave _localgame;

	private int lastSpotId = -1;

	private List<Bearers> unlockedSelection = new List<Bearers>();

	private int curdec;

	private NavPoint openav;

	private List<Variables> multi10 = new List<Variables>
	{
		Variables.hull,
		Variables.oxygen,
		Variables.people,
		Variables.power
	};

	private string cacheGroup;

	private int cacheId = -1;

	private List<Bearer> regulars;

	private List<Outcome> null_outcomes = new List<Outcome>();

	private List<ModalAct> modalsToFire = new List<ModalAct>();

	private Effect curEffect;

	private object lastinstance;

	private Vector2 cPo;

	private bool forcenext;

	public GameStates state
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

	private void Awake()
	{
		diff = this;
		questionY = question.rectTransform.anchoredPosition.y;
		slot = (DataStore.localSaveFileSystem.HasKey("slot") ? DataStore.localSaveFileSystem.GetInt("slot") : 0);
		specialCards = new Dictionary<Bearers, CardAct>();
		foreach (Transform item in characterRepo)
		{
			CardAct component = item.GetComponent<CardAct>();
			if (component != null && !item.name.Equals("character"))
			{
				Bearers key = (Bearers)Enum.Parse(typeof(Bearers), item.name);
				specialCards.Add(key, component);
			}
		}
	}

	private CardAct GetCardAct(Bearers bearer)
	{
		return specialCards[bearer];
	}

	private void Start()
	{
		StartCoroutine(ChronicSave());
		Load();
	}

	private IEnumerator ChronicSave()
	{
		WaitForSeconds swait = new WaitForSeconds(20f);
		while (true)
		{
			yield return swait;
			if (cardSc is ConcertCard concertCard && concertCard.isPlaying)
			{
				yield return null;
				continue;
			}
			SaveGame();
			yield return 0;
			Resources.UnloadUnusedAssets();
		}
	}

	public void GameOver()
	{
		isOver = true;
	}

	private void ConfigureState(GameStates value)
	{
		_oldstate = _state;
		_state = value;
		switch (state)
		{
		case GameStates.interreign:
			break;
		case GameStates.gameover:
			break;
		case GameStates.restart:
			StartReign();
			break;
		case GameStates.start:
			decision = 0;
			StartReign();
			break;
		case GameStates.interaction:
			break;
		case GameStates.transition:
			ShowNextCard();
			break;
		}
	}

	public void StartReign(bool suspendNextcard = false)
	{
		if (suspendNextcard)
		{
			StopCoroutine("DoShowNextCard");
			StopCoroutine("HideCard");
		}
		AutomationController.Instance = new AutomationController();
		OnValidateDecision = (Action<int>)Delegate.Combine(OnValidateDecision, new Action<int>(AutomationController.Instance.OnValidateDecision));
		OnValidSelection = (Action<Card>)Delegate.Combine(OnValidSelection, new Action<Card>(AutomationController.Instance.OnValidateSelection));
		question.text = "";
		if (OnStart != null)
		{
			OnStart(state);
		}
		HideDecision();
		InitNumbers();
		cardType = CardTypes.character;
		decision = 0;
		StartCoroutine("YieldStart");
		scKi.effectsBloc.GetComponent<EffectsStats>().DataLoad();
	}

	private void IntercaleEnd(int dec)
	{
		OnValidateDecision = (Action<int>)Delegate.Remove(OnValidateDecision, new Action<int>(IntercaleEnd));
		ShowDataCol(yes: true);
	}

	private IEnumerator YieldStart()
	{
		if ((bool)AnimBut.diff)
		{
			AnimBut.diff.Lock(direct: true);
		}
		yield return new WaitForSeconds(0.2f);
		if (state == GameStates.restart)
		{
			yield return new WaitForSeconds(0.4f);
		}
		else
		{
			yield return new WaitForSeconds(1.3f);
		}
		if (OnSuspendStart != null)
		{
			while (!OnSuspendStart(arg: false))
			{
				yield return null;
			}
			yield return new WaitForSeconds(0.8f);
		}
		OnSuspendStart = null;
		StartInteraction();
		state = GameStates.transition;
	}

	private void EndJourney(bool forcend = false)
	{
		deadCaptain.Trigger();
	}

	private bool SomethingToSave()
	{
		return GetInt(Variables.distance) > 2;
	}

	public void SetResurrect()
	{
		if (resurrectSave != null && SomethingToSave())
		{
			Equalize();
			AddInt(Variables.journey);
			ObjectiveAct.diff.ResetNick();
			SetInt(Variables.length, 0);
			GameSave gameSave = new GameSave(this, withresurrect: true);
			gameSave.ressurectCard = resurrectSave.ressurectCard;
			gameSave.currentCard = resurrectSave.currentCard;
			gameSave.navigation = resurrectSave.navigation;
			gameSave.place_name = resurrectSave.place_name;
			gameSave.place = resurrectSave.place;
			gameSave.objectives = ObjectiveAct.diff.PrepareSave();
			DataStore.SaveSlot("GameSave", gameSave);
			storeSave = "";
		}
	}

	public void SaveGame()
	{
		if (!string.IsNullOrEmpty(storeSave))
		{
			DataStore.SaveSlot("GameSave", storeSave);
		}
	}

	private void StoreGame()
	{
		if (!card.name.Contains("_choice") && cardType != CardTypes.end && !card.name.Equals("_arrival") && !card.name.Equals("_nomoney") && !card.weight.Equals(0) && !(card.place_name == "Mara") && SomethingToSave())
		{
			if (isLand(card.place))
			{
				SaveResurrect();
			}
			storeSave = DataStore.Prepare(new GameSave(this, withresurrect: false));
		}
	}

	private void SaveResurrect()
	{
		if (card == null || !(card.name == "_resurrection"))
		{
			resurrectSave = new GameSave(this, withresurrect: true);
			DataStore.SaveSlot("ResurrectSave", resurrectSave);
		}
	}

	private void OnApplicationPause(bool pauseStatus)
	{
		if (pauseStatus)
		{
			SaveGame();
		}
	}

	private void OnApplicationQuit()
	{
		SaveGame();
	}

	private void OnLoadOverallLocal(OverallSave save)
	{
		if (save != null)
		{
			speaker.SetLang(save.language);
		}
		DataStore.LoadGame(OnLoadGame);
	}

	private void Load()
	{
		DataStore.LoadGame(OnLoadGame);
	}

	private void OnLoadGame(GameSave save)
	{
		_cloudgame = save;
		DataStore.LoadGame(OnLoadGameLocal, local: true);
	}

	private void OnLoadGameLocal(GameSave save)
	{
		bool flag = _cloudgame != null;
		bool flag2 = save != null;
		bool flag3 = flag && flag2 && _cloudgame.time - save.time > 0;
		bool flag4 = false;
		if (flag && flag2 && !flag3)
		{
			DataVariable dataVariable = _cloudgame.datavar.Find((DataVariable it) => it.var == Variables.distance);
			DataVariable dataVariable2 = save.datavar.Find((DataVariable it) => it.var == Variables.distance);
			if (dataVariable != null && dataVariable2 != null && dataVariable.val > dataVariable2.val)
			{
				flag4 = true;
			}
		}
		_localgame = save;
		if (flag3 || flag4)
		{
			string overridecancel = DataStore.FormatSave(_localgame);
			string overrideaction = DataStore.FormatSave(_cloudgame);
			scDiaXl.gameObject.SetActive(value: true);
			scDiaXl.Init("cloudsave", "actioncloud", "actionlocal", CloudChoice, null, overrideaction, overridecancel);
		}
		else if (!flag2 && flag)
		{
			FinalLoad(_cloudgame, local: false);
		}
		else
		{
			FinalLoad(save, local: true);
		}
	}

	private void LoadLocal(bool no)
	{
		FinalLoad(_localgame, local: true);
	}

	private void CloudChoice(bool validate)
	{
		if (validate)
		{
			FinalLoad(_cloudgame, local: false);
		}
		else
		{
			FinalLoad(_localgame, local: false);
		}
	}

	private void FinalLoad(GameSave save, bool local)
	{
		if (save != null)
		{
			OnLoadGameFinal(save);
			DataStore.LoadSlot<GameSave>("ResurrectSave", CacheResurrect, local);
		}
		else
		{
			DataStore.LoadSlot<GameSave>("ResurrectSave", OnLoadGameFinal, local);
		}
	}

	private void CacheResurrect(GameSave save)
	{
		resurrectSave = save;
	}

	private void OnLoadGameFinal(GameSave lastSave)
	{
		cards = reader.GetCards(hidden: false);
		hiddenCards = reader.GetCards(hidden: true);
		if (OnGameInit != null)
		{
			OnGameInit(lastSave);
		}
		foreach (Card card3 in cards)
		{
			string mMsM = card3.question.mMsM;
			if (mMsM.StartsWith("shortcut:"))
			{
				int result = 0;
				int.TryParse(mMsM.Substring(9), out result);
				Card card = GetCard(result);
				if (card != null)
				{
					card3.question = card.question;
				}
			}
		}
		foreach (Card hiddenCard in hiddenCards)
		{
			string mMsM2 = hiddenCard.question.mMsM;
			if (mMsM2.StartsWith("shortcut:"))
			{
				int result2 = 0;
				int.TryParse(mMsM2.Substring(9), out result2);
				Card card2 = GetCard(result2);
				if (card2 != null)
				{
					hiddenCard.question = card2.question;
				}
			}
		}
		foreach (Bearer bearerModel in reader.bearerModels)
		{
			if (bearerModel.type == BearerTypes.system || bearerModel.type == BearerTypes.special || bearerModel.bearer == Bearers.computer)
			{
				AddBearer(bearerModel.bearer);
			}
		}
		seenCards = new List<int>();
		if (lastSave != null)
		{
			if (lastSave.isressurection)
			{
				resurrectSave = lastSave;
			}
			UpdateCards(cards, lastSave.cards);
			UpdateCards(hiddenCards, lastSave.cards);
			foreach (BearerSave bearer2 in lastSave.bearers)
			{
				Bearer bearer = HasBearer(bearer2.type);
				if (bearer == null)
				{
					bearer = AddBearer(bearer2.type);
				}
				if (bearer != null)
				{
					bearer.name = bearer2.name;
					bearer.vote = bearer2.vote;
					bearer.character = bearer2.character;
				}
			}
			foreach (DataCustom item in lastSave.datacustom)
			{
				SetInt(item.var, item.val);
			}
			foreach (DataVariable item2 in lastSave.datavar)
			{
				SetInt(item2.var, item2.val);
			}
			postponeEvents = new List<PostponeEvent>(lastSave.postponeEvents);
			deadCaptain.overallStats = lastSave.stats;
		}
		else
		{
			int value = -1;
			dataCustom = new List<DataCustom>
			{
				new DataCustom("mobile_keep", value)
			};
		}
		if (lastSave != null)
		{
			state = GameStates.restart;
			lastSpotId = lastSave.currentCard;
			if (lastSpotId == lastSave.ressurectCard)
			{
				lastSpotId = -1;
			}
			int notificationCard = SocialAct.diff.GetNotificationCard();
			if (notificationCard > 0)
			{
				OpenCard(notificationCard);
			}
			else
			{
				OpenCard(lastSave.ressurectCard);
			}
		}
		else
		{
			state = GameStates.start;
			_cloudgame = null;
		}
	}

	private void UpdateCards(List<Card> cardlist, CardSave[] cardSaves)
	{
		foreach (Card item in cardlist)
		{
			if (cardSaves.Length - 1 < item.id)
			{
				continue;
			}
			CardSave cardSave = cardSaves[item.id];
			if (cardSave != null)
			{
				item.weight = cardSave.we;
				item.weightReal = cardSave.wr;
				item.nextturn = cardSave.nt;
				item.isLocked = cardSave.lo;
				item.wasSeen = cardSave.se;
				if (cardSave.se)
				{
					seenCards.Add(item.id);
				}
			}
		}
	}

	public void ValidSelectionDirect(Card next)
	{
		InputAct.diff.DisableMenuNav(closewindows: false);
		BackgroundAct.diff.ShowBack();
		ExpandCards();
		foreach (CharacterCard item in selection)
		{
			StartCoroutine("HideCard", item);
			item.DisableChoice();
		}
		card = null;
		curBearer = null;
		InputAct.diff.RestoreSlideFocus();
		cardType = CardTypes.character;
		state = GameStates.interaction;
		OpenCard(next);
	}

	private void FillCharacter(string main, Bearer bearer)
	{
		if (bearer.bearer == Bearers.merchandise)
		{
			string[] array = this.card.question.mMsM.Split('£');
			if (array.Length == 1)
			{
				Card card = hiddenCards.Find((Card it) => it.bearer == Bearers.merchandise && it.bearerVariation == this.card.bearerVariation);
				if (card != null)
				{
					array = card.question.mMsM.Split('£');
				}
			}
			foreach (Condition condition in this.card.conditions)
			{
				if (condition.custom_name != null && condition.custom_name.StartsWith("nb_"))
				{
					int override_int = ((condition != null) ? GetInt(condition.custom_name) : 0);
					scCh.ShowName(SpeechAct.diff.FinalFormat(array[0]), SpeechAct.diff.GetSmartTextFinal("quantity", 0, override_int));
					return;
				}
			}
			scCh.ShowName(array[0]);
		}
		else if (bearer.bearer == Bearers.gameover)
		{
			string smartText = SpeechAct.diff.GetSmartText("gameover_card", 0, GetInt("idguitar"));
			smartText = smartText.Replace("<total>", "9");
			scCh.ShowName(SpeechAct.diff.GetSceneTextFinal("gameover_" + this.card.bearerVariation), SpeechAct.diff.FinalFormat(smartText));
		}
		else
		{
			scCh.ShowName(TreatText(main), SpeechAct.diff.FinalFormat(bearer.title.Get()));
		}
	}

	public void ValidSelection(CharacterCard scCa, Bearer be, Card ca)
	{
		if (state != GameStates.interaction || cardType != CardTypes.selection)
		{
			return;
		}
		InputAct.diff.DisableMenuNav(closewindows: false);
		ExpandCards();
		card = ca;
		GText gText = ca.question;
		curBearer = be;
		InputAct.diff.slideSign = ((curBearer.bearer != Bearers.upside) ? 1 : (-1));
		cardSc = scCa;
		scCa.SetPrice();
		string main = curBearer.generated.Get();
		if (card.bearerVariation.EndsWith("dark"))
		{
			main = "? ? ?";
		}
		ChangeQuestion(gText);
		FillCharacter(main, curBearer);
		lastBearer = curBearer.bearer;
		foreach (CharacterCard item in selection)
		{
			if (item != scCa)
			{
				StartCoroutine("HideCard", item);
				item.DisableChoice();
			}
		}
		cardType = CardTypes.character;
		UpdateStatBearer(curBearer.bearer, andmet: true);
		if (!isafterdeath)
		{
			BackgroundAct.diff.ShowBack();
		}
		InputAct.diff.RestoreSlideFocus();
		if (!CheckShortcut(card, gText, andopen: true))
		{
			LoadCard(card);
		}
		else
		{
			card.wasSeen = true;
			if (!seenCards.Contains(card.id))
			{
				seenCards.Add(card.id);
			}
		}
		if (OnValidSelection != null)
		{
			OnValidSelection(card);
		}
		JukeBox.diff.PlaySound(SFXTypes.ui_inventory_select);
	}

	public void ActivateFirstSelection()
	{
		if (cardType == CardTypes.selection && selection.Count > 0)
		{
			selection[0].ActivateButton(first: true);
		}
	}

	private IEnumerator InitSelection()
	{
		string n = this.card.name;
		bool isChoiceHidden = (n.EndsWith("hidden_choice") ? true : false);
		ignoredId = new List<int>();
		List<Card> obj = (this.card.name.StartsWith("_") ? hiddenCards.FindAll((Card it) => it.name == n) : cards.FindAll((Card it) => it.name == n));
		List<Card> validCards = new List<Card>();
		List<DataDisplay> validDisplay = new List<DataDisplay>();
		Card card = null;
		foreach (Card item in obj)
		{
			if (item.weight == 0 || item.bearer == Bearers.merchant)
			{
				card = item;
			}
			bool flag = TestCard(item, smallbatch: true);
			if (flag)
			{
				if (item.wasSeen || item.weight != 106 || unlockedSelection.Contains(item.bearer))
				{
					validDisplay.Add(DataDisplay.fullamount);
				}
				else
				{
					validDisplay.Add(DataDisplay.moving);
					if (!unlockedSelection.Contains(item.bearer))
					{
						unlockedSelection.Add(item.bearer);
					}
				}
				validCards.Add(item);
				this.card = item;
			}
			else if (item.weight == 106)
			{
				validDisplay.Add(DataDisplay.hidden);
				validCards.Add(item);
				this.card = item;
			}
			else if (!flag)
			{
				ignoredId.Add(item.id);
			}
		}
		int num = (n.Contains("4") ? 4 : (n.Contains("9") ? 9 : 16));
		if (num == 9 && (n.StartsWith("_buy9") || n.StartsWith("_sell9") || n.StartsWith("_black9")))
		{
			string nextName = BackgroundAct.diff.GetNextName();
			if (validCards.Count > 1)
			{
				validCards.Shuffle(nextName);
			}
			List<Card> list = new List<Card>();
			foreach (Card item2 in validCards)
			{
				if (item2.weight > 999 || Util.GetFloat(item2.id + nextName) > 0.4f)
				{
					list.Add(item2);
				}
			}
			validCards = list;
			if (validCards.Count < 2 && card != null)
			{
				nextCard = "cardId";
				nextCardId = card.id;
				SetupCharacterCard(notypechange: true);
				ShowCharacterCard(1);
				cardSc.GoToPos(centralPos);
				yield return new WaitForSeconds(0.4f);
				yield break;
			}
		}
		selection = new List<CharacterCard>();
		int nb = Mathf.Clamp(validCards.Count, 0, num);
		List<Card> list2 = new List<Card>();
		for (int num2 = 0; num2 < nb; num2++)
		{
			bool flag2 = false;
			Card card2 = validCards[num2];
			Bearer bearer = ((HasBearer(card2.bearer, card2.bearerIsAlso, card2.bearerIsNot) == null) ? AddBearer(card2.bearer, null, addtobearers: false) : SelectBearer(card2));
			if (bearer != null)
			{
				bearer = new Bearer(bearer);
				if (!selection.Contains(bearer.scCa))
				{
					selection.Add(bearer.scCa);
					flag2 = true;
				}
				else
				{
					bearer = AddBearer(card2.bearer, null, addtobearers: false);
					if (bearer != null)
					{
						selection.Add(bearer.scCa);
						flag2 = true;
					}
				}
			}
			if (!flag2)
			{
				list2.Add(card2);
			}
		}
		foreach (Card item3 in list2)
		{
			validCards.Remove(item3);
		}
		nb = selection.Count;
		if (nb < 2)
		{
			nextCard = "cardId";
			nextCardId = ((nb == 0) ? card.id : this.card.id);
			SetupCharacterCard(notypechange: true);
			ShowCharacterCard(1);
			cardSc.GoToPos(centralPos);
			yield return new WaitForSeconds(0.4f);
			yield break;
		}
		InputAct.diff.SuspendSlideFocus();
		BackgroundAct.diff.HideBack();
		int matrix = ((nb > 9) ? 4 : ((nb > 4) ? 3 : 2));
		int lastcol = nb % matrix;
		int lastrow = (nb - lastcol) / matrix;
		int num3 = Mathf.FloorToInt((nb - 1) / matrix);
		float amo = matrix switch
		{
			3 => 0.3f, 
			4 => 0.225f, 
			_ => 0.45f, 
		};
		bool showSubtitle = ((matrix <= 3) ? true : false);
		float subtitleSize = ((matrix == 3) ? 1.1f : 1f);
		float t = 0.1f;
		Vector2 displace = ((num3 == 3) ? new Vector2(-110f, 75f) : ((num3 == 2 && matrix == 4) ? new Vector2(-110f, 50f) : ((num3 == 2 && matrix == 3) ? new Vector2(-100f, 60f) : ((num3 == 1 && matrix == 3) ? new Vector2(-100f, 20f) : ((num3 == 1 && matrix == 2) ? new Vector2(-75f, 50f) : new Vector2(-75f, -15f))))));
		ShrinkCards(amo, displace);
		if (OnInitSelection != null)
		{
			OnInitSelection(validCards);
		}
		bool hasfirst = false;
		InputAct.diff.OpenInventory();
		JukeBox.diff.PlaySound(SFXTypes.ui_character_transition);
		for (int i = 0; i < nb; i++)
		{
			Card card3 = validCards[i];
			CharacterCard characterCard = selection[i];
			characterCard.gameObject.SetActive(value: true);
			int num4 = i % matrix;
			int num5 = Mathf.FloorToInt(i / matrix);
			Vector2 vector = ((num5 == lastrow) ? (Vector2.right * 165f * (matrix - lastcol)) : Vector2.zero);
			Vector2 target = centralPos + Vector2.right * 330f * num4 + Vector2.down * 330f * num5 + vector;
			characterCard.GoToPos(target);
			scCh.ShowName("");
			bool flag3 = ((!hasfirst && validDisplay[i] != DataDisplay.hidden) ? true : false);
			characterCard.UpdateChoiceCard(card3, isChoiceHidden, showSubtitle, validDisplay[i], flag3, subtitleSize);
			if (flag3)
			{
				hasfirst = true;
			}
			yield return new WaitForSeconds(t);
			t -= 0.01f;
		}
		yield return new WaitForSeconds(0.4f);
	}

	private CharacterCard InstantiateCard(Bearer bear)
	{
		CharacterCard component;
		GameObject gameObject;
		if (specialCards.ContainsKey(bear.bearer))
		{
			component = specialCards[bear.bearer].GetComponent<CharacterCard>();
			gameObject = component.gameObject;
		}
		else
		{
			gameObject = UnityEngine.Object.Instantiate(charaPrefab, characterRepo);
			component = gameObject.GetComponent<CharacterCard>();
		}
		if (component != null)
		{
			component.Init(bear);
		}
		gameObject.SetActive(value: false);
		return component;
	}

	private void SetBearer(Bearer perso, Bearers chara, Bearer charamodel = null)
	{
		if (!roles.ContainsKey(chara))
		{
			roles.Add(chara, perso);
		}
		if (charamodel == null)
		{
			if (!perso.title.isEmpty)
			{
				return;
			}
			{
				foreach (Bearers cha in perso.character)
				{
					Bearer bearer = reader.bearerModels.Find((Bearer it) => it.bearer == cha);
					if (!bearer.title.isEmpty)
					{
						perso.title = bearer.title;
						break;
					}
				}
				return;
			}
		}
		if (!perso.character.Contains(chara))
		{
			perso.character.Add(chara);
		}
		if (!charamodel.title.isEmpty)
		{
			perso.title = charamodel.title;
		}
	}

	private void AddChara(Bearers chara, Bearer perso)
	{
		Bearer charamodel = CardReader.diff.bearerModels.Find((Bearer it) => it.bearer == chara);
		if (!bearers.Contains(perso))
		{
			bearers.Add(perso);
		}
		SetBearer(perso, chara, charamodel);
	}

	private void AddChara(Bearers chara, Bearers all)
	{
		Bearer bearer = bearers.Find((Bearer it) => it.bearer == all);
		if (bearer != null)
		{
			AddChara(chara, bearer);
			return;
		}
		CardReader.diff.AddCharacterToModel(chara, all);
		foreach (Bearer item in bearers.FindAll((Bearer it) => it.character.Contains(all)))
		{
			if (!item.character.Contains(chara))
			{
				item.character.Add(chara);
			}
		}
	}

	private void RemoveChara(Bearers chara, Bearers all)
	{
		Bearer bearer = bearers.Find((Bearer it) => it.bearer == all);
		if (bearer == null)
		{
			CardReader.diff.RemoveCharacterFromModel(chara, all);
			{
				foreach (Bearer item in bearers.FindAll((Bearer it) => it.character.Contains(all)))
				{
					if (item.character.Contains(chara))
					{
						item.character.Remove(chara);
						if (chara == Bearers.antagonist)
						{
							item.vote = 1f;
						}
					}
				}
				return;
			}
		}
		RemoveChara(chara, bearer);
	}

	private void RemoveChara(Bearers chara, Bearer individual)
	{
		if (individual.character.Contains(chara))
		{
			individual.character.Remove(chara);
			if (roles.ContainsKey(chara))
			{
				roles.Remove(chara);
			}
		}
	}

	private Bearer AddBearer(Bearers bearer, Bearers target)
	{
		if (target != Bearers.none)
		{
			return AddBearer(bearer, bearers.Find((Bearer it) => it.bearer == target));
		}
		return AddBearer(bearer);
	}

	public Bearer AddBearer(Bearers bearer, Bearer targetBearer = null, bool addtobearers = true)
	{
		Bearer bearer2 = new Bearer(reader.bearerModels.Find((Bearer it) => it.bearer == bearer));
		switch (bearer2.type)
		{
		case BearerTypes.tag:
		{
			List<Bearer> list2 = bearers.FindAll((Bearer it) => it.character.Contains(bearer));
			if (addtobearers && list2.Count > 0)
			{
				return null;
			}
			if (targetBearer == null)
			{
				List<Bearer> list3 = reader.bearerModels.FindAll((Bearer it) => it.character.Contains(bearer) && !roles.ContainsValue(bearers.Find((Bearer ti) => ti.bearer == it.bearer)) && postponeEvents.Find((PostponeEvent tt) => tt.bear == it.bearer) == null);
				list3.Shuffle();
				if (list3.Count == 0)
				{
					return null;
				}
				targetBearer = new Bearer(list3[0]);
				targetBearer.scCa = InstantiateCard(targetBearer);
				if (addtobearers)
				{
					bearers.Add(targetBearer);
				}
			}
			else if (addtobearers && !bearers.Contains(targetBearer))
			{
				bearers.Add(targetBearer);
			}
			if (addtobearers)
			{
				SetBearer(targetBearer, bearer, bearer2);
			}
			return targetBearer;
		}
		case BearerTypes.generated:
		case BearerTypes.individual:
		case BearerTypes.special:
		case BearerTypes.system:
		{
			List<Bearer> list = bearers.FindAll((Bearer it) => it.bearer == bearer);
			if (addtobearers && list.Count >= bearer2.max)
			{
				return null;
			}
			Bearer bearer3 = new Bearer(bearer2);
			if (bearer2.type != BearerTypes.system)
			{
				bearer3.scCa = InstantiateCard(bearer3);
			}
			if (addtobearers)
			{
				bearers.Add(bearer3);
			}
			if (addtobearers)
			{
				SetBearer(bearer3, bearer);
			}
			return bearer3;
		}
		default:
			return null;
		}
	}

	private void RemoveFromListBearers(Bearer b)
	{
		if (bearers.Contains(b))
		{
			bearers.Remove(b);
			bearersEnum.Remove(b.bearer);
		}
	}

	private void RemoveBearer(Bearers bearer)
	{
		Bearer bearer2 = HasBearer(bearer);
		if (bearer2 == null)
		{
			if (!roles.ContainsKey(bearer))
			{
				return;
			}
			bearer2 = roles[bearer];
		}
		RemoveBearer(bearer2);
	}

	private void RemoveBearer(Bearer bear)
	{
		if (roles.ContainsKey(bear.bearer))
		{
			roles.Remove(bear.bearer);
		}
		foreach (Bearers item in bear.character)
		{
			if (roles.ContainsKey(item))
			{
				roles.Remove(item);
			}
		}
		CharacterCard scCa = bear.scCa;
		if (bear.staydead != -1)
		{
			postponeEvents.Add(new PostponeEvent(GetInt(Variables.distance) + bear.staydead, bear.bearer));
		}
		RemoveFromListBearers(bear);
		UnityEngine.Object.Destroy(scCa.gameObject);
	}

	public bool HasBearer(Bearer bearer)
	{
		return bearers.Find((Bearer it) => it.bearer == bearer.bearer) != null;
	}

	public Bearer HasBearer(Bearers bearer, Bearers isalso = Bearers.none, Bearers isnot = Bearers.none)
	{
		if (isalso != Bearers.none && bearer == Bearers.anyone)
		{
			bearer = isalso;
		}
		Bearer bearer2 = bearers.Find((Bearer it) => it.bearer == bearer && !it.character.Contains(isnot));
		if (bearer2 != null)
		{
			return bearer2;
		}
		List<Bearer> list = bearers.FindAll((Bearer it) => it.character.Contains(bearer) && !it.character.Contains(isnot));
		if (list.Count > 0)
		{
			list.Shuffle();
			return list[0];
		}
		return null;
	}

	private void ShowNextCard()
	{
		StopCoroutine("DoShowNextCard");
		StartCoroutine("DoShowNextCard");
		OnUpdate?.Invoke(card);
	}

	private IEnumerator HideCard(CardAct cardsc)
	{
		if (cardsc == null)
		{
			yield break;
		}
		if (decision == -1)
		{
			JukeBox.diff.PlaySound(SFXTypes.card_discard_right);
		}
		else
		{
			JukeBox.diff.PlaySound(SFXTypes.card_discard_left);
		}
		cardsc.HideCard();
		cardSc.ShowDecision(0);
		float t = 0f;
		bool nodec = ((decision != -1 && decision != 1) ? true : false);
		float dec = ((!nodec) ? decision : ((!(Util.Rand() > 0.5f)) ? 1 : (-1)));
		while (t < 1f)
		{
			Vector2 vec = dec * Vector2.right * Time.deltaTime * 2000f * t;
			cardsc.Disappear(vec, nodec);
			t += Time.deltaTime * 4f;
			yield return null;
		}
		HideDecision();
		UnsetCard(cardsc);
		if (OnCardHiding == null)
		{
			yield break;
		}
		while (!OnCardHiding(cardType))
		{
			yield return new WaitForSeconds(0.2f);
			if (OnCardHiding == null)
			{
				break;
			}
		}
	}

	private void UnsetCard(CardAct cardsc)
	{
		cardsc.GoToPos(Vector2.right * 1000f);
		cardsc.RotateTo(0f);
		cardsc.Unset();
		if (cardsc != null)
		{
			cardsc.gameObject.SetActive(value: false);
		}
	}

	private IEnumerator MoveCardToCenter(CardAct cardsc, int dec)
	{
		if (!(cardsc == null))
		{
			float t = 0f;
			Vector2 opos = centralPos + 100 * dec * Vector2.right;
			Vector2 tarpos = centralPos;
			while (t < 1f)
			{
				cardsc.GoToPos(Vector3.Lerp(opos, tarpos, t));
				t += Time.deltaTime * 6f;
				yield return null;
			}
			cardsc.GoToPos(tarpos);
		}
	}

	private IEnumerator DoShowNextCard()
	{
		curdec = decision;
		if (cardSc != null)
		{
			yield return StartCoroutine("HideCard", cardSc);
		}
		DoDestroy();
		if (card != null && card.name != "_misplayed")
		{
			lastCard = card;
		}
		SetInt(Variables.price, 0);
		switch (cardType)
		{
		case CardTypes.character:
			SetupCharacterCard();
			break;
		case CardTypes.effect:
			DisplayEffectCard();
			break;
		case CardTypes.end:
			yield break;
		}
		if (OnNewCard != null && card != null)
		{
			OnNewCard(card);
		}
		if (OnNewCardSuspend != null)
		{
			while (OnNewCardSuspend(cardType))
			{
				yield return 0;
			}
		}
		yield return 0;
		while (InputAct.diff.isInMenu)
		{
			yield return 0;
		}
		switch (cardType)
		{
		case CardTypes.character:
			ShowCharacterCard(curdec);
			goto default;
		case CardTypes.selection:
			yield return StartCoroutine("InitSelection");
			break;
		case CardTypes.intercale:
			DisplayIntercaleCard();
			goto default;
		case CardTypes.custom:
			scCh.ShowName("");
			DeleteQuestion();
			cardSc.InitCard("", "", TreatText(card.question), curdec);
			goto default;
		default:
			cardSc.GoToPos(centralPos);
			yield return new WaitForSeconds(0.4f);
			break;
		}
		StoreGame();
		LoadCard(card);
		NewYear();
		if (OnRefresh != null && card != null)
		{
			OnRefresh(card);
		}
		state = GameStates.interaction;
	}

	private bool isLand(Backgrounds type)
	{
		return NavigationAct.diff.placeToLand.Contains(type);
	}

	public void LoadCard(Card card)
	{
		if (card == null || cardType == CardTypes.selection)
		{
			return;
		}
		if (!isLand(card.place) && card.name != "launch" && !card.name.StartsWith("_"))
		{
			SetInt(Variables.stop, -1);
		}
		lastcardId = card.id;
		Util.Write("cardid> " + card.id);
		if (cardType != CardTypes.intercale)
		{
			foreach (string item in incrCustom)
			{
				if (GetInt(item) > 0)
				{
					AddInt(item, 1);
				}
			}
		}
		if (card.bearer == Bearers.intercale || cardType != CardTypes.intercale)
		{
			TreatOutcomes(2);
		}
		if (card.lockturn == -1)
		{
			DestroyCard(card);
		}
		List<Card> list = ((card.name == "default") ? new List<Card>() : cards.FindAll((Card it) => it.name == card.name));
		if (list.Count == 0)
		{
			list = new List<Card> { card };
		}
		foreach (Card item2 in list)
		{
			if (item2.lockturn < 0)
			{
				item2.nextturn = GetInt(Variables.turns) + 1000;
			}
			else
			{
				item2.nextturn = GetInt(Variables.turns) + item2.lockturn;
			}
		}
		if (!card.wasSeen)
		{
			card.wasSeen = true;
			if (!seenCards.Contains(card.id))
			{
				seenCards.Add(card.id);
			}
			scKi.AddAchieve(card.name, AchieveTypes.card);
		}
	}

	private void DisplayIntercaleCard()
	{
		cardSc = GetCardAct(Bearers.intercale);
		cardSc.gameObject.SetActive(value: true);
		cardSc.InitCard("", "", intercale, curdec);
		scCh.ShowName("");
	}

	private bool CheckShortcut(Card ccard, GText quest, bool andopen = false)
	{
		Outcome outcome = ccard.load_outcomes.Find((Outcome it) => it.variable == Variables.chain);
		if (outcome != null)
		{
			if (OnShortcut != null)
			{
				OnShortcut(ccard);
			}
			ChangeQuestion(quest);
			LoadCard(ccard);
			ShowOutcome(GetOutcomeList(1));
			TreatOutcomes(1);
			card = SelectCard(outcome.custom_name);
			if (andopen)
			{
				OpenCard(card);
			}
			else if (card.name.EndsWith("_choice"))
			{
				cardType = CardTypes.selection;
				return false;
			}
			return true;
		}
		if (!andopen)
		{
			return true;
		}
		return false;
	}

	private void SetupCharacterCard(bool notypechange = false)
	{
		card = SelectCard(nextCard);
		if (card.name.EndsWith("_choice") && !notypechange)
		{
			cardType = CardTypes.selection;
			return;
		}
		GText quest = card.question;
		if (CheckShortcut(card, quest))
		{
			switch (card.bearer)
			{
			case Bearers.gameover:
				DeadCloneAct.diff.AddStat("o_" + card.bearerVariation);
				break;
			case Bearers.end:
				OpenEndCard(quest);
				return;
			case Bearers.intercale:
				DeleteQuestion();
				SetIntercale(quest);
				return;
			case Bearers.map:
			case Bearers.concert:
				cardType = CardTypes.custom;
				cardSc = GetCardAct(card.bearer);
				return;
			}
			cardType = CardTypes.character;
		}
	}

	private void ShowCharacterCard(int curdec)
	{
		curBearer = SelectBearer(card);
		InputAct.diff.slideSign = ((curBearer.bearer != Bearers.upside) ? 1 : (-1));
		GText text = card.question;
		speaker.isSelfMale = curBearer.character.Contains(Bearers.male);
		ChangeQuestion(text);
		if (OnCharacter != null)
		{
			OnCharacter(card.bearer);
		}
		cardSc = curBearer.scCa;
		cardSc.gameObject.SetActive(value: true);
		curBearer.scCa.SetupPrice(card, single: true);
		curBearer.scCa.UpdateCharacCard(card, curdec);
		string main = curBearer.generated.Get();
		if (card.bearerVariation.EndsWith("dark"))
		{
			main = "? ? ?";
		}
		FillCharacter(main, curBearer);
		lastBearer = curBearer.bearer;
		UpdateStatBearer(lastBearer, andmet: true);
	}

	private Bearer SelectBearer(Card card)
	{
		if (card.bearer == Bearers.anyone || card.bearer == Bearers.none)
		{
			if (card.bearer == Bearers.none && curBearer != null)
			{
				return curBearer;
			}
			List<Bearer> list = ((card.bearerIsAlso != Bearers.none) ? bearers.FindAll((Bearer it) => !it.character.Contains(Bearers.antagonist) && it.type == BearerTypes.individual && (it.character.Contains(card.bearerIsAlso) || it.bearer == card.bearerIsAlso)) : ((card.bearerIsNot != Bearers.none) ? bearers.FindAll((Bearer it) => !it.character.Contains(Bearers.antagonist) && it.type == BearerTypes.individual && !it.character.Contains(card.bearerIsNot)) : bearers.FindAll((Bearer it) => !it.character.Contains(Bearers.antagonist) && it.type == BearerTypes.individual)));
			if (list.Count == 0)
			{
				list = bearers.FindAll((Bearer it) => !it.character.Contains(Bearers.antagonist) && it.type == BearerTypes.individual);
			}
			return list[Util.RandInt(0, list.Count)];
		}
		if (roles.ContainsKey(card.bearer))
		{
			return roles[card.bearer];
		}
		Bearer bearer = bearers.Find((Bearer it) => it.bearer == card.bearer);
		if (bearer == null)
		{
			List<Bearer> list2 = ((card.bearerIsAlso != Bearers.none) ? bearers.FindAll((Bearer it) => it.character.Contains(card.bearer) && it.character.Contains(card.bearerIsAlso)) : ((card.bearerIsNot != Bearers.none) ? bearers.FindAll((Bearer it) => it.character.Contains(card.bearer) && !it.character.Contains(card.bearerIsNot)) : bearers.FindAll((Bearer it) => it.character.Contains(card.bearer))));
			if (list2.Count == 0)
			{
				list2 = bearers.FindAll((Bearer it) => !it.character.Contains(Bearers.antagonist) && it.type == BearerTypes.individual);
			}
			bearer = list2[Util.RandInt(0, list2.Count)];
		}
		roles.Add(card.bearer, bearer);
		return bearer;
	}

	public bool HasSeenBearer(Bearers b)
	{
		return seenBearers.Contains((int)b);
	}

	public void UpdateStatBearer(Bearers b, bool andmet)
	{
		if (GetRegularBearers().Find((Bearer it) => it.bearer == b) != null)
		{
			DeadCloneAct.diff.AddStat("b_" + b);
			scKi.AddAchieve(b.ToString(), AchieveTypes.character);
		}
	}

	private void OpenEndCard(GText quest)
	{
		AddSeenEndCard(card.bearerVariation);
		JukeBox.diff.StopAllSoundAndMusic();
		CameffectAct.diff.StopEffect();
		JukeBox.diff.nomusic = true;
		JukeBox.diff.FadeOutVO();
		JukeBox.diff.PlaySound(SFXTypes.ui_death_drone);
		AudioClip audioClip = (AudioClip)Resources.Load("end_sfx/" + card.bearerVariation, typeof(AudioClip));
		if (audioClip != null)
		{
			JukeBox.diff.PlaySound(audioClip, fadeIn: true);
		}
		cardType = CardTypes.end;
		cardSc = GetCardAct(Bearers.end);
		cardSc.gameObject.SetActive(value: true);
		scCh.ShowName("");
		cardSc.InitCard(card.bearerVariation);
		ChangeQuestion(quest);
		if (card.bearerVariation != "")
		{
			cardSc.CustomImage(card.bearerVariation, "deaths");
		}
		curBearer = null;
		InputAct.diff.slideSign = 1f;
		scMeters.CheckDanger();
	}

	public void DeleteQuestion()
	{
		StopCoroutine("DoChangeQuestion");
		StartCoroutine("DoChangeQuestion", " ");
		scCh.ShowName("");
	}

	public void ChangeQuestion(GText text, bool withCut = false)
	{
		string value = ((OnQuestion == null) ? TreatText(text) : OnQuestion(TreatText(text)));
		StopCoroutine("DoChangeQuestion");
		StartCoroutine("DoChangeQuestion", value);
	}

	public void ChangeQuestion(string text)
	{
		string value = ((OnQuestion == null) ? text : OnQuestion(text));
		StopCoroutine("DoChangeQuestion");
		StartCoroutine("DoChangeQuestion", value);
	}

	private IEnumerator DoChangeQuestion(string formatText)
	{
		Vector2 opos = new Vector2(0f, questionY);
		Vector2 hidepos = new Vector2(0f, questionY - 4f);
		Vector2 showpos = new Vector2(0f, questionY + 4f);
		question.CrossFadeAlpha(0.01f, 0.3f, ignoreTimeScale: true);
		RectTransform qTrans = question.rectTransform;
		float t = 0f;
		while (t < 1f)
		{
			qTrans.anchoredPosition = Vector2.Lerp(opos, hidepos, t);
			t += Time.deltaTime * 4f;
			yield return null;
		}
		question.text = ((formatText == "<do not translate>") ? " " : formatText);
		question.CrossFadeAlpha(1f, 0.6f, ignoreTimeScale: true);
		qTrans.anchoredPosition = showpos;
		t = 0f;
		while (t < 1f)
		{
			qTrans.anchoredPosition = Vector2.Lerp(showpos, opos, t);
			t += Time.deltaTime * 4f;
			yield return null;
		}
		qTrans.anchoredPosition = opos;
	}

	public Card SelectCard(string next, Bearers forcebearer = Bearers.none)
	{
		bool flag = string.IsNullOrEmpty(next);
		List<Card> list = new List<Card>();
		bool smallbatch = false;
		if (forceCard != null)
		{
			Card ca = forceCard;
			forceCard = null;
			return CardToReturn(ca);
		}
		if (!flag)
		{
			smallbatch = true;
			bool flag2 = next.Substring(0, 1) == "_";
			if (next == "cardId")
			{
				list = hiddenCards.FindAll((Card it) => it.id == nextCardId);
				forcenext = true;
			}
			else
			{
				List<Card> list2 = (flag2 ? hiddenCards : cards);
				list = ((forcebearer == Bearers.none || forcebearer == Bearers.anyone) ? list2.FindAll((Card it) => it.name == next) : list2.FindAll((Card it) => it.name == next && (it.bearer == forcebearer || it.bearer == Bearers.anyone)));
				if (flag2)
				{
					next = next.Substring(1);
				}
			}
			if (list.Count == 0)
			{
				int cid = 0;
				int.TryParse(next, out cid);
				if (cid > 0)
				{
					Card item = (flag2 ? hiddenCards.Find((Card it) => it.id == cid) : cards.Find((Card it) => it.id == cid));
					list.Add(item);
				}
			}
			if (list.Count == 1)
			{
				Card card = list[0];
				if (((card.bearer == Bearers.end || card.bearer == Bearers.anyone || HasBearer(card.bearer) != null) && TestCond(card.conditions)) || forcenext)
				{
					TriggerWeightVar(card);
					forcenext = false;
					return CardToReturn(card);
				}
			}
			else if (list.Count == 0)
			{
				next = (nextCard = "");
				smallbatch = false;
				list = cards;
			}
		}
		else
		{
			openav = NavigationAct.diff.GetArrivalPoint();
			if (openav != null)
			{
				if (openav.cid != -1)
				{
					return CardToReturn(hiddenCards.Find((Card it) => it.id == openav.cid));
				}
				BackgroundAct.diff.SetNextName(openav.name, openav.type);
				list = hiddenCards.FindAll((Card it) => it.name == "_arrival");
			}
			else
			{
				list = cards;
			}
		}
		return ProcessCards(list, smallbatch);
	}

	public void SetRandomiserSuffix(string suf)
	{
		randoSuffix = suf;
	}

	public List<Card> GetHiddenCards(string name)
	{
		return hiddenCards.FindAll((Card it) => it.name == name);
	}

	public List<Card> GetHiddenCards()
	{
		return hiddenCards;
	}

	public List<Card> GetCards()
	{
		return cards;
	}

	private void TriggerWeightVar(Card card)
	{
		if (card.weightVar != 0 && card.weight > -1)
		{
			card.weight = Mathf.Clamp(card.weight + card.weightVar, 0, 100000000);
		}
	}

	public Card ProcessCards(List<Card> cardstotest, bool smallbatch, bool failsafe = true)
	{
		List<Card> list = new List<Card>();
		List<int> list2 = new List<int> { 0 };
		ignoredId = new List<int>();
		for (int i = 0; i < cardstotest.Count; i++)
		{
			Card card = cardstotest[i];
			bool flag = TestCard(card, smallbatch);
			if (flag)
			{
				if (card.weight == -1)
				{
					return CardToReturn(card, failsafe);
				}
				int weight = card.weight;
				list2.Add(weight + list2[list2.Count - 1]);
				list.Add(card);
				TriggerWeightVar(card);
			}
			else if (!flag)
			{
				ignoredId.Add(card.id);
				if (card.weight != card.weightReal)
				{
					card.weight = card.weightReal;
				}
				if (card.weightNocond > 0)
				{
					list2.Add(card.weightNocond + list2[list2.Count - 1]);
					list.Add(card);
				}
			}
		}
		if (list.Count == 0)
		{
			if (failsafe)
			{
				return CardToReturn(cardstotest[0], failsafe);
			}
			return null;
		}
		if (list.Count == 1)
		{
			return CardToReturn(list[0], failsafe);
		}
		float num = (NavigationAct.diff.hasLanded ? Util.GetFloat(BackgroundAct.diff.nameBack + list[0].name + randoSuffix, 0.01f, list2[list2.Count - 1]) : Util.Rand(0.01f, list2[list2.Count - 1]));
		for (int j = 0; j < list2.Count; j++)
		{
			if ((float)list2[j] > num)
			{
				return CardToReturn(list[j - 1], failsafe);
			}
		}
		return null;
	}

	private Card CardToReturn(Card ca, bool failsafe = true)
	{
		postponeEvents.RemoveAll((PostponeEvent it) => it.card == ca.name && !string.IsNullOrEmpty(ca.name));
		if (ca.id == 142)
		{
			foreach (Card card in cards)
			{
				card.nextturn -= 500;
			}
		}
		if (HasBearer(ca.bearer) == null)
		{
			AddBearer(ca.bearer);
		}
		if (ca.weight != ca.weightReal)
		{
			ca.weight = ca.weightReal;
		}
		return ca;
	}

	private bool TestCard(int id)
	{
		Card card = hiddenCards.Find((Card it) => it.id == id);
		if (card == null)
		{
			return false;
		}
		return TestCard(card, smallbatch: false, ignoreBearer: true);
	}

	public bool TestCard(Card card, bool smallbatch, bool ignoreBearer = false)
	{
		if (card.isLocked)
		{
			return false;
		}
		if (card.weight == 0 || card.weight == -666)
		{
			return false;
		}
		if (card.bearer == lastBearer && !smallbatch && card.bearer != Bearers.anyone && card.bearer != Bearers.computer)
		{
			return false;
		}
		if (GetBool("unknown") && !smallbatch && (card.name.Contains("<goal>") || card.name.Contains("<planet>")))
		{
			return false;
		}
		bool flag = BackgroundAct.diff.Landing();
		bool flag2 = string.IsNullOrEmpty(card.place_name);
		if (!smallbatch && !isLand(card.place) && flag2 && flag)
		{
			return false;
		}
		if ((!smallbatch || flag) && !BackgroundAct.diff.PlaceMatch(card.place) && card.place != Backgrounds.defaut)
		{
			return false;
		}
		if (!flag && isLand(card.place) && !smallbatch)
		{
			return false;
		}
		if (flag && !flag2 && !BackgroundAct.diff.NameMatch(card.place_name))
		{
			return false;
		}
		Bearers cabear = card.bearer;
		Bearers bearers = card.bearerIsAlso;
		Bearers bearerIsNot = card.bearerIsNot;
		Bearer bearer;
		if (cabear == Bearers.anyone && bearers != Bearers.none)
		{
			bearer = HasBearer(bearers);
			bearers = Bearers.none;
		}
		else
		{
			bearer = HasBearer(cabear);
		}
		if (!isafterdeath && bearer == null && postponeEvents.Find((PostponeEvent it) => it.bear == card.bearer) != null)
		{
			return false;
		}
		if (!isafterdeath && !ignoreBearer)
		{
			if (bearer == null)
			{
				if (cabear != Bearers.end && cabear != Bearers.anyone && cabear != Bearers.none && card.conditions.Find((Condition it) => it.bearer == cabear && it.variable == Variables.set && it.condition == Conditions.notequal) == null)
				{
					return false;
				}
				if (bearers != Bearers.none && !reader.HasModelCharacter(cabear, bearers))
				{
					return false;
				}
				if (bearerIsNot != Bearers.none && reader.HasModelCharacter(cabear, bearerIsNot))
				{
					return false;
				}
			}
			else
			{
				if (bearers != Bearers.none && !bearer.character.Contains(bearers))
				{
					return false;
				}
				if (bearerIsNot != Bearers.none && bearer.character.Contains(bearerIsNot))
				{
					return false;
				}
			}
		}
		if (card.nextturn > GetInt(Variables.turns))
		{
			return false;
		}
		return TestCond(card.conditions);
	}

	public bool TestCond(List<Condition> conditions)
	{
		bool flag = true;
		foreach (Condition cond in conditions)
		{
			if (cond.place != Backgrounds.none)
			{
				if (cond.place == Backgrounds.defaut)
				{
					int result = 0;
					int.TryParse(cond.custom_name, out result);
					flag = NavigationAct.diff.TestNav(result, cond.value, cond.condition);
				}
				else
				{
					flag = NavigationAct.diff.TestNav(cond.place, cond.value, cond.condition);
				}
				continue;
			}
			Bearer bearer = HasBearer(cond.bearer);
			switch (cond.condition)
			{
			case Conditions.above:
				if (cond.bearer != Bearers.none)
				{
					if (bearer == null)
					{
						flag = false;
					}
					else if (bearer.vote < (float)cond.value)
					{
						flag = false;
					}
				}
				else if (GetVal(cond.variable, cond.custom_name) < cond.value)
				{
					flag = false;
				}
				break;
			case Conditions.below:
				if (cond.bearer != Bearers.none)
				{
					if (bearer == null)
					{
						flag = false;
					}
					else if (bearer.vote > (float)cond.value)
					{
						flag = false;
					}
				}
				else if (GetVal(cond.variable, cond.custom_name) > cond.value)
				{
					flag = false;
				}
				break;
			case Conditions.equal:
				if (cond.variable == Variables.seen)
				{
					if (cond.bearer != Bearers.none)
					{
						if (!seenBearers.Contains((int)cond.bearer))
						{
							flag = false;
						}
					}
					else if (!seenCards.Contains(cond.value))
					{
						flag = false;
					}
				}
				else if (cond.variable == Variables.chain)
				{
					if (cond.bearer == Bearers.none && lastcardId != cond.value)
					{
						flag = false;
					}
					else if (cond.bearer != Bearers.none && lastBearer != cond.bearer)
					{
						flag = false;
					}
				}
				else if (cond.bearer != Bearers.none)
				{
					if (bearer == null)
					{
						flag = false;
					}
					else if (cond.bearerIsAlso != Bearers.none && !bearer.character.Contains(cond.bearerIsAlso))
					{
						flag = false;
					}
					else if (cond.bearerIsNot != Bearers.none && bearer.character.Contains(cond.bearerIsNot))
					{
						flag = false;
					}
				}
				else if (cond.custom_name == "goal")
				{
					if (!NavigationAct.diff.HasGoal(cond.value))
					{
						flag = false;
					}
				}
				else if (GetVal(cond.variable, cond.custom_name) != cond.value)
				{
					flag = false;
				}
				break;
			case Conditions.notequal:
				if (cond.variable == Variables.seen)
				{
					if (cond.bearer != Bearers.none)
					{
						if (seenBearers.Contains((int)cond.bearer))
						{
							flag = false;
						}
					}
					else if (seenCards.Contains(cond.value))
					{
						flag = false;
					}
				}
				else if (cond.bearer != Bearers.none)
				{
					if (bearer != null)
					{
						flag = false;
					}
				}
				else if (cond.variable == Variables.chain)
				{
					if (card == null)
					{
						flag = false;
					}
					else if (!ignoredId.Contains(cond.value))
					{
						flag = false;
					}
					else if (GetVal(cond.variable, cond.custom_name) == cond.value)
					{
						flag = false;
					}
				}
				break;
			case Conditions.round:
				if (cond.bearer != Bearers.none)
				{
					Bearer bearer2 = reader.bearerModels.Find((Bearer it) => it.bearer == cond.bearer);
					if (cond.bearerIsAlso != Bearers.none && !bearer2.character.Contains(cond.bearerIsAlso))
					{
						flag = false;
					}
					else if (cond.bearerIsNot != Bearers.none && bearer2.character.Contains(cond.bearerIsNot))
					{
						flag = false;
					}
				}
				else if (GetVal(cond.variable, cond.custom_name) % cond.value != 0)
				{
					flag = false;
				}
				break;
			}
			if (!cond.orlimit && !flag)
			{
				return false;
			}
			if (!(cond.orlimit && flag))
			{
				continue;
			}
			return true;
		}
		return flag;
	}

	private int GetVal(Variables var, string customname)
	{
		if (!string.IsNullOrEmpty(customname))
		{
			return GetInt(customname);
		}
		int num = GetInt(var);
		if (multi10.Contains(var))
		{
			if (num >= 2)
			{
				return Mathf.CeilToInt((float)num / 11f);
			}
			return 0;
		}
		return num;
	}

	private void DestroyCard(Card card)
	{
		cardtoberemoved = card;
	}

	private void DoDestroy()
	{
		if (cardtoberemoved != null)
		{
			cardtoberemoved.weight = (cardtoberemoved.weightReal = -666);
			cardtoberemoved.weightVar = 0;
			cardtoberemoved = null;
		}
		if (bearertoremove != Bearers.none)
		{
			RemoveBearer(bearertoremove);
			bearertoremove = Bearers.none;
		}
	}

	public string TreatText(GText text, bool withcut = false)
	{
		string text2 = text.Get();
		bool maleSelf = true;
		foreach (KeyValuePair<Bearers, Bearer> role in roles)
		{
			string value = "<" + role.Key.ToString() + ">";
			if (text2.Contains(value) && role.Value.character.Contains(Bearers.female))
			{
				maleSelf = false;
			}
		}
		string value2 = "<self>";
		if (text2.Contains(value2) && curBearer != null && curBearer.character.Contains(Bearers.female))
		{
			maleSelf = false;
		}
		return TreatText(text.Get(speaker.isMonarkMale, maleSelf), withcut);
	}

	public string TreatText(string input, bool withcut = false)
	{
		if (string.IsNullOrEmpty(input))
		{
			return "";
		}
		foreach (KeyValuePair<Bearers, Bearer> role in roles)
		{
			string text = "<" + role.Key.ToString() + ">";
			if (input.Contains(text))
			{
				Bearer value = role.Value;
				if (value != null)
				{
					input = input.Replace(text, value.generated.Get());
				}
			}
		}
		string text2 = "<self>";
		if (input.Contains(text2) && curBearer != null)
		{
			input = input.Replace(text2, curBearer.generated.Get());
		}
		text2 = "<destination>";
		if (input.Contains(text2))
		{
			string sceneText = SpeechAct.diff.GetSceneText(NavigationAct.diff.GetSetGoal(nextonly: true));
			input = input.Replace(text2, sceneText);
		}
		text2 = "<place>";
		if (input.Contains(text2))
		{
			string sceneText2 = SpeechAct.diff.GetSceneText(BackgroundAct.diff.GetNextName());
			input = input.Replace(text2, sceneText2);
		}
		text2 = "<goal>";
		if (input.Contains(text2))
		{
			string sceneText3 = SpeechAct.diff.GetSceneText(NavigationAct.diff.GetSetGoal());
			input = input.Replace(text2, sceneText3);
		}
		text2 = "<planet>";
		if (input.Contains(text2))
		{
			string sceneText4 = SpeechAct.diff.GetSceneText(NavigationAct.diff.GetSetGoal(nextonly: false, Backgrounds.planet));
			input = input.Replace(text2, sceneText4);
		}
		text2 = "<last>";
		if (input.Contains(text2))
		{
			string sceneText5 = SpeechAct.diff.GetSceneText(BackgroundAct.diff.lastBack);
			input = input.Replace(text2, sceneText5);
		}
		text2 = "<band>";
		if (input.Contains(text2))
		{
			string newValue = ((SpeechAct.diff.lang == "ar") ? GetGroupName() : ("<color=#e2081e>" + GetGroupName() + "</color>"));
			input = input.Replace(text2, newValue);
		}
		text2 = "<barname>";
		if (input.Contains(text2))
		{
			string barname = SpeechAct.diff.GetBarname(BackgroundAct.diff.GetNextName());
			input = input.Replace(text2, barname);
		}
		foreach (DataCustom item in dataCustom)
		{
			string text3 = "<" + item.var + ">";
			if (input.Contains(text3))
			{
				input = input.Replace(text3, item.val.ToString());
			}
		}
		if (curBearer != null)
		{
			input = input.Replace("<anyone>", curBearer.name);
		}
		if (input.Contains("€"))
		{
			input = input.Remove(input.IndexOf("€"), 1);
		}
		if (input.Contains("£"))
		{
			input = input.Split(new char[1] { '£' }, StringSplitOptions.None)[1];
		}
		foreach (DataVariable item2 in dataVar)
		{
			string text4 = "<" + item2.var.ToString() + ">";
			if (input.Contains(text4))
			{
				input = ((item2.var == Variables.price) ? input.Replace(text4, SpeechAct.diff.GetSmartText("money", 0, -item2.val)) : input.Replace(text4, Mathf.Abs(item2.val).ToString()));
			}
		}
		return SpeechAct.diff.FinalFormat(input);
	}

	public string GetGroupName()
	{
		int cid = GetInt("bandname");
		if (cid < 1)
		{
			cid = 220;
		}
		if (cacheId == cid)
		{
			return cacheGroup;
		}
		cacheId = cid;
		string text = hiddenCards.Find((Card it) => it.id == cid).question.Get();
		int num;
		int num2;
		if (SpeechAct.diff.lang == "ar")
		{
			num = text.LastIndexOf("r>") + 2;
			num2 = text.LastIndexOf("<c");
		}
		else
		{
			num2 = text.LastIndexOf("</");
			num = text.LastIndexOf("e>") + 2;
		}
		cacheGroup = text.Substring(num, num2 - num);
		return cacheGroup;
	}

	public void LockCards(Condition cond, bool locked = true)
	{
		string name = cond.custom_name;
		for (int i = 0; i < 2; i++)
		{
			List<Card> list = ((i == 0) ? hiddenCards : cards);
			for (int j = 0; j < list.Count; j++)
			{
				Card card = list[j];
				if (cond.bearer == Bearers.none)
				{
					if (cond.variable == Variables.chain)
					{
						if (card.name == cond.custom_name)
						{
							card.isLocked = locked;
						}
					}
					else if (card.conditions.Find((Condition it) => it.condition == Conditions.notequal && it.value == cond.value) != null)
					{
						card.isLocked = locked;
					}
					else if (card.conditions.Find((Condition it) => it.condition == Conditions.equal && it.value == -1 && it.custom_name == name) != null)
					{
						card.isLocked = locked;
					}
				}
				else if (card.conditions.Find((Condition it) => it.condition == Conditions.notequal && it.value == 0 && it.bearer == cond.bearer) != null)
				{
					card.isLocked = locked;
				}
			}
		}
	}

	public void LockCardsOutcome(Condition cond, bool locked = true)
	{
		string name = cond.custom_name;
		for (int i = 0; i < 2; i++)
		{
			List<Card> list = ((i == 0) ? hiddenCards : cards);
			for (int j = 0; j < list.Count; j++)
			{
				Card card = list[j];
				if (cond.bearer == Bearers.none)
				{
					if (card.yes_outcomes.Find((Outcome it) => it.value == -1 && it.custom_name == name) != null || card.no_outcomes.Find((Outcome it) => it.value == -1 && it.custom_name == name) != null)
					{
						card.isLocked = locked;
					}
				}
				else if (card.yes_outcomes.Find((Outcome it) => it.variable == Variables.remove && it.bearer == cond.bearer) != null || card.no_outcomes.Find((Outcome it) => it.variable == Variables.remove && it.bearer == cond.bearer) != null)
				{
					card.isLocked = locked;
				}
			}
		}
	}

	public List<Bearer> GetRegularBearers()
	{
		if (regulars != null)
		{
			return regulars;
		}
		regulars = CardReader.diff.bearerModels.FindAll((Bearer it) => (it.type == BearerTypes.special && !string.IsNullOrEmpty(it.name)) || it.type == BearerTypes.individual || it.type == BearerTypes.generated);
		return regulars;
	}

	public int GetCardsNb()
	{
		return hiddenCards.Count + cards.Count;
	}

	public int GetSeenCardsNb()
	{
		return seenCards.Count;
	}

	public void Equalize()
	{
		SetInt(Variables.length, 0);
		SetInt(Variables.oxygen, 50);
		SetInt(Variables.power, 50);
		SetInt(Variables.hull, 50);
		SetInt(Variables.people, 50);
	}

	private void Equalize(Variables var)
	{
		SetInt(var, 50);
		SetInt(var, 50);
	}

	private float GetVote(Bearers bearer)
	{
		return bearers.Find((Bearer it) => it.bearer == bearer)?.vote ?? 0f;
	}

	private void SetDefaultVariable(Variables var)
	{
		DataVariable dataVariable = dataVar.Find((DataVariable it) => it.var == var);
		if (dataVariable != null)
		{
			SetDefaultVariable(dataVariable);
		}
	}

	private void SetDefaultVariable(DataVariable data)
	{
		data.val = GetDefaultVariable(data.var);
		if (OnDataChange != null)
		{
			OnDataChange(data.var, data.val);
		}
	}

	private int GetDefaultVariable(Variables var)
	{
		switch (var)
		{
		case Variables.journey:
			return 1;
		case Variables.length:
		case Variables.overall:
			return 0;
		case Variables.distance:
			return 0;
		case Variables.price:
			return 0;
		default:
			return 0;
		}
	}

	public bool GetBool(string var)
	{
		return GetInt(var) == 1;
	}

	public bool GetBool(Variables var)
	{
		return GetInt(var) == 1;
	}

	public int GetInt(Variables var)
	{
		DataVariable dataVariable = dataVar.Find((DataVariable it) => it.var == var);
		if (dataVariable == null)
		{
			int defaultVariable = GetDefaultVariable(var);
			dataVar.Add(new DataVariable(var, defaultVariable));
			return defaultVariable;
		}
		return dataVariable.val;
	}

	public int GetInt(string var)
	{
		DataCustom dataCustom = this.dataCustom.Find((DataCustom it) => it.var == var);
		if (dataCustom == null)
		{
			if (var.StartsWith("nb_"))
			{
				return 0;
			}
			return -1;
		}
		return dataCustom.val;
	}

	public void SetBool(string var, bool boo)
	{
		int num = (boo ? 1 : (-1));
		DataCustom dataCustom = this.dataCustom.Find((DataCustom it) => it.var == var);
		if (dataCustom == null)
		{
			this.dataCustom.Add(new DataCustom(var, num));
		}
		else
		{
			dataCustom.val = num;
		}
	}

	public bool SetInt(Variables var, int val)
	{
		DataVariable dataVariable = dataVar.Find((DataVariable it) => it.var == var);
		if (dataVariable == null)
		{
			dataVar.Add(new DataVariable(var, val));
		}
		else
		{
			if (dataVariable.val == val)
			{
				return false;
			}
			dataVariable.val = val;
		}
		if (OnDataChange != null)
		{
			OnDataChange(var, val);
		}
		return true;
	}

	public bool SetInt(string var, int val)
	{
		DataCustom dataCustom = this.dataCustom.Find((DataCustom it) => it.var == var);
		if (dataCustom == null)
		{
			this.dataCustom.Add(new DataCustom(var, val));
		}
		else
		{
			if (dataCustom.val == val)
			{
				return false;
			}
			dataCustom.val = val;
		}
		return true;
	}

	public void AddInt(Variables var, int val = 1, int min = 0, int max = 999999999)
	{
		if (val == 0)
		{
			return;
		}
		DataVariable dataVariable = dataVar.Find((DataVariable it) => it.var == var);
		if (dataVariable == null)
		{
			dataVar.Add(new DataVariable(var, val));
			if (OnDataChange != null)
			{
				OnDataChange(var, val);
			}
		}
		else
		{
			dataVariable.val = Mathf.Clamp(dataVariable.val + val, min, max);
			if (OnDataChange != null)
			{
				OnDataChange(var, dataVariable.val);
			}
		}
	}

	public void AddInt(string var, int val, int min = 0, int max = 999999999)
	{
		DataCustom dataCustom = this.dataCustom.Find((DataCustom it) => it.var == var);
		if (dataCustom == null)
		{
			this.dataCustom.Add(new DataCustom(var, Mathf.Clamp(val, min, max)));
		}
		else
		{
			dataCustom.val = Mathf.Clamp(dataCustom.val + val, min, max);
		}
	}

	public int GetLength()
	{
		return GetInt(Variables.length);
	}

	public int GetDistance()
	{
		return GetInt(Variables.distance);
	}

	public bool IsMaxAge(int age)
	{
		if (age >= maxage)
		{
			maxage = age;
			return true;
		}
		return false;
	}

	public bool Has(string var)
	{
		DataCustom dataCustom = this.dataCustom.Find((DataCustom it) => it.var == var);
		if (dataCustom != null && dataCustom.val > 0)
		{
			return true;
		}
		return false;
	}

	private bool Has(Variables var)
	{
		DataVariable dataVariable = dataVar.Find((DataVariable it) => it.var == var);
		if (dataVariable != null && dataVariable.val > 0)
		{
			return true;
		}
		return false;
	}

	public void AddKnownVariable(Variables var)
	{
		if (dataVar.Find((DataVariable it) => it.var == var) == null)
		{
			int defaultVariable = GetDefaultVariable(var);
			dataVar.Add(new DataVariable(var, defaultVariable));
		}
	}

	private void SetDefaultCustom(DataCustom data)
	{
		data.val = GetDefaultCustom(data.var);
	}

	private int GetDefaultCustom(string var)
	{
		if (!var.StartsWith("nb_") && !var.StartsWith("inc_"))
		{
			return -1;
		}
		return 0;
	}

	public void AddCustomVariable(string var)
	{
		if (dataCustom.Find((DataCustom it) => it.var == var) == null)
		{
			if (var.StartsWith("inc_"))
			{
				incrCustom.Add(var);
			}
			dataCustom.Add(new DataCustom(var, GetDefaultCustom(var)));
		}
	}

	public void AddEndCard(string name)
	{
		if (!string.IsNullOrEmpty(name) && !endCards.Contains(name))
		{
			endCards.Add(name);
		}
	}

	private void AddSeenEndCard(string id)
	{
		if (DeadCloneAct.diff.AddStat("e_" + id))
		{
			PlayModal(ModalTypes.death, " ", 0f, speaker.GetSceneTextFinal("end_" + id));
		}
	}

	public void ForceDecision(bool yes = true)
	{
		cardType = CardTypes.character;
		if (yes)
		{
			decision = 1;
		}
		else
		{
			decision = -1;
		}
		ValidateDecision();
	}

	private void ValidateDecision()
	{
		if (cardType != CardTypes.intercale || card.bearer == Bearers.intercale)
		{
			nextCard = "";
		}
		if (OnValidateDecision != null)
		{
			OnValidateDecision(decision);
		}
		if (cardType == CardTypes.end)
		{
			EndJourney();
		}
		AddOutcomes();
		if (state != GameStates.gameover)
		{
			state = GameStates.transition;
		}
		decision = -10;
		onlyNo = (onlyYes = false);
		if ((bool)AnimBut.diff)
		{
			AnimBut.diff.Lock();
		}
	}

	private List<Outcome> GetOutcomeList(int dec)
	{
		return dec switch
		{
			2 => card.load_outcomes, 
			1 => card.yes_outcomes, 
			-1 => card.no_outcomes, 
			_ => null_outcomes, 
		};
	}

	private void UpdateDecision(int dec)
	{
		decision = dec;
		if (card != null && cardType != CardTypes.intercale)
		{
			ShowOutcome(GetOutcomeList(dec));
		}
	}

	public string GetCurCardName()
	{
		if (card == null)
		{
			return "";
		}
		return card.name;
	}

	public float GetCurCardOutcome(bool yes, Variables variab)
	{
		if (card == null)
		{
			return 0f;
		}
		Outcome outcome = (yes ? card.yes_outcomes : card.no_outcomes).Find((Outcome it) => it.variable == variab);
		if (outcome != null)
		{
			return outcome.value;
		}
		return 0f;
	}

	public void HideOutcome()
	{
		ShowOutcome(null_outcomes);
	}

	private void ShowOutcome(List<Outcome> outco)
	{
		scMeters.ShowOutcome(outco, curBearer);
	}

	private void InitNumbers()
	{
		scMeters.SetDefault();
	}

	public void ShowDataCol(bool yes)
	{
		scMeters.ShowAllData(yes);
	}

	public void SetIntercale(GText source)
	{
		intercale = TreatText(source);
		cardType = CardTypes.intercale;
	}

	private void AddOutcomes()
	{
		if (decision == 0 || card == null || string.IsNullOrEmpty(card.name))
		{
			return;
		}
		bool num = cardType != CardTypes.intercale || card.bearer == Bearers.intercale;
		if (cardType != CardTypes.intercale)
		{
			if (decision == -1 && !card.answer_no.isEmpty)
			{
				if (card.no_outcomes.Find((Outcome it) => it.variable == Variables.chain && it.custom_name.EndsWith("_choice")) != null)
				{
					ChangeQuestion(card.answer_no);
				}
				else
				{
					SetIntercale(card.answer_no);
				}
			}
			else if (decision == 1 && !card.answer_yes.isEmpty)
			{
				if (card.yes_outcomes.Find((Outcome it) => it.variable == Variables.chain && it.custom_name.EndsWith("_choice")) != null)
				{
					ChangeQuestion(card.answer_yes);
				}
				else
				{
					SetIntercale(card.answer_yes);
				}
			}
		}
		else
		{
			cardType = CardTypes.character;
		}
		if (curBearer != null && curBearer.hasVote)
		{
			float vote = curBearer.vote;
			curBearer.vote = Mathf.Clamp(vote / 2f + (float)(decision * 2), -5f, 5f);
		}
		if (num)
		{
			TreatOutcomes(decision);
		}
		AddInt(Variables.turns);
		if (!(nextCard == ""))
		{
			return;
		}
		int num2 = GetInt(Variables.power);
		int num3 = GetInt(Variables.oxygen);
		int num4 = GetInt(Variables.hull);
		int num5 = GetInt(Variables.people);
		if (num2 == 0 || num2 == 100 || num3 == 0 || num3 == 100 || num4 == 0 || num4 == 100 || num5 == 0 || num5 == 100 || BackgroundAct.diff.Landing())
		{
			return;
		}
		List<PostponeEvent> list = postponeEvents.FindAll((PostponeEvent it) => it.distance <= GetInt(Variables.distance));
		if (list.Count > 0 && !GetBool(Variables.stop))
		{
			PostponeEvent postponeEvent = list[0];
			if (postponeEvent.bear == Bearers.none)
			{
				SetNextCard(postponeEvent.card);
			}
			postponeEvents.Remove(postponeEvent);
		}
	}

	private void TreatOutcomes(int decision)
	{
		List<Outcome> outcomeList = GetOutcomeList(decision);
		TreatOutcomes(outcomeList, decision);
	}

	public void TreatOutcomes(List<Outcome> outco, int decision = -1)
	{
		bool flag = false;
		Dictionary<Variables, int> dictionary = new Dictionary<Variables, int>();
		for (int i = 0; i < outco.Count; i++)
		{
			Outcome outcome = outco[i];
			bool flag2 = false;
			if (outcome.orlimit)
			{
				if (Util.Rand() > 0.5f)
				{
					flag2 = true;
				}
				else
				{
					flag = true;
				}
			}
			if (flag)
			{
				flag = false;
				continue;
			}
			if (flag2)
			{
				flag = true;
			}
			string custom_name = outcome.custom_name;
			switch (outcome.variable)
			{
			case Variables.custom:
				if (string.IsNullOrEmpty(custom_name))
				{
					continue;
				}
				if (custom_name.StartsWith("sfx_"))
				{
					try
					{
						SFXTypes sFXTypes = (SFXTypes)Enum.Parse(typeof(SFXTypes), custom_name.Substring(4));
						JukeBox.diff.PlaySound(sFXTypes);
					}
					catch
					{
					}
				}
				else if (custom_name.StartsWith("eff_"))
				{
					string value = custom_name.Substring(4);
					EffectStyles style = (EffectStyles)Enum.Parse(typeof(EffectStyles), value);
					CameffectAct.diff.PlayEffect(style);
				}
				else if (custom_name.StartsWith("mus_"))
				{
					if (custom_name == "mus_stop")
					{
						JukeBox.diff.StopMusic();
					}
					else
					{
						JukeBox.diff.PlayImportantMusic(custom_name.Substring(4));
					}
				}
				else if (custom_name.StartsWith("nb_") || custom_name.StartsWith("inc_"))
				{
					if (outcome.value == 0)
					{
						SetInt(custom_name, 0);
					}
					else
					{
						AddInt(custom_name, outcome.value);
					}
				}
				else if (outcome.display == DataDisplay.fullamount)
				{
					SetInt(custom_name, outcome.value);
				}
				else
				{
					AddInt(custom_name, outcome.value);
				}
				continue;
			case Variables.set:
			{
				if (outcome.bearer != Bearers.none)
				{
					Bearers all = (Bearers)Enum.Parse(typeof(Bearers), outcome.custom_name);
					AddChara(outcome.bearer, all);
					continue;
				}
				SetInt(Variables.destination, outcome.value);
				int cid = 0;
				int.TryParse(outcome.custom_name, out cid);
				if (cid > 0)
				{
					Card card = cards.Find((Card it) => it.id == cid);
					if (card == null)
					{
						card = hiddenCards.Find((Card it) => it.id == cid);
					}
					if (card != null)
					{
						NavigationAct.diff.AddPoint(card, outcome.value);
					}
				}
				else
				{
					Backgrounds type = (Backgrounds)Enum.Parse(typeof(Backgrounds), outcome.custom_name);
					NavigationAct.diff.AddPoint(type, outcome.value);
				}
				continue;
			}
			case Variables.add:
				AddBearer(outcome.bearer);
				continue;
			case Variables.chain:
				if (outcome.value > 0)
				{
					postponeEvents.Add(new PostponeEvent(GetDistance() + outcome.value, custom_name));
				}
				else if (decision != 2)
				{
					SetNextCard(custom_name);
				}
				continue;
			case Variables.remove:
				if (string.IsNullOrEmpty(custom_name))
				{
					bearertoremove = ((outcome.bearer == Bearers.anyone) ? curBearer.bearer : outcome.bearer);
				}
				else if (custom_name == "self" || custom_name == "anyone")
				{
					RemoveChara(outcome.bearer, curBearer.bearer);
				}
				else
				{
					RemoveChara(outcome.bearer, (Bearers)Enum.Parse(typeof(Bearers), custom_name));
				}
				continue;
			case Variables.destroy:
				if (outcome.bearer != Bearers.none)
				{
					bearertoremove = outcome.bearer;
				}
				else
				{
					DestroyCard(this.card);
				}
				continue;
			case Variables.people:
			case Variables.oxygen:
			case Variables.power:
			case Variables.hull:
				dictionary.Add(outcome.variable, outcome.value);
				continue;
			case Variables.money:
			{
				int num = GetInt(Variables.price);
				MonoBehaviour.print("price " + num);
				if (num == 0)
				{
					if (outcome.display == DataDisplay.fullamount)
					{
						SetInt(Variables.money, outcome.value);
					}
					else
					{
						AddInt(Variables.money, outcome.value);
					}
					continue;
				}
				int num2 = GetInt(Variables.money) + num;
				MonoBehaviour.print("new money " + num2);
				SetInt(Variables.price, 0);
				if (num2 < 0)
				{
					SetNextCard("_nomoney");
					return;
				}
				SetInt(Variables.money, num2);
				if (this.card.name.StartsWith("_sell"))
				{
					AddInt("nb_sale", 1);
				}
				continue;
			}
			}
			if (!string.IsNullOrEmpty(custom_name))
			{
				if (SetInt(custom_name, outcome.value) && outcome.variable != Variables.custom)
				{
					if (outcome.value == 0)
					{
						AddInt(outcome.variable, -1);
					}
					else
					{
						AddInt(outcome.variable);
					}
				}
			}
			else if (outcome.display == DataDisplay.fullamount)
			{
				SetInt(outcome.variable, outcome.value);
			}
			else
			{
				AddInt(outcome.variable, outcome.value);
			}
		}
		JukeBox.diff.PlayValues(dictionary);
		if (decision == 2)
		{
			scMeters.ShowOutcome(outco, curBearer, andresolve: true);
		}
		else
		{
			scMeters.ResolveAddition();
		}
	}

	public void NewYear()
	{
		if (NavigationAct.diff.GetNextPointDistance() < 1)
		{
			return;
		}
		bool num = GetInt(Variables.stop) == 1;
		bool flag = GetInt(Variables.skip) == 1;
		if (!num)
		{
			if (flag)
			{
				SetInt(Variables.skip, -1);
				return;
			}
			AddInt(Variables.distance);
			AddInt(Variables.length);
			AddInt(Variables.overall);
			JukeBox.diff.PlaySound(SFXTypes.ui_ageup);
		}
	}

	public void SetNextCard(string name)
	{
		switch (name)
		{
		case "previous":
			if (lastCard != null)
			{
				OpenCard(lastCard);
			}
			break;
		case "lastspot":
			if (lastSpotId > -1)
			{
				OpenCard(lastSpotId);
			}
			break;
		case "force_end":
			state = GameStates.gameover;
			SetBool("onmara", boo: false);
			SetResurrect();
			SceneManager.LoadScene(0);
			break;
		default:
			nextCard = name;
			break;
		}
	}

	public void SetRulerName(string newname)
	{
	}

	public void AddEffectCard(Effect effect)
	{
		cardType = CardTypes.effect;
		cardSc = GetCardAct(Bearers.effect);
		cardSc.gameObject.SetActive(value: true);
		curEffect = effect;
	}

	private void DisplayEffectCard()
	{
		cardSc.gameObject.SetActive(value: true);
		cardSc.GetComponent<EffectCard>().InitEffect(curEffect, curdec);
		cardType = CardTypes.character;
		card = null;
	}

	public ModalAct PlayModal(ModalTypes type, object instance, float delay = 0f, string txtid = "", bool decal = true)
	{
		if (instance == lastinstance && modalsToFire.Count == 1)
		{
			return null;
		}
		lastinstance = instance;
		GameObject obj = UnityEngine.Object.Instantiate(modalPrefab);
		obj.transform.SetParent(modalUI, worldPositionStays: false);
		ModalAct component = obj.GetComponent<ModalAct>();
		modalsToFire.Add(component);
		component.Init(type, instance, delay, txtid, decal);
		if (modalsToFire.Count == 1)
		{
			StopCoroutine("FireModals");
			StartCoroutine("FireModals");
		}
		return component;
	}

	public void DestroyModals()
	{
		foreach (Transform item in modalUI)
		{
			if (item != null)
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
		}
		modalsToFire = new List<ModalAct>();
	}

	private IEnumerator FireModals()
	{
		while (modalsToFire.Count > 0)
		{
			ModalAct scmod = modalsToFire[0];
			yield return StartCoroutine(scmod.Fire());
			modalsToFire.RemoveAt(0);
			if (OnNewModal != null)
			{
				OnNewModal(scmod);
			}
		}
	}

	private void ShrinkCards(float amo, Vector2 displace)
	{
		StopCoroutine("ResizeCards");
		StopCoroutine("DisplaceCards");
		StartCoroutine("ResizeCards", amo);
		StartCoroutine("DisplaceCards", displace);
	}

	private void ExpandCards()
	{
		StopCoroutine("ResizeCards");
		StopCoroutine("DisplaceCards");
		StartCoroutine("ResizeCards", 1);
		StartCoroutine("DisplaceCards", new Vector2(0f, 0f));
	}

	private IEnumerator ResizeCards(float targ)
	{
		float t = 0f;
		Vector3 tSize = new Vector3(targ, targ, 1f);
		while (t < 1f)
		{
			t += Time.deltaTime * 2f;
			characterRepo.localScale = Vector3.Lerp(characterRepo.localScale, tSize, Easing.QuintEaseOut(t, 0f, 1f, 1f));
			yield return 0;
		}
		characterRepo.localScale = tSize;
	}

	private IEnumerator DisplaceCards(Vector2 targ)
	{
		float t = 0f;
		RectTransform caTrans = characterRepo.GetComponent<RectTransform>();
		while (t < 1f)
		{
			t += Time.deltaTime * 2f;
			caTrans.anchoredPosition = Vector3.Lerp(caTrans.anchoredPosition, targ, Easing.QuintEaseOut(t, 0f, 1f, 1f));
			yield return 0;
		}
		caTrans.anchoredPosition = targ;
	}

	private void StartInteraction()
	{
		cPo = centralPos;
		InputAct.diff.GetSlideFocus(UpdateSlide, StopSlide, StartSlide, ValidSlide, DownSlide, allowCumul: false, 0.2f);
	}

	public bool DownSlide(bool down)
	{
		if (state != GameStates.interaction)
		{
			return false;
		}
		return true;
	}

	public void UpdateSlide(Vector2 amo)
	{
		float num = Mathf.Abs(amo.x);
		float f = amo.x;
		if (onlyYes)
		{
			f = num;
		}
		if (onlyNo)
		{
			f = 0f - num;
		}
		float y = amo.y;
		cPo = centralPos + new Vector2(Mathf.Sign(f) * Easing.QuintEaseOut(Mathf.Clamp(num * 2.4f, 0f, 1f), 0f, 1f, 1f) * 80f, y * 400f);
	}

	public void ValidSlide(Vector2 xp)
	{
		if (cardType != CardTypes.selection && state == GameStates.interaction)
		{
			cPo = centralPos;
			ValidateDecision();
		}
	}

	public void StopSlide()
	{
		if (cardType != CardTypes.selection)
		{
			if (cardSc != null)
			{
				cardSc.ShowDecision(0);
			}
			HideDecision(forcenull: true);
			cPo = centralPos;
		}
	}

	public void StartSlide(Vector2 amo)
	{
		float num = amo.x;
		if (onlyYes)
		{
			num = Mathf.Abs(num);
		}
		if (onlyNo)
		{
			num = 0f - Mathf.Abs(num);
		}
		if (num < 0f)
		{
			ShowYes();
		}
		else if (num > 0f)
		{
			ShowNo();
		}
	}

	private void ShowYes()
	{
		ShowDec(-1, SFXTypes.card_swipe_left);
	}

	private void ShowNo()
	{
		ShowDec(1, SFXTypes.card_swipe_right);
	}

	private void ShowDec(int dec, SFXTypes sfx)
	{
		if (cardType != CardTypes.selection && decision != dec)
		{
			UpdateDecision(dec);
			if (OnChoice != null)
			{
				OnChoice(dec);
			}
			if ((bool)cardSc)
			{
				cardSc.ShowDecision(dec);
			}
			JukeBox.diff.PlaySound(sfx);
		}
	}

	private void HideDecision(bool forcenull = false)
	{
		if (decision != 0)
		{
			if (forcenull)
			{
				UpdateDecision(0);
			}
			else
			{
				UpdateDecision(decision);
			}
			if (OnChoice != null)
			{
				OnChoice(decision);
			}
		}
	}

	private Card GetCard(int id)
	{
		Card card = cards.Find((Card it) => it.id == id);
		if (card == null)
		{
			card = hiddenCards.Find((Card it) => it.id == id);
		}
		return card;
	}

	public void OpenCard(string next)
	{
		nextCard = next;
	}

	public void OpenCard(int id)
	{
		OpenCard(GetCard(id));
	}

	public void OpenCard(Card nxt)
	{
		if (nxt != null)
		{
			if (cardType == CardTypes.selection)
			{
				ValidSelectionDirect(nxt);
			}
			nextCard = nxt.name;
			forceCard = nxt;
			forcenext = true;
			cardType = CardTypes.character;
			StopCoroutine("YieldUntilInteraction");
			GameStates gameStates = state;
			if (gameStates != GameStates.interaction)
			{
				StartCoroutine("YieldUntilInteraction");
			}
			else
			{
				state = GameStates.transition;
			}
		}
	}

	private IEnumerator YieldUntilInteraction()
	{
		while (state != GameStates.interaction)
		{
			yield return null;
		}
		state = GameStates.transition;
	}

	private void Update()
	{
		if (state == GameStates.interaction && cardType != CardTypes.selection && cardSc != null)
		{
			if (decision == 0)
			{
				cardSc.LerpToPos(cPo, Time.deltaTime * 6f);
			}
			else
			{
				cardSc.LerpToPos(cPo, Time.deltaTime * 8f);
			}
			cardSc.SlerpToPos(cPo.x, cPo.y);
		}
	}
}
