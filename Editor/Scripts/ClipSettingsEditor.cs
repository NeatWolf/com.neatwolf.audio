using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.EditorTools;

namespace NeatWolf.Audio.Editor
{
    [CustomEditor(typeof(ClipSettings))]
    public class ClipSettingsEditor : Editor
    {
        private Coroutine playPreviewCoroutine;

        public override void OnInspectorGUI()
        {
            ClipSettings clipSettings = (ClipSettings)target;

            DrawDefaultInspector();

            if (GUILayout.Button("Play Preview"))
            {
                if (playPreviewCoroutine != null)
                {
                    EditorCoroutineUtility.StopCoroutine(playPreviewCoroutine);
                }
                playPreviewCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(PlayPreview(clipSettings));
            }
        }

        private IEnumerator PlayPreview(ClipSettings clipSettings)
        {
            GameObject audioPreviewGameObject = new GameObject("AudioPreview");
            AudioSource previewAudioSource = audioPreviewGameObject.AddComponent<AudioSource>();

            // Apply the ClipSettings to the AudioSource
            previewAudioSource.clip = clipSettings.Clip;
            previewAudioSource.volume = clipSettings.Volume;
            previewAudioSource.pitch = clipSettings.Pitch;
            previewAudioSource.panStereo = clipSettings.PanStereo;
            previewAudioSource.spatialBlend = clipSettings.SpatialBlend;
            //previewAudioSource.rolloffMode = clipSettings.RolloffMode;

            // Play the clip
            previewAudioSource.Play();

            // Wait for the clip to finish playing
            yield return new WaitForSecondsRealtime(clipSettings.Clip.length);

            // Cleanup
            DestroyImmediate(previewAudioSource.gameObject);
        }
    }
}

/*namespace NeatWolf.Audio.Editor
{
    [CustomEditor(typeof(ClipSettings))]
    public class ClipSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            ClipSettings clipSettings = (ClipSettings)target;

            DrawDefaultInspector();

            if (GUILayout.Button("Play Preview"))
            {
                // This very simplified.
                AudioSource.PlayClipAtPoint(clipSettings.Clip, Vector3.zero);
            }
        }
    }
}
*/