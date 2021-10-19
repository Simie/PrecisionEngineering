using System;
using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;

namespace PrecisionEngineering.Data
{
    /// <summary>
    /// Helper class for locating the NetTool.
    /// </summary>
    public static class NetToolLocator
    {
        private static readonly List<int> Cache = new List<int>(); 

        /// <summary>
        /// Attempt to locate the NetTool and create a NetToolProxy object if successful.
        /// </summary>
        /// <returns>A <c>NetToolProxy</c> object if successful, otherwise null.</returns>
        public static NetToolProxy Locate()
        {
            var toolType = typeof (NetTool);

            // Hack to include FineRoadHeights. If more mods start replacing the NetTool
            // it might be wise to implement an interface system like in Road Protractor
            if (AppDomain.CurrentDomain.GetAssemblies().Any(q => q.FullName.Contains("FineRoadHeights")))
            {
                toolType = Type.GetType("NetToolFine, FineRoadHeights") ?? toolType;
            }

            Debug.Log($"Looking for NetTool of type `{toolType}`");

            var tools = Object.FindObjectsOfType(toolType).Cast<ToolBase>().ToList();

            if (tools.Count == 0 && toolType != typeof(NetTool))
            {
                Debug.Log($"Falling back to default NetTool");
                toolType = typeof(NetTool);
                tools = Object.FindObjectsOfType(toolType).Cast<ToolBase>().ToList();
            }

            Debug.Log($"Found Tools ({tools.Count}): " + string.Join(", ", tools.Select(q => q.name + $" ({q.GetInstanceID()})").ToArray()));

            if (tools.Count == 0)
            {
                Debug.LogError("Could not find NetTool");
                return null;
            }

            if (tools.Count > 1)
            {
                // May other mods use NetTool as base for creating new tools (like Upgrade Untouchable) - so try to get exact class, once it's used by the game as default net tool
                Debug.Log($"Multiple NetTool instances found, trying to get exact type member.");
                tools = tools.Where(p => p.GetType() == toolType).ToList();
            }

            if (tools.Count > 1)
            {
                // The "First" NetTool created is the one we want, since it's the one Unity seems to use.
                Debug.Log($"Multiple NetTool instances found, using cache to filter to an already known one.");
                tools = tools.Where(p => Cache.Contains(p.GetInstanceID())).ToList();
            }

            if (tools.Count > 1)
            {
                Debug.LogWarning("Still more than 1 NetTool instance found. Using the last one in the hopes it's the right one...");
            }

            var tool = tools.LastOrDefault();

            if (tool == null)
            {
                Debug.LogError("Failed to find NetTool");
                return null;
            }

            if(!Cache.Contains(tool.GetInstanceID()))
                Cache.Add(tool.GetInstanceID());

            return NetToolProxy.Create(tool);
        }
    }
}
