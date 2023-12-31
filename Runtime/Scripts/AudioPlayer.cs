using System;
using System.Collections;
using UnityEngine;

namespace NeatWolf.Audio
{
    // AudioPlayerContext
    public class AudioPlayerContext
    {
        public AudioPlayer AudioPlayer { get; set; }
        public AudioObject AudioObject { get; set; }
        public Vector3 Position { get; set; }
        public Transform TargetTransform { get; set; }
        public ClipSettings ClipSettings { get; set; }
        public bool ParentToTarget { get; set; }
    }

    /// <summary>
    ///     This class represents an AudioPlayer, responsible for playing an AudioObject at a certain position.
    ///     An AudioSource component must be present on the same gameobject.
    ///     Usually this is made into a prefab
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioPlayer : MonoBehaviour
    {
        [SerializeField] private AudioSourceConfigurator _configurator;

        private AudioSource _audioSource;
        private PlayMode _playMode;

        internal float targetVolumeMultiplier = 1f; // The volume we want to reach.
        private float volumeVelocity; // A variable used by Mathf.SmoothDamp.
        public AudioPlayerContext Context { get; private set; }

        public float Volume { get; set; } = 1f; // Default to max volume.

        public PlayMode PlayMode
        {
            get => _playMode;
            set
            {
                _playMode?.UnregisterListeners(this);
                _playMode = value;
                _playMode?.RegisterListeners(this);
            }
        }

        public bool UseSpatialBlendPlayerMultiplier { get; set; }
        public float SpatialBlendMultiplier { get; set; }

        public float CurrentVolumeMultiplier { get; set; } = 1f;

        protected virtual void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            Context = new AudioPlayerContext { AudioPlayer = this };
        }

        private void Update()
        {
            // Gradually adjust the current volume to match the target volume.
            CurrentVolumeMultiplier = Mathf.SmoothDamp(CurrentVolumeMultiplier, targetVolumeMultiplier,
                ref volumeVelocity, AudioManager.Instance.OcclusionSmoothTime);

            // Apply the current volume to the audio source.
            UpdateVolume();
        }

        public event Action<AudioPlayerContext> OnClipBeginPlaying;
        public event Action<AudioPlayerContext> OnClipFinishPlaying;
        public event Action<AudioPlayerContext> OnNextLoopStart;
        public event Action<AudioPlayerContext> OnIntervalBegin;
        public event Action<AudioPlayerContext> OnIntervalEnd;

        public virtual void PlayClipAtPosition(AudioObject audioObject, Vector3 position,
            ClipSettings clipSettings = null)
        {
            UpdateContext(audioObject, position, null, clipSettings, false);
            Play(Context);
        }

        public virtual void PlayClipAtTransform(AudioObject audioObject, Transform transform,
            ClipSettings clipSettings = null, bool followTransform = false)
        {
            UpdateContext(audioObject, transform.position, transform, clipSettings, followTransform);
            Play(Context);
        }

        public virtual void Play(AudioPlayerContext context)
        {
            Context = context;

            Context.Position = Context.ParentToTarget && Context.TargetTransform != null
                ? Context.TargetTransform.position
                : Context.Position;
            transform.position = Context.Position;


            var settings = _configurator.Configure(_audioSource, Context.AudioObject, Context.ClipSettings);
            Volume = _audioSource.volume;

            //if (UseSpatialBlendPlayerMultiplier)
            //    _audioSource.spatialBlend *= SpatialBlendMultiplier;

            if (settings.AudioClip == null || _audioSource.outputAudioMixerGroup == null) return;


            _audioSource.Play();
            targetVolumeMultiplier = 1f;
            CurrentVolumeMultiplier = 1f;

            if (InvokeEventAndCheckStopRequest(OnClipBeginPlaying)) return;

            StartCoroutine(StopAtEndPositionCoroutine(settings.Duration));
        }


        public virtual void UpdateVolume()
        {
            _audioSource.volume = Volume * CurrentVolumeMultiplier;
        }

        public virtual void UpdateSpatialBlend(float overrideValue)
        {
            //if (UseSpatialBlendPlayerMultiplier)
            _audioSource.spatialBlend = overrideValue;
        }

        public virtual void UpdateSpread(float overrideValue)
        {
            _audioSource.spread = overrideValue;
        }

        protected virtual IEnumerator StopAtEndPositionCoroutine(float endPosition)
        {
            yield return new WaitForSeconds(endPosition);

            if (InvokeEventAndCheckStopRequest(OnClipFinishPlaying)) yield break;

            if (Context.AudioObject.Looping)
            {
                var interval = Mathf.Max(0f, Context.AudioObject.GetLoopInterval());
                //if (interval > 0)
                //{
                if (InvokeEventAndCheckStopRequest(OnNextLoopStart)) yield break;

                StartCoroutine(IntervalCoroutine(interval));
                //}
                //else //restart the clip
            }
        }

        protected virtual IEnumerator IntervalCoroutine(float interval)
        {
            if (InvokeEventAndCheckStopRequest(OnIntervalBegin)) yield break;

            if (interval > 0f)
                yield return new WaitForSeconds(interval);

            if (InvokeEventAndCheckStopRequest(OnIntervalEnd)) yield break;

            if (Context.ParentToTarget && Context.TargetTransform != null)
                Context.Position = Context.TargetTransform.position;

            if (interval > 0f)
                Play(Context);
        }

        protected virtual void UpdateContext(AudioObject audioObject, Vector3 position, Transform transform,
            ClipSettings clipSettings, bool followTransform)
        {
            Context.AudioObject = audioObject;
            Context.Position = position;
            Context.TargetTransform = transform;
            Context.ClipSettings = clipSettings;
            Context.ParentToTarget = followTransform;
        }

        protected virtual bool InvokeEventAndCheckStopRequest(Action<AudioPlayerContext> action)
        {
            action?.Invoke(Context);
            return false; // No StopAndRecycleRequested flag exists in the original class
        }

        public virtual void Play()
        {
            Play(Context);
        }
    }
}