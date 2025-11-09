using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace NotificationSystem
{
    public class NotificationCanvasController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject notificationPanel;
        [SerializeField] private TMP_Text notificationText;

        [Header("Settings")]
        [SerializeField] private float displayDuration = 2f;

        private bool isShowing = false;
        
        public async void ShowNotification(string message)
        {
            if (isShowing) return; // prevent overlapping messages
            isShowing = true;

            notificationText.text = message;
            notificationPanel.SetActive(true);

            await Task.Delay((int)(displayDuration * 1000));

            notificationPanel.SetActive(false);
            isShowing = false;
        }
        
        public void HideNotification()
        {
            notificationPanel.SetActive(false);
            isShowing = false;
        }
    }
}