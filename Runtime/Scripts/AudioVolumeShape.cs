using System;
using UnityEngine;

namespace NeatWolf.Audio
{
    /// <summary>
    /// The AudioVolumeShape class represents the abstract base class for defining 
    /// different shapes of audio volumes, each with their own specific data and behavior. 
    /// The shapes are used to determine the audio behavior based on the listener's position.
    /// </summary>
    public abstract class AudioVolumeShape : ScriptableObject
    {
        /// <summary>
        /// The ShapeData class represents the abstract base class for the data 
        /// associated with a specific AudioVolumeShape.
        /// </summary>
        [Serializable]
        public abstract class ShapeData { }
        
        /// <summary>
        /// Creates a new instance of the shape data associated with the audio volume.
        /// </summary>
        /// <returns>A new instance of the shape data.</returns>
        public abstract ShapeData CreateInstanceData();

        /// <summary>
        /// Determines if a given position is inside the audio volume.
        /// </summary>
        /// <param name="position">The position to check.</param>
        /// <param name="data">The shape data associated with the audio volume.</param>
        /// <returns>True if the position is inside the audio volume, false otherwise.</returns>
        public abstract bool IsInside(Vector3 position, ShapeData data);

        /// <summary>
        /// Gets the closest point on the audio volume to a given position.
        /// This method assumes that position is in the local space of the AudioVolume, with the origin at (0,0,0).
        /// Therefore, this method can be used to compute the closest point on the shape relative to the center of the shape.
        /// </summary>
        /// <param name="position">The position to calculate the closest point from.</param>
        /// <param name="data">The shape data associated with the audio volume.</param>
        /// <returns>The closest point on the audio volume to the given position.</returns>
        public abstract Vector3 GetClosestPoint(Vector3 position, ShapeData data);

#if UNITY_EDITOR
        /// <summary>
        /// Draws editor handles for the shape and updates the shape data based on user input. 
        /// This method can change the provided shape data.
        /// </summary>
        /// <param name="transform">The transform of the object the shape is attached to.</param>
        /// <param name="data">The shape data to be potentially modified. This is passed as a reference, 
        /// meaning changes to it inside this method will persist outside this method as well.</param>
        public abstract void DrawHandles(Transform transform, ref ShapeData data);

        /// <summary>
        /// Draws a gizmo representing the shape in the scene view, but only when the object 
        /// the shape is attached to is not selected.
        /// </summary>
        /// <param name="transform">The transform of the object the shape is attached to.</param>
        /// <param name="data">The shape data associated with the audio volume.</param>
        public abstract void DrawGizmo(Transform transform, ShapeData data);

        /// <summary>
        /// Draws a gizmo representing the shape in the scene view, but only when the object 
        /// the shape is attached to is selected.
        /// </summary>
        /// <param name="transform">The transform of the object the shape is attached to.</param>
        /// <param name="featherRadius">The feather radius of the audio volume.</param>
        /// <param name="data">The shape data associated with the audio volume.</param>
        /// <param name="featherSteps">The number of steps to use for feathering. The default is 2.</param>
        public abstract void DrawGizmoSelected(Transform transform, float featherRadius, ShapeData data, int featherSteps=2);
#endif
    }
}
