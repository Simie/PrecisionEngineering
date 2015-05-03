using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework.Plugins;
using UE=UnityEngine;

namespace PrecisionEngineering
{
	static class Debug
	{

		public const bool Enabled = true;

		private const string Prefix = "[PrecisionEngineering] ";

		public static void Log(string str)
		{
			var message = Prefix + str;
			DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, message);
			UE.Debug.Log(message);
		}

		public static void LogWarning(string str)
		{
			DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning, Prefix + str);
			UE.Debug.LogWarning(str);
		}

		public static void LogError(string str)
		{
			DebugOutputPanel.AddMessage(PluginManager.MessageType.Error, Prefix + str);
			UE.Debug.LogError(str);
		}

	}
}
