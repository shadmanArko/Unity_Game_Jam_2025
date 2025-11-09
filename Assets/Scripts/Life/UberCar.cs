using UnityEngine;
using Utilities;

namespace Life
{
    public class UberCar : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float reverseSpeed = 3f;
        [SerializeField] private float stopDistance = 0.5f;
        [SerializeField] private float laneChangeSpeed = 3f;
    
        [Header("Road Bounds")]
        [SerializeField] private float minY = -2f;
        [SerializeField] private float maxY = 2f;
    
        [Header("Obstacle Avoidance")]
        [SerializeField] private float earlyAvoidanceDistance = 5f;
        [SerializeField] private float detectionWidth = 1f;
        [SerializeField] private LayerMask obstacleLayer;
        [SerializeField] private float avoidanceOffset = 1.5f;
        [SerializeField] private float safetyMargin = 0.3f; // Extra margin from obstacles
    
        [Header("References")]
        [SerializeField] private Transform player;
        [SerializeField] private Transform destinationTarget;
    
        [Header("Pickup Settings")]
        [SerializeField] private float pickupRadius = 2f;
        [SerializeField] private float waitTimeBeforeStart = 1f;

        public enum CarState
        {
            Idle,
            ComingToPlayer,
            WaitingForPlayer,
            MovingToDestination,
            Stopped
        }
    
        private CarState currentState = CarState.Idle;
        private Vector3 playerPickupPosition;
        private bool isReversing = false;
        private float waitTimer = 0f;
        private float targetY;
        private bool isAvoidingObstacle = false;
        private float lastAvoidanceCheckY;
    
        void Start()
        {
            targetY = transform.position.y;
            lastAvoidanceCheckY = transform.position.y;
        
            // If obstacle layer not set, try to find it
            if (obstacleLayer == 0)
            {
                obstacleLayer = LayerMask.GetMask("Default");
            }
        }
    
        void Update()
        {
            switch (currentState)
            {
                case CarState.ComingToPlayer:
                    MoveToPlayer();
                    break;
                
                case CarState.WaitingForPlayer:
                    CheckForPlayerNearby();
                    break;
                
                case CarState.MovingToDestination:
                    MoveToDestination();
                    break;
            }
        
            // Always apply Y movement smoothly
            if (currentState != CarState.Idle && currentState != CarState.Stopped)
            {
                ApplyYMovement();
            }
        }
    
        /// <summary>
        /// Call this method to summon the Uber car to the player
        /// </summary>
        [ContextMenu("call Uber")]
        public void CallUber()
        {
            if (player == null)
            {
                Debug.LogError("Player reference not set!");
                return;
            }
        
            playerPickupPosition = player.position;
        
            // Determine if car needs to reverse based on position
            isReversing = transform.position.x > playerPickupPosition.x;
        
            // Set initial target Y to current Y
            targetY = transform.position.y;
            lastAvoidanceCheckY = transform.position.y;
        
            currentState = CarState.ComingToPlayer;
        
            Debug.Log($"Uber called! Coming to player. Reversing: {isReversing}");
        }
    
        private void MoveToPlayer()
        {
            float distanceX = Mathf.Abs(transform.position.x - playerPickupPosition.x);
        
            if (distanceX <= stopDistance)
            {
                // Arrived at player X position
                currentState = CarState.WaitingForPlayer;
                waitTimer = 0f;
                Debug.Log("Uber arrived at pickup location!");
                return;
            }
        
            // Check for obstacles and adjust Y if needed
            CheckAndAvoidObstacles(isReversing);
        
            // Move in X direction only
            float direction = isReversing ? -1f : 1f;
            float speed = isReversing ? reverseSpeed : moveSpeed;
        
            Vector3 newPos = transform.position;
            newPos.x += direction * speed * Time.deltaTime;
            transform.position = newPos;
        
            // Update sprite direction
            UpdateSpriteDirection(direction);
        }
    
