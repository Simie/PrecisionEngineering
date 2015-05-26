using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrecisionEngineering.Data
{
	public static class NetToolLocator
	{

		public static NetToolProxy Locate()
		{

			NetToolProxy p = null;

			// Hack to include FineRoadHeights. If more mods start replacing the NetTool
			// it might be wise to implement an interface system like in Road Protractor
			if (AppDomain.CurrentDomain.GetAssemblies().Any(q => q.FullName.Contains("FineRoadHeights"))) {

				Debug.Log("Locating FineRoadHeights tool...");

				var t = Type.GetType("NetToolFine, FineRoadHeights");
				ToolBase instance;

				if (t != null && (instance = UnityEngine.Object.FindObjectOfType(t) as ToolBase) != null &&
				    (p = NetToolProxy.Create(instance)) != null) {

					Debug.Log("Success!");
					return p;

				}

				Debug.Log("Not found, falling back to default NetTool.");

			}

			var tool = UnityEngine.Object.FindObjectOfType<NetTool>();

			if(tool != null)
				p = NetToolProxy.Create(tool);

			return p;

		}

	}
}
