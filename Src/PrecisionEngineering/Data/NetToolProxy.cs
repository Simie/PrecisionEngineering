using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Remoting.Messaging;

namespace PrecisionEngineering.Data
{

	/// <summary>
	/// Wrapper around NetTool to expose private properties so we can perform calculations with
	/// the control point data.
	/// </summary>
	public class NetToolProxy
	{

		public bool IsValid { get { return _target != null; } }

		public bool IsEnabled { get { return _target.enabled; } }


		public bool IsSnappingEnabled
	    {
	        get { return (bool) _snapField.GetValue(_target); }
	        set { _snapField.SetValue(_target, value); }
	    }

		public NetTool.Mode Mode
		{
			get { return (NetTool.Mode) _modeField.GetValue(_target); }
		}

		public ToolBase.ToolErrors BuildErrors
		{
			get { return _target.GetErrors(); }
		}

		public int ControlPointsCount { get { return (int)_controlPointCountField.GetValue(_target); } }

		public IList<NetTool.ControlPoint> ControlPoints
		{
			get { return (IList<NetTool.ControlPoint>) _controlPointsField.GetValue(_target); }
		}

		public FastList<NetTool.NodePosition> NodePositions
		{
			get { return NetTool.m_nodePositionsMain; }
		}

		public NetInfo NetInfo
		{
			get { return (NetInfo) _netInfoField.GetValue(_target); }
		}

		public ToolController ToolController
		{
			get { return (ToolController)_toolControllerField.GetValue(_target); }
		}

		private readonly ToolBase _target;

		private readonly FieldInfo _controlPointCountField;
		private readonly FieldInfo _controlPointsField;
		private readonly FieldInfo _toolControllerField;
		private readonly FieldInfo _netInfoField;
		private readonly FieldInfo _snapField;
		private readonly FieldInfo _modeField;

	    public static NetToolProxy Create(ToolBase target)
	    {

	        NetToolProxy p;

	        try {

	            p = new NetToolProxy(target);
	            return p;

	        } catch (Exception e) {
	            
                Debug.LogError(string.Format("Error acquiring NetToolProxy from object {0}", target));
                Debug.LogError(e.ToString());
                
	        }

	        return null;

	    }

		private NetToolProxy(ToolBase target)
		{

			_target = target;

		    var t = target.GetType();

            _controlPointCountField = GetPrivateField(t, "m_controlPointCount");
			_controlPointsField = GetPrivateField(t, "m_controlPoints");
			_toolControllerField = GetPrivateField(t, "m_toolController");
			_netInfoField = GetPublicField(t, "m_prefab");
			_snapField = GetPublicField(t, "m_snap");
			_modeField = GetPublicField(t, "m_mode");

		}

		private static FieldInfo GetPrivateField(Type t, string name)
		{
			var f = t.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);

			if(f == null)
                throw new Exception(string.Format("Error getting private field: {0} on type {1}", name, t));

			return f;
		}

		private static FieldInfo GetPublicField(Type t, string name)
		{
			var f = t.GetField(name, BindingFlags.Public | BindingFlags.Instance);

			if(f == null)
                throw new Exception(string.Format("Error getting public field: {0} on type {1}", name, t));

            return f;
		}

	}
}
