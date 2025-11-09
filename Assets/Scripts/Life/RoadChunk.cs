using UnityEngine;
using Utilities;

public class RoadChunk : MonoBehaviour
{
    public int chunkIndex;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered chunk " + chunkIndex);
        }

        if (other.CompareTag("Vehicle"))
        {
            Debug.Log("Vehicle entered chunk " + chunkIndex);
            Actions.OnPlayerEnteredRoadChunkIndex?.Invoke(chunkIndex);
        }
    }
}
