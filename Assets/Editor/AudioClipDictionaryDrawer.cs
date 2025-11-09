#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomPropertyDrawer(typeof(AudioClipDictionary))]
public class AudioClipDictionaryDrawer : PropertyDrawer
{
    private ReorderableList list;
    
    private ReorderableList GetList(SerializedProperty property)
    {
        if (list == null)
        {
            var keysProperty = property.FindPropertyRelative("keys");
            var valuesProperty = property.FindPropertyRelative("values");
            
            list = new ReorderableList(property.serializedObject, keysProperty, true, true, true, true);
            
            // Draw header
            list.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width * 0.4f, rect.height), "Key");
                EditorGUI.LabelField(new Rect(rect.x + rect.width * 0.45f, rect.y, rect.width * 0.5f, rect.height), "Audio Clip");
            };
            
            // Draw each element
            list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                rect.y += 2;
                float keyWidth = rect.width * 0.4f;
                float valueWidth = rect.width * 0.55f;
                float spacing = 5f;
                
                var keyElement = keysProperty.GetArrayElementAtIndex(index);
                var valueElement = valuesProperty.GetArrayElementAtIndex(index);
                
                EditorGUI.PropertyField(
                    new Rect(rect.x, rect.y, keyWidth, EditorGUIUtility.singleLineHeight),
                    keyElement,
                    GUIContent.none
                );
                
                EditorGUI.PropertyField(
                    new Rect(rect.x + keyWidth + spacing, rect.y, valueWidth, EditorGUIUtility.singleLineHeight),
                    valueElement,
                    GUIContent.none
                );
            };
            
            // Add callback
            list.onAddCallback = (ReorderableList l) =>
            {
                keysProperty.arraySize++;
                valuesProperty.arraySize++;
                
                var newKey = keysProperty.GetArrayElementAtIndex(keysProperty.arraySize - 1);
                var newValue = valuesProperty.GetArrayElementAtIndex(valuesProperty.arraySize - 1);
                
                newKey.stringValue = "";
                newValue.objectReferenceValue = null;
            };
            
            // Remove callback
            list.onRemoveCallback = (ReorderableList l) =>
            {
                if (keysProperty.arraySize > 0)
                {
                    keysProperty.DeleteArrayElementAtIndex(l.index);
                    valuesProperty.DeleteArrayElementAtIndex(l.index);
                }
            };
        }
        
        return list;
    }
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        var keysProperty = property.FindPropertyRelative("keys");
        
        // Sync values array size with keys
        var valuesProperty = property.FindPropertyRelative("values");
        if (valuesProperty.arraySize != keysProperty.arraySize)
        {
            valuesProperty.arraySize = keysProperty.arraySize;
        }
        
        GetList(property).DoList(position);
        
        EditorGUI.EndProperty();
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return GetList(property).GetHeight();
    }
}
#endif