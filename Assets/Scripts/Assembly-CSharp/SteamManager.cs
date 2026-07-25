using System;
using System.Text;
using AOT;
using Steamworks;
using UnityEngine;

[DisallowMultipleComponent]
public class SteamManager : MonoBehaviour
{
	protected static bool s_EverInitialized;

	protected static SteamManager s_instance;

	protected bool m_bInitialized;

	protected bool m_bSteamInputInitialized;

	protected Callback<SteamInputDeviceConnected_t> m_SteamInputDeviceConnected;

	protected Callback<SteamInputDeviceDisconnected_t> m_SteamInputDeviceDisconnected;

	protected Callback<SteamInputGamepadSlotChange_t> m_SteamInputGamepadSlotChange;

	protected Callback<SteamInputActionEvent_t> m_SteamInputActionEvent;

	protected SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;

	public InputHandle_t[] controllerHandles = new InputHandle_t[16];

	public static SteamManager Instance
	{
		get
		{
			if (s_instance == null)
			{
				return new GameObject("SteamManager").AddComponent<SteamManager>();
			}
			return s_instance;
		}
	}

	public static bool Initialized => Instance.m_bInitialized;

	[MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
	protected static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
	{
		Debug.LogWarning(pchDebugText);
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void InitOnPlayMode()
	{
		s_EverInitialized = false;
		s_instance = null;
	}

	protected virtual void Awake()
	{
		if (s_instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		s_instance = this;
#if UNITY_WEBGL && !UNITY_EDITOR
		DisableSteam();
		return;
#endif
		if (s_EverInitialized)
		{
			throw new Exception("Tried to Initialize the SteamAPI twice in one session!");
		}
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		try
		{
			if (!Packsize.Test())
			{
				Debug.LogWarning("[Steamworks.NET] Packsize test failed; continuing without Steam services.", this);
				DisableSteam();
				return;
			}
			if (!DllCheck.Test())
			{
				Debug.LogWarning("[Steamworks.NET] DLL check failed; continuing without Steam services.", this);
				DisableSteam();
				return;
			}
			if (SteamAPI.RestartAppIfNecessary(new AppId_t(1663400u)))
			{
				Debug.LogWarning("[Steamworks.NET] Steam requested a relaunch; standalone editor build will continue without Steam.");
				DisableSteam();
				return;
			}
			m_bInitialized = SteamAPI.Init();
			m_bSteamInputInitialized = m_bInitialized && SteamInput.Init(bExplicitlyCallRunFrame: false);
		}
		catch (Exception ex) when (ex is DllNotFoundException || ex is EntryPointNotFoundException || ex is BadImageFormatException)
		{
			Debug.LogWarning("[Steamworks.NET] Native Steam library is unavailable; continuing in standalone mode.\n" + ex.Message, this);
			DisableSteam();
			return;
		}
		if (!m_bInitialized)
		{
			Debug.LogWarning("[Steamworks.NET] SteamAPI_Init() failed; continuing in standalone mode.", this);
			DisableSteam();
			return;
		}
		m_SteamInputDeviceConnected = Callback<SteamInputDeviceConnected_t>.Create(OnSteamInputDeviceConnected);
		m_SteamInputDeviceDisconnected = Callback<SteamInputDeviceDisconnected_t>.Create(OnSteamInputDeviceDisconnected);
		m_SteamInputGamepadSlotChange = Callback<SteamInputGamepadSlotChange_t>.Create(OnSteamInputGamepadSlotChangeCallback);
		SteamInput.EnableDeviceCallbacks();
		if (!m_bSteamInputInitialized)
		{
			Debug.LogError("[Steamworks.NET] SteamInput_Init() failed.", this);
		}
		else
		{
			s_EverInitialized = true;
		}
	}

	private void DisableSteam()
	{
		m_bInitialized = false;
		m_bSteamInputInitialized = false;
		PlayerPrefs.SetInt("nosocial", 1);
	}

	private void OnSteamInputDeviceConnected(SteamInputDeviceConnected_t pCallback)
	{
		Debug.Log($"[{2801} - SteamInputDeviceConnected] - {pCallback.m_ulConnectedDeviceHandle}, of type: {SteamInput.GetInputTypeForHandle(pCallback.m_ulConnectedDeviceHandle)}" + $", with index: {SteamInput.GetGamepadIndexForController(pCallback.m_ulConnectedDeviceHandle)}");
	}

	private void OnSteamInputDeviceDisconnected(SteamInputDeviceDisconnected_t pCallback)
	{
		string text = 2802.ToString();
		InputHandle_t ulDisconnectedDeviceHandle = pCallback.m_ulDisconnectedDeviceHandle;
		Debug.Log("[" + text + " - SteamInputDeviceDisconnected] - " + ulDisconnectedDeviceHandle.ToString());
	}

	private void OnSteamInputGamepadSlotChangeCallback(SteamInputGamepadSlotChange_t pCallback)
	{
		Debug.LogError($"Slot has changed for: {pCallback.m_ulDeviceHandle} handle. {pCallback.m_eDeviceType} device type. New Slot: {pCallback.m_nNewGamepadSlot}, Old Slot: {pCallback.m_nOldGamepadSlot}");
	}

	protected virtual void OnEnable()
	{
		if (s_instance == null)
		{
			s_instance = this;
		}
		if (m_bInitialized && m_SteamAPIWarningMessageHook == null)
		{
			m_SteamAPIWarningMessageHook = SteamAPIDebugTextHook;
			SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
		}
	}

	protected virtual void OnDestroy()
	{
		if (s_instance != this)
		{
			return;
		}
		s_instance = null;
		if (m_bInitialized)
		{
			SteamAPI.Shutdown();
			if (m_bSteamInputInitialized)
			{
				SteamInput.Shutdown();
			}
		}
	}

	protected virtual void Update()
	{
		if (m_bInitialized)
		{
			SteamAPI.RunCallbacks();
		}
	}

	public void GetSteamInputControllerData()
	{
		if (m_bSteamInputInitialized && Initialized)
		{
			try
			{
				int connectedControllers = SteamInput.GetConnectedControllers(controllerHandles);
				if (connectedControllers > 0)
				{
					Debug.LogError($"Steamworks Controller Count > 0: {connectedControllers}");
					for (int i = 0; i < connectedControllers; i++)
					{
						if (controllerHandles[i].m_InputHandle != 0L)
						{
							Debug.LogError($"Steamworks controller [{i}] is non zero, so get type");
							switch (SteamInput.GetInputTypeForHandle(controllerHandles[i]))
							{
							case ESteamInputType.k_ESteamInputType_Unknown:
								Debug.LogError("unknown!\n");
								break;
							case ESteamInputType.k_ESteamInputType_SteamController:
								Debug.LogError("Steam controller!\n");
								break;
							case ESteamInputType.k_ESteamInputType_XBox360Controller:
								Debug.LogError("XBox 360 controller!\n");
								break;
							case ESteamInputType.k_ESteamInputType_XBoxOneController:
								Debug.LogError("XBox One controller!\n");
								break;
							case ESteamInputType.k_ESteamInputType_GenericGamepad:
								Debug.LogError("Generic XInput!\n");
								break;
							case ESteamInputType.k_ESteamInputType_PS4Controller:
								Debug.LogError("PS4 controller!\n");
								break;
							}
						}
					}
				}
				else
				{
					Debug.LogError("Steamworks Controller Count is or less than 0");
				}
				return;
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.ToString());
				return;
			}
		}
		Debug.LogError($"Steamworks ({Initialized}) and/or SteamInput ({m_bSteamInputInitialized}) not init on Controller data get");
	}
}