        private void CheckAndAvoidObstacles(bool checkReverse)
        {
            Vector3 rayOrigin = transform.position;
            Vector3 rayDirection = checkReverse ? Vector3.left : Vector3.right;
            float checkDistance = earlyAvoidanceDistance;
        
            // Multiple detection rays at different heights for better coverage
            int rayCount = 5;
            float heightStep = (detectionWidth * 2f) / (rayCount - 1);
            
            RaycastHit2D closestObstacle = new RaycastHit2D();
            float closestDistance = Mathf.Infinity;
            bool obstacleFound = false;
        
            // Cast multiple rays for better detection
            for (int i = 0; i < rayCount; i++)
            {
                float yOffset = -detectionWidth + (i * heightStep);
                Vector3 rayStart = rayOrigin + Vector3.up * yOffset;
                
                RaycastHit2D hit = Physics2D.Raycast(rayStart, rayDirection, checkDistance, obstacleLayer);
                
                // Debug visualization
                Color rayColor = hit.collider != null && hit.collider.CompareTag("Obstacle") ? Color.red : Color.green;
                Debug.DrawRay(rayStart, rayDirection * checkDistance, rayColor);
                
                if (hit.collider != null && hit.collider.CompareTag("Obstacle"))
                {
                    if (hit.distance < closestDistance)
                    {
                        closestDistance = hit.distance;
                        closestObstacle = hit;
                        obstacleFound = true;
                    }
                }
            }
        
            if (obstacleFound)
            {
                float obstacleY = closestObstacle.collider.bounds.center.y;
                float obstacleHeight = closestObstacle.collider.bounds.size.y;
                float currentY = transform.position.y;
                
                // Determine best avoidance direction
                float obstacleTop = obstacleY + (obstacleHeight / 2f) + safetyMargin;
                float obstacleBottom = obstacleY - (obstacleHeight / 2f) - safetyMargin;
                
                // Smart avoidance logic
                bool canGoUp = (currentY + avoidanceOffset) <= maxY;
                bool canGoDown = (currentY - avoidanceOffset) >= minY;
                
                // If obstacle is higher than car, prefer going down (lower Y)
                if (obstacleY > currentY)
                {
                    if (canGoDown)
                    {
                        targetY = obstacleBottom - avoidanceOffset;
                        isAvoidingObstacle = true;
                    }
                    else if (canGoUp)
                    {
                        // Can't go down, must go up
                        targetY = obstacleTop + avoidanceOffset;
                        isAvoidingObstacle = true;
                    }
                }
                // If obstacle is lower than car, prefer going up (higher Y)
                else
                {
                    if (canGoUp)
                    {
                        targetY = obstacleTop + avoidanceOffset;
                        isAvoidingObstacle = true;
                    }
                    else if (canGoDown)
                    {
                        // Can't go up, must go down
                        targetY = obstacleBottom - avoidanceOffset;
                        isAvoidingObstacle = true;
                    }
                }
                
                // Clamp to road bounds
                targetY = Mathf.Clamp(targetY, minY, maxY);
                lastAvoidanceCheckY = targetY;
                
                // Visual feedback
                Debug.DrawLine(transform.position, closestObstacle.point, Color.magenta);
                Debug.DrawLine(rayOrigin, new Vector3(rayOrigin.x, targetY, rayOrigin.z), Color.cyan);
            }
            else
            {
                // No obstacle detected - gradually return to center
                if (isAvoidingObstacle)
                {
                    // Check if we're clear of obstacles before returning to center
                    float centerY = (minY + maxY) / 2f;
                    
                    // Only return to center if we're far from last avoidance
                    if (Mathf.Abs(transform.position.y - lastAvoidanceCheckY) > 0.2f)
                    {
                        targetY = Mathf.MoveTowards(targetY, centerY, laneChangeSpeed * Time.deltaTime * 0.3f);
                        
                        // Reset avoidance flag when close to center
                        if (Mathf.Abs(targetY - centerY) < 0.2f)
                        {
                            isAvoidingObstacle = false;
                        }
                    }
                    else
                    {
                        // Still in avoidance maneuver
                        float returnSpeed = laneChangeSpeed * Time.deltaTime * 0.5f;
                        targetY = Mathf.MoveTowards(targetY, centerY, returnSpeed);
                    }
                }
            }
        }
    
        private void ApplyYMovement()
        {
            // Smoothly move to target Y
            Vector3 newPos = transform.position;
            newPos.y = Mathf.MoveTowards(newPos.y, targetY, laneChangeSpeed * Time.deltaTime);
        
            // Clamp Y within road bounds
            newPos.y = Mathf.Clamp(newPos.y, minY, maxY);
        
            transform.position = newPos;
        }
    
        private void CheckForPlayerNearby()
        {
            if (player == null) return;
        
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
            if (distanceToPlayer <= pickupRadius)
            {
                waitTimer += Time.deltaTime;
            
                if (waitTimer >= waitTimeBeforeStart)
                {
                    StartRide();
                }
            }
            else
            {
                waitTimer = 0f;
            }
        }
    
