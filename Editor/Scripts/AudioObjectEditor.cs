using System.Collections;
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
        // This coroutine will be used to stop the preview after the clip has played
        private EditorCoroutine _previewCoroutine;

        // The AudioSource used to preview the audio in the Editor
        private AudioSource _previewSource;

        /// <summary>
        /// This method is called by Unity to draw the custom inspector in the editor.
        /// </summary>
        public override void OnInspectorGUI()
        {
            // Cast the target to AudioObject
            AudioObject audioObject = (AudioObject)target;

            // Draw the default inspector
            DrawDefaultInspector();

            // The "Play Preview" button always stops the previous sound and plays a new one
            if (GUILayout.Button("Play Preview"))
            {
                // If a sound is playing, it gets stopped, and a new one is started
                StopPreview();
                PlayPreview(audioObject);
            }
        }

        /// <summary>
        /// Starts a new preview using the settings of the provided AudioObject.
        /// </summary>
        /// <param name="audioObject">The AudioObject whose settings will be used for the preview.</param>
        private void PlayPreview(AudioObject audioObject)
        {
            // Check and set the _configurator if necessary
            audioObject.ValidateConfigurator();
            
            // Create a new GameObject for the preview and add an AudioSource to it
            _previewSource = EditorUtility.CreateGameObjectWithHideFlags("Audio preview", HideFlags.HideAndDontSave, typeof(AudioSource)).GetComponent<AudioSource>();

            // Get a random clip from the AudioObject
            ClipSettings randomClipSettings = audioObject.GetClipSettings();

            // If the ClipSettings is null, abort the preview
            if (randomClipSettings == null) return;

            // Use the AudioSourceConfigurator from AudioObject to start the preview
            AudioSourceSettings settings = audioObject.Configurator.Configure(_previewSource, audioObject, randomClipSettings);
            _previewSource.Play();

            // Start a coroutine to stop the preview after the clip has played
            _previewCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(StopPreviewAfterSeconds(settings.Duration));
        }
        
        /// <summary>
        /// Coroutine to stop the preview after the specified number of seconds.
        /// </summary>
        /// <param name="seconds">The number of seconds to wait before stopping the preview.</param>
        /// <returns>Yields control back to Unity while waiting.</returns>
        private IEnumerator StopPreviewAfterSeconds(float seconds)
        {
            yield return new EditorWaitForSeconds(seconds);
            StopPreview();
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

            // Stop the AudioSource and destroy the GameObject
            if (_previewSource != null)
            {
                _previewSource.Stop();
                if (_previewSource.gameObject != null)
                {
                    DestroyImmediate(_previewSource.gameObject);
                }
                _previewSource = null;
            }
        }
    }
}
