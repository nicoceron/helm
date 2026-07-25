using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

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

	[MenuItem("Helm/Build WebGL")]
	public static void BuildWebGL()
	{
		BuildWebGL(BuildOptions.None);
	}

	[MenuItem("Helm/Build WebGL (Development)")]
	public static void BuildWebGLDevelopment()
	{
		WebGLExceptionSupport previousExceptionSupport = PlayerSettings.WebGL.exceptionSupport;
		try
		{
			PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.FullWithStacktrace;
			BuildWebGL(BuildOptions.Development);
		}
		finally
		{
			PlayerSettings.WebGL.exceptionSupport = previousExceptionSupport;
		}
	}

	private static void BuildMacOS(BuildOptions options)
	{
		HelmCampaignCompiler.Compile();
		ConfigurePlatformPlugins();
		string outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Builds", "macOS", "Helm.app"));
		string[] scenes = GetEnabledScenes();

		Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
		if (Directory.Exists(outputPath))
		{
			Directory.Delete(outputPath, true);
		}
		ConfigureCommonPlayerSettings();
		PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
		PlayerSettings.macRetinaSupport = false;
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

	private static void BuildWebGL(BuildOptions options)
	{
		HelmCampaignCompiler.Compile();
		ConfigurePlatformPlugins();
		string outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Builds", "WebGL"));
		string[] scenes = GetEnabledScenes();

		if (Directory.Exists(outputPath))
		{
			Directory.Delete(outputPath, true);
		}
		Directory.CreateDirectory(outputPath);
		ConfigureCommonPlayerSettings();
		PlayerSettings.WebGL.template = "APPLICATION:Default";
		PlayerSettings.WebGL.exceptionSupport = (options & BuildOptions.Development) != 0
			? WebGLExceptionSupport.FullWithStacktrace
			: WebGLExceptionSupport.FullWithoutStacktrace;
		PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
		PlayerSettings.WebGL.decompressionFallback = true;
		PlayerSettings.WebGL.dataCaching = true;
		PlayerSettings.WebGL.threadsSupport = false;

		bool previousRunInBackground = PlayerSettings.runInBackground;
		PlayerSettings.runInBackground = false;
		BuildReport report;
		WebGLBuildPreparation preparation = new WebGLBuildPreparation(scenes);
		try
		{
			preparation.Prepare();
			report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
			{
				scenes = preparation.ScenePaths,
				locationPathName = outputPath,
				target = BuildTarget.WebGL,
				options = options
			});
		}
		finally
		{
			preparation.Dispose();
			PlayerSettings.runInBackground = previousRunInBackground;
		}

		if (report.summary.result != BuildResult.Succeeded)
		{
			throw new InvalidOperationException($"WebGL build failed: {report.summary.result} ({report.summary.totalErrors} errors)");
		}

		CapWebGLPixelRatio(outputPath);
		Debug.Log($"HELM_WEBGL_BUILD_SUCCEEDED path={outputPath} size={report.summary.totalSize} options={options}");
	}

	private static string[] GetEnabledScenes()
	{
		string[] scenes = EditorBuildSettings.scenes
			.Where(scene => scene.enabled)
			.Select(scene => scene.path)
			.ToArray();

		if (scenes.Length == 0)
		{
			throw new InvalidOperationException("No enabled scenes are present in EditorBuildSettings.");
		}
		return scenes;
	}

	private static void ConfigureCommonPlayerSettings()
	{
		PlayerSettings.companyName = "Helm Project";
		PlayerSettings.productName = "Helm";
		PlayerSettings.applicationIdentifier = "com.helm.lionriseprotocol";
		PlayerSettings.defaultScreenWidth = 1280;
		PlayerSettings.defaultScreenHeight = 720;
	}

	private static void CapWebGLPixelRatio(string outputPath)
	{
		string indexPath = Path.Combine(outputPath, "index.html");
		if (!File.Exists(indexPath))
		{
			throw new FileNotFoundException("Unity did not generate the WebGL index.", indexPath);
		}

		string html = File.ReadAllText(indexPath);
		const string marker = "var config = {";
		if (!html.Contains("devicePixelRatio: 1") && html.Contains(marker))
		{
			html = html.Replace(marker, marker + "\n        devicePixelRatio: 1,\n        autoSyncPersistentDataPath: true,");
		}
		html = html.Replace("<canvas id=\"unity-canvas\" width=960 height=600 tabindex=\"-1\"></canvas>",
			"<canvas id=\"unity-canvas\" width=1280 height=720 tabindex=\"-1\"></canvas>");
		html = html.Replace("canvas.style.width = \"960px\";\n        canvas.style.height = \"600px\";",
			"canvas.style.width = \"100%\";\n        canvas.style.height = \"100%\";");
		html = html.Replace("document.querySelector(\"#unity-loading-bar\").style.display = \"none\";",
			"document.querySelector(\"#unity-loading-bar\").style.display = \"none\";\n                window.parent.postMessage({ type: \"helm-unity-ready\" }, window.location.origin);");
		html = html.Replace("alert(message);",
			"console.error(\"Unity initialization failed\", message);\n                unityShowBanner(String(message), \"error\");\n                window.parent.postMessage({ type: \"helm-unity-error\", message: String(message) }, window.location.origin);");
		html = AddChunkedDataFetchBridge(outputPath, html);
		File.WriteAllText(indexPath, html);

		string stylePath = Path.Combine(outputPath, "TemplateData", "style.css");
		if (File.Exists(stylePath))
		{
			File.AppendAllText(stylePath,
				"\nhtml, body { width: 100%; height: 100%; overflow: hidden; background: #000; }" +
				"\n#unity-container.unity-desktop { inset: 0; width: 100%; height: 100%; transform: none; }" +
				"\n#unity-canvas { display: block; width: 100% !important; height: 100% !important; }" +
				"\n#unity-footer { display: none; }\n");
		}
	}

	private static string AddChunkedDataFetchBridge(string outputPath, string html)
	{
		const int chunkSize = 20 * 1024 * 1024;
		string buildFolder = Path.Combine(outputPath, "Build");
		string dataPath = Directory.GetFiles(buildFolder, "*.data.unityweb").SingleOrDefault();
		if (dataPath == null || new FileInfo(dataPath).Length <= chunkSize)
		{
			return html;
		}

		long totalSize = new FileInfo(dataPath).Length;
		int chunkCount = (int)Math.Ceiling((double)totalSize / chunkSize);
		byte[] buffer = new byte[1024 * 1024];
		using (FileStream source = File.OpenRead(dataPath))
		{
			for (int index = 0; index < chunkCount; index++)
			{
				using FileStream destination = File.Create($"{dataPath}.part{index}");
				int remaining = (int)Math.Min(chunkSize, totalSize - source.Position);
				while (remaining > 0)
				{
					int read = source.Read(buffer, 0, Math.Min(buffer.Length, remaining));
					if (read == 0)
					{
						break;
					}
					destination.Write(buffer, 0, read);
					remaining -= read;
				}
			}
		}
		File.Delete(dataPath);

		string partUrls = string.Join(", ",
			Enumerable.Range(0, chunkCount).Select(index =>
				$"new URL(buildUrl + \"/{Path.GetFileName(dataPath)}.part{index}\", document.URL).href"));
		string bridge = $@"
      // Sites limits individual static assets to 25 MiB. Stream the compressed
      // Unity data chunks as one response without creating a second full-size Blob.
      const helmNativeFetch = window.fetch.bind(window);
      const helmDataUrl = new URL(config.dataUrl, document.URL).href;
      const helmDataParts = [{partUrls}];
      window.fetch = async function(resource, init) {{
        const requestedUrl = typeof resource === ""string""
          ? new URL(resource, document.URL).href
          : resource.url;
        if (requestedUrl !== helmDataUrl) return helmNativeFetch(resource, init);
        const stream = new ReadableStream({{
          async start(controller) {{
            try {{
              for (const partUrl of helmDataParts) {{
                const response = await helmNativeFetch(partUrl, {{ cache: ""no-store"" }});
                if (!response.ok || !response.body) throw new Error(`Failed to load ${{partUrl}}`);
                const reader = response.body.getReader();
                while (true) {{
                  const {{ done, value }} = await reader.read();
                  if (done) break;
                  controller.enqueue(value);
                }}
              }}
              controller.close();
            }} catch (error) {{
              controller.error(error);
            }}
          }}
        }});
        return new Response(stream, {{
          headers: {{
            ""Content-Type"": ""application/octet-stream"",
            ""Content-Length"": ""{totalSize}"",
            ""Cache-Control"": ""public, max-age=31536000, immutable""
          }}
        }});
      }};

";
		const string scriptMarker = "      var script = document.createElement(\"script\");";
		return html.Replace(scriptMarker, bridge + scriptMarker);
	}

	private sealed class WebGLBuildPreparation : IDisposable
	{
		private const string TemporaryScenesFolder = "Assets/__HelmWebGLBuild";
		private const string MusicSourceFolder = "Assets/Resources/music";
		private const string MusicStagingFolder = "Assets/__HelmWebGLMusic";
		private readonly string[] sourceScenes;
		private bool musicMoved;

		public string[] ScenePaths { get; private set; }

		public WebGLBuildPreparation(string[] sourceScenes)
		{
			this.sourceScenes = sourceScenes;
		}

		public void Prepare()
		{
			AssetDatabase.DeleteAsset(TemporaryScenesFolder);
			AssetDatabase.DeleteAsset(MusicStagingFolder);
			AssetDatabase.CreateFolder("Assets", "__HelmWebGLBuild");

			ScenePaths = sourceScenes.Select(CopyAndOptimizeScene).ToArray();
			string moveError = AssetDatabase.MoveAsset(MusicSourceFolder, MusicStagingFolder);
			if (!string.IsNullOrEmpty(moveError))
			{
				throw new InvalidOperationException($"Could not stage browser music assets: {moveError}");
			}
			musicMoved = true;
			AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
		}

		private static string CopyAndOptimizeScene(string sourcePath)
		{
			string destinationPath = $"{TemporaryScenesFolder}/{Path.GetFileName(sourcePath)}";
			if (!AssetDatabase.CopyAsset(sourcePath, destinationPath))
			{
				throw new InvalidOperationException($"Could not create temporary WebGL scene: {sourcePath}");
			}

			Scene scene = EditorSceneManager.OpenScene(destinationPath, OpenSceneMode.Single);
			JukeBox[] jukeboxes = Resources.FindObjectsOfTypeAll<JukeBox>()
				.Where(jukebox => jukebox.gameObject.scene == scene)
				.ToArray();

			foreach (JukeBox jukebox in jukeboxes)
			{
				jukebox.musics = jukebox.musics
					.Where(music => music != null && music.sample != null)
					.GroupBy(music => music.command ?? string.Empty)
					.Select(group => group.OrderBy(GetAudioAssetSize).First())
					.ToList();
				EditorUtility.SetDirty(jukebox);
			}

			EditorSceneManager.SaveScene(scene);
			return destinationPath;
		}

		private static long GetAudioAssetSize(Music music)
		{
			string assetPath = AssetDatabase.GetAssetPath(music.sample);
			string absolutePath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", assetPath));
			return File.Exists(absolutePath) ? new FileInfo(absolutePath).Length : long.MaxValue;
		}

		public void Dispose()
		{
			if (musicMoved && AssetDatabase.IsValidFolder(MusicStagingFolder))
			{
				string moveError = AssetDatabase.MoveAsset(MusicStagingFolder, MusicSourceFolder);
				if (!string.IsNullOrEmpty(moveError))
				{
					throw new InvalidOperationException($"Could not restore browser music assets: {moveError}");
				}
				musicMoved = false;
			}
			AssetDatabase.DeleteAsset(TemporaryScenesFolder);
			AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
		}
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
