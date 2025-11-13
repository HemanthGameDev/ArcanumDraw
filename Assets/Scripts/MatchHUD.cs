using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MatchHUD : MonoBehaviour
{
    [Header("Match UI References")]
    [SerializeField] private TextMeshProUGUI matchStateText;
    [SerializeField] private TextMeshProUGUI matchTimerText;
    [SerializeField] private TextMeshProUGUI victoryText;
    [SerializeField] private GameObject victoryPanel;
    
    [Header("Player Health Bars")]
    [SerializeField] private Slider player1HealthBar;
    [SerializeField] private Slider player2HealthBar;
    [SerializeField] private TextMeshProUGUI player1HealthText;
    [SerializeField] private TextMeshProUGUI player2HealthText;
    [SerializeField] private TextMeshProUGUI player1NameText;
    [SerializeField] private TextMeshProUGUI player2NameText;
    
    [Header("References")]
    [SerializeField] private MatchManager matchManager;
    
    private void Start()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }
        
        if (matchManager != null)
        {
            matchManager.OnMatchStateChanged += OnMatchStateChanged;
            matchManager.OnPlayerWon += OnPlayerWon;
            
            PlayerStats player1 = matchManager.GetPlayer1();
            PlayerStats player2 = matchManager.GetPlayer2();
            
            if (player1 != null)
            {
                player1.OnHealthChanged += (current, max) => UpdatePlayerHealth(player1HealthBar, player1HealthText, current, max);
                if (player1NameText != null) player1NameText.text = player1.gameObject.name;
            }
            
            if (player2 != null)
            {
                player2.OnHealthChanged += (current, max) => UpdatePlayerHealth(player2HealthBar, player2HealthText, current, max);
                if (player2NameText != null) player2NameText.text = player2.gameObject.name;
            }
        }
    }
    
    private void Update()
    {
        if (matchManager != null && matchTimerText != null)
        {
            if (matchManager.CurrentState == MatchState.CastingPhase)
            {
                float timeRemaining = matchManager.MatchTimeRemaining;
                if (timeRemaining >= 0f)
                {
                    int minutes = Mathf.FloorToInt(timeRemaining / 60f);
                    int seconds = Mathf.FloorToInt(timeRemaining % 60f);
                    matchTimerText.text = $"Time: {minutes:00}:{seconds:00}";
                }
                else
                {
                    matchTimerText.text = matchManager.GetMatchTimeString();
                }
            }
        }
    }
    
    private void OnDestroy()
    {
        if (matchManager != null)
        {
            matchManager.OnMatchStateChanged -= OnMatchStateChanged;
            matchManager.OnPlayerWon -= OnPlayerWon;
        }
    }
    
    private void OnMatchStateChanged(MatchState newState)
    {
        if (matchStateText == null) return;
        
        switch (newState)
        {
            case MatchState.PreMatch:
                matchStateText.text = "Pre-Match";
                matchStateText.color = Color.white;
                break;
                
            case MatchState.MatchStarting:
                matchStateText.text = "Match Starting...";
                matchStateText.color = Color.yellow;
                break;
                
            case MatchState.CastingPhase:
                matchStateText.text = "FIGHT!";
                matchStateText.color = Color.green;
                break;
                
            case MatchState.MatchEnding:
                matchStateText.text = "Match Ending";
                matchStateText.color = Color.cyan;
                break;
                
            case MatchState.MatchEnded:
                matchStateText.text = "Match Ended";
                matchStateText.color = Color.gray;
                break;
        }
    }
    
    private void OnPlayerWon(PlayerStats winner)
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
        
        if (victoryText != null)
        {
            victoryText.text = $"{winner.gameObject.name} WINS!";
        }
    }
    
    private void UpdatePlayerHealth(Slider healthBar, TextMeshProUGUI healthText, float currentHealth, float maxHealth)
    {
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
        
        if (healthText != null)
        {
            healthText.text = $"{currentHealth:F0}/{maxHealth:F0}";
        }
    }
}
