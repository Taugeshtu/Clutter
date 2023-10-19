using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer( typeof( BaseSharedConfig ), true )]
public class SharedConfigDrawer : PropertyDrawer {
	private float _currentHeight = EditorGUIUtility.singleLineHeight;
	
	public override float GetPropertyHeight( SerializedProperty property, GUIContent label ) {
		return _currentHeight;
	}
	
	public override void OnGUI( Rect position, SerializedProperty property, GUIContent label ) {
		// Draw config reference field
		var labelRect = position;
		labelRect.height = EditorGUIUtility.singleLineHeight;
		EditorGUI.BeginProperty( position, label, property );
		EditorGUI.PropertyField( labelRect, property, label, false );
		EditorGUI.EndProperty();
		_currentHeight = EditorGUIUtility.singleLineHeight;
		
		if( property.objectReferenceValue != null ) {
			var nestedHeight = _DrawNestedConfig( property, position );
			_currentHeight = nestedHeight;
		}
	}
	
	private static float _DrawNestedConfig( SerializedProperty property, Rect position ) {
		var serializedObject = new SerializedObject( property.objectReferenceValue );
		var nestedProperty = serializedObject.FindProperty( "Config" );
		
		var nestedRect = position;
		// nestedRect.width = EditorGUIUtility.labelWidth;
		var nestedHeight = EditorGUI.GetPropertyHeight( nestedProperty, GUIContent.none, true );
		// EditorGUI.DrawRect( nestedRect, Color.black.WithA( 0.5f ) );
		
		EditorGUI.BeginProperty( nestedRect, GUIContent.none, nestedProperty );
		EditorGUI.PropertyField( nestedRect, nestedProperty, GUIContent.none, true );
		EditorGUI.EndProperty();
		
		serializedObject.ApplyModifiedProperties();
		return nestedHeight;
	}
}
