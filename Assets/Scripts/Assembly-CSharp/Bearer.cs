using System;
using System.Collections.Generic;
using SVGImporter;
using UnityEngine;

[Serializable]
public class Bearer
{
	public string name;

	public string firstname;

	public GText title;

	public GText generated;

	public SVGAsset sprite;

	public Bearers bearer;

	public bool hasVote;

	public int super;

	public float vote;

	public int max = 1;

	public BearerTypes type;

	public List<Bearers> character = new List<Bearers>();

	public CharacterCard scCa;

	public bool hasEyes;

	public SVGAsset eyes;

	public int staydead = 100;

	public int eyesPos;

	public Bearer(Bearer be)
	{
		if (be != null)
		{
			name = be.name;
			generated = ((be.generated != null) ? be.generated.TreatName() : new GText("anyone"));
			title = be.title;
			staydead = be.staydead;
			bearer = be.bearer;
			sprite = be.sprite;
			eyes = be.eyes;
			eyesPos = be.eyesPos;
			title = be.title;
			max = be.max;
			hasVote = be.hasVote;
			super = be.super;
			vote = be.vote;
			type = be.type;
			character = be.character;
			scCa = be.scCa;
			hasEyes = be.hasEyes;
		}
	}

	public void ResetName(string seed)
	{
		generated.TreatName(bearer, seed);
	}

	public Bearer(string[] rawele, string[] columns, char dele, Dictionary<string, string[]> i18n)
	{
		string[] array = new string[2];
		bool flag = false;
		max = 1;
		for (int i = 0; i < columns.Length; i++)
		{
			string text = rawele[i];
			switch (columns[i])
			{
			case "id":
				if (i18n.Count > 0 && i18n.ContainsKey(text))
				{
					array = i18n[text];
					flag = true;
				}
				break;
			case "bearer":
				bearer = (Bearers)Enum.Parse(typeof(Bearers), text);
				sprite = (SVGAsset)Resources.Load("bearers/" + text, typeof(SVGAsset));
				break;
			case "type":
				type = (BearerTypes)Enum.Parse(typeof(BearerTypes), text);
				if (type == BearerTypes.individual)
				{
					hasVote = true;
				}
				break;
			case "staydead":
				int.TryParse(text, out staydead);
				break;
			case "tag1":
			case "tag2":
			case "tag3":
			case "tag4":
				if (!string.IsNullOrEmpty(text))
				{
					character.Add((Bearers)Enum.Parse(typeof(Bearers), text));
				}
				break;
			case "eyes":
			{
				hasEyes = true;
				if (text == "no")
				{
					hasEyes = false;
					break;
				}
				int.TryParse(text, out eyesPos);
				string text2 = bearer.ToString();
				eyes = (SVGAsset)Resources.Load("eyes/" + text2, typeof(SVGAsset));
				if (eyes == null)
				{
					eyes = (SVGAsset)Resources.Load("eyes/anyone", typeof(SVGAsset));
				}
				break;
			}
			case "generated":
				generated = (flag ? new GText(array[0]) : new GText(text));
				name = (flag ? array[0] : text);
				break;
			case "title":
				title = (flag ? new GText(array[1]) : new GText(text));
				break;
			}
		}
	}
}
