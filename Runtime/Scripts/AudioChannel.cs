using UnityEngine;
using UnityEngine.Audio;

namespace NeatWolf.Audio
{
    /// <summary>
    /// This class represents an AudioChannel, which is a handle for a specific AudioMixerGroup at runtime.
    /// Note: The main AudioMixer asset MUST reside in the Resources folder.
    /// Each AudioChannel ScriptableObject is associated with an AudioMixerGroup.
    /// This association is made by naming the AudioChannel ScriptableObject after the desired AudioMixerGroup.
    /// When an AudioChannel needs to resolve its corresponding AudioMixerGroup, it retrieves the first AudioMixer
    /// found in the Resources folder, then searches that mixer's groups for one with a matching name.
    /// If a matching group is found, it is stored for future reference.
    /// </summary>
    [CreateAssetMenu(menuName = "Audio/Audio Channel")]
    public class AudioChannel : ScriptableObject
    {
        private AudioMixerGroup _resolvedMixerGroup;

        /// <summary>
        /// Resolve the associated AudioMixerGroup at runtime. 
        /// It does so by loading the first AudioMixer it finds in the Resources folder, and then looking for 
        /// an AudioMixerGroup within that mixer which matches the name of this AudioChannel. 
        /// If found, the AudioMixerGroup is stored for future use.
        /// </summary>
        /// <returns>The resolved AudioMixerGroup. If no matching group is found, null is returned.</returns>
        public AudioMixerGroup ResolveMixerGroup()
        {
            if (_resolvedMixerGroup == null)
            {
                var mixers = Resources.LoadAll<AudioMixer>("");
                if (mixers.Length == 0)
                {
                    Debug.LogError("No AudioMixer assets found in Resources directory.");
                    return null;
                }

                var mixer = mixers[0]; // Take the first AudioMixer.

                var groups = mixer.FindMatchingGroups(this.name);
                if (groups.Length == 0)
                {
                    Debug.LogError("Failed to find an AudioMixerGroup with the name " + this.name);
                    return null;
                }

                _resolvedMixerGroup = groups[0]; // Take the first matching group. Modify as needed if your setup involves multiple matching groups.
            }

            return _resolvedMixerGroup;
        }
    }
}
