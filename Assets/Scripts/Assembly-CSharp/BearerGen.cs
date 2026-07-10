using System;
using System.Collections.Generic;
using System.Linq;
using SVGImporter;
using UnityEngine;

[Serializable]
public class BearerGen
{
	public Bearers bearer;

	public int eyeLayer = -1;

	public List<SVGAssetList> layers = new List<SVGAssetList>();

	public BearerGen(Bearers be)
	{
		bearer = be;
		SVGAsset[] array = Resources.LoadAll("bearers/" + be.ToString() + "/", typeof(SVGAsset)).Cast<SVGAsset>().ToArray();
		float num = -1f;
		SVGAssetList sVGAssetList = new SVGAssetList();
		SVGAsset[] array2 = array;
		foreach (SVGAsset sVGAsset in array2)
		{
			if (sVGAsset.name.Substring(1, 1).Equals("-"))
			{
				int result = -1;
				int.TryParse(sVGAsset.name.Substring(0, 1), out result);
				if (num != (float)result)
				{
					num = result;
					sVGAssetList = new SVGAssetList
					{
						listAsset = new List<SVGAsset>()
					};
					layers.Add(sVGAssetList);
				}
				sVGAssetList.listAsset.Add(sVGAsset);
				if (sVGAsset.name.Contains("eye"))
				{
					eyeLayer = result;
				}
			}
		}
	}

	public List<SVGAsset> GetLayer(int id)
	{
		if (id > layers.Count - 1)
		{
			return null;
		}
		return layers[id].listAsset;
	}
}
