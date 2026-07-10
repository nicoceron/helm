using System;

[Serializable]
public class CardSave
{
	public int nt;

	public int we;

	public bool lo;

	public bool se;

	public int wr;

	public CardSave(int next, int wei, bool locks, bool see, int real)
	{
		nt = next;
		we = wei;
		lo = locks;
		se = see;
		wr = real;
	}
}
