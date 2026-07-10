namespace Rewired
{
	public static class UnityInputOverride
	{
		private static bool _enabled = true;

		private static int _playerId = 0;

		public static bool enabled
		{
			get
			{
				return _enabled;
			}
			set
			{
				_enabled = value;
			}
		}

		public static int playerId
		{
			get
			{
				return _playerId;
			}
			set
			{
				if (ReInput.isReady && ReInput.players.GetPlayer(value) != null)
				{
					_playerId = value;
				}
			}
		}
	}
}
