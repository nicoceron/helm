public class AllObjectiveBox : ContentBox
{
	private ObjectiveAct scOb;

	private bool isready;

	public override void Validate()
	{
	}

	public override void Init(object instance, string txtid, bool trig, bool stayHidden = false)
	{
		scOb = CardReader.diff.GetComponent<ObjectiveAct>();
		scOb.ShowObjectives(base.transform, -20, replace: true, thenupdate: true);
	}

	public void ForceStop()
	{
		ModalAct.diff.ForceClose();
	}

	public override void Close()
	{
		isready = true;
		scOb.DestroyBoxes();
	}

	private bool YieldStart(bool none)
	{
		return isready;
	}
}
