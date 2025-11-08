using UnityEngine;

namespace _Scripts.InputSystem
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 5f;
    
        [Header("Jump Settings")]
        public float jumpHeight = 2f;      // How high the sprite goes
        public float jumpDuration = 0.6f;  // How long the jump takes
        private float jumpTimer = 0f;
        private bool isJumping = false;
        private float groundYPosition;     // The actual ground position
    
        [Header("Visual")]
        public Transform spriteTransform;  // The visual sprite
        public Transform shadowTransform;  // The shadow sprite
    
        [Header("Depth Settings")]
        public float minDepth = -2f;  // Top of the walkable area (back)
        public float maxDepth = 2f;   // Bottom of the walkable area (front)
    
        [Header("UI")]
        public GameObject pauseMenuUI;
    
        private bool facingRight = true;
        private bool isPaused = false;
        private float moveInputX;
        private float moveInputY;

        private void Start()
        {
            // Store the ground position
            groundYPosition = transform.position.y;
        
            // Create sprite container if not assigned
            if (spriteTransform == null)
            {
                GameObject spriteObj = new GameObject("Sprite");
                spriteObj.transform.parent = transform;
                spriteObj.transform.localPosition = Vector3.zero;
                spriteTransform = spriteObj.transform;
            
                // Move existing sprite renderer to this object
                SpriteRenderer sr = GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    SpriteRenderer newSR = spriteObj.AddComponent<SpriteRenderer>();
                    newSR.sprite = sr.sprite;
                    newSR.sortingLayerName = sr.sortingLayerName;
                    newSR.sortingOrder = sr.sortingOrder;
                    Destroy(sr);
                }
            }
        
            // Create shadow if not assigned
            if (shadowTransform == null)
            {
                GameObject shadowObj = new GameObject("Shadow");
                shadowObj.transform.parent = transform;
                shadowObj.transform.localPosition = Vector3.zero;
                shadowTransform = shadowObj.transform;
            
                // Add a simple shadow sprite
                SpriteRenderer shadowSR = shadowObj.AddComponent<SpriteRenderer>();
                shadowSR.color = new Color(0, 0, 0, 0.5f);
                shadowSR.sortingOrder = -1;
            
                // Create a simple circle for shadow (you can replace with your own shadow sprite)
                Texture2D shadowTex = new Texture2D(32, 32);
                for (int y = 0; y < 32; y++)
                {
                    for (int x = 0; x < 32; x++)
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(16, 16));
                        shadowTex.SetPixel(x, y, dist < 12 ? Color.white : Color.clear);
                    }
                }
                shadowTex.Apply();
                shadowSR.sprite = Sprite.Create(shadowTex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
            }
        
            // Make sure we start in gameplay context
            SimpleInputManager.Instance.SetContext(SimpleInputManager.InputContext.Gameplay);
        }

        private void Update()
        {
            // Check if we're in gameplay context before processing game inputs
            if (SimpleInputManager.Instance.GetCurrentContext() != SimpleInputManager.InputContext.Gameplay)
                return;
        
            // Get input
            moveInputX = SimpleInputManager.Instance.GetAxis("Horizontal");  // Left/Right
            moveInputY = SimpleInputManager.Instance.GetAxis("Vertical");    // Up/Down (into screen)
        
            // Flip character sprite based on horizontal movement
            if (moveInputX > 0 && !facingRight)
                Flip();
            else if (moveInputX < 0 && facingRight)
                Flip();
        
            // Jump - can jump anytime, even while already jumping
            if (SimpleInputManager.Instance.GetButtonDown("Jump") && !isJumping)
            {
                StartJump();
            }
        
            // Update jump
            if (isJumping)
            {
                UpdateJump();
            }
        
            // Attack
            if (SimpleInputManager.Instance.GetButtonDown("Fire"))
            {
                Attack();
            }
        
            // Interact
            if (SimpleInputManager.Instance.GetButtonDown("Interact"))
            {
                Interact();
            }
        
            // Open pause menu
            if (SimpleInputManager.Instance.GetButtonDown("Pause"))
            {
                TogglePauseMenu();
            }
        }
    
        void FixedUpdate()
        {
            // Move the player (this is the real position, always on ground)
            // X = left/right, Y = up/down into screen (depth)
            var movement = new Vector3(moveInputX, moveInputY, 0) * (moveSpeed * Time.fixedDeltaTime);
            transform.position += movement;
        
            // Clamp the Y position (depth - up/down on screen)
            Vector3 pos = transform.position;
            pos.y = Mathf.Clamp(pos.y, minDepth, maxDepth);
            transform.position = pos;
        
            // Update sorting order based on depth
            UpdateSortingOrder();
        }
    
        void StartJump()
        {
            isJumping = true;
            jumpTimer = 0f;
        }
    
        void UpdateJump()
        {
            jumpTimer += Time.deltaTime;
        
            // Calculate jump progress (0 to 1)
            float progress = jumpTimer / jumpDuration;
        
            if (progress >= 1f)
            {
                // Jump finished
                isJumping = false;
                spriteTransform.localPosition = Vector3.zero;
            
                // Make shadow normal size
                if (shadowTransform != null)
                    shadowTransform.localScale = Vector3.one;
            }
            else
            {
                // Parabolic jump curve (goes up then down)
                // In 2D: Y is up, so we move the sprite UP on Y axis
                float height = Mathf.Sin(progress * Mathf.PI) * jumpHeight;
                spriteTransform.localPosition = new Vector3(0, height, 0); // Y for visual height in 2D
            
                // Scale shadow based on height (smaller when higher)
                if (shadowTransform != null)
                {
                    float shadowScale = 1f - (height / jumpHeight) * 0.3f;
                    shadowTransform.localScale = new Vector3(shadowScale, shadowScale, 1f);
                }
            }
        }

        private void UpdateSortingOrder()
        {
            // Characters lower on screen (higher Y value) should be drawn in front
            SpriteRenderer sr = spriteTransform.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100);
            }
        
            // Shadow is always behind sprite
            SpriteRenderer shadowSR = shadowTransform.GetComponent<SpriteRenderer>();
            if (shadowTransform != null)
            {
                if (shadowSR != null)
                {
                    shadowSR.sortingOrder = sr.sortingOrder - 1;
                }
            }
        }

        private void Flip()
        {
            facingRight = !facingRight;
            Vector3 scale = spriteTransform.localScale;
            scale.x *= -1;
            spriteTransform.localScale = scale;
        }
    
        void Attack()
        {
            Debug.Log("Attack!");
            // Add your attack logic here
            // Can't attack while jumping in most beat 'em ups
            if (!isJumping)
            {
                // Do attack animation
            }
        }
    
        void Interact()
        {
            Debug.Log("Interact!");
            // Add your interact logic here
        }
    
        void TogglePauseMenu()
        {
            isPaused = !isPaused;
        
            if (pauseMenuUI != null)
            {
                pauseMenuUI.SetActive(isPaused);
            }
        
            Time.timeScale = isPaused ? 0f : 1f;
        }
    
        // Visualize depth limits in editor
        void OnDrawGizmosSelected()
        {
            // Draw depth boundaries
            Gizmos.color = Color.yellow;
            Vector3 pos = transform.position;
            Gizmos.DrawLine(new Vector3(pos.x - 2, minDepth, pos.z), new Vector3(pos.x + 2, minDepth, pos.z));
            Gizmos.DrawLine(new Vector3(pos.x - 2, maxDepth, pos.z), new Vector3(pos.x + 2, maxDepth, pos.z));
        
            // Draw movement area
            Gizmos.color = new Color(0, 1, 0, 0.2f);
            Vector3 center = new Vector3(pos.x, (minDepth + maxDepth) / 2, pos.z);
            Vector3 size = new Vector3(4, maxDepth - minDepth, 0.1f);
            Gizmos.DrawCube(center, size);
        }
    }
}