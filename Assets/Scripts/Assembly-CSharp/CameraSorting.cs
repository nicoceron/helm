using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class CameraSorting : MonoBehaviour
{
	public TransparencySortMode transparencySortMode;

	protected Camera cameraTarget;

	private void OnEnable()
	{
		if (cameraTarget == null)
		{
			cameraTarget = GetComponent<Camera>();
		}
		cameraTarget.transparencySortMode = transparencySortMode;
	}

	private void OnValidate()
	{
		if (cameraTarget == null)
		{
			cameraTarget = GetComponent<Camera>();
		}
		cameraTarget.transparencySortMode = transparencySortMode;
	}
}
