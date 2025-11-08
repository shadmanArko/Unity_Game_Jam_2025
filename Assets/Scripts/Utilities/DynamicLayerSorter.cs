using System;
using UnityEngine;

namespace Utilities
{
    public class DynamicLayerSorter : MonoBehaviour
    {
        [Header("Sorting Settings")]
        [SerializeField] private int sortingOffset;
        [SerializeField] private float yMultiplier = 1000f; // Higher = more precise sorting differences

        [SerializeField] private SpriteRenderer spriteRenderer;
        
        private void LateUpdate()
        {
            spriteRenderer.sortingOrder = -(int)(transform.position.y * yMultiplier) + sortingOffset;
        }
    }
}