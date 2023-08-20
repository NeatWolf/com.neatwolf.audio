//using NeatWolf.Spatial.Partitioning;

using System;
using UnityEngine;

namespace NeatWolf.Audio
{
    public class AudioVolumePortal:MonoBehaviour
    {
        public bool Enabled { get; set; } = true;

        // Potentially other properties for the portal, e.g. size, direction, etc.
        private void OnEnable()
        {
            AudioManager.RegisterAudioVolumePortal(this);
        }
    }
}