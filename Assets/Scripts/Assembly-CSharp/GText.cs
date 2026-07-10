using System;

[Serializable]
public class GText
{
	public Genres genre;

	public bool isEmpty;

	public string mMsM;

	public string mFsM;

	public string mFsF;

	public string mMsF;

	public GText()
	{
	}

	public GText(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			isEmpty = true;
			return;
		}
		bool flag = input.Contains("*");
		bool flag2 = false;
		genre = ((flag && flag2) ? Genres.all : ((flag && !flag2) ? Genres.mona : ((!flag && flag2) ? Genres.self : Genres.univ)));
		if (genre == Genres.univ)
		{
			mMsM = input;
			return;
		}
		string[] array = TreatInput(input, '*', '(', ')');
		mMsM = "";
		mFsM = "";
		mFsF = "";
		mMsF = "";
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = new string[1] { array[i] };
			if (array2.Length == 4)
			{
				if (i != 2)
				{
					mMsM = mMsM + array2[0] + array2[1] + array2[3];
				}
				if (flag2 && i != 2)
				{
					mMsF = mMsF + array2[0] + array2[2] + array2[3];
				}
				if (flag && i != 1)
				{
					mFsM = mFsM + array2[0] + array2[1] + array2[3];
				}
				if (flag2 && flag && i != 1)
				{
					mFsF = mFsF + array2[0] + array2[2] + array2[3];
				}
			}
			else
			{
				if (i != 2)
				{
					mMsM += array[i];
				}
				if (flag2 && i != 2)
				{
					mMsF += array[i];
				}
				if (flag && i != 1)
				{
					mFsM += array[i];
				}
				if (flag && flag2 && i != 1)
				{
					mFsF += array[i];
				}
			}
		}
	}

	public GText TreatName(Bearers reference, string seed)
	{
		Bearer bearer = CardReader.diff.bearerModels.Find((Bearer it) => it.bearer == reference);
		mMsM = SpeechAct.diff.GenericName(bearer.name, seed);
		if (!string.IsNullOrEmpty(mMsF))
		{
			mMsF = SpeechAct.diff.GenericName(bearer.name, seed);
		}
		if (!string.IsNullOrEmpty(mFsF))
		{
			mFsF = SpeechAct.diff.GenericName(bearer.name, seed);
		}
		if (!string.IsNullOrEmpty(mFsM))
		{
			mFsM = SpeechAct.diff.GenericName(bearer.name, seed);
		}
		return this;
	}

	public GText TreatName()
	{
		if (isEmpty)
		{
			return this;
		}
		mMsM = SpeechAct.diff.GenericName(mMsM);
		if (!string.IsNullOrEmpty(mMsF))
		{
			mMsF = SpeechAct.diff.GenericName(mMsF);
		}
		if (!string.IsNullOrEmpty(mFsF))
		{
			mFsF = SpeechAct.diff.GenericName(mFsF);
		}
		if (!string.IsNullOrEmpty(mFsM))
		{
			mFsM = SpeechAct.diff.GenericName(mFsM);
		}
		return this;
	}

	public string Get()
	{
		return Get(SpeechAct.diff.isMonarkMale, SpeechAct.diff.isSelfMale);
	}

	public string Get(bool maleKing, bool maleSelf)
	{
		switch (genre)
		{
		case Genres.mona:
			if (!maleKing)
			{
				return mFsM;
			}
			return mMsM;
		case Genres.self:
			if (!maleSelf)
			{
				return mMsF;
			}
			return mMsM;
		case Genres.all:
			if (maleKing)
			{
				if (!maleSelf)
				{
					return mMsF;
				}
				return mMsM;
			}
			if (!maleSelf)
			{
				return mFsF;
			}
			return mFsM;
		default:
			return mMsM;
		}
	}

	private string[] TreatInput(string input, char delim, char start, char end)
	{
		if (!input.Contains(delim.ToString()))
		{
			return new string[1] { input };
		}
		string[] array = new string[4] { "", "", "", "" };
		string[] array2 = input.Split(delim);
		string[] array3 = array2[0].Split(start);
		string[] array4 = array2[1].Split(end);
		if (array3.Length == 1)
		{
			array[1] = array3[0];
		}
		else
		{
			array[0] = array3[0];
			array[1] = array3[1];
		}
		if (array4.Length == 1)
		{
			array[2] = array4[0];
		}
		else
		{
			array[2] = array4[0];
			array[3] = array4[1];
		}
		return array;
	}
}
