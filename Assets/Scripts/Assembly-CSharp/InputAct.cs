using System;
using System.Collections;
using System.Collections.Generic;
using Rewired;
using Steamworks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InputAct : MonoBehaviour
{
	private enum Positions
	{
		idle = 0,
		down = 1,
		whiledown = 2,
		up = 3
	}

	public Inputs curInput;

	public Action<bool> OnSwitchMenu;

	private Action<Vector2> OnSlideUpdate;

	private Action OnSlideStop;

	public Func<bool, bool> OnAction;

	public Action OnActionDown;

	private Action<Vector2> OnSlideValid;

	private Action<Vector2> OnSlideStart;

	private Func<bool, bool> OnSlideDown;

	public Action<Inputs> OnSwitchControl;

	private Player player;

	public static InputAct diff;

	private Vector2 tdecal;

	public bool touching;

	public Vector2 tpo = Vector2.zero;

	public bool whiledown;

	public bool up;

	private Vector2 cPo;

	private float yMin;

	private float yMax;

	private bool isSuspended;

	public GameObject leapPrefab;

	private bool isActionFocus;

	public bool isSimulating;

	private float simulationPos;

	public JourneyAct scKi;

	public float slideSign = 1f;

	private EventSystem eventSys;

	public GameObject KingdomFirst;

	public bool isInMenu;

	public bool isInventory;

	private GameObject quitBut;

	private bool suspendQuit;

	private Dictionary<Selectable, bool> objectSelectableInteractable = new Dictionary<Selectable, bool>();

	public bool longPortrait;

	public List<Inputs> availableInputs;

	private bool touch2;

	private GameObject dia;

	public GameObject DiaPrefab;

	public Transform Canvas;

	private bool nodeadzone;

	private Vector2 ori = Vector2.zero;

	private float stopamo = 0.01f;

	private Vector2 interpos = Vector2.zero;

	private bool hasClick;

	private bool hasTapAction;

	public bool NavigationMode()
	{
		if (curInput != Inputs.mouse)
		{
			return curInput != Inputs.touch;
		}
		return false;
	}

	public bool isLandscape(bool ignoreForcePortrait = false)
	{
		if (!ignoreForcePortrait && PlayerPrefs.HasKey("forceportrait"))
		{
			return false;
		}
		return true;
	}

	public void DisableMenuNav(bool closewindows = true)
	{
		DisableMenuNav(closewindows, false);
	}

	public void DisableMenuNav(bool closewindows = true, bool ignoreanimstate = false)
	{
		isInMenu = false;
		if (closewindows && isInventory)
		{
			if (objectSelectableInteractable.Count > 0)
			{
				foreach (KeyValuePair<Selectable, bool> item in objectSelectableInteractable)
				{
					item.Key.interactable = item.Value;
				}
				objectSelectableInteractable.Clear();
			}
			ActivateButtons(on: true);
		}
		else
		{
			eventSys.sendNavigationEvents = false;
			eventSys.SetSelectedGameObject(null);
			isInventory = false;
		}
		if (closewindows)
		{
			if (OnSwitchMenu != null)
			{
				OnSwitchMenu(obj: false);
			}
			if ((bool)NavigationAct.diff)
			{
				NavigationAct.diff.Activate();
			}
			if ((bool)MoneyUI.diff)
			{
				MoneyUI.diff.Activate();
			}
			if ((bool)MetersAct.diff)
			{
				MetersAct.diff.Activate();
			}
			if (scKi != null)
			{
				scKi.CloseWindows();
			}
			return;
		}
		RestoreSlideFocus();
		if ((bool)AnimBut.diff)
		{
			if (ignoreanimstate)
			{
				AnimBut.diff.Lock();
			}
			else
			{
				AnimBut.diff.ResetLock();
			}
		}
	}

	public void SetSelect(GameObject go)
	{
		eventSys.SetSelectedGameObject(go);
		go.GetComponent<Selectable>().Select();
	}

	public void SetQuit(GameObject but)
	{
		quitBut = but;
		suspendQuit = false;
	}

	public void SuspendQuit()
	{
		suspendQuit = true;
	}

	public void MenuNav(bool isinmenu = true)
	{
		Inputs inputs = curInput;
		if ((uint)inputs <= 2u || inputs == Inputs.tv || inputs == Inputs.ninSwitch)
		{
			eventSys.sendNavigationEvents = true;
		}
		isInMenu = isinmenu;
		if (isinmenu && OnSwitchMenu != null)
		{
			OnSwitchMenu(obj: true);
		}
		if (isinmenu)
		{
			if ((bool)NavigationAct.diff)
			{
				NavigationAct.diff.Deactivate();
			}
			if ((bool)MoneyUI.diff)
			{
				MoneyUI.diff.Deactivate();
			}
			if ((bool)MetersAct.diff)
			{
				MetersAct.diff.Deactivate();
			}
		}
	}

	public void OpenEffects()
	{
		if (!isInMenu)
		{
			DefaultWindow();
			scKi.OpenEffects(openmenu: true);
		}
	}

	public void OpenOptions()
	{
		if (!isInMenu && !(scKi == null))
		{
			DefaultWindow();
			scKi.OpenOptions(openmenu: true);
		}
	}

	public void OpenStats()
	{
		if (!isInMenu && !(scKi == null))
		{
			DefaultWindow();
			scKi.OpenStats(openmenu: true);
		}
	}

	public void OpenInventory()
	{
		if (!isInventory && !isInMenu && !(scKi == null))
		{
			AnimBut.diff.UnLock(ControlModes.multichoice);
			MenuNav(isinmenu: false);
			isInventory = true;
		}
	}

	public void ActivateButtons(bool on)
	{
		bool flag = on && NavigationMode();
		AutoSelectMe[] array = UnityEngine.Object.FindObjectsOfType<AutoSelectMe>();
		bool flag2 = true;
		AutoSelectMe[] array2 = array;
		foreach (AutoSelectMe autoSelectMe in array2)
		{
			if (autoSelectMe.Activate(on, flag && flag2))
			{
				flag2 = false;
			}
			if (isInventory && !on)
			{
				Selectable component = autoSelectMe.gameObject.GetComponent<Selectable>();
				if (component != null)
				{
					objectSelectableInteractable.Add(component, component.interactable);
					component.interactable = false;
				}
			}
		}
	}

	private void DefaultWindow()
	{
		if (isInventory)
		{
			ActivateButtons(on: false);
		}
		AnimBut.diff.UnLock(ControlModes.selectback);
		MenuNav();
	}

	private void Awake()
	{
		eventSys = GetComponent<EventSystem>();
		try
		{
			if (ReInput.players != null)
			{
				player = ReInput.players.GetPlayer(0);
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Rewired is unavailable; using Unity keyboard and mouse input. " + ex.Message);
			player = null;
		}
		diff = this;
	}

	public void SetIphoneX()
	{
		float num = (float)Screen.height / (float)Screen.width;
		longPortrait = false;
		if (num > 2f || num < 0.5f)
		{
			longPortrait = true;
			Canvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(600f, 674f);
		}
		else
		{
			Canvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(600f, 600f);
		}
	}

	private void Start()
	{
		availableInputs = new List<Inputs>
		{
			Inputs.none,
			Inputs.mouse,
			Inputs.keyboard,
			Inputs.xbox,
			Inputs.ps,
			Inputs.ninSwitch,
			Inputs.automated
		};
		if (PlayerPrefs.HasKey("input_keep"))
		{
			Inputs inputs = (Inputs)PlayerPrefs.GetInt("input_keep");
			if (player == null && inputs != Inputs.mouse && inputs != Inputs.keyboard && inputs != Inputs.touch)
			{
				inputs = Inputs.mouse;
			}
			SwitchControl(inputs);
		}
		else
		{
			SwitchControl(Inputs.mouse);
		}
	}

	private Inputs isPS()
	{
		if (AutomationController.Instance != null && AutomationController.Instance.Active)
		{
			return Inputs.automated;
		}
		Debug.LogWarning("someone pushed a button");
		if (player == null)
		{
			return Inputs.keyboard;
		}
		Controller controller = player.controllers.GetLastActiveController();
		if (controller == null && player.controllers.joystickCount > 0)
		{
			controller = player.controllers.Joysticks[0];
		}
		if (controller == null)
		{
			return Inputs.keyboard;
		}
		if (controller.type == ControllerType.Joystick)
		{
			Joystick obj = (Joystick)controller;
			string text = obj.hardwareTypeGuid.ToString();
			InputHandle_t controllerForGamepadIndex = default(InputHandle_t);
			if (SteamManager.Initialized)
			{
				try
				{
					controllerForGamepadIndex = SteamInput.GetControllerForGamepadIndex((int)obj.systemId.Value);
				}
				catch (Exception ex)
				{
					Debug.LogWarning("Steam Input controller lookup failed; using Rewired device information. " + ex.Message);
				}
			}
			if (controllerForGamepadIndex.m_InputHandle != 0L)
			{
				switch (SteamInput.GetInputTypeForHandle(controllerForGamepadIndex))
				{
				case ESteamInputType.k_ESteamInputType_SteamController:
				case ESteamInputType.k_ESteamInputType_XBox360Controller:
				case ESteamInputType.k_ESteamInputType_XBoxOneController:
				case ESteamInputType.k_ESteamInputType_GenericGamepad:
				case ESteamInputType.k_ESteamInputType_SteamDeckController:
					return Inputs.xbox;
				case ESteamInputType.k_ESteamInputType_PS4Controller:
				case ESteamInputType.k_ESteamInputType_PS3Controller:
				case ESteamInputType.k_ESteamInputType_PS5Controller:
					return Inputs.ps;
				case ESteamInputType.k_ESteamInputType_SwitchJoyConPair:
				case ESteamInputType.k_ESteamInputType_SwitchProController:
					return Inputs.ninSwitch;
				}
			}
			if (controller.hardwareName.Contains("Joy") || controller.hardwareName.Contains("Handheld") || controller.hardwareName.Contains("Pro"))
			{
				player.controllers.maps.SetMapsEnabled(state: false, controller, 0, 0);
				player.controllers.maps.SetMapsEnabled(state: true, controller, 0, 1);
				player.controllers.maps.LoadMap(ControllerType.Joystick, controller.id, 0, 1);
				return Inputs.ninSwitch;
			}
			switch (text)
			{
			case "bc043dba-df07-4135-929c-5b4398d29579":
				return Inputs.tv;
			case "cd9718bf-a87a-44bc-8716-60a0def28a9f":
			case "c3ad3cad-c7cf-4ca8-8c2e-e3df8d9960bb":
			case "c309ca09-51d6-4458-aca5-f506f5a8c1e2":
			case "71dfe6c8-9e81-428f-a58e-c7e664b7fbed":
			case "d2aef070-7caa-42ff-b2dc-daac7e4a62b4":
				return Inputs.ps;
			case "3d919cfa-468e-49f4-bce9-f6c43f2e7e62":
				if (controller.hardwareName.Contains("DUALSHOCK 4"))
				{
					return Inputs.ps;
				}
				return Inputs.xbox;
			default:
				return Inputs.xbox;
			}
		}
		MonoBehaviour.print("no joystick but a button pushed, let's assume something else");
		return Inputs.keyboard;
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			if (scKi == null)
			{
				StopCoroutine(CheckAction(suspendSlide: false));
				QuitGame();
			}
			else if (isInMenu)
			{
				if ((bool)quitBut)
				{
					if (suspendQuit)
					{
						suspendQuit = false;
						return;
					}
					PointerEventData eventData = new PointerEventData(EventSystem.current);
					ExecuteEvents.Execute(quitBut, eventData, ExecuteEvents.submitHandler);
					quitBut = null;
				}
			}
			else
			{
				OpenStats();
			}
		}
		if ((Input.GetKey(KeyCode.G) || Input.GetKey(KeyCode.R) || Input.touchCount > 4) && !touch2)
		{
			touch2 = true;
			StartCoroutine("Check2touch");
		}
		if (player == null)
		{
			if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
			{
				SwitchControl(Inputs.mouse);
			}
			else if (Input.anyKeyDown)
			{
				SwitchControl(Inputs.keyboard);
			}
			return;
		}
		switch (curInput)
		{
		case Inputs.touch:
			if (Input.touchCount == 1 && isInMenu && Input.touches[0].phase == TouchPhase.Began && !eventSys.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
			{
				JukeBox.diff.PlaySound(SFXTypes.ui_error);
			}
			break;
		case Inputs.xbox:
		case Inputs.ps:
		case Inputs.keyboard:
		case Inputs.ninSwitch:
			if (player.GetButtonUp("back") && isInMenu && (bool)quitBut)
			{
				if (suspendQuit)
				{
					suspendQuit = false;
					return;
				}
				PointerEventData eventData4 = new PointerEventData(EventSystem.current);
				ExecuteEvents.Execute(quitBut, eventData4, ExecuteEvents.submitHandler);
			}
			if (!player.GetButtonUp("second"))
			{
				break;
			}
			if (isInMenu)
			{
				if ((bool)quitBut)
				{
					if (suspendQuit)
					{
						suspendQuit = false;
						return;
					}
					PointerEventData eventData5 = new PointerEventData(EventSystem.current);
					ExecuteEvents.Execute(quitBut, eventData5, ExecuteEvents.submitHandler);
				}
			}
			else
			{
				OpenStats();
			}
			break;
		case Inputs.tv:
			if (player.GetButtonUp("inventory") && isInMenu && (bool)quitBut)
			{
				if (suspendQuit)
				{
					suspendQuit = false;
					return;
				}
				PointerEventData eventData2 = new PointerEventData(EventSystem.current);
				ExecuteEvents.Execute(quitBut, eventData2, ExecuteEvents.submitHandler);
			}
			if (!player.GetButtonUp("second"))
			{
				break;
			}
			if (isInMenu)
			{
				if ((bool)quitBut)
				{
					if (suspendQuit)
					{
						suspendQuit = false;
						return;
					}
					PointerEventData eventData3 = new PointerEventData(EventSystem.current);
					ExecuteEvents.Execute(quitBut, eventData3, ExecuteEvents.submitHandler);
				}
			}
			else
			{
				OpenStats();
			}
			break;
		}
		foreach (Inputs availableInput in availableInputs)
		{
			if (curInput == Inputs.automated && AutomationController.Instance != null && AutomationController.Instance.Active)
			{
				break;
			}
			if (curInput == Inputs.automated && AutomationController.Instance != null && !AutomationController.Instance.Active)
			{
				curInput = Inputs.none;
			}
			if (availableInput == curInput && !isSimulating)
			{
				continue;
			}
			switch (availableInput)
			{
			case Inputs.ninSwitch:
				if ((player.GetAnyButtonUp() || player.GetAxis("horizontal") > 0f || player.GetAxis("vertical") > 0f) && isPS() == Inputs.ninSwitch)
				{
					SwitchControl(Inputs.ninSwitch);
				}
				break;
			case Inputs.tv:
				if (player.GetAnyButtonUp() && isPS() == Inputs.tv)
				{
					SwitchControl(Inputs.tv);
				}
				break;
			case Inputs.xbox:
				if ((player.GetAnyButtonUp() || player.GetAxis("horizontal") > 0f || player.GetAxis("vertical") > 0f) && isPS() == Inputs.xbox)
				{
					SwitchControl(Inputs.xbox);
				}
				break;
			case Inputs.ps:
				if ((player.GetAnyButtonUp() || player.GetAxis("horizontal") > 0f || player.GetAxis("vertical") > 0f) && isPS() == Inputs.ps)
				{
					SwitchControl(Inputs.ps);
				}
				break;
			case Inputs.touch:
				if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended)
				{
					SwitchControl(Inputs.touch);
				}
				break;
			case Inputs.keyboard:
				if (player.GetAnyButtonUp() && isPS() == Inputs.keyboard)
				{
					SwitchControl(Inputs.keyboard);
				}
				break;
			case Inputs.automated:
				if (AutomationController.Instance != null && AutomationController.Instance.Active)
				{
					SwitchControl(Inputs.automated);
				}
				break;
			case Inputs.mouse:
				if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
				{
					SwitchControl(Inputs.mouse);
				}
				break;
			}
		}
	}

	private IEnumerator Check2touch()
	{
		int n = 0;
		WaitForSeconds swait = new WaitForSeconds(1f);
		while (Input.touchCount > 1 || Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.G))
		{
			yield return swait;
			n++;
			if (n == 12)
			{
				OfferReset();
				touch2 = false;
				yield break;
			}
		}
		touch2 = false;
	}

	public void OfferReset(bool all = false, string overridetxt = null)
	{
		if (!DiaPrefab.activeInHierarchy)
		{
			DiaPrefab.SetActive(value: true);
			if (all)
			{
				DiaPrefab.GetComponent<DialogAct>().Init("resetall_label", "reset", OfferResetAllValid, overridetxt);
			}
			else
			{
				DiaPrefab.GetComponent<DialogAct>().Init("reset_label", "reset", OfferResetValid, overridetxt);
			}
		}
	}

	private void OfferResetAllValid(bool doit)
	{
		if (doit)
		{
			ResetGame(all: true);
		}
	}

	private void OfferResetValid(bool doit)
	{
		if (doit)
		{
			ResetGame(all: true);
		}
	}

	public void ResetGame(bool all = false)
	{
		GameAct.diff.storeSave = null;
		DataStore.Reset(all);
		SceneManager.LoadScene("disclaimer");
	}

	public void QuitGame()
	{
		if (!DiaPrefab.activeInHierarchy)
		{
			DiaPrefab.SetActive(value: true);
			DiaPrefab.GetComponent<DialogAct>().Init("quit_label", "quit", QuitGameValid);
		}
	}

	private void QuitGameValid(bool doit)
	{
		if (doit)
		{
			StartCoroutine("DoQuit");
		}
	}

	private IEnumerator DoQuit()
	{
		yield return null;
		Application.Quit();
	}

	private IEnumerator AutomationInputCoroutine()
	{
		yield return AutomationController.Instance.UpdateCoroutine();
	}

	public void SwitchControl(Inputs input)
	{
		StopCoroutine("AutomationInputCoroutine");
		if (isSimulating)
		{
			StopSimulation();
			StopCoroutine("CheckSlide");
			StartCoroutine("CheckSlide");
			return;
		}
		Util.Write("Inputs > Switch controls to: " + input);
		curInput = input;
		Cursor.visible = curInput == Inputs.mouse;
		GraphicRaycaster component = Canvas.GetComponent<GraphicRaycaster>();
		switch (curInput)
		{
		case Inputs.automated:
			if (AutomationController.Instance != null)
			{
				AutomationController.Instance.ResetInteractionState();
				StartCoroutine("AutomationInputCoroutine");
			}
			break;
		case Inputs.xbox:
		case Inputs.ps:
		case Inputs.keyboard:
		case Inputs.tv:
		case Inputs.ninSwitch:
			component.enabled = false;
			eventSys.sendNavigationEvents = true;
			GameAct.diff?.ActivateFirstSelection();
			if (isInventory || isInMenu)
			{
				AutoSelectMe[] array = UnityEngine.Object.FindObjectsOfType<AutoSelectMe>();
				for (int i = 0; i < array.Length && !array[i].Activate(ison: true, isnav: true); i++)
				{
				}
			}
			break;
		case Inputs.mouse:
		case Inputs.touch:
			component.enabled = true;
			eventSys.sendNavigationEvents = false;
			break;
		}
		if (curInput != Inputs.none)
		{
			PlayerPrefs.SetInt("input_keep", (int)curInput);
		}
		if ((bool)AnimBut.diff)
		{
			AnimBut.diff.ResetLock(withlastmode: false);
		}
		if (OnSlideUpdate != null && !isInMenu)
		{
			StopCoroutine("CheckSlide");
			StartCoroutine("CheckSlide");
		}
		if (OnSwitchControl != null)
		{
			OnSwitchControl(input);
		}
	}

	public void SuspendSlideFocus()
	{
		isSuspended = true;
		slideSign = 1f;
	}

	public void RestoreSlideFocus()
	{
		isSuspended = false;
		slideSign = 1f;
	}

	public void RemoveSlideFocus()
	{
		if (OnSlideStop != null)
		{
			OnSlideStop();
		}
		OnSlideUpdate = null;
		OnSlideStop = null;
		OnSlideStart = null;
		OnSlideValid = null;
		OnSlideDown = null;
	}

	public bool GetSlideFocus(Action<Vector2> update, Action stop, Action<Vector2> start, Action<Vector2> valid, Func<bool, bool> down, bool allowCumul = false, float min = 0f, float max = 1f)
	{
		RemoveSlideFocus();
		OnSlideUpdate = update;
		OnSlideStop = stop;
		OnSlideStart = start;
		OnSlideValid = valid;
		OnSlideDown = down;
		yMax = max;
		yMin = min;
		RestoreSlideFocus();
		StopCoroutine("CheckSlide");
		StartCoroutine("CheckSlide");
		nodeadzone = allowCumul;
		if (allowCumul && curInput != Inputs.touch && curInput != Inputs.ps && curInput != Inputs.xbox && curInput != Inputs.tv)
		{
			return true;
		}
		return false;
	}

	private IEnumerator CheckSlide()
	{
		float side = 0f;
		Vector2 decal = Vector2.zero;
		WaitForSeconds swait = new WaitForSeconds(0.4f);
		while (true)
		{
			if (isSuspended)
			{
				yield return null;
				continue;
			}
			while (!CheckDown())
			{
				yield return null;
			}
			float tdown = Time.realtimeSinceStartup;
			if (curInput == Inputs.touch)
			{
				decal = Vector2.zero;
			}
			bool isActive = false;
			Vector2 lori = Vector2.zero;
			while (!CheckUp(isActive, decal, tdown))
			{
				lori = ((curInput == Inputs.keyboard) ? Vector2.Lerp(lori, ori, Time.deltaTime * 4f) : ori);
				decal = GetSlidePos() - lori;
				decal.x = ((nodeadzone || curInput == Inputs.tv) ? Mathf.Clamp(decal.x, -1f, 1f) : ((decal.x < 0f) ? Mathf.Clamp(decal.x + stopamo, -1f, 0f) : Mathf.Clamp(decal.x - stopamo, 0f, 1f)));
				if (CheckBounds(decal))
				{
					float num = Mathf.Sign(decal.x);
					if (!isActive)
					{
						isActive = true;
						side = 0f - num;
					}
					if (isActive && side != num)
					{
						side = num;
						if (OnSlideStart != null && OnSlideStart != null)
						{
							OnSlideStart(decal);
						}
					}
					if (curInput != Inputs.touch && OnSlideUpdate != null)
					{
						OnSlideUpdate(decal);
					}
				}
				else if (isActive)
				{
					if (OnSlideStop != null && OnSlideStop != null)
					{
						OnSlideStop();
					}
					isActive = false;
				}
				if (curInput == Inputs.touch && OnSlideUpdate != null)
				{
					OnSlideUpdate(decal);
				}
				yield return null;
			}
			yield return swait;
		}
	}

	private bool CheckDown()
	{
		if (isSimulating)
		{
			ori = new Vector2(0f, 0f);
			return true;
		}
		if (OnSlideDown != null && !OnSlideDown(arg: true))
		{
			return false;
		}
		ori = Vector2.zero;
		switch (curInput)
		{
		case Inputs.touch:
			if (Input.touchCount == 0)
			{
				return false;
			}
			ori = new Vector2(slideSign * Input.touches[0].position.x * 3f, Input.touches[0].position.y * 0.3f + 0.5f) / Screen.height;
			return true;
		case Inputs.mouse:
			ori = new Vector2(slideSign * 0.5f, 0.05f);
			return true;
		case Inputs.automated:
			ori = new Vector2(0.5f, 0.05f);
			return true;
		case Inputs.keyboard:
			if (Input.GetKeyUp(KeyCode.LeftArrow))
			{
				ori = new Vector2(slideSign * 0.4f, 0f);
				return true;
			}
			if (Input.GetKeyUp(KeyCode.RightArrow))
			{
				ori = new Vector2(slideSign * -0.4f, 0f);
				return true;
			}
			return false;
		default:
			return true;
		}
	}

	private Vector2 GetSlidePos()
	{
		if (isSimulating)
		{
			float b = Mathf.Lerp(0f, 5f * (Mathf.PingPong(Time.realtimeSinceStartup * 1.4f, 2f) - 1f), Time.deltaTime * 6f);
			simulationPos = Mathf.Lerp(simulationPos, b, Time.deltaTime * 2f);
			return new Vector2(slideSign * simulationPos, 0f);
		}
		switch (curInput)
		{
		case Inputs.touch:
			if (Input.touchCount == 0)
			{
				return new Vector2(0f, 0f);
			}
			return new Vector2(slideSign * Input.touches[0].position.x * 3f, Input.touches[0].position.y * 0.3f + 0.5f) / Screen.height;
		case Inputs.mouse:
			return new Vector2(slideSign * Input.mousePosition.x, Input.mousePosition.y * 0.1f + 0.05f) / Screen.width;
		case Inputs.automated:
			if (AutomationController.Instance != null)
			{
				if (AutomationController.Instance.AutoSlide && AutomationController.Instance.SlideLeft)
				{
					return new Vector2(0.2f, 0f);
				}
				if (AutomationController.Instance.AutoSlide && AutomationController.Instance.SlideRight)
				{
					return new Vector2(0.8f, 0f);
				}
			}
			return new Vector2(0.5f, 0f);
		case Inputs.keyboard:
			if (Input.GetKeyUp(KeyCode.LeftArrow))
			{
				ori = new Vector2(slideSign * 0.4f, 0f);
			}
			else if (Input.GetKeyUp(KeyCode.RightArrow))
			{
				ori = new Vector2(slideSign * -0.4f, 0f);
			}
			return Vector2.zero;
		case Inputs.xbox:
		case Inputs.ps:
		case Inputs.ninSwitch:
			return new Vector2(slideSign * player.GetAxis("horizontal") * 0.5f, player.GetAxis("vertical") * 0.1f);
		case Inputs.tv:
			return new Vector2(slideSign * player.GetAxis("horizontal") * 0.5f, player.GetAxis("vertical") * 0.1f);
		default:
			return Vector2.zero;
		}
	}

	private Vector2 GetPointerPos()
	{
		switch (curInput)
		{
		case Inputs.touch:
			if (Input.touchCount > 0)
			{
				return new Vector2(Input.touches[0].position.x, Input.touches[0].position.y);
			}
			return new Vector2(0f, 0f);
		case Inputs.mouse:
			return new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		default:
			return new Vector2(0f, 0f);
		}
	}

	public Vector2 GetPointerVirt(bool noagro = false)
	{
		switch (curInput)
		{
		case Inputs.automated:
			return Vector2.zero;
		case Inputs.keyboard:
		{
			Vector2 result = (Input.GetKey(KeyCode.LeftArrow) ? Vector2.left : (Input.GetKey(KeyCode.RightArrow) ? Vector2.right : Vector2.zero));
			if (noagro)
			{
				return result;
			}
			interpos = Vector2.Lerp(interpos, new Vector2(Mathf.Clamp(result.x * 0.3f * Mathf.Abs(result.x * result.x), -1f, 1f), Mathf.Clamp(result.y * 0.3f * Mathf.Abs(result.y * result.y), -1f, 1f)), Time.deltaTime * 3f);
			return interpos;
		}
		case Inputs.xbox:
		case Inputs.ps:
		case Inputs.tv:
		case Inputs.ninSwitch:
		{
			if (noagro)
			{
				return new Vector2(player.GetAxis("horizontal"), player.GetAxis("vertical"));
			}
			Vector2 vector = new Vector2(player.GetAxis("horizontal"), player.GetAxis("vertical"));
			interpos = Vector2.Lerp(interpos, new Vector2(Mathf.Clamp(vector.x * 0.3f * Mathf.Abs(vector.x * vector.x), -1f, 1f), Mathf.Clamp(vector.y * 0.3f * Mathf.Abs(vector.y * vector.y), -1f, 1f)), Time.deltaTime * 3f);
			return interpos;
		}
		default:
		{
			Vector2 pointerPos = GetPointerPos();
			return new Vector2((pointerPos.x - (float)(Screen.width / 2)) / (float)Screen.width, (pointerPos.y - (float)(Screen.height / 2)) / (float)Screen.height);
		}
		}
	}

	private bool CheckUp(bool active, Vector2 decal, float tdown)
	{
		if (isSuspended)
		{
			return true;
		}
		if (isSimulating && curInput != Inputs.automated)
		{
			return false;
		}
		if (isActionFocus)
		{
			return false;
		}
		switch (curInput)
		{
		case Inputs.touch:
			if (Input.touchCount > 0)
			{
				return false;
			}
			break;
		case Inputs.mouse:
			if (!Input.GetMouseButtonUp(0))
			{
				return false;
			}
			break;
		case Inputs.automated:
			if (AutomationController.Instance != null && AutomationController.Instance.AutoSlide && AutomationController.Instance != null && AutomationController.Instance.DebugFramecount++ % 30 != 0)
			{
				return false;
			}
			break;
		case Inputs.keyboard:
			if ((!Input.GetKeyDown(KeyCode.LeftArrow) || !(decal.x < 0f)) && (!Input.GetKeyDown(KeyCode.RightArrow) || !(decal.x > 0f)) && (!Input.GetKeyDown(KeyCode.Return) || decal.Equals(0)))
			{
				return false;
			}
			break;
		case Inputs.xbox:
		case Inputs.ps:
		case Inputs.ninSwitch:
			if (!player.GetButtonUp("select"))
			{
				return false;
			}
			break;
		case Inputs.tv:
			if (!player.GetAxis("vertical").Equals(0f) || !player.GetAxis("horizontal").Equals(0f) || Time.realtimeSinceStartup - tdown < 0.2f)
			{
				return false;
			}
			break;
		}
		if (active && Time.realtimeSinceStartup - tdown > 0.05f)
		{
			OnSlideValid(decal);
		}
		else if (OnSlideStop != null)
		{
			OnSlideStop();
		}
		return true;
	}

	private bool CheckBounds(Vector2 p)
	{
		if (isInMenu || isInventory)
		{
			return false;
		}
		float x = p.x;
		float num = 0f;
		switch (curInput)
		{
		case Inputs.touch:
			if (!isSimulating && Input.touchCount == 0)
			{
				return false;
			}
			break;
		case Inputs.mouse:
			if (!isSimulating)
			{
				num = Input.mousePosition.y / (float)Screen.height;
				if (num > yMax || num < yMin)
				{
					return false;
				}
			}
			break;
		}
		float num2 = Mathf.Abs(x);
		if (nodeadzone || num2 > stopamo * 2f)
		{
			return true;
		}
		return false;
	}

	public void GetActionFocus(Func<bool, bool> action, bool suspendSlide = false, Action down = null, bool tapaction = false)
	{
		CancelTapAction();
		OnAction = action;
		OnActionDown = down;
		hasTapAction = tapaction;
		StopCoroutine("CheckAction");
		StartCoroutine("CheckAction", suspendSlide);
	}

	public void TapAction()
	{
		StopCheckAction();
		CancelTapAction();
	}

	public void CancelTapAction()
	{
		hasClick = true;
		hasTapAction = false;
		isActionFocus = false;
		OnAction = null;
		OnActionDown = null;
		StopCoroutine("CheckAction");
	}

	private bool StopCheckAction()
	{
		if (isInMenu || isInventory)
		{
			return false;
		}
		if ((bool)AnimBut.diff)
		{
			AnimBut.diff.Tap();
		}
		if (OnAction != null && OnAction(arg: true))
		{
			CancelTapAction();
			return true;
		}
		return false;
	}

	public IEnumerator CheckAction(bool suspendSlide)
	{
		if (suspendSlide)
		{
			isActionFocus = true;
		}
		hasClick = false;
		while (true)
		{
			switch (curInput)
			{
			case Inputs.touch:
				if (Input.touchCount == 1 && OnActionDown != null && Input.touches[0].phase == TouchPhase.Began)
				{
					OnActionDown();
				}
				if ((hasTapAction && Input.touchCount == 1 && Input.touches[0].phase == TouchPhase.Ended && StopCheckAction()) || hasClick)
				{
					yield break;
				}
				break;
			case Inputs.mouse:
				if (Input.GetMouseButtonDown(0) && OnActionDown != null)
				{
					OnActionDown();
				}
				if ((hasTapAction && Input.GetMouseButtonUp(0) && StopCheckAction()) || hasClick)
				{
					yield break;
				}
				break;
			case Inputs.automated:
				if (AutomationController.Instance != null && AutomationController.Instance.InteractionState.CanValidateMap && StopCheckAction())
				{
					yield break;
				}
				if (AutomationController.Instance.InteractionState.CanValidateAction)
				{
					if (OnActionDown != null)
					{
						OnActionDown();
					}
					yield return null;
					if ((hasTapAction && StopCheckAction()) || hasClick)
					{
						yield break;
					}
				}
				break;
			case Inputs.keyboard:
				if (Input.GetKeyDown(KeyCode.Return) && StopCheckAction())
				{
					yield break;
				}
				break;
			case Inputs.xbox:
			case Inputs.ps:
			case Inputs.tv:
			case Inputs.ninSwitch:
				if (player.GetButtonUp("select") && StopCheckAction())
				{
					yield break;
				}
				break;
			}
			yield return null;
		}
	}

	private void StopSimulation()
	{
		isSimulating = false;
		if ((bool)AnimBut.diff)
		{
			AnimBut.diff.Lock();
		}
	}

	public void Simulate(float min, float max)
	{
		simulationPos = 0f;
		isSimulating = true;
		StopCoroutine("CheckSlide");
		StopCoroutine("CheckSlide");
		StartCoroutine("CheckSlide");
		if ((bool)AnimBut.diff)
		{
			AnimBut.diff.UnLock(ControlModes.next);
		}
	}

	private IEnumerator YieldSimulBut()
	{
		yield return new WaitForSeconds(2f);
		if ((bool)AnimBut.diff)
		{
			AnimBut.diff.UnLock(ControlModes.next);
		}
	}
}
