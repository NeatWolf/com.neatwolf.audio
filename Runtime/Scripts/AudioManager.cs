using UnityEngine;
using UnityEngine.Pool;

namespace NeatWolf.Audio
{
    /// <summary>
    /// This class represents an AudioManager, responsible for managing the playing of AudioObjects.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private GameObject audioPlayerPrefab; // Assign this in the inspector
        
        public static AudioManager Instance { get; private set; }

        private ObjectPool<GameObject> _audioPlayerPool;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogError("More than one AudioManager instance detected. Only one AudioManager should exist in the scene.");
                Destroy(gameObject);
                return;
            }
            
            
            // Initialize the pool
            _audioPlayerPool = new ObjectPool<GameObject>(() => Instantiate(audioPlayerPrefab));

        }

        /// <summary>
        /// Play a sound at a given position.
        /// </summary>
        /// <param name="audioObject">The AudioObject to be played.</param>
        /// <param name="position">The position at which to play the sound.</param>
        public void PlaySoundAtPosition(AudioObject audioObject, Vector3 position)
        {
            if (audioObject == null)
            {
                Debug.LogError("AudioObject is null in AudioManager's PlaySoundAtPosition.");
                return;
            }

            // Fetch an AudioSourcePlayer from the pool
            GameObject audioPlayerGameObject = _audioPlayerPool.Get();

            if (audioPlayerGameObject == null)
            {
                Debug.LogError("Failed to get AudioSourcePlayer from pool in AudioManager.");
                return;
            }
            
            AudioPlayer audioSourcePlayer = audioPlayerGameObject.GetComponent<AudioPlayer>();
            if (audioSourcePlayer == null)
            {
                Debug.LogError("Failed to get AudioSourcePlayer from pool in AudioManager.");
                return;
            }

            audioSourcePlayer.transform.SetParent(transform);
            audioSourcePlayer.PlayAtPosition(audioObject, position);
        }
    }
}
