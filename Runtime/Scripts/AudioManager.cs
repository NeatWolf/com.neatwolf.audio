//using NeatWolf.Spatial.Partitioning;

using System;
using NeatWolf.Spatial.Partitioning;
using UnityEngine;
using UnityEngine.Pool;

namespace NeatWolf.Audio
{
    /// <summary>
    /// AudioManager is responsible for managing the playback of AudioObjects. 
    /// It fetches an instance of AudioPlayer from a pool, configures it according to the AudioObject's settings, and initiates the audio playback.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private GameObject audioPlayerPrefab; // Assign this in the inspector
        [SerializeField] private LayerMask occlusionLayerMask; // Define the occlusion layer mask.
        [SerializeField] private float occlusionFactor = 0.5f; // The factor by which to reduce volume if LOS is blocked.
        [field: SerializeField] public float OcclusionSmoothTime { get; } = 0.1f;

        private static AudioManager instance;
        private static bool hasAwoken = false;
        private static bool hasStarted = false;

        [field: NonSerialized]
        public Octree<AudioVolumePortal> PortalsTree { get; private set; }

        /// <summary>
        /// Singleton instance of the AudioManager.
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
                        if (!hasAwoken)
                        {
                            instance.Awake();
                        }

                        if (!hasStarted)
                        {
                            instance.Start();
                        }
                    }
                }

                return instance;
            }
        }

        private ObjectPool<GameObject> _audioPlayerPool;

        private void Awake()
        {
            // Check for duplicates
            if (instance != null && instance != this)
            {
                Debug.LogError("More than one AudioManager instance detected. Destroying duplicate.");
                Destroy(this.gameObject);
                return;
            }

            instance = this;
            
            // Initialize the pool
            _audioPlayerPool = new ObjectPool<GameObject>(() => Instantiate(audioPlayerPrefab));
            // Initialize the Octree with some sensible default values.
            // This would depend on the size of your scene and the number of portals.
            PortalsTree = new Octree<AudioVolumePortal>(Vector3.zero, new Vector3(500, 500, 500), 5, 1, 10);
            
            hasAwoken = true;
        }

        private void Start()
        {
            // Add additional initialization code here if needed
            hasStarted = true;
        }
        
        public float GetOcclusionFactor(Vector3 sourcePosition, Vector3 targetPosition)
        {
            if (Physics.Linecast(sourcePosition, targetPosition, occlusionLayerMask))
            {
                return occlusionFactor;
            }
            else
            {
                return 1f;
            }
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
        /// Play an AudioObject.
        /// If RepositionToTarget is true, it uses the targetTransform's position, otherwise it uses audioPosition. 
        /// If ParentToTarget is true, the AudioPlayer becomes a child of targetTransform. 
        /// The method returns the instance of AudioPlayer that's playing the AudioObject.
        /// </summary>
        /// <param name="audioObject">The AudioObject to be played.</param>
        /// <param name="audioPosition">The position at which to play the AudioObject if RepositionToTarget is false.</param>
        /// <param name="targetTransform">The Transform at which to play the AudioObject and/or parent the AudioPlayer to if RepositionToTarget and/or ParentToTarget is true. Can be null.</param>
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
            GameObject audioPlayerGameObject = _audioPlayerPool.Get();

            // Ensure the pool has available instances
            if (audioPlayerGameObject == null)
            {
                Debug.LogError("Failed to get AudioPlayer from pool in AudioManager.");
                return null;
            }

            // Ensure the fetched object has an AudioPlayer component
            AudioPlayer audioPlayer = audioPlayerGameObject.GetComponent<AudioPlayer>();

            if (audioPlayer == null)
            {
                Debug.LogError("Fetched object from pool in AudioManager does not have an AudioPlayer component.");
                return null;
            }

            // Create a context for the AudioPlayer
            AudioPlayerContext context = new AudioPlayerContext
            {
                AudioPlayer = audioPlayer,
                AudioObject = audioObject,
                Position = audioObject.RepositionToTarget && targetTransform != null ? targetTransform.position : audioPosition,
                TargetTransform = audioObject.RepositionToTarget ? targetTransform : null,
                ClipSettings = null, // Set the clip settings here if necessary
                ParentToTarget = audioObject.ParentToTransform
            };

            // Parent the AudioPlayer to targetTransform if ParentToTarget is true
            audioPlayer.transform.SetParent(context.ParentToTarget? context.TargetTransform: null);

            // Play the audio
            audioPlayer.Play(context);

            return audioPlayer;
        }

        /*/// <summary>
        /// Add an AudioVolumePortal to the Octree.
        /// </summary>
        /// <param name="portal">The AudioVolumePortal to add.</param>
        public void RegisterAudioVolumePortal(AudioVolumePortal portal)
        {
            audioVolumePortalOctree.Insert(new OctreeNode<AudioVolumePortal>(portal, portal.transform.position));
        }

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
