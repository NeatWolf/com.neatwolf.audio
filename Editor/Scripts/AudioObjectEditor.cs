using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace NeatWolf.Audio
{
    /// <summary>
    /// Custom editor for AudioObject class.
    /// </summary>
    [CustomEditor(typeof(AudioObject))]
    public class AudioObjectEditor : Editor
    {
        private EditorCoroutine _previewCoroutine;
        private AudioSource _previewSource;

        /// <summary>
        /// Called by the Unity Editor to render the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            AudioObject audioObject = (AudioObject)target;

            DrawDefaultInspector();

            // If a preview is currently playing, show a button to stop it. Otherwise, show a button to start a new preview.
            if (_previewSource != null && _previewSource.isPlaying)
            {
                if (GUILayout.Button("Stop Preview"))
                {
                    StopPreview();
                }
            }
            else
            {
                if (GUILayout.Button("Play Preview"))
                {
                    PlayPreview(audioObject);
                }
            }
        }

        /// <summary>
        /// Starts a new preview of the AudioObject's clips.
        /// </summary>
        /// <param name="audioObject">The AudioObject to preview.</param>
        private void PlayPreview(AudioObject audioObject)
        {
            // Create a new AudioSource to use for the preview
            _previewSource = EditorUtility.CreateGameObjectWithHideFlags("Audio preview", HideFlags.HideAndDontSave, typeof(AudioSource)).GetComponent<AudioSource>();

            // Get a random clip from the AudioObject and its corresponding ClipSettings
            ClipSettings randomClipSetting;
            AudioClip clip = audioObject.GetClipSettings(out randomClipSetting);

            if (clip == null) return;

            // Use the ClipSettingsDrawer's static method to start the preview
            ClipSettingsDrawer.PlayClipSettingsPreview(_previewSource, randomClipSetting, out _previewCoroutine);
        }

        /// <summary>
        /// Stops the currently playing preview, if there is one.
        /// </summary>
        private void StopPreview()
        {
            // Stop the coroutine if it's running
            if (_previewCoroutine != null)
            {
                EditorCoroutineUtility.StopCoroutine(_previewCoroutine);
                _previewCoroutine = null;
            }

            // Destroy the AudioSource
            if (_previewSource != null)
            {
                if (_previewSource.gameObject != null)
                {
                    DestroyImmediate(_previewSource.gameObject);
                }
                _previewSource = null;
            }
        }
    }
}