using System;
using System.Collections.Generic;
using System.IO;
using Prime31;
using UnityEngine;

public class AutomationLogger
{
	public readonly string[] GuitarNames = new string[8] { "star", "sleetar", "venusiel", "paraglider", "atomizz", "galaxy", "spectrum", "glitar" };

	public List<string> Debug_Selection;

	public List<string> Debug_RecordOutput;

	public List<CardDecisionRecord> Debug_Decisions;

	public List<IAutomationRecord> Log;

	public List<Card> CardsSwiped;

	public List<Card> CardsSelected;

	public List<Card> CardsDeaths;

	public string SessionName;

	private string m_sessionFolderPath;

	public AutomationLogger()
	{
		SessionName = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
		Log = new List<IAutomationRecord>();
		CardsSwiped = new List<Card>();
		CardsSelected = new List<Card>();
		CardsDeaths = new List<Card>();
		m_sessionFolderPath = Path.Combine(Application.persistentDataPath, SessionName);
		Directory.CreateDirectory(m_sessionFolderPath);
	}

	public void RecordDecision(Card card, AutomationController.CardSlideDirection decision)
	{
		CardsSwiped.Add(card);
		CardDecisionRecord item = new CardDecisionRecord
		{
			CardName = card.name,
			CardId = card.id,
			Decision = decision
		};
		Log.Add(item);
		WriteToFile();
	}

	public void RecordSelection(Card card)
	{
		CardSelectionRecord item = new CardSelectionRecord
		{
			CardName = card.place_name,
			CardId = card.id
		};
		CardsSelected.Add(card);
		Log.Add(item);
		WriteToFile();
	}

	public void RecordUIAction(string action)
	{
		UIRecord item = new UIRecord
		{
			Action = action
		};
		Log.Add(item);
		WriteToFile();
	}

	public void RecordDeath(Card card)
	{
		UIRecord item = new UIRecord
		{
			Action = "Death"
		};
		CardsDeaths.Add(card);
		Log.Add(item);
		WriteToFile();
	}

	public void WriteToFile()
	{
		List<Card> list = new List<Card>();
		list.AddRange(CardsSwiped);
		list.AddRange(CardsSelected);
		list.AddRange(CardsDeaths);
		using (FileStream stream = File.Create(Path.Combine(m_sessionFolderPath, "cards.txt")))
		{
			StreamWriter streamWriter = new StreamWriter(stream);
			foreach (Card item in list)
			{
				string text = item.name.TrimStart('_');
				string text2 = item.id.ToString();
				if (text == text2)
				{
					streamWriter.WriteLine(text2 ?? "");
				}
				else
				{
					streamWriter.WriteLine(text2 + " (" + text + ")");
				}
			}
			streamWriter.Flush();
		}
		using (FileStream stream2 = File.Create(Path.Combine(m_sessionFolderPath, "recap.txt")))
		{
			StreamWriter streamWriter2 = new StreamWriter(stream2);
			streamWriter2.WriteLine($"Cards swiped: {CardsSwiped.Count}");
			streamWriter2.WriteLine($"Cards selected: {CardsSelected.Count}");
			streamWriter2.WriteLine($"Deaths: {CardsDeaths.Count}");
			streamWriter2.Flush();
		}
		using (FileStream stream3 = File.Create(Path.Combine(m_sessionFolderPath, "guitars.txt")))
		{
			StreamWriter streamWriter3 = new StreamWriter(stream3);
			string[] guitarNames = GuitarNames;
			foreach (string text3 in guitarNames)
			{
				if (GameAct.diff.GetInt(text3) > 0)
				{
					streamWriter3.WriteLine(text3 ?? "");
				}
			}
			streamWriter3.Flush();
		}
		Json.jsonEncode(GameAct.diff.dataVar);
		using (FileStream stream4 = File.Create(Path.Combine(m_sessionFolderPath, "data_var.txt")))
		{
			StreamWriter streamWriter4 = new StreamWriter(stream4);
			foreach (DataVariable item2 in GameAct.diff.dataVar)
			{
				streamWriter4.WriteLine($"{item2.var}: {item2.val}");
			}
			streamWriter4.Flush();
		}
		using FileStream stream5 = File.Create(Path.Combine(m_sessionFolderPath, "data_custom.txt"));
		StreamWriter streamWriter5 = new StreamWriter(stream5);
		foreach (DataCustom item3 in GameAct.diff.dataCustom)
		{
			streamWriter5.WriteLine($"{item3.var}: {item3.val}");
		}
		streamWriter5.Flush();
	}
}
