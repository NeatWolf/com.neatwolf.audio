using System.Collections;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace NeatWolf.Audio
{
    /// <summary>
    /// Custom drawer for ClipSettings class.
    /// Each ClipSettings will have its own preview button in the inspector.
    /// </summary>
    [CustomPropertyDrawer(typeof(ClipSettings))]
    public class ClipSettingsDrawer : PropertyDrawer
    {
        private AudioSource _previewSource;
        private EditorCoroutine _previewCoroutine;

        /// <summary>
        /// GUI for ClipSettings property with an additional "Play Preview" button.
        /// </summary>
        /// <param name="position">Rectangle on the screen to use for the property GUI.</param>
        /// <param name="property">The SerializedProperty to make the custom GUI for.</param>
        /// <param name="label">The label of this property.</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position.height = EditorGUI.GetPropertyHeight(property);
            EditorGUI.PropertyField(position, property, label, true);

            position.y += position.height;
            position.height = EditorGUIUtility.singleLineHeight;

            if (property.isExpanded
                && GUI.Button(position, "Play Preview"))
            {
                AudioObject audioObject = (AudioObject)property.serializedObject.targetObject;
                
                // Validate the configurator
                audioObject.ValidateConfigurator();
                
                ClipSettings clipSettings = GetClipSettingsFromSerializedProperty(property);

                // Obtain the configurator from the parent AudioObject
                AudioSourceConfigurator configurator = audioObject.Configurator;

                if (_previewSource == null)
                {
                    _previewSource = EditorUtility.CreateGameObjectWithHideFlags("Audio preview", HideFlags.HideAndDontSave, typeof(AudioSource)).GetComponent<AudioSource>();
                }

                if (_previewCoroutine != null)
                {
                    EditorCoroutineUtility.StopCoroutine(_previewCoroutine);
                    _previewCoroutine = null;
                }

                // Use the configurator from the parent AudioObject
                var audioSourceSettings = configurator.Configure(_previewSource, audioObject, clipSettings);
                _previewSource.clip = audioSourceSettings.AudioClip;
                _previewSource.Play();
                
                _previewCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(StopPreviewAfterSeconds(_previewSource, audioSourceSettings.Duration));
            }

            EditorGUI.EndProperty();
        }
        
        /// <summary>
        /// Method to extract ClipSettings from SerializedProperty.
        /// </summary>
        /// <param name="property">The SerializedProperty object representing ClipSettings in the inspector.</param>
        /// <returns>A ClipSettings object.</returns>
        private ClipSettings GetClipSettingsFromSerializedProperty(SerializedProperty property)
        {
            // Create a new ClipSettings instance
            ClipSettings clipSettings = new ClipSettings();
    
            // Assign field values from the SerializedProperty to the new ClipSettings instance
            clipSettings.AudioClip = property.FindPropertyRelative("audioClip").objectReferenceValue as AudioClip;
            clipSettings.Volume = property.FindPropertyRelative("volume").floatValue;
            clipSettings.Pitch = property.FindPropertyRelative("pitch").floatValue;
            clipSettings.PanStereo = property.FindPropertyRelative("panStereo").floatValue;
            clipSettings.StartPosition = property.FindPropertyRelative("startPosition").floatValue;
            clipSettings.EndPosition = property.FindPropertyRelative("endPosition").floatValue;

            return clipSettings;
        }

        /// <summary>
        /// Overridden Unity method to calculate the height of the property.
        /// </summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded)
            {
                var propertyHeight = EditorGUI.GetPropertyHeight(property, label, true);
                propertyHeight += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // Add space for the Play Preview button
                return propertyHeight;
            }
            else
            {
                return EditorGUI.GetPropertyHeight(property, label, false);
            }
        }

        /// <summary>
        /// Unity method that's called when the drawer is disabled.
        /// </summary>
        private void OnDisable()
        {
            if (_previewCoroutine != null)
            {
                EditorCoroutineUtility.StopCoroutine(_previewCoroutine);
                _previewCoroutine = null;
            }

            if (_previewSource != null)
            {
                if (_previewSource.gameObject != null)
                {
                    Object.DestroyImmediate(_previewSource.gameObject);
                }
                _previewSource = null;
            }
        }

        /// <summary>
        /// Coroutine to stop the preview after a certain number of seconds.
        /// </summary>
        /// <param name="source">The AudioSource playing the preview.</param>
        /// <param name="seconds">The number of seconds to wait before stopping.</param>
        /// <returns>An IEnumerator to allow this method to be used as a coroutine.</returns>
        private static IEnumerator StopPreviewAfterSeconds(AudioSource source, float seconds)
        {
            yield return new EditorWaitForSeconds(seconds);
            if (source != null)
            {
                if (source.gameObject != null)
                {
                    Object.DestroyImmediate(source.gameObject);
                }
                source = null;
            }
        }
    }
}