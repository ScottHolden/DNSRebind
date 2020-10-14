using System;
using System.IO;

namespace RebindMeta
{
	// TODO: Proper logger, this is kinda messy
	public static class Logger
	{
		private static readonly StreamWriter _output;
		static Logger()
		{
			_output = File.AppendText("log.txt");
		}
		private static bool _lastWrite = false;
		public static void WriteLine(string thing)
		{
			thing = (_lastWrite ? "\n" : "") + DateTime.Now.ToString() + ": " + thing;
			_lastWrite = false;
			Console.WriteLine(thing);
			_output.WriteLine(thing);
			_output.Flush();
		}
		public static void Write(string thing)
		{
			_lastWrite = true;
			Console.Write(thing);
			_output.Write(thing);
		}
	}
}