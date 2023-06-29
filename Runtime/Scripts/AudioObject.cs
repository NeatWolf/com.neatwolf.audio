using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NeatWolf.Audio
{
    /// <summary>
    /// This class represents a setting for an AudioClip, 
    /// holding values for pitch, volume, pan and starting/ending positions.
    /// </summary>
    [Serializable]
    public class ClipSettings
    {
        [SerializeField] private AudioClip audioClip;
        [SerializeField] private float volume;
        [SerializeField] private float pitch;
        [SerializeField] private float panStereo;
        [SerializeField] private float startPosition;
        [SerializeField] private float endPosition;

        // Constructor
        public ClipSettings()
        {
            audioClip = null;
            volume = 1f;
            pitch = 1f;
            panStereo = 0f;
            startPosition = 0f;
            endPosition = 0f;  // 0 is considered uninitialized, will be set to clip's length in the custom drawer
        }

        /// <summary>
        /// The AudioClip to be played.
        /// </summary>
        public AudioClip AudioClip => audioClip;

        /// <summary>
        /// The pitch of the AudioClip.
        /// </summary>
        public float Pitch
        {
            get => pitch;
            set => pitch = value;
        }

        /// <summary>
        /// The volume of the AudioClip.
        /// </summary>
        public float Volume
        {
            get => volume;
            set => volume = value;
        }

        /// <summary>
        /// The pan position of the AudioClip.
        /// </summary>
        public float PanStereo => panStereo;

        /// <summary>
        /// The starting position in the AudioClip.
        /// </summary>
        public float StartPosition => startPosition;

        /// <summary>
        /// The ending position in the AudioClip.
        /// </summary>
        public float EndPosition => endPosition;
    }

    /// <summary>
    /// This enum represents different play modes for an AudioObject.
    /// </summary>
    public enum PlayMode
    {
        Random,
        RandomDifferent
    }

    /// <summary>
    /// This class represents an AudioObject, holding multiple AudioClip settings and providing methods to play them.
    /// </summary>
    [CreateAssetMenu(menuName = "Audio/Audio Object")]
    public class AudioObject : ScriptableObject
    {
        [SerializeField]
        private List<ClipSettings> audioClipsSettings;
        [SerializeField]
        private AudioChannel audioChannel;

        [SerializeField, Tooltip("Range of possible volume multipliers")]
        private Vector2 volumeRange = new Vector2(1f, 1f);

        [SerializeField, Tooltip("Range of possible pitch multipliers")]
        private Vector2 pitchRange = new Vector2(1f, 1f);
        [SerializeField]
        private AudioRolloffMode rolloffMode;
        [SerializeField]
        private float spatialBlend;
        [SerializeField]
        private PlayMode playMode;
        private List<ClipSettings> audioClipsHistory = new List<ClipSettings>();

        /// <summary>
        /// The list of settings for AudioClips in this AudioObject.
        /// </summary>
        public List<ClipSettings> AudioClipsSettings => audioClipsSettings;
    
        /// <summary>
        /// The AudioChannel for this AudioObject.
        /// </summary>
        public AudioChannel AudioChannel => audioChannel;

        /// <summary>
        /// The AudioRolloffMode for this AudioObject.
        /// </summary>
        public AudioRolloffMode RolloffMode => rolloffMode;

        /// <summary>
        /// The spatial blend value for this AudioObject.
        /// </summary>
        public float SpatialBlend => spatialBlend;

        public Vector2 VolumeRange
        {
            get => volumeRange;
            set => volumeRange = value;
        }
        
        public Vector2 PitchRange
        {
            get => pitchRange;
            set => pitchRange = value;
        }

        /// <summary>
        /// Returns a ClipSettings following the playMode rules.
        /// </summary>
        /// <param name="clipSettings">The ClipSettings to play.</param>
        /// <returns>ClipSettings to play.</returns>
        public AudioClip GetClipSettings(out ClipSettings clipSettings)
        {
            clipSettings = null;
            ClipSettings chosenClip = null;

            switch (playMode)
            {
                case PlayMode.Random:
                    chosenClip = GetRandomClip();
                    break;
                case PlayMode.RandomDifferent:
                    chosenClip = GetRandomDifferentClip();
                    break;
            }
        
            if (chosenClip == null )
            {
                Debug.LogError("Chosen audioclip was null in AudioObject " + this.name);
                clipSettings = null;
                return null;
            }
        
            if (chosenClip.AudioClip == null)
            {
                Debug.LogError("Audio clip is null in AudioObject " + this.name);
                clipSettings = null;
                return null;
            }
            
            clipSettings = chosenClip;
            return clipSettings.AudioClip;
        }

        private ClipSettings GetRandomClip()
        {
            int randomIndex = Random.Range(0, audioClipsSettings.Count);
            return audioClipsSettings[randomIndex];
        }

        private ClipSettings GetRandomDifferentClip()
        {
            List<ClipSettings> nonHistoryClips = audioClipsSettings.Except(audioClipsHistory).ToList();
            if (nonHistoryClips.Count == 0)
            {
                // All clips have been played, clear the history.
                audioClipsHistory.Clear();
                nonHistoryClips = audioClipsSettings;
            }

            int randomIndex = Random.Range(0, nonHistoryClips.Count);
            ClipSettings chosenClip = nonHistoryClips[randomIndex];
            audioClipsHistory.Add(chosenClip);
            return chosenClip;
        }

        public void PlayAtPoint(Vector3 position)
        {
            if (AudioManager.Instance == null)
            {
                Debug.LogError("AudioManager instance is null. Make sure AudioManager is present in the scene.");
                return;
            }
        
            AudioManager.Instance.PlaySoundAtPosition(this, position);
        }
    }
}