using System;
using System.Collections.Generic;
using Life;
using UnityEngine;
using Utilities;

public class BikeCarSpawner : MonoBehaviour
{
    private int currentRoadChunkIndex = 0;
    private List<RoadChunk> roadChunks = new List<RoadChunk>();
    [SerializeField] private UberCar carPrefab;
    [SerializeField] private UberCar bikePrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        roadChunks = new List<RoadChunk>(FindObjectsOfType<RoadChunk>());
        Actions.OnCarCalledAction += HandleCarCalled;
        Actions.OnBikeCalledAction += HandleBikeCalled;
        Actions.OnPlayerEnteredRoadChunkIndex += HandlePlayerEnteredRoadChunkIndex;
    }

    private void HandlePlayerEnteredRoadChunkIndex(int obj)
    {
        currentRoadChunkIndex = obj;
    }

    private void HandleBikeCalled()
    {
        RoadChunk targetChunk = null;
        if (currentRoadChunkIndex < 2)
        {
            targetChunk = roadChunks.Find(chunk => chunk.chunkIndex == currentRoadChunkIndex + 1);
            
        }
        else
        {
            targetChunk = roadChunks.Find(chunk => chunk.chunkIndex == currentRoadChunkIndex - 1);
        }
        if (targetChunk != null)
        {
            var bike = Instantiate(bikePrefab, targetChunk.transform.position + new Vector3(0, 2, 0), Quaternion.identity);
            bike.GetComponent<UberCar>().CallUber();
        }
        else
        {
            Debug.Log("No target chunk found for bike at index " + currentRoadChunkIndex);
        }
    }

    private void HandleCarCalled()
    {
        RoadChunk targetChunk = null;
        if (currentRoadChunkIndex < 2)
        {
            targetChunk = roadChunks.Find(chunk => chunk.chunkIndex == currentRoadChunkIndex + 1);
            
        }
        else
        {
            targetChunk = roadChunks.Find(chunk => chunk.chunkIndex == currentRoadChunkIndex - 1);
        }
        if (targetChunk != null)
        {
            var car = Instantiate(carPrefab, targetChunk.transform.position + new Vector3(0, 2, 0), Quaternion.identity);
            car.GetComponent<UberCar>().CallUber();
        }
        else
        {
            Debug.Log("No target chunk found for bike at index " + currentRoadChunkIndex);
        }
    }

    private void OnDisable()
    {
        Actions.OnCarCalledAction -= HandleCarCalled;
        Actions.OnBikeCalledAction -= HandleBikeCalled;
        Actions.OnPlayerEnteredRoadChunkIndex -= HandlePlayerEnteredRoadChunkIndex;
    }
}
