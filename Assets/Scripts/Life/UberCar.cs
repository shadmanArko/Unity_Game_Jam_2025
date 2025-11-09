using System;
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
        
        [Header("Road Bounds (World Space)")]
        [SerializeField] private float roadCenterY = 0f; // Y position of road center
        [SerializeField] private float roadHalfWidth = 2f; // Half width of road
    
        [Header("Obstacle Avoidance")]
        [SerializeField] private float earlyAvoidanceDistance = 5f;
        [SerializeField] private float detectionWidth = 1f;
        [SerializeField] private LayerMask obstacleLayer;
        [SerializeField] private float avoidanceOffset = 1.5f;
        [SerializeField] private float safetyMargin = 0.3f;
    
        [Header("Stuck Detection")]
        [SerializeField] private float stuckCheckInterval = 2f; // Check every N seconds
        [SerializeField] private float stuckDistanceThreshold = 0.5f; // If moved less than this, consider stuck
        [SerializeField] private float stuckTimeThreshold = 3f; // Time to consider stuck
    
        [Header("References")]
        [SerializeField] private Transform player;
        [SerializeField] private Transform destinationTarget;
    
        [Header("Pickup Settings")]
        [SerializeField] private float pickupRadius = 2f;
        [SerializeField] private float waitTimeBeforeStart = 1f;
        public bool isBike = false;
        private Animator _animator;
        
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
        
        // Stuck detection variables
        private Vector3 lastPositionCheck;
        private float stuckCheckTimer = 0f;
        private float stuckTimer = 0f;
        private bool isStuck = false;
        
        // Computed road bounds
        private float minY => roadCenterY - roadHalfWidth;
        private float maxY => roadCenterY + roadHalfWidth;

        private void OnEnable()
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            destinationTarget = GameObject.FindGameObjectWithTag("Office")?.transform;
        }

        void Start()
        {
            targetY = transform.position.y;
            lastAvoidanceCheckY = transform.position.y;
            lastPositionCheck = transform.position;
        
            if (obstacleLayer == 0)
            {
                obstacleLayer = LayerMask.GetMask("Default");
            }

            _animator = GetComponent<Animator>();
            
        }
    
        void Update()
        {
            switch (currentState)
            {
                case CarState.ComingToPlayer:
                    MoveToPlayer();
                    CheckIfStuck();
                    HandleStuckWhileComingToPlayer();
                    break;
                
                case CarState.WaitingForPlayer:
                    CheckForPlayerNearby();
                    break;
                
                case CarState.MovingToDestination:
                    MoveToDestination();
                    CheckIfStuck();
                    HandleStuckWhileMovingToDestination();
                    break;
            }
        
            // Always apply Y movement smoothly
            if (currentState != CarState.Idle && currentState != CarState.Stopped)
            {
                ApplyYMovement();
            }
        }
        
        private void CheckIfStuck()
        {
            stuckCheckTimer += Time.deltaTime;
            
            if (stuckCheckTimer >= stuckCheckInterval)
            {
                float distanceMoved = Vector3.Distance(transform.position, lastPositionCheck);
                
                if (distanceMoved < stuckDistanceThreshold)
                {
                    // Car hasn't moved much
                    stuckTimer += stuckCheckTimer;
                    
                    if (stuckTimer >= stuckTimeThreshold && !isStuck)
                    {
                        isStuck = true;
                        Debug.LogWarning("Uber car is stuck!");
                    }
                }
                else
                {
                    // Car is moving, reset stuck timer
                    stuckTimer = 0f;
                    if (isStuck)
                    {
                        isStuck = false;
                        Debug.Log("Uber car is no longer stuck.");
                    }
                }
                
                lastPositionCheck = transform.position;
                stuckCheckTimer = 0f;
            }
        }
        
        private void HandleStuckWhileComingToPlayer()
        {
            if (!isStuck) return;
            
            // If stuck while coming to player, check if player is in range
            if (player != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, player.position);
                
                if (distanceToPlayer <= pickupRadius)
                {
                    Debug.Log("Car is stuck but player is in range. Picking up player!");
                    StartRide();
                    isStuck = false;
                    stuckTimer = 0f;
                }
            }
        }
        
        private void HandleStuckWhileMovingToDestination()
        {
            if (!isStuck) return;
            
            // If stuck while going to destination, drop player here
            Debug.Log("Car is stuck while going to destination. Dropping player!");
            StopCar();
            isStuck = false;
            stuckTimer = 0f;
        }
    
        [ContextMenu("call Uber")]
        public void CallUber()
        {
            if (player == null)
            {
                Debug.LogError("Player reference not set!");
                return;
            }
        
            playerPickupPosition = player.position;
            isReversing = transform.position.x > playerPickupPosition.x;
            targetY = transform.position.y;
            lastAvoidanceCheckY = transform.position.y;
            
            // Reset stuck detection
            lastPositionCheck = transform.position;
            stuckCheckTimer = 0f;
            stuckTimer = 0f;
            isStuck = false;
        
            currentState = CarState.ComingToPlayer;
        
            Debug.Log($"Uber called! Coming to player. Reversing: {isReversing}");
        }
    
        private void MoveToPlayer()
        {
            float distanceX = Mathf.Abs(transform.position.x - playerPickupPosition.x);
        
            if (distanceX <= stopDistance)
            {
                currentState = CarState.WaitingForPlayer;
                waitTimer = 0f;
                Debug.Log("Uber arrived at pickup location!");
                return;
            }
        
            CheckAndAvoidObstacles(isReversing);
        
            float direction = isReversing ? -1f : 1f;
            float speed = isReversing ? reverseSpeed : moveSpeed;
        
            Vector3 newPos = transform.position;
            newPos.x += direction * speed * Time.deltaTime;
            transform.position = newPos;
        
            UpdateSpriteDirection(direction);
        }
    
        private void CheckAndAvoidObstacles(bool checkReverse)
        {
            Vector3 rayOrigin = transform.position;
            Vector3 rayDirection = checkReverse ? Vector3.left : Vector3.right;
            float checkDistance = earlyAvoidanceDistance;
        
            int rayCount = 5;
            float heightStep = (detectionWidth * 2f) / (rayCount - 1);
            
            RaycastHit2D closestObstacle = new RaycastHit2D();
            float closestDistance = Mathf.Infinity;
            bool obstacleFound = false;
        
            for (int i = 0; i < rayCount; i++)
            {
                float yOffset = -detectionWidth + (i * heightStep);
                Vector3 rayStart = rayOrigin + Vector3.up * yOffset;
                
                RaycastHit2D hit = Physics2D.Raycast(rayStart, rayDirection, checkDistance, obstacleLayer);
                
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
                
                float obstacleTop = obstacleY + (obstacleHeight / 2f) + safetyMargin;
                float obstacleBottom = obstacleY - (obstacleHeight / 2f) - safetyMargin;
                
                bool canGoUp = (currentY + avoidanceOffset) <= maxY;
                bool canGoDown = (currentY - avoidanceOffset) >= minY;
                
                if (obstacleY > currentY)
                {
                    if (canGoDown)
                    {
                        targetY = obstacleBottom - avoidanceOffset;
                        isAvoidingObstacle = true;
                    }
                    else if (canGoUp)
                    {
                        targetY = obstacleTop + avoidanceOffset;
                        isAvoidingObstacle = true;
                    }
                }
                else
                {
                    if (canGoUp)
                    {
                        targetY = obstacleTop + avoidanceOffset;
                        isAvoidingObstacle = true;
                    }
                    else if (canGoDown)
                    {
                        targetY = obstacleBottom - avoidanceOffset;
                        isAvoidingObstacle = true;
                    }
                }
                
                targetY = Mathf.Clamp(targetY, minY, maxY);
                lastAvoidanceCheckY = targetY;
                
                Debug.DrawLine(transform.position, closestObstacle.point, Color.magenta);
                Debug.DrawLine(rayOrigin, new Vector3(rayOrigin.x, targetY, rayOrigin.z), Color.cyan);
            }
            else
            {
                if (isAvoidingObstacle)
                {
                    float centerY = roadCenterY;
                    
                    if (Mathf.Abs(transform.position.y - lastAvoidanceCheckY) > 0.2f)
                    {
                        targetY = Mathf.MoveTowards(targetY, centerY, laneChangeSpeed * Time.deltaTime * 0.3f);
                        
                        if (Mathf.Abs(targetY - centerY) < 0.2f)
                        {
                            isAvoidingObstacle = false;
                        }
                    }
                    else
                    {
                        float returnSpeed = laneChangeSpeed * Time.deltaTime * 0.5f;
                        targetY = Mathf.MoveTowards(targetY, centerY, returnSpeed);
                    }
                }
            }
        }
    
        private void ApplyYMovement()
        {
            Vector3 newPos = transform.position;
            newPos.y = Mathf.MoveTowards(newPos.y, targetY, laneChangeSpeed * Time.deltaTime);
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
            isReversing = false;
            
            // Reset stuck detection for destination journey
            lastPositionCheck = transform.position;
            stuckCheckTimer = 0f;
            stuckTimer = 0f;
            isStuck = false;
            
            Debug.Log("Player entered! Starting ride to destination.");
        
            if (player != null)
            {
                var playerController = player.GetComponent<SimplePlayerController>();
                if (playerController != null)
                {
                    playerController.DisableObject();
                    Actions.OnCameraTargetTransformChanged?.Invoke(transform);
                    if (_animator != null)
                        _animator.SetTrigger("OnPassangerBoard");
                }
            }
        }
    
        private void MoveToDestination()
        {
            CheckAndAvoidObstacles(false);
        
            if (destinationTarget != null)
            {
                float targetX = destinationTarget.position.x;
            
                float distanceX = Mathf.Abs(transform.position.x - targetX);
                if (distanceX <= stopDistance)
                {
                    StopCar();
                    return;
                }
            }
        
            Vector3 newPos = transform.position;
            newPos.x += moveSpeed * Time.deltaTime;
            transform.position = newPos;
        
            UpdateSpriteDirection(1f);
        }
    
        public void StopCar()
        {
            currentState = CarState.Stopped;
            isStuck = false;
            stuckTimer = 0f;
            Debug.Log("Uber stopped!");
        
            if (player != null)
            {
                var playerController = player.GetComponent<SimplePlayerController>();
                if (playerController != null)
                {
                    playerController.transform.position = transform.position + Vector3.up * 2f;
                    playerController.EnableObject();
                    Actions.OnCameraTargetTransformChanged?.Invoke(playerController.transform);
                    if (_animator != null)
                        _animator.SetTrigger("OnPassangerLeft");
                    Actions.OnPlayerDroppedOff?.Invoke(isBike);
                }
            }
        }

        

        public void ResetCar()
        {
            currentState = CarState.Idle;
            waitTimer = 0f;
            isReversing = false;
            isAvoidingObstacle = false;
            targetY = transform.position.y;
            lastAvoidanceCheckY = transform.position.y;
            isStuck = false;
            stuckTimer = 0f;
            stuckCheckTimer = 0f;
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
        
        public bool IsCarStuck()
        {
            return isStuck;
        }
    
        private void OnDrawGizmosSelected()
        {
            // Draw road bounds in WORLD SPACE (fixed position)
            Gizmos.color = Color.cyan;
            float roadVisualLength = 100f;
            Vector3 roadLeft = new Vector3(-roadVisualLength, roadCenterY + roadHalfWidth, 0);
            Vector3 roadRight = new Vector3(roadVisualLength, roadCenterY + roadHalfWidth, 0);
            Vector3 roadLeftBottom = new Vector3(-roadVisualLength, roadCenterY - roadHalfWidth, 0);
            Vector3 roadRightBottom = new Vector3(roadVisualLength, roadCenterY - roadHalfWidth, 0);
            
            Gizmos.DrawLine(roadLeft, roadRight);
            Gizmos.DrawLine(roadLeftBottom, roadRightBottom);
            
            // Draw road center line
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(new Vector3(-roadVisualLength, roadCenterY, 0), 
                           new Vector3(roadVisualLength, roadCenterY, 0));
        
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
                
                if (isAvoidingObstacle)
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawLine(transform.position, new Vector3(transform.position.x, targetY, transform.position.z));
                }
                
                // Draw stuck indicator
                if (isStuck)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(transform.position, 1f);
                    Gizmos.DrawLine(transform.position + Vector3.up, transform.position + Vector3.down);
                    Gizmos.DrawLine(transform.position + Vector3.left, transform.position + Vector3.right);
                }
            }
        }
    }
}