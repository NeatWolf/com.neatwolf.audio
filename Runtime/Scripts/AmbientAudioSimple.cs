using UnityEngine;

namespace NeatWolf.Audio
{
    public class AmbientAudioSimple: MonoBehaviour
    {
        public AudioObject audioObject;

        [SerializeField]
        private bool playOnAwake;

        private AudioPlayer _player;

        private void Awake()
        {
            if (playOnAwake)
                _player = AudioManager.Instance.Play(audioObject, Vector3.zero);
        }
    }
}