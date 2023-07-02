using UnityEngine;

namespace NeatWolf.Audio
{
    public class PlayMode : ScriptableObject
    {
        //protected AudioPlayerContext _context;

        public virtual void RegisterListeners(AudioPlayer audioPlayer)
        {
            //_context = audioPlayer.Context;
            audioPlayer.OnClipBeginPlaying += OnClipBeginPlaying;
            audioPlayer.OnClipFinishPlaying += OnClipFinishPlaying;
            audioPlayer.OnNextLoopStart += OnNextLoopStart;
            audioPlayer.OnIntervalBegin += OnIntervalBegin;
            audioPlayer.OnIntervalEnd += OnIntervalEnd;
        }

        public virtual void UnregisterListeners(AudioPlayer audioPlayer)
        {
            audioPlayer.OnClipBeginPlaying -= OnClipBeginPlaying;
            audioPlayer.OnClipFinishPlaying -= OnClipFinishPlaying;
            audioPlayer.OnNextLoopStart -= OnNextLoopStart;
            audioPlayer.OnIntervalBegin -= OnIntervalBegin;
            audioPlayer.OnIntervalEnd -= OnIntervalEnd;
        }

        protected virtual void OnClipBeginPlaying(AudioPlayerContext context)
        {
            // Default implementation...
        }

        protected virtual void OnClipFinishPlaying(AudioPlayerContext context)
        {
            // Default implementation...
        }

        protected virtual void OnNextLoopStart(AudioPlayerContext context)
        {
            // Default implementation...
        }

        protected virtual void OnIntervalBegin(AudioPlayerContext context)
        {
            // Default implementation...
        }

        protected virtual void OnIntervalEnd(AudioPlayerContext context)
        {
            if (context.AudioObject.Looping)
            {
                context.AudioPlayer.Play(context);
            }
        }
    }
}