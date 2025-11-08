using UnityEngine;
using System.Collections.Generic;

public class SideScrollingMapGenerator : MonoBehaviour
{
    [System.Serializable]
    public class AssetLayer
    {
        [Tooltip("The prefab(s) to repeat. If multiple, will randomly select from list")]
        public List<GameObject> assetPrefabs = new List<GameObject>();
        
        [Tooltip("Number of times to repeat assets")]
        public int repeatCount = 10;
        
        [Tooltip("Space between each instance (0 for seamless)")]
        public float spacing = 0f;
        
        [Tooltip("Starting X position for this layer (overrides global start origin X)")]
        public float startX = 0f;
        
        [Tooltip("Y offset for this layer (useful for layering roads, buildings, etc.)")]
        public float yOffset = 0f;
        
        [Tooltip("Z offset for this layer (for proper sorting)")]
        public float zOffset = 0f;
        
        [Tooltip("Use global start origin X instead of layer-specific startX")]
        public bool useGlobalStartX = true;
        
        /// <summary>
        /// Gets a random prefab from the list, or the only one if list has 1 item
        /// </summary>
        public GameObject GetRandomPrefab()
        {
            if (assetPrefabs == null || assetPrefabs.Count == 0)
                return null;
            
            if (assetPrefabs.Count == 1)
                return assetPrefabs[0];
            
            return assetPrefabs[Random.Range(0, assetPrefabs.Count)];
        }
    }
    
    [Header("Generation Settings")]
    [Tooltip("Starting position for generation")]
    public Vector3 startOrigin = Vector3.zero;
    
    [Tooltip("All asset layers to generate")]
    public List<AssetLayer> assetLayers = new List<AssetLayer>();
    
    [Header("Options")]
    [Tooltip("Generate on Start")]
    public bool generateOnStart = true;
    
    [Tooltip("Parent for spawned objects (keeps hierarchy clean)")]
    public Transform spawnParent;
    
    // Store generated objects for cleanup
    private List<GameObject> generatedObjects = new List<GameObject>();
    
    void Start()
    {
        if (generateOnStart)
        {
            GenerateMap();
        }
    }
    
    /// <summary>
    /// Generates the entire map based on configured layers
    /// </summary>
    public void GenerateMap()
    {
        ClearMap();
        
        foreach (AssetLayer layer in assetLayers)
        {
            if (layer.assetPrefabs == null || layer.assetPrefabs.Count == 0)
            {
                Debug.LogWarning("AssetLayer has no prefabs assigned, skipping...");
                continue;
            }
            
            GenerateLayer(layer);
        }
        
        Debug.Log($"Map generation complete! Generated {generatedObjects.Count} objects.");
    }
    
    /// <summary>
    /// Generates a single layer of repeating assets
    /// </summary>
    private void GenerateLayer(AssetLayer layer)
    {
        // Use layer-specific start X or global start origin X
        float startXPos = layer.useGlobalStartX ? startOrigin.x : layer.startX;
        Vector3 currentPosition = new Vector3(startXPos, startOrigin.y + layer.yOffset, startOrigin.z + layer.zOffset);
        
        for (int i = 0; i < layer.repeatCount; i++)
        {
            bool roadChunk = false;
            // Get a random prefab from the layer (or the only one if single)
            GameObject prefabToSpawn = layer.GetRandomPrefab();
            
            if (prefabToSpawn == null)
            {
                Debug.LogWarning($"Layer returned null prefab at index {i}, skipping...");
                continue;
            }
            
            // Instantiate the asset
            GameObject instance = Instantiate(prefabToSpawn, currentPosition, Quaternion.identity);
            if (prefabToSpawn.GetComponent<RoadChunk>())
            {
                roadChunk = true;
            }
            // Set parent
            if (spawnParent != null)
            {
                instance.transform.SetParent(spawnParent);
            }
            else
            {
                instance.transform.SetParent(transform);
            }

            if (roadChunk)
            {
                instance.GetComponent<RoadChunk>().chunkIndex = i;
            }
            // Name it for easy identification
            instance.name = $"{prefabToSpawn.name}_{i}";
            
            // Track for cleanup
            generatedObjects.Add(instance);
            
            // Calculate next position
            float assetWidth = GetAssetWidth(instance);
            currentPosition.x += assetWidth + layer.spacing;
        }
    }
    
    /// <summary>
    /// Gets the width of an asset based on its sprite/renderer bounds
    /// </summary>
    private float GetAssetWidth(GameObject asset)
    {
        // Try SpriteRenderer first (for 2D)
        SpriteRenderer spriteRenderer = asset.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            return spriteRenderer.bounds.size.x;
        }
        
        // Try generic Renderer (for 3D or other renderers)
        Renderer renderer = asset.GetComponent<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds.size.x;
        }
        
        // Check children for SpriteRenderer
        SpriteRenderer[] childSprites = asset.GetComponentsInChildren<SpriteRenderer>();
        if (childSprites.Length > 0)
        {
            Bounds combinedBounds = childSprites[0].bounds;
            foreach (SpriteRenderer sr in childSprites)
            {
                combinedBounds.Encapsulate(sr.bounds);
            }
            return combinedBounds.size.x;
        }
        
        // Check children for any Renderer
        Renderer[] childRenderers = asset.GetComponentsInChildren<Renderer>();
        if (childRenderers.Length > 0)
        {
            Bounds combinedBounds = childRenderers[0].bounds;
            foreach (Renderer r in childRenderers)
            {
                combinedBounds.Encapsulate(r.bounds);
            }
            return combinedBounds.size.x;
        }
        
        // Default fallback
        Debug.LogWarning($"Could not determine width for {asset.name}, using default value of 1");
        return 1f;
    }
    
    /// <summary>
    /// Clears all generated objects
    /// </summary>
    public void ClearMap()
    {
        foreach (GameObject obj in generatedObjects)
        {
            if (obj != null)
            {
                DestroyImmediate(obj);
            }
        }
        generatedObjects.Clear();
    }
    
    /// <summary>
    /// Regenerates the map (clears and generates)
    /// </summary>
    public void RegenerateMap()
    {
        GenerateMap();
    }
    
#if UNITY_EDITOR
    // Editor helper to generate from inspector
    [ContextMenu("Generate Map")]
    private void GenerateMapContextMenu()
    {
        GenerateMap();
    }
    
    [ContextMenu("Clear Map")]
    private void ClearMapContextMenu()
    {
        ClearMap();
    }
#endif
}