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
        
        // Money
        public static Action<int> OnIncreaseMoneyAction;
        public static Action<int> OnDecreaseMoneyAction;
        public static Action<int> OnMoneyChangedTo;
        
        // Energy
        public static Action<int> OnIncreaseEnergyAction;
        public static Action<int> OnDecreaseEnergyAction;
        public static Action<int> OnEnergyChangedTo;
        
        // Time
        public static Action<int> OnIncreaseTimeAction;
        public static Action<int> OnDecreaseTimeAction;
        public static Action OnStartTimeAction;
        public static Action OnStopTimeAction;
    }
}