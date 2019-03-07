using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

#if UNITY_EDITOR
namespace SkeletoolInner {

[CustomPropertyDrawer( typeof( SkeletoolPose ) )]
class SkeletoolStateDrawer : PropertyDrawer {
	// Draw the property inside the given rect
	public override void OnGUI( Rect position, SerializedProperty property, GUIContent label ) {
		EditorGUI.BeginProperty( position, label, property );
		
		position = EditorGUI.PrefixLabel( position, GUIUtility.GetControlID( FocusType.Keyboard ), label );
		
		// Don't make child fields be indented
		var indent = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;
		
		// Draw fields - passs GUIContent.none to each so they are drawn without labels
		var labelWidth = 40;
		var inputWidth = 70;
		var spacing = 3;
		
		var xRect = new Rect( position.x, position.y, labelWidth, position.height );
		EditorGUI.LabelField( xRect, "Name" );
		xRect = new Rect( position.x +labelWidth, position.y, inputWidth, position.height );
		EditorGUI.PropertyField( xRect, property.FindPropertyRelative( "Name" ), GUIContent.none );
		
		var yRect = new Rect( position.x + labelWidth + inputWidth + spacing, position.y, labelWidth, position.height );
		EditorGUI.LabelField( yRect, "Root" );
		yRect = new Rect( position.x + labelWidth + inputWidth + spacing + labelWidth, position.y, inputWidth, position.height );
		EditorGUI.PropertyField( yRect, property.FindPropertyRelative( "Root" ), GUIContent.none );
		
		EditorGUI.indentLevel = indent;
		EditorGUI.EndProperty();
	}
}

}
#endif
