using System;
using System.Linq;
using NeatWolf.Spatial.Partitioning;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NeatWolf.Audio
{
    /// <summary>
    ///     This class represents a volume of space that modifies the properties of an AudioObject.
    ///     Each volume can be assigned to a specific AudioVolumePortalGroup.
    ///     The volume will only affect the sound propagation through the portals of its group.
    /// </summary>
    [ExecuteAlways]
    public class AudioVolume : MonoBehaviour
    {
        private static readonly Type[] audioFilterTypes =
        {
            typeof(AudioLowPassFilter),
            typeof(AudioHighPassFilter),
            typeof(AudioReverbFilter),
            typeof(AudioEchoFilter),
            typeof(AudioDistortionFilter),
            typeof(AudioChorusFilter)
            // Add more types if more come out
        };

        [SerializeField] private AudioVolumeShape shape;
        [SerializeReference] [SerializeField] internal AudioVolumeShape.ShapeData shapeData;
        [SerializeField] private AudioObject audioObject;
        [SerializeField] private float featherZone;
        [SerializeField] private bool isInverted = false;
        [SerializeField] private GameObject audioFiltersOcclusionDummy;
        [SerializeField] private GameObject audioFiltersBlendDummy;
        [HideInInspector] [SerializeField] private AudioVolumeShape previousShape;
        [HideInInspector] [SerializeField] private AudioObject previousAudioObject;

        private AudioPlayer _audioPlayer;

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
        
        /// <summary>
        ///     The group this volume belongs to.
        ///     If no group is assigned, the volume will belong to the default group (null).
        /// </summary>
        [SerializeField] private AudioVolumePortalGroup portalGroup = AudioVolumePortalGroup.DEFAULT;
        public AudioVolumePortalGroup PortalGroup
        {
            get => portalGroup;
            protected set => portalGroup = value;
        }

        private void Awake()
        {
            previousShape = shape;
            previousAudioObject = audioObject;

            if (!Application.isPlaying)
                return;
            Play();
            CopyFiltersFromDummies();
        }

        private void Update()
        {
            EnsureNoRotation();
            
            if (InvalidComponents())
                return;
            
            ApplyCommonAudioPlayerUpdates();
        }

        private void ApplyCommonAudioPlayerUpdates()
        {
            var blendFactor = GetBlendFactor(isInverted);
            float occlusionVolumeMultiplier;

            Vector3 listenerPos = AudioVolumeListener.Instance.transform.position;

            AudioVolumePortal foundPortal;
            Vector3 worldClosestPoint;
            Vector3 soundSourcePosition = GetClosestAudioPosition(listenerPos, blendFactor, out foundPortal, out worldClosestPoint);

            if (foundPortal)
                blendFactor = GetBlendFactorFromPortal(listenerPos, foundPortal);
            
            // Get the occlusion factor and apply it to the volume.
            var occlusionVolumeFactor  =
                AudioManager.Instance.GetOccludedVolumeFactor(transform.position,
                    worldClosestPoint,
                    soundSourcePosition,
                    listenerPos,
                    isInverted);
            // Volume is not inverted and player is inside
            // OR
            // Volume is inverted and player is outside
            if (!isInverted == shape.IsInside(listenerPos, shapeData))
                occlusionVolumeMultiplier = occlusionVolumeFactor;
            else
                occlusionVolumeMultiplier = 1f;
            //if (isInverted)
            //    HandleInvertedVolume(blendFactor, listenerPos, closestAudioPosition, out occlusionVolumeMultiplier);
            //else
            //    HandleStandardVolume(blendFactor, listenerPos, closestAudioPosition, out occlusionVolumeMultiplier);
            
            _audioPlayer.UpdateSpatialBlend(blendFactor);
            _audioPlayer.UpdateSpread(Mathf.Lerp(360f, 0f, blendFactor));
            _audioPlayer.transform.position = soundSourcePosition;
            

            //_audioPlayer.targetVolumeMultiplier = (1-blendFactor) * occlusionVolumeMultiplier;
            _audioPlayer.targetVolumeMultiplier = occlusionVolumeMultiplier;
            
            ApplyAllEffects(blendFactor, 1 - occlusionVolumeFactor);
        }

        private void EnsureNoRotation()
        {
            if (transform.rotation != Quaternion.identity)
            {
                Debug.LogWarning(name +
                                 ": AudioVolume does not support rotation at the moment. Resetting rotation to identity.");
                transform.rotation = Quaternion.identity;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }
        
        private bool InvalidComponents()
        {
            return (!Application.isPlaying || _audioPlayer == null || shape == null || shapeData == null);
        }
        
        private Vector3 GetClosestAudioPosition(Vector3 listenerPos, float blendFactor, out AudioVolumePortal foundPortal, out Vector3 worldClosestPoint)
        {
            Vector3 soundSourcePosition;
            
            worldClosestPoint = GetWorldClosestPoint(listenerPos);
            
            // if blendFactor == 0 => full power
            OctreeNode<AudioVolumePortal> closestPortal;
            if (blendFactor >= 1f || isInverted)
                // Find the closest portal to the worldClosestPoint
            {
                closestPortal = AudioManager.Instance.GetPortals(portalGroup).FindNearestEnabledNode(worldClosestPoint);
                foundPortal = closestPortal.Data;
            }
            else foundPortal = null;


            if (foundPortal != null)
                soundSourcePosition = foundPortal.transform.position;
            else
                soundSourcePosition = worldClosestPoint;

            return soundSourcePosition;
        }
        
        protected virtual void ApplyAllEffects(float blendFactor, float occlusionFactor)
        {
            foreach (var behaviour in _audioPlayer.GetComponents<Behaviour>())
                if (IsAudioFilter(behaviour))
                    ApplyEffects(behaviour, blendFactor, occlusionFactor);
        }

        private void Update2()
        {
            if (transform.rotation != Quaternion.identity)
            {
                Debug.LogWarning(name +
                                 ": AudioVolume does not support rotation at the moment. Resetting rotation to identity.");
                transform.rotation = Quaternion.identity;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }

            if (!Application.isPlaying || _audioPlayer == null || shape == null || shapeData == null)
                return;

            var blendFactor = GetBlendFactor();
            //_audioPlayer.SpatialBlendMultiplier = blendFactor;
            _audioPlayer.UpdateSpatialBlend(blendFactor);
            _audioPlayer.UpdateSpread(Mathf.Lerp(360f, 0f, blendFactor));
            if (blendFactor > 0)
            {
                var worldClosestPoint = GetWorldClosestPoint(AudioVolumeListener.Instance.transform.position);

                // Find the closest portal to the worldClosestPoint
                var closestPortal = AudioManager.Instance.GetPortals(portalGroup).FindNearestNode(worldClosestPoint);
                Vector3 soundSourcePosition;


                if (closestPortal != null && closestPortal.Enabled)
                    // If there's an enabled portal close to the worldClosestPoint, snap the sound to the portal position.
                    soundSourcePosition = closestPortal.Position;
                else
                    // Otherwise, proceed as before.
                    soundSourcePosition = worldClosestPoint;

                _audioPlayer.transform.position = soundSourcePosition;

                // Get the occlusion factor and apply it to the volume.
                /*var occlusionFactor =
                    AudioManager.Instance.GetOccludedVolumeFactor(transform.position,
                        AudioVolumeListener.Instance.transform.position,
                        soundSourcePosition);
                _audioPlayer.targetVolumeMultiplier = occlusionFactor;*/
            }
            else
            {
                _audioPlayer.targetVolumeMultiplier = 1f;
            }

            ApplyAllEffects(blendFactor, 1 - _audioPlayer.CurrentVolumeMultiplier);
        }


        private void OnDrawGizmos()
        {
            if (shape != null && shapeData != null)
            {
                shape.DrawGizmo(transform, shapeData);
                DrawDebugLine();
            }
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

        protected virtual void CopyFiltersFromDummies()
        {
            CopyFiltersFromDummy(audioFiltersOcclusionDummy);
            CopyFiltersFromDummy(audioFiltersBlendDummy);
        }

        private void CopyFiltersFromDummy(GameObject dummy)
        {
            if (dummy == null) return;

            // Check if the dummy is a prefab.
            if (!dummy.scene.IsValid())
            {
                // Instantiate the prefab, copy the components, then immediately destroy the instance.
                var dummyInstance = Instantiate(dummy);
                CopyFilters(dummyInstance.GetComponents<Behaviour>());
                Destroy(dummyInstance);
            }
            else
            {
                // If it's not a prefab (i.e., it's an instantiated GameObject), just copy the components directly.
                CopyFilters(dummy.GetComponents<Behaviour>());
            }
        }

        private void CopyFilters(Behaviour[] filters)
        {
            foreach (var dummyFilter in filters)
                if (IsAudioFilter(dummyFilter) && dummyFilter.enabled)
                    CopyComponent(dummyFilter, _audioPlayer.gameObject);
        }


        // This is the function you need to add
        private bool IsAudioFilter(Behaviour behaviour)
        {
            var behaviourType = behaviour.GetType();
            return audioFilterTypes.Contains(behaviourType);
        }

        private Component CopyComponent(Component original, GameObject destination)
        {
            var type = original.GetType();
            var copy = destination.AddComponent(type);
            // Copied from https://answers.unity.com/questions/458207/copy-a-component-at-runtime.html
            var fields = type.GetFields();
            foreach (var field in fields) field.SetValue(copy, field.GetValue(original));
            return copy;
        }

        private void ApplyEffects(Behaviour filter, float blendFactor, float occlusionFactor)
        {
            var sumFactor = Mathf.Clamp01(blendFactor + occlusionFactor);
            var averageFactor = (blendFactor + occlusionFactor) * 0.5f;
            var multipliedFactor = blendFactor * occlusionFactor;
            var invMultipliedFactor = (1f - (1f - blendFactor) * (1f - occlusionFactor)); 

            switch (filter)
            {
                case AudioLowPassFilter lowPassFilter:
                    // The cutoff frequency decreases more when both occlusion and distance are high.
                    lowPassFilter.cutoffFrequency = Mathf.Lerp(22000, 3000, invMultipliedFactor);
                    // Lowpass resonance slightly increases with occlusion, simulating an enclosed space.
                    lowPassFilter.lowpassResonanceQ = Mathf.Lerp(1, 1.5f, invMultipliedFactor);
                    break;
                case AudioHighPassFilter highPassFilter:
                    // The high pass filter should not be applied strongly, or it will choke the sound.
                    // Instead, it should gradually take effect as the occlusion increases.
                    highPassFilter.cutoffFrequency = Mathf.Lerp(10, 500, occlusionFactor * 0.1f);
                    // Highpass resonance slightly decreases with occlusion, simulating an enclosed space.
                    highPassFilter.highpassResonanceQ = Mathf.Lerp(2, 1, occlusionFactor);
                    break;
                case AudioEchoFilter echoFilter:
                    // Echo delay slightly decreases with occlusion, simulating sound bouncing off close surfaces.
                    echoFilter.delay = Mathf.Lerp(500, 50, occlusionFactor * 0.5f);
                    // Wet mix increases more when both distance and occlusion are high.
                    echoFilter.wetMix = Mathf.Lerp(0, 1, multipliedFactor);
                    break;
                case AudioDistortionFilter distortionFilter:
                    // Distortion level increases more when both occlusion and distance are high.
                    distortionFilter.distortionLevel = Mathf.Lerp(0, 1, multipliedFactor);
                    break;
                case AudioReverbFilter reverbFilter:
                    // Dry level decreases more when both distance and occlusion are high.
                    reverbFilter.dryLevel = Mathf.Lerp(0, -2000, multipliedFactor);
                    // Decay time increases with occlusion, simulating a more enclosed space.
                    reverbFilter.decayTime = Mathf.Lerp(0.1f, 20.0f, averageFactor);
                    // Reflections level increases with occlusion, simulating a more complex sound field.
                    reverbFilter.reflectionsLevel = Mathf.Lerp(-10000, 1000, averageFactor);
                    break;
                case AudioChorusFilter chorusFilter:
                    // Wet mix increases more when both distance and occlusion are high.
                    chorusFilter.wetMix1 = Mathf.Lerp(0, 0.5f, averageFactor);
                    // Depth increases with occlusion, simulating a more complex sound field.
                    chorusFilter.depth = Mathf.Lerp(0, 1, averageFactor);
                    // Rate decreases with distance, simulating a wider spread of sound sources.
                    chorusFilter.rate = Mathf.Lerp(0.8f, 0, blendFactor);
                    break;
            }
        }


        private Vector3 GetWorldClosestPoint(Vector3 worldPosition)
        {
            var localPosition = worldPosition - transform.position;
            var localClosestPoint = shape.GetClosestPoint(localPosition, shapeData);
            return transform.position + localClosestPoint;
        }

        private float GetBlendFactor(bool invertVolume=false)
        {
            var listenerPos = AudioVolumeListener.Instance.transform.position;
            var localListenerPos = listenerPos - transform.position;

            // if the volume is not inverted, the shape must be inside.
            // And viceversa.
            if (!invertVolume == shape.IsInside(localListenerPos, shapeData))
            {
                return 0f;
            }

            var localClosestPoint = shape.GetClosestPoint(localListenerPos, shapeData);
            var worldClosestPoint = localClosestPoint + transform.position;
            var distanceToEdge = Vector3.Distance(worldClosestPoint, listenerPos);

            return Mathf.Clamp01(distanceToEdge / featherZone);
        }

        private float GetBlendFactorFromPortal(Vector3 listenerPos, AudioVolumePortal portal)
        {
            var distanceFromPortal = Vector3.Distance(portal.transform.position, listenerPos);

            var linearValue = Mathf.Clamp01(distanceFromPortal / featherZone);
            return
                Mathf.Lerp(0.5f, 1f, linearValue);
            //(1f - (1f-linearValue) * (1f-linearValue)); //quadratic, but concentrated near 1
        }

        private void DrawDebugLine()
        {
            if (!Application.isPlaying || AudioVolumeListener.Instance == null || _audioPlayer == null) return;

            var listenerPos = AudioVolumeListener.Instance.transform.position;
            var playerPos = _audioPlayer.transform.position;

            // Calculate the lerp value based on the position of the listener relative to the shape.
            var lerpValue = GetBlendFactor(isInverted);

            // Lerp colors and alpha based on the lerp value.
            var startColor = Color.green;
            var endColor = new Color(1, 0, 0, 0.33f);
            var color = Color.Lerp(startColor, endColor, lerpValue);

            // Draw the debug line.
            Debug.DrawLine(listenerPos, playerPos, color);
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
                if (_audioPlayer != null) _audioPlayer.UseSpatialBlendPlayerMultiplier = true;
                //_audioPlayer.PlayMode = 
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

                if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(audioVolume);
            }
        }
    }
#endif
}