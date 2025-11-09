using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Life
{
    public class SimplePlayerController : MonoBehaviour
    {
        [Header("Movement")] [SerializeField] private float moveSpeed = 5f;
        
        [Header("Movement Bounds")] [SerializeField]
        private bool useXBounds = true;
    public void DisableObject()
    {
        gameObject.SetActive(false);
    }
   
    public void EnableObject()
    {
        gameObject.SetActive(true);
    }

        [SerializeField] private float minX = -10f;
        [SerializeField] private float maxX = 10f;

        [SerializeField] private bool useYBounds = true;
        [SerializeField] private float minY = -5f;
        [SerializeField] private float maxY = 5f;

        [Header("Visual")] [SerializeField] private Transform spriteTransform;
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRend;
        [SerializeField] private Rigidbody2D rb;

        [Header("Interaction")] [SerializeField]
        private float interactionRange = 2f;

        [SerializeField] private KeyCode interactKey = KeyCode.E;

        private bool facingRight = true;
        private List<IInteractable> interactablesInRange = new List<IInteractable>();
        private Vector2 moveInput;
        private bool isMoving;

        private void Start()
        {
            // If no sprite transform is assigned, use self
            if (spriteTransform == null)
            {
                spriteTransform = transform;
            }

            SimpleInputManager.Instance.SetContext(SimpleInputManager.InputContext.Gameplay);
        }

        private void Update()
        {
            // Get WASD input
            float moveInputX = SimpleInputManager.Instance.GetAxis("Horizontal"); // A/D or Left/Right arrows
            float moveInputY = SimpleInputManager.Instance.GetAxis("Vertical"); // W/S or Up/Down arrows

            moveInput = new Vector2(moveInputX, moveInputY);

            // Check if player is moving
            isMoving = moveInput.sqrMagnitude > 0.1f;
            animator.Play(isMoving ? $"Move" : $"Idle");

            switch (moveInputX)
            {
                // Flip sprite based on horizontal movement
                case > 0.1f when !facingRight:
                case < -0.1f when facingRight:
                    Flip();
                    break;
            }

            // Handle interaction
            if (SimpleInputManager.Instance.GetButtonDown("Interact"))
            {
                TryInteract();
            }

            // Update sorting order based on Y position (optional for 2.5D games)
            // UpdateSortingOrder();
        }

        private void FixedUpdate()
        {
            // Calculate movement
            Vector2 movement = moveInput.normalized * (moveSpeed * Time.fixedDeltaTime);
            Vector2 newPosition = rb.position + movement;

            // Apply bounds to the new position
            if (useXBounds)
            {
                newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            }

            if (useYBounds)
            {
                newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);
            }

            // Move using Rigidbody2D.MovePosition
            rb.MovePosition(newPosition);
        }

        private void Flip()
        {
            facingRight = !facingRight;
            Vector3 scale = spriteTransform.localScale;
            scale.x *= -1;
            spriteTransform.localScale = scale;
        }

        private void UpdateSortingOrder()
        {
            // Objects lower on screen (lower Y) should be drawn in front
            if (spriteRend == null) return;
            spriteRend.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100);
        }

        private void TryInteract()
        {
            Debug.Log("Attempting to interact...");
            if (interactablesInRange.Count == 0)
            {
                Debug.Log("Nothing to interact with nearby");
                return;
            }

            // Find the closest interactable
            IInteractable closestInteractable = null;
            float closestDistance = float.MaxValue;

            foreach (IInteractable interactable in interactablesInRange)
            {
                if (interactable == null) continue;

                MonoBehaviour interactableMB = interactable as MonoBehaviour;
                if (interactableMB == null) continue;

                float distance = Vector3.Distance(transform.position, interactableMB.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = interactable;
                }
            }

            // Interact with the closest one
            if (closestInteractable != null)
            {
                closestInteractable.Interact(this);
            }
        }

        // Detect interactables entering range
        private void OnTriggerEnter2D(Collider2D other)
        {
            IInteractable interactable = other.GetComponent<IInteractable>();
            if (interactable != null && !interactablesInRange.Contains(interactable))
            {
                interactablesInRange.Add(interactable);
                Debug.Log($"Can interact with: {other.gameObject.name}");
            }

            if (other.GetComponent<RoadChunk>())
            {
                Debug.Log(
                    $"Entered RoadChunk: {other.gameObject.name} with index {other.GetComponent<RoadChunk>().chunkIndex}");
                Actions.OnPlayerEnteredRoadChunkIndex?.Invoke(other.GetComponent<RoadChunk>().chunkIndex);
            }
        }

        // Detect interactables leaving range
        private void OnTriggerExit2D(Collider2D other)
        {
            IInteractable interactable = other.GetComponent<IInteractable>();
            if (interactable != null && interactablesInRange.Contains(interactable))
            {
                interactablesInRange.Remove(interactable);
                Debug.Log($"Out of range: {other.gameObject.name}");
            }
        }

        // Visualize bounds in editor
        void OnDrawGizmosSelected()
        {
            // Draw X bounds
            if (useXBounds)
            {
                Gizmos.color = Color.red;
                Vector3 pos = transform.position;

                // Left bound
                Gizmos.DrawLine(
                    new Vector3(minX, pos.y - 10, pos.z),
                    new Vector3(minX, pos.y + 10, pos.z)
                );

                // Right bound
                Gizmos.DrawLine(
                    new Vector3(maxX, pos.y - 10, pos.z),
                    new Vector3(maxX, pos.y + 10, pos.z)
                );
            }

            // Draw Y bounds
            if (useYBounds)
            {
                Gizmos.color = Color.green;
                Vector3 pos = transform.position;

                // Bottom bound
                Gizmos.DrawLine(
                    new Vector3(pos.x - 10, minY, pos.z),
                    new Vector3(pos.x + 10, minY, pos.z)
                );

                // Top bound
                Gizmos.DrawLine(
                    new Vector3(pos.x - 10, maxY, pos.z),
                    new Vector3(pos.x + 10, maxY, pos.z)
                );
            }

            // Draw movement area
            if (useXBounds && useYBounds)
            {
                Gizmos.color = new Color(1, 1, 0, 0.2f);
                Vector3 center = new Vector3((minX + maxX) / 2, (minY + maxY) / 2, transform.position.z);
                Vector3 size = new Vector3(maxX - minX, maxY - minY, 0.1f);
                Gizmos.DrawCube(center, size);
            }

            // Draw interaction range
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }

    // Interface for interactable objects
    public interface IInteractable
    {
        public bool IsInteractable { get; set; }
        void Interact(SimplePlayerController player);
    }

    // Example interactable object - attach this to objects you want to interact with
    public class ExampleInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string interactionMessage = "Hello!";

        public bool IsInteractable { get; set; }

        public void Interact(SimplePlayerController player)
        {
            Debug.Log($"Interacted: {interactionMessage}");
            // Add your interaction logic here
            // For example: open a door, pick up an item, talk to NPC, etc.
        }

        void OnDrawGizmos()
        {
            // Draw a small indicator for interactable objects
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }
    }
}