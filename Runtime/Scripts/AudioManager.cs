#nullable enable
using System;
using System.Collections.Generic;
using NeatWolf.Spatial.Partitioning;
using UnityEngine;
using UnityEngine.Pool;

namespace NeatWolf.Audio
{
    /// <summary>
    ///     AudioManager is responsible for managing the playback of AudioObjects.
    ///     It fetches an instance of AudioPlayer from a pool, configures it according to the AudioObject's settings, and
    ///     initiates the audio playback.
    ///     It also manages the registration of AudioVolumePortals and their assignment to different AudioVolumePortalGroups.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager instance;
        private static bool hasAwoken;
        private static bool hasStarted;
        [SerializeField] private GameObject audioPlayerPrefab; // Assign this in the inspector
        [SerializeField] private LayerMask occlusionLayerMask; // Define the occlusion layer mask.

        [SerializeField]
        private float occlusionFactor = 0.5f; // The factor by which to reduce volume if LOS is blocked.

        [SerializeField] private float occlusionSmoothTime = 0.1f;

        private ObjectPool<GameObject> _audioPlayerPool;

        public float OcclusionSmoothTime
        {
            get => occlusionSmoothTime;
            set => occlusionSmoothTime = value;
        }

        // Replaced the single Octree with a dictionary of Octrees, one for each AudioVolumePortalGroup.
        /// <summary>
        ///     A dictionary that maps each AudioVolumePortalGroup to an Octree of AudioVolumePortals.
        ///     This allows for efficient spatial queries for portals within a specific group.
        /// </summary>
        [field: NonSerialized]
        private Dictionary<AudioVolumePortalGroup, Octree<AudioVolumePortal>> portals = new();

        public Octree<AudioVolumePortal> GetPortals(AudioVolumePortalGroup key)
        {
            if (key == null)
            {
                // Handle null key access
                key = AudioVolumePortalGroup.DEFAULT;
            }

            // Initialize new Octree<AudioVolumePortal> and add it to the dictionary
            portals.TryAdd(key, new Octree<AudioVolumePortal>(Vector3.zero, new Vector3(500, 500, 500), 5, 1, 10));

            return portals[key];
        }
        
        /// <summary>
        ///     Singleton instance of the AudioManager.
        /// </summary>
        public static AudioManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<AudioManager>();

