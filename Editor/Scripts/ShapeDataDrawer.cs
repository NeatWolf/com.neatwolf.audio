#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace NeatWolf.Audio
{
    [CustomPropertyDrawer(typeof(AudioVolumeShape.ShapeData))]
    public class ShapeDataDrawer : PropertyDrawer
    {
        private Dictionary<string, SerializedProperty> _serializedProperties = new Dictionary<string, SerializedProperty>();
        private bool _cached = false;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (string.IsNullOrEmpty(property.managedReferenceFullTypename))
                return;

            if (!_cached)
            {
                CacheSerializedProperties(property);
                _cached = true;
            }

            EditorGUI.BeginProperty(position, label, property);

            foreach (var item in _serializedProperties)
            {
                var serializedProperty = item.Value;
                EditorGUI.PropertyField(position, serializedProperty, GUIContent.none, true);
                position.y += EditorGUI.GetPropertyHeight(serializedProperty, GUIContent.none, true);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (string.IsNullOrEmpty(property.managedReferenceFullTypename))
                return 0f;//EditorGUIUtility.singleLineHeight;

            if (!_cached)
            {
                CacheSerializedProperties(property);
                _cached = true;
            }

            float totalHeight = 0f;

            foreach (var item in _serializedProperties)
            {
                var serializedProperty = item.Value;
                totalHeight += EditorGUI.GetPropertyHeight(serializedProperty, GUIContent.none, true);
            }

            return totalHeight;
        }

        private void CacheSerializedProperties(SerializedProperty property)
        {
            //Clears the cached properties first.
            _serializedProperties.Clear();

            //Debug.Log(property.name);
            var managedReferenceValue = property.managedReferenceFullTypename;
            var splitType = managedReferenceValue.Split(' ');
            if (splitType.Length != 2)
            {
                Debug.LogError("Unable to get the type of the managed reference.");
                return;
            }

            var assemblyName = splitType[0];
            var typeName = splitType[1];
            var type = Assembly.Load(assemblyName).GetType(typeName);

            if (type == null)
            {
                Debug.LogError($"Unable to find type {typeName} in assembly {assemblyName}.");
                return;
            }

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (field.IsPublic || Attribute.IsDefined(field, typeof(SerializeField)))
                {
                    var serializedProperty = property.FindPropertyRelative(field.Name);
                    _serializedProperties.Add(field.Name, serializedProperty);
                }
            }
        }
    }
}
#endif
