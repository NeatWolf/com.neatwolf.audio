using System.Collections;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace NeatWolf.Audio
{
    /// <summary>
    /// Custom drawer for ClipSettings class.
    /// </summary>
    [CustomPropertyDrawer(typeof(ClipSettings))]
    public class ClipSettingsDrawer : PropertyDrawer
    {
        /// <summary>
        /// GUI for property.
        /// </summary>
        /// <param name="position">Rectangle on the screen to use for the property GUI.</param>
        /// <param name="property">The SerializedProperty to make the custom GUI for.</param>
        /// <param name="label">The label of this property.</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Debug.Log(property.serializedObject.targetObject);
            
            // Fetch the properties
            SerializedProperty clipProp = property.FindPropertyRelative("audioClip");
            SerializedProperty volumeProp = property.FindPropertyRelative("volume");
            SerializedProperty pitchProp = property.FindPropertyRelative("pitch");
            SerializedProperty panProp = property.FindPropertyRelative("panStereo");
            SerializedProperty startProp = property.FindPropertyRelative("startPosition");
            SerializedProperty endProp = property.FindPropertyRelative("endPosition");

            // Set up the layout
            Rect singleFieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            // Create fields for properties
            EditorGUI.PropertyField(singleFieldRect, clipProp);
            singleFieldRect.y += EditorGUIUtility.singleLineHeight;

            EditorGUI.Slider(singleFieldRect, volumeProp, 0f, 2f, new GUIContent("Volume"));
            singleFieldRect.y += EditorGUIUtility.singleLineHeight;

            EditorGUI.Slider(singleFieldRect, pitchProp, -8f, 8f, new GUIContent("Pitch"));
            singleFieldRect.y += EditorGUIUtility.singleLineHeight;

            EditorGUI.Slider(singleFieldRect, panProp, -1f, 1f, new GUIContent("Pan"));
            singleFieldRect.y += EditorGUIUtility.singleLineHeight;

            EditorGUI.PropertyField(singleFieldRect, startProp, new GUIContent("Start Position"));
            singleFieldRect.y += EditorGUIUtility.singleLineHeight;

            // If end position is uninitialized (0), and there's a valid AudioClip, set it to clip's length
            if (Mathf.Approximately(endProp.floatValue, 0) && clipProp.objectReferenceValue != null)
            {
                AudioClip clip = (AudioClip)clipProp.objectReferenceValue;
                endProp.floatValue = clip.length;
            }

            EditorGUI.PropertyField(singleFieldRect, endProp, new GUIContent("End Position"));
            singleFieldRect.y += EditorGUIUtility.singleLineHeight;

            EditorGUI.EndProperty();
        }

        /// <summary>
        /// Plays a preview of the ClipSettings.
        /// </summary>
        /// <param name="previewSource">The AudioSource to use for the preview.</param>
        /// <param name="clipSettings">The ClipSettings to preview.</param>
        /// <param name="previewCoroutine">The coroutine for the preview, allowing it to be stopped later.</param>
        public static void PlayClipSettingsPreview(AudioSource previewSource, ClipSettings clipSettings, out EditorCoroutine previewCoroutine)
        {
            AudioClip clip = clipSettings.AudioClip;
            if (clip == null)
            {
                previewCoroutine = null;
                return;
            }

            var previewDuration = ConfigureAudioSource(previewSource, clipSettings, clip);

            // Start playing the clip and begin the coroutine to stop it after the calculated duration
            previewSource.Play();
            previewCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(StopPreviewAfterSeconds(previewSource, previewDuration));
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
        
        private static float ConfigureAudioSource(AudioSource previewSource, ClipSettings clipSettings, AudioClip clip)
        {
            previewSource.clip = clip;
            //previewSource.volume = clipSettings.Volume *
            //                       Random.Range(audioObject.VolumeRange.x, clipSettings.AudioObject.VolumeRange.y);
            //previewSource.pitch = Random.Range(clipSettings.AudioObject.PitchRange.x, clipSettings.AudioObject.PitchRange.y);
            previewSource.volume = clipSettings.Volume;
            previewSource.pitch = clipSettings.Pitch;
            previewSource.panStereo = clipSettings.PanStereo;

            // Seek to the correct start position based on the pitch
            if (clipSettings.Pitch >= 0f)
                previewSource.time = Mathf.Abs(clipSettings.StartPosition * clipSettings.Pitch);
            else
                previewSource.time = Mathf.Abs((clip.length - clipSettings.StartPosition) * clipSettings.Pitch);

            // Calculate the duration of the preview based on the start and end positions and the pitch
            float previewDuration = Mathf.Abs((clipSettings.EndPosition - clipSettings.StartPosition) / clipSettings.Pitch);
            return previewDuration;
        }
    }
}