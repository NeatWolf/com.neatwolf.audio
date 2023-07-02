using System;
using System.Collections.Generic;
using System.Linq;
using NeatWolf.Attributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
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
        public AudioClip AudioClip
        {
            get => audioClip;
            set => audioClip = value;
        }

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
        public float PanStereo
        {
            get => panStereo;
            set => panStereo = value;
        }

        /// <summary>
        /// The starting position in the AudioClip.
        /// </summary>
        public float StartPosition
        {
            get => startPosition;
            set => startPosition = value;
        }

        /// <summary>
        /// The ending position in the AudioClip.
        /// </summary>
        public float EndPosition
        {
            get => endPosition;
            set => endPosition = value;
        }
    }

    /// <summary>
    /// This enum represents different play modes for an AudioObject.
    /// </summary>
    // The strategy to pick clips from the list
    public enum ClipPickStrategy
    {
        First,
        Sequential,
        Random,
        RandomDifferent
    }

    /// <summary>
    /// This class represents an AudioObject, holding multiple AudioClip settings and providing methods to play them.
    /// </summary>
    [CreateAssetMenu(menuName = "Audio/Audio Object")]
    public class AudioObject : ScriptableObject
    {
        // List of settings for each clip this AudioObject will play
        [SerializeField]
        private List<ClipSettings> audioClipsSettings;

        // The channel that will be used to play this audio
        [SerializeField]
        [ScriptableObjectCollection("Assets/Settings/Audio/")]
        private AudioChannel audioChannel;

        // Volume that will be applied to the chosen clip (multiplier)
        [SerializeField, Tooltip("Range of possible volume multipliers")]
        private Vector2 volumeRange = new Vector2(1f, 1f);

        // Pitch that will be applied to the chosen clip (multiplier)
        [SerializeField, Tooltip("Range of possible pitch multipliers")]
        private Vector2 pitchRange = new Vector2(1f, 1f);

        // Defines how the audio will be attenuated the further from the listener it gets
        [SerializeField]
        private AudioRolloffMode rolloffMode;

        // 0 means the audio is completely 2D, 1 means it's completely 3D
        [SerializeField]
        private float spatialBlend;

        // The strategy to pick clips from the list
        [SerializeField]
        private ClipPickStrategy clipPickStrategy;

        private int _clipIndex = 0;

        // Clips that have already been chosen, only used with PlayMode.RandomDifferent
        private List<ClipSettings> _audioClipsHistory = new List<ClipSettings>();
        
        [Space]
        // Configurator that sets up the AudioSource for previewing.
        [SerializeField, Tooltip("Configurator that sets up the AudioSource for previewing.")]
        private AudioSourceConfigurator _configurator;

        [SerializeField] private bool _looping;
        [SerializeField] private bool _parentToTransform;
        [FormerlySerializedAs("repositionToTransform")] [FormerlySerializedAs("_followTransform")] [SerializeField] private bool repositionToTarget;
        
        [SerializeField, Tooltip("The range for the interval between loops.")]
        private Vector2 intervalRange = new Vector2(0f, 0f);


        public Vector2 IntervalRange
        {
            get => intervalRange;
            set => intervalRange = value;
        }

        public float GetInterval()
        {
            return Random.Range(intervalRange.x, intervalRange.y);
        }

        private void OnValidate()
        {
            ValidateConfigurator();
        }

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

        public AudioSourceConfigurator Configurator => _configurator;

        public bool Looping
        {
            get { return _looping; }
        }
        
        public bool RepositionToTarget
        {
            get { return repositionToTarget; }
        }

        public bool ParentToTransform
        {
            get { return _parentToTransform; }
        }

        /// <summary>
        /// Returns a ClipSettings following the playMode rules.
        /// </summary>
        /// <returns>ClipSettings to play.</returns>
        public ClipSettings GetClipSettings()
        {
            ClipSettings chosenClip = null;

            switch (clipPickStrategy)
            {
                case ClipPickStrategy.First:
                    chosenClip = GetFirstClip();
                    break;
                case ClipPickStrategy.Sequential:
                    chosenClip = GetSequentialClip();
                    break;
                case ClipPickStrategy.Random:
                    chosenClip = GetRandomClip();
                    break;
                case ClipPickStrategy.RandomDifferent:
                    chosenClip = GetRandomDifferentClip();
                    break;
            }
    
            if (chosenClip == null )
            {
                Debug.LogError("Chosen audioclip was null in AudioObject " + this.name);
                return null;
            }
    
            if (chosenClip.AudioClip == null)
            {
                Debug.LogError("Audio clip is null in AudioObject " + this.name);
                return null;
            }
        
            return chosenClip;
        }
        
        private ClipSettings GetFirstClip()
        {
            return audioClipsSettings[0];
        }

        private ClipSettings GetSequentialClip()
        {
            ClipSettings chosenClip = audioClipsSettings[_clipIndex];
            _clipIndex = (_clipIndex + 1) % audioClipsSettings.Count;
            return chosenClip;
        }

        private ClipSettings GetRandomClip()
        {
            int randomIndex = Random.Range(0, audioClipsSettings.Count);
            return audioClipsSettings[randomIndex];
        }

        private ClipSettings GetRandomDifferentClip()
        {
            List<ClipSettings> nonHistoryClips = audioClipsSettings.Except(_audioClipsHistory).ToList();
            if (nonHistoryClips.Count == 0)
            {
                // All clips have been played, clear the history.
                _audioClipsHistory.Clear();
                nonHistoryClips = audioClipsSettings;
            }

            int randomIndex = Random.Range(0, nonHistoryClips.Count);
            ClipSettings chosenClip = nonHistoryClips[randomIndex];
            _audioClipsHistory.Add(chosenClip);
            return chosenClip;
        }

        /*public void PlayAtPoint(Vector3 position)
        {
            if (AudioManager.Instance == null)
            {
                Debug.LogError("AudioManager instance is null. Make sure AudioManager is present in the scene.");
                return;
            }
        
            AudioManager.Instance.PlaySoundAtPosition(this, position);
        }*/
        
        public void ValidateConfigurator()
        {
            if (_configurator == null)
            {
                string path = "Assets/Settings/DefaultAudioSourceConfigurator.asset";
#if UNITY_EDITOR
                _configurator = AssetDatabase.LoadAssetAtPath<AudioSourceConfigurator>(path);
#endif

                if (_configurator == null)
                {
                    Debug.LogWarning($"Failed to load AudioSourceConfigurator from path: {path}. Please ensure the asset exists at this location.");
                }
                else
                {
                    Debug.LogWarning("AudioSourceConfigurator was null and has been set to the default.");
                    EditorUtility.SetDirty(this);
                }
            }
        }
    }
}