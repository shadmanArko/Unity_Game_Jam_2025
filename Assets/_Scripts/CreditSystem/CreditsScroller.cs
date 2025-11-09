using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class CreditsScroller : MonoBehaviour
{
    [Header("Credits Text")]
    [TextArea(10, 30)]
    public string creditsText = @"<size=60><b><color=#FFD700>YOUR GAME TITLE</color></b></size>

<size=40><color=#00BFFF>Created by</color></size>
<b>Your Name</b>

<size=40><color=#00BFFF>Programming</color></size>
<b>Your Name</b>

<size=40><color=#00BFFF>Art & Design</color></size>
<b>Artist Name</b>

<size=40><color=#00BFFF>Music & Sound</color></size>
<b>Sound Designer Name</b>

<size=40><color=#00BFFF>Special Thanks</color></size>
<i>Friends and Family</i>

<size=50><color=#FF69B4>Thank you for playing!</color></size>";

    [Header("Settings")]
    public float scrollSpeed = 50f;
    public string sceneToLoad = "MainMenu";
    
    private RectTransform textRect;
    private TextMeshProUGUI tmpText;
    private float startY;
    private float canvasHeight;
    
    void Start()
    {
        SetupCredits();
        StartCoroutine(ScrollCredits());
    }
    
    void SetupCredits()
    {
        tmpText = GetComponent<TextMeshProUGUI>();
        textRect = GetComponent<RectTransform>();
        
        if (tmpText != null)
        {
            // Set the text
            tmpText.text = creditsText;
            
            // Force update to calculate size
            Canvas.ForceUpdateCanvases();
            tmpText.ForceMeshUpdate();
            
            // Auto resize height based on content
            float textHeight = tmpText.preferredHeight;
            textRect.sizeDelta = new Vector2(textRect.sizeDelta.x, textHeight);
            
            // Get canvas height
            Canvas canvas = GetComponentInParent<Canvas>();
            canvasHeight = canvas.GetComponent<RectTransform>().rect.height;
            
            // Start position (just below the screen)
            startY = -canvasHeight / 2 - textRect.rect.height / 2;
            textRect.anchoredPosition = new Vector2(0, startY);
        }
    }
    
    IEnumerator ScrollCredits()
    {
        while (true)
        {
            // Scroll up
            textRect.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;
            
            // Check if text has scrolled past the top
            if (textRect.anchoredPosition.y > canvasHeight / 2 + textRect.rect.height / 2)
            {
                // Reset to bottom
                textRect.anchoredPosition = new Vector2(0, startY);
            }
            
            // Check for exit input
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                GoToMainMenu();
                yield break;
            }
            
            yield return null;
        }
    }
    
    void GoToMainMenu()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}

// ===== SETUP INSTRUCTIONS =====
//
// 1. CREATE NEW SCENE:
//    File → New Scene → Save as "Credits"
//
// 2. CREATE UI:
//    - Right-click in Hierarchy → UI → Canvas
//    - Right-click Canvas → UI → Text - TextMeshPro
//    - Name it "CreditsText"
//
// 3. SETUP TEXT:
//    - Select CreditsText
//    - Set Anchor to middle-center
//    - Set Width: 800 (height will auto-resize)
//    - Set Font Size: 36
//    - Set Alignment: Center/Top
//    - Set Color: White
//    - IMPORTANT: Enable "Rich Text" checkbox in TextMeshPro component!
//
// 4. ADD SCRIPT:
//    - Add CreditsScroller script to the CreditsText object
//    - Edit the credits text in Inspector
//    - Set scroll speed (default 50)
//    - Set scene to load when pressing ESC (usually "MainMenu")
//
// 5. OPTIONAL - ADD BACKGROUND:
//    - Right-click Canvas → UI → Image
//    - Move it above CreditsText in hierarchy
//    - Stretch to full screen
//    - Set color to black
//
// 6. TEXTMESHPRO RICH TEXT TAGS YOU CAN USE:
//    <b>Bold Text</b>
//    <i>Italic Text</i>
//    <u>Underline Text</u>
//    <s>Strikethrough</s>
//    <size=50>Bigger Text</size>
//    <color=#FF0000>Red Text</color>
//    <color=red>Red Text</color>
//    <color=#FFFF00>Yellow Text</color>
//    <mark=#FFFF0080>Highlighted Text</mark>
//    <link="url">Link Text</link>
//    <uppercase>UPPERCASE</uppercase>
//    <lowercase>lowercase</lowercase>
//    <gradient preset="rainbow">Rainbow Text</gradient>
//
// 7. Common Colors (Hex Codes):
//    #FFD700 = Gold
//    #00BFFF = Sky Blue
//    #FF69B4 = Hot Pink
//    #32CD32 = Lime Green
//    #FF4500 = Orange Red
//    #9370DB = Purple
//    #FFFFFF = White
//    #000000 = Black