using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NeatWolf.Audio
{
    [Serializable]
    [CreateAssetMenu(menuName = "Audio/SphereVolumeShape", fileName = "SphereVolumeShape")]
    public class SphereVolumeShape : AudioVolumeShape
    {
        [Serializable]
        public class SphereShapeData : ShapeData
        {
            public Vector3 Center;
            public float Radius = 1f;
        }

        public override ShapeData CreateInstanceData()
        {
            return new SphereShapeData();
        }

        public override bool IsInside(Vector3 position, ShapeData data)
        {
            SphereShapeData sphereData = (SphereShapeData) data;
            Vector3 relativePos = position - sphereData.Center;
            return relativePos.sqrMagnitude <= sphereData.Radius * sphereData.Radius;
        }

        public override Vector3 GetClosestPoint(Vector3 position, ShapeData data)
        {
            SphereShapeData sphereData = (SphereShapeData) data;
            Vector3 directionFromCenter = (position - sphereData.Center).normalized;
            return sphereData.Center + directionFromCenter * sphereData.Radius;
        }

#if UNITY_EDITOR
        public override void DrawHandles(Transform transform, ref ShapeData shapeData)
        {
            SphereShapeData sphereData = (SphereShapeData) shapeData;
            Vector3 globalCenter = transform.position + sphereData.Center;
            
            //EditorGUI.BeginChangeCheck();
            float newRadius = Handles.RadiusHandle(transform.rotation, globalCenter, sphereData.Radius, true);
            //if (EditorGUI.EndChangeCheck())
            //{
            //    Undo.RecordObject(this, "Changed Sphere Volume Size");
                sphereData.Radius = newRadius;
            //}
        }

        public override void DrawGizmo(Transform transform, ShapeData data)
        {
            SphereShapeData sphereData = (SphereShapeData) data;
            Gizmos.matrix = Matrix4x4.TRS(transform.position + sphereData.Center, transform.rotation, Vector3.one);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(Vector3.zero, sphereData.Radius);
        }

        public override void DrawGizmoSelected(Transform transform, float featherRadius, ShapeData data, int featherSteps=2)
        {
            SphereShapeData sphereData = (SphereShapeData) data;
            Gizmos.matrix = Matrix4x4.TRS(transform.position + sphereData.Center, transform.rotation, Vector3.one);
            Gizmos.color = new Color(0, 1, 1, 0.5f);
            Gizmos.DrawWireSphere(Vector3.zero, sphereData.Radius + featherRadius);
        }
#endif
    }
}
