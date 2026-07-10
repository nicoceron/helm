using System;

[Serializable]
public class VO
{
	public Bearers type;

	public string[] samples;

	public VO()
	{
	}

	public VO(Bearers typ, string[] samp)
	{
		type = typ;
		samples = samp;
	}
}
