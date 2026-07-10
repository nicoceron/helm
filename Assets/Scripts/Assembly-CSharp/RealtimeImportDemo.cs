using SVGImporter;
using UnityEngine;
using UnityEngine.UI;

public class RealtimeImportDemo : MonoBehaviour
{
	public SVGImage preview;

	public InputField svgInput;

	protected SVGAsset svgAsset;

	public void Load()
	{
		if (!(svgInput == null) && !string.IsNullOrEmpty(svgInput.text))
		{
			if (svgAsset != null)
			{
				Object.Destroy(svgAsset);
			}
			svgAsset = SVGAsset.Load(svgInput.text);
			preview.vectorGraphics = svgAsset;
		}
	}
}
