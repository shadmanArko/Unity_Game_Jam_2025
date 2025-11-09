using System.Collections;
using UnityEngine;

public class Protest : MonoBehaviour
{
    [SerializeField] private float protestDuration = 5.0f; // Time before disabling collider
    [SerializeField] private float moveSpeed = 2.0f;       // Speed of movement

    private Collider2D protestCollider;

    private void Start()
    {
        protestCollider = GetComponent<Collider2D>();
        StartCoroutine(ProtestRoutine());
    }

    private IEnumerator ProtestRoutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < protestDuration)
        {
            // Move left smoothly
            transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Disable the collider after duration
        if (protestCollider != null)
        {
            protestCollider.enabled = false;
        }
    }
}