        private void StartRide()
        {
            currentState = CarState.MovingToDestination;
            isReversing = false; // Always move forward to destination
            Debug.Log("Player entered! Starting ride to destination.");
        
            // Disable player control
            if (player != null)
            {
                var playerController = player.GetComponent<SimplePlayerController>();
                if (playerController != null)
                {
                    playerController.DisableObject();
                    Actions.OnCameraTargetTransformChanged?.Invoke(transform);
                }
            }
        }
    
        private void MoveToDestination()
        {
            // Check for obstacles and adjust Y if needed
            CheckAndAvoidObstacles(false);
        
            // Determine target X position
            if (destinationTarget != null)
            {
                float targetX = destinationTarget.position.x;
            
                // Check if reached destination
                float distanceX = Mathf.Abs(transform.position.x - targetX);
                if (distanceX <= stopDistance)
                {
                    StopCar();
                    return;
                }
            }
        
            // Move in positive X direction
            Vector3 newPos = transform.position;
            newPos.x += moveSpeed * Time.deltaTime;
            transform.position = newPos;
        
            UpdateSpriteDirection(1f);
        }
    
        /// <summary>
        /// Call this to stop the car
        /// </summary>
        public void StopCar()
        {
            currentState = CarState.Stopped;
            Debug.Log("Uber stopped!");
        
            // Re-enable player control
            if (player != null)
            {
                var playerController = player.GetComponent<SimplePlayerController>();
                if (playerController != null)
                {
                    playerController.transform.position = transform.position + Vector3.up * 2f;
                    playerController.EnableObject();
                    Actions.OnCameraTargetTransformChanged?.Invoke(playerController.transform);
                }
            }
        }
    
        /// <summary>
        /// Reset the car to idle state
        /// </summary>
        public void ResetCar()
        {
            currentState = CarState.Idle;
            waitTimer = 0f;
            isReversing = false;
            isAvoidingObstacle = false;
            targetY = transform.position.y;
            lastAvoidanceCheckY = transform.position.y;
        }
    
        private void UpdateSpriteDirection(float directionX)
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                if (isReversing)
                {
                    spriteRenderer.flipX = directionX > 0;
                }
                else
                {
                    spriteRenderer.flipX = directionX < 0;
                }
            }
        }
    
        // Public getters
        public bool IsMoving()
        {
            return currentState == CarState.ComingToPlayer || currentState == CarState.MovingToDestination;
        }
    
        public bool IsWaitingForPlayer()
        {
            return currentState == CarState.WaitingForPlayer;
        }
    
        public CarState GetCurrentState()
        {
            return currentState;
        }
    
        // Visualize in editor
        private void OnDrawGizmosSelected()
        {
            // Draw road bounds
            Gizmos.color = Color.cyan;
            Vector3 leftBound = transform.position + Vector3.left * 50f;
            Vector3 rightBound = transform.position + Vector3.right * 50f;
        
            Gizmos.DrawLine(leftBound + Vector3.up * maxY, rightBound + Vector3.up * maxY);
            Gizmos.DrawLine(leftBound + Vector3.up * minY, rightBound + Vector3.up * minY);
        
            // Draw pickup radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, pickupRadius);
        
            // Draw detection range
            Gizmos.color = Color.red;
            Vector3 detectionDir = isReversing ? Vector3.left : Vector3.right;
            Gizmos.DrawLine(transform.position, transform.position + detectionDir * earlyAvoidanceDistance);
            
            // Draw detection width
            Gizmos.DrawLine(transform.position + Vector3.up * detectionWidth, 
                transform.position + Vector3.up * detectionWidth + detectionDir * earlyAvoidanceDistance);
            Gizmos.DrawLine(transform.position + Vector3.down * detectionWidth, 
                transform.position + Vector3.down * detectionWidth + detectionDir * earlyAvoidanceDistance);
        
            // Draw target Y
            if (Application.isPlaying && (currentState == CarState.ComingToPlayer || currentState == CarState.MovingToDestination))
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(new Vector3(transform.position.x, targetY, transform.position.z), 0.3f);
                
                // Draw avoidance state
                if (isAvoidingObstacle)
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawLine(transform.position, new Vector3(transform.position.x, targetY, transform.position.z));
                }
            }
        }
    }
}