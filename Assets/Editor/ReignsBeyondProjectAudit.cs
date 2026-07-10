using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class HelmProjectAudit
{
	[MenuItem("Helm/Audit project")]
	public static void Run()
	{
		int missingScripts = 0;
		int missingReferences = 0;
		int sceneCount = 0;
		int prefabCount = 0;
		StringBuilder details = new StringBuilder();

		foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes.Where(item => item.enabled))
		{
			Scene scene = EditorSceneManager.OpenScene(buildScene.path, OpenSceneMode.Single);
			GameObject[] roots = scene.GetRootGameObjects();
			AuditHierarchy(roots, buildScene.path, ref missingScripts, ref missingReferences, details);
			sceneCount++;

			if (buildScene.path.Contains("/reigns_", StringComparison.Ordinal))
			{
				RequireExactlyOne<GameAct>(roots, buildScene.path);
				RequireExactlyOne<MapCard>(roots, buildScene.path);
				RequireExactlyOne<ConcertCard>(roots, buildScene.path);
				RequireExactlyOne<NavigationAct>(roots, buildScene.path);
				RequireExactlyOne<SpaceUI>(roots, buildScene.path);
			}
		}

		foreach (string guid in AssetDatabase.FindAssets("t:Prefab"))
		{
			string path = AssetDatabase.GUIDToAssetPath(guid);
			GameObject root = PrefabUtility.LoadPrefabContents(path);
			try
			{
				AuditHierarchy(new[] { root }, path, ref missingScripts, ref missingReferences, details);
				prefabCount++;
			}
			finally
			{
				PrefabUtility.UnloadPrefabContents(root);
			}
		}

		string summary = $"HELM_AUDIT scenes={sceneCount} prefabs={prefabCount} missingScripts={missingScripts} missingReferences={missingReferences}";
		if (details.Length > 0)
		{
			Debug.LogError(summary + "\n" + details);
		}
		if (missingScripts != 0 || missingReferences != 0)
		{
			throw new InvalidOperationException(summary);
		}
		Debug.Log(summary);
	}

	private static void AuditHierarchy(IEnumerable<GameObject> roots, string assetPath, ref int missingScripts, ref int missingReferences, StringBuilder details)
	{
		foreach (GameObject root in roots)
		{
			foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
			{
				GameObject gameObject = transform.gameObject;
				int missingOnObject = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
				if (missingOnObject > 0)
				{
					missingScripts += missingOnObject;
					details.AppendLine($"Missing script x{missingOnObject}: {assetPath} :: {HierarchyPath(transform)}");
				}

				foreach (Component component in gameObject.GetComponents<Component>())
				{
					if (component == null)
					{
						continue;
					}
					SerializedObject serializedObject = new SerializedObject(component);
					SerializedProperty property = serializedObject.GetIterator();
					while (property.NextVisible(true))
					{
						if (property.propertyType == SerializedPropertyType.ObjectReference &&
							property.objectReferenceValue == null && property.objectReferenceInstanceIDValue != 0)
						{
							missingReferences++;
							details.AppendLine($"Missing reference: {assetPath} :: {HierarchyPath(transform)} :: {component.GetType().Name}.{property.propertyPath}");
						}
					}
				}
			}
		}
	}

	private static void RequireExactlyOne<T>(IEnumerable<GameObject> roots, string scenePath) where T : Component
	{
		int count = roots.Sum(root => root.GetComponentsInChildren<T>(true).Length);
		if (count != 1)
		{
			throw new InvalidOperationException($"Expected exactly one {typeof(T).Name} in {scenePath}, found {count}.");
		}
	}

	private static string HierarchyPath(Transform transform)
	{
		List<string> parts = new List<string>();
		for (Transform current = transform; current != null; current = current.parent)
		{
			parts.Add(current.name);
		}
		parts.Reverse();
		return string.Join("/", parts);
	}
}
