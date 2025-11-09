using System.Collections;
using Life;
using UnityEngine;

public class cat : MonoBehaviour, IInteractable
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float moveDistance = 3f; // Distance to move in each direction
    [SerializeField] private float minWaitTime = 1f;
    [SerializeField] private float maxWaitTime = 3f;
    [SerializeField] private float obstacleCheckDistance = 0.5f;
    [SerializeField] private LayerMask obstacleLayer = ~0; // Check all layers by default

    [Header("Components")]
    [SerializeField] private Rigidbody2D rb; // NEEDED: For smooth physics-based movement (rb.MovePosition)
    [SerializeField] private Animator animator; // NEEDED: To play animations
    [SerializeField] private SpriteRenderer spriteRenderer; // NEEDED: For flipping sprite left/right
    [SerializeField] private CapsuleCollider2D interactionCollider; // NEEDED: Detects when player is near (must be set as Trigger)

    [Header("Animation Settings")]
    [SerializeField] private string[] movementAnimations = { "CatWalk", "CatRun" };
    [SerializeField] private string[] idleAnimations = { "CatIdle", "CatSitting", "CatLaying", "CatSleeping", "CatItch", "CatLicking", "CatMeow", "CatStretching" };

    [Header("Interaction")]
    [SerializeField] private GameObject interactablePopUp; // Popup that shows "Press E to interact"
    [SerializeField] private bool isInteractable = true;
    [SerializeField] private string pettingAnimationName = "catpet"; // Animation to play when petted

    private bool facingRight = true;
    private bool isMoving = false;
    private Coroutine currentBehaviorCoroutine;
    private Vector2 startPosition;

    public bool IsInteractable 
    { 
        get => isInteractable; 
        set => isInteractable = value; 
    }

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        // Get components if not assigned
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
        
        if (animator == null)
            animator = GetComponent<Animator>();
        
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (interactionCollider == null)
            interactionCollider = GetComponent<CapsuleCollider2D>();

        // Ensure Rigidbody2D exists and is configured
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0; // No gravity for cat
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Prevent rotation

        // Ensure interaction collider is set as trigger
        if (interactionCollider != null && !interactionCollider.isTrigger)
        {
            Debug.LogWarning("Cat's CapsuleCollider2D should be set as Trigger for interaction detection!");
        }

        // Initialize facing direction based on sprite's current state
        if (spriteRenderer != null)
        {
            facingRight = !spriteRenderer.flipX;
        }

        // Store starting position
        startPosition = transform.position;

        // Start random behavior
        if (currentBehaviorCoroutine == null)
        {
            currentBehaviorCoroutine = StartCoroutine(RandomBehaviorLoop());
        }
    }

    private IEnumerator RandomBehaviorLoop()
    {
        while (true)
        {
            // Decide what to do: move or idle
            bool shouldMove = Random.Range(0f, 1f) > 0.3f; // 70% chance to move

            if (shouldMove)
            {
                // Choose random direction
                Vector2 direction = GetRandomDirection();
                yield return StartCoroutine(MoveInDirection(direction));
            }
            else
            {
                // Play random idle animation
                yield return StartCoroutine(PlayRandomIdleAnimation());
            }

            // Wait before next action
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private Vector2 GetRandomDirection()
    {
        // Randomly choose one of four directions: x, -x, y, -y
        int direction = Random.Range(0, 4);
        switch (direction)
        {
            case 0: return Vector2.right;      // +x
            case 1: return Vector2.left;        // -x
            case 2: return Vector2.up;          // +y
            case 3: return Vector2.down;        // -y
            default: return Vector2.right;
        }
    }

    private IEnumerator MoveInDirection(Vector2 direction)
    {
        isMoving = true;
        Vector2 targetPosition = (Vector2)transform.position + direction * moveDistance;
        Vector2 startPos = transform.position;

        // Flip sprite based on horizontal movement
        if (direction.x > 0.1f && !facingRight)
        {
            Flip();
        }
        else if (direction.x < -0.1f && facingRight)
        {
            Flip();
        }

        // Play random movement animation
        if (movementAnimations.Length > 0)
        {
            string randomAnim = movementAnimations[Random.Range(0, movementAnimations.Length)];
            if (animator != null)
            {
                animator.Play(randomAnim);
            }
        }

        // Move towards target, avoiding obstacles
        float distanceToTarget = Vector2.Distance(transform.position, targetPosition);
        float distanceTraveled = 0f;

        while (distanceTraveled < moveDistance)
        {
            // Check for obstacles ahead
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, obstacleCheckDistance, obstacleLayer);
            
            if (hit.collider != null && !hit.collider.isTrigger)
            {
                // Obstacle detected, try to move around it
                Vector2 perpendicular = new Vector2(-direction.y, direction.x); // Perpendicular direction
                Vector2 avoidDirection = perpendicular * Random.Range(-1f, 1f); // Random perpendicular direction
                
                // Try moving in perpendicular direction
                Vector2 avoidPosition = (Vector2)transform.position + avoidDirection * moveDistance * 0.5f;
                
                // Check if perpendicular path is clear
                RaycastHit2D avoidHit = Physics2D.Raycast(transform.position, avoidDirection.normalized, obstacleCheckDistance, obstacleLayer);
                
                if (avoidHit.collider == null || avoidHit.collider.isTrigger)
                {
                    // Move around obstacle
                    Vector2 newPos = Vector2.MoveTowards(transform.position, avoidPosition, moveSpeed * Time.fixedDeltaTime);
                    rb.MovePosition(newPos);
                }
                else
                {
                    // Can't move around, stop here
                    break;
                }
            }
            else
            {
                // No obstacle, move normally
                Vector2 newPos = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.fixedDeltaTime);
                rb.MovePosition(newPos);
            }

            distanceTraveled = Vector2.Distance(startPos, transform.position);
            yield return new WaitForFixedUpdate();
        }

        isMoving = false;
    }

    private IEnumerator PlayRandomIdleAnimation()
    {
        if (idleAnimations.Length > 0 && animator != null)
        {
            string randomAnim = idleAnimations[Random.Range(0, idleAnimations.Length)];
            animator.Play(randomAnim);
            
            // Wait for animation to play (you can adjust this based on animation length)
            yield return new WaitForSeconds(Random.Range(1f, 3f));
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        
        // Flip using SpriteRenderer
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !facingRight;
        }
    }

    // Show popup when player enters interaction range
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        if (!isInteractable) return;
        
        ShowInteractPopUp();
    }

    // Hide popup when player leaves interaction range
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        
        HideInteractPopUp();
    }

    private void ShowInteractPopUp()
    {
        if (interactablePopUp != null)
        {
            interactablePopUp.SetActive(true);
        }
    }

    private void HideInteractPopUp()
    {
        if (interactablePopUp != null)
        {
            interactablePopUp.SetActive(false);
        }
    }

    // Interaction implementation - called when player presses E
    public void Interact(SimplePlayerController player)
    {
        if (!IsInteractable) return;

        // Stop movement
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        isMoving = false;

        // Play petting animation
        if (animator != null)
        {
            animator.Play(pettingAnimationName);
        }

        Debug.Log("Cat is being petted!");
    }

    private void OnDrawGizmosSelected()
    {
        // Draw movement distance
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, moveDistance);

        // Draw obstacle check distance
        Gizmos.color = Color.red;
        if (isMoving)
        {
            Vector2 direction = (Vector2)transform.position - startPosition;
            if (direction.magnitude > 0.1f)
            {
                Gizmos.DrawRay(transform.position, direction.normalized * obstacleCheckDistance);
            }
        }
    }
}
