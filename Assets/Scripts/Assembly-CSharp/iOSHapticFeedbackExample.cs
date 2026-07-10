using UnityEngine;

public class iOSHapticFeedbackExample : MonoBehaviour
{
	private bool supported;

	private void Start()
	{
		supported = iOSHapticFeedback.Instance.IsSupported();
		if (supported)
		{
			Debug.Log("iOS Haptic Feedback supported");
		}
		else
		{
			Debug.Log("Your device does not support iOS haptic feedback");
		}
	}

	private void OnGUI()
	{
		if (supported)
		{
			GUI.Label(new Rect(0f, 0f, 300f, 50f), "Your device supports haptic feedback.");
		}
		else
		{
			GUI.Label(new Rect(0f, 0f, 300f, 50f), "Your device does not support haptic feedback.");
		}
		for (int i = 0; i < 7; i++)
		{
			Rect position = new Rect(0f, 70 + i * 60, 300f, 50f);
			iOSHapticFeedback.iOSFeedbackType iOSFeedbackType = (iOSHapticFeedback.iOSFeedbackType)i;
			if (GUI.Button(position, "Trigger " + iOSFeedbackType))
			{
				iOSHapticFeedback.Instance.Trigger((iOSHapticFeedback.iOSFeedbackType)i);
			}
		}
		if (GUI.Button(new Rect(0f, 490f, 300f, 50f), "Globally enabled: " + iOSHapticFeedback.Instance.IsEnabled))
		{
			iOSHapticFeedback.Instance.IsEnabled = !iOSHapticFeedback.Instance.IsEnabled;
		}
	}
}
