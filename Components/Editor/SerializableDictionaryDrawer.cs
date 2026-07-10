using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(SerializableDictionary<,>))]
public class SerializableDictionaryDrawer : PropertyDrawer
{
    private static Dictionary<string, bool> _foldoutStates = new Dictionary<string, bool>();
    private const float FOLDOUT_HEIGHT = 18f;
    private const float ENTRY_HEIGHT = 20f;
    private const float SPACING = 2f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var keysProperty = property.FindPropertyRelative("keys");
        var valuesProperty = property.FindPropertyRelative("values");
        
        if (keysProperty == null || valuesProperty == null)
            return EditorGUIUtility.singleLineHeight;

        float height = EditorGUIUtility.singleLineHeight; // Main foldout
        
        string foldoutKey = GetFoldoutKey(property);
        bool isExpanded = _foldoutStates.ContainsKey(foldoutKey) ? _foldoutStates[foldoutKey] : false;
        
        if (isExpanded)
        {
            int count = keysProperty.arraySize;
            for (int i = 0; i < count; i++)
            {
                var keyProp = keysProperty.GetArrayElementAtIndex(i);
                var valueProp = valuesProperty.GetArrayElementAtIndex(i);
                
                float keyHeight = EditorGUI.GetPropertyHeight(keyProp, GUIContent.none, true);
                float valueHeight = EditorGUI.GetPropertyHeight(valueProp, GUIContent.none, true);
                float entryHeight = Mathf.Max(keyHeight, valueHeight);
                
                height += entryHeight + SPACING;
            }
            
            // Add button height
            height += EditorGUIUtility.singleLineHeight + SPACING;
        }
        
        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        var keysProperty = property.FindPropertyRelative("keys");
        var valuesProperty = property.FindPropertyRelative("values");
        
        if (keysProperty == null || valuesProperty == null)
        {
            EditorGUI.LabelField(position, label.text, "SerializableDictionary error");
            EditorGUI.EndProperty();
            return;
        }

        string foldoutKey = GetFoldoutKey(property);
        bool isExpanded = _foldoutStates.ContainsKey(foldoutKey) ? _foldoutStates[foldoutKey] : false;
        
        // Main foldout
        Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        string displayLabel = $"{label.text} (Count: {keysProperty.arraySize})";
        isExpanded = EditorGUI.Foldout(foldoutRect, isExpanded, displayLabel, true);
        _foldoutStates[foldoutKey] = isExpanded;
        
        if (isExpanded)
        {
            EditorGUI.indentLevel++;
            float currentY = position.y + EditorGUIUtility.singleLineHeight + SPACING;
            
            // Draw each key-value pair
            for (int i = 0; i < keysProperty.arraySize; i++)
            {
                var keyProp = keysProperty.GetArrayElementAtIndex(i);
                var valueProp = valuesProperty.GetArrayElementAtIndex(i);
                
                float keyHeight = EditorGUI.GetPropertyHeight(keyProp, GUIContent.none, true);
                float valueHeight = EditorGUI.GetPropertyHeight(valueProp, GUIContent.none, true);
                float entryHeight = Mathf.Max(keyHeight, valueHeight);
                
                Rect entryRect = new Rect(position.x, currentY, position.width, entryHeight);
                DrawKeyValuePair(entryRect, keyProp, valueProp, i, keysProperty, valuesProperty);
                
                currentY += entryHeight + SPACING;
            }
            
            // Add/Remove buttons
            Rect buttonRect = new Rect(position.x, currentY, position.width, EditorGUIUtility.singleLineHeight);
            DrawButtons(buttonRect, keysProperty, valuesProperty);
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUI.EndProperty();
    }
    
    private void DrawKeyValuePair(Rect rect, SerializedProperty keyProp, SerializedProperty valueProp, int index, SerializedProperty keysArray, SerializedProperty valuesArray)
    {
        float buttonWidth = 20f;
        float spacing = 5f;
        float availableWidth = rect.width - buttonWidth - spacing;
        float keyWidth = availableWidth * 0.4f;
        float valueWidth = availableWidth * 0.6f;
        
        // Key field
        Rect keyRect = new Rect(rect.x, rect.y, keyWidth, rect.height);
        EditorGUI.PropertyField(keyRect, keyProp, GUIContent.none, true);
        
        // Value field
        Rect valueRect = new Rect(rect.x + keyWidth + spacing, rect.y, valueWidth, rect.height);
        EditorGUI.PropertyField(valueRect, valueProp, GUIContent.none, true);
        
        // Remove button
        Rect removeRect = new Rect(rect.x + availableWidth + spacing, rect.y, buttonWidth, EditorGUIUtility.singleLineHeight);
        if (GUI.Button(removeRect, "×"))
        {
            keysArray.DeleteArrayElementAtIndex(index);
            valuesArray.DeleteArrayElementAtIndex(index);
        }
    }
    
    private void DrawButtons(Rect rect, SerializedProperty keysArray, SerializedProperty valuesArray)
    {
        float buttonWidth = 60f;
        float spacing = 5f;
        
        Rect addRect = new Rect(rect.x, rect.y, buttonWidth, rect.height);
        if (GUI.Button(addRect, "Add"))
        {
            keysArray.arraySize++;
            valuesArray.arraySize++;
            
            // Initialize new entries to avoid null references
            var newKeyProp = keysArray.GetArrayElementAtIndex(keysArray.arraySize - 1);
            var newValueProp = valuesArray.GetArrayElementAtIndex(valuesArray.arraySize - 1);
            
            // Reset to default values to avoid carrying over previous values
            if (newKeyProp.propertyType == SerializedPropertyType.ObjectReference)
                newKeyProp.objectReferenceValue = null;
            if (newValueProp.propertyType == SerializedPropertyType.ObjectReference)
                newValueProp.objectReferenceValue = null;
        }
        
        Rect clearRect = new Rect(rect.x + buttonWidth + spacing, rect.y, buttonWidth, rect.height);
        if (GUI.Button(clearRect, "Clear"))
        {
            keysArray.arraySize = 0;
            valuesArray.arraySize = 0;
        }
    }
    
    private string GetFoldoutKey(SerializedProperty property)
    {
        return property.serializedObject.targetObject.GetInstanceID() + "." + property.propertyPath;
    }
}