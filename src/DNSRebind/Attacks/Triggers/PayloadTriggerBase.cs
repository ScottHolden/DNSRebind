using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace RebindMeta
{
	public abstract class PayloadTriggerBase
	{
		protected readonly HttpClient _attackClient;
		public PayloadTriggerBase(Uri targetHost)
		{
			_attackClient = new HttpClient()
			{
				BaseAddress = targetHost
			};
		}

		public abstract Task<string> TriggerAsync(string hostname, AttackTarget target);
	}
}
