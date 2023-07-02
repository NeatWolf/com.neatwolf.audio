using UnityEngine;
using UnityEngine.Audio;

namespace NeatWolf.Audio
{
    /// <summary>
    /// This class represents an AudioChannel, which is a handle for a specific AudioMixerGroup at runtime.
    /// Note: Mixers MUST reside in the project folder Assets/Audio/Mixers/
    /// AudioChannel MUST have matching names for the mixer to be resolved successfully.
    /// </summary>
    [CreateAssetMenu(menuName = "Audio/Audio Channel")]
    public class AudioChannel : ScriptableObject
    {
        private AudioMixerGroup _resolvedMixerGroup;

        /// <summary>
        /// Resolve the associated AudioMixerGroup at runtime.
        /// </summary>
        /// <returns>The resolved AudioMixerGroup.</returns>
        public AudioMixerGroup ResolveMixerGroup()
        {
            if (_resolvedMixerGroup == null)
            {
                _resolvedMixerGroup = Resources.Load<AudioMixerGroup>("Audio/Mixers/" + this.name);
                if (_resolvedMixerGroup == null)
                {
                    Debug.LogError("Failed to load AudioMixerGroup for AudioChannel " + this.name);
                }
            }

            return _resolvedMixerGroup;
        }
    }
}