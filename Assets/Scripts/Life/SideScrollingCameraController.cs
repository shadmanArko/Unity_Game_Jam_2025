using System;
using UnityEngine;
using Utilities;

public class SideScrollingCameraController : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform player;
    
    [Header("Camera Offset")]
    [SerializeField] private float xOffset = 0f;
    [SerializeField] private float yOffset = 2f;
    [SerializeField] private float zOffset = -10f;
    
    [Header("Smoothing/Damping")]
    [SerializeField] private float xDamping = 2f;
    [SerializeField] private float yDamping = 2f;
    [SerializeField] private float zDamping = 2f;
    
    [Header("Y Bounds")]
    [SerializeField] private bool useYBounds = true;
    [SerializeField] private float minY = 0f;
    [SerializeField] private float maxY = 10f;
    
    [Header("Optional X Bounds")]
    [SerializeField] private bool useXBounds = false;
    [SerializeField] private float minX = -50f;
    [SerializeField] private float maxX = 50f;
    
    [Header("Look Ahead")]
    [SerializeField] private bool useLookAhead = true;
    [SerializeField] private float lookAheadDistanceX = 3f;
    [SerializeField] private float lookAheadDistanceY = 1f;
    [SerializeField] private float lookAheadDamping = 3f;
    [SerializeField] private float lookAheadThreshold = 0.1f; // Minimum speed to trigger look ahead
    
    private Vector3 velocity = Vector3.zero;
    private Vector3 lastPlayerPosition;
    private Vector3 currentLookAhead = Vector3.zero;
    private Vector3 lookAheadVelocity = Vector3.zero;
    
    void Start()
    {
        if (player != null)
        {
            lastPlayerPosition = player.position;
        }
        Actions.OnCameraTargetTransformChanged += SetTarget;
    }
    
    void LateUpdate()
    {
        if (player == null)
        {
            Debug.LogWarning("Player transform not assigned to camera controller!");
            return;
        }
        
        // Calculate player movement direction
        Vector3 playerVelocity = (player.position - lastPlayerPosition) / Time.deltaTime;
        
        // Calculate look ahead offset based on movement direction
        Vector3 targetLookAhead = Vector3.zero;
        
        if (useLookAhead)
        {
            // Check if player is moving fast enough to trigger look ahead
            if (Mathf.Abs(playerVelocity.x) > lookAheadThreshold)
            {
                // Negative sign removed - now matches movement direction
                targetLookAhead.x = Mathf.Sign(playerVelocity.x) * lookAheadDistanceX;
            }
            
            if (Mathf.Abs(playerVelocity.y) > lookAheadThreshold)
            {
                targetLookAhead.y = Mathf.Sign(playerVelocity.y) * lookAheadDistanceY;
            }
            
            // Smooth the look ahead transition
            currentLookAhead = Vector3.SmoothDamp(
                currentLookAhead, 
                targetLookAhead, 
                ref lookAheadVelocity, 
                1f / lookAheadDamping
            );
        }
        
        // Update last position for next frame
        lastPlayerPosition = player.position;
        
        // Calculate target position with offsets and look ahead
        Vector3 targetPos = new Vector3(
            player.position.x + xOffset + currentLookAhead.x,
            player.position.y + yOffset + currentLookAhead.y,
            player.position.z + zOffset
        );
        
        // Apply Y bounds
        if (useYBounds)
        {
            targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);
        }
        
        // Apply X bounds (optional)
        if (useXBounds)
        {
            targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
        }
        
        // Smooth movement with individual damping per axis
        Vector3 currentPos = transform.position;
        Vector3 newPos = new Vector3(
            Mathf.SmoothDamp(currentPos.x, targetPos.x, ref velocity.x, 1f / xDamping),
            Mathf.SmoothDamp(currentPos.y, targetPos.y, ref velocity.y, 1f / yDamping),
            Mathf.SmoothDamp(currentPos.z, targetPos.z, ref velocity.z, 1f / zDamping)
        );
        
        transform.position = newPos;
    }
    
    // Helper method to set player at runtime
    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
    }
    
    // Helper method to set Y bounds at runtime
    public void SetYBounds(float min, float max)
    {
        minY = min;
        maxY = max;
        useYBounds = true;
    }
    public void SetTarget(Transform newTarget)
    {
        player = newTarget;
    }
    // Visualize bounds in editor
    void OnDrawGizmosSelected()
    {
        Camera cam = GetComponent<Camera>();
        if (cam == null) return;
        
        // Calculate camera view size
        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;
        
        if (useYBounds)
        {
            // Draw the camera view rectangle at MIN Y bound
            Gizmos.color = Color.red;
            Vector3 minBoundCenter = new Vector3(transform.position.x, minY, 0);
            Vector3 viewSize = new Vector3(camWidth, camHeight, 0.1f);
            Gizmos.DrawWireCube(minBoundCenter, viewSize);
            
            // Draw label line
            Gizmos.DrawLine(
                new Vector3(transform.position.x - camWidth/2f - 2f, minY, 0),
                new Vector3(transform.position.x - camWidth/2f, minY, 0)
            );
            
            // Draw the camera view rectangle at MAX Y bound
            Gizmos.color = Color.green;
            Vector3 maxBoundCenter = new Vector3(transform.position.x, maxY, 0);
            Gizmos.DrawWireCube(maxBoundCenter, viewSize);
            
            // Draw label line
            Gizmos.DrawLine(
                new Vector3(transform.position.x - camWidth/2f - 2f, maxY, 0),
                new Vector3(transform.position.x - camWidth/2f, maxY, 0)
            );
            
            // Draw connecting lines between bounds
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(
                new Vector3(transform.position.x - camWidth/2f, minY - camHeight/2f, 0),
                new Vector3(transform.position.x - camWidth/2f, maxY - camHeight/2f, 0)
            );
            Gizmos.DrawLine(
                new Vector3(transform.position.x + camWidth/2f, minY - camHeight/2f, 0),
                new Vector3(transform.position.x + camWidth/2f, maxY - camHeight/2f, 0)
            );
        }
        
        if (useXBounds)
        {
            // Draw the camera view rectangle at MIN X bound
            Gizmos.color = Color.cyan;
            Vector3 minXCenter = new Vector3(minX, transform.position.y, 0);
            Vector3 viewSize = new Vector3(camWidth, camHeight, 0.1f);
            Gizmos.DrawWireCube(minXCenter, viewSize);
            
            // Draw the camera view rectangle at MAX X bound
            Gizmos.color = Color.magenta;
            Vector3 maxXCenter = new Vector3(maxX, transform.position.y, 0);
            Gizmos.DrawWireCube(maxXCenter, viewSize);
        }
        
        // Draw current camera view
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position, new Vector3(camWidth, camHeight, 0.1f));
    }

    private void OnDisable()
    {
        Actions.OnCameraTargetTransformChanged -= SetTarget;
    }
}