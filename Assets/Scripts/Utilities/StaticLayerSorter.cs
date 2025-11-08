using UnityEngine;

namespace Utilities
{
    public class StaticLayerSorter : MonoBehaviour
    {
        [Header("Sorting Settings")]
        [SerializeField] private int sortingOffset;
        [SerializeField] private float yMultiplier = 1000f; // Higher = more precise sorting differences

        [SerializeField] private SpriteRenderer spriteRenderer;


        private void Start()
        {
            spriteRenderer.sortingOrder = -(int)(transform.position.y * yMultiplier) + sortingOffset;
        }
    }
}