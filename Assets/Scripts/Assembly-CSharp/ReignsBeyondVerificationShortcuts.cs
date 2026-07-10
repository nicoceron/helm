using System.Collections;
using System.Text;
using UnityEngine;

public sealed class ReignsBeyondVerificationShortcuts : MonoBehaviour
{
#if DEVELOPMENT_BUILD || UNITY_EDITOR
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void Install()
	{
		if (FindFirstObjectByType<ReignsBeyondVerificationShortcuts>() != null)
		{
			return;
		}
		GameObject host = new GameObject("Helm Verification Shortcuts");
		DontDestroyOnLoad(host);
		host.AddComponent<ReignsBeyondVerificationShortcuts>();
	}

	private void Update()
	{
		if (GameAct.diff == null)
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.F6))
		{
			OpenVerificationCard(1, "opening");
		}
		else if (Input.GetKeyDown(KeyCode.F7))
		{
			OpenVerificationCard(8, "labor crisis");
		}
		else if (Input.GetKeyDown(KeyCode.F8))
		{
			OpenVerificationCard(16, "solar strike");
		}
		else if (Input.GetKeyDown(KeyCode.F9))
		{
			StartCoroutine(DumpState("current"));
		}
		else if (Input.GetKeyDown(KeyCode.F10))
		{
			Debug.Log("[Verification] Invoking the current focused action");
			InputAct.diff.OnAction?.Invoke(true);
			StartCoroutine(DumpState("forced action"));
		}
		else if (Input.GetKeyDown(KeyCode.F12))
		{
			GameAct.diff.SetInt(Variables.distance, 3);
			DataStore.SaveSlot("GameSave", new GameSave(GameAct.diff, withresurrect: false));
			Debug.Log($"[Verification] Saved to {Application.persistentDataPath}");
		}
	}

	private static void PrepareNavigationLayer()
	{
		if (NavigationAct.diff != null)
		{
			NavigationAct.diff.Activate();
		}
		GameAct.diff.SetInt(Variables.stop, -1);
	}

	private void OpenVerificationCard(int cardId, string label)
	{
		Debug.Log($"[Verification] Opening {label} card {cardId}");
		GameAct.diff.OpenCard(cardId);
		StartCoroutine(DumpState(label));
	}

	private static IEnumerator DumpState(string label)
	{
		yield return new WaitForSeconds(1.25f);
		StringBuilder report = new StringBuilder();
		report.AppendLine($"[Verification] {label} state");
		report.AppendLine($"card={GameAct.diff?.card?.id}:{GameAct.diff?.card?.name} type={GameAct.diff?.cardType} state={GameAct.diff?.state}");
		if (InputAct.diff != null)
		{
			string action = InputAct.diff.OnAction == null ? "null" : $"{InputAct.diff.OnAction.Method.DeclaringType?.Name}.{InputAct.diff.OnAction.Method.Name}";
			report.AppendLine($"input={InputAct.diff.curInput} menu={InputAct.diff.isInMenu} inventory={InputAct.diff.isInventory} action={action}");
		}

		MapCard map = FindFirstObjectByType<MapCard>(FindObjectsInactive.Include);
		if (map != null)
		{
			report.AppendLine($"map active={map.gameObject.activeInHierarchy} spots={map.MapSpots.Count} circle={DescribeRect(map.circle)} line={DescribeLine(map.line)}");
		}

		ConcertCard concert = FindFirstObjectByType<ConcertCard>(FindObjectsInactive.Include);
		if (concert != null)
		{
			report.AppendLine($"concert active={concert.gameObject.activeInHierarchy} playing={concert.isPlaying} guitar={DescribeTransform(concert.guitar)} children={concert.guitar.childCount} line={DescribeLine(concert.line)} secondLine={DescribeLine(concert.secondline)} peopleBack={concert.peopleBack.gameObject.activeInHierarchy}/{concert.peopleBack.color}");
			Renderer[] renderers = concert.GetComponentsInChildren<Renderer>(true);
			for (int i = 0; i < renderers.Length && i < 32; i++)
			{
				Renderer renderer = renderers[i];
				string shader = renderer.sharedMaterial != null && renderer.sharedMaterial.shader != null ? renderer.sharedMaterial.shader.name : "none";
				report.AppendLine($"concertRenderer[{i}]={renderer.name} active={renderer.gameObject.activeInHierarchy} enabled={renderer.enabled} shader={shader} bounds={renderer.bounds}");
			}
		}

		SpaceUI space = FindFirstObjectByType<SpaceUI>(FindObjectsInactive.Include);
		if (space != null)
		{
			report.AppendLine($"space active={space.gameObject.activeInHierarchy} ship={space.ship.gameObject.activeInHierarchy}/{space.ship.enabled}@{DescribeRect(space.ship.rectTransform)} enemy={space.enemy.gameObject.activeInHierarchy}/{space.enemy.enabled}@{DescribeRect(space.enemy.rectTransform)} stars={space.stars.Count}");
		}

		Camera[] cameras = FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
		for (int i = 0; i < cameras.Length; i++)
		{
			Camera camera = cameras[i];
			report.AppendLine($"camera[{i}]={camera.name} active={camera.gameObject.activeInHierarchy} enabled={camera.enabled} depth={camera.depth} clear={camera.clearFlags}/{camera.backgroundColor} cull={camera.cullingMask}");
		}
		Debug.Log(report.ToString());
	}

	private static string DescribeLine(LineRenderer line)
	{
		if (line == null)
		{
			return "null";
		}
		string shader = line.sharedMaterial != null && line.sharedMaterial.shader != null ? line.sharedMaterial.shader.name : "none";
		return $"active={line.gameObject.activeInHierarchy} enabled={line.enabled} points={line.positionCount} width={line.widthMultiplier} shader={shader}";
	}

	private static string DescribeRect(RectTransform rect)
	{
		return rect == null ? "null" : $"active={rect.gameObject.activeInHierarchy} pos={rect.anchoredPosition3D} size={rect.rect.size}";
	}

	private static string DescribeTransform(Transform value)
	{
		return value == null ? "null" : $"active={value.gameObject.activeInHierarchy} local={value.localPosition} scale={value.localScale}";
	}
#endif
}
