using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public Button resumeButton;
    public Button settingsButton;
    public Button quitButton;
    
    void Start()
    {
        // Connect button events
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);
        
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);
        
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }
    
    void ResumeGame()
    {
        Debug.Log("Resume clicked!");
        gameObject.SetActive(false);
        Time.timeScale = 1f;
    }
    
    void OpenSettings()
    {
        Debug.Log("Settings clicked!");
        // Open settings menu
    }
    
    void QuitGame()
    {
        Debug.Log("Quit clicked!");
        // Load main menu or quit
        SceneManager.LoadScene("MainMenu");
    }
}