using System.Collections.Generic;
using Rewired;

public class HapticInterface
{
	public enum HapticDuration
	{
		Tap = 0,
		Short = 1,
		Medium = 2,
		Long = 3
	}

	public static Dictionary<iOSHapticFeedback.iOSFeedbackType, float> HapticValueMap = new Dictionary<iOSHapticFeedback.iOSFeedbackType, float>
	{
		{
			iOSHapticFeedback.iOSFeedbackType.SelectionChange,
			0.2f
		},
		{
			iOSHapticFeedback.iOSFeedbackType.ImpactLight,
			0.4f
		},
		{
			iOSHapticFeedback.iOSFeedbackType.ImpactMedium,
			0.6f
		},
		{
			iOSHapticFeedback.iOSFeedbackType.ImpactHeavy,
			0.8f
		},
		{
			iOSHapticFeedback.iOSFeedbackType.Success,
			1f
		},
		{
			iOSHapticFeedback.iOSFeedbackType.Warning,
			1f
		},
		{
			iOSHapticFeedback.iOSFeedbackType.Failure,
			1f
		},
		{
			iOSHapticFeedback.iOSFeedbackType.None,
			0f
		}
	};

	public static Dictionary<HapticDuration, float> HapticDurationMap = new Dictionary<HapticDuration, float>
	{
		{
			HapticDuration.Tap,
			0.05f
		},
		{
			HapticDuration.Short,
			0.15f
		},
		{
			HapticDuration.Medium,
			0.5f
		},
		{
			HapticDuration.Long,
			1f
		}
	};

	public static void InitializeInterface()
	{
	}

	public static void Trigger(iOSHapticFeedback.iOSFeedbackType typeToTrigger, HapticDuration duration)
	{
		if (InputAct.diff.curInput == Inputs.mouse || InputAct.diff.curInput == Inputs.keyboard || InputAct.diff.curInput == Inputs.none || InputAct.diff.curInput == Inputs.automated)
		{
			return;
		}
		float num = (HapticValueMap.TryGetValue(typeToTrigger, out num) ? num : 0f);
		if (num > 0f)
		{
			float num2 = (HapticDurationMap.TryGetValue(duration, out num2) ? num2 : 0f);
			if (num2 > 0f && ReInput.players.GetPlayer(0).controllers.joystickCount > 0)
			{
				ReInput.players.GetPlayer(0).SetVibration(0, num, num2);
				ReInput.players.GetPlayer(0).SetVibration(1, num, num2);
			}
		}
	}
}
