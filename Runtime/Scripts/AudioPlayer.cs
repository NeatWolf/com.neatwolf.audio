using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeatWolf.Audio
{
    /// <summary>
    /// This class represents an AudioPlayer, responsible for playing an AudioObject at a certain position.
    /// An AudioSource component must be present on the same gameobject.
    /// Usually this is made into a prefab
    /// </summary>
    public class AudioPlayer : MonoBehaviour
    {
        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        /// <summary>
        /// Play an AudioObject at a given position.
        /// </summary>
        /// <param name="audioObject">The AudioObject to be played.</param>
        /// <param name="position">The position at which to play the AudioObject.</param>
        public void PlayAtPosition(AudioObject audioObject, Vector3 position)
        {
            transform.position = position;
            ClipSettings clipSettings;
            _audioSource.clip = audioObject.GetClipSettings(out clipSettings);
            _audioSource.pitch = clipSettings.Pitch;
            _audioSource.volume = clipSettings.Volume;
            _audioSource.panStereo = clipSettings.PanStereo;
            _audioSource.spatialBlend = audioObject.SpatialBlend;
            _audioSource.outputAudioMixerGroup = audioObject.AudioChannel.ResolveMixerGroup();

            _audioSource.Play();

            StartCoroutine(StopAtEndPositionCoroutine(clipSettings.EndPosition));
        }

        private IEnumerator StopAtEndPositionCoroutine(float endPosition)
        {
            yield return new WaitForSeconds(endPosition);
            _audioSource.Stop();
            UnityEngine.Pool.GenericPool<AudioPlayer>.Release(this);
        }
    }

}
