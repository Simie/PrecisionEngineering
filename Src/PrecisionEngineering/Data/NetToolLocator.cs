using System;
using System.Linq;
using Object = UnityEngine.Object;

namespace PrecisionEngineering.Data
{
    /// <summary>
    /// Helper class for locating the NetTool.
    /// </summary>
    public static class NetToolLocator
    {
        /// <summary>
        /// Attempt to locate the NetTool and create a NetToolProxy object if successful.
        /// </summary>
        /// <returns>A <c>NetToolProxy</c> object if successful, otherwise null.</returns>
        public static NetToolProxy Locate()
        {
            NetToolProxy p = null;

            // Hack to include FineRoadHeights. If more mods start replacing the NetTool
            // it might be wise to implement an interface system like in Road Protractor
            if (AppDomain.CurrentDomain.GetAssemblies().Any(q => q.FullName.Contains("FineRoadHeights")))
            {
                Debug.Log("Locating FineRoadHeights tool...");

                var t = Type.GetType("NetToolFine, FineRoadHeights");
                ToolBase instance;

                if (t != null && (instance = Object.FindObjectOfType(t) as ToolBase) != null &&
                    (p = NetToolProxy.Create(instance)) != null)
                {
                    Debug.Log("Success!");
                    return p;
                }

                Debug.Log("Not found, falling back to default NetTool.");
            }

            var tool = Object.FindObjectOfType<NetTool>();

            if (tool != null)
            {
                p = NetToolProxy.Create(tool);
            }

            return p;
        }
    }
}
