using System;
using System.Collections.Generic;

public class OverallSave
{
	public int cloneNb = 1;

	public DateTime time;

	public List<JourneySave> journeys;

	public string language;

	public OverallSave()
	{
		time = DateTime.UtcNow;
		language = SpeechAct.diff.lang;
	}
}
