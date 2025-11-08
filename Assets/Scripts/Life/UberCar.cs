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
        [SerializeField] private float detectionDistance = 3f;
        [SerializeField] private float detectionWidth = 0.8f;
        [SerializeField] private LayerMask obstacleLayer;
        [SerializeField] private float avoidanceOffset = 1f;
        [SerializeField] private float earlyAvoidanceDistance = 5f; // Start avoiding earlier
    
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
    
        void Start()
        {
            targetY = transform.position.y;
        
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
            // Cast a ray in the direction of movement
            Vector3 rayOrigin = transform.position;
            Vector3 rayDirection = checkReverse ? Vector3.left : Vector3.right;
        
            // Use early avoidance distance for detection
            float checkDistance = earlyAvoidanceDistance;
        
            // Draw debug rays
            Debug.DrawRay(rayOrigin, rayDirection * checkDistance, Color.red);
            Debug.DrawRay(rayOrigin + Vector3.up * detectionWidth, rayDirection * checkDistance, Color.yellow);
            Debug.DrawRay(rayOrigin + Vector3.down * detectionWidth, rayDirection * checkDistance, Color.yellow);
        
            // Check for obstacles ahead with longer distance
            RaycastHit2D centerHit = Physics2D.Raycast(rayOrigin, rayDirection, checkDistance, obstacleLayer);
            RaycastHit2D upperHit = Physics2D.Raycast(rayOrigin + Vector3.up * detectionWidth, rayDirection, checkDistance, obstacleLayer);
            RaycastHit2D lowerHit = Physics2D.Raycast(rayOrigin + Vector3.down * detectionWidth, rayDirection, checkDistance, obstacleLayer);
        
            RaycastHit2D obstacleHit = new RaycastHit2D();
            bool obstacleDetected = false;
        
            // Check if any hit an obstacle and get the closest one
            if (centerHit.collider != null && centerHit.collider.CompareTag("Obstacle"))
            {
                obstacleDetected = true;
                obstacleHit = centerHit;
            }
            if (upperHit.collider != null && upperHit.collider.CompareTag("Obstacle"))
            {
                if (!obstacleDetected || upperHit.distance < obstacleHit.distance)
                {
                    obstacleDetected = true;
                    obstacleHit = upperHit;
                }
            }
            if (lowerHit.collider != null && lowerHit.collider.CompareTag("Obstacle"))
            {
                if (!obstacleDetected || lowerHit.distance < obstacleHit.distance)
                {
                    obstacleDetected = true;
                    obstacleHit = lowerHit;
                }
            }
        
            if (obstacleDetected)
            {
                isAvoidingObstacle = true;
            
                // Get obstacle position
                float obstacleY = obstacleHit.collider.transform.position.y;
                float currentY = transform.position.y;
            
                // Smart avoidance: go opposite direction from obstacle
                // If obstacle is higher, go lower. If obstacle is lower, go higher.
                if (obstacleY > currentY)
                {
                    // Obstacle is above, move down
                    targetY = currentY - avoidanceOffset;
                
                    // Make sure we have enough space
                    if (targetY < minY)
                    {
                        // Can't go down enough, try going up instead
                        targetY = currentY + avoidanceOffset;
                    }
                }
                else
                {
                    // Obstacle is below or at same level, move up
                    targetY = currentY + avoidanceOffset;
                
                    // Make sure we have enough space
                    if (targetY > maxY)
                    {
                        // Can't go up enough, try going down instead
                        targetY = currentY - avoidanceOffset;
                    }
                }
            
                // Clamp target Y within road bounds
                targetY = Mathf.Clamp(targetY, minY, maxY);
            
                Debug.DrawLine(transform.position, obstacleHit.point, Color.magenta);
            }
            else
            {
                // No obstacle, return to center lane gradually
                if (isAvoidingObstacle)
                {
                    float centerY = (minY + maxY) / 2f;
                    targetY = Mathf.MoveTowards(targetY, centerY, Time.deltaTime * 0.5f);
                
                    // Check if back to center
                    if (Mathf.Abs(transform.position.y - centerY) < 0.1f)
                    {
                        isAvoidingObstacle = false;
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
            float targetX;
            if (destinationTarget != null)
            {
                targetX = destinationTarget.position.x;
            
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
                    playerController.transform.position = transform.position + Vector3.up * 2f; // Place player next to car
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
        }
    
        private void UpdateSpriteDirection(float directionX)
        {
            // Flip sprite based on movement direction
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
            Gizmos.DrawLine(transform.position + Vector3.up * detectionWidth, 
                transform.position + Vector3.up * detectionWidth + detectionDir * earlyAvoidanceDistance);
            Gizmos.DrawLine(transform.position + Vector3.down * detectionWidth, 
                transform.position + Vector3.down * detectionWidth + detectionDir * earlyAvoidanceDistance);
        
            // Draw target Y
            if (Application.isPlaying && (currentState == CarState.ComingToPlayer || currentState == CarState.MovingToDestination))
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(new Vector3(transform.position.x, targetY, transform.position.z), 0.3f);
            }
        }
    }
}