using UnityEngine;

namespace NeatWolf.Audio
{
    /// <summary>
    ///     This class is used to group AudioVolumePortals together.
    ///     It doesn't contain any data, it's just used as a reference to differentiate between different groups.
    ///     The only info it carries is its reference when it's created as an asset.
    /// NOTE: if no PortalGroup is specified either in AudioVolume or AudioVolumePortal,
    ///     it defaults to the "null" default group.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioVolumePortalGroup", menuName = "Audio/AudioVolumePortalGroup", order = 1)]
    public class AudioVolumePortalGroup : ScriptableObject
    {
        // Initialized by AudioManager singleton Awake
        public static AudioVolumePortalGroup DEFAULT;
        

        /*[RuntimeInitializeOnLoadMethod]
        private static void InitDefault()
        {
            DEFAULT = CreateInstance<AudioVolumePortalGroup>();
        }*/
    }
}