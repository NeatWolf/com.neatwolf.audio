using UnityEngine;

namespace NeatWolf.Audio
{
    /// <summary>
    /// Struct that holds settings for an AudioSource.
    /// </summary>
    public struct AudioSourceSettings
    {
        public AudioClip AudioClip;
        public float Volume;
        public float Pitch;
        public float PanStereo;
        public float Time;
        public float Duration;
    }

    /// <summary>
    /// A ScriptableObject that configures an AudioSource given an AudioObject and optional ClipSettings.
    /// </summary>
    [CreateAssetMenu(menuName = "Audio/Audio Source Configurator")]
    public class AudioSourceConfigurator : ScriptableObject
    {
        /// <summary>
        /// Configures an AudioSource using the settings from an AudioObject and optional ClipSettings.
        /// </summary>
        /// <param name="source">The AudioSource to configure.</param>
        /// <param name="audioObject">The AudioObject that holds the settings.</param>
        /// <param name="clipSettings">Optional ClipSettings. If null, the default settings from the AudioObject are used.</param>
        /// <returns>Returns a struct with the configured AudioSource settings.</returns>
        public AudioSourceSettings Configure(AudioSource source, AudioObject audioObject, ClipSettings clipSettings = null)
        {
            // If no clipSettings are provided, we use the default ones from the audioObject
            if (clipSettings == null)
            {
                clipSettings = audioObject.GetClipSettings();
            }

            // Assign AudioClip to AudioSource
            source.clip = clipSettings.AudioClip;

            // Calculate and assign Volume to AudioSource
            float volume = clipSettings.Volume * Random.Range(audioObject.VolumeRange.x, audioObject.VolumeRange.y);
            source.volume = volume;

            // Calculate and assign Pitch to AudioSource
            float pitch = clipSettings.Pitch * Random.Range(audioObject.PitchRange.x, audioObject.PitchRange.y);
            source.pitch = pitch;

            // Assign PanStereo to AudioSource
            source.panStereo = clipSettings.PanStereo;

            source.playOnAwake = false;

            // Calculate and assign start time to AudioSource
            float time;
            if (source.pitch >= 0f)
            {
                time = Mathf.Abs(clipSettings.StartPosition * source.pitch); // You have to multiply instead of divide.
            }
            else
            {
                time = Mathf.Abs((source.clip.length - clipSettings.StartPosition) * source.pitch); // Negative pitch requires seeking from the end
            }
            source.time = time;

            // Calculate duration
            float duration = (clipSettings.EndPosition - clipSettings.StartPosition) / Mathf.Abs(pitch);

            // Return AudioSourceSettings
            return new AudioSourceSettings
            {
                AudioClip = source.clip,
                Volume = volume,
                Pitch = pitch,
                PanStereo = source.panStereo,
                Time = time,
                Duration = duration
            };
        }
    }
}