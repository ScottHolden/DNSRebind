namespace RebindMeta
{
	public class AttackTarget
	{
		public int Port { get; }
		public RebindTarget RebindTarget { get; }

		public AttackTarget(int port, RebindTarget target)
		{
			Port = port;
			RebindTarget = target;
		}

		public AttackTarget WithPort(int port)
			=> new AttackTarget(port, RebindTarget);
	}
}
