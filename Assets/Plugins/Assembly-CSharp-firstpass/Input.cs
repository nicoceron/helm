using System;
using Rewired;
using UnityEngine;

public static class Input
{
	private static bool rewiredError;

	private static Player player
	{
		get
		{
			if (!UnityInputOverride.enabled)
			{
				return null;
			}
			if (!ReInput.isReady)
			{
				if (!rewiredError)
				{
					rewiredError = true;
					Debug.LogWarning("Rewired: Error overriding Unity input. Rewired is not initialized! Do you have a Rewired Input Manager in the scene? Input calls will be handled by UnityEngine.Input.");
				}
				return null;
			}
			if (ReInput.players.playerCount <= 0)
			{
				if (!rewiredError)
				{
					rewiredError = true;
					Debug.LogWarning("Rewired: Error overriding Unity input. There are no Rewired Players defined! You must have at least one Rewired Player to override Unity input. Input calls will be handled by UnityEngine.Input.");
				}
				return null;
			}
			return ReInput.players.GetPlayer(UnityInputOverride.playerId);
		}
	}

	public static Vector3 acceleration => UnityEngine.Input.acceleration;

	public static int accelerationEventCount => UnityEngine.Input.accelerationEventCount;

	public static AccelerationEvent[] accelerationEvents => UnityEngine.Input.accelerationEvents;

	public static bool anyKey => UnityEngine.Input.anyKey;

	public static bool anyKeyDown => UnityEngine.Input.anyKeyDown;

	public static bool backButtonLeavesApp
	{
		get
		{
			return UnityEngine.Input.backButtonLeavesApp;
		}
		set
		{
			UnityEngine.Input.backButtonLeavesApp = value;
		}
	}

	public static Compass compass => UnityEngine.Input.compass;

	public static bool compensateSensors
	{
		get
		{
			return UnityEngine.Input.compensateSensors;
		}
		set
		{
			UnityEngine.Input.compensateSensors = value;
		}
	}

	public static Vector2 compositionCursorPos
	{
		get
		{
			return UnityEngine.Input.compositionCursorPos;
		}
		set
		{
			UnityEngine.Input.compositionCursorPos = value;
		}
	}

	public static string compositionString => UnityEngine.Input.compositionString;

	public static DeviceOrientation deviceOrientation => UnityEngine.Input.deviceOrientation;

	[Obsolete("eatKeyPressOnTextFieldFocus property is deprecated, and only provided to support legacy behavior.")]
	public static bool eatKeyPressOnTextFieldFocus
	{
		get
		{
			return UnityEngine.Input.eatKeyPressOnTextFieldFocus;
		}
		set
		{
			UnityEngine.Input.eatKeyPressOnTextFieldFocus = value;
		}
	}

	public static Gyroscope gyro => UnityEngine.Input.gyro;

	public static IMECompositionMode imeCompositionMode
	{
		get
		{
			return UnityEngine.Input.imeCompositionMode;
		}
		set
		{
			UnityEngine.Input.imeCompositionMode = value;
		}
	}

	public static bool imeIsSelected => UnityEngine.Input.imeIsSelected;

	public static string inputString => UnityEngine.Input.inputString;

	[Obsolete("isGyroAvailable property is deprecated. Please use SystemInfo.supportsGyroscope instead.")]
	public static bool isGyroAvailable => UnityEngine.Input.isGyroAvailable;

	public static Vector3 mousePosition => UnityEngine.Input.mousePosition;

	public static bool mousePresent => UnityEngine.Input.mousePresent;

	public static Vector2 mouseScrollDelta => UnityEngine.Input.mouseScrollDelta;

	public static bool multiTouchEnabled
	{
		get
		{
			return UnityEngine.Input.multiTouchEnabled;
		}
		set
		{
			UnityEngine.Input.multiTouchEnabled = value;
		}
	}

	public static bool simulateMouseWithTouches
	{
		get
		{
			return UnityEngine.Input.simulateMouseWithTouches;
		}
		set
		{
			UnityEngine.Input.simulateMouseWithTouches = value;
		}
	}

