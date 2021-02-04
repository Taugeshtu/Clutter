using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR

[CustomPropertyDrawer( typeof( Vector2b ) )]
class Vector2bDrawer : PropertyDrawer {
	// Draw the property inside the given rect
	public override void OnGUI( Rect position, SerializedProperty property, GUIContent label ) {
		EditorGUI.BeginProperty( position, label, property );
		
		position = EditorGUI.PrefixLabel( position, GUIUtility.GetControlID( FocusType.Keyboard ), label );
		
		// Don't make child fields be indented
		var indent = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;
		
		// Calculate rects
		
		
		
		// Draw fields - passs GUIContent.none to each so they are drawn without labels
		var labelWidth = 12;
		var inputWidth = 50;
		var spacing = 3;
		
		var xRect = new Rect( position.x, position.y, labelWidth, position.height );
		EditorGUI.LabelField( xRect, "X" );
		xRect = new Rect( position.x +labelWidth, position.y, inputWidth, position.height );
		EditorGUI.PropertyField( xRect, property.FindPropertyRelative( "x" ), GUIContent.none );
		
		var yRect = new Rect( position.x + (labelWidth + inputWidth + spacing), position.y, labelWidth, position.height );
		EditorGUI.LabelField( yRect, "Y" );
		yRect = new Rect( position.x + (labelWidth + inputWidth + spacing) +labelWidth, position.y, inputWidth, position.height );
		EditorGUI.PropertyField( yRect, property.FindPropertyRelative( "y" ), GUIContent.none );
		
		EditorGUI.indentLevel = indent;
		EditorGUI.EndProperty();
	}
}

#endif

