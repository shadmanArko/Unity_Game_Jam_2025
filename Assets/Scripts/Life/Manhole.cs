using System.Collections;
using Life;
using UnityEngine;
using Utilities;

public class Manhole : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var playerController = other.GetComponent<SimplePlayerController>();
            if (playerController != null)
            {
                playerController.DisableObject();
                GetComponent<Animator>().SetTrigger("Fall");
                StartCoroutine(EnablePlayerAfterDelay(other.gameObject, 1.0f));
            }
        }
    }

    private IEnumerator EnablePlayerAfterDelay(GameObject otherGameObject, float delay)
    {
        yield return new WaitForSeconds(delay);

        var playerController = otherGameObject.GetComponent<SimplePlayerController>();
        if (playerController != null)
        {
            playerController.transform.position = transform.position + new Vector3(2, 1, 0); // Move player above the manhole
            playerController.EnableObject(); // Assuming you have this method
            Actions.OnDecreaseTimeAction ?.Invoke(15); // Decrease time by 5 seconds
        }
    }
}