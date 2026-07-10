using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class HelmBuild
{
	[MenuItem("Helm/Build macOS")]
	public static void BuildMacOS()
	{
		BuildMacOS(BuildOptions.None);
	}

	[MenuItem("Helm/Build macOS (Development)")]
	public static void BuildMacOSDevelopment()
	{
		BuildMacOS(BuildOptions.Development);
	}

	private static void BuildMacOS(BuildOptions options)
	{
		HelmCampaignCompiler.Compile();
		ConfigurePlatformPlugins();
		string outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Builds", "macOS", "Helm.app"));
		string[] scenes = EditorBuildSettings.scenes
			.Where(scene => scene.enabled)
			.Select(scene => scene.path)
			.ToArray();

		if (scenes.Length == 0)
		{
			throw new InvalidOperationException("No enabled scenes are present in EditorBuildSettings.");
		}

		Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
		if (Directory.Exists(outputPath))
		{
			Directory.Delete(outputPath, true);
		}
		PlayerSettings.companyName = "Helm Project";
		PlayerSettings.productName = "Helm";
		PlayerSettings.applicationIdentifier = "com.helm.lionriseprotocol";
		PlayerSettings.defaultScreenWidth = 1280;
		PlayerSettings.defaultScreenHeight = 720;
		PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
		PlayerSettings.runInBackground = true;

		BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
		{
			scenes = scenes,
			locationPathName = outputPath,
			target = BuildTarget.StandaloneOSX,
			options = options
		});

		if (report.summary.result != BuildResult.Succeeded)
		{
			throw new InvalidOperationException($"macOS build failed: {report.summary.result} ({report.summary.totalErrors} errors)");
		}

		Debug.Log($"HELM_BUILD_SUCCEEDED path={outputPath} size={report.summary.totalSize} options={options}");
	}

	private static void ConfigurePlatformPlugins()
	{
		const string windowsBackendPath = "Assets/Plugins/Rewired_Windows.dll";
		PluginImporter importer = AssetImporter.GetAtPath(windowsBackendPath) as PluginImporter;
		if (importer == null)
		{
			return;
		}
		importer.SetCompatibleWithAnyPlatform(false);
		importer.SetCompatibleWithEditor(false);
		importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSX, false);
		importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows, true);
		importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64, true);
		importer.SaveAndReimport();
	}
}
