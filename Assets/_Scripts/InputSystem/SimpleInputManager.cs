using UnityEngine;
using System.Collections.Generic;

public class SimpleInputManager : MonoBehaviour
{
    public static SimpleInputManager Instance;
    
    // Current input context
    public enum InputContext { Gameplay, UI, Menu }
    private InputContext currentContext = InputContext.Gameplay;
    
    // Dictionary to store key bindings for different contexts
    private Dictionary<string, KeyCode> keyboardBindings = new Dictionary<string, KeyCode>();
    private Dictionary<string, KeyCode> gamepadBindings = new Dictionary<string, KeyCode>();
    
    // Context-specific bindings
    private Dictionary<InputContext, Dictionary<string, KeyCode>> contextKeyboardBindings = new Dictionary<InputContext, Dictionary<string, KeyCode>>();
    private Dictionary<InputContext, Dictionary<string, KeyCode>> contextGamepadBindings = new Dictionary<InputContext, Dictionary<string, KeyCode>>();
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetDefaultBindings();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void SetDefaultBindings()
    {
        // GAMEPLAY CONTEXT
        var gameplayKB = new Dictionary<string, KeyCode>();
        gameplayKB["Jump"] = KeyCode.Space;
        gameplayKB["MoveUp"] = KeyCode.W;
        gameplayKB["MoveDown"] = KeyCode.S;
        gameplayKB["MoveLeft"] = KeyCode.A;
        gameplayKB["MoveRight"] = KeyCode.D;
        gameplayKB["Fire"] = KeyCode.Mouse0;
        gameplayKB["Interact"] = KeyCode.E;
        contextKeyboardBindings[InputContext.Gameplay] = gameplayKB;
        
        var gameplayGP = new Dictionary<string, KeyCode>();
        gameplayGP["Jump"] = KeyCode.JoystickButton4; // A/X button
        gameplayGP["Fire"] = KeyCode.JoystickButton1; // B/Circle
        gameplayGP["Interact"] = KeyCode.JoystickButton2; // X/Square
        contextGamepadBindings[InputContext.Gameplay] = gameplayGP;
        
        // UI CONTEXT
        var uiKB = new Dictionary<string, KeyCode>();
        uiKB["Submit"] = KeyCode.Return; // Enter for UI
        uiKB["Cancel"] = KeyCode.Escape;
        uiKB["MoveUp"] = KeyCode.W;
        uiKB["MoveDown"] = KeyCode.S;
        uiKB["MoveLeft"] = KeyCode.A;
        uiKB["MoveRight"] = KeyCode.D;
        contextKeyboardBindings[InputContext.UI] = uiKB;
        
        var uiGP = new Dictionary<string, KeyCode>();
        uiGP["Submit"] = KeyCode.JoystickButton0; // A/X - Press button
        uiGP["Cancel"] = KeyCode.JoystickButton1; // B/Circle - Back
        contextGamepadBindings[InputContext.UI] = uiGP;
        
        // Set default context bindings
        keyboardBindings = gameplayKB;
        gamepadBindings = gameplayGP;
    }
    
    // Switch input context
    public void SetContext(InputContext context)
    {
        currentContext = context;
        
        if (contextKeyboardBindings.ContainsKey(context))
            keyboardBindings = contextKeyboardBindings[context];
        
        if (contextGamepadBindings.ContainsKey(context))
            gamepadBindings = contextGamepadBindings[context];
        
        Debug.Log($"Input context changed to: {context}");
    }
    
    public InputContext GetCurrentContext()
    {
        return currentContext;
    }
    
    // Check if a button is pressed
    public bool GetButtonDown(string action)
    {
        if (keyboardBindings.ContainsKey(action) && Input.GetKeyDown(keyboardBindings[action]))
            return true;
        
        if (gamepadBindings.ContainsKey(action) && Input.GetKeyDown(gamepadBindings[action]))
            return true;
        
        return false;
    }
    
    // Check if a button is held
    public bool GetButton(string action)
    {
        if (keyboardBindings.ContainsKey(action) && Input.GetKey(keyboardBindings[action]))
            return true;
        
        if (gamepadBindings.ContainsKey(action) && Input.GetKey(gamepadBindings[action]))
            return true;
        
        return false;
    }
    
    // Check if a button is released
    public bool GetButtonUp(string action)
    {
        if (keyboardBindings.ContainsKey(action) && Input.GetKeyUp(keyboardBindings[action]))
            return true;
        
        if (gamepadBindings.ContainsKey(action) && Input.GetKeyUp(gamepadBindings[action]))
            return true;
        
        return false;
    }
    
    // Get axis for movement (gamepad sticks or keyboard WASD)
    public float GetAxis(string axisName)
    {
        // Check gamepad axis first
        float gamepadInput = Input.GetAxis(axisName);
        if (Mathf.Abs(gamepadInput) > 0.1f)
            return gamepadInput;
        
        // Fallback to keyboard
        if (axisName == "Horizontal")
        {
            if (GetButton("MoveLeft")) return -1f;
            if (GetButton("MoveRight")) return 1f;
        }
        else if (axisName == "Vertical")
        {
            if (GetButton("MoveDown")) return -1f;
            if (GetButton("MoveUp")) return 1f;
        }
        
        return 0f;
    }
    
    // Rebind a key
    public void RebindKey(string action, KeyCode newKey, bool isGamepad)
    {
        if (isGamepad)
        {
            if (gamepadBindings.ContainsKey(action))
                gamepadBindings[action] = newKey;
            else
                gamepadBindings.Add(action, newKey);
        }
        else
        {
            if (keyboardBindings.ContainsKey(action))
                keyboardBindings[action] = newKey;
            else
                keyboardBindings.Add(action, newKey);
        }
    }
    
    // Get current binding
    public KeyCode GetBinding(string action, bool isGamepad)
    {
        if (isGamepad && gamepadBindings.ContainsKey(action))
            return gamepadBindings[action];
        
        if (!isGamepad && keyboardBindings.ContainsKey(action))
            return keyboardBindings[action];
        
        return KeyCode.None;
    }
}