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

        private static AudioManager instance;
        private static bool hasAwoken = false;
        private static bool hasStarted = false;

        /// <summary>
        /// Singleton instance of the AudioManager.
        /// </summary>
        public static AudioManager Instance 
        { 
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<AudioManager>();

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
            hasAwoken = true;
        }

        private void Start()
        {
            // Add additional initialization code here if needed
            hasStarted = true;
        }

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
    }
}
