using UnityEngine;

namespace NeatWolf.Audio
{
    /// <summary>
    ///     This class represents a portal through which sound can propagate.
    ///     Each portal can be assigned to a specific AudioVolumePortalGroup.
    /// </summary>
    public class AudioVolumePortal : MonoBehaviour
    {
        /// <summary>
        ///     The group this portal belongs to.
        ///     If no group is assigned, the portal will belong to the default group.
        /// </summary>
        [SerializeField] private AudioVolumePortalGroup portalGroup = AudioVolumePortalGroup.DEFAULT;
        public AudioVolumePortalGroup PortalGroup
        {
            get => portalGroup;
            protected set => portalGroup = value;
        }
        
        //public bool Enabled { get; set; } = true;

        // Potentially other properties for the portal, e.g. size, direction, etc.
        private void OnEnable()
        {
            // When the portal is enabled, it registers itself to the AudioManager.
            // The AudioManager will add it to the correct Octree based on its group.
            AudioManager.RegisterAudioVolumePortal(this, PortalGroup);
        }
    }
}