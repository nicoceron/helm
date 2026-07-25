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
		Debug.Log("HELM_CAMPAIGN_COMPILED decisions=16 minigames=2 endings=5");
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
		int decisions = cards
			.Where(card => !IsEnding(card) && !IsCinematic(card) && !IsMinigame(card))
			.Select(card => card[1])
			.Distinct()
			.Count();
		int minigames = cards
			.Where(IsMinigame)
			.Select(card => card[1])
			.Distinct()
			.Count();
		int cinematics = cards
			.Where(IsCinematic)
			.Select(card => card[1])
			.Distinct()
			.Count();
		int endings = cards.Count(IsEnding);
		if (decisions != 16 || minigames != 2 || cinematics < 6 || endings != 5)
		{
			throw new InvalidDataException(
				$"Helm requires 16 policy decisions, 2 minigames, at least 6 cinematics, and 5 endings. " +
				$"Found {decisions}, {minigames}, {cinematics}, and {endings}.");
		}
		if (cards[0][1] != "first_card")
		{
			throw new InvalidDataException("The first Helm card must be named first_card.");
		}
		if (!cards[0][9].Contains("SCENARIO S1", StringComparison.Ordinal) ||
			!cards[0][9].Contains("BIG BROTHER IS WATCHING", StringComparison.Ordinal))
		{
			throw new InvalidDataException("The first Helm card must identify SCENARIO S1: BIG BROTHER IS WATCHING.");
		}

		HashSet<int> ids = new HashSet<int>();
		foreach (string[] card in cards)
		{
			if (!int.TryParse(card[2], out int id) || !ids.Add(id))
			{
				throw new InvalidDataException($"Card id '{card[2]}' is invalid or duplicated.");
			}
			if (!IsEnding(card) && !IsCinematic(card) && card[9].Length > 125)
			{
				throw new InvalidDataException($"Card id {card[2]} has {card[9].Length} question characters. Helm policy cards must stay at or below 125 to preserve the original layout.");
			}
			if (IsCinematic(card) && card[3] != "intercale" && card[9].Length > 135)
			{
				throw new InvalidDataException($"Character interstitial id {card[2]} has {card[9].Length} question characters. Keep it at or below 135.");
			}
			if (card[15].Length > 20 || card[18].Length > 20)
			{
				throw new InvalidDataException($"Card id {card[2]} has a choice label longer than 20 characters. Keep choices compact like the original game.");
			}
		}

		ValidateCampaignBalance(cards);
	}

	private static bool IsEnding(string[] card)
	{
		return card[3].StartsWith("end", StringComparison.Ordinal);
	}

	private static bool IsCinematic(string[] card)
	{
		return card[3] == "intercale" || card[0] == "briefing" || card[0] == "cinematic";
	}

	private static bool IsMinigame(string[] card)
	{
		return card[3] == "concert" || card[3] == "fight";
	}

	private static void ValidateCampaignBalance(List<string[]> cards)
	{
		List<string[]> decisionRows = cards.Where(card => !IsEnding(card)).ToList();
		Dictionary<string, List<string[]>> stages = decisionRows
			.GroupBy(card => card[1])
			.ToDictionary(group => group.Key, group => group.ToList());
		List<string[]> endings = cards.Where(IsEnding).ToList();
		string[] meterNames = { "power", "oxygen", "people", "hull" };
		Dictionary<string, int> minimums = meterNames.ToDictionary(name => name, name => 50);
		Dictionary<string, int> maximums = meterNames.ToDictionary(name => name, name => 50);
		HashSet<string> reachedEndings = new HashSet<string>();
		Dictionary<string, int> endingCounts = endings.ToDictionary(card => card[2], card => 0);
		int branchingStageCount = decisionRows
			.Where(card => !IsCinematic(card))
			.Select(card => card[1])
			.Distinct()
			.Count();
		int timelineCount = 1 << branchingStageCount;

		for (int mask = 0; mask < timelineCount; mask++)
		{
			Dictionary<string, int> meters = meterNames.ToDictionary(name => name, name => 50);
			Dictionary<string, int> state = new Dictionary<string, int>
			{
				{ "nb_growth", 0 },
				{ "nb_capacity", 0 },
				{ "nb_trust", 0 }
			};
			string currentStage = "first_card";
			int branchIndex = 0;
			int graphSteps = 0;

			while (currentStage != "_verdict")
			{
				graphSteps++;
				if (graphSteps > 64)
				{
					throw new InvalidDataException($"Timeline mask {mask} exceeded 64 graph nodes near '{currentStage}'.");
				}
				if (!stages.TryGetValue(currentStage, out List<string[]> variants))
				{
					throw new InvalidDataException($"Timeline mask {mask} cannot find story stage '{currentStage}'.");
				}

				string[] decision = variants.FirstOrDefault(card => ConditionMatches(card[6], state));
				if (decision == null)
				{
					throw new InvalidDataException($"Timeline mask {mask} has no valid variant for story stage '{currentStage}'.");
				}

				string loadNext = ApplyOutcome(decision[14], meters, state);
				bool cinematic = IsCinematic(decision);
				string outcome;
				if (cinematic)
				{
					outcome = decision[17];
				}
				else
				{
					outcome = decision[((mask >> branchIndex) & 1) == 1 ? 17 : 20];
					branchIndex++;
				}
				string outcomeNext = ApplyOutcome(outcome, meters, state);
				currentStage = string.IsNullOrEmpty(outcomeNext) ? loadNext : outcomeNext;
				if (string.IsNullOrEmpty(currentStage))
				{
					throw new InvalidDataException($"Card id {decision[2]} does not chain to the next story stage.");
				}

				foreach (string meterName in meterNames)
				{
					minimums[meterName] = Math.Min(minimums[meterName], meters[meterName]);
					maximums[meterName] = Math.Max(maximums[meterName], meters[meterName]);
				}
			}

			if (branchIndex != branchingStageCount)
			{
				throw new InvalidDataException(
					$"Timeline mask {mask} traversed {branchIndex} branching stages instead of {branchingStageCount}.");
			}

			Dictionary<string, int> verdictState = new Dictionary<string, int>(state);
			foreach (KeyValuePair<string, int> meter in meters)
			{
				verdictState[meter.Key] = meter.Value;
			}
			string[] ending = endings.FirstOrDefault(card => card[1] == currentStage && ConditionMatches(card[6], verdictState));
			if (ending == null)
			{
				throw new InvalidDataException($"Timeline mask {mask} has no matching ending.");
			}
			reachedEndings.Add(ending[2]);
			endingCounts[ending[2]]++;
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
		foreach (KeyValuePair<string, int> ending in endingCounts)
		{
			float share = (float)ending.Value / timelineCount;
			if (share < 0.05f || share > 0.4f)
			{
				throw new InvalidDataException(
					$"Ending {ending.Key} is selected in {share:P1} of timelines. Keep every verdict between 5% and 40%.");
			}
		}

		Debug.Log("HELM_BALANCE_VALID " +
			string.Join(" ", meterNames.Select(name => $"{name}={minimums[name]}..{maximums[name]}")) + " " +
			$"timelines={timelineCount} " +
			string.Join(" ", endingCounts.Select(pair => $"ending_{pair.Key}={pair.Value}")));
	}

	private static string ApplyOutcome(string outcome, Dictionary<string, int> meters, Dictionary<string, int> state)
	{
		string nextStage = null;
		foreach (string term in outcome.Split(new[] { " and " }, StringSplitOptions.RemoveEmptyEntries))
		{
			if (term.StartsWith(">", StringComparison.Ordinal))
			{
				nextStage = term.Substring(1);
				continue;
			}

			Match meter = Regex.Match(term, "^(power|oxygen|people|hull)([+-])(\\d+)$");
			if (meter.Success)
			{
				int delta = int.Parse(meter.Groups[3].Value);
				meters[meter.Groups[1].Value] += meter.Groups[2].Value == "+" ? delta : -delta;
				continue;
			}

			Match increment = Regex.Match(term, "^([a-z][a-z0-9_]*)([+-])(\\d+)$");
			if (increment.Success)
			{
				string name = increment.Groups[1].Value;
				int current = state.TryGetValue(name, out int value) ? value : 0;
				int delta = int.Parse(increment.Groups[3].Value);
				state[name] = current + (increment.Groups[2].Value == "+" ? delta : -delta);
				continue;
			}

			Match assignment = Regex.Match(term, "^([a-z][a-z0-9_]*)=(-?\\d+)$");
			if (assignment.Success)
			{
				state[assignment.Groups[1].Value] = int.Parse(assignment.Groups[2].Value);
				continue;
			}

			if (Regex.IsMatch(term, "^[a-z][a-z0-9_]*$") &&
				!term.StartsWith("mus_", StringComparison.Ordinal) &&
				!term.StartsWith("sfx_", StringComparison.Ordinal) &&
				!term.StartsWith("eff_", StringComparison.Ordinal))
			{
				state[term] = state.TryGetValue(term, out int value) ? value + 1 : 1;
			}
		}
		return nextStage;
	}

	private static bool ConditionMatches(string condition, Dictionary<string, int> state)
	{
		if (string.IsNullOrEmpty(condition))
		{
			return true;
		}

		foreach (string term in condition.Split(new[] { " and " }, StringSplitOptions.RemoveEmptyEntries))
		{
			Match comparison = Regex.Match(term, "^([a-z][a-z0-9_]*)([<>=])(-?\\d+)$");
			if (comparison.Success)
			{
				string name = comparison.Groups[1].Value;
				int defaultValue = name.StartsWith("nb_", StringComparison.Ordinal) ? 0 : -1;
				int value = state.TryGetValue(name, out int current) ? current : defaultValue;
				int target = int.Parse(comparison.Groups[3].Value);
				switch (comparison.Groups[2].Value)
				{
					case ">": if (value <= target) return false; break;
					case "<": if (value >= target) return false; break;
					case "=": if (value != target) return false; break;
				}
				continue;
			}

			if (!Regex.IsMatch(term, "^[a-z][a-z0-9_]*$") ||
				!state.TryGetValue(term, out int flag) || flag != 1)
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
