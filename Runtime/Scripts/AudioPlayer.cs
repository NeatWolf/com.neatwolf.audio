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
    /// This class represents an AudioPlayer, responsible for playing an AudioObject at a certain position.
    /// An AudioSource component must be present on the same gameobject.
    /// Usually this is made into a prefab
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioPlayer : MonoBehaviour
    {
        public AudioPlayerContext Context { get; private set; }

        [SerializeField]
        private AudioSourceConfigurator _configurator;

        private AudioSource _audioSource;
        private PlayMode _playMode;

        public event Action<AudioPlayerContext> OnClipBeginPlaying;
        public event Action<AudioPlayerContext> OnClipFinishPlaying;
        public event Action<AudioPlayerContext> OnNextLoopStart;
        public event Action<AudioPlayerContext> OnIntervalBegin;
        public event Action<AudioPlayerContext> OnIntervalEnd;

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

        protected virtual void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            Context = new AudioPlayerContext { AudioPlayer = this };
        }

        public virtual void PlayClipAtPosition(AudioObject audioObject, Vector3 position, ClipSettings clipSettings = null)
        {
            UpdateContext(audioObject, position, null, clipSettings, false);
            Play(Context);
        }

        public virtual void PlayClipAtTransform(AudioObject audioObject, Transform transform, ClipSettings clipSettings = null, bool followTransform = false)
        {
            UpdateContext(audioObject, transform.position, transform, clipSettings, followTransform);
            Play(Context);
        }

        public virtual void Play(AudioPlayerContext context)
        {
            context.Position = context.ParentToTarget && context.TargetTransform != null
                ? context.TargetTransform.position
                : context.Position;
            transform.position = context.Position;

            AudioSourceSettings settings = _configurator.Configure(_audioSource, context.AudioObject, context.ClipSettings);

            if (UseSpatialBlendPlayerMultiplier)
                _audioSource.spatialBlend *= SpatialBlendMultiplier;

            if (settings.AudioClip == null || _audioSource.outputAudioMixerGroup == null)
            {
                return;
            }

            _audioSource.Play();

            if (InvokeEventAndCheckStopRequest(OnClipBeginPlaying))
            {
                return;
            }

            StartCoroutine(StopAtEndPositionCoroutine(settings.Duration));
        }

        protected virtual IEnumerator StopAtEndPositionCoroutine(float endPosition)
        {
            yield return new WaitForSeconds(endPosition);

            if (InvokeEventAndCheckStopRequest(OnClipFinishPlaying))
            {
                yield break;
            }

            if (Context.AudioObject.Looping)
            {
                float interval = Context.AudioObject.GetInterval();
                if (interval > 0)
                {
                    if (InvokeEventAndCheckStopRequest(OnNextLoopStart))
                    {
                        yield break;
                    }

                    StartCoroutine(IntervalCoroutine(interval));
                }
            }
        }

        protected virtual IEnumerator IntervalCoroutine(float interval)
        {
            if (InvokeEventAndCheckStopRequest(OnIntervalBegin))
            {
                yield break;
            }

            yield return new WaitForSeconds(interval);

            if (InvokeEventAndCheckStopRequest(OnIntervalEnd))
            {
                yield break;
            }

            if (Context.ParentToTarget && Context.TargetTransform != null)
            {
                Context.Position = Context.TargetTransform.position;
            }

            Play(Context);
        }

        protected virtual void UpdateContext(AudioObject audioObject, Vector3 position, Transform transform, ClipSettings clipSettings, bool followTransform)
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