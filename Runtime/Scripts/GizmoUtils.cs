using UnityEngine;

namespace NeatWolf.Audio
{
    public static class GizmoUtils
    {
        public static void DrawWireCubeRounded(Vector3 center, Vector3 size, float featherRadius, int steps)
        {
            DrawFaces(center, size, featherRadius);
            DrawSides(center, size, featherRadius, steps);
        }

        public static void DrawFaces(Vector3 center, Vector3 size, float featherRadius)
        {
            Vector3 halfSize = size / 2;

            // Top face
            Vector3 topFaceCenter = center + Vector3.up * featherRadius;
            Gizmos.DrawLine(topFaceCenter + new Vector3(-halfSize.x, halfSize.y, -halfSize.z), topFaceCenter + new Vector3(halfSize.x, halfSize.y, -halfSize.z));
            Gizmos.color = Color.red;
            Gizmos.DrawLine(topFaceCenter + new Vector3(halfSize.x, halfSize.y, -halfSize.z), topFaceCenter + new Vector3(halfSize.x, halfSize.y, halfSize.z));
            Gizmos.color = Color.green;
            Gizmos.DrawLine(topFaceCenter + new Vector3(halfSize.x, halfSize.y, halfSize.z), topFaceCenter + new Vector3(-halfSize.x, halfSize.y, halfSize.z));
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(topFaceCenter + new Vector3(-halfSize.x, halfSize.y, halfSize.z), topFaceCenter + new Vector3(-halfSize.x, halfSize.y, -halfSize.z));

            // Bottom face
            Vector3 bottomFaceCenter = center - Vector3.up * featherRadius;
            Gizmos.DrawLine(bottomFaceCenter + new Vector3(-halfSize.x, -halfSize.y, -halfSize.z), bottomFaceCenter + new Vector3(halfSize.x, -halfSize.y, -halfSize.z));
            Gizmos.DrawLine(bottomFaceCenter + new Vector3(halfSize.x, -halfSize.y, -halfSize.z), bottomFaceCenter + new Vector3(halfSize.x, -halfSize.y, halfSize.z));
            Gizmos.DrawLine(bottomFaceCenter + new Vector3(halfSize.x, -halfSize.y, halfSize.z), bottomFaceCenter + new Vector3(-halfSize.x, -halfSize.y, halfSize.z));
            Gizmos.DrawLine(bottomFaceCenter + new Vector3(-halfSize.x, -halfSize.y, halfSize.z), bottomFaceCenter + new Vector3(-halfSize.x, -halfSize.y, -halfSize.z));

            // Front face
            Vector3 frontFaceCenter = center + Vector3.forward * featherRadius;
            Gizmos.DrawLine(frontFaceCenter + new Vector3(-halfSize.x, -halfSize.y, halfSize.z), frontFaceCenter + new Vector3(halfSize.x, -halfSize.y, halfSize.z));
            Gizmos.color = Color.red;
            Gizmos.DrawLine(frontFaceCenter + new Vector3(halfSize.x, -halfSize.y, halfSize.z), frontFaceCenter + new Vector3(halfSize.x, halfSize.y, halfSize.z));
            Gizmos.color = Color.green;
            Gizmos.DrawLine(frontFaceCenter + new Vector3(halfSize.x, halfSize.y, halfSize.z), frontFaceCenter + new Vector3(-halfSize.x, halfSize.y, halfSize.z));
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(frontFaceCenter + new Vector3(-halfSize.x, halfSize.y, halfSize.z), frontFaceCenter + new Vector3(-halfSize.x, -halfSize.y, halfSize.z));

            Gizmos.color = Color.green;
            Gizmos.DrawLine(center + new Vector3(halfSize.x, halfSize.y, halfSize.z), center + new Vector3(-halfSize.x, halfSize.y, halfSize.z));
            
            // Back face
            Vector3 backFaceCenter = center - Vector3.forward * featherRadius;
            Gizmos.DrawLine(backFaceCenter + new Vector3(-halfSize.x, -halfSize.y, -halfSize.z), backFaceCenter + new Vector3(halfSize.x, -halfSize.y, -halfSize.z));
            Gizmos.DrawLine(backFaceCenter + new Vector3(halfSize.x, -halfSize.y, -halfSize.z), backFaceCenter + new Vector3(halfSize.x, halfSize.y, -halfSize.z));
            Gizmos.DrawLine(backFaceCenter + new Vector3(halfSize.x, halfSize.y, -halfSize.z), backFaceCenter + new Vector3(-halfSize.x, halfSize.y, -halfSize.z));
            Gizmos.DrawLine(backFaceCenter + new Vector3(-halfSize.x, halfSize.y, -halfSize.z), backFaceCenter + new Vector3(-halfSize.x, -halfSize.y, -halfSize.z));

            // Left face
            Vector3 leftFaceCenter = center + Vector3.right * featherRadius;
            Gizmos.DrawLine(leftFaceCenter + new Vector3(-halfSize.x, -halfSize.y, -halfSize.z), leftFaceCenter + new Vector3(-halfSize.x, -halfSize.y, halfSize.z));
            Gizmos.DrawLine(leftFaceCenter + new Vector3(-halfSize.x, -halfSize.y, halfSize.z), leftFaceCenter + new Vector3(-halfSize.x, halfSize.y, halfSize.z));
            Gizmos.DrawLine(leftFaceCenter + new Vector3(-halfSize.x, halfSize.y, halfSize.z), leftFaceCenter + new Vector3(-halfSize.x, halfSize.y, -halfSize.z));
            Gizmos.DrawLine(leftFaceCenter + new Vector3(-halfSize.x, halfSize.y, -halfSize.z), leftFaceCenter + new Vector3(-halfSize.x, -halfSize.y, -halfSize.z));

            // Right face
            Vector3 rightFaceCenter = center - Vector3.right * featherRadius;
            Gizmos.DrawLine(rightFaceCenter + new Vector3(halfSize.x, -halfSize.y, -halfSize.z), rightFaceCenter + new Vector3(halfSize.x, -halfSize.y, halfSize.z));
            Gizmos.DrawLine(rightFaceCenter + new Vector3(halfSize.x, -halfSize.y, halfSize.z), rightFaceCenter + new Vector3(halfSize.x, halfSize.y, halfSize.z));
            Gizmos.DrawLine(rightFaceCenter + new Vector3(halfSize.x, halfSize.y, halfSize.z), rightFaceCenter + new Vector3(halfSize.x, halfSize.y, -halfSize.z));
            Gizmos.DrawLine(rightFaceCenter + new Vector3(halfSize.x, halfSize.y, -halfSize.z), rightFaceCenter + new Vector3(halfSize.x, -halfSize.y, -halfSize.z));
        }
        
