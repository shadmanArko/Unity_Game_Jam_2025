using UnityEngine;
using Utilities;

namespace MainMenuSystem
{
    public class MainMenuPanelClickDetector : MonoBehaviour
    {
        public void PointerClick()
        {
            Debug.Log("Panel clicked!");
            GameReference.instance.mainMenuCanvasController.OnClickScreen();
        }
    }
}