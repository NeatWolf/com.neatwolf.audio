using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace NeatWolf.Audio
{
    /// <summary>
    /// This class represents an AudioManager, responsible for managing the playing of AudioObjects.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }
        [FormerlySerializedAs("audioSourcePrefab")] public AudioPlayer audioPrefab;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Play a sound at a given position.
        /// </summary>
        /// <param name="audioObject">The AudioObject to be played.</param>
        /// <param name="position">The position at which to play the sound.</param>
        public void PlaySoundAtPosition(AudioObject audioObject, Vector3 position)
        {
            AudioPlayer audioPlayer = UnityEngine.Pool.GenericPool<AudioPlayer>.Get();
            audioPlayer.transform.SetParent(transform);
            audioPlayer.PlayAtPosition(audioObject, position);
        }
    }
}
