using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class HelmCampaignCompiler
{
	private const string StoryRoot = "Assets/Helm/Story";
	private const string TextRoot = "Assets/Resources/texts";

	private const string CardsI18nHeader = "card;id;bearer;place;question;ar_question;du_question;tc_question;sc_question;ge_question;fr_question;ca_question;it_question;ja_question;ko_question;po_question;ru_question;tu_question;sp_question;override_yes;ar_override_yes;du_override_yes;tc_override_yes;sc_override_yes;ge_override_yes;fr_override_yes;ca_override_yes;it_override_yes;ja_override_yes;ko_override_yes;po_override_yes;ru_override_yes;tu_override_yes;sp_override_yes;answer_yes;ar_answer_yes;du_answer_yes;tc_answer_yes;sc_answer_yes;ge_answer_yes;fr_answer_yes;ca_answer_yes;it_answer_yes;ja_answer_yes;ko_answer_yes;po_answer_yes;ru_answer_yes;tu_answer_yes;sp_answer_yes;override_no;ar_override_no;du_override_no;tc_override_no;sc_override_no;ge_override_no;fr_override_no;ca_override_no;it_override_no;ja_override_no;ko_override_no;po_override_no;ru_override_no;tu_override_no;sp_override_no;answer_no;ar_answer_no;du_answer_no;tc_answer_no;sc_answer_no;ge_answer_no;fr_answer_no;ca_answer_no;it_answer_no;ja_answer_no;ko_answer_no;po_answer_no;ru_answer_no;tu_answer_no;sp_answer_no;vide;";
	private const string CharactersI18nHeader = "id;type;generated;ar_generated;du_generated;tc_generated;sc_generated;ge_generated;fr_generated;ca_generated;it_generated;ja_generated;ko_generated;po_generated;ru_generated;tu_generated;sp_generated;title;ar_title;du_title;tc_title;sc_title;ge_title;fr_title;ca_title;it_title;ja_title;ko_title;po_title;ru_title;tu_title;sp_title;";
	private const string ObjectivesI18nHeader = "name;title;ar_title;du_title;tc_title;sc_title;ge_title;fr_title;ca_title;it_title;ja_title;ko_title;po_title;ru_title;tu_title;sp_title;description;ar_description;du_description;tc_description;sc_description;ge_description;fr_description;ca_description;it_description;ja_description;ko_description;po_description;ru_description;tu_description;sp_description;achievement;ar_achievement;du_achievement;tc_achievement;sc_achievement;ge_achievement;fr_achievement;ca_achievement;it_achievement;ja_achievement;ko_achievement;po_achievement;ru_achievement;tu_achievement;sp_achievement;";

	[InitializeOnLoadMethod]
	private static void QueueCompile()
	{
		EditorApplication.delayCall += CompileIfSourcesExist;
	}

	private static void CompileIfSourcesExist()
	{
		if (File.Exists(Path.Combine(StoryRoot, "helm_cards.csv")))
		{
			Compile();
		}
	}

	[MenuItem("Helm/Compile campaign data")]
	public static void Compile()
	{
		CompileTable("helm_cards.csv", "cards.txt", 22);
		CompileTable("helm_characters.csv", "characters.txt", 14);
		CompileTable("helm_objectives.csv", "objectives.txt", 8);
		CompileUiOverrides();
		WriteEncoded(Path.Combine(TextRoot, "cards_i18n.txt"), CardsI18nHeader);
		WriteEncoded(Path.Combine(TextRoot, "characters_i18n.txt"), CharactersI18nHeader);
		WriteEncoded(Path.Combine(TextRoot, "objectives_i18n.txt"), ObjectivesI18nHeader);
		AssetDatabase.Refresh();
		Debug.Log("HELM_CAMPAIGN_COMPILED decisions=16 endings=5");
	}

	private static void CompileTable(string sourceName, string targetName, int expectedColumns)
	{
		string sourcePath = Path.Combine(StoryRoot, sourceName);
		string raw = Normalize(File.ReadAllText(sourcePath, Encoding.UTF8));
		string[] lines = raw.Split('\n');
		if (lines.Length < 2)
		{
			throw new InvalidDataException($"{sourcePath} must contain a header and at least one row.");
		}

		for (int i = 0; i < lines.Length; i++)
		{
			int columns = lines[i].Split(';').Length;
			if (columns != expectedColumns)
			{
				throw new InvalidDataException($"{sourcePath}:{i + 1} has {columns} columns. Expected {expectedColumns}.");
			}
		}

		if (sourceName == "helm_cards.csv")
		{
			ValidateCards(lines);
		}
		else if (sourceName == "helm_characters.csv")
		{
			ValidateCharacters(lines);
		}

		WriteEncoded(Path.Combine(TextRoot, targetName), raw);
	}

	private static void ValidateCharacters(string[] lines)
	{
		foreach (string line in lines.Skip(1))
		{
			string[] character = line.Split(';');
			for (int i = 7; i <= 10; i++)
			{
				if (!string.IsNullOrEmpty(character[i]) && !Enum.TryParse(character[i], out Bearers _))
				{
					throw new InvalidDataException($"Character '{character[0]}' has invalid tag '{character[i]}' in column {i + 1}.");
				}
			}

			string eyes = character[11];
			if (!string.IsNullOrEmpty(eyes) && eyes != "no" && !int.TryParse(eyes, out int _))
			{
				throw new InvalidDataException($"Character '{character[0]}' has invalid eyes value '{eyes}'.");
			}
		}
	}

	private static void ValidateCards(string[] lines)
	{
		List<string[]> cards = lines.Skip(1).Select(line => line.Split(';')).ToList();
		int decisions = cards.Count(card => !card[3].StartsWith("end", StringComparison.Ordinal));
		int endings = cards.Count(card => card[3].StartsWith("end", StringComparison.Ordinal));
		if (decisions != 16 || endings != 5)
		{
			throw new InvalidDataException($"Helm requires exactly 16 decisions and 5 endings. Found {decisions} and {endings}.");
		}
		if (cards[0][1] != "first_card")
		{
			throw new InvalidDataException("The first Helm card must be named first_card.");
		}

		HashSet<int> ids = new HashSet<int>();
		foreach (string[] card in cards)
		{
			if (!int.TryParse(card[2], out int id) || !ids.Add(id))
			{
				throw new InvalidDataException($"Card id '{card[2]}' is invalid or duplicated.");
			}
		}

		ValidateCampaignBalance(cards);
	}

	private static void ValidateCampaignBalance(List<string[]> cards)
	{
		List<string[]> decisions = cards.Where(card => !card[3].StartsWith("end", StringComparison.Ordinal)).ToList();
		List<string[]> endings = cards.Where(card => card[3].StartsWith("end", StringComparison.Ordinal)).ToList();
		string[] meterNames = { "power", "oxygen", "people", "hull" };
		Dictionary<string, int> minimums = meterNames.ToDictionary(name => name, name => 101);
		Dictionary<string, int> maximums = meterNames.ToDictionary(name => name, name => -1);
		HashSet<string> reachedEndings = new HashSet<string>();

		for (int mask = 0; mask < 1 << decisions.Count; mask++)
		{
			Dictionary<string, int> meters = meterNames.ToDictionary(name => name, name => 50);
			Dictionary<string, int> scores = new Dictionary<string, int>
			{
				{ "nb_open", 0 },
				{ "nb_order", 0 },
				{ "nb_human", 0 }
			};

			for (int i = 0; i < decisions.Count; i++)
			{
				string outcome = decisions[i][((mask >> i) & 1) == 1 ? 17 : 20];
				foreach (string term in outcome.Split(new[] { " and " }, StringSplitOptions.RemoveEmptyEntries))
				{
					Match meter = Regex.Match(term, "^(power|oxygen|people|hull)([+-])(\\d+)$");
					if (meter.Success)
					{
						int delta = int.Parse(meter.Groups[3].Value);
						meters[meter.Groups[1].Value] += meter.Groups[2].Value == "+" ? delta : -delta;
						continue;
					}

					Match score = Regex.Match(term, "^(nb_open|nb_order|nb_human)\\+(\\d+)$");
					if (score.Success)
					{
						scores[score.Groups[1].Value] += int.Parse(score.Groups[2].Value);
					}
				}
			}

			foreach (string meterName in meterNames)
			{
				minimums[meterName] = Math.Min(minimums[meterName], meters[meterName]);
				maximums[meterName] = Math.Max(maximums[meterName], meters[meterName]);
			}

			string[] ending = endings.FirstOrDefault(card => EndingMatches(card[6], scores));
			if (ending == null)
			{
				throw new InvalidDataException($"Timeline mask {mask} has no matching ending.");
			}
			reachedEndings.Add(ending[2]);
		}

		foreach (string meterName in meterNames)
		{
			if (minimums[meterName] <= 0 || maximums[meterName] >= 100)
			{
				throw new InvalidDataException($"Campaign can push {meterName} outside the safe range: {minimums[meterName]}..{maximums[meterName]}.");
			}
		}
		if (reachedEndings.Count != endings.Count)
		{
			throw new InvalidDataException($"Only {reachedEndings.Count} of {endings.Count} endings are reachable across all timelines.");
		}

		Debug.Log("HELM_BALANCE_VALID " + string.Join(" ", meterNames.Select(name => $"{name}={minimums[name]}..{maximums[name]}")));
	}

	private static bool EndingMatches(string condition, Dictionary<string, int> scores)
	{
		if (string.IsNullOrEmpty(condition))
		{
			return true;
		}
		foreach (string term in condition.Split(new[] { " and " }, StringSplitOptions.RemoveEmptyEntries))
		{
			Match match = Regex.Match(term, "^(nb_open|nb_order|nb_human)>(\\d+)$");
			if (!match.Success || scores[match.Groups[1].Value] <= int.Parse(match.Groups[2].Value))
			{
				return false;
			}
		}
		return true;
	}

	private static void CompileUiOverrides()
	{
		string targetPath = Path.Combine(TextRoot, "all_i18n.txt");
		string decoded = Decode(File.ReadAllText(targetPath, Encoding.UTF8));
		List<string[]> rows = Normalize(decoded).Split('\n').Select(line => line.Split(';')).ToList();
		string[] overrides = Normalize(File.ReadAllText(Path.Combine(StoryRoot, "helm_ui_overrides.csv"), Encoding.UTF8)).Split('\n');

		foreach (string line in overrides.Skip(1))
		{
			string[] values = line.Split(';');
			if (values.Length != 3)
			{
				throw new InvalidDataException($"Invalid UI override: {line}");
			}
			string id = values[0];
			string match = values[1];
			string replacement = values[2];
			string[] row = rows.FirstOrDefault(candidate => candidate.Length > 1 &&
				(!string.IsNullOrEmpty(id) ? candidate[0] == id : candidate[1] == match || candidate[1] == replacement));
			if (row == null)
			{
				throw new InvalidDataException($"UI override target not found: id='{id}' match='{match}'");
			}
			for (int i = 1; i < row.Length; i++)
			{
				// The campaign is authored in English for this spin-up. Mirroring
				// the replacement prevents a machine's saved locale from reviving
				// legacy product names or unrelated end-screen copy.
				if (i == row.Length - 1 && string.IsNullOrEmpty(row[i]))
				{
					continue;
				}
				row[i] = replacement;
			}
		}

		WriteEncoded(targetPath, string.Join("\n", rows.Select(row => string.Join(";", row))));
	}

	private static string Normalize(string value)
	{
		return value.Replace("\r\n", "\n").Replace('\r', '\n').TrimEnd('\n');
	}

	private static string Decode(string encoded)
	{
		return Encoding.UTF8.GetString(Convert.FromBase64String(encoded.Trim()));
	}

	private static void WriteEncoded(string path, string raw)
	{
		string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(Normalize(raw)));
		if (File.Exists(path) && File.ReadAllText(path, Encoding.UTF8).Trim() == encoded)
		{
			return;
		}
		File.WriteAllText(path, encoded, new UTF8Encoding(false));
	}
}