	public static bool stylusTouchSupported => UnityEngine.Input.stylusTouchSupported;

	public static int touchCount => UnityEngine.Input.touchCount;

	public static Touch[] touches => UnityEngine.Input.touches;

	public static bool touchPressureSupported => UnityEngine.Input.touchPressureSupported;

	public static bool touchSupported => UnityEngine.Input.touchSupported;

	public static AccelerationEvent GetAccelerationEvent(int index)
	{
		return UnityEngine.Input.GetAccelerationEvent(index);
	}

	public static float GetAxis(string axisName)
	{
		if (player == null)
		{
			return UnityEngine.Input.GetAxis(axisName);
		}
		return player.GetAxis(axisName);
	}

	public static float GetAxisRaw(string axisName)
	{
		if (player == null)
		{
			return UnityEngine.Input.GetAxisRaw(axisName);
		}
		return player.GetAxisRaw(axisName);
	}

	public static bool GetButton(string buttonName)
	{
		if (player == null)
		{
			return UnityEngine.Input.GetButton(buttonName);
		}
		return player.GetButton(buttonName);
	}

	public static bool GetButtonDown(string buttonName)
	{
		if (player == null)
		{
			return UnityEngine.Input.GetButtonDown(buttonName);
		}
		return player.GetButtonDown(buttonName);
	}

	public static bool GetButtonUp(string buttonName)
	{
		if (player == null)
		{
			return UnityEngine.Input.GetButtonUp(buttonName);
		}
		return player.GetButtonUp(buttonName);
	}

	public static string[] GetJoystickNames()
	{
		if (!UnityInputOverride.enabled || !ReInput.isReady)
		{
			return UnityEngine.Input.GetJoystickNames();
		}
		int joystickCount = ReInput.controllers.joystickCount;
		string[] array = new string[joystickCount];
		for (int i = 0; i < joystickCount; i++)
		{
			array[i] = ReInput.controllers.Joysticks[i].name;
		}
		return array;
	}

	public static bool GetKey(KeyCode key)
	{
		return UnityEngine.Input.GetKey(key);
	}

	public static bool GetKey(string name)
	{
		return UnityEngine.Input.GetKey(name);
	}

	public static bool GetKeyDown(KeyCode key)
	{
		return UnityEngine.Input.GetKeyDown(key);
	}

	public static bool GetKeyDown(string name)
	{
		return UnityEngine.Input.GetKeyDown(name);
	}

	public static bool GetKeyUp(KeyCode key)
	{
		return UnityEngine.Input.GetKeyUp(key);
	}

	public static bool GetKeyUp(string name)
	{
		return UnityEngine.Input.GetKeyUp(name);
	}

	public static bool GetMouseButton(int button)
	{
		if (!UnityInputOverride.enabled || !ReInput.isReady)
		{
			return UnityEngine.Input.GetMouseButton(button);
		}
		return ReInput.controllers.Mouse.GetButton(button);
	}

	public static bool GetMouseButtonDown(int button)
	{
		if (!UnityInputOverride.enabled || !ReInput.isReady)
		{
			return UnityEngine.Input.GetMouseButtonDown(button);
		}
		return ReInput.controllers.Mouse.GetButtonDown(button);
	}

	public static bool GetMouseButtonUp(int button)
	{
		if (!UnityInputOverride.enabled || !ReInput.isReady)
		{
			return UnityEngine.Input.GetMouseButtonUp(button);
		}
		return ReInput.controllers.Mouse.GetButtonUp(button);
	}

	[Obsolete("Use ps3 move API instead", true)]
	public static Vector3 GetPosition(int deviceID)
	{
		throw new NotSupportedException();
	}

	[Obsolete("Use ps3 move API instead", true)]
	public static Quaternion GetRotation(int deviceID)
	{
		throw new NotSupportedException();
	}

	public static Touch GetTouch(int index)
	{
		return UnityEngine.Input.GetTouch(index);
	}

	public static bool IsJoystickPreconfigured(string joystickName)
	{
		return false;
	}

	public static void ResetInputAxes()
	{
		UnityEngine.Input.ResetInputAxes();
	}
}
