using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

namespace NeatWolf.Audio
{
    /// <summary>
    /// This class represents an AudioPlayer, responsible for playing an AudioObject at a certain position.
    /// An AudioSource component must be present on the same gameobject.
    /// Usually this is made into a prefab
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioPlayer : MonoBehaviour
    {
        /// <summary>
        /// Configurator that sets up the AudioSource with data from an AudioObject.
        /// </summary>
        [SerializeField, Tooltip("Configurator that sets up the AudioSource with data from an AudioObject.")]
        private AudioSourceConfigurator _configurator;

        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        public void PlayAtPosition(AudioObject audioObject, Vector3 position, ClipSettings clipSettings = null)
        {
            // Position the AudioSource at the desired position
            transform.position = position;

            // Configure the AudioSource and get the settings
            AudioSourceSettings settings = _configurator.Configure(_audioSource, audioObject, clipSettings);

            // Error checking
            if (settings.AudioClip == null)
            {
                Debug.LogError("Audio clip is null in AudioSourcePlayer.");
                return;
            }

            if (_audioSource.outputAudioMixerGroup == null)
            {
                Debug.LogError("AudioMixerGroup is null in AudioSourcePlayer.");
                return;
            }

            // Play the AudioSource
            _audioSource.Play();

            // Stop playback at the end of the clip
            StartCoroutine(StopAtEndPositionCoroutine(settings.Duration));
        }

        private IEnumerator StopAtEndPositionCoroutine(float endPosition)
        {
            yield return new WaitForSeconds(endPosition);
            _audioSource.Stop();
            GenericPool<AudioPlayer>.Release(this);
        }
    }
}
