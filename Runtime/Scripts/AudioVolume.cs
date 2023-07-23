using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NeatWolf.Audio
{
    [ExecuteAlways]
    public class AudioVolume : MonoBehaviour
    {
        [SerializeField] private AudioVolumeShape shape;
        [SerializeReference, SerializeField] internal AudioVolumeShape.ShapeData shapeData;
        [SerializeField] private AudioObject audioObject;
        [SerializeField] private float featherZone;

        private AudioPlayer _audioPlayer;
        [SerializeField] private AudioVolumeShape previousShape;

        public AudioVolumeShape Shape
        {
            get => shape;
            set
            {
                shape = value;
                shapeData = shape != null ? shape.CreateInstanceData() : null;
            }
        }

        public AudioObject AudioObject
        {
            get => audioObject;
            set
            {
                audioObject = value;
                OnAudioObjectChanged();
            }
        }

        public float FeatherZone
        {
            get => featherZone;
            set => featherZone = Mathf.Max(0, value);
        }

        private void Awake()
        {
            previousShape = shape;
            OnAudioObjectChanged();
        }

        private void Update()
        {
            if (!Application.isPlaying || _audioPlayer == null || shape == null || shapeData == null)
                return;

            var listenerPos = AudioVolumeListener.Instance.transform.position;
            var distanceToEdge = Vector3.Distance(shape.GetClosestPoint(listenerPos, shapeData), listenerPos);

            if (shape.IsInside(listenerPos, shapeData))
            {
                _audioPlayer.SpatialBlendMultiplier = 0f;
            }
            else
            {
                _audioPlayer.SpatialBlendMultiplier = Mathf.Clamp01(distanceToEdge / featherZone);
                _audioPlayer.transform.position = shape.GetClosestPoint(listenerPos, shapeData);
            }
        }

        private void OnDrawGizmos()
        {
            if (shape != null && shapeData != null) Shape.DrawGizmo(transform, shapeData);
        }

        private void OnDrawGizmosSelected()
        {
            if (shape != null && shapeData != null) Shape.DrawGizmoSelected(transform, featherZone, shapeData, 5);
        }

        private void OnValidate()
        {
            if (shape != previousShape)
            {
                Shape = shape;
                previousShape = shape;
            }
            OnAudioObjectChanged();
        }

        private void OnAudioObjectChanged()
        {
            if (!Application.isPlaying)
                return;

            if (audioObject != null)
            {
                _audioPlayer = AudioManager.Instance.Play(audioObject, transform.position);
                if (_audioPlayer != null)
                {
                    _audioPlayer.UseSpatialBlendPlayerMultiplier = true;
                    //_audioPlayer.PlayMode = 
                }
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(AudioVolume))]
    public class AudioVolumeEditor : Editor
    {
        protected virtual void OnSceneGUI()
        {
            var audioVolume = (AudioVolume)target;
            if (audioVolume.Shape != null && audioVolume.shapeData != null)
            {
                EditorGUI.BeginChangeCheck(); // Start change check
                Undo.RecordObject(audioVolume, "Modified shapeData");
                audioVolume.Shape.DrawHandles(audioVolume.transform, ref audioVolume.shapeData);

                if (EditorGUI.EndChangeCheck()) // If changes were detected
                {
                    //Undo.RecordObject(audioVolume, "Modified AudioVolume shape"); // Register Undo
                    //Debug.Log("Recording Undo");
                    EditorUtility.SetDirty(audioVolume); // Mark object as dirty
                }
            }
        }
    }
#endif
}
