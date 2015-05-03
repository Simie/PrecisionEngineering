using System.Collections.Generic;
using System.Reflection;

namespace PrecisionEngineering.Data
{

	/// <summary>
	/// Wrapper around NetTool to expose private properties so we can calculate for our GUI
	/// </summary>
	public class NetToolProxy
	{

		public bool IsEnabled { get { return _target.enabled; } }

		public bool IsSnappingEnabled { get { return _target.m_snap; } }

		public NetTool.Mode Mode { get { return _target.m_mode; } }

		public ToolBase.ToolErrors BuildErrors
		{
			get { return _target.GetErrors(); }
		}

		public int ControlPointsCount { get { return (int)_controlPointCountField.GetValue(_target); } }

		public IList<NetTool.ControlPoint> ControlPoints
		{
			get { return _controlPoints; }
		}

		public FastList<NetTool.NodePosition> NodePositions
		{
			get { return NetTool.m_nodePositionsMain; }
		}


		private NetTool _target;

		private NetTool.ControlPoint[] _controlPoints;

		private readonly FieldInfo _controlPointCountField;
		private readonly FieldInfo _buildErrorsFieldInfo;

		public NetToolProxy(NetTool target)
		{

			_target = target;

			_controlPointCountField = GetPrivateField("m_controlPointCount");
			_buildErrorsFieldInfo = GetPrivateField("m_buildErrors");

			Debug.Log("Loading Control Points");
			_controlPoints =
				(NetTool.ControlPoint[])
					(typeof (NetTool).GetField("m_controlPoints", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(target));


		}

		private static FieldInfo GetPrivateField(string name)
		{
			var f = typeof (NetTool).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);

			if(f == null)
				Debug.LogError(string.Format("Error getting field: {0}", name));

			return f;
		}

	}
}
