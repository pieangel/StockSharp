using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecng.Logging;

namespace SciTrader.Services
{
	public class LogService
	{
		private readonly LogManager _logManager;

		public LogService(LogManager logManager)
		{
			_logManager = logManager;
		}

		public void LogInfo(string message)
		{
			_logManager.Application.LogInfo(message);
			Console.WriteLine($"[INFO] {message}"); // Optional console logging
		}

		public void LogError(string message)
		{
			_logManager.Application.LogError(message);
			Console.WriteLine($"[ERROR] {message}");
		}
	}

}