                    if (instance == null)
                    {
                        Debug.LogError("An instance of AudioManager is needed in the scene, but none was found.");
                    }
                    else
                    {
                        // Initialize if needed
                        if (!hasAwoken) instance.Awake();

                        if (!hasStarted) instance.Start();
                    }
                }

                return instance;
            }
        }

        private void Awake()
        {
            if (hasAwoken)
                return;
            AudioVolumePortalGroup.DEFAULT = ScriptableObject.CreateInstance<AudioVolumePortalGroup>();
            
            // Check for duplicates
            if (instance != null && instance != this)
            {
                Debug.LogError("More than one AudioManager instance detected. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            instance = this;

            // Initialize the pool
            _audioPlayerPool = new ObjectPool<GameObject>(() => Instantiate(audioPlayerPrefab));
            // Initialize the dictionary
            // This would depend on the size of your scene and the number of portals.
            // The default group is null. null! suppresses the warning
            // Using null with the "!" is a way to bypass the compiler's null-safety checks.
            //Portals = new Dictionary<AudioVolumePortalGroup, Octree<AudioVolumePortal>>();
            // Initialize the Octree for the default group
            //Portals[AudioVolumePortalGroup.DEFAULT] = new Octree<AudioVolumePortal>(Vector3.zero, new Vector3(500, 500, 500), 5, 1, 10);
            GetPortals(AudioVolumePortalGroup.DEFAULT);
            
            hasAwoken = true;
        }

        private void Start()
        {
            if (hasStarted)
                return;
            
            // Add additional initialization code here if needed
            hasStarted = true;
        }

        public float GetOccludedVolumeFactor(Vector3 volumeCentre, Vector3 shapeClosestPosition, Vector3 closestAudioPosition, Vector3 listenerPosition,
            bool invertedVolume = false)
        {
            if (invertedVolume)
            {
                // no extra check towards volume centre if using inverted volume logic
                //if (Physics.Linecast(closestAudioPosition, listenerPosition, occlusionLayerMask))
                //    return occlusionFactor;

                if (Physics.CheckSphere(closestAudioPosition, 0.25f, occlusionLayerMask))
                    return occlusionFactor;
                return 1f;
            }

            // handle occluded portals
            if (Physics.CheckSphere(closestAudioPosition, 0.25f, occlusionLayerMask))
                return occlusionFactor;
            
            if (Physics.Linecast(closestAudioPosition, listenerPosition, occlusionLayerMask)
                || Physics.Linecast(shapeClosestPosition, closestAudioPosition, occlusionLayerMask)
                || Physics.Linecast(shapeClosestPosition, listenerPosition, occlusionLayerMask))
                return occlusionFactor;
            return 1f;
        }

        /// <summary>
        /// Calculates and returns the volume multiplier for an AudioPlayer, based on occlusion.
        /// </summary>
        /// <param name="audioPlayer">The AudioPlayer for which to calculate the volume multiplier.</param>
        /// <returns>The volume multiplier based on occlusion.</returns>
        /*public float GetVolumeMultiplier(AudioPlayer audioPlayer)
        {
            // If the line of sight from the AudioVolumeListener to the AudioPlayer is blocked, reduce the volume by the occlusion factor.
            if (Physics.Linecast(AudioVolumeListener.Instance.transform.position, audioPlayer.transform.position, occlusionLayerMask))
            {
                return Mathf.Lerp(audioPlayer.Volume, audioPlayer.Volume * occlusionFactor, occlusionSmoothTime);
            }
            else
            {
                return audioPlayer.Volume;
            }
        }*/

        /// <summary>
        ///     Play an AudioObject.
        ///     If RepositionToTarget is true, it uses the targetTransform's position, otherwise it uses audioPosition.
        ///     If ParentToTarget is true, the AudioPlayer becomes a child of targetTransform.
        ///     The method returns the instance of AudioPlayer that's playing the AudioObject.
        /// </summary>
        /// <param name="audioObject">The AudioObject to be played.</param>
        /// <param name="audioPosition">The position at which to play the AudioObject if RepositionToTarget is false.</param>
        /// <param name="targetTransform">
        ///     The Transform at which to play the AudioObject and/or parent the AudioPlayer to if
        ///     RepositionToTarget and/or ParentToTarget is true. Can be null.
        /// </param>
        /// <returns>The AudioPlayer that's playing the AudioObject.</returns>
        public AudioPlayer Play(AudioObject audioObject, Vector3 audioPosition, Transform targetTransform = null)
        {
            // AudioObject must not be null
            if (audioObject == null)
            {
                Debug.LogError("AudioObject is null in AudioManager's Play.");
                return null;
            }

            // Fetch an AudioPlayer from the pool
            var audioPlayerGameObject = _audioPlayerPool.Get();

            // Ensure the pool has available instances
            if (audioPlayerGameObject == null)
            {
                Debug.LogError("Failed to get AudioPlayer from pool in AudioManager.");
                return null;
            }

            // Ensure the fetched object has an AudioPlayer component
            var audioPlayer = audioPlayerGameObject.GetComponent<AudioPlayer>();

            if (audioPlayer == null)
            {
                Debug.LogError("Fetched object from pool in AudioManager does not have an AudioPlayer component.");
                return null;
            }

            // Create a context for the AudioPlayer
            var context = new AudioPlayerContext
            {
                AudioPlayer = audioPlayer,
                AudioObject = audioObject,
                Position = audioObject.RepositionToTarget && targetTransform != null
                    ? targetTransform.position
                    : audioPosition,
                TargetTransform = audioObject.RepositionToTarget ? targetTransform : null,
                ClipSettings = null, // Set the clip settings here if necessary
                ParentToTarget = audioObject.ParentToTransform
            };

            // Parent the AudioPlayer to targetTransform if ParentToTarget is true
            audioPlayer.transform.SetParent(context.ParentToTarget ? context.TargetTransform : null);

            // Play the audio
            audioPlayer.Play(context);

            return audioPlayer;
        }

        /// <summary>
        ///     Register an AudioVolumePortal to the AudioManager.
        ///     The portal will be added to the Octree of its group.
        ///     If the group doesn't exist in the dictionary, a new Octree will be created for it.
        /// </summary>
        /// <param name="portal">The portal to register.</param>
        /// <param name="nullablePortalGroup">The group the portal belongs to.</param>
        public static void RegisterAudioVolumePortal(AudioVolumePortal portal,
            AudioVolumePortalGroup? nullablePortalGroup=null)
        {
            Octree<AudioVolumePortal> octree = Instance.GetPortals(nullablePortalGroup!);
            octree.Insert(portal.transform.position, portal);
        }

        /*
        /// <summary>
        /// Remove an AudioVolumePortal from the Octree.
        /// </summary>
        /// <param name="portal">The AudioVolumePortal to remove.</param>
        public void DeregisterAudioVolumePortal(AudioVolumePortal portal)
        {
            audioVolumePortalOctree.Remove(portal.transform.position);
        }

        /// <summary>
        /// Finds and returns the AudioVolumePortal closest to a given position.
        /// </summary>
        /// <param name="position">The position for which to find the closest AudioVolumePortal.</param>
        /// <returns>The closest AudioVolumePortal to the position, or null if none exist.</returns>
        public AudioVolumePortal FindClosestAudioVolumePortal(Vector3 position)
        {
            return audioVolumePortalOctree.FindNearestNode(position)?.Data;
        }*/
    }
}