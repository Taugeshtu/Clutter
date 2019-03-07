using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor( typeof( Transform ), true )]
public class TransformInspector: Editor {
	public static TransformInspector s_Instance;
	
	private SerializedProperty m_cachedPosition;
	private SerializedProperty m_cachedRotation;
	private SerializedProperty m_cachedScale;
	
	private void OnEnable() {
		s_Instance = this;
		
		m_cachedPosition = serializedObject.FindProperty( "m_LocalPosition" );
		m_cachedRotation = serializedObject.FindProperty( "m_LocalRotation" );
		m_cachedScale = serializedObject.FindProperty( "m_LocalScale" );
	}
	
	private void OnDestroy() {
		s_Instance = null;
	}
	
	public override void OnInspectorGUI() {
		EditorGUIUtility.labelWidth = 15.0f;
		
		serializedObject.Update();
		
		_DrawPosition();
		_DrawRotation();
		_DrawScale();
		
		serializedObject.ApplyModifiedProperties();
	}
	
	private void _DrawPosition() {
		GUILayout.BeginHorizontal();
		var reset = GUILayout.Button( "P", GUILayout.Width( 20.0f ) );
		
		EditorGUILayout.PropertyField( m_cachedPosition.FindPropertyRelative( "x" ) );
		EditorGUILayout.PropertyField( m_cachedPosition.FindPropertyRelative( "y" ) );
		EditorGUILayout.PropertyField( m_cachedPosition.FindPropertyRelative( "z" ) );
		
		if( reset ) {
			m_cachedPosition.vector3Value = Vector3.zero;
		}
		GUILayout.EndHorizontal();
	}
	
	private void _DrawScale() {
		GUILayout.BeginHorizontal();
		var reset = GUILayout.Button( "S", GUILayout.Width( 20.0f ) );
		
		EditorGUILayout.PropertyField( m_cachedScale.FindPropertyRelative( "x" ) );
		EditorGUILayout.PropertyField( m_cachedScale.FindPropertyRelative( "y" ) );
		EditorGUILayout.PropertyField( m_cachedScale.FindPropertyRelative( "z" ) );
		
		if( reset ) {
			m_cachedScale.vector3Value = Vector3.one;
		}
		GUILayout.EndHorizontal();
	}
	
	// Rotation is ugly as hell... since there is no native support for quaternion property drawing
	private void _DrawRotation() {
		GUILayout.BeginHorizontal();
		
		var reset = GUILayout.Button( "R", GUILayout.Width( 20.0f ) );
		
		var visible = (serializedObject.targetObject as Transform).localEulerAngles;
		
		visible.x = _WrapAngle( visible.x );
		visible.y = _WrapAngle( visible.y );
		visible.z = _WrapAngle( visible.z );
		
		var changed = _CheckDifference( m_cachedRotation );
		var altered = Axes.None;
		
		var opt = GUILayout.MinWidth( 30.0f );
		
		if( _FloatField( "X", ref visible.x, ((changed & Axes.X) != 0), opt ) ) { altered |= Axes.X; }
		if( _FloatField( "Y", ref visible.y, ((changed & Axes.Y) != 0), opt ) ) { altered |= Axes.Y; }
		if( _FloatField( "Z", ref visible.z, ((changed & Axes.Z) != 0), opt ) ) { altered |= Axes.Z; }
		
		if( reset ) {
			m_cachedRotation.quaternionValue = Quaternion.identity;
		}
		else if( altered != Axes.None ) {
			_RegisterUndo( "Change Rotation", serializedObject.targetObjects );
			
			foreach( var obj in serializedObject.targetObjects ) {
				var t = (obj as Transform);
				var v = t.localEulerAngles;
				
				if( (altered & Axes.X) != 0 ) { v.x = visible.x; }
				if( (altered & Axes.Y) != 0 ) { v.y = visible.y; }
				if( (altered & Axes.Z) != 0 ) { v.z = visible.z; }
				
				t.localEulerAngles = v;
			}
		}
		
		GUILayout.EndHorizontal();
	}
	
	private enum Axes {
		None = 0,
		X = 1,
		Y = 2,
		Z = 4,
		All = 7
	}
	
	private Axes _CheckDifference( Transform transform, Vector3 original ) {
		var next = transform.localEulerAngles;
		var axes = Axes.None;
		if( _Differs( next.x, original.x ) ) {
			axes |= Axes.X;
		}
		if( _Differs( next.y, original.y ) ) {
			axes |= Axes.Y;
		}
		if( _Differs( next.z, original.z ) ) {
			axes |= Axes.Z;
		}
		return axes;
	}
	
	private Axes _CheckDifference( SerializedProperty property ) {
		var axes = Axes.None;
		
		if( property.hasMultipleDifferentValues ) {
			var original = property.quaternionValue.eulerAngles;
			foreach( var obj in serializedObject.targetObjects ) {
				axes |= _CheckDifference( obj as Transform, original );
				if( axes == Axes.All ) {
					break;
				}
			}
		}
		return axes;
	}
	
	private static bool _FloatField( string name, ref float a, bool hidden, GUILayoutOption option ) {
		var newValue = a;
		GUI.changed = false;
		
		if( hidden ) {
			float.TryParse( EditorGUILayout.TextField( name, "--", option ), out newValue );
		}
		else {
			newValue = EditorGUILayout.FloatField( name, newValue, option );
		}
		
		if( GUI.changed && _Differs( newValue, a ) ) {
			a = newValue;
			return true;
		}
		return false;
	}
	
	private static bool _Differs( float a, float b ) {
		return (Mathf.Abs( a - b ) > 0.0001f);
	}
	
	[System.Diagnostics.DebuggerHidden]
	[System.Diagnostics.DebuggerStepThrough]
	private static float _WrapAngle( float a ) {
		while( a > 180.0f ) {
			a -= 360.0f;
		}
		while( a < -180.0f ) {
			a += 360.0f;
		}
		return a;
	}
	
	private static void _RegisterUndo( string name, Object[] objects ) {
		if( objects == null ) {
			return;
		}
		if( objects.Length == 0 ) {
			return;
		}
		
		UnityEditor.Undo.RecordObjects( objects, name );
		foreach( var obj in objects ) {
			if( obj != null ) {
				EditorUtility.SetDirty( obj );
			}
		}
	}
}
