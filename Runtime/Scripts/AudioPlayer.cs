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
            clipSettings.Volume *= Random.Range(audioObject.VolumeRange.x, audioObject.VolumeRange.y);
            clipSettings.Pitch *= Random.Range(audioObject.PitchRange.x, audioObject.PitchRange.y);
            _audioSource.pitch = clipSettings.Pitch;
            _audioSource.volume = clipSettings.Volume;
            _audioSource.panStereo = clipSettings.PanStereo;
            _audioSource.spatialBlend = audioObject.SpatialBlend;
            _audioSource.rolloffMode = audioObject.RolloffMode;
            _audioSource.outputAudioMixerGroup = audioObject.AudioChannel.ResolveMixerGroup();

            if (_audioSource.clip == null)
            {
                Debug.LogError("Audio clip is null in AudioSourcePlayer.");
                return;
            }
        
            if (_audioSource.outputAudioMixerGroup == null)
            {
                Debug.LogError("AudioMixerGroup is null in AudioSourcePlayer.");
                return;
            }
            
            _audioSource.Play();

            StartCoroutine(StopAtEndPositionCoroutine(clipSettings.EndPosition));
        }

        private IEnumerator StopAtEndPositionCoroutine(float endPosition)
        {
            yield return new WaitForSeconds(endPosition);
            _audioSource.Stop();
            GenericPool<AudioPlayer>.Release(this);
        }
        

    }

}
