using DNS.Client.RequestResolver;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using DNS.Server;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace RebindMeta
{
	public class RebindDnsServer : IRequestResolver
	{
		private readonly DnsServer _dnsServer;
		private readonly IPAddress _bindIp;
		private readonly IPAddress _publicIp;
		private readonly string _dnsSuffix;
		public RebindDnsServer(string bindIp, string publicIp, string dnsSuffix)
		{
			_publicIp = IPAddress.Parse(publicIp);
			_bindIp = IPAddress.Parse(bindIp);
			_dnsSuffix = "." + dnsSuffix.TrimStart('.');

			_dnsServer = new DnsServer(this);
			_dnsServer.Errored += (sender, e) => Logger.WriteLine("DNS Server Error: " + e.Exception.Message);
		}

		public Task RunAsync() => _dnsServer.Listen(53, _bindIp);

		public Dictionary<string, IPAddress> RemappedDomains = new Dictionary<string, IPAddress>();
		
		public void AddRemap(string host, string ip)
			=> RemappedDomains.Add(host.ToLower().Trim(), IPAddress.Parse(ip));
		public bool CheckRemap(string host)
			=> RemappedDomains.ContainsKey(host.ToLower().Trim());

		public string GetSubdomain(string part)
			=> part + _dnsSuffix;

		Task<IResponse> IRequestResolver.Resolve(IRequest request, CancellationToken cancellationToken = default)
		{
			IResponse response = Response.FromRequest(request);

			foreach (Question question in response.Questions)
			{
				string host = question.Name.ToString().ToLower().Trim();

				if (question.Type == RecordType.A && host.EndsWith(_dnsSuffix))
				{
					IPAddress ip = RemappedDomains.ContainsKey(host) ?
														RemappedDomains[host] :
														_publicIp;
					IResourceRecord record = new IPAddressResourceRecord(
													question.Name,
													ip,
													TimeSpan.FromSeconds(10));
					response.AnswerRecords.Add(record);
					Logger.WriteLine("!!! DNS: " + question.Name.ToString() + " -> " + ip.ToString());
				}
			}

			return Task.FromResult(response);
		}
	}
}
