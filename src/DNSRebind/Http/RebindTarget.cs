using System.Collections.Generic;

namespace RebindMeta
{
	public class RebindTarget
	{
		public string RebindIP { get; }
		public string Path { get; }
		public Dictionary<string, string> Headers { get; }

		public RebindTarget(string rebindIP, string path)
			: this (rebindIP, path, new Dictionary<string, string>())
		{

		}
		public RebindTarget(string rebindIP, string path, Dictionary<string, string> headers)
		{
			RebindIP = rebindIP;
			Path = path;
			Headers = headers;
		}
	}
}