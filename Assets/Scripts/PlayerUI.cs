using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("Bileþen Referanslarý")]
    public Health playerHealth;

    [Header("UI Elemanlarý")]
    public TextMeshProUGUI healthText;

    [Header("Win/Loss Ekraný")]
    public GameObject winLossScreen;
    public TextMeshProUGUI statusText;

    private bool isPaused = false;

    void Start()
    {
        if (playerHealth == null)
        {
            Debug.LogError("PlayerUI script'i için oyuncu caný referansý atanmamýþ");
            this.enabled = false;
            return;
        }

        if (winLossScreen != null)
        {
            winLossScreen.SetActive(false);
        }

        Time.timeScale = 1f;
        isPaused = false;
    }

    void Update()
    {
        if (isPaused)
        {
            if (Input.GetButtonDown("Submit") || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                RestartGame();
            }
        }
        else
        {
            if (playerHealth.CurrentHealth > 0)
            {
                if (healthText != null)
                {
                    healthText.text = "Can: " + playerHealth.CurrentHealth.ToString("F0");
                }
            }
        }
    }

    public void ShowGameOverScreen()
    {
        if (winLossScreen == null) return;

        if (statusText != null)
        {
            statusText.text = "GAME OVER";
        }

        winLossScreen.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ShowWinScreen()
    {
        if (winLossScreen == null) return;

        if (statusText != null)
        {
            statusText.text = "YOU WIN!";
        }

        winLossScreen.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void RestartGame()
    {

        SecurityAI.isAlarmTriggered = false;

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}