using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EscapeCanvas : MonoBehaviour
{
    [SerializeField] private GameObject escapeCanvasPanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button endRunButton;
    [SerializeField] private Button exitGameButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private bool isPanelActive = false;
    void Start()
    {
        resumeButton.onClick.AddListener(ResumeGame);
        endRunButton.onClick.AddListener(EndRunAndRestartGame);
        exitGameButton.onClick.AddListener(ExitGame);
    }

    private void ExitGame()
    {
        Application.Quit();
    }

    private void EndRunAndRestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ResumeGame()
    {
        escapeCanvasPanel.SetActive(false);
        isPanelActive = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            escapeCanvasPanel.SetActive(!isPanelActive);
            isPanelActive = !isPanelActive;
        }
    }
}
