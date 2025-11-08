using System;

using UnityEngine;

namespace Utilities
{
    public static class Actions
    {
        public static Action<Vector2> OnMoveInputAction;
        public static Action OnInteractInputAction;
        
        // RoadChunk related actions
        public static Action<int> OnPlayerEnteredRoadChunkIndex;
        public static Action<Transform> OnCameraTargetTransformChanged;
        
        // Time
        public static Action<int> OnIncreaseTimeAction;
        public static Action<int> OnDecreaseTimeAction;
        public static Action OnStartTimeAction;
        public static Action OnStopTimeAction;
    }
}