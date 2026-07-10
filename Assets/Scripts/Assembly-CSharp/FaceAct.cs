using UnityEngine;

public class FaceAct : MonoBehaviour
{
	public static FaceAct diff;

	private void Awake()
	{
		diff = this;
	}

	public Vector2 GetFacePos(float trust)
	{
		return Vector2.zero;
	}
}
