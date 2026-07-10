public class iOSHapticFeedbackAdvanced : iOSHapticFeedback
{
	protected override void Awake()
	{
		base.Awake();
	}

	public override void Trigger(iOSFeedbackType feedbackType)
	{
		TriggerFeedbackGenerator((int)feedbackType, advanced: true);
	}

	public void InstantiateFeedbackGenerator(iOSFeedbackType feedbackType)
	{
		InstantiateFeedbackGenerator((int)feedbackType);
	}

	public void PrepareFeedbackGenerator(iOSFeedbackType feedbackType)
	{
		PrepareFeedbackGenerator((int)feedbackType);
	}

	public void TriggerFeedbackGenerator(iOSFeedbackType feedbackType)
	{
		Trigger(feedbackType);
	}

	public void ReleaseFeedbackGenerator(iOSFeedbackType feedbackType)
	{
		ReleaseFeedbackGenerator((int)feedbackType);
	}
}
