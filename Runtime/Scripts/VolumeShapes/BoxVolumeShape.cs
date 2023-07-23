using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NeatWolf.Audio
{
    [Serializable]
    [CreateAssetMenu(menuName = "Audio/BoxVolumeShape", fileName = "BoxVolumeShape")]
    public class BoxVolumeShape : AudioVolumeShape
    {
        [Serializable]
        public class BoxShapeData : ShapeData
        {
            public Vector3 Center;
            public Vector3 Size = Vector3.one;
        }
        
        public override ShapeData CreateInstanceData()
        {
            return new BoxShapeData();
        }

        public override bool IsInside(Vector3 position, ShapeData data)
        {
            BoxShapeData boxData = (BoxShapeData) data;
            Vector3 relativePos = position - boxData.Center;
            return Mathf.Abs(relativePos.x) <= boxData.Size.x / 2f && Mathf.Abs(relativePos.y) <= boxData.Size.y / 2f && Mathf.Abs(relativePos.z) <= boxData.Size.z / 2f;
        }

        public override Vector3 GetClosestPoint(Vector3 position, ShapeData data)
        {
            BoxShapeData boxData = (BoxShapeData) data;
            return Vector3.Max(boxData.Center - boxData.Size / 2f, Vector3.Min(boxData.Center + boxData.Size / 2f, position));
        }

#if UNITY_EDITOR
        public override void DrawHandles(Transform transform, ref ShapeData shapeData)
        {
            BoxShapeData boxData = (BoxShapeData) shapeData;
            Vector3[] directions = { Vector3.right, Vector3.up, Vector3.forward, Vector3.left, Vector3.down, Vector3.back };

            for (int i = 0; i < directions.Length; i++)
            {
                Vector3 direction = directions[i];
                Vector3 handlePos = transform.position + transform.rotation * (boxData.Center + Vector3.Scale(direction, boxData.Size) / 2);

                Vector3 newHandlePos = Handles.FreeMoveHandle(handlePos, HandleUtility.GetHandleSize(handlePos) * 0.1f, Vector3.zero, Handles.DotHandleCap);
                
                Vector3 diff = (newHandlePos - handlePos) * 0.5f;
                Vector3 localDiff = Quaternion.Inverse(transform.rotation) * diff;

                if (i < 3)
                {
                    boxData.Size += 2 * direction * Vector3.Dot(localDiff, direction);
                    boxData.Size = Vector3.Max(Vector3.zero, boxData.Size);
                }
                else
                {
                    boxData.Size -= 2 * direction * Vector3.Dot(localDiff, direction);
                    boxData.Size = Vector3.Max(Vector3.zero, boxData.Size);
                }

                boxData.Center += Vector3.Dot(localDiff, direction) * direction;
            }
        }
        
        public override void DrawGizmo(Transform transform, ShapeData data)
        {
            BoxShapeData boxData = (BoxShapeData) data;
            Gizmos.matrix = Matrix4x4.TRS(transform.position + boxData.Center, transform.rotation, transform.localScale);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(Vector3.zero, boxData.Size);
        }

        public override void DrawGizmoSelected(Transform transform, float featherRadius, ShapeData data, int featherSteps = 2)
        {
            BoxShapeData boxData = (BoxShapeData) data;
            Gizmos.matrix = Matrix4x4.TRS(transform.position + boxData.Center, transform.rotation, transform.localScale);
            Gizmos.color = new Color(0, 1, 1, 0.5f);
            Gizmos.DrawWireCube(Vector3.zero, boxData.Size + featherRadius * 2 * Vector3.one);
        }
#endif
    }
}
