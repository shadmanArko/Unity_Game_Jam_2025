using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance; // Singleton to access from anywhere
    
    [SerializeField] private Image fadeImage; // Black image that covers the screen
    [SerializeField] private float fadeDuration = 1f; // How long the fade takes
    [SerializeField] private GameObject fadeCanvas;
    
    private void Awake()
    {
        // Singleton pattern - only one FadeManager exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persists between scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Make sure we start with a clear screen
        SetAlpha(0f);
    }
    
    // Fade to black
    public IEnumerator FadeIn()
    {
        yield return Fade(0f, 1f);
    }
    
    // Fade from black to clear
    public IEnumerator FadeOut()
    {
        yield return Fade(1f, 0f);
    }
    
    // Main fade function
    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }
        
        SetAlpha(endAlpha);
    }
    
    // Helper function to set the image alpha
    private void SetAlpha(float alpha)
    {
        Color color = fadeImage.color;
        color.a = alpha;
        fadeImage.color = color;
    }

    public void EnableFadeCanvas()
    {
        fadeCanvas.SetActive(true);
    }

    public void DisableFadeCanvas()
    {
        fadeCanvas.SetActive(false);
    }
}