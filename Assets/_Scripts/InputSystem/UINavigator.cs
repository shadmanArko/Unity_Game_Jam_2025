using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UINavigator : MonoBehaviour
{
    [Header("UI Buttons")]
    public List<Button> buttons = new List<Button>();
    
    private int currentIndex = 0;
    private float inputDelay = 0.2f;
    private float lastInputTime = 0f;
    
    void OnEnable()
    {
        // Switch to UI context when this UI opens
        SimpleInputManager.Instance.SetContext(SimpleInputManager.InputContext.UI);
        
        // Select first button
        if (buttons.Count > 0)
        {
            SelectButton(0);
        }
    }
    
    void OnDisable()
    {
        // Switch back to gameplay context when UI closes
        SimpleInputManager.Instance.SetContext(SimpleInputManager.InputContext.Gameplay);
    }
    
    void Update()
    {
        // Prevent input spam
        if (Time.time - lastInputTime < inputDelay)
            return;
        
        // Navigate with keyboard/gamepad
        float vertical = SimpleInputManager.Instance.GetAxis("Vertical");
        
        if (vertical > 0.5f || SimpleInputManager.Instance.GetButtonDown("MoveUp"))
        {
            NavigateUp();
            lastInputTime = Time.time;
        }
        else if (vertical < -0.5f || SimpleInputManager.Instance.GetButtonDown("MoveDown"))
        {
            NavigateDown();
            lastInputTime = Time.time;
        }
        
        // Handle horizontal navigation if needed
        float horizontal = SimpleInputManager.Instance.GetAxis("Horizontal");
        if (horizontal > 0.5f || SimpleInputManager.Instance.GetButtonDown("MoveRight"))
        {
            NavigateRight();
            lastInputTime = Time.time;
        }
        else if (horizontal < -0.5f || SimpleInputManager.Instance.GetButtonDown("MoveLeft"))
        {
            NavigateLeft();
            lastInputTime = Time.time;
        }
        
        // Submit button (A/X or Enter)
        if (SimpleInputManager.Instance.GetButtonDown("Submit"))
        {
            PressCurrentButton();
        }
        
        // Cancel button (B/Circle or Escape)
        if (SimpleInputManager.Instance.GetButtonDown("Cancel"))
        {
            CloseUI();
        }
    }
    
    void NavigateUp()
    {
        currentIndex--;
        if (currentIndex < 0)
            currentIndex = buttons.Count - 1;
        SelectButton(currentIndex);
    }
    
    void NavigateDown()
    {
        currentIndex++;
        if (currentIndex >= buttons.Count)
            currentIndex = 0;
        SelectButton(currentIndex);
    }
    
    void NavigateLeft()
    {
        // Optional: for grid layouts
        NavigateUp();
    }
    
    void NavigateRight()
    {
        // Optional: for grid layouts
        NavigateDown();
    }
    
    void SelectButton(int index)
    {
        if (index < 0 || index >= buttons.Count)
            return;
        
        currentIndex = index;
        
        // Highlight the button
        EventSystem.current.SetSelectedGameObject(buttons[index].gameObject);
        
        // Visual feedback (optional)
        foreach (var btn in buttons)
        {
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            btn.colors = colors;
        }
        
        ColorBlock selectedColors = buttons[index].colors;
        selectedColors.normalColor = Color.yellow;
        buttons[index].colors = selectedColors;
    }
    
    void PressCurrentButton()
    {
        if (currentIndex >= 0 && currentIndex < buttons.Count)
        {
            buttons[currentIndex].onClick.Invoke();
        }
    }
    
    void CloseUI()
    {
        // Close this UI panel
        gameObject.SetActive(false);
    }
}