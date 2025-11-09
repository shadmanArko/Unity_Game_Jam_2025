using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class ObstacleSpawner : MonoBehaviour
{
    private int lastChunkIndex = -1;
    [SerializeField] private List<GameObject> roadBlockerObstacles = new List<GameObject>();
    [SerializeField] private List<GameObject> officeBlockerObjects = new List<GameObject>();
    private List<RoadChunk> roadChunks = new List<RoadChunk>();
    [SerializeField] [Range(0, 1)] private float obstacleSpawnProbability = 0.3f; // 30% chance to spawn an obstacle
    private List<GameObject> spawnedObstacles = new List<GameObject>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        roadChunks = new List<RoadChunk>(FindObjectsOfType<RoadChunk>());
        Actions.OnPlayerEnteredRoadChunkIndex += HandlePlayerEnteredRoadChunk;
        
    }

    private void HandlePlayerEnteredRoadChunk(int chunkIndex)
    {
        bool isMovingForward = chunkIndex > lastChunkIndex;
        lastChunkIndex = chunkIndex;
        var probability = UnityEngine.Random.Range(0f, 1f);
        var targetChunkIndex = chunkIndex+1;
        if (isMovingForward && probability < obstacleSpawnProbability)
        {
            // Spawn obstacle in this chunk
            RoadChunk targetChunk = roadChunks.Find(chunk => chunk.chunkIndex == targetChunkIndex);
            if (targetChunk != null)
            {
                // For simplicity, just log the spawn event
                Debug.Log("Spawning obstacle in chunk " + chunkIndex);
                // Here you would instantiate your obstacle prefab at a random position within the chunk
                if (chunkIndex> 8)
                {
                    var obstacle = GameObject.Instantiate(officeBlockerObjects[UnityEngine.Random.Range(0, officeBlockerObjects.Count)],
                        targetChunk.transform.position,
                        Quaternion.identity);
                    spawnedObstacles.Add(obstacle);
                }
                else
                {
                    var obstacle = GameObject.Instantiate(roadBlockerObstacles[UnityEngine.Random.Range(0, roadBlockerObstacles.Count)],
                        targetChunk.transform.position,
                        Quaternion.identity);
                    spawnedObstacles.Add(obstacle);
                }
                
            }
            else
            {
                Debug.Log("No target chunk found for index " + targetChunkIndex);
            }
        }
        {
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDisable()
    {
        Actions.OnPlayerEnteredRoadChunkIndex -= HandlePlayerEnteredRoadChunk;
    }
}
