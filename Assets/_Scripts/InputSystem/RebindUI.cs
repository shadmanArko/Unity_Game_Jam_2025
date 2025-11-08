using UnityEngine;
using UnityEngine.UI;

public class RebindUI : MonoBehaviour
{
    public Text statusText;
    private string actionToRebind = "";
    private bool isWaitingForKey = false;
    private bool isGamepadRebind = false;
    
    void Update()
    {
        if (isWaitingForKey)
        {
            // Check for any key press
            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key))
                {
                    // Rebind the action
                    SimpleInputManager.Instance.RebindKey(actionToRebind, key, isGamepadRebind);
                    
                    statusText.text = $"{actionToRebind} rebound to {key}";
                    isWaitingForKey = false;
                    actionToRebind = "";
                    break;
                }
            }
        }
    }
    
    // Call this from a button in your UI
    public void StartRebind(string action, bool gamepad)
    {
        actionToRebind = action;
        isGamepadRebind = gamepad;
        isWaitingForKey = true;
        statusText.text = $"Press any key for {action}...";
    }
    
    // Example button methods you can connect to UI buttons
    public void RebindJumpKeyboard()
    {
        StartRebind("Jump", false);
    }
    
    public void RebindJumpGamepad()
    {
        StartRebind("Jump", true);
    }
    
    public void RebindFireKeyboard()
    {
        StartRebind("Fire", false);
    }
    
    public void RebindInteractKeyboard()
    {
        StartRebind("Interact", false);
    }
}