        public static void DrawSides(Vector3 center, Vector3 size, float featherRadius, int steps)
        {
            Vector3 halfSize = size / 2;

            Vector3 topLineStart = center + Vector3.up * featherRadius + new Vector3(halfSize.x, halfSize.y, halfSize.z);
            Vector3 topLineEnd = center + Vector3.up * featherRadius + new Vector3(-halfSize.x, halfSize.y, halfSize.z);

            Vector3 frontLineStart = center + Vector3.forward * featherRadius + new Vector3(halfSize.x, -halfSize.y, halfSize.z);
            Vector3 frontLineEnd = center + Vector3.forward * featherRadius + new Vector3(-halfSize.x, halfSize.y, halfSize.z);

            Vector3 innerLineStart = center + new Vector3(halfSize.x, -halfSize.y, halfSize.z);
            Vector3 innerLineEnd = center + new Vector3(-halfSize.x, halfSize.y, halfSize.z);

            // Calculate the rectangle's size based on the arc dimensions and steps count
            Vector3 rectSize = new Vector3((topLineEnd - topLineStart).magnitude / steps, (frontLineEnd - frontLineStart).magnitude, featherRadius);

            Vector3 rotationAxis = (innerLineEnd - innerLineStart).normalized;

            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;

                // Calculate the rotation for the rectangle based on the spherical interpolation of the directions
                Vector3 startDir = (topLineStart - innerLineStart).normalized;
                Vector3 endDir = (frontLineStart - innerLineStart).normalized;
                Quaternion rot = Quaternion.FromToRotation(startDir, endDir);

                Quaternion slerpRot = Quaternion.Slerp(Quaternion.identity, rot, t);
                Quaternion finalRot = Quaternion.AngleAxis(90f, rotationAxis) * slerpRot;

                // Calculate the position for the rectangle's center
                Vector3 rectCenter = center + slerpRot * startDir * featherRadius;

                // Draw the rectangle
                DrawRectangle(rectCenter, rectSize, finalRot);
            }
        }


        public static void DrawRectangle(Vector3 center, Vector3 size, Quaternion rotation)
        {
            Vector3 halfSize = size / 2;

            Vector3[] corners = new Vector3[4]
            {
                center + rotation * new Vector3(-halfSize.x, -halfSize.y, 0), // Bottom left corner
                center + rotation * new Vector3(halfSize.x, -halfSize.y, 0), // Bottom right corner
                center + rotation * new Vector3(halfSize.x, halfSize.y, 0), // Top right corner
                center + rotation * new Vector3(-halfSize.x, halfSize.y, 0) // Top left corner
            };

            Gizmos.DrawLine(corners[0], corners[1]);
            Gizmos.DrawLine(corners[1], corners[2]);
            Gizmos.DrawLine(corners[2], corners[3]);
            Gizmos.DrawLine(corners[3], corners[0]);
        }
    }
}