using SVGImporter;
using UnityEngine;
using UnityEngine.UI;

public class EffectBox : MonoBehaviour
{
	public SVGImage icon;

	public Text title;

	public Text description;

	public Image[] outcoSlots;

	public Sprite imgChurch;

	public Sprite imgKing;

	public Sprite imgPeople;

	public Sprite imgIntrigue;

	private RectTransform thisrect;

	private string objId;

	private EffectAct scEf;

	private Effect effect;

	public void Init(Effect effect)
	{
		icon.vectorGraphics = (SVGAsset)Resources.Load("effects/" + effect.tag, typeof(SVGAsset));
		title.text = effect.title;
		description.text = effect.description;
	}

	private int SetData(Sprite img, float val, int n)
	{
		outcoSlots[n].gameObject.SetActive(value: true);
		outcoSlots[n].sprite = img;
		outcoSlots[n].GetComponentInChildren<Text>().text = ((val > 0f) ? ("+" + val) : val.ToString());
		n++;
		return n;
	}
}
