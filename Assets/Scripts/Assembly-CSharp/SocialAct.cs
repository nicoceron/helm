using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class SocialAct : MonoBehaviour
{
	public static SocialAct diff;

	private bool hasStatsPending;

	private string iosBoardId = "longest_travel";

	private List<string> seenAchieve = new List<string>();

	public bool socialReadyOrNot;

	private bool authenticated;

	private AppId_t appId = new AppId_t(0u);

	private bool steamStatsRequested;

	private bool steamStatsAndAchievementsReady;

	protected Callback<UserStatsReceived_t> steamUserStatsRecievedCallback;

	protected Callback<UserStatsStored_t> steamUserStatsStoredCallback;

	private bool hasNotification;

	private void OnHideUnity(bool isGameShown)
	{
		if (!isGameShown)
		{
			Time.timeScale = 0f;
		}
		else
		{
			Time.timeScale = 1f;
		}
	}

	private void Awake()
	{
		if (diff != null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		diff = this;
		Object.DontDestroyOnLoad(base.gameObject);
		steamStatsRequested = false;
		if (SteamManager.Initialized)
		{
			steamUserStatsRecievedCallback = Callback<UserStatsReceived_t>.Create(OnSteamUserStatsReceived);
			steamUserStatsStoredCallback = Callback<UserStatsStored_t>.Create(OnSteamUserStatsStored);
		}
		else
		{
			PlayerPrefs.SetInt("nosocial", 1);
			socialReadyOrNot = true;
		}
	}

	private void Start()
	{
		if (!SteamManager.Initialized)
		{
			PlayerPrefs.SetInt("nosocial", 1);
			socialReadyOrNot = true;
			return;
		}
		if (!PlayerPrefs.HasKey("nosocial") && !socialReadyOrNot)
		{
			socialReadyOrNot = false;
			appId = SteamUtils.GetAppID();
			if (!steamStatsRequested)
			{
				bool flag = SteamUserStats.RequestCurrentStats();
				steamStatsRequested = flag;
			}
		}
	}

	public void SocialAuthent()
	{
		if (PlayerPrefs.HasKey("nosocial") || !SteamManager.Initialized)
		{
			socialReadyOrNot = true;
			return;
		}
		Social.localUser.Authenticate(SocialAuthent);
	}

	private void SocialAuthent(bool success)
	{
		if (success)
		{
			authenticated = true;
			Social.LoadAchievements(ProcessLoadedAchievements);
		}
		else
		{
			PlayerPrefs.SetInt("nosocial", 1);
		}
		socialReadyOrNot = true;
	}

	private void ProcessLoadedAchievements(IAchievement[] achievements)
	{
	}

	public void AddAchieve(string id)
	{
		if (seenAchieve.Contains(id))
		{
			return;
		}
		seenAchieve.Add(id);
		if (SteamManager.Initialized && steamStatsAndAchievementsReady)
		{
			if (SteamUserStats.GetAchievement(id, out var pbAchieved))
			{
				if (!pbAchieved)
				{
					pbAchieved = true;
					if (!SteamUserStats.SetAchievement(id))
					{
						Debug.LogError("Achievement for " + id + " not set.");
					}
					else
					{
						SteamUserStats.StoreStats();
					}
				}
				else
				{
					Debug.LogError(id + " already granted");
				}
			}
			else
			{
				Debug.LogError("GetAchievement call Failed");
			}
		}
		else
		{
			Debug.LogError($"Steam not initialized ({SteamManager.Initialized}), or not ready ({steamStatsAndAchievementsReady})");
		}
	}

	public void SetScore(int years)
	{
		if (!Social.localUser.authenticated)
		{
			return;
		}
		string pchName = iosBoardId;
		if (!steamStatsAndAchievementsReady)
		{
			if (SteamManager.Initialized && !steamStatsRequested)
			{
				bool flag = SteamUserStats.RequestCurrentStats();
				steamStatsRequested = flag;
			}
		}
		else if (!SteamUserStats.SetStat(pchName, years))
		{
			Debug.LogError("Steam set Stat failed for lightyear score");
		}
		else
		{
			SteamUserStats.StoreStats();
		}
	}

	public void OpenLeaderBoard()
	{
		Social.ShowLeaderboardUI();
	}

	public void OpenAchievements()
	{
		Social.ShowAchievementsUI();
	}

	public int GetNotificationCard()
	{
		return 0;
	}

	public void RequestAuthorization()
	{
	}

	public void OnSteamUserStatsReceived(UserStatsReceived_t pCallback)
	{
		if (!SteamManager.Initialized)
		{
			Debug.Log("SteamManager not init when getting OnSteamUserStatsRecieved callback");
		}
		else if (appId.m_AppId == pCallback.m_nGameID && EResult.k_EResultOK == pCallback.m_eResult)
		{
			Debug.Log("Received stats and achievements from Steam\n");
			steamStatsAndAchievementsReady = true;
		}
	}

	private void OnSteamUserStatsStored(UserStatsStored_t pCallback)
	{
		if (appId.m_AppId == pCallback.m_nGameID)
		{
			if (EResult.k_EResultOK == pCallback.m_eResult)
			{
				Debug.Log("StoreStats - success");
			}
			else
			{
				Debug.Log("StoreStats - failed, " + pCallback.m_eResult);
			}
		}
	}
}
