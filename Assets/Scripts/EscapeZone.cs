using UnityEngine;

public class EscapeZone : MonoBehaviour
{
    [Header("Referanslar")]
    public PlayerUI playerUIManager;

    private bool hasWon = false;

    void Start()
    {
        if (playerUIManager == null)
        {
            Debug.LogError("EscapeZone, PlayerUI Manager referansýný bulamýyor!", this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        if (hasWon || !other.CompareTag("Player"))
        {
            return;
        }

        if (SecurityAI.isAlarmTriggered)
        {
            hasWon = true; 

            if (playerUIManager != null)
            {
                playerUIManager.ShowWinScreen(); 
            }
        }
    }
}
