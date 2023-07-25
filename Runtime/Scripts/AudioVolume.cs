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
        [SerializeField] private AudioObject previousAudioObject;

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
                if (audioObject != value)
                {
                    audioObject = value;
                    OnAudioObjectChanged();
                }
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
            previousAudioObject = audioObject;
            Play();
        }

        private void Update()
        {
            if(transform.rotation != Quaternion.identity)
            {
                Debug.LogWarning(this.name + ": AudioVolume does not support rotation at the moment. Resetting rotation to identity.");
                transform.rotation = Quaternion.identity;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }

            if (!Application.isPlaying || _audioPlayer == null || shape == null || shapeData == null)
                return;

            float blendFactor = GetBlendFactor();
            //_audioPlayer.SpatialBlendMultiplier = blendFactor;
            _audioPlayer.UpdateSpatialBlend(blendFactor);
            _audioPlayer.UpdateSpread(Mathf.Lerp(360f,0f,blendFactor));
            if(blendFactor > 0) 
            {
                _audioPlayer.transform.position = GetWorldClosestPoint(AudioVolumeListener.Instance.transform.position );
            }
        }
        
        private Vector3 GetWorldClosestPoint(Vector3 worldPosition)
        {
            Vector3 localPosition = worldPosition - transform.position;
            Vector3 localClosestPoint = shape.GetClosestPoint(localPosition, shapeData);
            return transform.position + localClosestPoint;
        }

        private float GetBlendFactor()
        {
            var listenerPos = AudioVolumeListener.Instance.transform.position;
            var localListenerPos = listenerPos - transform.position;

            if (shape.IsInside(localListenerPos, shapeData))
            {
                return 0f;
            }
            else
            {
                var localClosestPoint = shape.GetClosestPoint(localListenerPos, shapeData);
                var worldClosestPoint = localClosestPoint + transform.position;
                var distanceToEdge = Vector3.Distance(worldClosestPoint, listenerPos);

                return Mathf.Clamp01(distanceToEdge / featherZone);
            }
        }



        private void OnDrawGizmos()
        {
            if (shape != null && shapeData != null) 
            {
                shape.DrawGizmo(transform, shapeData);
                DrawDebugLine();
            }
        }

        private void DrawDebugLine()
        {
            if (!Application.isPlaying || AudioVolumeListener.Instance == null || _audioPlayer == null)
            {
                return;
            }

            Vector3 listenerPos = AudioVolumeListener.Instance.transform.position;
            Vector3 playerPos = _audioPlayer.transform.position;

            // Calculate the lerp value based on the position of the listener relative to the shape.
            float lerpValue = GetBlendFactor();

            // Lerp colors and alpha based on the lerp value.
            Color startColor = Color.green;
            Color endColor = new Color(1, 0, 0, 0.33f);
            Color color = Color.Lerp(startColor, endColor, lerpValue);

            // Draw the debug line.
            Debug.DrawLine(listenerPos, playerPos, color);
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

            if (audioObject != previousAudioObject)
            {
                AudioObject = audioObject;
                previousAudioObject = audioObject;
            }
        }

        private void OnAudioObjectChanged()
        {
            // This can be used to fire off other events in the future.
        }

        private void Play()
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
                EditorGUI.BeginChangeCheck();
                Undo.RecordObject(audioVolume, "Modified shapeData");
                audioVolume.Shape.DrawHandles(audioVolume.transform, ref audioVolume.shapeData);

                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(audioVolume);
                }
            }
        }
    }
#endif
}
