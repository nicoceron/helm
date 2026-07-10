using System;
using UnityEngine;

public class iOSHapticFeedback : MonoBehaviour
{
	[Serializable]
	public class iOSFeedbackTypeSettings
	{
		public bool SelectionChange = true;

		public bool ImpactLight = true;

		public bool ImpactMedium = true;

		public bool ImpactHeavy = true;

		public bool NotificationSuccess = true;

		public bool NotificationWarning = true;

		public bool NotificationFailure = true;

		public bool Notifications
		{
			get
			{
				if (!NotificationSuccess && !NotificationWarning)
				{
					return NotificationFailure;
				}
				return true;
			}
		}
	}

	public enum iOSFeedbackType
	{
		SelectionChange = 0,
		ImpactLight = 1,
		ImpactMedium = 2,
		ImpactHeavy = 3,
		Success = 4,
		Warning = 5,
		Failure = 6,
		None = 7
	}

	private static iOSHapticFeedback _instance;

	public iOSFeedbackTypeSettings usedFeedbackTypes = new iOSFeedbackTypeSettings();

	private bool feedbackGeneratorsSetUp;

	public bool debug = true;

	private bool _isEnabled = true;

	public static iOSHapticFeedback Instance
	{
		get
		{
			if (!_instance)
			{
				Debug.LogWarning("No iOS Haptic Feedback instance available. Creating one.");
				_instance = new GameObject("iOS Haptic Feedback").AddComponent<iOSHapticFeedback>();
			}
			return _instance;
		}
	}

	public bool IsEnabled
	{
		get
		{
			return _isEnabled;
		}
		set
		{
			_isEnabled = value;
			if (debug)
			{
				Debug.Log("iOSHapticFeedback globally enabled: " + value);
			}
		}
	}

	protected virtual void Awake()
	{
		if ((bool)_instance)
		{
			Debug.LogWarning("There is already an instance of iOSHapticFeedback.");
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		_instance = this;
		for (int i = 0; i < 5; i++)
		{
			if (FeedbackIdSet(i))
			{
				InstantiateFeedbackGenerator(i);
			}
		}
		feedbackGeneratorsSetUp = true;
	}

	protected void OnDestroy()
	{
		if (!feedbackGeneratorsSetUp)
		{
			return;
		}
		for (int i = 0; i < 5; i++)
		{
			if (FeedbackIdSet(i))
			{
				ReleaseFeedbackGenerator(i);
			}
		}
	}

	protected bool FeedbackIdSet(int id)
	{
		if ((id != 0 || !usedFeedbackTypes.SelectionChange) && (id != 1 || !usedFeedbackTypes.ImpactLight) && (id != 2 || !usedFeedbackTypes.ImpactMedium) && (id != 3 || !usedFeedbackTypes.ImpactHeavy))
		{
			if (id == 4 || id == 5 || id == 6)
			{
				return usedFeedbackTypes.Notifications;
			}
			return false;
		}
		return true;
	}

	private void _instantiateFeedbackGenerator(int id)
	{
	}

	private void _prepareFeedbackGenerator(int id)
	{
	}

	private void _triggerFeedbackGenerator(int id, bool advanced)
	{
	}

	private void _releaseFeedbackGenerator(int id)
	{
	}

	protected void InstantiateFeedbackGenerator(int id)
	{
		if (debug)
		{
			iOSFeedbackType iOSFeedbackType2 = (iOSFeedbackType)id;
			Debug.Log("Instantiate iOS feedback generator " + iOSFeedbackType2);
		}
		_instantiateFeedbackGenerator(id);
	}

	protected void PrepareFeedbackGenerator(int id)
	{
		if (debug)
		{
			iOSFeedbackType iOSFeedbackType2 = (iOSFeedbackType)id;
			Debug.Log("Prepare iOS feedback generator " + iOSFeedbackType2);
		}
		_prepareFeedbackGenerator(id);
	}

	protected void TriggerFeedbackGenerator(int id, bool advanced)
	{
		if (debug)
		{
			iOSFeedbackType iOSFeedbackType2 = (iOSFeedbackType)id;
			Debug.Log("Trigger iOS feedback generator " + iOSFeedbackType2.ToString() + ", advanced mode: " + advanced);
		}
		_triggerFeedbackGenerator(id, advanced);
	}

	protected void ReleaseFeedbackGenerator(int id)
	{
		if (debug)
		{
			iOSFeedbackType iOSFeedbackType2 = (iOSFeedbackType)id;
			Debug.Log("Release iOS feedback generator " + iOSFeedbackType2);
		}
		_releaseFeedbackGenerator(id);
	}

	public virtual void Trigger(iOSFeedbackType feedbackType)
	{
		if (_isEnabled)
		{
			if (FeedbackIdSet((int)feedbackType))
			{
				TriggerFeedbackGenerator((int)feedbackType, advanced: false);
			}
			else
			{
				Debug.LogError("You cannot trigger a feedback generator without instantiating it first");
			}
		}
		else if (debug)
		{
			Debug.Log("Haptic Feedback not triggered because the property 'IsEnabled' of the iOSHapticFeedback component has beeen disabled.");
		}
	}

	public bool IsSupported()
	{
		return false;
	}
}
