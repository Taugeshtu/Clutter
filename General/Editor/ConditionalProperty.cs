using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomPropertyDrawer( typeof( ConditionalHideAttribute ) )]
public class ConditionalPropertyDrawer : PropertyDrawer {
	
#region Implementation
	public override void OnGUI( Rect position, SerializedProperty property, GUIContent label ) {
		var conditionalHide = (ConditionalHideAttribute) attribute;
		var enabled = _GetConditionalVisible( conditionalHide, property );
		
		if( enabled ) {
			EditorGUI.PropertyField( position, property, label, true );
		}
	}
	
	public override float GetPropertyHeight( SerializedProperty property, GUIContent label ) {
		var conditionalHide = (ConditionalHideAttribute) attribute;
		var enabled = _GetConditionalVisible( conditionalHide, property );
		
		if( enabled ) {
			return EditorGUI.GetPropertyHeight( property, label );
		}
		
		//We want to undo the spacing added before and after the property
		return -EditorGUIUtility.standardVerticalSpacing;
	}
#endregion
	
	
#region Public
#endregion
	
	
#region Private
	private bool _GetConditionalVisible( ConditionalHideAttribute conditionalHide, SerializedProperty property ) {
		SerializedProperty sourcePropertyValue = null;
		
		//Get the full relative property path of the sourcefield so we can have nested hiding.Use old method when dealing with arrays
		if( !property.isArray ) {
			// returns the property path of the property we want to apply the attribute to
			var propertyPath = property.propertyPath;
			// changes the path to the conditionalsource property path
			var conditionPath = propertyPath.Replace( property.name, conditionalHide.Source );
			sourcePropertyValue = property.serializedObject.FindProperty( conditionPath );
			
			// if the find failed->fall back to the old system
			if( sourcePropertyValue == null ) {
				//original implementation (doens't work with nested serializedObjects)
				sourcePropertyValue = property.serializedObject.FindProperty( conditionalHide.Source );
			}
		}
		else {
			// original implementation (doens't work with nested serializedObjects)
			sourcePropertyValue = property.serializedObject.FindProperty( conditionalHide.Source );
		}
		
		if( sourcePropertyValue != null ) {
			var result = true;
			switch( sourcePropertyValue.propertyType ) {
				case SerializedPropertyType.Boolean:
					result = sourcePropertyValue.boolValue;
					break;
				case SerializedPropertyType.Enum:
					result = (sourcePropertyValue.enumValueIndex == conditionalHide.EnumIndex);
					break;
				default:
					Debug.LogError( "Data type of the property used for conditional hiding [" + sourcePropertyValue.propertyType + "] is currently not supported" );
					break;
			}
			
			if( conditionalHide.Inverted ) {
				result = !result;
			}
			return result;
		}
		
		return true;
	}
#endregion
	
	
#region Temporary
#endregion
}
